'''
Part of server that works with database
You must change the config file varibles, that contain information about database connection
Current implementation requires postgresql and databases[postgresql] for async working
Just a queries part, no data manipulation
'''

import asyncio
import asyncpg
import databases
from databases import Database
from sqlalchemy.sql import select, text
# from asyncpg.pgproto.pgproto import UUID
# from sqlalchemy.dialects.postgresql import UUID
from typing import List, Generator, Dict
import typing
from config import *
import models
from models import Message, UserAccount, SuperAccount
from crypto import *
from helpers import *
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from sqlalchemy_aio import ASYNCIO_STRATEGY


class DataBase:
    def __init__(self, connect_string=""):
        """Setting params for the connection"""
        self.connect_string = connect_string
        # self.database = Database(connect_string)
        self.session_constr = sessionmaker()

    def __enter__(self) -> "DataBase":
        self.engine = create_engine(self.connect_string, strategy=ASYNCIO_STRATEGY)
        self.session_constr.configure(bind=self.engine)
        # asyncio.get_event_loop().run_until_complete(self.connect())
        return self

    def __exit__(self, exc_type=None, exc_value=None, traceback=None) -> None:
        pass
        # asyncio.get_event_loop().run_until_complete(self.disconnect())

    # def _t(self, key):
    #    '''Translates tablename according to self._voc'''
    #    return self._voc[key] if self._voc else key

    async def connect(self):
        '''Establishing connection with the database
        Example for PostgreSQL - postgresql://scott:tiger@localhost/mydatabase
        Can be useful https://stackoverflow.com/questions/769683/show-tables-in-postgresql'''
        # await self.database.connect()

    # async def iterate(self, query: str, **kwargs) -> Generator[None, None, None]:
    #    '''
    #        https://www.encode.io/databases/database_queries/
    #        Actually returns a Generator of records
    #    '''
    #    result = await self.database.iterate(query=query, kwargs=kwargs)
    #    return result

    # async def fetch_all(self, query: str, **kwargs) -> List:
    #    '''
    #        https://www.encode.io/databases/database_queries/
    #        Actually returns a List of Records
    #    '''
    #    result = await self.database.fetch_all(query=query, values=kwargs)
    #    return result

    # async def fetch_one(self, query: str, **kwargs):
    #    '''
    #        https://www.encode.io/databases/database_queries/
    #        Actually returns single Record
    #    '''
    #    result = await self.database.fetch_one(query=query, values=kwargs)
    #    return result

    # async def execute(self, query: str, **kwargs):
    #    '''
    #        https://www.encode.io/databases/database_queries/
    #        Actually returns something i cant explain
    #    '''
    #    result = await self.database.execute(query=query, values=kwargs)
    #    return result

    # async def execute_many(self, query: str, values: List[Dict]):
    #    '''
    #        https://www.encode.io/databases/database_queries/
    #        Actually returns something i cant explain
    #    '''
    #    result = await self.database.execute_many(query=query, values=values)
    #    return result

    # async def disconnect(self):
    #    await self.database.disconnect()

    async def get_user(self, login: str) -> typing.Optional[typing.Any]:
        """ Get user with specified UUID. Just send str representation of UUID """
        session = self.session_constr()
        return await session.query(UserAccount).filter_by(login=login).first()

    async def all_messages_in_tred(self, tred_id: int):
        query = text(
            "select author_login, m.timestamp as message_time, m.body as message_body, header as head_tred, t.body as tred_body, t.timestamp as tred_time, creator_id "
            "from messenger.message m "
            "inner join messenger.tred t "
            "on m.tred_id  = t.tred_id "
            "where m.is_deleted is false "
            "and t.tred_id = :tred_id "
            "order by m.timestamp; ")
        session = self.session_constr()
        result = await session.execute(query, {"tred_id": tred_id})
        return result

    async def all_people_in_personal_list(self, list_id: int):
        query = ("select * from messenger.personal_lists pl "
                 "inner join messenger.people_inlist pi "
                 "on pl.list_id = pi.list_id "
                 "where pl.list_id = :list_id")
        result = await self.fetch_all(query, list_id=list_id)
        return result

    async def all_messages_from_current_user(self, login: str):
        query = ("select * from messenger.message m "
                 "inner join messenger.user_account u "
                 "on m.author_login = u.login "
                 "where u.login::text = :login "
                 "order by m.timestamp;")
        result = await self.fetch_all(query, login=login)
        return result

    async def all_people_in_current_tred(self, tred_id: int):
        query = ("select * from messenger.tred_participation tp "
                 "inner join messenger.tred t "
                 "on tp.tred_id = t.tred_id "
                 "where t.tred_id = :tred_id;")
        result = await self.fetch_all(query, tred_id=tred_id)
        return result

    async def create_new_super_account(self) -> SuperAccount:
        session = self.session_constr()
        new_account = SuperAccount()
        session.add(new_account)
        session.commit()
        return new_account

    async def create_new_user(self) -> Tuple[UserAccount, str]:
        session = self.session_constr()
        new_acc = UserAccount()

        password = generate_password()
        salt, hashed = hash_password(password)
        new_acc.salt = salt
        new_acc.pword = hashed

        session.add(new_acc)
        session.commit()
        return new_acc, password

    async def create_new_message(self, values):
        query = (
            "insert into messenger.message(author_login, tred_id, body, is_deleted) "
            "values(:author_login, :tred_id, :body, :is_deleted);")
        result = await self.execute(query=query, **values)
        return result

    async def create_new_tred(self, values):
        query = ("insert into messenger.tred(creator_id, header, body) "
                 "values (:creator_id, :header, :body);")
        result = await self.execute(query=query, **values)
        return result

    async def create_new_tred_participation(self, values):
        query = (
            "insert into messenger.tred_participation(tred_id, superacc_id) "
            "values (:tred_id, :superacc_id);")
        result = await self.execute(query=query, **values)
        return result

    async def create_new_union_request(self, values):
        query = (
            "insert into messenger.union_requests(from_super_id, to_super_id, is_accepted) "
            "values (:from_super_id, :to_super_id, :is_accepted);")
        result = await self.execute(query=query, **values)
        return result

    async def create_new_personal_list(self, values):
        query = ("insert into messenger.personal_lists(list_name, owner_id) "
                 "values (:list_name, :owner_id);")
        result = await self.execute(query=query, **values)
        return result

    async def create_new_people_inlist(self, values):
        query = ("insert into messenger.people_inlist(list_id, friend_id) "
                 "values (:list_id, :friend_id);")
        result = await self.execute(query=query, **values)
        return result

    async def get_max_super_id(self):
        query = "select max(sa.super_id) from messenger.super_account sa;"
        result = await self.fetch_one(query)
        return result

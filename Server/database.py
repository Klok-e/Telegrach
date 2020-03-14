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
from config import *
from models import *
from crypto import *
from helpers import *


class DataBase:

    def __init__(self, user: str, passwd: str, addr: str, schema='', voc=None):
        '''Setting params for the connection'''
        self._user = user  # database user
        self._pw = passwd  # password
        self._addr = addr  # a tuple (host, port)
        self._schema = schema  # schemaname
        self._connection_string = None
        self._database = None

    def _t(self, key):
        '''Translates tablename according to self._voc'''
        return self._voc[key] if self._voc else key

    async def connect(self, dbname: str):
        '''Establishing connection with the database
        Example for PostgreSQL - postgresql://scott:tiger@localhost/mydatabase
        Can be useful https://stackoverflow.com/questions/769683/show-tables-in-postgresql'''
        self._connection_string = f'{dbname}://{self._user}:{self._pw}@{self._addr[0]}'
        self._database = Database(self._connection_string)
        await self._database.connect()

    async def iterate(self, query: str, **kwargs) -> Generator[None, None, None]:
        '''
            https://www.encode.io/databases/database_queries/
            Actually returns a Generator of records
        '''
        result = await self._database.iterate(query=query, kwargs=kwargs)
        return result

    async def fetch_all(self, query: str, **kwargs) -> List:
        '''
            https://www.encode.io/databases/database_queries/
            Actually returns a List of Records
        '''
        result = await self._database.fetch_all(query=query, values=kwargs)
        return result

    async def fetch_one(self, query: str, **kwargs):
        '''
            https://www.encode.io/databases/database_queries/
            Actually returns single Record
        '''
        result = await self._database.fetch_one(query=query, values=kwargs)
        return result

    async def execute(self, query: str, **kwargs):
        '''
            https://www.encode.io/databases/database_queries/
            Actually returns something i cant explain
        '''
        result = await self._database.execute(query=query, values=kwargs)
        return result

    async def execute_many(self, query: str, values: List[Dict]):
        '''
            https://www.encode.io/databases/database_queries/
            Actually returns something i cant explain
        '''
        result = await self._database.execute_many(query=query, values=values)
        return result

    async def disconnect(self):
        await self._database.disconnect()

    async def get_current_user(self, login: str):
        ''' Get user with specified UUID. Just send str representation of UUID '''
        # query = UserAccount.select().where(UserAccount.c.login == login)
        query = (
            "select * from messenger.user_account ua where ua.login::text = :login")
        result = await self.fetch_one(query=query, login=login)
        return result

    async def all_messages_in_tred(self, tred_id: int):
        query = ("select author_login, m.timestamp as message_time, m.body as message_body, header as head_tred, t.body as tred_body, t.timestamp as tred_time, creator_id " 
                 "from messenger.message m "
                 "inner join messenger.tred t "
                 "on m.tred_id  = t.tred_id "
                 "where m.is_deleted is false "
                 "and t.tred_id = :tred_id "
                 "order by m.timestamp; ") 
        result = await self.fetch_all(query, tred_id=tred_id)
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

    async def create_new_super_account(self, super_id):
        query = SuperAccount.insert()
        result = await self.execute(query=query, super_id=super_id)
        return result

    async def create_new_user(self, values):
        query = UserAccount.insert()
        result = await self.execute(query, **values)
        return result

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

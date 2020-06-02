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
from typing import List, Generator, Dict, Optional, Iterable
from types import SimpleNamespace
from config import *
import models
from models import Message, UserAccount, SuperAccount, Tred
from crypto import *
from helpers import *


class DataBase:
    def __init__(self, connect_string):
        """Setting params for the connection"""
        self.database = Database(connect_string)

    def __enter__(self) -> "DataBase":
        asyncio.get_event_loop().run_until_complete(self.connect())
        return self

    def __exit__(self, exc_type=None, exc_value=None, traceback=None) -> None:
        asyncio.get_event_loop().run_until_complete(self.disconnect())

    async def connect(self):
        '''Establishing connection with the database
        Example for PostgreSQL - postgresql://scott:tiger@localhost/mydatabase
        Can be useful https://stackoverflow.com/questions/769683/show-tables-in-postgresql'''
        await self.database.connect()

        # set default schema
        await self.execute(f"set search_path to {schema_name()}")

    async def disconnect(self):
        await self.database.disconnect()

        # async def iterate(self, query: str, **kwargs) -> Generator[None, None, None]:
        #    '''
        #        https://www.encode.io/databases/database_queries/
        #        Actually returns a Generator of records
        #    '''
        #    result = await self.database.iterate(query=query, kwargs=kwargs)
        #    return result

    async def fetch_all(self, query: str, **kwargs) -> List:
        '''
            https://www.encode.io/databases/database_queries/
            Actually returns a List of Records
        '''
        result = await self.database.fetch_all(query=query, values=kwargs)
        return result

    async def fetch_one(self, query: str, **kwargs):
        '''
            https://www.encode.io/databases/database_queries/
            Actually returns single Record
        '''
        result = await self.database.fetch_one(query=query, values=kwargs)
        return result

    async def execute(self, query: str, **kwargs):
        '''
            https://www.encode.io/databases/database_queries/
            Actually returns something i cant explain
        '''
        result = await self.database.execute(query=query, values=kwargs)
        return result

    async def next_in(self, sequence_name: str) -> int:
        query = (
            f"select nextval('{sequence_name}')"
        )
        result = await self.fetch_one(query)
        return result['nextval']

    # async def execute_many(self, query: str, values: List[Dict]):
    #    '''
    #        https://www.encode.io/databases/database_queries/
    #        Actually returns something i cant explain
    #    '''
    #    result = await self.database.execute_many(query=query, values=values)
    #    return result

    async def get_user(self, login: str) -> Optional[UserAccount]:
        """ Get user with specified UUID. Just send str representation of UUID """
        query = (
            "select u.login, u.salt, u.pword, u.super_id "
            "from user_account u "
            "where u.login = :login"
        )
        result = await self.fetch_one(query=query, login=login)
        if result is None:
            return None
        else:
            return UserAccount(
                login=result['login'],
                salt=result['salt'],
                pword=result['pword'],
                super_id=result['super_id'])

    async def get_super(self, super_id: int) -> Optional[SuperAccount]:
        query = (
            "select s.super_id "
            "from super_account s "
            "where s.super_id = :super_id"
        )
        result = await self.fetch_one(query=query, super_id=super_id)
        return SuperAccount(super_id=result['super_id'])

    async def threads_with_id_above(self, thread_id: int) -> Iterable[Tred]:
        query = (
            "select tred_id, creator_id, header, body, timestamp "
            "from tred t "
            "where t.tred_id > :id "
            "order by t.tred_id"
        )
        threads = await self.fetch_all(query, id=thread_id)
        return map(
            lambda d: Tred(
                tred_id=d["tred_id"],
                creator_id=d["creator_id"],
                header=d["header"],
                body=d["body"],
                timestamp=d["timestamp"]),
            threads)

    async def messages_with_id_above(self, message_id: int) -> Iterable[Message]:
        query = (
            "select m.message_id, m.author_login, m.tred_id, m.timestamp, m.body, m.is_deleted "
            "from message m "
            "where m.message_id > :message_id "
            "order by m.message_id ")
        result = await self.fetch_all(query, message_id=message_id)
        return map(lambda d: Message(**d), result)

    async def all_people_in_personal_list(self, list_id: int):
        query = ("select * from personal_lists pl "
                 "inner join people_inlist pi "
                 "on pl.list_id = pi.list_id "
                 "where pl.list_id = :list_id")
        result = await self.fetch_all(query, list_id=list_id)
        return result

    async def all_messages_from_current_user(self, login: str):
        query = ("select * from message m "
                 "inner join user_account u "
                 "on m.author_login = u.login "
                 "where u.login::text = :login "
                 "order by m.timestamp;")
        result = await self.fetch_all(query, login=login)
        return result

    async def all_people_in_current_tred(self, tred_id: int):
        query = ("select * from tred_participation tp "
                 "inner join tred t "
                 "on tp.tred_id = t.tred_id "
                 "where t.tred_id = :tred_id;")
        result = await self.fetch_all(query, tred_id=tred_id)
        return result

    async def create_new_super_account(self) -> SuperAccount:
        next_id = await self.next_in("super_account_super_id_seq")
        query = (
            "insert into super_account (super_id)"
            "values (:next_id)"
        )
        await self.execute(query=query, next_id=next_id)
        return SuperAccount(super_id=next_id)

    async def create_new_user(self, super_acc: Optional[SuperAccount] = None) -> Tuple[UserAccount, str]:
        if super_acc is None:
            super_acc = await self.create_new_super_account()

        new_acc = UserAccount()

        password = generate_password()
        salt, hashed = hash_password(password)
        new_acc.salt = salt
        new_acc.pword = hashed
        new_acc.super_id = super_acc.super_id
        new_acc.login = generate_uuid4()

        # handle a minuscule chance that the login already exists in the
        # database
        count_user = (
            "select count(*) "
            "from user_account u "
            "where u.login = :login "
        )
        while (await self.fetch_one(count_user, login=new_acc.login))['count'] > 0:
            new_acc.login = generate_uuid4()

        query = (
            "insert into user_account (login, salt, pword, super_id)"
            "values (:login, :salt, :pword, :super_id)"
        )

        await self.execute(query, login=new_acc.login, salt=new_acc.salt,
                           pword=new_acc.pword, super_id=new_acc.super_id)
        return new_acc, password

    async def create_new_message(self, author_login: str, tred_id: int, body: str, file_id: int):
        query = (
            "insert into message(author_login, tred_id, body, file_id) "
            "values(:author_login, :tred_id, :body, :file_id);")
        result = await self.execute(query=query, author_login=author_login, tred_id=tred_id, body=body, file_id=file_id)
        return result

    async def create_new_tred(self, creator_id: int, header: str, body: str):
        query = ("insert into tred(creator_id, header, body) "
                 "values (:creator_id, :header, :body);")
        result = await self.execute(query=query, creator_id=creator_id, header=header, body=body)
        return result

    async def create_new_file(self, extension: str, filename: str, filedata: bytes):
        next_id = await self.next_in("files_file_id_seq")
        query = ("insert into files(file_id, extension, filename, data) "
                 "values (:next_id ,:extension, :filename, :filedata);")
        result = await self.execute(next_id=next_id, query=query, extension=extension, filename=filename, filedata=filedata)
        return next_id

    async def get_file(self, file_id: int):
        query = ("select * from files where file_id = :file_id")
        result = await self.fetch_one(query, file_id=file_id)
        return result

    async def create_new_tred_participation(self, values):
        query = (
            "insert into tred_participation(tred_id, superacc_id) "
            "values (:tred_id, :superacc_id);")
        result = await self.execute(query=query, **values)
        return result

    async def create_new_union_request(self, values):
        query = (
            "insert into union_requests(from_super_id, to_super_id, is_accepted) "
            "values (:from_super_id, :to_super_id, :is_accepted);")
        result = await self.execute(query=query, **values)
        return result

    async def create_new_personal_list(self, values):
        query = ("insert into personal_lists(list_name, owner_id) "
                 "values (:list_name, :owner_id);")
        result = await self.execute(query=query, **values)
        return result

    async def create_new_people_inlist(self, values):
        query = ("insert into people_inlist(list_id, friend_id) "
                 "values (:list_id, :friend_id);")
        result = await self.execute(query=query, **values)
        return result

    async def get_max_super_id(self):
        query = "select max(sa.super_id) from super_account sa;"
        result = await self.fetch_one(query)
        return result


async def create_file_test(db: Database):
    file_test_data = b"5487639875639"
    file_test_name = "wow"
    file_test_ext = "exe"
    result = await db.create_new_file(file_test_ext, file_test_name, file_test_data)
    print(type(result))


async def get_file_test(db: Database):
    id = 5
    result = await db.get_file(id)
    print(result)


async def init_database(db: DataBase):
    """For testing new db just run"""
    await db.create_new_user()
    await db.create_new_tred(1, "initial", "initial")


async def main() -> None:
    from config import connect_string
    db = DataBase(connect_string())

    await db.connect()

    # await init_database(db)
    
    # await create_file_test(db)
    # await get_file_test(db)



if __name__ == '__main__':
    asyncio.run(main())
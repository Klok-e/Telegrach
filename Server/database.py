'''
Part of server that works with database
You must change the config file varibles, that contain information about database connection
Current implementation requires postgresql and databases[postgresql] for async working
Just a queries part, no data manipulation
'''

import asyncio
import asyncpg
from databases import Database
from sqlalchemy.sql import select, text
from config import *
from models import *
from crypto import *
from helpers import *

class DataBase:

    def __init__(self, user, passwd, addr, schema='', voc=None):
        '''Setting params for the connection'''
        self._user = user # database user
        self._pw = passwd # password
        self._addr = addr # a tuple (host, port)
        self._schema = schema # schemaname 
        self._connection_string = None
        self._database = None

    def _t(self, key):
        '''Translates tablename according to self._voc'''
        return self._voc[key] if self._voc else key

    async def connect(self, dbname):
        '''Establishing connection with the database
        Example for PostgreSQL - postgresql://scott:tiger@localhost/mydatabase
        Can be useful https://stackoverflow.com/questions/769683/show-tables-in-postgresql'''
        self._connection_string = f'{dbname}://{self._user}:{self._pw}@{self._addr[0]}'
        self._database = Database(self._connection_string)
        await self._database.connect()

    async def FetchAll(self, query, **kwargs):
        '''https://www.encode.io/databases/database_queries/'''
        result = await self._database.fetch_all(query=query, values=kwargs)
        return result

    async def Execute(self, query, **kwargs):
        '''https://www.encode.io/databases/database_queries/'''
        result = await self._database.execute(query=query, values=kwargs)
        return result

    async def FetchOne(self, query, **kwargs):
        '''https://www.encode.io/databases/database_queries/'''
        result = await self._database.fetch_one(query=query, values=kwargs)
        return result

    async def ExecuteMany(self, query, **kwargs):
        '''https://www.encode.io/databases/database_queries/'''
        result = await self._database(query=query, values=kwargs)
        return result

    async def disconnect(self):
        await self._database.disconnect()

    async def AllMessagesInTred(self, tred_id):
        query = ("select * from messenger.message m "
                 "inner join messenger.tred t "
                 "on m.tred_id  = t.tred_id "
                 "where m.is_deleted is false "
                 "and m.tred_id = :tred_id "
                 "order by m.timestamp;")
        result = await self.FetchAll(query, tred_id=tred_id)
        return result

    async def AllPeopleInPersonalList(self, list_id):
        query = ("select * from messenger.personal_lists pl " 
                 "inner join messenger.people_inlist pi "
                 "on pl.list_id = pi.list_id "
                 "where pl.list_id = :list_id")
        result = await self.FetchAll(query, list_id=list_id)
        return result

    async def AllMessagesFromCurrentUser(self, login):
        query = ("select * from messenger.message m "
                 "inner join messenger.user_account u "
                 "on m.author_login = u.login "
                 "where u.login = :login "
                 "order by m.timestamp;")
        result = await self.FetchAll(query, login=login)
        return result

    async def CreateNewUser(self, login, salt, pword, super_id):
        values = {"login": login,
                  "salt": salt,
                  "pword": pword,
                  "super_id": super_id}
        query = UserAccount.insert()
        result = await self.Execute(query, **values)
        return result

    async def AllPeopleInCurrentTred(self, tred_id):
        query = ("select * from messenger.tred_participation tp "
                 "inner join messenger.tred t "
                 "where t.tred_id = :tred_id" 
                 "on tp.tred_id = t.tred_id;")
        result = await self.FetchAll(query, tred_id=tred_id)
        return result


async def main():
    a = DataBase(DB_USER, DB_PW, (DB_HOST, DB_PORT), SCHEMA_NAME, VOCAB)
    await a.connect(DB)
    
    pw = HashPassword("qwerty")
    res = await a.CreateNewUser(**create)
    # for i in res:
    #     print(dict(i.items()))
    await a.disconnect()

if __name__ == '__main__':
    asyncio.run(main())
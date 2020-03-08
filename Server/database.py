'''
Part of server that works with database
You must change the config file varibles, that contain information about database connection
Current implementation requires postgresql and databases[postgresql] for async working
'''
# Just testing the asyncing framework working

import asyncio
import asyncpg
from databases import Database
from sqlalchemy.sql import select, text
from config import *
from models import *

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

    async def AllMessagesInTred(self, tred_id):
        '''Returns all messages in tred with current id'''
        query = ("select * from messenger.message m "
                 "inner join messenger.tred t "
                 "on m.tred_id  = t.tred_id "
                 "where m.is_deleted is false "
                 "and m.tred_id = :tred_id "
                 "order by m.timestamp;")
        res = await self.MakeQuery(query, tred_id=tred_id)
        return res

    async def AllPeopleInFriendlist(self, list_id):
        '''Returns all people in personallist with specified id'''
        query = ("select * from messenger.personal_lists pl " 
                 "inner join messenger.people_inlist pi "
                 "on pl.list_id = pi.list_id "
                 "where pl.list_id = :list_id")
        res = await self.MakeQuery(query, list_id=list_id)
        return res

    async def FetchAll(self, query, **kwargs):
        result = await self._database.fetch_all(query=query, values=kwargs)
        return result

    async def disconnect(self):
        await self._database.disconnect()


async def main():
    a = DataBase(DB_USER, DB_PW, (DB_HOST, DB_PORT), SCHEMA_NAME, VOCAB)
    await a.connect(DB)
    res = await a.AllMessagesInTred(1)
    for i in res:
        print(i)
    await a.disconnect()

if __name__ == '__main__':
    asyncio.run(main())







'''
Part of server that works with database
You must change the config file varibles, that contain information about database connection
Current implementation requires postgresql and databases[postgresql] for async working
'''
# Just testing the asyncing framework working

import asyncio
from config import *
from databases import Database


class DataBase:

	def __init__(self, user, passwd, addr, schema='', voc=None):
		'''Setting params for the connection'''
		self._user = user # database user
		self._pw = passwd # password
		self._addr = addr # a tuple (host, port)
		self._schema = schema # schemaname 
		self._connection_string = None
		self._database = None
		self._voc = voc # translation tablenames

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

	async def SelectAll(self, table):
		try:
			table = self._t(table)
			query = f"SELECT * FROM {self._schema}.{table}"
			rows = await self._database.fetch_all(query=query)
			print(rows)
		except:
			pass

	async def disconnect(self):
		await self._database.disconnect()


async def main():
	a = DataBase(DB_USER, DB_PW, (DB_HOST, DB_PORT), SCHEMA_NAME, VOCAB)
	await a.connect(DB)
	await a.SelectAll('s_acc')
	await a.disconnect()

if __name__ == '__main__':
	asyncio.run(main())







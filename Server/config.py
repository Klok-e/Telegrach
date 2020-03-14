# constants to connect to the db *to edit*
DB = 'postgresql'
DB_USER = ''
DB_PW = ''
DB_HOST = 'localhost'
DB_PORT = 5432  # Default port for PostgreSQL

# Schema name in dbsm. Change if you have prefer another.
# It also assumes that you have created schema via schema_creation.sql
SCHEMA_NAME = 'messenger'

# Params for server address
HOST = 'localhost'
PORT = 9999
ADDRESS = HOST, PORT
SEPARATOR = b'\n\n\n\n'

# Size of buffer for recieving and sending data
BUFFSIZE = 2048

# Some logging params
# Levels:
#	CRITICAL 50
#	ERROR 40
#	WARNING 30
#	INFO 20
#	DEBUG 10
#	NOTSET 0
LOG_FILE_SERVER = "logs/server.log"
LOG_LEVEL_SERVER = 10
LOG_FORMAT_SERVER = "%(asctime)s In %(filename)s | %(levelname)s | %(message)s"

# Optionally you can declare pseudonims for the tables in your app
# If you decided to not then set it to Falsy-value
# Вообще вот сейчас пишу этот коммент и у меня сомнения по этому поводу
VOCAB = {
    'tr': 'tred',
    's_acc': 'super_account',
    'u_req': 'union_requests',
    'p_list': 'personal_lists',
    'p_inlist': 'people_inlist',
    't_ptp': 'tred_participation',
    'ms': 'message',
    'u_acc': 'user_account',
}

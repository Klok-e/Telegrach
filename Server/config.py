import os

# constants to connect to the db *to edit*
DB = 'postgresql'
DB_USER = ''
DB_PW = ''
DB_HOST = 'localhost'
DB_PORT = 5432  # Default port for PostgreSQL

# Schema name in dbsm. Change if you have prefer another.
# It also assumes that you have created schema via schema_creation.sql
SCHEMA_NAME = ''

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


def get_connect_string():
    # password is optional
    password = os.getenv("TELEGRACH_DB_PW", None) or DB_PW
    user = os.getenv("TELEGRACH_DB_USER", None) or DB_USER
    hostname = os.getenv("TELEGRACH_DB_HOST", None) or DB_HOST
    port = os.getenv("TELEGRACH_DB_PORT", None) or DB_PORT
    schema = os.getenv("TELEGRACH_SCHEMA_NAME", None) or SCHEMA_NAME
    if any(
            (us := not user,
             ho := not hostname,
             po := not port,
             sc := not schema)):
        msg = ""
        for v in [
            x[1] for x in [
                (us, "TELEGRACH_DB_USER"), (ho, "TELEGRACH_DB_HOST"),
                (po, "TELEGRACH_DB_PORT", (sc, "TELEGRACH_SCHEMA_NAME"))] if x[0]]:
            msg += f"Error: {v} not specified\n"
        raise RuntimeError(msg)
    return f'{DB}://{user}{":" if password else ""}{password}@{hostname}:{port}/{schema}'

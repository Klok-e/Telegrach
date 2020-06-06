import os

# constants to connect to the db *to edit*
__DB_USER = ''
__DB_PW = ''
__DB_HOST = 'localhost'
__DB_PORT = 5432  # Default port for PostgreSQL
__DB_NAME = ''

# Schema name in dbsm. Change if you have prefer another.
# It also assumes that you have created schema via schema_creation.sql
__SCHEMA_NAME = ''

# Params for server address
HOST = '0.0.0.0'
PORT = 9999
ADDRESS = HOST, PORT
SEPARATOR = b'\n\n\n\n'

# Size of buffer for recieving and sending data
BUFFSIZE = 2048

MESSAGE_PREFIX_SIZE = 4

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


def schema_name():
    return os.getenv("TELEGRACH_SCHEMA_NAME", None) or __SCHEMA_NAME


def connect_string():
    # password is optional
    password = os.getenv("TELEGRACH_DB_PW", None) or __DB_PW
    user = os.getenv("TELEGRACH_DB_USER", None) or __DB_USER
    hostname = os.getenv("TELEGRACH_DB_HOST", None) or __DB_HOST
    port = os.getenv("TELEGRACH_DB_PORT", None) or __DB_PORT
    schema = schema_name()
    db_name = os.getenv("TELEGRACH_DB_NAME", None) or __DB_NAME
    if any((not user, not hostname, not port,
            not schema, not db_name)):
        us = not user
        ho = not hostname
        po = not port
        sc = not schema
        db_n = not db_name
        msg = ""
        for v in [
            x[1] for x in [
                (us,
                 "TELEGRACH_DB_USER"),
                (ho,
                 "TELEGRACH_DB_HOST"),
                (po,
                 "TELEGRACH_DB_PORT",
                 (sc,
                  "TELEGRACH_SCHEMA_NAME"),
                 (db_n,
                  "TELEGRACH_DB_NAME"))] if x[0]]:
            msg += f"Error: {v} not specified\n"
        raise RuntimeError(msg)
    return f'postgresql://{user}{":" if password else ""}{password}@{hostname}:{port}/{db_name}'

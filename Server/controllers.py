'''
    A layer between the database and server.
    Designed for datamanipulation and validation
'''

from sqlalchemy.sql import func
from typing import Tuple, Dict
from config import *
from helpers import *
from crypto import *

def create_user(super_id: int) -> Tuple[Tuple[str, str], Dict]:
    ''' 
        Creates new user
        Returns users login and password and dict with values for database.
    '''
    login = generate_uuid4()
    password = generate_password()
    salt, _hash = hash_password(password)
    values = {
        "login": login,
        "pword": _hash,
        "salt": salt,
        "super_id": super_id,
    }
    return ((login, password), values)

def create_union_request(_from, to):
    time = func.now()
    is_accepted = False
    values = {
        "from_super_id": _from,
        "to_super_id": to,
        "timestamp": time,
        "is_accepted": is_accepted,
    }
    return values

def create_tred(max_id, creator_id, header, body):
    _id = max_id + 1
    time = func.now()
    values = {
        "tred_id": _id,
        "creator_id": creator_id,
        "header": header,
        "body": body,
        "timestamp": time,
    }
    return values

def create_tred_participation(max_id, tred_id, super_id):
    _id = max_id + 1
    values = {
        "participation_id": _id,
        "tred_id": tred_id,
        "superacc_id": super_id,
    }
    return values

def create_message(max_id, author, tred_id, body):
    _id = max_id + 1
    time = func.now()
    is_deleted = False
    # if answered:
    #   body += ">>>" + answered

    values = {
        "message_id": _id,
        "author_login": author,
        "tred_id": tred_id,
        "timestamp": time,
        "body": body,
        "is_deleted": is_deleted
    }
    return values

def create_personal_list(max_id, list_name, owner_id):
    _id = max_id + 1
    values = {
        "list_id": _id,
        "list_name": list_name,
        "owner_id": owner_id
    }
    return values

def create_people_inlist(list_id, friend_id):
    values = {
        "list_id": list_id,
        "friend_id": friend_id
    }
    return values

def validate_user(values: Dict, password: str):
    stored_salt = values["salt"]
    stored_pword = values["pword"]
    result = validate_password(stored_salt, stored_pword, password)

if __name__ == '__main__':
    print(create_user(1))
    print(create_people_inlist(1,1))
    print(create_personal_list(1, "test", 1))
    print(create_message(1, generate_uuid4(), 1, "test"))
    print(create_tred_participation(1, 1, 1))
    print(create_tred(1, 1, "test_header", "test_body"))
    print(create_union_request(1, 1))

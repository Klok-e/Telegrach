'''
	A layer between the database and server.
	Designed for datamanipulation and validation
'''

from sqlalchemy.sql import func
from typing import Tuple, Dict
from config import *
from helpers import *
from crypto import *


def CreateUser(super_id: int) -> Tuple[Tuple[str, str], Dict]:
	''' 
		Creates new user
		Returns users login and password and dct for database.
	'''
	login = generate_uuid()
	password = GeneratePassword()
	salt, _hash = HashPassword(password)
	values = {
		"login": login,
		"pword": _hash,
		"salt": salt,
		"super_id": super_id,
	}
	return ((login, password), values)

def CreateUnionRequest(_from, to):
	time = func.now()
	is_accepted = False
	values = {
		"from_super_id": _from,
		"to_super_id": to,
		"timestamp": time,
		"is_accepted": is_accepted,
	}
	return values

def CreateTred(max_id, creator_id, header, body):
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

def CreateTredParticipation(max_id, tred_id, super_id):
	_id = max_id + 1
	values = {
		"participation_id": _id,
		"tred_id": tred_id,
		"superacc_id": super_id,
	}
	return values

def CreateMessage(max_id, author, tred_id, body):
	_id = max_id + 1
	time = func.now()
	is_deleted = False
	# if answered:
	# 	body += ">>>" + answered

	values = {
		"message_id": _id,
		"author_login": author,
		"tred_id": tred_id,
		"timestamp": time,
		"body": body,
		"is_deleted": is_deleted
	}
	return values

def CreatePersonalList(max_id, list_name, owner_id):
	_id = max_id + 1
	values = {
		"list_id": _id,
		"list_name": list_name,
		"owner_id": owner_id
	}
	return values

def CreatePeopleInlist(list_id, friend_id):
	values = {
		"list_id": list_id,
		"friend_id": friend_id
	}

if __name__ == '__main__':
	pass
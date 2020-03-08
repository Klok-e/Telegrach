import uuid
from sqlalchemy import Column, Integer, String, MetaData, Table, ForeignKey, DateTime, Boolean
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.sql import func
from config import SCHEMA_NAME

def generate_uuid():
    return str(uuid.uuid4())

metadata = MetaData(schema=SCHEMA_NAME)

SuperAccount = Table("super_account", metadata,
                     Column('super_id', Integer, primary_key=True))

UserAccount = Table("user_account", metadata,
                    Column("login", UUID, primary_key=True, default=generate_uuid),
                    Column("salt", String, nullable=False),
                    Column("pword", String, nullable=False),
                    Column("super_id", Integer, ForeignKey(SCHEMA_NAME + ".super_account.super_id")))

UnionRequests = Table("union_requests", metadata,
                      Column("from_super_id", Integer, ForeignKey(SCHEMA_NAME + ".super_account.super_id"), primary_key=True),
                      Column("to_super_id", Integer, ForeignKey(SCHEMA_NAME + ".super_account.super_id"), primary_key=True),
                      Column("timestamp", DateTime, nullable=False, default=func.now(), primary_key=True),
                      Column("is_accepted", Boolean, nullable=False, primary_key=True))

Tred = Table("tred", metadata,
             Column("tred_id", Integer, primary_key=True),
             Column("creator_id", Integer, ForeignKey(SCHEMA_NAME + ".super_account.super_id")),
             Column("header", String, nullable=False),
             Column("body", String),
             Column("timestamp", DateTime, nullable=False, default=func.now()))

TredParticipation = Table("tred_participation", metadata,
                          Column("participation_id", Integer, primary_key=True),
                          Column("tred_id", Integer, ForeignKey(SCHEMA_NAME + ".tred.tred_id")),
                          Column("superacc_id", Integer, ForeignKey(SCHEMA_NAME + ".super_account.super_id")))

Message = Table("message", metadata,
                Column("message_id", Integer, primary_key=True),
                Column("author_login", UUID, ForeignKey(SCHEMA_NAME + ".user_account.login")),
                Column("tred_id", Integer, ForeignKey(SCHEMA_NAME + ".tred.tred_id")),
                Column("timestamp", DateTime, nullable=False, default=func.now()),
                Column("body", String),
                Column("is_deleted", Boolean, default=True))

PersonalLists = Table("personal_lists", metadata,
                      Column("list_id", Integer, primary_key=True),
                      Column("list_name", String, nullable=False),
                      Column("owner_id", Integer, ForeignKey(SCHEMA_NAME + ".super_account.super_id")))

PeopleInList = Table("people_inlist", metadata,
                     Column("list_id", Integer, ForeignKey(SCHEMA_NAME + ".personal_lists.list_id")),
                     Column("friend_id", Integer, ForeignKey(SCHEMA_NAME + ".super_account.super_id")))
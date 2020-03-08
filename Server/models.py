from sqlalchemy import Column, Integer, String, MetaData, Table, ForeignKey, DateTime, Boolean
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.types import BigInteger
from sqlalchemy.sql import func
from helpers import generate_uuid
from config import SCHEMA_NAME


metadata = MetaData(schema=SCHEMA_NAME)

SuperAccount = Table("super_account", metadata,
                     Column('super_id', BigInteger, primary_key=True))

UserAccount = Table("user_account", metadata,
                    Column("login", UUID, primary_key=True, default=generate_uuid),
                    Column("salt", String(32), nullable=False),
                    Column("pword", String(128), nullable=False),
                    Column("super_id", BigInteger, ForeignKey(SCHEMA_NAME + ".super_account.super_id")))

UnionRequests = Table("union_requests", metadata,
                      Column("from_super_id", BigInteger, ForeignKey(SCHEMA_NAME + ".super_account.super_id"), primary_key=True),
                      Column("to_super_id", BigInteger, ForeignKey(SCHEMA_NAME + ".super_account.super_id"), primary_key=True),
                      Column("timestamp", DateTime, nullable=False, default=func.now(), primary_key=True),
                      Column("is_accepted", Boolean, nullable=False, primary_key=True))

Tred = Table("tred", metadata,
             Column("tred_id", BigInteger, primary_key=True),
             Column("creator_id", BigInteger, ForeignKey(SCHEMA_NAME + ".super_account.super_id")),
             Column("header", String(128), nullable=False),
             Column("body", String(256)),
             Column("timestamp", DateTime, nullable=False, default=func.now()))

TredParticipation = Table("tred_participation", metadata,
                          Column("participation_id", BigInteger, primary_key=True),
                          Column("tred_id", BigInteger, ForeignKey(SCHEMA_NAME + ".tred.tred_id")),
                          Column("superacc_id", BigInteger, ForeignKey(SCHEMA_NAME + ".super_account.super_id")))

Message = Table("message", metadata,
                Column("message_id", BigInteger, primary_key=True),
                Column("author_login", UUID, ForeignKey(SCHEMA_NAME + ".user_account.login")),
                Column("tred_id", BigInteger, ForeignKey(SCHEMA_NAME + ".tred.tred_id")),
                Column("timestamp", DateTime, nullable=False, default=func.now()),
                Column("body", String(256)),
                Column("is_deleted", Boolean, default=True))

PersonalLists = Table("personal_lists", metadata,
                      Column("list_id", BigInteger, primary_key=True),
                      Column("list_name", String(32), nullable=False),
                      Column("owner_id", BigInteger, ForeignKey(SCHEMA_NAME + ".super_account.super_id")))

PeopleInList = Table("people_inlist", metadata,
                     Column("list_id", BigInteger, ForeignKey(SCHEMA_NAME + ".personal_lists.list_id")),
                     Column("friend_id", BigInteger, ForeignKey(SCHEMA_NAME + ".super_account.super_id")))
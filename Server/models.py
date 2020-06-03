from sqlalchemy import Column, Integer, String, MetaData, \
    Table, ForeignKey, DateTime, Boolean, \
    Binary
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.types import BigInteger, LargeBinary
from sqlalchemy.sql import func, expression
from helpers import generate_uuid4
import config
from sqlalchemy.ext.declarative import declarative_base

Base = declarative_base(metadata=MetaData(schema=config.schema_name()))


class SuperAccount(Base):
    __tablename__ = "super_account"
    # the first integer is automatically autoincrement
    super_id = Column('super_id', BigInteger, primary_key=True)


class UserAccount(Base):
    __tablename__ = "user_account"
    login = Column(UUID, primary_key=True, default=generate_uuid4)
    nickname = Column(String(20), nullable=True)
    salt = Column(String(32), nullable=False)
    pword = Column(String(128), nullable=False)
    super_id = Column(BigInteger, ForeignKey("super_account.super_id"))
    last_request_time = Column(
        DateTime,
        nullable=False,
        server_default=func.now())


class UnionRequests(Base):
    __tablename__ = "union_requests"
    from_super_id = Column(
        BigInteger,
        ForeignKey("super_account.super_id"),
        primary_key=True)
    to_super_id = Column(
        BigInteger,
        ForeignKey("super_account.super_id"),
        primary_key=True)
    timestamp = Column(
        DateTime,
        nullable=False,
        default=func.now(),
        primary_key=True)
    is_accepted = Column(Boolean, nullable=False, primary_key=True)


class Tred(Base):
    __tablename__ = "tred"
    tred_id = Column(BigInteger, primary_key=True)
    creator_id = Column(BigInteger, ForeignKey("super_account.super_id"))
    header = Column(String(128), nullable=False)
    body = Column(String(256))
    timestamp = Column(DateTime, nullable=False, server_default=func.now())


class TredParticipation(Base):
    __tablename__ = "tred_participation"
    participation_id = Column(BigInteger, primary_key=True)
    tred_id = Column(BigInteger, ForeignKey("tred.tred_id"))
    superacc_id = Column(BigInteger, ForeignKey("super_account.super_id"))


class Message(Base):
    __tablename__ = "message"
    message_id = Column(BigInteger, primary_key=True)
    author_login = Column(UUID, ForeignKey("user_account.login"))
    tred_id = Column(BigInteger, ForeignKey("tred.tred_id"))
    timestamp = Column(DateTime, nullable=False, server_default=func.now())
    body = Column(String(256))
    file_id = Column(BigInteger, ForeignKey("files.file_id"), nullable=True)
    is_deleted = Column(
        Boolean,
        nullable=False,
        server_default=expression.false())


class File(Base):
    __tablename__ = "files"
    file_id = Column(BigInteger, primary_key=True)
    extension = Column(String(10))
    filename = Column(String(100), nullable=False)
    data = Column(LargeBinary, nullable=False)


class PersonalLists(Base):
    __tablename__ = "personal_lists"
    list_id = Column(BigInteger, primary_key=True)
    list_name = Column(String(32), nullable=False)
    owner_id = Column(BigInteger, ForeignKey("super_account.super_id"))


class PeopleInList(Base):
    __tablename__ = "people_inlist"
    list_id = Column(
        BigInteger,
        ForeignKey("personal_lists.list_id"),
        primary_key=True)
    friend_id = Column(
        BigInteger,
        ForeignKey("super_account.super_id"),
        primary_key=True)

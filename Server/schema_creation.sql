create schema messenger;

SELECT * FROM pg_available_extensions;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
SELECT * FROM pg_extension;

create table messenger.super_account(
	super_id bigserial primary keybigint references messenger.super_account(super_id),
);

create table messenger.user_account(
	login uuid primary key default uuid_generate_v4(),
	salt char(32) not null,
	pword char(128) not null,
	super_id bigint references messenger.super_account(super_id)
);

create table messenger.union_requests(
	from_super_id bigint references messenger.super_account(super_id),
	to_super_id bigint references messenger.super_account(super_id),
	timestamp timestamp not null default localtimestamp,
	is_accepted boolean not null,
	PRIMARY KEY (from_super_id, to_super_id,is_accepted, timestamp)
);

create table messenger.tred(
	tred_id bigserial primary key,
	creator_id bigint references messenger.super_account(super_id),
	header varchar(128) not null,
	body varchar(256),
	timestamp timestamp not null default localtimestamp 
);

create table messenger.tred_participation(
	participation_id bigserial primary key,
	tred_id bigint references messenger.tred(tred_id),
	superacc_id bigint references messenger.super_account(super_id)
);

create table messenger.message(
	message_id bigserial primary key,
	author_login uuid references messenger.user_account(login),
	tred_id bigint references messenger.tred(tred_id),
	timestamp timestamp not null default localtimestamp,
	body varchar(256),
	is_deleted boolean default true
);

create table messenger.personal_lists(
	list_id bigserial primary key,
	list_name varchar(32) not null,
	owner_id bigint references messenger.super_account(super_id)
);

create table messenger.people_inlist(
	list_id bigint references messenger.personal_lists(list_id),
	friend_id bigint references messenger.super_account(super_id)
);

-- If you`ve already created schema - Run this. Old values was too short to store generated password
-- ALTER TABLE messenger.user_account 
--     ALTER COLUMN salt TYPE CHAR(32),
--     ALTER COLUMN pword TYPE CHAR(128);
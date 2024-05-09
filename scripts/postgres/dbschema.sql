CREATE TABLE IF NOT EXISTS users (
    id 	BIGSERIAL PRIMARY KEY,
    fullname VARCHAR(50) NOT NULL,
    username VARCHAR(50) NOT NULL,
	password VARCHAR(50) NOT NULL,
	email VARCHAR(50),
	role integer, --1: admin, 2: trader,
	status integer --0: inactive, 1: active, 2: deleted
);

CREATE TABLE IF NOT EXISTS usersettings (
    id 	BIGSERIAL PRIMARY KEY,
    userid BIGINT NOT NULL,
    apikey VARCHAR(40),
	secretkey VARCHAR(50),
	passphrase VARCHAR(50),
	telechannel VARCHAR(20)
);

CREATE TABLE IF NOT EXISTS configs (
    id 	BIGSERIAL PRIMARY KEY,
	customid VARCHAR(100),
    userid BIGINT NOT NULL,
    symbol VARCHAR(50),
	positionside varchar(30),
	ordertype integer,
	orderchange numeric(4,2),
	amount numeric(8,2),
	originamount numeric(8,2),
	increaseamountpercent INTEGER,
	amountlimit numeric(8,2),
	amountexpire integer,
	increaseocpercent INTEGER,
	createdby varchar(200),
	createddate timestamp,
	editeddate timestamp,
	expire INTEGER,
	isactive boolean 
);

CREATE TABLE IF NOT EXISTS scanner (
    id 	BIGSERIAL PRIMARY KEY,
    userid BIGINT NOT NULL,
    title VARCHAR(500),
	positionside varchar(30),
	ordertype integer,
	orderchange numeric(4,2),
	elastic INTEGER,
	turnover numeric(8,2),
	onlypairs JSONB,
	blacklist JSONB,
	ocnumber INTEGER,
	amount numeric(8,2),
	amountlimit numeric(8,2),
	amountexpire integer,
	autoamount INTEGER,
	configexpire INTEGER,
	isactive boolean 
);

CREATE TABLE IF NOT EXISTS scannersetting (
    id 	BIGSERIAL PRIMARY KEY,
    userid BIGINT NOT NULL,
	blacklist JSONB,
	maxopen INTEGER
);

INSERT INTO users(fullname, username, password, email, role, status) VALUES('admin','admin','23D42F5F3F66498B2C8FF4C20B8C5AC826E47146','admin@bisurfer.xyz',1,1);
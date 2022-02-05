CREATE TABLE Users
(
id INT IDENTITY PRIMARY KEY not null,
email VARCHAR(30) not null,
password password(60) not null,
ROLE INT not null DEFAULT(0),
name VARCHAR(60) not null
); -- 150 + 8 (int) = 158 bytes
CREATE TABLE Materials
(
id INT IDENTITY PRIMARY KEY not null,
name VARCHAR(60) not null,
description VARCHAR(500) not null default(''),
folder VARCHAR(30) not null,
imgs INT not null, default(0), -- will be removed
comments VARCHAR(500) not null default(''),
category VARCHAR(100) not null,
author VARCHAR(60) not null
); -- 1250 + 8 = 1258 bytes
CREATE TABLE Tasks
(
id INT IDENTITY PRIMARY KEY not null,
author VARCHAR(60) not null,
name VARCHAR(60) not null,
description VARCHAR(500) not null default(''),
comments VARCHAR(500) not null default(''),
flags VARCHAR(500) not null default(''),
imgs INT not null default(0), -- will be removed
links VARCHAR(300) not null default(''),
fldr VARCHAR(30) not null,
role INT not null default(0),
category VARCHAR(50) not null default('')
); -- 2000 + 12 = 2012 bytes
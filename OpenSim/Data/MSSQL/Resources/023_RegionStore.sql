BEGIN TRANSACTION

ALTER TABLE regionsettings DROP COLUMN loaded_creation_date
ALTER TABLE regionsettings DROP COLUMN loaded_creation_time
ALTER TABLE regionsettings ADD loaded_creation_datetime int NOT NULL default 0

COMMIT

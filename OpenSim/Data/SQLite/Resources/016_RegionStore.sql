BEGIN;

ALTER TABLE prims ADD COLUMN VolumeDetect INTEGER NOT NULL DEFAULT 0;

COMMIT;

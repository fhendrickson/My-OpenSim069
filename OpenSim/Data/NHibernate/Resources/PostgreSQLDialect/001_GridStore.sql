CREATE TABLE Regions (
  RegionID VARCHAR(36) NOT NULL,
  OwnerID VARCHAR(36) DEFAULT NULL,
  OriginID VARCHAR(36) DEFAULT NULL,
  RegionHandle BIGINT DEFAULT NULL,
  RegionName VARCHAR(32) DEFAULT NULL,
  RegionRecvKey VARCHAR(128) DEFAULT NULL,
  RegionSendKey VARCHAR(128) DEFAULT NULL,
  RegionSecret VARCHAR(128) DEFAULT NULL,
  RegionDataURI VARCHAR(255) DEFAULT NULL,
  ServerIP VARCHAR(64) DEFAULT NULL,
  ServerPort INT DEFAULT NULL,
  ServerURI VARCHAR(255) DEFAULT NULL,
  RegionLocX INT DEFAULT NULL,
  RegionLocY INT DEFAULT NULL,
  RegionLocZ INT DEFAULT NULL,
  EastOverrideHandle BIGINT DEFAULT NULL,
  WestOverrideHandle BIGINT DEFAULT NULL,
  SouthOverrideHandle BIGINT DEFAULT NULL,
  NorthOverrideHandle BIGINT DEFAULT NULL,
  RegionAssetURI VARCHAR(255) DEFAULT NULL,
  RegionAssetRecvKey VARCHAR(128) DEFAULT NULL,
  RegionAssetSendKey VARCHAR(128) DEFAULT NULL,
  RegionUserURI VARCHAR(255) DEFAULT NULL,
  RegionUserRecvKey VARCHAR(128) DEFAULT NULL,
  RegionUserSendKey VARCHAR(128) DEFAULT NULL, 
  RegionMapTextureId VARCHAR(36) DEFAULT NULL,
  ServerHttpPort INT DEFAULT NULL, 
  ServerRemotingPort INT DEFAULT NULL,
  PRIMARY KEY (RegionID)
);

CREATE INDEX RegionNameIndex ON Regions (RegionName);
CREATE INDEX RegionHandleIndex ON Regions (RegionHandle);
CREATE INDEX RegionHandlesIndex ON Regions (EastOverrideHandle,WestOverrideHandle,SouthOverrideHandle,NorthOverrideHandle);

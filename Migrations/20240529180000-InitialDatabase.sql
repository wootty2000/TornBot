CREATE TABLE IF NOT EXISTS `Migrations` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `Name` varchar(256) NOT NULL,
    `RunAt` datetime(6) NULL,
    PRIMARY KEY (`Id`)
);

START TRANSACTION;

CREATE TABLE IF NOT EXISTS `Settings` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `Module` longtext NOT NULL,
    `Setting` longtext NOT NULL,
    `Value` longtext NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE IF NOT EXISTS `TornPlayers` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `Name` longtext NOT NULL,
    `Level` smallint unsigned NOT NULL,
    `LastUpdated` datetime(6) NOT NULL,
    `Faction_Name` longtext NOT NULL,
    `Faction_ID` int unsigned NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE IF NOT EXISTS `ApiKeys` (
    `PlayerId` int unsigned NOT NULL AUTO_INCREMENT,
    `FactionId` int unsigned NOT NULL,
    `AccessLevel` smallint unsigned NOT NULL,
    `ApiKey` longtext NOT NULL,
    `TornStatsApiKey` longtext NOT NULL,
    `TornApiAddedTimestamp` datetime(6) NULL,
    `TornStatsApiAddedTimestamp` datetime(6) NULL,
    `TornLastUsed` datetime(6) NULL,
    `TornStatsLastUsed` datetime(6) NULL,
    PRIMARY KEY (`PlayerId`)
);

CREATE TABLE IF NOT EXISTS `BattleStats` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `PlayerId` int unsigned NOT NULL,
    `Strength` bigint unsigned NOT NULL,
    `StrengthTimestamp` datetime(6) NOT NULL,
    `Defense` bigint unsigned NOT NULL,
    `DefenseTimestamp` datetime(6) NOT NULL,
    `Speed` bigint unsigned NOT NULL,
    `SpeedTimestamp` datetime(6) NOT NULL,
    `Dexterity` bigint unsigned NOT NULL,
    `DexterityTimestamp` datetime(6) NOT NULL,
    `Total` bigint unsigned NOT NULL,
    `TotalTimestamp` datetime(6) NOT NULL,
    `Timestamp` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
);

COMMIT;
CREATE TABLE LoadOuts
(
    `PlayerId` int unsigned NOT NULL PRIMARY KEY,
    `Timestamp` datetime(6) NOT NULL,
    `PrimaryWeapon` bigint unsigned,
    `PrimaryWeaponMods` VARCHAR(255),
    `SecondaryWeapon` bigint unsigned,
    `SecondaryWeaponMods` VARCHAR(255),
    `MeleeWeapon` bigint unsigned,
    `TemporaryWeapon` bigint unsigned,
    `HelmetArmor` bigint unsigned,
    `ChestArmor` bigint unsigned,
    `PantsArmor` bigint unsigned,
    `GlovesArmor` bigint unsigned,
    `BootsArmor` bigint unsigned
);

CREATE TABLE ArmoryItems
(
    `Uid` bigint unsigned NOT NULL PRIMARY KEY,
    `Id` int unsigned NOT NULL,
    `Name` VARCHAR(255) NOT NULL,
    `Type` int unsigned,
    `Color` int unsigned,
    `Damage` float unsigned,
    `Accuracy` float unsigned,
    `Armor` float unsigned,
    `Quality` float unsigned,
    `BonusId1` int unsigned,
    `BonusVal1` int unsigned,
    `BonusId2` int unsigned,
    `BonusVal2` int unsigned,
    `BonusId3` int unsigned,
    `BonusVal3` int unsigned
);

CREATE TABLE ArmoryItemRWBonus
(
    `Id` int unsigned NOT NULL,
    `Value` int unsigned NULL,
    `Bonus` VARCHAR(255) NOT NULL,
    `Description` VARCHAR(512) NOT NULL
);

CREATE TABLE WeaponMods
(
    `Id` int unsigned NOT NULL PRIMARY KEY,
    `Title` VARCHAR(255) NOT NULL,
    `Description` VARCHAR(255) NOT NULL
);
CREATE TABLE PlayerStatus
(
    Id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    PlayerId INT UNSIGNED NOT NULL,
    WeekStarting DATE NOT NULL,
    StatusLog JSON NOT NULL,
    UNIQUE KEY (PlayerId, WeekStarting),
    INDEX idx_PlayerWeek (PlayerId, WeekStarting)
);

ALTER TABLE TornPlayers
    DROP COLUMN Faction_Name;
ALTER TABLE TornPlayers
    CHANGE Faction_ID FactionId INT UNSIGNED NOT NULL AFTER Level;


CREATE TABLE TornFactions
(
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(256) NOT NULL,
    Tag VARCHAR(16) NOT NULL DEFAULT '',
    TagImage VARCHAR(256) NOT NULL DEFAULT '',
    Monitor BIT DEFAULT 0 NOT NULL
)

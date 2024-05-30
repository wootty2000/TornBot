alter table ApiKeys
    change ApiKey TornApiKey longtext not null;#

alter table ApiKeys
    change AccessLevel TornAccessLevel smallint unsigned not null after TornApiKey;

alter table ApiKeys
    add TornState SMALLINT UNSIGNED default 0 not null after TornAccessLevel;

alter table ApiKeys
    modify TornStatsApiKey longtext not null after TornApiAddedTimestamp;

alter table ApiKeys
    modify TornLastUsed datetime(6) null after TornState;

alter table ApiKeys
    modify TornStatsLastUsed datetime(6) null after TornStatsApiKey;

UPDATE ApiKeys SET TornState = 1;
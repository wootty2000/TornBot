//
// TornBot
//
// Copyright (C) 2024 TornBot.com
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using Microsoft.Extensions.Hosting;
using TornBot.Database;
using TornBot.Services.TornApi.Services;
using TornBot.Services.TornStatsApi.Services;

namespace TornBot.Services.Players.Service
{
    public class PlayersService : IHostedService
    {
        private const int MaxStatsCacheAge = 14;
        private const int MaxTornPlayerCacheAge = 7;

        private DatabaseContext _database;
        private TornApiService _torn;
        private TornStatsApiService _tornStats;

        public PlayersService(
            DatabaseContext database,
            TornApiService torn,
            TornStatsApiService tornStats
        ) 
        {
            _database = database;
            _torn = torn;
            _tornStats = tornStats;
        }

        
        public Entities.TornPlayer? GetPlayer(UInt32 id, bool forceUpdate = false)
        {
            Entities.TornPlayer? tornPlayer;

            // Lets try and get a record from the database. If there is no record, we get given a null
            Database.Entities.TornPlayer? dbPlayer = _database.TornPlayers.Where(s => s.Id == id).FirstOrDefault();

            // Check what we got from the database
            if (dbPlayer == null)
            {
                // There was no record in the database. Fetch from Torn API and add to the database
                tornPlayer = _torn.GetPlayer(id);
                if (tornPlayer != null)
                {
                    _database.TornPlayers.Add(new Database.Entities.TornPlayer(tornPlayer));
                    _database.SaveChanges();

                    return tornPlayer;
                }
                else
                    return null;
            }
            else if (dbPlayer.LastUpdated.CompareTo(DateTime.UtcNow.AddDays(-MaxTornPlayerCacheAge)) < 0 || forceUpdate)
            {
                // We have a record in the database, but it's stale so we need to update with a fresh pull
                // Or, we were told to idOrName get a fresh copy
                tornPlayer = _torn.GetPlayer(id);
                if(tornPlayer != null)
                {
                    _database.TornPlayers.Update(new Database.Entities.TornPlayer(dbPlayer.Id, tornPlayer));
                    _database.SaveChanges();

                    return tornPlayer;
                }
                else
                    return dbPlayer.ToTornPlayer();
            }
            else
            {
                // The record from the database was ok to use
                return dbPlayer.ToTornPlayer();
            }
        }
        

        public Entities.TornPlayer GetPlayer(string name, bool forceUpdate = false)
        {
            TornBot.Entities.TornPlayer? tornPlayer;
            
            // Lets try and get a record from the database. If there is no record, we get given a null
            Database.Entities.TornPlayer? dbPlayer = _database.TornPlayers.Where(s => s.Name  == name).FirstOrDefault();

            if (dbPlayer != null)
            {
                //Check to see if the local cache is stale or forceUpdate is true
                if (dbPlayer.LastUpdated.CompareTo(DateTime.Now.AddDays(-MaxTornPlayerCacheAge)) < 0 || forceUpdate)
                {
                    //Lets pull a copy from Torn, based on the Id from the local cache
                    tornPlayer = _torn.GetPlayer(dbPlayer.Id);
                    if (tornPlayer != null && tornPlayer.Name == name)
                    {
                        //The players has not changed their name. We now have a valid player Id
                        _database.TornPlayers.Update(new TornBot.Database.Entities.TornPlayer(tornPlayer));
                        _database.SaveChanges();
                        return tornPlayer;
                    }
                    else
                    {
                        //The local cache name doesnt match the return from Torn. 
                        //We can use TornStats spy lookup on a name
                        Entities.BattleStats tsBattleStats = GetBattleStats(name, true);
                        if (tsBattleStats != null)
                        {
                            return GetPlayer(tsBattleStats.PlayerId, true);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    return dbPlayer.ToTornPlayer();
                }
            }
            else
            {
                Entities.BattleStats battleStats = GetBattleStats(name, true);
                if (battleStats != null)
                {
                    UInt32 id = battleStats.PlayerId;
                    
                    tornPlayer = _torn.GetPlayer(id);
                    if (tornPlayer != null)
                    {
                        _database.TornPlayers.Add(new Database.Entities.TornPlayer(tornPlayer));
                        _database.SaveChanges();

                        return tornPlayer;
                    }
                    else
                        return null; 
                }
                else
                {
                    return null;
                }
            }
        }

        public Entities.BattleStats? GetBattleStats(string IdOrName, bool checkUpdate = false)
        {
            UInt32 idUInt;
            Database.Entities.BattleStats? dbBattleStats = null;
            
            //If we have checkUpdate we want to skip this so that we try to get something from TornStats
            if (!checkUpdate)
            {
                if (UInt32.TryParse(IdOrName, out idUInt))
                {
                    dbBattleStats = _database.BattleStats.Where(s => s.PlayerId == idUInt).FirstOrDefault();
                }
                else
                {
                    Database.Entities.TornPlayer? dbPlayer =
                        _database.TornPlayers.Where(s => s.Name == IdOrName).FirstOrDefault();
                    if (dbPlayer != null)
                    {
                        dbBattleStats = _database.BattleStats.Where(s => s.PlayerId == dbPlayer.Id).FirstOrDefault();
                    }
                    else
                    {
                        dbBattleStats = null;
                    }

                }
            }

            //If
            // 1) we get a valid stats from the Database
            // 2) the stats are less than 2 weeks old
            // 3) we are not asked to check for an external update
            if (
                dbBattleStats != null &&
                dbBattleStats.Timestamp.CompareTo(DateTime.UtcNow.AddDays(-MaxStatsCacheAge)) < 0 &&
                !checkUpdate
            )
            {
                // All is good, we can return what we have;
                return dbBattleStats.ToBattleStats();
            }
            else
            {
                // We need to figure out what, if anything, we can send back
                //Lets try and get some battleStats from TornStats
                Entities.BattleStats tsBattleStats = _tornStats.GetPlayerStats(IdOrName);

                //If we have nothing, end game
                if(dbBattleStats == null && tsBattleStats == null)
                {
                    //We have nothing, so hand back null
                    return null;
                }
                else if (tsBattleStats == null)
                {
                    //We have something from the database and but nothing else
                    return dbBattleStats.ToBattleStats();
                }
                else
                {
                    // We might have something from the DB
                    // We do have something from TS

                    //If we dont have anything in the database OR
                    //TS stat is newer than what we have
                    if (dbBattleStats == null || tsBattleStats.BattleStatsTimestamp.CompareTo(dbBattleStats.Timestamp) > 0)
                    {
                        //TS is newer (or local one doesnt exist)
                        Database.Entities.BattleStats newDbBattleStats = new Database.Entities.BattleStats(tsBattleStats);
                        _database.BattleStats.Add(newDbBattleStats);
                        _database.SaveChanges();

                        return tsBattleStats;
                    }
                    else
                    {
                        // Return what was in the database. Its either newer or same age as TS
                        return dbBattleStats.ToBattleStats();
                    }
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }
    }
}

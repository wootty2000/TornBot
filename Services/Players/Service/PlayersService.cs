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

        
        public Entities.TornPlayer GetPlayer(UInt32 id, bool forceUpdate = false)
        {
            // Lets try and get a record from the database. If there is no record, we get given a null
            Database.Entities.TornPlayer? dbPlayer = _database.TornPlayers.Where(s => s.Id == id).FirstOrDefault();

            return GetPlayerWorker(id.ToString(), dbPlayer, forceUpdate);
        }

        public Entities.TornPlayer GetPlayer(string name, bool forceUpdate = false)
        {
            // Lets try and get a record from the database. If there is no record, we get given a null
            Database.Entities.TornPlayer? dbPlayer = _database.TornPlayers.Where(s => s.Name  == name).FirstOrDefault();

            return GetPlayerWorker(name, dbPlayer, forceUpdate);
        }

        private Entities.TornPlayer GetPlayerWorker(string idOrName, Database.Entities.TornPlayer? dbPlayer, bool forceUpdate = false)
        {
            Entities.TornPlayer tornPlayer;

            // Check what we got from the database
            if (dbPlayer == null)
            {
                // There was no record in the database. Fetch from Torn API and add to the database
                tornPlayer = _torn.GetPlayer(idOrName);
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
                tornPlayer = _torn.GetPlayer(idOrName);
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

        public Entities.Stats GetStats(string IdOrName, bool checkUpdate = false)
        {
            UInt32 idUInt;
            Database.Entities.Stats? dbStats;
            if (UInt32.TryParse(IdOrName, out idUInt))
            {
                dbStats = _database.Stats.Where(s => s.PlayerId == idUInt).FirstOrDefault();
            }
            else
            {
                Database.Entities.TornPlayer? dbPlayer = _database.TornPlayers.Where(s => s.Name == IdOrName).FirstOrDefault();
                if (dbPlayer != null)
                {
                    dbStats = _database.Stats.Where(s => s.PlayerId == dbPlayer.Id).FirstOrDefault();
                }
                else
                {
                    dbStats = null;
                }

            }
            //If
            // 1) we get a valid stats from the Database
            // 2) the stats are less than 2 weeks old
            // 3) we are not asked to check for an external update
            if (
                dbStats != null &&
                dbStats.Timestamp.CompareTo(DateTime.UtcNow.AddDays(-MaxStatsCacheAge)) < 0 &&
                !checkUpdate
            )
            {
                // All is good, we can return what we have;
                return dbStats.ToStats();
            }
            else
            {
                // We need to figure out what, if anything, we can send back
                //Lets try and get some stats from TornStats
                Entities.Stats tsStats = _tornStats.GetPlayerStats(IdOrName.ToString());

                //If we have nothing, end game
                if(dbStats == null && tsStats == null)
                {
                    //We have nothing, so hand back null
                    return null;
                }
                else if (tsStats == null)
                {
                    //We have something from the database and but nothing else
                    return dbStats.ToStats();
                }
                else
                {
                    // We might have something from the DB
                    // We do have something from TS

                    //If we dont have anything in the database OR
                    //TS stat is newer than what we have
                    if (dbStats == null || tsStats.StatsTimestamp.CompareTo(dbStats.Timestamp) > 0)
                    {
                        //TS is newer (or local one doesnt exist)
                        Database.Entities.Stats newDbStats = new Database.Entities.Stats(tsStats);
                        _database.Stats.Add(newDbStats);
                        _database.SaveChanges();

                        return tsStats;
                    }
                    else
                    {
                        // Return what was in the database. Its either newer or same age as TS
                        return dbStats.ToStats();
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

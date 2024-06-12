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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TornBot.Services.Database;
using TornBot.Exceptions;
using TornBot.Services.TornApi.Services;
using TornBot.Services.TornStatsApi.Services;

namespace TornBot.Services.Players.Service
{
    public class PlayersService : IHostedService
    {
        private const int MaxStatsCacheAge = 14;
        private const int MaxTornPlayerCacheAge = 7;

        private readonly IConfigurationRoot _config;
        private DatabaseContext _database;
        private TornApiService _torn;
        private TornStatsApiService _tornStats;
        private readonly ArmoryService _armoryService;
        
        public PlayersService(
            DatabaseContext database,
            TornApiService torn,
            TornStatsApiService tornStats,
            IConfigurationRoot config
        ) 
        {
            _database = database;
            _torn = torn;
            _tornStats = tornStats;
            _config = config;
        }
        
        /// <summary>
        /// Attempts to find a Torn Player by id
        /// Will check the DB first, if its stale, will call Torn API to update the DB
        /// </summary>
        /// <param name="name">Torn Player name</param>
        /// <param name="forceUpdate">Force an update from Torn API. always consider the DB record to be stale</param>
        /// <returns>Entities.TornPlayer</returns>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public Entities.TornPlayer GetPlayer(UInt32 id, bool forceUpdate = false)
        {
            Entities.TornPlayer tornPlayer;

            // Lets try and get a record from the database. If there is no record, we get given a null
            Database.Entities.TornPlayer? dbPlayer = _database.TornPlayers.FirstOrDefault(s => s.Id == id);

            // Check what we got from the database
            if (dbPlayer == null)
            {
                //There was no record in the database. Fetch from Torn API and add to the database
                try
                {
                    tornPlayer = _torn.GetPlayer(id);

                    _database.TornPlayers.Add(new Services.Database.Entities.TornPlayer(tornPlayer));
                    _database.SaveChanges();

                    return tornPlayer;
                }
                catch (Exception e)
                {
                    //Redundant but makes reading the code easier
                    //Pass any exceptions up the chain
                    throw;
                }
                
            }
            else if (dbPlayer.LastUpdated.CompareTo(DateTime.UtcNow.AddDays(-MaxTornPlayerCacheAge)) < 0 || forceUpdate)
            {
                // We have a record in the database, but it's stale so we need to update with a fresh pull
                // Or, we were told to get a fresh copy
                try
                {
                    tornPlayer = _torn.GetPlayer(id);
                    dbPlayer.ParseTornPlayer(tornPlayer);
                    _database.TornPlayers.Update(dbPlayer);
                    _database.SaveChanges();

                    return tornPlayer;
                }
                catch (Exception e)
                {
                    //Something went wrong in getting the API data. Just return the existing data instead
                    return dbPlayer.ToTornPlayer();
                }
            }
            else
            {
                // The record from the database was ok to use
                return dbPlayer.ToTornPlayer();
            }
        }

        /// <summary>
        /// Attempts to find a Torn Player by name
        /// Will check the DB first, if its stale, will call Torn API to update the DB
        /// If no record is found, will try calling TornStats GetBattleStats(name) and try to get a player id
        /// </summary>
        /// <param name="name">Torn Player name</param>
        /// <param name="forceUpdate">Force an update from Torn API. always consider the DB record to be stale</param>
        /// <returns>Entities.TornPlayer</returns>
        /// <exception cref="PlayerNotFoundException">Can not find the player by name</exception>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public Entities.TornPlayer GetPlayer(string name, bool forceUpdate = false)
        {
            TornBot.Entities.TornPlayer tornPlayer;
            
            // Lets try and get a record from the database. If there is no record, we get given a null
            Services.Database.Entities.TornPlayer? dbPlayer = _database.TornPlayers.Where(s => s.Name  == name).FirstOrDefault();

            if (dbPlayer != null)
            {
                //Check to see if the local cache is stale or forceUpdate is true
                if (dbPlayer.LastUpdated.CompareTo(DateTime.Now.AddDays(-MaxTornPlayerCacheAge)) < 0 || forceUpdate)
                {
                    try
                    {
                        //Lets pull a copy from Torn, based on the Id from the local cache
                        tornPlayer = _torn.GetPlayer(dbPlayer.Id);
                        if (tornPlayer.Name == name)
                        {
                            //The players has not changed their name. We now have a valid player Id
                            _database.TornPlayers.Update(new Database.Entities.TornPlayer(tornPlayer));
                            _database.SaveChanges();
                            return tornPlayer;
                        }
                    }
                    catch (Exception)
                    {
                        //If there was an API exception we can try a lookup on TornStats an and then try another Torn API call                      
                    }
                    
                    //The local cache name doesnt match the return from Torn (or Torn API had an issue)
                    //We can use TornStats spy lookup on a name
                    try
                    {                    
                        Entities.BattleStats tsBattleStats = GetBattleStats(name, true);
                        return GetPlayer(tsBattleStats.PlayerId, true);
                    }
                    catch (Exception e)
                    {
                        //We've failed to get any updated data but we still have our DB record. Use it
                        return dbPlayer.ToTornPlayer();
                    }
                }
                else
                {
                    //Record is not stale. Lets use it
                    return dbPlayer.ToTornPlayer();
                }
            }
            else
            {
                Entities.BattleStats battleStats;
                try
                {
                    battleStats = GetBattleStats(name, true);
                }
                catch (Exception e)
                {
                    throw new PlayerNotFoundException("Unable to player by name");
                }

                try
                {
                    UInt32 id = battleStats.PlayerId;

                    tornPlayer = _torn.GetPlayer(id);

                    _database.TornPlayers.Add(new Database.Entities.TornPlayer(tornPlayer));
                    _database.SaveChanges();

                    return tornPlayer;
                }
                catch (ApiCallFailureException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new ApiCallFailureException("Unable to get player", e);
                }
            }
        }

        /// <summary>
        /// Attempts to find a Torn Player by name
        /// Will check the DB first, if its stale, will call Torn API to update the DB
        /// If no record is found, will try calling TornStats GetBattleStats(name) and try to get a player id
        /// </summary>
        /// <param name="IdOrName">Torn Player id or name</param>
        /// <param name="checkUpdate">Force an update from TornStats. always consider the DB record to be stale</param>
        /// <returns>Entities.BattleStats</returns>
        /// <exception cref="BattleStatsNotAvailableException">No Battle Stats have been found. Inner exception can include API exceptions</exception>
        public Entities.BattleStats GetBattleStats(string IdOrName, bool checkUpdate = false)
        {
            UInt32 idUInt;
            Services.Database.Entities.BattleStats? dbBattleStats = null;
            
            //If we have checkUpdate we want to skip this so that we try to get something from TornStats
            if (!checkUpdate)
            {
                if (UInt32.TryParse(IdOrName, out idUInt))
                {
                    dbBattleStats = _database.BattleStats.Where(s => s.PlayerId == idUInt).FirstOrDefault();
                }
                else
                {
                    Services.Database.Entities.TornPlayer? dbPlayer = _database.TornPlayers.Where(s => s.Name == IdOrName).FirstOrDefault();
                    
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
                Entities.BattleStats tsBattleStats;
                try
                {
                    tsBattleStats = _tornStats.GetPlayerStats(IdOrName);
                }
                catch (Exception e)
                {
                    if (dbBattleStats == null)
                    {
                        //We have nothing. Throw an exception
                        throw new BattleStatsNotAvailableException("Error in getting spy from TornStats", e);
                    }
                    else
                    {
                        //We have something from the DB. That's all we have, so return it
                        return dbBattleStats.ToBattleStats();
                    }
                }

                
                // We might have something from the DB
                // We do have something from TS

                //If we dont have anything in the database OR
                //TS stat is newer than what we have
                if (dbBattleStats == null || tsBattleStats.BattleStatsTimestamp.CompareTo(dbBattleStats.Timestamp) > 0)
                {
                    //TS is newer (or local one doesnt exist)
                    Services.Database.Entities.BattleStats newDbBattleStats = new Services.Database.Entities.BattleStats(tsBattleStats);
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
        
        public void RecordPlayerLoadOut(UInt32 playerId, LoadOut loadOut)
        {
            Database.Entities.LoadOuts dbLoadOuts = _database.LoadOuts.FirstOrDefault(lo => lo.PlayerId == playerId);
            bool newLoadOut = false;
            if (dbLoadOuts == null)
            {
                newLoadOut = true;
                dbLoadOuts = new Database.Entities.LoadOuts();
                dbLoadOuts.PlayerId = playerId;
            }
            
            dbLoadOuts.Timestamp = DateTime.UtcNow;

            string mods;
            
            if (loadOut.PrimaryWeapon != null)
            {
                if (!_armoryService.CheckItemInDatabase(loadOut.PrimaryWeapon))
                {
                    _armoryService.AddItem(loadOut.PrimaryWeapon);
                }

                dbLoadOuts.PrimaryWeapon = loadOut.PrimaryWeapon.Uid;

                mods = "";
                foreach (var mod in loadOut.PrimaryWeapon.Mods)
                {
                    if (mods == "")
                        mods = mod.Id.ToString();
                    else
                        mods += String.Format(",{0}", mod.Id.ToString());
                }

                dbLoadOuts.PrimaryWeaponMods = mods;
            }
            
            if (loadOut.SecondaryWeapon != null)
            {
                if (!_armoryService.CheckItemInDatabase(loadOut.SecondaryWeapon))
                {
                    _armoryService.AddItem(loadOut.SecondaryWeapon);
                }

                dbLoadOuts.SecondaryWeapon = loadOut.SecondaryWeapon.Uid;

                mods = "";
                foreach (var mod in loadOut.SecondaryWeapon.Mods)
                {
                    if (mods == "")
                        mods = mod.Id.ToString();
                    else
                        mods += String.Format(",{0}", mod.Id.ToString());
                }

                dbLoadOuts.SecondaryWeaponMods = mods;
            }
            
            if (loadOut.MeleeWeapon != null)
            {
                if (!_armoryService.CheckItemInDatabase(loadOut.MeleeWeapon))
                {
                    _armoryService.AddItem(loadOut.MeleeWeapon);
                }

                dbLoadOuts.MeleeWeapon = loadOut.MeleeWeapon.Uid;
            }
            
            if (loadOut.TempWeapon != null)
            {
                if (!_armoryService.CheckItemInDatabase(loadOut.TempWeapon))
                {
                    _armoryService.AddItem(loadOut.TempWeapon);
                }

                dbLoadOuts.TemporaryWeapon = loadOut.TempWeapon.Uid;
            }
            
            // - Armor
            
            if (loadOut.HelmetArmor != null)
            {
                if (_armoryService.CheckItemInDatabase(loadOut.HelmetArmor))
                {
                    _armoryService.AddItem(loadOut.HelmetArmor);
                }
                
                dbLoadOuts.HelmetArmor = loadOut.HelmetArmor.Uid;
            }
            
            if (loadOut.ChestArmor != null)
            {
                if (_armoryService.CheckItemInDatabase(loadOut.ChestArmor))
                {
                    _armoryService.AddItem(loadOut.ChestArmor);
                }
                
                dbLoadOuts.ChestArmor = loadOut.ChestArmor.Uid;
            }
            
            if (loadOut.PantsArmor != null)
            {
                if (_armoryService.CheckItemInDatabase(loadOut.PantsArmor))
                {
                    _armoryService.AddItem(loadOut.PantsArmor);
                }
                
                dbLoadOuts.PantsArmor = loadOut.PantsArmor.Uid;
            }
            
            if (loadOut.GlovesArmor != null)
            {
                if (_armoryService.CheckItemInDatabase(loadOut.GlovesArmor))
                {
                    _armoryService.AddItem(loadOut.GlovesArmor);
                }
                
                dbLoadOuts.GlovesArmor = loadOut.GlovesArmor.Uid;
            }
            
            if (loadOut.BootsArmor != null)
            {
                if (_armoryService.CheckItemInDatabase(loadOut.BootsArmor))
                {
                    _armoryService.AddItem(loadOut.BootsArmor);
                }
                
                dbLoadOuts.BootsArmor = loadOut.BootsArmor.Uid;
            }

            if (newLoadOut)
                _database.LoadOuts.Add(dbLoadOuts);
            else
                _database.LoadOuts.Update(dbLoadOuts);
            
            _database.SaveChanges();
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

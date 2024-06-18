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
using TornBot.Entities;
using TornBot.Services.Database;
using TornBot.Exceptions;
using TornBot.Services.Armory.Service;
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
            IConfigurationRoot config,
            ArmoryService armoryService
        ) 
        {
            _database = database;
            _torn = torn;
            _tornStats = tornStats;
            _config = config;
            _armoryService = armoryService;
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
        
        /// <summary>
        /// Records a player's Load Out
        /// </summary>
        /// <param name="playerId">Torn Player id</param>
        /// <param name="loadOut">Deserialised and specific parts of the json load out</param>
        /// <returns>void</returns>
        public void RecordPlayerLoadOut(UInt32 playerId, LoadOut loadOut)
        {
            Database.Entities.LoadOuts? dbLoadOuts = _database.LoadOuts.FirstOrDefault(lo => lo.PlayerId == playerId);
            
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
                if (!_armoryService.CheckItemInDatabase(loadOut.PrimaryWeapon.ToArmoryItem()))
                    _armoryService.AddItem(loadOut.PrimaryWeapon.ToArmoryItem());

                dbLoadOuts.PrimaryWeapon = loadOut.PrimaryWeapon.Uid;

                mods = null;
                foreach (var mod in loadOut.PrimaryWeapon.Mods)
                {
                    if (!_armoryService.CheckWeaponModInDatabase(mod))
                        _armoryService.AddWeaponMod(mod);

                    if (mods == null)
                        mods = mod.Id.ToString();
                    else
                        mods = String.Format("{0},{1}", mods, mod.Id.ToString());
                }

                dbLoadOuts.PrimaryWeaponMods = mods;
            }
            else
            {
                dbLoadOuts.PrimaryWeapon = null;
                dbLoadOuts.PrimaryWeaponMods = null;
            }

            if (loadOut.SecondaryWeapon != null)
            {
                if (!_armoryService.CheckItemInDatabase(loadOut.SecondaryWeapon.ToArmoryItem()))
                    _armoryService.AddItem(loadOut.SecondaryWeapon.ToArmoryItem());

                dbLoadOuts.SecondaryWeapon = loadOut.SecondaryWeapon.Uid;

                mods = null;
                foreach (var mod in loadOut.SecondaryWeapon.Mods)
                {
                    if (!_armoryService.CheckWeaponModInDatabase(mod))
                        _armoryService.AddWeaponMod(mod);
                    
                    if (mods == null)
                        mods = mod.Id.ToString();
                    else
                        mods = String.Format("{0},{1}", mods, mod.Id.ToString());
                }

                dbLoadOuts.SecondaryWeaponMods = mods;
            }
            else
            {
                dbLoadOuts.SecondaryWeapon = null;
                dbLoadOuts.SecondaryWeaponMods = null;
            }

            if (loadOut.MeleeWeapon != null)
            {
                if (!_armoryService.CheckItemInDatabase(loadOut.MeleeWeapon.ToArmoryItem()))
                    _armoryService.AddItem(loadOut.MeleeWeapon.ToArmoryItem());

                dbLoadOuts.MeleeWeapon = loadOut.MeleeWeapon.Uid;
            }
            else
                dbLoadOuts.MeleeWeapon = null;

            if (loadOut.TempWeapon != null)
            {
                if (!_armoryService.CheckItemInDatabase(loadOut.TempWeapon.ToArmoryItem()))
                    _armoryService.AddItem(loadOut.TempWeapon.ToArmoryItem());

                // We dont store Temp weapons by UID, only ID as they are not unique
                dbLoadOuts.TemporaryWeapon = loadOut.TempWeapon.Id;
            }
            else
                dbLoadOuts.TemporaryWeapon = null;
            
            // - Armor
            
            if (loadOut.HelmetArmor != null)
            {
                if (!_armoryService.CheckItemInDatabase(loadOut.HelmetArmor.ToArmoryItem()))
                {
                    _armoryService.AddItem(loadOut.HelmetArmor.ToArmoryItem());
                }
                
                dbLoadOuts.HelmetArmor = loadOut.HelmetArmor.Uid;
            }
            else
                dbLoadOuts.HelmetArmor = null;

            if (loadOut.ChestArmor != null)
            {
                if (!_armoryService.CheckItemInDatabase(loadOut.ChestArmor.ToArmoryItem()))
                {
                    _armoryService.AddItem(loadOut.ChestArmor.ToArmoryItem());
                }
                
                dbLoadOuts.ChestArmor = loadOut.ChestArmor.Uid;
            }
            else
                dbLoadOuts.ChestArmor = null;

            if (loadOut.PantsArmor != null)
            {
                if (!_armoryService.CheckItemInDatabase(loadOut.PantsArmor.ToArmoryItem()))
                {
                    _armoryService.AddItem(loadOut.PantsArmor.ToArmoryItem());
                }
                
                dbLoadOuts.PantsArmor = loadOut.PantsArmor.Uid;
            }
            else
                dbLoadOuts.PantsArmor = null;

            if (loadOut.GlovesArmor != null)
            {
                if (!_armoryService.CheckItemInDatabase(loadOut.GlovesArmor.ToArmoryItem()))
                {
                    _armoryService.AddItem(loadOut.GlovesArmor.ToArmoryItem());
                }
                
                dbLoadOuts.GlovesArmor = loadOut.GlovesArmor.Uid;
            }
            else
                dbLoadOuts.GlovesArmor = null;

            if (loadOut.BootsArmor != null)
            {
                if (!_armoryService.CheckItemInDatabase(loadOut.BootsArmor.ToArmoryItem()))
                {
                    _armoryService.AddItem(loadOut.BootsArmor.ToArmoryItem());
                }
                
                dbLoadOuts.BootsArmor = loadOut.BootsArmor.Uid;
            }
            else
                dbLoadOuts.BootsArmor = null;

            if (newLoadOut)
                _database.LoadOuts.Add(dbLoadOuts);
            else
                _database.LoadOuts.Update(dbLoadOuts);
            
            _database.SaveChanges();
        }

        /// <summary>
        /// Get a player's Load Out
        /// </summary>
        /// <param name="playerId">Torn Player id</param>
        /// <returns>TornBot.Entities.LoadOut</returns>
        public TornBot.Entities.LoadOut GetPlayerLoadOut(UInt32 playerId)
        {
            TornBot.Entities.LoadOut loadOut = new LoadOut();
            Database.Entities.LoadOuts? dbLoadOut = _database.LoadOuts.FirstOrDefault(lo => lo.PlayerId == playerId);

            if (dbLoadOut == null)
            {
                return loadOut;
            }

            loadOut.Timestamp = dbLoadOut.Timestamp;

            if (dbLoadOut.PrimaryWeapon != null)
            {
                loadOut.PrimaryWeapon = _armoryService.GetItem(dbLoadOut.PrimaryWeapon.Value).ToArmoryItemPrimaryWeapon();
                if (dbLoadOut.PrimaryWeaponMods != null)
                {
                    foreach (var id in dbLoadOut.PrimaryWeaponMods.Split(","))
                    {
                        loadOut.PrimaryWeapon.Mods.Add(_armoryService.GetWeaponMod(UInt16.Parse(id)));
                    }
                }
            }

            if (dbLoadOut.SecondaryWeapon != null)
            {
                loadOut.SecondaryWeapon = _armoryService.GetItem(dbLoadOut.SecondaryWeapon.Value).ToArmoryItemSecondaryWeapon();
                if (dbLoadOut.SecondaryWeaponMods != null)
                {
                    foreach (var id in dbLoadOut.SecondaryWeaponMods.Split(","))
                    {
                        loadOut.SecondaryWeapon.Mods.Add(_armoryService.GetWeaponMod(UInt16.Parse(id)));
                    }
                }
            }

            if (dbLoadOut.MeleeWeapon != null)
                loadOut.MeleeWeapon = _armoryService.GetItem(dbLoadOut.MeleeWeapon.Value).ToArmoryItemMeleeWeapon();

            if (dbLoadOut.TemporaryWeapon != null)
                loadOut.TempWeapon = _armoryService.GetItem(dbLoadOut.TemporaryWeapon.Value, true).ToArmoryItemTemporaryWeapon();

            if (dbLoadOut.HelmetArmor != null)
                loadOut.HelmetArmor = _armoryService.GetItem(dbLoadOut.HelmetArmor.Value).ToArmoryItemDefensive();
            
            if (dbLoadOut.ChestArmor != null)
                loadOut.ChestArmor = _armoryService.GetItem(dbLoadOut.ChestArmor.Value).ToArmoryItemDefensive();
            
            if (dbLoadOut.PantsArmor != null)
                loadOut.PantsArmor = _armoryService.GetItem(dbLoadOut.PantsArmor.Value).ToArmoryItemDefensive();
            
            if (dbLoadOut.GlovesArmor != null)
                loadOut.GlovesArmor = _armoryService.GetItem(dbLoadOut.GlovesArmor.Value).ToArmoryItemDefensive();
            
            if (dbLoadOut.BootsArmor != null)
                loadOut.BootsArmor = _armoryService.GetItem(dbLoadOut.BootsArmor.Value).ToArmoryItemDefensive();

            return loadOut;
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

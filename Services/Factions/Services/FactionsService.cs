// TornBot
// 
// Copyright (C) 2024 TornBot.com
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
// 
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TornBot.Entities;
using TornBot.Exceptions;
using TornBot.Services.Factions.Database.Dao;
using TornBot.Services.TornApi.Services;

namespace TornBot.Services.Factions.Services;

public class FactionsService : IHostedService
{
    private readonly IConfigurationRoot _config;
    private readonly IFactionDao _factionDao;
    private readonly TornApiService _tornApiService;

    public FactionsService(
        IConfigurationRoot config,
        IFactionDao factionDao,
        TornApiService tornApiService
    )
    {
        _config = config;
        _factionDao = factionDao;
        _tornApiService = tornApiService;
    }

    /// <summary>
    /// Gets the faction name from the database by faction id
    /// </summary>
    /// <param name="id">Faction id</param>
    /// <returns>string</returns>
    public string GetFactionNameById(UInt32 id)
    {
        // TODO If there is no result in the DB, get it from Torn API, save it and then return the name
        return _factionDao.GetFactionNameById(id);
    }

    /// <summary>
    /// Gets a TornFaction object from the database by faction id
    /// </summary>
    /// <param name="id">Faction id</param>
    /// <returns>Entities.TornFaction</returns>
    public Entities.TornFaction GetFaction(UInt32 id)
    {
        Database.Entities.TornFactions? faction = _factionDao.GetFactionById(id);

        if (faction == null)
            return new TornFaction();
        else
            return faction.ToTornFaction();
    }

    public void UpdateFaction(TornFaction faction)
    {
        _factionDao.AddOrUpdateTornFaction(faction);
    }
    
    /// <summary>
    /// Scans a faction, checking if any members are revivable
    /// Will check the DB first, if its stale, will call Torn API to update the DB
    /// If no record is found, will try calling TornStats GetBattleStats(name) and try to get a player id
    /// </summary>
    /// <param name="factionId">Faction id to scan</param>
    /// <param name="usedInsiderKey">Output - true is an insider key was used against the home faction. Expect false positives</param>
    /// <returns>List<Entities.TornPlayer></returns>
    /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
    public List<Entities.TornPlayer> GetReviveStatus(
        UInt32 factionId,
        out bool usedInsiderKey 
    )
    {
        InteractionContext? ctx = null;
        return GetReviveStatus(factionId, out usedInsiderKey, ref ctx);
    }
    
    /// <summary>
    /// Scans a faction, checking if any members are revivable
    /// Will check the DB first, if its stale, will call Torn API to update the DB
    /// If no record is found, will try calling TornStats GetBattleStats(name) and try to get a player id
    /// </summary>
    /// <param name="factionID">Faction id to scan</param>
    /// <param name="usedInsiderKey">Output - true is an insider key was used against the home faction. Expect false positives</param>
    /// <param name="ctx">InteractionContext used to update a slash command's response text</param>
    /// <returns>List<Entities.TornPlayer></returns>
    /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
    public List<Entities.TornPlayer> GetReviveStatus (
        UInt32 factionID,
        out bool usedInsiderKey,
        ref InteractionContext? ctx
    )
    {
        usedInsiderKey = false;
        
        if (ctx is not null)
            ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Looking for revivable players"));
        
        Entities.TornPlayer tornPlayer;
        List<Entities.TornPlayer> tornPlayerInitialList = new List<Entities.TornPlayer>();
        List<Entities.TornPlayer> tornPlayerExtRevivableList = new List<Entities.TornPlayer>();

        //TODO = Move this to the DB
        UInt32 homeFactionId = GetHomeFactionId();

        TornBot.Entities.TornFaction faction = _tornApiService.GetFaction(factionID);
        _factionDao.AddOrUpdateTornFaction(faction);

        string[] pinWheel = { "|", "/", "-", "\\" };
        byte pinWheelPos = 0;
        const int totalSleepLength = 5000;
        const int sleepSteps = 10;
        int sleepCounter;

        byte membersChecked = 0;

        foreach (var member in faction.Members)
        {
            if (member.Status != TornPlayer.PlayerStatus.Fallen)
            {
                UInt32 memberID = member.Id;
                while (true)
                {
                    try
                    {
                        tornPlayer = _tornApiService.GetPlayer(memberID);
                        pinWheelPos = 0;
                        break;
                    }
                    catch (ApiCallFailureException e)
                    {
                        if (e.InnerException is AllKeysRateLimitedException)
                        {
                            for (sleepCounter = 0; sleepCounter < sleepSteps; sleepCounter++)
                            {
                                if (ctx is not null)
                                {
                                    // TODO Move away from .Wait()
                                    ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                                        String.Format(
                                            "Checked {0} of {1} faction members. Waiting for API keys to become usable (rate limiting) {2}",
                                            membersChecked,
                                            faction.Members.Count,
                                            pinWheel[pinWheelPos]
                                        )))
                                    .Wait();
                                }

                                if (++pinWheelPos > pinWheel.Length - 1)
                                    pinWheelPos = 0;

                                System.Threading.Thread.Sleep(totalSleepLength / sleepSteps);
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                if (tornPlayer.Revivable == 1)
                {
                    tornPlayerInitialList.Add(tornPlayer);
                }
            }

            membersChecked++;
            if (ctx is not null)
                ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    String.Format("Checked {0} of {1} faction members ", membersChecked, faction.Members.Count)
                    )).Wait();
        }

        // TODO If the faction we are checking is not a home faction, then we can skip the secondary check
        membersChecked = 0;
        foreach (Entities.TornPlayer tornPlayerInitial in tornPlayerInitialList)
        {
            while (true)
            {
                try
                {
                    tornPlayer = _tornApiService.GetPlayer(tornPlayerInitial.Id, true);
                    pinWheelPos = 0;
                    break;
                }
                catch (ApiCallFailureException e)
                {
                    if (e.InnerException is AllKeysRateLimitedException)
                    {
                        for (sleepCounter = 0; sleepCounter < sleepSteps; sleepCounter++)
                        {
                            if (ctx is not null)
                            {
                                ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                                    String.Format(
                                        "Rechecked {0} of {1} revivable members. Waiting for API keys to become usable (rate limiting) {2}",
                                        membersChecked,
                                        tornPlayerInitialList.Count,
                                        pinWheel[pinWheelPos]
                                    )))
                                    .Wait();
                            }

                            if (++pinWheelPos > pinWheel.Length - 1)
                                pinWheelPos = 0;

                            System.Threading.Thread.Sleep(totalSleepLength / sleepSteps);
                        }
                    }
                    else if (e.InnerException is NoMoreKeysAvailableException)
                    {
                        //If we do not have any outsider keys to use, just return what we have
                        usedInsiderKey = true;
                        return tornPlayerInitialList;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            
            if (tornPlayer.Revivable == 1)
            {
                tornPlayerExtRevivableList.Add(tornPlayer);
            }

            membersChecked++;
            
            if (ctx is not null)
                ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(String.Format("Rechecked {0} of {1} revivable members ", membersChecked, tornPlayerInitialList.Count))).Wait();

        }

        return tornPlayerExtRevivableList; 
    }

    public List<TornFaction> GetFactionsForMonitoring()
    {
        List<Database.Entities.TornFactions> dbFactions = _factionDao.GetFactionsForMonitoring();

        List<TornFaction> tornFactions = new List<TornFaction>();
        foreach (var dbFaction in dbFactions)
        {
            tornFactions.Add(dbFaction.ToTornFaction());
        }

        return tornFactions;
    }
    
    public UInt32 GetHomeFactionId()
    {
        //TODO = Move this to the DB
        return _config.GetValue<UInt32>("TornFactionId"); //get faction id
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
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

using Microsoft.EntityFrameworkCore;
using TornBot.Services.Players.Database.Entities;

namespace TornBot.Services.Players.Database.Dao;

public class TornPlayerDao : ITornPlayerDao
{
    private readonly TornPlayerDbContext _context;
    private readonly DbSet<TornPlayer> _dbSet;

    public TornPlayerDao(TornPlayerDbContext context)
    {
        _context = context;
        _dbSet = _context.TornPlayers;
    }

    public TornBot.Entities.TornPlayer GetPlayer(UInt32 playerId)
    {
        TornPlayer? dbPlayer = _dbSet.FirstOrDefault(tp => tp.Id == playerId);
        
        if (dbPlayer == null)
            return new TornBot.Entities.TornPlayer();
        else
            return dbPlayer.ToTornPlayer();
    }

    public void SavePlayer(TornBot.Entities.TornPlayer tornPlayer)
    {
        TornPlayer? dbTornPlayer = _dbSet.FirstOrDefault(tp => tp.Id == tornPlayer.Id);

        if (dbTornPlayer == null)
        {
            _dbSet.Add(new TornPlayer(tornPlayer));
            _context.SaveChanges();
        }
        else
        {
            dbTornPlayer.ParseTornPlayer(tornPlayer);
            _dbSet.Update(dbTornPlayer);
            _context.SaveChanges();
        }
    }

    public void SavePlayers(List<Database.Entities.TornPlayer> tornPlayers)
    {
        foreach (var tornPlayer in tornPlayers)
        {
            TornPlayer? dbTornPlayer = _dbSet.FirstOrDefault(tp => tp.Id == tornPlayer.Id);

            if (dbTornPlayer == null)
            {
                _dbSet.Add(tornPlayer);
            }
            else if (!dbTornPlayer.IsEqual(tornPlayer))
            {
                dbTornPlayer.ParseTornPlayer(tornPlayer);
                _dbSet.Update(dbTornPlayer);
            }
        }

        _context.SaveChanges();
    }
    
    public List<TornBot.Entities.TornPlayer> GetMembersInFaction(UInt32 factionId)
    {
        List<TornPlayer> dbPlayers = _dbSet.Where(tp => tp.FactionId == factionId).ToList();

        List<TornBot.Entities.TornPlayer> players = new List<TornBot.Entities.TornPlayer>();
        foreach (var dbPlayer in dbPlayers)
        {
            players.Add(dbPlayer.ToTornPlayer());
        }

        return players;
    }

}
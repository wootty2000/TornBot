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

using Microsoft.EntityFrameworkCore;
using TornBot.Services.Factions.Database.Entities;

namespace TornBot.Services.Factions.Database.Dao;

public class FactionDao : IFactionDao
{
    private readonly FactionsDbContext _context;
    private readonly DbSet<TornFactions> _dbSet;
    
    public FactionDao(FactionsDbContext context)
    {
        _context = context;
        _dbSet = _context.TornFactions;
    }

    public TornFactions? GetFactionById(UInt32 id)
    {
        return _dbSet.FirstOrDefault(f => f.Id == id);
    }

    public string GetFactionNameById(UInt32 id)
    {
        TornFactions? faction = GetFactionById(id);
        
        if (faction == null)
            return "";
        else
            return faction.Name;
    }
    
    public List<TornFactions> GetFactionsForMonitoring()
    {
        List<TornFactions> dbFactions = _dbSet.Where(f => f.Monitor == true).ToList();

        return dbFactions;
    }

    public void AddOrUpdateTornFaction(TornBot.Entities.TornFaction faction)
    {
        TornFactions? dbFaction = _dbSet.FirstOrDefault(tf => tf.Id == faction.Id);

        if (dbFaction == null)
        {
            _dbSet.Add(new TornFactions(faction));
        }
        else
        {
            dbFaction.Name = faction.Name;
            dbFaction.Tag = faction.Tag;
            dbFaction.TagImage = faction.Tag_image;

            _dbSet.Update(dbFaction);
        }

        _context.SaveChanges();
    }
}

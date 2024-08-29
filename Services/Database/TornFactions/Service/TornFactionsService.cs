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

using Microsoft.Extensions.DependencyInjection;
using TornBot.Services.Database.TornFactions.Dao;

namespace TornBot.Services.Database.TornFactions.Service;

public class TornFactionsService
{
    private readonly IServiceProvider _serviceProvider;

    public TornFactionsService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public Entities.TornFactions? GetFactionById(UInt32 factionId)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<ITornFactionsDao>();

            return dao.GetFactionById(factionId);
        }
    }

    public string GetFactionNameById(UInt32 factionId)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<ITornFactionsDao>();

            return dao.GetFactionNameById(factionId);
        }
    }

    public List<Entities.TornFactions> GetFactionsForMonitoring()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<ITornFactionsDao>();

            return dao.GetFactionsForMonitoring();
        }
    }

    public void SaveFaction(TornBot.Entities.TornFaction faction)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<ITornFactionsDao>();

            dao.SaveFaction(faction);
        }

    }

}
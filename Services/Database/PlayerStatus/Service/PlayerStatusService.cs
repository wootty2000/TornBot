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
using TornBot.Entities;
using TornBot.Services.Database.PlayerStatus.Dao;

namespace TornBot.Services.Database.PlayerStatus.Service;

public class PlayerStatusService
{
    private readonly IServiceProvider _serviceProvider;

    public PlayerStatusService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void RecordPlayerStatus(UInt32 playerId, byte status, byte onlineStatus, DateTime now)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<IPlayerStatusDao>();

            dao.RecordPlayerStatus(playerId, status, onlineStatus, now);
        }
    }
    
    public void RecordPlayerStatuses(List<(UInt32 playerId, byte status, byte onlineStatus)> playerStatuses, DateTime now)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<IPlayerStatusDao>();

            dao.RecordPlayerStatuses(playerStatuses, now);
        }
    }

    public List<DateTime> GetPlayerStatusDatesForPlayer(UInt32 playerId)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<IPlayerStatusDao>();

            return dao.GetPlayerStatusDatesForPlayer(playerId);
        }
    }
    
    public TornBot.Entities.PlayerStatusData GetPlayerStatusData(UInt32 playerId, DateTime startDate)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<IPlayerStatusDao>();

            var data = dao.GetPlayerStatusData(playerId, startDate);

            if (data is null)
            {
                return new PlayerStatusData();
            }
            else
            {
                return data.ToPlayerStatusData();
            }
        }
    }
}
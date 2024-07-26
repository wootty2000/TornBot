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

using TornBot.Services.Players.Database.Entities;

namespace TornBot.Services.Players.Database.Dao;

public interface IPlayerStatusDao
{
    public void BeginTransaction();
    public void CommitTransaction();
    public void RollbackTransaction();

    public void RecordPlayerStatus(UInt32 playerId, byte status, byte onlineStatus, DateTime now);
    public void RecordPlayerStatuses(List<(UInt32 playerId, byte status, byte onlineStatus)> playerStatuses, DateTime now);
    public List<DateTime> GetPlayerStatusDatesForPlayer(UInt32 playerId);
    public PlayerStatus? GetPlayerStatusData(UInt32 playerId, DateTime weekStarting);
}
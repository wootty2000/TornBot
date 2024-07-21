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
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using TornBot.Services.Players.Database.Entities;


namespace TornBot.Services.Players.Database.Dao;

public class PlayerStatusDao : IPlayerStatusDao
{
    private readonly PlayerStatusDbContext _context;
    private readonly DbSet<PlayerStatus> _dbSet;

    public PlayerStatusDao(PlayerStatusDbContext context)
    {
        _context = context;
        _dbSet = _context.PlayerStatus;
    }

    //
    public void RecordPlayerStatus(UInt32 playerId, byte status, byte onlineStatus, DateTime now)
    {
        // DayOfWeek 0 is Sunday. We want to start on Monday
        var weekStarting = now.Date.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));

        var day = now.ToString("dddd").ToLower();
        var time = now.ToString("HH:mm");

        var weekStartingStr = weekStarting.ToString("yyyy-MM-dd");

        var path = $"$.{day}.{time}";
        
        var statusJson = new JArray(status, onlineStatus).ToString(Newtonsoft.Json.Formatting.None);

        var mergePatchJson = $"{{\"{day}\": {{\"{time}\": {statusJson}}}}}";

        var sql = @"
            INSERT INTO PlayerStatus (PlayerId, WeekStarting, StatusLog)
            VALUES (@playerId, @weekStarting, @mergePatchJson)
            ON DUPLICATE KEY UPDATE
            StatusLog = JSON_MERGE_PATCH(StatusLog, @mergePatchJson)";
            
        MySqlParameter[] queryParams = new MySqlParameter[]
        {
            new MySqlParameter("@playerId", playerId),
            new MySqlParameter("@weekStarting", weekStartingStr),
            new MySqlParameter("@mergePatchJson", MySqlDbType.JSON) { Value = mergePatchJson }
        };

        _context.Database.ExecuteSqlRaw(sql, queryParams);
    }
    
    public List<DateTime> GetPlayerStatusDatesForPlayer(UInt32 playerId)
    {
        // Have to use AsNoTracking because EF is not tracking the insert/update from above
        List<DateTime> dates = _dbSet.AsNoTracking().Where(ps => ps.PlayerId == playerId).Select(ps => ps.WeekStarting).ToList();

        return dates;
    }
    
    public PlayerStatus? GetPlayerStatusData(UInt32 playerId, DateTime weekStarting)
    {
        // Have to use AsNoTracking because EF is not tracking the insert/update from above
        return _dbSet.AsNoTracking().FirstOrDefault(ps => ps.PlayerId == playerId && ps.WeekStarting == weekStarting);
    }
}

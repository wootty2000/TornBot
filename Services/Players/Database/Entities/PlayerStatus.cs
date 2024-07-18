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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TornBot.Entities;

namespace TornBot.Services.Players.Database.Entities;

public class PlayerStatus
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public UInt32 Id { get; set; } = 0;
    
    [Required]
    public UInt32 PlayerId { get; set; } = 0;
    
    [Required]
    public DateTime WeekStarting { get; set; } = DateTime.UnixEpoch;

    [Required]
    public string StatusLog { get; set; } = "";

    // Helper method to deserialize StatusLog into a usable structure
    public Dictionary<string, Dictionary<string, PlayerStatusLogEntry>>? GetStatusLogEntries()
    {
        return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, PlayerStatusLogEntry>>>(StatusLog);
    }

    public TornBot.Entities.PlayerStatusData ToPlayerStatusData()
    {
        PlayerStatusData playerStatusData = new PlayerStatusData
        {
            Id = PlayerId,
            WeekStarting = WeekStarting
        };

        var data = GetStatusLogEntries();

        foreach (var day in data)
        {
            var entries = new Dictionary<string, PlayerStatusDataLogEntry>();

            foreach (var time in day.Value)
            {
                entries.Add(
                    time.Key,
                    new PlayerStatusDataLogEntry((byte)time.Value.PlayerStatus, (byte)time.Value.OnlineStatus));
            }
            
            playerStatusData.StatusData.Add(day.Key, entries);
        }

        return playerStatusData;
    }
}
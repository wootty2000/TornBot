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

namespace TornBot.Entities;

public class PlayerStatusData
{
    public UInt32 Id { get; set; } = 0;
    public DateTime WeekStarting { get; set; } = DateTime.UnixEpoch;
    
    public Dictionary<string, Dictionary<string, PlayerStatusDataLogEntry>> StatusData { get; set; } =
        new Dictionary<string, Dictionary<string, PlayerStatusDataLogEntry>>();
}

public class PlayerStatusDataLogEntry
{
    public byte PlayerStatus { get; set; } = 0;
    public byte OnlineStatus { get; set; } = 0;

    public PlayerStatusDataLogEntry()
    {
    }

    public PlayerStatusDataLogEntry(byte playerStatus, byte onlineStatus)
    {
        PlayerStatus = playerStatus;
        OnlineStatus = onlineStatus;
    }
}
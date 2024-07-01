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

using TornBot.Services.Players.Service;

namespace TornBot.Entities
{
    public class TornPlayer
    {
        public UInt32 Id { get; set; } = 0;

        public string Name { get; set; } = "";

        public UInt32 FactionId { get; set; } = 0;

        public UInt16 Level { get; set; } = 0;

        public UInt16 Revivable { get; set; } = 0;

        public PlayerStatus Status { get; set; } = PlayerStatus.Unknown;
        public PlayerOnlineStatus OnlineStatus { get; set; } = PlayerOnlineStatus.Unknown;
        public DateTime LastAction { get; set; } = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public long LastActionUnixTime => new DateTimeOffset(LastAction).ToUnixTimeSeconds();

        public enum PlayerStatus
        {
            Unknown = 0,
            Okay = 1,
            Traveling = 2,
            Abroad = 3,
            Hospital = 4,
            Jail = 5,
            Federal = 6,
            Fallen = 7
        };

        public enum PlayerOnlineStatus
        {
            Unknown = 0,
            Offline = 1,
            Idle = 2,
            Online = 3
        };

        
        public TornPlayer()
        { }

        public TornPlayer(UInt32 id, string name, UInt16 lvl, UInt32 factionId)
        {
            this.Id = id;
            this.Name = name;
            this.Level = lvl;
            this.FactionId = factionId;
        }

    }
}

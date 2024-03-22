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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornBot.Database.Entities
{
    public class TornPlayer
    {
        public UInt32 Id { get; set; }
        public string Name { get; set; }
        public UInt16 Level { get; set; }
        public DateTime LastUpdated { get; set; }

        public TornPlayer() { }

        // Used for a new insert
        public TornPlayer(TornBot.Entities.TornPlayer tornPlayer)
        {
            Id = tornPlayer.Id;
            Name = tornPlayer.Name;
            Level = tornPlayer.Level;
            LastUpdated = DateTime.UtcNow;
        }

        // Used for updates.
        // For TornPlayers, we dont actually need this constructor as the DB id (primary key) is actually the tornPlayer.id but its how we do it for the other DB entites
        public TornPlayer(UInt32 id, TornBot.Entities.TornPlayer tornPlayer)
        {
            Id = id;
            Name = tornPlayer.Name;
            Level = tornPlayer.Level;
            LastUpdated = DateTime.UtcNow;
        }

        public TornBot.Entities.TornPlayer ToTornPlayer()
        {
            TornBot.Entities.TornPlayer tornPlayer = new TornBot.Entities.TornPlayer();
            tornPlayer.Id = Id;
            tornPlayer.Name = Name;
            tornPlayer.Level = Level;

            return tornPlayer;
        }
        
    }
}

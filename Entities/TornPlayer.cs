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

namespace TornBot.Entities
{
    public class TornPlayer
    {
        private UInt32 id;
        private string name;
        private TornFaction faction;

        public TornPlayer()
        { }

        public TornPlayer(UInt32 id, string name, TornFaction tornFaction)
        {
            this.id = id;
            this.name = name;
            this.faction = tornFaction;
        }

        public UInt32 Id
        { get { return id; } set { id = value; } }

        public string Name
        { get { return name; } set {  name = value; } }

        public TornFaction Faction
        { get { return faction; } set {  faction = value; } }
    }
}

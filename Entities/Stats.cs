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

namespace TornBot.Entities
{
    public class Stats
    {
        private UInt32 playerId = 0;

        private UInt64 strength = 0;
        private DateTime strengthTimestamp = DateTime.FromFileTimeUtc(0);

        private UInt64 defense = 0;
        private DateTime defenseTimestamp = DateTime.FromFileTimeUtc(0);

        private UInt64 speed = 0;
        private DateTime speedTimestamp = DateTime.FromFileTimeUtc(0);

        private UInt64 dexterity = 0;
        private DateTime dexterityTimestamp = DateTime.FromFileTimeUtc(0);

        private UInt64 total = 0;
        private DateTime totalTimestamp = DateTime.FromFileTimeUtc(0);

        private DateTime statsTimestamp = DateTime.FromFileTimeUtc(0);


        public UInt32 PlayerId { get { return playerId; } set { playerId = value; } }

        public UInt64 Strength { get { return strength; } set { strength = value; } }
        public DateTime StrengthTimestamp { get { return strengthTimestamp; } set { strengthTimestamp = value; } }

        public UInt64 Defense { get { return defense; } set { defense = value; } }
        public DateTime DefenseTimestamp { get { return defenseTimestamp; } set { defenseTimestamp = value; } }

        public UInt64 Speed { get { return speed; } set { speed = value; } }
        public DateTime SpeedTimestamp { get { return speedTimestamp; } set { speedTimestamp = value; } }

        public UInt64 Dexterity { get { return dexterity; } set { dexterity = value; } }
        public DateTime DexterityTimestamp { get { return dexterityTimestamp; } set { dexterityTimestamp = value; } }

        public UInt64 Total { get { return total; } set {  total = value; } }
        public DateTime TotalTimestamp { get { return totalTimestamp; } set { totalTimestamp = value; } }

        public DateTime StatsTimestamp { get { return statsTimestamp; } set {  statsTimestamp = value; } }
    }
}

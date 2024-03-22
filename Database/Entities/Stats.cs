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
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace TornBot.Database.Entities
{
    public class Stats
    {
        public UInt32 Id { get; set; }
        public UInt32 PlayerId { get; set; }

        public UInt64 Strength { get; set; }
        public DateTime StrengthTimestamp { get; set; }

        public UInt64 Defense { get; set; }
        public DateTime DefenseTimestamp { get; set; }

        public UInt64 Speed { get; set; }
        public DateTime SpeedTimestamp { get; set; }

        public UInt64 Dexterity { get; set; }
        public DateTime DexterityTimestamp { get; set; }
        public UInt64 Total { get; set; }
        public DateTime TotalTimestamp { get; set; }

        public DateTime Timestamp { get; set; }

        public Stats() { }

        public Stats(TornBot.Entities.Stats stats)
        {
            PlayerId = stats.PlayerId;
            Strength = stats.Strength;
            StrengthTimestamp = stats.StrengthTimestamp;
            Defense = stats.Defense;
            DefenseTimestamp = stats.DefenseTimestamp;
            Speed = stats.Speed;
            SpeedTimestamp = stats.SpeedTimestamp;
            Dexterity = stats.Dexterity;
            DexterityTimestamp = stats.DexterityTimestamp;
            Total = stats.Total;
            TotalTimestamp = stats.TotalTimestamp;
            Timestamp = stats.StatsTimestamp;
        }

        public TornBot.Entities.Stats ToStats()
        {
            TornBot.Entities.Stats stats = new TornBot.Entities.Stats();
            stats.PlayerId = PlayerId;
            stats.Strength = Strength;
            stats.StrengthTimestamp = StrengthTimestamp;
            stats.Defense = Defense;
            stats.DefenseTimestamp = DefenseTimestamp;
            stats.Speed = Speed;
            stats.SpeedTimestamp = SpeedTimestamp;
            stats.Dexterity = Dexterity;
            stats.DexterityTimestamp = DexterityTimestamp;
            stats.Total = Total;
            stats.TotalTimestamp = TotalTimestamp;

            return stats;
        }

    }
}

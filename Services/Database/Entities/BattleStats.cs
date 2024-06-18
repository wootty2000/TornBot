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

namespace TornBot.Services.Database.Entities
{
    public class BattleStats
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

        public BattleStats() { }

        public BattleStats(TornBot.Entities.BattleStats battleStats)
        {
            PlayerId = battleStats.PlayerId;
            Strength = battleStats.Strength;
            StrengthTimestamp = battleStats.StrengthTimestamp;
            Defense = battleStats.Defense;
            DefenseTimestamp = battleStats.DefenseTimestamp;
            Speed = battleStats.Speed;
            SpeedTimestamp = battleStats.SpeedTimestamp;
            Dexterity = battleStats.Dexterity;
            DexterityTimestamp = battleStats.DexterityTimestamp;
            Total = battleStats.Total;
            TotalTimestamp = battleStats.TotalTimestamp;
            Timestamp = battleStats.BattleStatsTimestamp;
        }

        public TornBot.Entities.BattleStats ToBattleStats()
        {
            TornBot.Entities.BattleStats battleStats = new TornBot.Entities.BattleStats();
            battleStats.PlayerId = PlayerId;
            battleStats.Strength = Strength;
            battleStats.StrengthTimestamp = StrengthTimestamp;
            battleStats.Defense = Defense;
            battleStats.DefenseTimestamp = DefenseTimestamp;
            battleStats.Speed = Speed;
            battleStats.SpeedTimestamp = SpeedTimestamp;
            battleStats.Dexterity = Dexterity;
            battleStats.DexterityTimestamp = DexterityTimestamp;
            battleStats.Total = Total;
            battleStats.TotalTimestamp = TotalTimestamp;
            battleStats.BattleStatsTimestamp = Timestamp;

            return battleStats;
        }

    }
}

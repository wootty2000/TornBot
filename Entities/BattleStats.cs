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
    public class BattleStats
    {
        public UInt32 PlayerId { get; set; } = 0;

        public UInt64 Strength { get ; set; } = 0;
        public DateTime StrengthTimestamp { get ; set; } = DateTime.FromFileTimeUtc(0);

        public UInt64 Defense { get ; set; } = 0;
        public DateTime DefenseTimestamp { get ; set; } = DateTime.FromFileTimeUtc(0);

        public UInt64 Speed { get ; set; } = 0;
        public DateTime SpeedTimestamp { get ; set; } = DateTime.FromFileTimeUtc(0);

        public UInt64 Dexterity { get ; set; } = 0;
        public DateTime DexterityTimestamp { get ; set; } = DateTime.FromFileTimeUtc(0);

        public UInt64 Total { get ; set; } = 0;
        public DateTime TotalTimestamp { get ; set; } = DateTime.FromFileTimeUtc(0);

        public DateTime BattleStatsTimestamp { get ; set; } = DateTime.FromFileTimeUtc(0);
    }
}

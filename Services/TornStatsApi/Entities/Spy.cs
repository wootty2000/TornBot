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
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TornBot.Services.TornStatsApi.Entities
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);

    public class Spy
    {
        [JsonPropertyName("spy")]
        public SpySpy spy { get; set; }

        public TornBot.Entities.Stats ToStats()
        {
            TornBot.Entities.Stats stats = new TornBot.Entities.Stats();

            stats.PlayerId = UInt32.Parse(this.spy.PlayerId.ToString());

            stats.Strength = this.spy.Strength;
            stats.StrengthTimestamp = DateTime.UnixEpoch.AddSeconds(this.spy.StrengthTimestamp);

            stats.Defense = this.spy.Defense;
            stats.DefenseTimestamp = DateTime.UnixEpoch.AddSeconds(this.spy.DefenseTimestamp);

            stats.Speed = this.spy.Speed;
            stats.SpeedTimestamp = DateTime.UnixEpoch.AddSeconds(this.spy.SpeedTimestamp);

            stats.Dexterity = this.spy.Dexterity;
            stats.DexterityTimestamp = DateTime.UnixEpoch.AddSeconds(this.spy.DexterityTimestamp);

            stats.Total = this.spy.Total;
            stats.TotalTimestamp = DateTime.UnixEpoch.AddSeconds(this.spy.TotalTimestamp);

            stats.StatsTimestamp = DateTime.UnixEpoch.AddSeconds(this.spy.Timestamp);

            return stats;
        }

    }
}

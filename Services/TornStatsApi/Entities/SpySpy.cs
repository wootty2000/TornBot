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
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TornBot.Services.TornStatsApi.Entities
{
    public class SpySpy
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("player_name")]
        public string PlayerName { get; set; }

        [JsonPropertyName("player_id")]
        public object PlayerId { get; set; }

        [JsonPropertyName("player_level")]
        public UInt16 PlayerLevel { get; set; }

        [JsonPropertyName("player_faction")]
        public string PlayerFaction { get; set; }

        //[JsonPropertyName("target_score")]
        //public double TargetScore { get; set; }

        //[JsonPropertyName("your_score")]
        //public double YourScore { get; set; }

        //[JsonPropertyName("fair_fight_bonus")]
        //public double FairFightBonus { get; set; }

        //[JsonPropertyName("difference")]
        //public string Difference { get; set; }

        [JsonPropertyName("timestamp")]
        public UInt32 Timestamp { get; set; }

        [JsonPropertyName("strength")]
        public UInt64 Strength { get; set; }

        //[JsonPropertyName("deltaStrength")]
        //public UInt64 DeltaStrength { get; set; }

        [JsonPropertyName("strength_timestamp")]
        public UInt32 StrengthTimestamp { get; set; }

        //[JsonPropertyName("effective_strength")]
        //public UInt64 EffectiveStrength { get; set; }

        [JsonPropertyName("defense")]
        public UInt64 Defense { get; set; }

        //[JsonPropertyName("deltaDefense")]
        //public UInt64 DeltaDefense { get; set; }

        [JsonPropertyName("defense_timestamp")]
        public UInt32 DefenseTimestamp { get; set; }

        //[JsonPropertyName("effective_defense")]
        //public UInt64 EffectiveDefense { get; set; }

        [JsonPropertyName("speed")]
        public UInt64 Speed { get; set; }

        //[JsonPropertyName("deltaSpeed")]
       // public UInt64 DeltaSpeed { get; set; }

        [JsonPropertyName("speed_timestamp")]
        public UInt64 SpeedTimestamp { get; set; }

        //[JsonPropertyName("effective_speed")]
        //public UInt64 EffectiveSpeed { get; set; }

        [JsonPropertyName("dexterity")]
        public UInt64 Dexterity { get; set; }

        //[JsonPropertyName("deltaDexterity")]
        //public UInt64 DeltaDexterity { get; set; }

        [JsonPropertyName("dexterity_timestamp")]
        public UInt32 DexterityTimestamp { get; set; }

        //[JsonPropertyName("effective_dexterity")]
        //public UInt64 EffectiveDexterity { get; set; }

        [JsonPropertyName("total")]
        public UInt64 Total { get; set; }

        //[JsonPropertyName("deltaTotal")]
       //public UInt64 DeltaTotal { get; set; }

       [JsonPropertyName("total_timestamp")]
        public UInt32 TotalTimestamp { get; set; }
    }

}

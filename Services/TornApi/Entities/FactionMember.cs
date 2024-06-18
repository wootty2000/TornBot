using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TornBot.Services.TornApi.Entities
{
    public class FactionMember
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("level")]
        public UInt32 Level { get; set; }

        [JsonPropertyName("days_in_faction")]
        public UInt16 DaysInFaction { get; set; }

        [JsonPropertyName("last_action")]
        public UserLastAction LastAction { get; set; }

        [JsonPropertyName("status")]
        public UserStatus Status { get; set; }

        [JsonPropertyName("position")]
        public string Position { get; set; }
    }
}

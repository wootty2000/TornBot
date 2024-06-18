using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TornBot.Services.TornApi.Entities
{
    public class FactionRank
    {
        [JsonPropertyName("level")]
        public UInt16 Level { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("division")]
        public UInt16 Division { get; set; }

        [JsonPropertyName("position")]
        public UInt16 Position { get; set; }

        [JsonPropertyName("wins")]
        public UInt16 Wins { get; set; }
    }
}

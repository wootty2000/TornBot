using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TornBot.Services.TornApi.Entities
{
    public class FactionMemberLastAction
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("timestamp")]
        public UInt32 Timestamp { get; set; }

        [JsonPropertyName("relative")]
        public string Relative { get; set; }
    }
}

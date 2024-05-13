using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TornBot.Services.Database.Entities;
using TornBot.Entities;

namespace TornBot.Services.TornApi.Entities
{
    public class Key
    {
        [JsonPropertyName("access_level")]
        public UInt16 AccessLevel { get; set; }

        [JsonPropertyName("access_type")]
        public string AccessType { get; set; }
        
        [JsonPropertyName("selections")]
        public Selections Selections { get; set; }

        [JsonPropertyName("player_id")]
        public UInt16 PlayerId { get; set; }

        public TornBot.Entities.ApiKeys ToApiKey()
        {
            TornBot.Entities.ApiKeys apiKey = new TornBot.Entities.ApiKeys();

            apiKey.AccessLevel = this.AccessLevel;
            apiKey.AccessType = this.AccessType;
            apiKey.PlayerId = this.PlayerId;

            return apiKey;
        }
    }
    
    public class Selections
    {
        [JsonPropertyName("company")]
        public List<string> Company { get; set; }

        [JsonPropertyName("faction")]
        public List<string> Faction { get; set; }

        [JsonPropertyName("market")]
        public List<string> Market { get; set; }

        [JsonPropertyName("property")]
        public List<string> Property { get; set; }

        [JsonPropertyName("torn")]
        public List<string> Torn { get; set; }

        [JsonPropertyName("user")]
        public List<string> User { get; set; }

        [JsonPropertyName("key")]
        public List<string> Key { get; set; }
    }
}

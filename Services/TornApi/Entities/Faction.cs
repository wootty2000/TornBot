using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TornBot.Services.TornApi.Entities
{
    public class Faction
    {
        [JsonPropertyName("ID")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("tag_image")]
        public string TagImage { get; set; }

        [JsonPropertyName("leader")]
        public int Leader { get; set; }

        [JsonPropertyName("co-leader")]
        public int CoLeader { get; set; }

        [JsonPropertyName("respect")]
        public int Respect { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("capacity")]
        public int Capacity { get; set; }

        [JsonPropertyName("best_chain")]
        public int BestChain { get; set; }
        /*  to do
        [JsonPropertyName("ranked_wars")]
        public Dictionary<string, object> RankedWars { get; set; }

        [JsonPropertyName("territory_wars")]
        public Dictionary<string, object> TerritoryWars { get; set; }

        [JsonPropertyName("raid_wars")]
        public Dictionary<string, object> RaidWars { get; set; }

        [JsonPropertyName("peace")]
        public Dictionary<string, object> Peace { get; set; }*/

        [JsonPropertyName("rank")]
        public FactionRank FactionRank { get; set; }

        [JsonPropertyName("members")]
        public Dictionary<UInt32, FactionMember> FactionMember { get; set; }

    }
}

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
        public UInt32 Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("tag_image")]
        public string TagImage { get; set; }

        [JsonPropertyName("leader")]
        public UInt32 Leader { get; set; }

        [JsonPropertyName("co-leader")]
        public UInt32 CoLeader { get; set; }

        [JsonPropertyName("respect")]
        public UInt64 Respect { get; set; }

        [JsonPropertyName("age")]
        public UInt16 Age { get; set; }

        [JsonPropertyName("capacity")]
        public UInt16 Capacity { get; set; }

        [JsonPropertyName("best_chain")]
        public UInt32 BestChain { get; set; }
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

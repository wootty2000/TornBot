using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornBot.Entities
{
    public class ApiKeys
    {
        private UInt32 playerId = 0;

        private UInt32 factionId = 0;

        private UInt16 accessLevel = 0;

        private string accessType = "";

        private string apiKey = "";

        private string tornStatsApiKey = "";

        private DateTime? tornApiAddedTimestamp = DateTime.FromFileTimeUtc(0);

        //private DateTime? tornstatsApiAddedTimestamp = DateTime.FromFileTimeUtc(0);

        private DateTime? tornLastUsed = DateTime.FromFileTimeUtc(0);
        private DateTime? tornstatsLastUsed = DateTime.FromFileTimeUtc(0);


        public UInt32 PlayerId { get; set; }
        public UInt32 FactionId { get; set; }

        public UInt16 AccessLevel { get; set; }
        public string AccessType { get; set; }

        public string ApiKey { get; set; }
        public string TornStatsApiKey { get; set; }

        public DateTime? TornApiAddedTimestamp { get; set; }
        //public DateTime? TornStatsApiAddedTimestamp { get; set; }
        public DateTime? TornLastUsed { get; set; }
        public DateTime? TornStatsLastUsed { get; set; }

    }
}

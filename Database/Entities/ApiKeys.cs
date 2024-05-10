using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mysqlx.Notice.Warning.Types;
using System.Xml.Linq;
using TornBot.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TornBot.Database.Entities
{
    public class ApiKeys
    {
        [Key]
        public UInt32 PlayerId { get; set; }
        public UInt32 FactionId { get; set; }

        public UInt16 AccessLevel { get; set; }
        public string AccessType { get; set; }

        public String ApiKey { get; set; }
        public String TornStatsApiKey { get; set; }

        public DateTime? TornApiAddedTimestamp { get; set; }
        public DateTime? TornStatsApiAddedTimestamp { get; set; }

        public DateTime? TornLastUsed { get; set; }
        public DateTime? TornStatsLastUsed { get; set; }

        public ApiKeys() { }
        
        public ApiKeys(TornBot.Entities.ApiKeys apiKeys) //add api key or update it for torn
        {
            PlayerId = apiKeys.PlayerId;
            FactionId = apiKeys.FactionId;
            AccessLevel = apiKeys.AccessLevel;
            AccessType = apiKeys.AccessType;
            ApiKey = apiKeys.ApiKey;
            TornStatsApiKey = apiKeys.TornStatsApiKey;
            TornApiAddedTimestamp = apiKeys.TornApiAddedTimestamp; 
            //TornStatsApiAddedTimestamp = apiKeys.TornStatsApiAddedTimestamp;
            TornLastUsed = DateTime.UtcNow;
            TornStatsLastUsed = apiKeys.TornStatsLastUsed;
        }
        /*
        public ApiKeys(TornBot.Entities.ApiKeys apiKeys) //add api key or update it for tornStats
        {
            PlayerId = apiKeys.PlayerId;
            FactionId = apiKeys.FactionId;
            AccessLevel = apiKeys.AccessLevel;
            AccessType = apiKeys.AccessType;
            ApiKey = apiKeys.ApiKey;
            TornStatsApiKey = apiKeys.TornStatsApiKey;
            TornApiAddedTimestamp = apiKeys.TornApiAddedTimestamp; //being passed in case we just want to use the api key and not add/update
            TornStatsApiAddedTimestamp = apiKeys.TornStatsApiAddedTimestamp;
            TornLastUsed = apiKeys.TornLastUsed;
            TornStatsLastUsed = DateTime.UtcNow;
        }*/

        public TornBot.Entities.ApiKeys ToApiKey()
        {
            TornBot.Entities.ApiKeys apiKeys = new TornBot.Entities.ApiKeys();
            apiKeys.PlayerId = PlayerId;
            apiKeys.FactionId = FactionId;
            apiKeys.AccessLevel = AccessLevel;
            apiKeys.AccessType = AccessType;
            apiKeys.ApiKey = ApiKey;
            apiKeys.TornStatsApiKey = TornStatsApiKey;
            apiKeys.TornApiAddedTimestamp = TornApiAddedTimestamp;
            //apiKeys.TornStatsApiAddedTimestamp = TornStatsApiAddedTimestamp;
            apiKeys.TornLastUsed = TornLastUsed;
            apiKeys.TornStatsLastUsed = TornStatsLastUsed;

            return apiKeys;
        }
    }
}

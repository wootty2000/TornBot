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

namespace TornBot.Services.Database.Entities
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
        
    }
}

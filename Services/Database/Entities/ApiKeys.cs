﻿using System;
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
        
        public String TornApiKey { get; set; }
        public UInt16 TornAccessLevel { get; set; }
        public UInt16 TornState { get; set; }
        public DateTime? TornLastUsed { get; set; }
        public DateTime? TornApiAddedTimestamp { get; set; }
        
        public String? TornStatsApiKey { get; set; }
        public DateTime? TornStatsLastUsed { get; set; }
        public DateTime? TornStatsApiAddedTimestamp { get; set; }
        
    }
}

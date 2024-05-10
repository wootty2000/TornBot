// TornBot
// 
// Copyright (C) 2024 TornBot.com
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
// 
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
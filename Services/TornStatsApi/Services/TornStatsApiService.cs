//
// TornBot
//
// Copyright (C) 2024 TornBot.com
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TornBot.Services.TornStatsApi.Services
{
    public class TornStatsApiService
    {
        private string baseUrl = "https://www.tornstats.com/api/";

        private readonly ILogger _logger;
        private readonly TornStatsApiKeys _tornStatsApiKeys;

        public TornStatsApiService(
            ILogger<TornStatsApiService> logger,
            TornStatsApiKeys tornStatsApiKeys)
        {
            _logger = logger;
            _tornStatsApiKeys = tornStatsApiKeys;
        }

        public string MakeApiRequest(string endpoint)
        {
            HttpClient client = new HttpClient();

            string url = baseUrl + endpoint;

            try
            {
                return client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error making API call to TornStats");
                return null;
            }
        }

        public TornBot.Entities.Stats GetStats(UInt32 id)
        {
            string key = _tornStatsApiKeys.GetNextKey();

            return GetPlayer(id, key);
        }

        public TornBot.Entities.Stats GetPlayer(UInt32 playerId, string apiKey)
        {
            string url = String.Format("v2/{0}/spy/user/{1}", apiKey, playerId.ToString());
            string apiResponse = MakeApiRequest(url);

            //Return if the response was null
            if(apiResponse == null)
            {
                return null;
            }

            try
            {
                TornStatsApi.Entities.Spy spy = JsonSerializer.Deserialize<TornStatsApi.Entities.Spy>(apiResponse);

                TornBot.Entities.Stats stats = spy.ToStats();

                return stats;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deserializing TornStats API Spy");
                
                return null;
            }
        }
    }
}

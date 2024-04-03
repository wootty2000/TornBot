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

        public TornBot.Entities.BattleStats GetPlayerStats(string playerIdOrName)
        {
            string key = _tornStatsApiKeys.GetNextKey();

            return GetPlayerStats(playerIdOrName, key);
        }

        public TornBot.Entities.BattleStats GetPlayerStats(string playerIdOrName, string apiKey)
        {
            string url = String.Format("v2/{0}/spy/user/{1}", apiKey, playerIdOrName);
            string apiResponse = MakeApiRequest(url);

            bool responseHandled = TornBot.Services.ResponseHandler.HandleResponse(JsonSerializer.Deserialize<dynamic>(apiResponse));

            if (responseHandled)
            {
                return null;
            }

            try
            {
                TornStatsApi.Entities.Spy spy = JsonSerializer.Deserialize<TornStatsApi.Entities.Spy>(apiResponse);

                TornBot.Entities.BattleStats battleStats = spy.ToBattleStats();

                return battleStats;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deserializing TornStats API Spy");

                return null;
            }
        }
    }
}

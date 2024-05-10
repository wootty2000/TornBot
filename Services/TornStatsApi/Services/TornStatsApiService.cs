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
using TornBot.Exceptions;

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

        /// <summary>
        /// Makes the actual GET request to TornStats' API
        /// </summary>
        /// <param name="endpoint">String of the endpoint including all parameters (inc Api Key)</param>
        /// <returns>String - the raw response from the API</returns>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public string MakeApiRequest(string endpoint)
        {
            HttpClient client = new HttpClient();
            string url = baseUrl + endpoint;

            JsonElement jsonResponse;
            try
            {
                string response = client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
                
                //Try to XML serialise the response string and use it to check for error status / error codes
                jsonResponse = JsonSerializer.Deserialize<dynamic>(response);

                //If the status property is True, the call was successful
                if (jsonResponse.TryGetProperty("status", out JsonElement statusElement) && statusElement.ValueKind == JsonValueKind.True)
                    return response;
            }
            catch (Exception e)
            {
                //TODO - log this properly. We probably want the ApiKey to be passed in for logging who's key went wrong
                Console.WriteLine("Error in rest call to TornStats: " + e.Message);
                throw new ApiCallFailureException("Error making API call to TornStats", e);
            }

            jsonResponse.TryGetProperty("message", out JsonElement messageElement);
            
            //If we got here, there was a an error code from Torn's API. Return the most appropriate exception
            throw messageElement.GetString() switch
            {
                "ERROR: User not found." => new ApiCallFailureException("Supplied API key is invalid", new InvalidKeyException()),
                "Error: No data found." => new ApiCallFailureException("Spy not found", new PlayerNotFoundException()),
                _ => new ApiCallFailureException( "Unknown Exception", new UnknownException(String.Format("TornState API Error Message: {0}", messageElement.GetString())))
            };

        }

        /// <summary>
        /// Attempt to get the Player's BattleStats from TornStats' API
        /// Will use the next available API key from the key store
        /// </summary>
        /// <param name="playerIdOrName">Torn Player id or name</param>
        /// <returns>TornBot.Entities.BattleStats object</returns>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornBot.Entities.BattleStats GetPlayerStats(string playerIdOrName)
        {
            //Loop until we get a valid response or run out of API keys or a other API call failure
            while (true)
            {
                string key;

                try
                {
                    key = _tornStatsApiKeys.GetNextKey();
                    
                    return GetPlayerStats(playerIdOrName, key);

                }
                catch (ApiCallFailureException e)
                {
                    if (e.InnerException is not null)
                    {
                        if (e.InnerException is InvalidKeyException)
                        {
                            //TODO mark the key as invalid in the key store
                            continue;
                        } 
                        else
                        {
                            throw;
                        }

                    }
                }
                catch (Exception e)
                {
                    throw new ApiCallFailureException("Error in TornStats API GetPlayerStats(string playerIdOrName)", e);
                }
            }
        }

        /// <summary>
        /// Attempt to get the Player's BattleStats from TornStats' API
        /// </summary>
        /// <param name="playerIdOrName">Torn Player id or name</param>
        /// <param name="apiKey">Api Key to use</param>
        /// <returns>TornBot.Entities.BattleStats object</returns>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornBot.Entities.BattleStats GetPlayerStats(string playerIdOrName, string apiKey)
        {
            string url = String.Format("v2/{0}/spy/user/{1}", apiKey, playerIdOrName);
            
            try
            {   
                string apiResponse = MakeApiRequest(url);

                TornStatsApi.Entities.Spy spy = JsonSerializer.Deserialize<TornStatsApi.Entities.Spy>(apiResponse);

                TornBot.Entities.BattleStats battleStats = spy.ToBattleStats();

                return battleStats;
            }
            catch (ApiCallFailureException)
            {
                throw;
            }
            catch (Exception e)
            {
                //TODO log this correctly
                throw new ApiCallFailureException("Error deserializing TornStats spy data", e);
            }

        }
    }
}

﻿//
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

using System.Text.Json;
using TornBot.Exceptions;

namespace TornBot.Services.TornApi.Services
{
    public class TornApiService
    {
        private string baseUrl = "https://api.torn.com/";

        private readonly TornApiKeys tornApiKeys;

        public TornApiService(TornApiKeys tornApiKeys)
        {
            this.tornApiKeys = tornApiKeys;
        }

        /// <summary>
        /// Makes the actual GET request to Torn's API
        /// </summary>
        /// <param name="endpoint">String of the endpoint including all parameters (inc Api Key)</param>
        /// <returns>String - the raw response from the API</returns>
        /// <exception cref="InvalidKeyException">The supplied key is no longer valid</exception>
        /// <exception cref="ApiNotAvailableException">API system is currently unavailable</exception>
        /// <exception cref="ApiKeyOwnerInactiveException">The API Key is currently suspended due to owner inactivity greater than 7 days</exception>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        private string MakeApiRequest(string endpoint)
        {
            HttpClient client = new HttpClient();
            string url = baseUrl + endpoint;

            JsonElement codeElement;
            try
            {
                string response = client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;

                //Try to XML serialise the response string and use it to check for error status / error codes
                JsonElement jsonResponse = JsonSerializer.Deserialize<dynamic>(response);
                
                //If there is no 'error' property in the XML, return the original response string
                if (!jsonResponse.TryGetProperty("error", out JsonElement errorElement)) 
                    return response;
                
                //If we got here, there was an error property.
                //Check for code. If it's not provider, or it is but it's not a number, return the original string
                if (!errorElement.TryGetProperty("code", out codeElement) || codeElement.ValueKind != JsonValueKind.Number) 
                    return response;
            }
            catch (Exception e)
            {
                //TODO - log this properly. We probably want the ApiKey to be passed in for logging who's key went wrong
                Console.WriteLine("Error in rest call to Torn: " + e.Message);
                throw new ApiCallFailureException("Error is rest call to Torn", e);
            }    
            
            //If we got here, there was a an error code from Torn's API. Return the most appropriate exception
            throw codeElement.GetInt16() switch
            {
                2 => new InvalidKeyException(),
                8 => new ApiCallFailureException("IP Blocked Exception", new IPBlockedException()),
                9 => new ApiNotAvailableException(),
                13 => new ApiKeyOwnerInactiveException(),
                _ => new ApiCallFailureException( "Unknown Exception", new UnknownException(String.Format("Torn API Error: {0}", codeElement.GetInt16().ToString())))
            };
            
        }

        /// <summary>
        /// Attempt to get the Player's information from Torn's API
        /// Will use the next available API key from the key store
        /// </summary>
        /// <param name="id">Torn Player id</param>
        /// <returns>TornBot.Entities.TornPlayer object</returns>
        /// <exception cref="NoMoreKeysAvailableException">No more API keys are available. Either Invalid, Inactive or we are rate limiting key use</exception>
        /// <exception cref="ApiNotAvailableException">API system is currently unavailable</exception>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornBot.Entities.TornPlayer GetPlayer(UInt32 id)
        {
            //Loop until we get a valid response or run out of API keys or a other API call failure
            while (true)
            {
                string key;
                
                try
                {
                    key = tornApiKeys.GetNextKey();

                    return GetPlayer(id, key);
                }
                catch (InvalidKeyException)
                {
                    //TODO mark the key as invalid in the key store
                    continue;
                }
                catch (ApiKeyOwnerInactiveException)
                {
                    //TODO mark the key as owner inactive in the key store
                    continue;
                }
                catch (Exception)
                {
                    //Redundant but makes it easier to read and understand
                    throw;
                }
            }
        }

        /// <summary>
        /// Attempts to gets the Faction's general information from Torn's API
        /// </summary>
        /// <param name="id">Torn Player id</param>
        /// <param name="apiKey">Api Key to use</param>
        /// <returns>TornBot.Entities.TornPlayer object of the player</returns>
        /// <exception cref="NoMoreKeysAvailableException">No more API keys are available. Either Invalid, Inactive or we are rate limiting key use</exception>
        /// <exception cref="ApiNotAvailableException">API system is currently unavailable</exception>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornBot.Entities.TornPlayer GetPlayer(UInt32 id, string apiKey)
        {
            string url = String.Format("user/{0}?key={1}", id.ToString(), apiKey);
            string apiResponse = MakeApiRequest(url);

            try
            {
                TornApi.Entities.User user = JsonSerializer.Deserialize<TornApi.Entities.User>(apiResponse);
                TornBot.Entities.TornPlayer tornPlayer = user.ToTornPlayer();
            
                return tornPlayer;
            }
            catch (Exception e)
            {
                //TODO log this correctly
                throw new ApiCallFailureException("Error deserializing torn player data", e);
            }
        }

        /// <summary>
        /// Attempts to gets the Faction's general information from Torn's API
        /// Will use the next available API key from the key store
        /// </summary>
        /// <param name="id">Torn Faction id</param>
        /// <returns>TornApi.Entities.Faction object of the faction</returns>
        /// <exception cref="NoMoreKeysAvailableException">No more API keys are available. Either Invalid, Inactive or we are rate limiting key use</exception>
        /// <exception cref="ApiNotAvailableException">API system is currently unavailable</exception>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornApi.Entities.Faction GetFaction(UInt32 id)
        {
            //Loop until we get a valid response or run out of API keys or a other API call failure
            while (true)
            {
                string key;
                
                try
                {
                    key = tornApiKeys.GetNextKey();
                    
                    return GetFaction(id, key);
                }
                catch (InvalidKeyException)
                {
                    //TODO mark the key as invalid in the key store
                    continue;
                }
                catch (ApiKeyOwnerInactiveException)
                {
                    //TODO mark the key as owner inactive in the key store
                    continue;
                }
                catch (Exception)
                {
                    //Redundant but makes it easier to read and understand
                    throw;
                }
            }
        }

        /// <summary>
        /// Attempts to gets the Faction's general information from Torn's API
        /// </summary>
        /// <param name="id">Torn Player id</param>
        /// <param name="apiKey">Api Key to use</param>
        /// <returns>TornApi.Entities.Faction object of the faction</returns>
        /// <exception cref="NoMoreKeysAvailableException">No more API keys are available. Either Invalid, Inactive or we are rate limiting key use</exception>
        /// <exception cref="ApiNotAvailableException">API system is currently unavailable</exception>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornApi.Entities.Faction GetFaction(UInt32 id, string key)
        {
            string url = String.Format("faction/{0}?selections=&key={1}", id.ToString(), key);
            string apiResponse = MakeApiRequest(url);

            try
            {
                TornApi.Entities.Faction faction = JsonSerializer.Deserialize<TornApi.Entities.Faction>(apiResponse);
                return faction;
            }
            catch (Exception e)
            {
                //TODO log this correctly
                throw new ApiCallFailureException("Error deserializing torn faction data", e);
            }
        }

        /// <summary>
        /// Attempts to get Stock data
        /// </summary>
        /// <returns>TornApi.Entities.StockResponse</returns>
        /// <exception cref="NoMoreKeysAvailableException">No more API keys are available. Either Invalid, Inactive or we are rate limiting key use</exception>
        /// <exception cref="ApiNotAvailableException">API system is currently unavailable</exception>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornApi.Entities.StockResponse GetStocks()
        {
            string key = tornApiKeys.GetNextKey();
            string url = String.Format("torn/?selections=stocks&key={0}", key);
            string apiResponse = MakeApiRequest(url);

            try
            {
                TornApi.Entities.StockResponse stocks = JsonSerializer.Deserialize<TornApi.Entities.StockResponse>(apiResponse);
                return stocks;
            }
            catch (Exception e)
            {
                //TODO log this correctly
                throw new ApiCallFailureException("Error deserializing stocks data", e);
            }
        }
    }
}

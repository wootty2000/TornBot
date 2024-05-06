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

using System.Text.Json;
using TornBot.Exceptions;

namespace TornBot.Services.TornApi.Services
{
    public class TornApiService
    {
        private string baseUrl = "https://api.torn.com/";

        private readonly TornApiKeys tornApiKeys;

        public TornApiService(TornApiKeys tornAPIKeys)
        {
            this.tornApiKeys = tornAPIKeys;
        }

        public string MakeApiRequest(string endpoint)
        {
            HttpClient client = new HttpClient();

            string url = baseUrl + endpoint;

            try
            {
                string response = client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;

                JsonElement jsonResponse = JsonSerializer.Deserialize<dynamic>(response);
                if (jsonResponse.TryGetProperty("error", out JsonElement errorElement))
                {
                    if (errorElement.TryGetProperty("code", out JsonElement codeElement) && codeElement.ValueKind == JsonValueKind.Number)
                    {
                        switch (codeElement.GetInt16())
                        {
                            case 2:
                                throw new InvalidKeyException();
                            case 8:
                                throw new IPBlockedException();
                            case 9:
                                throw new ApiNotAvailableException();
                            default:
                                throw new UnknownException(String.Format("Torn API Error: {0}", codeElement.GetInt16().ToString()));
                        }
                    }
                }
                
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in rest call to Torn: " + e.Message);
                throw new UnknownException("Error in rest call to Torn", e);
            }
        }

        public TornBot.Entities.TornPlayer? GetPlayer(UInt32 id)
        {
            //Loop until we get a valid response or run out of API keys or a general API failure
            while (true)
            {
                try
                {
                    string key = tornApiKeys.GetNextKey();

                    return GetPlayer(id, key);
                }
                catch (InvalidKeyException e)
                {
                    continue;
                }
                catch (Exception e)
                {
                    //Redundant but makes it easier to read and understand
                    throw;
                }
            }
        }

        public TornApi.Entities.Faction GetFaction(UInt32 id)
        {
            //Loop until we get a valid response or run out of API keys or a general API failure
            while (true)
            {
                try
                {
                    string key = tornApiKeys.GetNextKey();
                    
                    return GetFaction(id, key);
                }
                catch (InvalidKeyException e)
                {
                    continue;
                }
                catch (Exception e)
                {
                    //Redundant but makes it easier to read and understand
                    throw;
                }
            }
        }

        public TornBot.Entities.TornPlayer? GetPlayer(UInt32 id, string apiKey)
        {
            string url = String.Format("user/{0}?key={1}", id.ToString(), apiKey);
            string apiResponse = MakeApiRequest(url);
            
            TornApi.Entities.User user = JsonSerializer.Deserialize<TornApi.Entities.User>(apiResponse);

            TornBot.Entities.TornPlayer tornPlayer = user.ToTornPlayer();

            return tornPlayer;
        }

        public TornApi.Entities.StockResponse GetStocks()
        {
            string key = tornApiKeys.GetNextKey();
            string url = String.Format("torn/?selections=stocks&key={0}", key);
            string apiResponse = MakeApiRequest(url);

            TornApi.Entities.StockResponse stocks = JsonSerializer.Deserialize<TornApi.Entities.StockResponse>(apiResponse);

            return stocks;
        }
        public TornApi.Entities.Faction GetFaction(UInt32 id, string key)
        {
            string url = String.Format("faction/{0}?selections=&key={1}", id.ToString(), key);
            string apiResponse = MakeApiRequest(url);

            TornApi.Entities.Faction faction = JsonSerializer.Deserialize<TornApi.Entities.Faction>(apiResponse);

            return faction;
        }

        public TornBot.Entities.ReviveStatus GetReviveStatus(UInt32 id)
        {
            //We ideally want to use an external API key so it doesnt return friends/faction
            string key = tornApiKeys.GetNextKey();
            string url = String.Format("user/{0}?key={1}", id.ToString(), key);
            string apiResponse = MakeApiRequest(url);

            TornApi.Entities.User user = JsonSerializer.Deserialize<TornApi.Entities.User>(apiResponse);

            TornBot.Entities.ReviveStatus reviveStatus = user.ToReviveStatus();

            return reviveStatus;
        }
    }
}

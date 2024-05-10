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
                return client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in rest call to Torn: " + e.Message);
                return null;
            }
        }

        public TornBot.Entities.TornPlayer? GetPlayer(UInt32 id)
        {
            string key = tornApiKeys.GetNextKey(7);

            return GetPlayer(id, key);
        }

        public TornApi.Entities.Faction? GetFaction(UInt32 id)
        {
            string key = tornApiKeys.GetNextKey(7);

            return GetFaction(id, key);
        }

        public TornBot.Entities.TornPlayer? GetPlayer(UInt32 id, string apiKey)
        {
            string url = String.Format("user/{0}?key={1}", id.ToString(), apiKey);
            string apiResponse = MakeApiRequest(url);

            bool responseHandled = TornBot.Services.ResponseHandler.HandleResponse(JsonSerializer.Deserialize<dynamic>(apiResponse));

            if (responseHandled)
            {
                return null;
            }

            TornApi.Entities.User user = JsonSerializer.Deserialize<TornApi.Entities.User>(apiResponse);

            TornBot.Entities.TornPlayer tornPlayer = user.ToTornPlayer();

            return tornPlayer;
        }

        public TornApi.Entities.StockResponse GetStocks()
        {
            string key = tornApiKeys.GetNextKey(7);
            string url = String.Format("torn/?selections=stocks&key={0}", key);
            string apiResponse = MakeApiRequest(url);
            bool responseHandled = TornBot.Services.ResponseHandler.HandleResponse(JsonSerializer.Deserialize<dynamic>(apiResponse));

            if (responseHandled)
            {
                return null;
            }
            TornApi.Entities.StockResponse stocks = JsonSerializer.Deserialize<TornApi.Entities.StockResponse>(apiResponse);

            return stocks;
        }
        public TornApi.Entities.Faction GetFaction(UInt32 id, string key)
        {
            string url = String.Format("faction/{0}?selections=&key={1}", id.ToString(), key);
            string apiResponse = MakeApiRequest(url);
            bool responseHandled = TornBot.Services.ResponseHandler.HandleResponse(JsonSerializer.Deserialize<dynamic>(apiResponse));

            if (responseHandled)
            {
                return null;
            }
            TornApi.Entities.Faction faction = JsonSerializer.Deserialize<TornApi.Entities.Faction>(apiResponse);

            return faction;
        }
        public TornBot.Entities.ApiKeys GetApiKeyInfo(string apiKey)
        {
            string url = String.Format("key/?selections=info&key={0}", apiKey);
            string apiResponse = MakeApiRequest(url);

            bool responseHandled = TornBot.Services.ResponseHandler.HandleResponse(JsonSerializer.Deserialize<dynamic>(apiResponse));

            if (responseHandled)
            {
                return null;
            }

            TornApi.Entities.Key api_key = JsonSerializer.Deserialize<TornApi.Entities.Key>(apiResponse);

            TornBot.Entities.ApiKeys toApiKey = api_key.ToApiKey();
            

            return toApiKey;
        }
        
        public TornBot.Entities.TornPlayer GetApiKeyUser(string apiKey)
        {
            string url = String.Format("user/?key={0}", apiKey);
            string apiResponse = MakeApiRequest(url);

            bool responseHandled = TornBot.Services.ResponseHandler.HandleResponse(JsonSerializer.Deserialize<dynamic>(apiResponse));

            if (responseHandled)
            {
                //return null;
            }

            TornApi.Entities.User user = JsonSerializer.Deserialize<TornApi.Entities.User>(apiResponse);
            TornBot.Entities.TornPlayer toTornPlayer = user.ToTornPlayer();

            return toTornPlayer;
        }
    }
}

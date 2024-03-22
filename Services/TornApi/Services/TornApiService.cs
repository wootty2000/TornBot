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

        public TornBot.Entities.TornPlayer? GetPlayer(string playerIdOrName)
        {
            string key = tornApiKeys.GetNextKey();

            return GetPlayer(playerIdOrName, key);
        }

        public TornBot.Entities.TornPlayer? GetPlayer(string playerIdOrName, string apiKey)
        {
            string url = String.Format("user/{0}?key={1}", playerIdOrName, apiKey);
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
            string key = tornApiKeys.GetNextKey();
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
    }
}

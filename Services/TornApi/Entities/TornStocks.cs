/**
 * TornBot
 *
 * Copyright (C) 2024 TornBot.com
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TornBot.Services.TornApi.Entities
{

    public class StockResponse
    {
        [JsonPropertyName("stocks")]
        public Dictionary<string, Stock> Stocks { get; set; }
    }

    public class Stock
    {
        [JsonPropertyName("stock_id")]
        public int StockId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("acronym")]
        public string Acronym { get; set; }

        [JsonPropertyName("current_price")]
        public decimal CurrentPrice { get; set; }

        [JsonPropertyName("market_cap")]
        public long MarketCap { get; set; }

        [JsonPropertyName("total_shares")]
        public long TotalShares { get; set; }

        [JsonPropertyName("investors")]
        public int Investors { get; set; }

        [JsonPropertyName("benefit")]
        public StockBenefit Benefit { get; set; }
    }

    public class StockBenefit
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("frequency")]
        public int Frequency { get; set; }

        [JsonPropertyName("requirement")]
        public int Requirement { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
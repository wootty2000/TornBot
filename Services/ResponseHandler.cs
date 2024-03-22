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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TornBot.Services
{
    internal class ResponseHandler
    {
        public static bool HandleResponse(JsonElement responseObject)
        {
            try
            {
                if (responseObject.TryGetProperty("error", out JsonElement errorElement))//for Torn api
                {
                    if (errorElement.TryGetProperty("code", out JsonElement codeElement) && codeElement.ValueKind == JsonValueKind.Number)
                    {
                        Console.WriteLine("Error code from Torn API: " + codeElement.GetInt16());
                        return true;
                    }
                }
                else if (responseObject.TryGetProperty("status", out JsonElement statusElement) && statusElement.ValueKind == JsonValueKind.False)//for TornStats api
                {
                    Console.WriteLine("Error code from TornStats API: False");
                    return true;
                }
                else if (responseObject.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception from API ResponseHandler: " + e);
                return true;
            }
        }
    }
}

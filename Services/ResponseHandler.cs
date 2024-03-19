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

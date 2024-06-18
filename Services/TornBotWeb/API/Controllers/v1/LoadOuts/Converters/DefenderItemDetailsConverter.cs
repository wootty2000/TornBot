// TornBot
// 
// Copyright (C) 2024 TornBot.com
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
// 
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Text.Json;
using System.Text.Json.Serialization;
using TornBot.Services.TornBotWeb.API.Controllers.v1.LoadOuts.Models;

namespace TornBot.Services.TornBotWeb.API.Controllers.v1.LoadOuts.Converters;

public class DefenderItemDetailsConverter : JsonConverter<DefenderItemDetails>
{
    public override DefenderItemDetails Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var defenderItemDetails = new DefenderItemDetails();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return defenderItemDetails;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();

                    if (propertyName == "item")
                    {
                        defenderItemDetails.Item = JsonSerializer.Deserialize<List<Item>>(ref reader, options);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }

            throw new JsonException("Expected end of object.");
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            throw new JsonException("Empty item list detected.");
        }
        else
        {
            throw new JsonException("Expected start of object or array.");
        }
    }
    
    public override void Write(Utf8JsonWriter writer, DefenderItemDetails value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("item");
        JsonSerializer.Serialize(writer, value.Item, options);

        writer.WriteEndObject();
    }
}
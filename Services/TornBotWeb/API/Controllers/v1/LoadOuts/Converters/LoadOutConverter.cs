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

public class LoadOutConverter : JsonConverter<LoadOutModel>
{
    public override LoadOutModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object.");
        }

        var loadOutModel = new LoadOutModel();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return loadOutModel;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "DB":
                        loadOutModel.Db = JsonSerializer.Deserialize<LoadOutDb>(ref reader, options);
                        break;
                    case "defenderUser":
                        loadOutModel.Db.DefenderUser = JsonSerializer.Deserialize<DefenderUser>(ref reader, options);
                        break;
                    case "defenderItems":
                        loadOutModel.Db.DefenderItems = JsonSerializer.Deserialize<Dictionary<string, DefenderItemDetails>>(ref reader, options);
                        break;
                    case "defenderAmmoStatus":
                        loadOutModel.Db.DefenderAmmoStatus = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        throw new JsonException("Expected end of object.");
    }

    public override void Write(Utf8JsonWriter writer, LoadOutModel value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
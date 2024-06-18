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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TornBot.Services.TornApi.Entities
{
    public class UserStatus
    {
        [JsonPropertyName("description")]
        public string description { get; set; }

        [JsonPropertyName("details")]
        public string details { get; set; }

        [JsonPropertyName("state")]
        public string state { get; set; }

        [JsonPropertyName("color")]
        public string color { get; set; }

        [JsonPropertyName("until")]
        public UInt32 until { get; set; }
    }

}

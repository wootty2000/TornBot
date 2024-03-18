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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TornBot.Services.TornApi.Entities
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);

    public class User
    {
        [JsonPropertyName("rank")]
        public string rank { get; set; }

        [JsonPropertyName("level")]
        public UInt16 level { get; set; }

        [JsonPropertyName("honor")]
        public UInt16 honor { get; set; }

        [JsonPropertyName("gender")]
        public string gender { get; set; }

        [JsonPropertyName("property")]
        public string property { get; set; }

        [JsonPropertyName("signup")]
        public string signup { get; set; }

        [JsonPropertyName("awards")]
        public UInt16 awards { get; set; }

        [JsonPropertyName("friends")]
        public UInt16 friends { get; set; }

        [JsonPropertyName("enemies")]
        public UInt16 enemies { get; set; }

        [JsonPropertyName("forum_posts")]
        public UInt32 forum_posts { get; set; }

        [JsonPropertyName("karma")]
        public UInt32 karma { get; set; }

        [JsonPropertyName("age")]
        public UInt16 age { get; set; }

        [JsonPropertyName("role")]
        public string role { get; set; }

        [JsonPropertyName("donator")]
        public UInt16 donator { get; set; }

        [JsonPropertyName("player_id")]
        public UInt32 PlayerId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("property_id")]
        public UInt32 PropertyId { get; set; }

        [JsonPropertyName("revivable")]
        public UInt16 revivable { get; set; }

        [JsonPropertyName("profile_image")]
        public string profile_image { get; set; }

        [JsonPropertyName("life")]
        public UserLife life { get; set; }

        [JsonPropertyName("status")]
        public UserStatus status { get; set; }

        [JsonPropertyName("job")]
        public UserJob Job { get; set; }

        [JsonPropertyName("faction")]
        public UserFaction Faction { get; set; }

        [JsonPropertyName("married")]
        public UserMarried Married { get; set; }

        //Dont think we need icons
        //[JsonPropertyName("basicicons")]
        //public Basicicons basicicons { get; set; }

        [JsonPropertyName("states")]
        public UserStates States { get; set; }

        [JsonPropertyName("last_action")]
        public UserLastAction LastAction { get; set; }

        [JsonPropertyName("competition")]
        public UserCompetition Competition { get; set; }

        public TornBot.Entities.TornPlayer ToTornPlayer()
        {
            TornBot.Entities.TornPlayer tornPlayer = new TornBot.Entities.TornPlayer();
            TornBot.Entities.TornFaction tornFaction = new TornBot.Entities.TornFaction();

            tornFaction.Id = this.Faction.FactionId;
            tornFaction.Name = this.Faction.FactionName;

            tornPlayer.Id = this.PlayerId;
            tornPlayer.Name = this.Name;
            tornPlayer.Faction = tornFaction;

            return tornPlayer;
        }
    }
}

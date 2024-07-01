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

using TornBot.Entities;

namespace TornBot.Services.Factions.Database.Entities;

public class TornFactions
{
    public UInt32 Id { get; set; } = 0;
    public string Name { get; set; } = "";
    public string Tag { get; set; } = "";
    public string TagImage { get; set; } = "";
    public bool Monitor { get; set; } = false;

    public TornFactions()
    {
    }

    public TornFactions(TornBot.Entities.TornFaction tornFaction, bool monitor = false)
    {
        Id = tornFaction.Id;
        Name = tornFaction.Name;
        Tag = tornFaction.Tag;
        TagImage = tornFaction.Tag_image;
        Monitor = monitor;
    }

    public TornBot.Entities.TornFaction ToTornFaction()
    {
        return new TornFaction
        {
            Id = Id,
            Name = Name,
            Tag = Tag,
            Tag_image = TagImage
        };
    }
}
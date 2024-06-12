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

using TornBot.Entities;

namespace TornBot.Services.Database.Entities;

public class WeaponMods
{
    public UInt16 Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    
    public WeaponMods() {}

    public WeaponMods(TornBot.Entities.WeaponMod mod)
    {
        Id = mod.Id;
        Title = mod.Title;
        Description = mod.Description;
    }

    public TornBot.Entities.WeaponMod ToWeaponMod()
    {
        return new WeaponMod
        {
            Id = Id,
            Title = Title,
            Description = Description
        };
    }
}
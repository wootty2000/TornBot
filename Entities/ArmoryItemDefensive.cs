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

namespace TornBot.Entities;

public class ArmoryItemDefensive
{
    public ulong Uid { get; set; }
    public ushort Id { get; set; }
    public string Name { get; set; }

    public ArmoryItem.ItemColor Color { get; set; }
    
    public Single Armor { get; set; }
    public Single Quality { get; set; }

    public List<ItemRankedWarBonus> Bonuses { get; set; } = new List<ItemRankedWarBonus>();
    
    public ArmoryItem ToArmoryItem()
    {
        return new ArmoryItem()
        {
            Uid = Uid,
            Id = Id,
            Name = Name,
            Color = Color,
            Type = ArmoryItem.ItemType.Defensive,
            Damage = 0,
            Accuracy = 0,
            Armor = Armor,
            Quality = Quality,
            Bonuses = Bonuses,
            Mods = new List<WeaponMod>()
        };
    }

}
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

public class ArmoryItem
{
    public enum ItemColor
    {
        None = 0,
        Yellow = 1,
        Orange = 2,
        Red = 3
    };

    public ArmoryItem()
    {
        Bonuses = new List<ItemRankedWarBonus>();
    }

    public UInt64 Uid { get; set; }
    public UInt16 Id { get; set; }
    public string Name { get; set; }

    public ItemColor Color { get; set; }
    public List<ItemRankedWarBonus> Bonuses { get; set; }

    public Single Damage { get; set; }
    public Single Accuracy { get; set; }
    public List<WeaponMod> Mods { get; set; }

}
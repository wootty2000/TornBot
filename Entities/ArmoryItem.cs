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
    public enum ItemType
    {
        Unknown = 0,
        Primary = 1,
        Secondary = 2,
        Melee = 3,
        Temporary = 4,
        Defensive = 5
    }
    
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
        Mods = new List<WeaponMod>();
    }

    public UInt64 Uid { get; set; }
    public UInt16 Id { get; set; }
    public string Name { get; set; }

    public ItemType Type { get; set; }
    public ItemColor Color { get; set; }
    
    public Single Damage { get; set; }
    public Single Accuracy { get; set; }
    public Single Armor { get; set; }
    public Single Quality { get; set; }

    public List<ItemRankedWarBonus> Bonuses { get; set; }
    
    public List<WeaponMod> Mods { get; set; }

    public ArmoryItemPrimaryWeapon ToArmoryItemPrimaryWeapon()
    {
        return new ArmoryItemPrimaryWeapon
        {
            Uid = Uid,
            Id = Id,
            Name = Name,
            Color = Color,
            Damage = Damage,
            Accuracy = Accuracy,
            Quality = Quality,
            Bonuses = Bonuses,
            Mods = Mods
        };
    }

    public ArmoryItemSecondaryWeapon ToArmoryItemSecondaryWeapon()
    {
        return new ArmoryItemSecondaryWeapon
        {
            Uid = Uid,
            Id = Id,
            Name = Name,
            Color = Color,
            Damage = Damage,
            Accuracy = Accuracy,
            Quality = Quality,
            Bonuses = Bonuses,
            Mods = Mods
        };
    }

    public ArmoryItemMeleeWeapon ToArmoryItemMeleeWeapon()
    {
        return new ArmoryItemMeleeWeapon
        {
            Uid = Uid,
            Id = Id,
            Name = Name,
            Color = Color,
            Damage = Damage,
            Accuracy = Accuracy,
            Quality = Quality,
            Bonuses = Bonuses
        };
    }

    public ArmoryItemTemporaryWeapon ToArmoryItemTemporaryWeapon()
    {
        return new ArmoryItemTemporaryWeapon
        {
            Uid = Uid,
            Id = Id,
            Name = Name,
            Color = Color,
            Damage = Damage,
            Accuracy = Accuracy,
            Quality = Quality
        };
    }

    public ArmoryItemDefensive ToArmoryItemDefensive()
    {
        return new ArmoryItemDefensive
        {
            Uid = Uid,
            Id = Id,
            Name = Name,
            Color = Color,
            Armor = Armor,
            Quality = Quality,
            Bonuses = Bonuses
        };
    }

}
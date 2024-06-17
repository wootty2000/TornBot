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

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using TornBot.Entities;

namespace TornBot.Services.TornApi.Entities.Torn;

public class ItemDetails
{
    [JsonPropertyName("itemdetails")]
    public Item item { get; set; } 

    public TornBot.Entities.ArmoryItem ToArmoryItem()
    {
        TornBot.Entities.ArmoryItem armoryItem = new ArmoryItem
        {
            Uid = item.Uid,
            Id = item.Id,
            Name = item.Name,
            Type = item.Type switch {
                "Primary" => ArmoryItem.ItemType.Primary,
                "Secondary" => ArmoryItem.ItemType.Secondary,
                "Melee" => ArmoryItem.ItemType.Melee,
                "Temporary" => ArmoryItem.ItemType.Temporary,
                "Defense" => ArmoryItem.ItemType.Defensive,
                _ => ArmoryItem.ItemType.Unknown
            },
            Color = item.Rarity switch
            {
                "Yellow" => ArmoryItem.ItemColor.Yellow,
                "Orange" => ArmoryItem.ItemColor.Orange,
                "Red" => ArmoryItem.ItemColor.Red,
                _ => ArmoryItem.ItemColor.None
            },
            Damage = item.Damage,
            Accuracy = item.Accuracy,
            Armor = item.Armor,
            Quality = item.Quality,
            Bonuses = new List<ItemRankedWarBonus>(),
            Mods = new List<WeaponMod>()
        };
        
        foreach (var bonus in item.Bonuses)
        {
            armoryItem.Bonuses.Add(new ItemRankedWarBonus
            {
                Id = UInt16.Parse(bonus.Key),
                Title = bonus.Value.Bonus,
                Description = bonus.Value.Description,
                Value = bonus.Value.Value
            });
        }

        return armoryItem;
    }
}

public class Item
{
    [JsonPropertyName("ID")]
    public UInt16 Id { get; set; }
    
    [JsonPropertyName("UID")]
    public UInt64 Uid { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("rarity")]
    public string Rarity { get; set; }

    [JsonPropertyName("damage")]
    public Single Damage { get; set; } = 0;
    
    [JsonPropertyName("accuracy")]
    public Single Accuracy { get; set; } = 0;
        
    [JsonPropertyName("armor")]
    public Single Armor { get; set; } = 0;
    
    [JsonPropertyName("quality")]
    public Single Quality { get; set; } = 0;

    [JsonPropertyName("bonuses")]
    public Dictionary<string, ItemRWBonus> Bonuses { get; set; } = new Dictionary<string, ItemRWBonus>();
}

public class ItemRWBonus
{
    [JsonPropertyName("bonus")]
    public string Bonus { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("value")]
    public UInt16 Value { get; set; }


}
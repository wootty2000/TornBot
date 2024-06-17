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

namespace TornBot.Services.TornBotWeb.API.Controllers.v1.LoadOuts.Models;

public class Item
{
    public string EquipSlot { get; set; }
    [JsonPropertyName("ID")]
    public object Id { get; set; }
    public string Name { get; set; }
    public string ArmouryID { get; set; }
    public string ArmoryID { get; set; }
    public string Dmg { get; set; }
    public object Acc { get; set; }
    public string GlowClass { get; set; }
    public Dictionary<string, CurrentBonus> CurrentBonuses { get; set; }
    public List<CurrentUpgrade> CurrentUpgrades { get; set; }
}

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

using System.ComponentModel.DataAnnotations;

namespace TornBot.Services.Database.Entities;

public class LoadOuts
{
    [Key]
    public UInt32 PlayerId { get; set; }
    public DateTime Timestamp { get; set; }
    public UInt64 PrimaryWeapon { get; set; }
    public string PrimaryWeaponMods { get; set; }
    public UInt64 SecondaryWeapon { get; set; }
    public string SecondaryWeaponMods { get; set; }
    public UInt64 MeleeWeapon { get; set; }
    public UInt64 TemporaryWeapon { get; set; }
    public UInt64 HelmetArmor { get; set; }
    public UInt64 ChestArmor { get; set; }
    public UInt64 PantsArmor { get; set; }
    public UInt64 GlovesArmor { get; set; }
    public UInt64 BootsArmor { get; set; }
}
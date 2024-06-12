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
using TornBot.Entities;

namespace TornBot.Services.Database.Entities;

public class ArmoryItems
{
    [Key]
    public UInt64 Uid { get; set; }
    public UInt16 Id { get; set; }
    public string Name { get; set; }
    public UInt16 Color { get; set; }
    public Single Damage { get; set; }
    public Single Accuracy { get; set; }
    public UInt16 BonusId1 { get; set; }
    public UInt16 BonusVal1 { get; set; }
    public UInt16 BonusId2 { get; set; }
    public UInt16 BonusVal2 { get; set; }
    public UInt16 BonusId3 { get; set; }
    public UInt16 BonusVal3 { get; set; }

}
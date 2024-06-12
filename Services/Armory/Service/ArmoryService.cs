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

using TornBot.Services.Database;
using TornBot.Services.Database.Entities;

namespace TornBot.Services.Armory.Service;

public class ArmoryService
{
    private DatabaseContext _database;
    
    public ArmoryService(DatabaseContext databaseContext)
    {
        _database = databaseContext;
    }

    public bool CheckItemInDatabase(TornBot.Entities.ArmoryItem armoryItem)
    {
        return _database.ArmoryItems.Any(ai => ai.Uid == armoryItem.Uid);
    }
    
    public void AddItem(TornBot.Entities.ArmoryItem armoryItem)
    {
        TornBot.Services.Database.Entities.ArmoryItems dbArmoryItems = new TornBot.Services.Database.Entities.ArmoryItems();
        
        dbArmoryItems.Uid = armoryItem.Uid;
        dbArmoryItems.Id = armoryItem.Id;
        dbArmoryItems.Name = armoryItem.Name;
        dbArmoryItems.Color = (byte)armoryItem.Color;
        dbArmoryItems.Damage = armoryItem.Damage;
        dbArmoryItems.Accuracy = armoryItem.Accuracy;

        ArmoryItemRWBonus bonus;
            
        //If the weapon has a 1st RW bonus
        if (armoryItem.Bonuses.Count > 0)
        {
            dbArmoryItems.BonusId1 = armoryItem.Bonuses[0].Id;
            dbArmoryItems.BonusVal1 = armoryItem.Bonuses[0].Value;

            
            if(
                !_database.ArmoryItemRWBonus.Any(rwb => rwb.Id == dbArmoryItems.BonusId1 && rwb.Value == dbArmoryItems.BonusId1) &&
                !_database.ArmoryItemRWBonus.Any(rwb => rwb.Id == dbArmoryItems.BonusId1 && rwb.Value == null)
                
            )
            {
                bonus = new ArmoryItemRWBonus
                {
                    Id = armoryItem.Bonuses[0].Id,
                    Value = armoryItem.Bonuses[0].Value,
                    Bonus = armoryItem.Bonuses[0].Title,
                    Description = armoryItem.Bonuses[0].Description
                };

                _database.ArmoryItemRWBonus.Add(bonus);
            }
            
            
            _database.ArmoryItems.Add(dbArmoryItems);
        }

        //If the weapon has a 2nd RW bonus
        if (armoryItem.Bonuses.Count > 1)
        {
            dbArmoryItems.BonusId2 = armoryItem.Bonuses[1].Id;
            dbArmoryItems.BonusVal2 = armoryItem.Bonuses[1].Value;

            if (
                !_database.ArmoryItemRWBonus.Any(rwb => rwb.Id == dbArmoryItems.BonusId2 && rwb.Value == null) &&
                !_database.ArmoryItemRWBonus.Any(rwb => rwb.Id == dbArmoryItems.BonusId2 && rwb.Value == dbArmoryItems.BonusId2)
            )
            {
                bonus = new ArmoryItemRWBonus
                {
                    Id = armoryItem.Bonuses[1].Id,
                    Value = armoryItem.Bonuses[1].Value,
                    Bonus = armoryItem.Bonuses[1].Title,
                    Description = armoryItem.Bonuses[1].Description
                };

                _database.ArmoryItemRWBonus.Add(bonus);
            }
        }

        //If the weapon has a 3rd RW bonus. Dont think this exists, but best to be safe
        if (armoryItem.Bonuses.Count > 2)
        {
            dbArmoryItems.BonusId3 = armoryItem.Bonuses[2].Id;
            dbArmoryItems.BonusVal3 = armoryItem.Bonuses[2].Value;

            if (
                !_database.ArmoryItemRWBonus.Any(rwb => rwb.Id == dbArmoryItems.BonusId3 && rwb.Value == null) &&
                !_database.ArmoryItemRWBonus.Any(rwb => rwb.Id == dbArmoryItems.BonusId3 && rwb.Value == dbArmoryItems.BonusId3)
            )
            {
                bonus = new ArmoryItemRWBonus
                {
                    Id = armoryItem.Bonuses[2].Id,
                    Value = armoryItem.Bonuses[2].Value,
                    Bonus = armoryItem.Bonuses[2].Title,
                    Description = armoryItem.Bonuses[2].Description
                };

                _database.ArmoryItemRWBonus.Add(bonus);
            }
        }
        
        _database.SaveChanges();
    }
}
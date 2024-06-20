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

    public bool CheckItemInDatabase(ArmoryItem armoryItem)
    {
        // There is no point in storing individual temp items. 
        // They are not unique and we do not want to store thousands of HEGs for example. 
        if (armoryItem.Type == ArmoryItem.ItemType.Temporary)
            return _database.ArmoryItems.Any(ai => ai.Id == armoryItem.Id);
        else
            return _database.ArmoryItems.Any(ai => ai.Uid == armoryItem.Uid);
    }
    
    public void AddItem(TornBot.Entities.ArmoryItem armoryItem)
    {
        TornBot.Services.Database.Entities.ArmoryItems dbArmoryItems = new TornBot.Services.Database.Entities.ArmoryItems();

        // There is no point in storing individual temp items. 
        // They are not unique and we do not want to store thousands of HEGs for example. 
        if (armoryItem.Type == ArmoryItem.ItemType.Temporary)
            dbArmoryItems.Uid = armoryItem.Id;
        else
            dbArmoryItems.Uid = armoryItem.Uid;

        dbArmoryItems.Id = armoryItem.Id;
        dbArmoryItems.Name = armoryItem.Name;
        dbArmoryItems.Type = (byte)armoryItem.Type;
        dbArmoryItems.Color = (byte)armoryItem.Color;
        dbArmoryItems.Damage = armoryItem.Damage;
        dbArmoryItems.Accuracy = armoryItem.Accuracy;
        dbArmoryItems.Armor = armoryItem.Armor;
        dbArmoryItems.Quality = armoryItem.Quality;

        ArmoryItemRWBonus bonus;
            
        //If the item has a 1st RW bonus
        if (armoryItem.Bonuses.Count > 0)
        {
            dbArmoryItems.BonusId1 = armoryItem.Bonuses[0].Id;
            dbArmoryItems.BonusVal1 = armoryItem.Bonuses[0].Value;

            // Bonus with id and value as null, its been manually created to allow for String.Format to add the value correctly
            // We can fall back on id and value if the above hasnt been created
            if(
                !_database.ArmoryItemRWBonus.Any(rwb => rwb.BonusId == dbArmoryItems.BonusId1 && rwb.Value == null) && 
                !_database.ArmoryItemRWBonus.Any(rwb => rwb.BonusId == dbArmoryItems.BonusId1 && rwb.Value == dbArmoryItems.BonusId1)
            )
            {
                bonus = new ArmoryItemRWBonus
                {
                    BonusId = armoryItem.Bonuses[0].Id,
                    Value = armoryItem.Bonuses[0].Value,
                    Bonus = armoryItem.Bonuses[0].Title,
                    Description = armoryItem.Bonuses[0].Description
                };

                _database.ArmoryItemRWBonus.Add(bonus);
            }
        }

        //If the item has a 2nd RW bonus
        if (armoryItem.Bonuses.Count > 1)
        {
            dbArmoryItems.BonusId2 = armoryItem.Bonuses[1].Id;
            dbArmoryItems.BonusVal2 = armoryItem.Bonuses[1].Value;

            if (
                !_database.ArmoryItemRWBonus.Any(rwb => rwb.BonusId == dbArmoryItems.BonusId2 && rwb.Value == null) &&
                !_database.ArmoryItemRWBonus.Any(rwb => rwb.BonusId == dbArmoryItems.BonusId2 && rwb.Value == dbArmoryItems.BonusId2)
            )
            {
                bonus = new ArmoryItemRWBonus
                {
                    BonusId = armoryItem.Bonuses[1].Id,
                    Value = armoryItem.Bonuses[1].Value,
                    Bonus = armoryItem.Bonuses[1].Title,
                    Description = armoryItem.Bonuses[1].Description
                };

                _database.ArmoryItemRWBonus.Add(bonus);
            }
        }

        //If the item has a 3rd RW bonus. Dont think this exists, but best to be safe
        if (armoryItem.Bonuses.Count > 2)
        {
            dbArmoryItems.BonusId3 = armoryItem.Bonuses[2].Id;
            dbArmoryItems.BonusVal3 = armoryItem.Bonuses[2].Value;

            if (
                !_database.ArmoryItemRWBonus.Any(rwb => rwb.BonusId == dbArmoryItems.BonusId3 && rwb.Value == null) &&
                !_database.ArmoryItemRWBonus.Any(rwb => rwb.BonusId == dbArmoryItems.BonusId3 && rwb.Value == dbArmoryItems.BonusId3)
            )
            {
                bonus = new ArmoryItemRWBonus
                {
                    BonusId = armoryItem.Bonuses[2].Id,
                    Value = armoryItem.Bonuses[2].Value,
                    Bonus = armoryItem.Bonuses[2].Title,
                    Description = armoryItem.Bonuses[2].Description
                };

                _database.ArmoryItemRWBonus.Add(bonus);
            }
        }
        
        _database.ArmoryItems.Add(dbArmoryItems);
        _database.SaveChanges();
       
    }

    public TornBot.Entities.ArmoryItem GetItem(UInt64 uid, bool isTemporaryWeapon = false)
    {
        Database.Entities.ArmoryItems? dbItem;
        
        // There is no point in storing individual temp items. 
        // They are not unique and we do not want to store thousands of HEGs for example. 
        if (isTemporaryWeapon)
            dbItem = _database.ArmoryItems.FirstOrDefault(ai => ai.Id == uid);
        else
            dbItem = _database.ArmoryItems.FirstOrDefault(ai => ai.Uid == uid);

        TornBot.Entities.ArmoryItem item = new TornBot.Entities.ArmoryItem();
        
        if (dbItem == null)
        {
            if (isTemporaryWeapon)
            {
                item.Uid = 0;
                item.Id = (UInt16)uid;
                item.Name = "Unkown Temp Item";
            }
            else
            {
                item.Uid = uid;
                item.Id = 0;
                item.Name = "Unknown Item";
            }
            item.Color = 0;
            
            //TODO - Pull the item from Torn API instead of giving an unknown item
            return item;
        }
        
        if (isTemporaryWeapon)
            item.Uid = 0;
        else
            item.Uid = dbItem.Uid;
        item.Id = dbItem.Id;
        item.Name = dbItem.Name;
        item.Type = (TornBot.Entities.ArmoryItem.ItemType)dbItem.Type;
        item.Color = (TornBot.Entities.ArmoryItem.ItemColor)dbItem.Color;
        item.Damage = dbItem.Damage;
        item.Accuracy = dbItem.Accuracy;
        item.Armor = dbItem.Armor;
        item.Quality = dbItem.Quality;
        if (dbItem.BonusId1 > 0)
            item.Bonuses.Add(GetItemRWBonus(dbItem.BonusId1, dbItem.BonusVal1));
        if (dbItem.BonusId2 > 0)
            item.Bonuses.Add(GetItemRWBonus(dbItem.BonusId2, dbItem.BonusVal2));
        if (dbItem.BonusId3 > 0)
            item.Bonuses.Add(GetItemRWBonus(dbItem.BonusId3, dbItem.BonusVal3));

        return item;
    }

    public TornBot.Entities.ItemRankedWarBonus GetItemRWBonus(UInt16 id, UInt16 value)
    {

        Database.Entities.ArmoryItemRWBonus? dbBonus = 
            _database.ArmoryItemRWBonus.Where(b => b.BonusId == id && b.Value == null).FirstOrDefault();
        TornBot.Entities.ItemRankedWarBonus bonus = new ItemRankedWarBonus();

        if (dbBonus == null)
        {
            dbBonus = _database.ArmoryItemRWBonus.FirstOrDefault(b =>
                b.BonusId == id && b.Value == value);
            bonus.Description = dbBonus.Description;
        }
        else
        {
            bonus.Description = String.Format(dbBonus.Description, value);
        }
        
        bonus.Id = id;
        bonus.Value = value;
        bonus.Title = dbBonus.Bonus;

        return bonus;
    }

    public bool CheckWeaponModInDatabase(TornBot.Entities.WeaponMod mod)
    {
        return _database.WeaponMods.Any(wm => wm.Id == mod.Id);
    }

    public void AddWeaponMod(TornBot.Entities.WeaponMod mod)
    {
        _database.WeaponMods.Add(new Database.Entities.WeaponMods(mod));
        _database.SaveChanges();
    }

    public TornBot.Entities.WeaponMod GetWeaponMod(UInt16 id)
    {
        Database.Entities.WeaponMods dbMod = _database.WeaponMods.FirstOrDefault(wm => wm.Id == id);
        
        if (dbMod == null)
            return new TornBot.Entities.WeaponMod();

        return dbMod.ToWeaponMod();
    }
}
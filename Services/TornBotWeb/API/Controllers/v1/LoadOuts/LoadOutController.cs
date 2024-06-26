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

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TornBot.Entities;
using TornBot.Services.Players.Service;
using TornBot.Services.TornApi.Services;
using TornBot.Services.TornBotWeb.API.Controllers.v1.LoadOuts.Converters;
using TornBot.Services.TornBotWeb.API.Controllers.v1.LoadOuts.Models;
using TornBot.Services.TornBotWeb.API.Controllers.v1.Spy;

namespace TornBot.Services.TornBotWeb.API.Controllers.v1.LoadOuts;

[Route("api/v1/[controller]")]
[ApiController]
public class LoadOutController : ControllerBase
{
    private readonly ILogger<LoadOutController> _logger;
    private readonly TornApiService _tornApiService;
    private readonly PlayersService _playersService;
    
    private readonly JsonSerializerOptions _options;

    public LoadOutController(ILogger<LoadOutController> logger, TornApiService tornApiService, PlayersService playersService)
    {
        _logger = logger;
        _tornApiService = tornApiService;
        _playersService = playersService;
        
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                //new LoadOutConverter(),
                new DefenderItemDetailsConverter()
            }
        };
    }

    [HttpGet]
    public IActionResult Get()
    {
        //{ "error": { "code" => 2, "error" => "Invalid API key" }}
        return BadRequest(Common.Common.GetErrorInvalidAPIKey());
    }
    
    // GET api/<LoadOutController>/1234?key=ApiKey
    [HttpGet("{id}")]
    public IActionResult Get([FromRoute] LoadOutGetModel model)
    {
        // Validate the API key
        if (!_tornApiService.IsApiKeyFromHomeFaction(model.key))
        {
            return BadRequest(Common.Common.GetErrorInvalidAPIKey());
        }

        TornBot.Entities.LoadOut loadOut = new LoadOut();
        
        loadOut = _playersService.GetPlayerLoadOut(UInt32.Parse(model.id));
        
        return Ok(loadOut);
    }
    
    [HttpPost("{id}")]
    [LoadOutPostResultFilter]
    public IActionResult Post([FromRoute] LoadOutPostModel loadOutPostModel)
    {
        // Validate the API key
        if (!_tornApiService.IsApiKeyFromHomeFaction(loadOutPostModel.key))
        {
            return BadRequest(Common.Common.GetErrorInvalidAPIKey());
        }
        
        LoadOutModel loadOutPost;
        LoadOutDb loadOutPostDb;
        try
        {
            loadOutPost = JsonSerializer.Deserialize<LoadOutModel>(loadOutPostModel.jsonElement.GetRawText(), _options);
            loadOutPostDb = loadOutPost.Db;
        }
        catch (JsonException ex)
        {
            string[] data = [loadOutPostModel.id, _tornApiService.GetPlayerId(loadOutPostModel.key)];
            _logger.LogError(ex, "Error deserializing LoadOut json. Req Id {id}. Key Owner {owner}", data); 
            
            return BadRequest(InvalidLoadOutData());
        }

        //Make sure the LoadOut is for the Id
        UInt32 playerId = UInt32.Parse(loadOutPostDb.DefenderUser.UserID.ToString());
        if (UInt32.Parse(loadOutPostModel.id) != playerId)
        {
            string[] data = [loadOutPostModel.id, playerId.ToString(), _tornApiService.GetPlayerId(loadOutPostModel.key)];
            _logger.LogError("Error deserializing LoadOut json. Req Id {id}. LoadOut Id {id2}. Key Owner {owner}", data); 

            return BadRequest(InvalidLoadOutData());
        }
        
        LoadOut loadOut = new TornBot.Entities.LoadOut();

        foreach (var defenderItem in loadOutPostDb.DefenderItems)
        {
            Item item = defenderItem.Value.Item[0];
            
            switch (item.EquipSlot)
            {
                // Weapons
                // Primary
                case "1":
                    try
                    {
                        loadOut.PrimaryWeapon = _tornApiService.GetItem(UInt64.Parse(item.ArmoryID)).ToArmoryItemPrimaryWeapon();
                    }
                    catch
                    {
                        loadOut.PrimaryWeapon = DecodeArmoryItem(item).ToArmoryItemPrimaryWeapon();
                    }

                    loadOut.PrimaryWeapon.Mods = DecodeWeaponMods(item);
                    break;
                
                // Secondary
                case "2":
                    try
                    {
                        loadOut.SecondaryWeapon = _tornApiService.GetItem(UInt64.Parse(item.ArmoryID)).ToArmoryItemSecondaryWeapon();
                    }
                    catch
                    {
                        loadOut.SecondaryWeapon = DecodeArmoryItem(item).ToArmoryItemSecondaryWeapon();
                    }
                    
                    loadOut.SecondaryWeapon.Mods = DecodeWeaponMods(item);
                    break;
                
                // Melee
                case "3":
                    try
                    {
                        loadOut.MeleeWeapon = _tornApiService.GetItem(UInt64.Parse(item.ArmoryID)).ToArmoryItemMeleeWeapon();
                    }
                    catch
                    {
                        loadOut.MeleeWeapon = DecodeArmoryItem(item).ToArmoryItemMeleeWeapon();
                    }
                    break;
                
                // Temporary
                case "5":
                    try
                    {
                        loadOut.TempWeapon = _tornApiService.GetItem(UInt64.Parse(item.ArmoryID)).ToArmoryItemTemporaryWeapon();
                    }
                    catch
                    {
                        loadOut.TempWeapon = DecodeArmoryItem(item).ToArmoryItemTemporaryWeapon();
                    }
                    break;
                
                // Armor
                // Helmet
                case "6":
                    try
                    {
                        loadOut.HelmetArmor = _tornApiService.GetItem(UInt64.Parse(item.ArmoryID)).ToArmoryItemDefensive();
                    }
                    catch
                    {
                        loadOut.HelmetArmor = DecodeArmoryItem(item).ToArmoryItemDefensive();
                    }
                    break;

                // Chest
                case "4":
                    try
                    {
                        loadOut.ChestArmor = _tornApiService.GetItem(UInt64.Parse(item.ArmoryID)).ToArmoryItemDefensive();
                    }
                    catch
                    {
                        loadOut.ChestArmor = DecodeArmoryItem(item).ToArmoryItemDefensive();
                    }
                    break;
                
                // Pants
                case "7":
                    try
                    {
                        loadOut.PantsArmor = _tornApiService.GetItem(UInt64.Parse(item.ArmoryID)).ToArmoryItemDefensive();
                    }
                    catch
                    {
                        loadOut.PantsArmor = DecodeArmoryItem(item).ToArmoryItemDefensive();
                    }
                    break;
                
                // Gloves
                case "9":
                    try
                    {
                        loadOut.GlovesArmor = _tornApiService.GetItem(UInt64.Parse(item.ArmoryID)).ToArmoryItemDefensive();
                    }
                    catch
                    {
                        loadOut.GlovesArmor = DecodeArmoryItem(item).ToArmoryItemDefensive();
                    }
                    break;
                
                // Boots
                case "8":
                    try
                    {
                        loadOut.BootsArmor = _tornApiService.GetItem(UInt64.Parse(item.ArmoryID)).ToArmoryItemDefensive();
                    }
                    catch
                    {
                        loadOut.BootsArmor = DecodeArmoryItem(item).ToArmoryItemDefensive();
                    }
                    break;
            }
        }
        
        _playersService.RecordPlayerLoadOut(playerId, loadOut);
        
        Console.Write(loadOut.ToString());
        return Ok(loadOut);
    }
    
    private List<WeaponMod> DecodeWeaponMods(Item item)
    {
        List<WeaponMod> weaponMods = new List<WeaponMod>();

        if (item.CurrentUpgrades is null)
            return weaponMods;
        
        WeaponMod weaponMod;
        foreach (var mod in item.CurrentUpgrades)
        {
            weaponMod = new WeaponMod();
            
            weaponMod.Id = UInt16.Parse(mod.UpgradeID);
            weaponMod.Title = mod.Title;
            weaponMod.Description = mod.Desc;
            
            weaponMods.Add(weaponMod);
        }

        return weaponMods;
    }
    
    private ArmoryItem DecodeArmoryItem(Item item)
    {
        ArmoryItem armoryItem = new ArmoryItem();
        
        armoryItem.Uid = UInt64.Parse(item.ArmoryID);
        armoryItem.Id = UInt16.Parse(item.Id.ToString());
        armoryItem.Name = item.Name;
        armoryItem.Damage = Single.Parse(item.Dmg);
        armoryItem.Accuracy = Single.Parse(item.Acc.ToString());
        armoryItem.Color = item.GlowClass switch
        {
            "glow-yellow" => ArmoryItem.ItemColor.Yellow,
            "glow-orange" => ArmoryItem.ItemColor.Orange,
            "glow-red" => ArmoryItem.ItemColor.Red,
            _ => ArmoryItem.ItemColor.None
        };

        if (item.CurrentBonuses is null)
            return armoryItem;
        
        foreach (var bonus in item.CurrentBonuses)
        {
            armoryItem.Bonuses.Add(DecodeRankedWarBonus(UInt16.Parse(bonus.Key), bonus.Value));
        }

        return armoryItem;
    }

    private ItemRankedWarBonus DecodeRankedWarBonus(UInt16 id, CurrentBonus bonus)
    {
        ItemRankedWarBonus itemRankedWarBonus = new ItemRankedWarBonus();
        itemRankedWarBonus.Id = id;
        itemRankedWarBonus.Value = (UInt16)bonus.Value;
        itemRankedWarBonus.Title = bonus.Title;
        itemRankedWarBonus.Description = bonus.Desc;

        return itemRankedWarBonus;
    }

    private object InvalidLoadOutData()
    {
        return new
        {
            error = new
            {
                code = 2,
                error = "Invalid LoadOut data"
            }
        };
    }
}
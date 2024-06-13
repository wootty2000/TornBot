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
    private readonly TornApiService _tornApiService;
    private readonly PlayersService _playersService;
    
    private readonly JsonSerializerOptions _options;

    public LoadOutController(TornApiService tornApiService, PlayersService playersService)
    {
        _tornApiService = tornApiService;
        _playersService = playersService;
        
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new LoadOutConverter(),
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
        if (!Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            return BadRequest(Common.Common.GetErrorInvalidAPIKey());
        }

        // Validate the API key
        if (!_tornApiService.IsApiKeyFromHomeFaction(apiKey))
        {
            return BadRequest(Common.Common.GetErrorInvalidAPIKey());
        }
        
        LoadOutModel loadOutPost;
        try
        {
            loadOutPost = JsonSerializer.Deserialize<LoadOutModel>(jsonElement.GetRawText(), _options);
        }
        catch (JsonException ex)
        {
            return BadRequest(ex.Message);
        }

        LoadOut loadOut = new TornBot.Entities.LoadOut();

        foreach (var defenderItem in loadOutPost.DefenderItems)
        {
            Item item = defenderItem.Value.Item[0];

            switch (item.EquipSlot)
            {
                //Weapons
                case "1":
                    loadOut.PrimaryWeapon = DecodeArmoryItem(item);
                    loadOut.PrimaryWeapon.Mods = DecodeWeaponMods(item);
                    break;
                case "2":
                    loadOut.SecondaryWeapon = DecodeArmoryItem(item);
                    loadOut.SecondaryWeapon.Mods = DecodeWeaponMods(item);
                    break;
                case "3":
                    loadOut.MeleeWeapon = DecodeArmoryItem(item);
                    break;
                case "5":
                    loadOut.TempWeapon = DecodeArmoryItem(item);
                    break;
                
                //Armor
                case "6":
                    loadOut.HelmetArmor = DecodeArmoryItem(item);
                    break;
                case "4":
                    loadOut.ChestArmor = DecodeArmoryItem(item);
                    break;
                case "7":
                    loadOut.PantsArmor = DecodeArmoryItem(item);
                    break;
                case "9":
                    loadOut.GlovesArmor = DecodeArmoryItem(item);
                    break;
                case "8":
                    loadOut.BootsArmor = DecodeArmoryItem(item);
                    break;
            }
        }
        
        UInt32 playerId = UInt32.Parse(loadOutPost.DefenderUser.UserID.ToString());
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
}
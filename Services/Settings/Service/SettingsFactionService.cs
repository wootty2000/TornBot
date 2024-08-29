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

namespace TornBot.Services.Settings.Service;

public class SettingsFactionService
{
    private SettingsService _settings;
    
    public SettingsFactionService(SettingsService settingsService)
    {
        _settings = settingsService;
        _settings.SetModuleName("Faction");
    }

    public UInt32[] GetHomeFactionIds()
    {
        string factionIds = _settings.GetSetting("HomeFactionIds");

        return Array.ConvertAll(factionIds.Split(','), UInt32.Parse);
    }

    public void SetHomeFactionsIds(UInt32[] factionIds)
    {
        _settings.SetSetting("HomeFactionIds", string.Join(',', factionIds));

    }
}
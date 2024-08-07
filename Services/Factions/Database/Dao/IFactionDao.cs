﻿//
// TornBot
//
// Copyright (C) 2024 TornBot.com
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using TornBot.Services.Factions.Database.Entities;

namespace TornBot.Services.Factions.Database.Dao;

public interface IFactionDao
{
    public TornFactions? GetFactionById(UInt32 id);
    public string GetFactionNameById(UInt32 id);
    public List<TornFactions> GetFactionsForMonitoring();
    public void AddOrUpdateTornFaction(TornBot.Entities.TornFaction faction);
}

//
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

namespace TornBot.Services.Database.TornPlayers.Entities;

public class TornPlayers
{
    public UInt32 Id { get; set; }
    public string Name { get; set; }
    public UInt16 Level { get; set; }
    public UInt32 FactionId { get; set; }
    public DateTime LastUpdated { get; set; }

    public TornPlayers() { }

    // Used for a new insert
    public TornPlayers(TornBot.Entities.TornPlayer tornPlayer)
    {
        Id = tornPlayer.Id;
        Name = tornPlayer.Name;
        Level = tornPlayer.Level;
        FactionId = tornPlayer.FactionId;
        LastUpdated = DateTime.UtcNow;
    }

    public bool IsEqual(TornPlayers tornPlayers)
    {
        if (Id != tornPlayers.Id)
            return false;
        if (!Name.Equals(tornPlayers.Name))
            return false;
        if (Level != tornPlayers.Level)
            return false;
        if (FactionId != tornPlayers.FactionId)
            return false;

        return true;
    }
        
    public void ParseTornPlayer(TornBot.Entities.TornPlayer tornPlayer)
    {
        Name = tornPlayer.Name;
        Level = tornPlayer.Level;
        FactionId = tornPlayer.FactionId;
        LastUpdated = DateTime.UtcNow;
    }

    public void ParseTornPlayer(TornPlayers tornPlayers)
    {
        Name = tornPlayers.Name;
        Level = tornPlayers.Level;
        FactionId = tornPlayers.FactionId;
        LastUpdated = DateTime.UtcNow;
    }

    public TornBot.Entities.TornPlayer ToTornPlayer()
    {
        return new TornBot.Entities.TornPlayer
        {
            Id = Id,
            Name = Name,
            Level = Level,
            FactionId = FactionId,
            DatabaseLastUpdated = LastUpdated
        };
    }
        
}
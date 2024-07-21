using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TornBot.Entities;

namespace TornBot.Services.TornApi.Entities;

public class FactionMember
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("level")]
    public UInt32 Level { get; set; }

    [JsonPropertyName("days_in_faction")]
    public UInt16 DaysInFaction { get; set; }

    [JsonPropertyName("last_action")]
    public UserLastAction LastAction { get; set; }

    [JsonPropertyName("status")]
    public UserStatus Status { get; set; }

    [JsonPropertyName("position")]
    public string Position { get; set; }

    public TornPlayer ToTornPlayer(UInt32 id)
    {
        return new TornPlayer
        {
            Id = id,
            Name = Name,
            Level = (UInt16)Level,
            Status = Status.state switch
            {
                "Okay" => TornPlayer.PlayerStatus.Okay,
                "Traveling" => TornPlayer.PlayerStatus.Traveling,
                "Abroad" => TornPlayer.PlayerStatus.Abroad,
                "Hospital" => TornPlayer.PlayerStatus.Hospital,
                "Jail" => TornPlayer.PlayerStatus.Jail,
                "Federal" => TornPlayer.PlayerStatus.Federal,
                "Fallen" => TornPlayer.PlayerStatus.Fallen,
                _ => TornPlayer.PlayerStatus.Unknown
            },
            OnlineStatus = LastAction.Status switch
            {
                "Offline" => TornPlayer.PlayerOnlineStatus.Offline,
                "Idle" => TornPlayer.PlayerOnlineStatus.Idle,
                "Online" => TornPlayer.PlayerOnlineStatus.Online,
                _ => TornPlayer.PlayerOnlineStatus.Unknown
            },
            LastAction = DateTime.UnixEpoch.AddSeconds(LastAction.Timestamp)
        };
    }
}


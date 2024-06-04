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

using DSharpPlus;
using DSharpPlus.Entities;
using NLog;
using NLog.Common;
using NLog.Targets;

namespace TornBot.Services.Logger.Targets;

[Target("Discord")]
public class DiscordTarget : TargetWithLayout
{
    private DiscordClient _discordClient;
    private DiscordChannel _logChannel;
    
    public void Initialize(DiscordClient discordClient, DiscordChannel logChannel)
    {
        _discordClient = discordClient;
        _logChannel = logChannel;
    }

    protected override void Write(LogEventInfo logEvent)
    {
        if (_discordClient == null || _logChannel == null)
        {
            return;
        }
        
        var logMessage = this.Layout.Render(logEvent);
        SendLogMessageToDiscord(logMessage).ConfigureAwait(false);
    }

    private async Task SendLogMessageToDiscord(string message)
    {
        try
        {
            await _logChannel.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            InternalLogger.Error($"Failed to send log message to Discord. Exception: {ex}");
        }
    }
}
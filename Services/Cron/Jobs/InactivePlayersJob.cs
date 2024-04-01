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

using Microsoft.Extensions.Logging;
using Quartz;
using TornBot.Services.Cron.Infrastructure;
using TornBot.Services.TornApi.Services;
using TornBot.Services.Discord.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Google.Protobuf.WellKnownTypes;

namespace TornBot.Services.Cron.Jobs
{
    public class InactivePlayersJob : WorkerJob
    {
        TornApiService tornApiService;
        DiscordService discordService;
        DiscordClient discord;

        //private static decimal[,] arrayOFstocks = new decimal[35, 2];

        private readonly ILogger<InactivePlayersJob> _logger;
        private static string _cronExpression = "0 10 0 1/1 * ? *";//<-At 00:10 daily

        public static void AddJob(IServiceCollection services)
        {
            services.AddQuartz(conf =>
            {
                JobKey jobKey = new JobKey("InactivePlayersJob-Job", "Cron");
                conf.AddJob<InactivePlayersJob>(j => j.WithIdentity(jobKey));
                conf.AddTrigger(trigger => trigger
                    .WithIdentity("InactivePlayersJob-Trigger", "Cron")
                    .ForJob(jobKey)
                    .WithCronSchedule(_cronExpression));
            });
            services.AddSingleton<InactivePlayersJob>();
        }

        public InactivePlayersJob(ILogger<InactivePlayersJob> logger, TornApiService tornAPIService, DiscordService discordService, DiscordClient discord)
        {
            _logger = logger;
            this.tornApiService = tornAPIService;
            this.discordService = discordService;
            this.discord = discord;
        }

        public Task Execute(IJobExecutionContext context)
        {
            //_logger.LogInformation("Stock update");
            UInt32 factionID = 0;
            TornApi.Entities.Faction Faction = tornApiService.GetFaction(factionID);

            if (Faction == null)
            {
                _logger.LogInformation("Stocks update failed due to API error");
                return null;
            }

            long currentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();


            var inactiveMembers = new List<string>();
            var inactiveTimes = new List<string>();

            foreach (var member in Faction.FactionMember)
            {

                if (member.Value.Status.State != "Fallen")
                {
                    long lastActionTimestamp = member.Value.LastAction.Timestamp;

                    double timeDifference = currentTimestamp - lastActionTimestamp;

                    timeDifference = timeDifference / 86400;
                    //Console.WriteLine(timeDifference);
                    if (timeDifference > 1)
                    {
                        string playerName = member.Value.Name + " [" + member.Key + "]";

                        inactiveMembers.Add($"[{playerName}](https://www.torn.com/profiles.php?XID={member.Key})");
                        //double time_h = timeDifference - (int)timeDifference;
                        //time_h = time_h * 24;
                        inactiveTimes.Add((int)timeDifference + "d " + (int)((timeDifference - (int)timeDifference) * 24) + "h");
                        //var playerLink = $"[{playerName}](https://www.torn.com/loader.php?sid=attack&user2ID={playerID})";
                    }
                }
            }
            long channel_id = long.Parse(discordService.GetInactivePlayerChannelId());

            var channel = discord.GetChannelAsync((ulong)channel_id);
            channel.ContinueWith((task) =>
            {
                if (task.IsCompletedSuccessfully && task.Result is DiscordChannel textChannel)
                {
                    if (inactiveMembers.Count > 0)
                    {

                        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                        {

                            Title = "Inactive Players",
                            Timestamp = DateTime.Now
                        };
                        embed.AddField("Player Name", string.Join("\n", inactiveMembers), true);
                        embed.AddField("Inactive for", string.Join("\n", inactiveTimes), true);

                        textChannel.SendMessageAsync(embed.Build()).Wait();
                    }
                    else
                    {
                        textChannel.SendMessageAsync(DateTime.Now.ToString("dd/MM/yyyy HH:mm") + " - No inactive members found.").Wait();
                    }
                }
            });
            return Task.CompletedTask;
        }
    }

}

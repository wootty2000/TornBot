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

using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using TornBot.Services.Cron.Infrastructure;
using TornBot.Services.Discord.Services;
using TornBot.Services.TornApi.Services;

namespace TornBot.Features.StocksMonitor.Cron
{
    public class StocksJob : WorkerJob
    {
        TornApiService tornApiService;
        DiscordService discordService;
        DiscordClient discord;

        private static decimal[,] arrayOFstocks = new decimal[35, 2];

        private readonly ILogger<StocksJob> _logger;
        private static string _cronExpression = "4/30 * * * * ? *";

        public static void AddJob(IServiceCollection services)
        {
            services.AddQuartz(conf =>
            {
                JobKey jobKey = new JobKey("StocksJob-Job", "Cron");
                conf.AddJob<StocksJob>(j => j.WithIdentity(jobKey));
                conf.AddTrigger(trigger => trigger
                    .WithIdentity("StocksJob-Trigger", "Cron")
                    .ForJob(jobKey)
                    .WithCronSchedule(_cronExpression));
            });
            services.AddSingleton<StocksJob>();
        }
        
        public StocksJob(ILogger<StocksJob> logger, TornApiService tornAPIService, DiscordService discordService, DiscordClient discord)
        {
            _logger = logger;
            this.tornApiService = tornAPIService;
            this.discordService = discordService;
            this.discord = discord;
        }
        
        public Task Execute(IJobExecutionContext context)
        {
            //_logger.LogInformation("Stock update");

            Services.TornApi.Entities.StockResponse stockResponse = tornApiService.GetStocks();

            if (stockResponse == null)
            {
                _logger.LogError("Stocks update failed due to API error");
                return null;
            }

            int i = 0;
            var stockMove = new List<(int num, long money, int investors, decimal share_price, string acronym, string name, string buy_sell, string info, string symbol, int role)>();

            bool start = false;

            foreach (var stock in stockResponse.Stocks)
            {
                decimal money_in_stock = (decimal)stock.Value.TotalShares * stock.Value.CurrentPrice;
                if (arrayOFstocks[i, 0] == 0)
                {
                    start = true;
                }

                //buy
                if ((money_in_stock - arrayOFstocks[i, 0]) >= 150000000000)
                {
                    stockMove.Add((stock.Value.StockId, (long)money_in_stock - (long)arrayOFstocks[i, 0], stock.Value.Investors - (int)arrayOFstocks[i, 1], stock.Value.CurrentPrice, stock.Value.Acronym, stock.Value.Name, "[BUY] ", "Purchase Info:", "+", 1));
                }
                //sell
                else if ((arrayOFstocks[i, 0] - money_in_stock) >= 150000000000)
                {
                    stockMove.Add((stock.Value.StockId, (long)money_in_stock - (long)arrayOFstocks[i, 0], stock.Value.Investors - (int)arrayOFstocks[i, 1], stock.Value.CurrentPrice, stock.Value.Acronym, stock.Value.Name, "[SELL] ", "Sale Info:", "-", 1));
                }

                arrayOFstocks[i, 0] = money_in_stock;
                arrayOFstocks[i, 1] = (decimal)stock.Value.Investors;

                i++;
            }
            
            bool isEmpty = !stockMove.Any();

            if (!isEmpty)
            {
                if (start != true)
                {
                    Console.WriteLine("Stock Move Data:");
                    foreach (var move in stockMove)
                    {
                        Console.WriteLine($"Stock ID: {move.num}, Acronym: {move.acronym}, Name: {move.name}, Money: {move.money}, Investors: {move.investors}, Buy/Sell: {move.buy_sell}");
                        string moneyIn = "";
                        double cal = 0;
                        long plus = 0;

                        if (move.money < 0)
                        {
                            plus = move.money * (-1);
                        }
                        else
                        {
                            plus = move.money;
                        }


                        if (plus > 999999999999)//t
                        {
                            cal = (double)plus / 1000000000000;
                            moneyIn = string.Format("{0:0.00}" + "tn", Math.Truncate(cal * 1000) / 1000);
                        }
                        else if (plus > 999999999)//b
                        {
                            cal = (double)plus / 1000000000;
                            moneyIn = string.Format("{0:0.00}" + "bn", Math.Truncate(cal * 1000) / 1000);
                        }



                        DiscordEmbedBuilder.EmbedAuthor embedAuthor = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = move.buy_sell + move.acronym,
                            IconUrl = $"https://www.torn.com/images/v2/stock-market/logos/{move.acronym}.png",
                            Url = $"https://www.torn.com/page.php?sid=stocks&stockID={move.num}&tab=owned"
                        };
                        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                        {
                            Timestamp = DateTime.Now,
                            Author = embedAuthor
                        };
                        embed.AddField(move.info, move.symbol + "$" + moneyIn);
                        if (move.investors < 0)
                        {
                            embed.AddField("Change in Investors:", move.investors.ToString());
                        }
                        else
                        {
                            embed.AddField("Change in Investors:", move.symbol + move.investors.ToString());
                        }

                        embed.AddField("Share price", "$" + move.share_price.ToString());

                        long channel_id = long.Parse(discordService.GetStocksChannelId());

                        var channel = discord.GetChannelAsync((ulong)channel_id);
                        channel.ContinueWith((task) =>
                        {
                            if (task.IsCompletedSuccessfully && task.Result is DiscordChannel textChannel)
                            {
                                textChannel.SendMessageAsync(embed.Build()).Wait();
                            }
                        });
                    }
                }
                else
                {
                    _logger.LogInformation("Stock market is being checked.");
                }

            }
            else
            {
                //Console.WriteLine("No stock move data available.");
            }
                        
            return Task.CompletedTask;
        }
    }

}

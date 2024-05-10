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

using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TornBot.Database;

namespace TornBot.Services.TornStatsApi.Services
{
    public class TornStatsApiKeys
    {
        private readonly IServiceProvider serviceProvider;

        public TornStatsApiKeys(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public string GetNextKey()
        {
            /* accessLevel is uint value that describing which AccessLevel from database we want to use 
             * AccessLevel
             * 1-4 - for torn perms (from public to full access)
             * 5 - with fac perms
             * 6 - outside api key (for checking revives)
             * 7 - any 1-4 torn perms
             * 
             */
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                TornBot.Entities.ApiKeys apiKeyInfo;
                Database.Entities.ApiKeys? dbPlayer;
                dbPlayer = dbContext.ApiKeys //get the torn stats api key that hasnt been used for the longest
                .Where(s => s.TornStatsApiKey != "")
                .OrderBy(s => s.TornLastUsed)
                .FirstOrDefault();
                if (dbPlayer != null)
                {
                    try
                    {
                    dbContext.Entry(dbPlayer).State = EntityState.Detached;
                    Console.WriteLine("Db torn stats api key used: " + dbPlayer.TornStatsApiKey);

                    apiKeyInfo = dbPlayer.ToApiKey();
                    apiKeyInfo.TornStatsLastUsed = DateTime.UtcNow;  //set LastUsed to now
                    dbContext.ApiKeys.Update(new Database.Entities.ApiKeys(apiKeyInfo)); //updates LastUsed 
                    dbContext.SaveChanges();
                    return dbPlayer.TornStatsApiKey;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred: " + ex.Message);
                        // You can log the exception or handle it as needed
                    }
                }
                else
                    return null;
                return null;
            }
        }
    }
}

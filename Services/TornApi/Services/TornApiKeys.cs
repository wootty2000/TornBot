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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TornBot.Services.Database;

namespace TornBot.Services.TornApi.Services
{
    public class TornApiKeys
    {
        //private readonly IConfigurationRoot config;
        private readonly IServiceProvider serviceProvider;

        public TornApiKeys(/*IConfigurationRoot config, */IServiceProvider serviceProvider)
        {
            //this.config = config;
            this.serviceProvider = serviceProvider;
        }
        /*
          public TornApiKeys(
            DatabaseContext database
        ){
            _database = database;
            
        }
         */
        public string GetNextKey(UInt16 accessLevel)
        {
            /* accessLevel is uint value that describing which AccessLevel from database we want to use 
             * AccessLevel
             * 1-4 - for torn perms (from public to full access)
             * 5 - with fac perms                                                       //to do
             * 6 - outside api key (for checking revives)
             * 7 - any 1-4 torn perms
             * 
             */
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                TornBot.Entities.ApiKeys apiKeyInfo;
                Database.Entities.ApiKeys? dbPlayer;

                if (accessLevel == 7)
                {
                dbPlayer = dbContext.ApiKeys //get the api key that hasnt been used for the longest
                .Where(s => s.AccessLevel < 5)
                .OrderBy(s => s.TornLastUsed)
                .FirstOrDefault();
                }
                else
                {
                dbPlayer = dbContext.ApiKeys //get the api key that hasnt been used for the longest
                .Where(s => s.AccessLevel == accessLevel)
                .OrderBy(s => s.TornLastUsed)
                .FirstOrDefault();
                }

                if (dbPlayer != null)
                {
                    dbContext.Entry(dbPlayer).State = EntityState.Detached;
                    Console.WriteLine("Db api key used: " + dbPlayer.ApiKey);

                    apiKeyInfo = dbPlayer.ToApiKey();
                    apiKeyInfo.TornLastUsed = DateTime.UtcNow;  //set LastUsed to now

                    dbContext.ApiKeys.Update(new Database.Entities.ApiKeys(apiKeyInfo)); //updates LastUsed 
                    dbContext.SaveChanges();

                    return dbPlayer.ApiKey;
                }else
                    return null;


                
            }
        }
    }
}
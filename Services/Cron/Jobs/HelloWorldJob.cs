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

namespace TornBot.Services.Cron.Jobs
{
    public class HelloWorldJob : WorkerJob
    {
        private readonly ILogger<HelloWorldJob> _logger;

        public static string GetCronExpression()
        {
            return "*/5 * * * * ? *";
        }

        public HelloWorldJob(ILogger<HelloWorldJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            //_logger.LogInformation("Hello world!");
            return Task.CompletedTask;
        }
    }

}

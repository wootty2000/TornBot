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

using Microsoft.Extensions.DependencyInjection;
using Quartz.Impl;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TornBot.Services.Cron.Jobs;
using TornBot.Services.Cron.Services;
using TornBot.Services.Cron.Infrastructure.JobFactory;
using TornBot.Services.Cron.Infrastructure;

namespace TornBot.Services.Cron
{
    public class CromModule : IModule
    {
        public IServiceCollection RegisterModule(IServiceCollection services)
        {
            Console.WriteLine("Adding Quartz");

            // Add Quartz services
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            //Add the main runner
            services.AddSingleton<QuartzJobRunner>();


            //Loop through all the Jobs and get a list of them to add as singltons to the service collection
            var type = typeof(WorkerJob);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

            foreach (var job in types)
            {
                //Get the job's cron expression via reflection
                string cronExpression = (string) job.GetMethod("GetCronExpression").Invoke(null, null);

                services.AddSingleton(job);
                services.AddSingleton(new JobSchedule(
                    jobType: job,
                    cronExpression: cronExpression));
            }

            //Add Quartz
            services.AddSingleton<QuartzService>();
            services.AddHostedService(s => s.GetRequiredService<QuartzService>());

            return services;
        }
    }
}

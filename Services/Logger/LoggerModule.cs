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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using TornBot.Services.Logger.Targets;

namespace TornBot.Services.Logger;

public class LoggerModule
{
    public void ConfigureNLog(IConfigurationRoot tornbotConfig, IServiceCollection services)
    {
        var config = new LoggingConfiguration();
        
        string connectionString = string.Format(
            "Server={0}; User ID={1}; Password={2}; Database={3}",
            tornbotConfig.GetValue<string>("DbHost"),
            tornbotConfig.GetValue<string>("DbUser"),
            tornbotConfig.GetValue<string>("DbPass"),
            tornbotConfig.GetValue<string>("DbDatabase")
        );
        
        // Configure database target for MySQL (replace with your details)
        var databaseTarget = new DatabaseTarget("TornBotMySql")
        {
            DBProvider = "MySql.Data.MySqlClient.MySqlConnection, MySql.Data",
            ConnectionString = connectionString,
            CommandText = "INSERT INTO Logs (Level, Message, Logger, Exception) VALUES (@Level, @Message, @Logger, @Exception);"
        };
        databaseTarget.Parameters.Add(new DatabaseParameterInfo("@Level", new SimpleLayout("${level}")));
        databaseTarget.Parameters.Add(new DatabaseParameterInfo("@Message", new SimpleLayout("${message}")));
        databaseTarget.Parameters.Add(new DatabaseParameterInfo("@Logger", new SimpleLayout("${logger}")));
        databaseTarget.Parameters.Add(new DatabaseParameterInfo("@Exception", new SimpleLayout("${exception:format=tostring}")));

        var inMemoryTarget = new InMemoryTarget();

        var discordTarget = new DiscordTarget()
        {
            Name = "Discord"
        };

        config.AddTarget(databaseTarget);
        config.AddTarget(discordTarget);

        config.AddRule(LogLevel.Warn, LogLevel.Fatal, databaseTarget);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, inMemoryTarget);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, discordTarget);
        
        LogManager.Configuration = config;
        
        services.AddSingleton(config);
    }
}
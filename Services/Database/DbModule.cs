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

namespace TornBot.Services.Database;

public interface IDbModule
{
    void RegisterDbModule(IConfigurationRoot config, IServiceCollection services);
}

public static class DbModuleExtensions
{
    static readonly List<IDbModule> registeredDbModules = new List<IDbModule>();

    public static IServiceCollection RegisterDbModules(this IConfigurationRoot config, IServiceCollection services)
    {
        var modules = DiscoverDbModules();
        foreach (var module in modules)
        {
            module.RegisterDbModule(config, services);

            registeredDbModules.Add(module);
        }

        return services;
    }

    private static IEnumerable<IDbModule> DiscoverDbModules()
    {
        return typeof(IDbModule).Assembly
            .GetTypes()
            .Where(p => p.IsClass && p.IsAssignableTo(typeof(IDbModule)))
            .Select(Activator.CreateInstance)
            .Cast<IDbModule>();
    }

    public static List<IDbModule> GetRegisteredDbModules()
    {
        return registeredDbModules;
    }
}

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

using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace TornBot.Services.Logger;
    
public class NLogLoggerFactory : ILoggerFactory
{
    private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

    public NLogLoggerFactory()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.AddNLog());
    }

    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        return _loggerFactory.CreateLogger(categoryName);
    }

    public void AddProvider(ILoggerProvider provider)
    {
        _loggerFactory.AddProvider(provider);
    }

    public void Dispose()
    {
        _loggerFactory.Dispose();
    }
}
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

using NLog;
using NLog.Targets;

namespace TornBot.Services.Logger.Targets;

public class InMemoryTarget : Target
{
    private readonly LogEventInfo[] _buffer;
    private int _head;
    private int _tail;

    public InMemoryTarget(int capacity = 1000)
    {
        _buffer = new LogEventInfo[capacity];
    }

    protected override void Write(LogEventInfo logEvent)
    {
        //We dont need to log CF Core Command Info messages
        if (logEvent.LoggerName == "Microsoft.EntityFrameworkCore.Database.Command" && logEvent.Level == LogLevel.Info)
            return;
        
        _buffer[_head] = logEvent;
        _head = (_head + 1) % _buffer.Length; // Wrap around if head reaches end

        if (_head == _tail)
        {
            // Buffer is full, overwrite the oldest log
            _tail = (_tail + 1) % _buffer.Length;
        }
    }

    public List<LogEventInfo> GetLogs(int count)
    {
        var logs = new List<LogEventInfo>();
        count = Math.Min(count, _buffer.Length); // Limit count to buffer capacity

        // Handle case where head is less than tail (no wrapping)
        if (_head < _tail)
        {
            int remaining = _head - _tail;
            if (count <= remaining)
            {
                for (int i = _tail; i < _tail + count; i++)
                {
                    logs.Add(_buffer[i]);
                }
            }
            else
            {
                for (int i = _tail; i < _head; i++)
                {
                    logs.Add(_buffer[i]);
                }
                for (int i = 0; i < count - remaining; i++)
                {
                    logs.Add(_buffer[i]);
                }
            }
        }
        else
        {
            // Handle case where head has wrapped around
            int remaining = _buffer.Length - _tail;
            if (count <= remaining)
            {
                for (int i = _tail; i < _tail + count; i++)
                {
                    logs.Add(_buffer[i]);
                }
            }
            else
            {
                for (int i = _tail; i < _buffer.Length; i++)
                {
                    logs.Add(_buffer[i]);
                }
                for (int i = 0; i < count - remaining; i++)
                {
                    logs.Add(_buffer[i]);
                }
            }
        }

        return logs;
    }
}

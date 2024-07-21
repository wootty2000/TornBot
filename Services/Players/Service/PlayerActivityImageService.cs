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

using SkiaSharp;
using TornBot.Entities;

namespace TornBot.Services.Players.Service;

public class PlayerActivityImageService : IPlayerActivityImageService
{
    private static readonly int SlotWidth = 5;
    private static readonly int SlotHeight = 20;
    private static readonly int HeaderHeight = 20;
    private static readonly int HeaderWidth = 100;
    private static readonly int BorderWidth = 20; // Width of the outer border
    private static int InfoHeight = 50; // Height for player info and week dates

    public Stream GeneratePlayerActivityImage(PlayerStatusData playerStatus)
    {
        InfoHeight = 50;
        
        int rows = 7; // Days in a week
        int columns = 288; // 24 hours * 12 slots per hour (5-minute intervals)
        int width = columns * SlotWidth + HeaderWidth + BorderWidth * 2;
        int height = rows * SlotHeight + HeaderHeight + BorderWidth * 2 + InfoHeight;

        var imageInfo = new SKImageInfo(width, height);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.White); // Background color

        DrawPlayerInfo(canvas, playerStatus);
        DrawHeaders(canvas);
        DrawGridLines(canvas, columns, rows);
        DrawPlayerStatusCells(canvas, playerStatus, rows);
        DrawBorders(canvas, width, height);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        MemoryStream memoryStream = new MemoryStream();
        data.SaveTo(memoryStream);
        memoryStream.Position = 0; // Reset stream position for reading
        return memoryStream;
    }

    public Stream GenerateFactionHeatmapImage(List<PlayerStatusData> playerStatuses, TornFaction faction, DateTime startDate)
    {
        InfoHeight = 70;
        
        int rows = 7; // Days in a week
        int columns = 288; // 24 hours * 12 slots per hour (5-minute intervals)
        int width = columns * SlotWidth + HeaderWidth + BorderWidth * 2;
        int height = rows * SlotHeight + HeaderHeight + BorderWidth * 2 + InfoHeight;

        var imageInfo = new SKImageInfo(width, height);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.White); // Background color

        Dictionary<DateTime, int> heatmapData = GenerateHeatMapData(playerStatuses, startDate, rows);
        
        int maxOnlinePlayers = 0;
        foreach (var entry in heatmapData)
        {
            if (entry.Value > maxOnlinePlayers)
                maxOnlinePlayers = entry.Value;
        }
        
        DrawFactionInfo(canvas, faction, startDate, maxOnlinePlayers); // Include faction info
        DrawHeaders(canvas);
        DrawGridLines(canvas, columns, rows);
        DrawHeatmapCells(canvas, heatmapData, maxOnlinePlayers);
        DrawBorders(canvas, width, height);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        MemoryStream memoryStream = new MemoryStream();
        data.SaveTo(memoryStream);
        memoryStream.Position = 0; // Reset stream position for reading
        return memoryStream;
    }
    
    private void DrawPlayerInfo(SKCanvas canvas, PlayerStatusData playerStatus)
    {
        var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Left,
            TextSize = 16,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

        // Draw Player Id
        string playerInfo = $"Player ID: {playerStatus.Id}";
        canvas.DrawText(playerInfo, BorderWidth, BorderWidth, paint);

        // Draw Week Dates
        DateTime weekStarting = playerStatus.WeekStarting;
        DateTime weekEnding = weekStarting.AddDays(6);
        string dateInfo = $"Week: {weekStarting:yyyy-MM-dd} 00:00 to {weekEnding:yyyy-MM-dd} 23:55";
        canvas.DrawText(dateInfo, BorderWidth, BorderWidth + 20, paint);
    }

    private void DrawFactionInfo(SKCanvas canvas, TornFaction faction, DateTime weekStarting, int maxOnlinePlayers)
    {
        var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Left,
            TextSize = 20,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

        // Draw faction Id
        string factionInfo = $"Faction: {faction.Name} - {faction.Tag} ({faction.Id})";
        canvas.DrawText(factionInfo, BorderWidth, BorderWidth + 10, paint);

        // Draw max online
        string maxOnlineInfo = $"Max online players: {maxOnlinePlayers}";
        canvas.DrawText(maxOnlineInfo, BorderWidth, BorderWidth + 30, paint);

        // Draw Week Dates
        DateTime weekEnding = weekStarting.AddDays(6);
        string dateInfo = $"Week: {weekStarting:yyyy-MM-dd} 00:00 to {weekEnding:yyyy-MM-dd} 23:55";
        canvas.DrawText(dateInfo, BorderWidth, BorderWidth + 50, paint);
    }

    private void DrawHeaders(SKCanvas canvas)
    {
        var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Left,
            TextSize = 16,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal)
        };

        // Draw time headers
        for (int timeSlot = 0; timeSlot < 288; timeSlot += 12)
        {
            TimeSpan time = TimeSpan.FromMinutes(timeSlot * 5);
            canvas.DrawText(
                time.ToString(@"hh\:mm"), 
                HeaderWidth + BorderWidth + timeSlot * SlotWidth, 
                BorderWidth + InfoHeight + 10,
                paint);
        }

        // Draw day headers
        string[] daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        for (int day = 0; day < daysOfWeek.Length; day++)
        {
            canvas.DrawText(
                daysOfWeek[day], 
                BorderWidth,
                HeaderHeight + BorderWidth + InfoHeight + 15 + day * SlotHeight, 
                paint);
        }
    }

    private void DrawGridLines(SKCanvas canvas, int columns, int rows)
    {
        var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        // Draw vertical grid lines
        for (int col = 0; col <= columns; col++)
        {
            int x = HeaderWidth + BorderWidth + col * SlotWidth;
            canvas.DrawLine(
                x, 
                HeaderHeight + BorderWidth + InfoHeight, 
                x, 
                HeaderHeight + BorderWidth + InfoHeight + rows * SlotHeight,
                paint
            );
        }

        // Draw horizontal grid lines
        for (int row = 0; row <= rows; row++)
        {
            int y = HeaderHeight + BorderWidth + InfoHeight + row * SlotHeight;
            canvas.DrawLine(
                HeaderWidth + BorderWidth, 
                y, 
                HeaderWidth + BorderWidth + columns * SlotWidth, 
                y, 
                paint
            );
        }
    }
    
    private void DrawPlayerStatusCells(SKCanvas canvas, PlayerStatusData playerStatus, int rows)
    {
        var statusData = playerStatus.StatusData; 

        for (int day = 0; day < rows; day++)
        {
            string dayName = Enum.GetName(typeof(DayOfWeek), (DayOfWeek)(((int)DayOfWeek.Monday + day) % 7)).ToLower();
            if (statusData.TryGetValue(dayName, out var dayData))
            {
                var daySlots = dayData;
                    
                foreach (var slot in daySlots)
                {
                    int timeSlotIndex = GetTimeSlotIndex(slot.Key);
                    SKColor color = GetColorForStatus(slot.Value.OnlineStatus);
                    SKRect cellRect = new SKRect(
                        HeaderWidth + BorderWidth + timeSlotIndex * SlotWidth, 
                        HeaderHeight + BorderWidth + InfoHeight + day * SlotHeight, 
                        HeaderWidth + BorderWidth + (timeSlotIndex + 1) * SlotWidth, 
                        HeaderHeight + BorderWidth + InfoHeight + (day + 1) * SlotHeight
                    );
                    var paint = new SKPaint { Color = color, Style = SKPaintStyle.Fill };
                    canvas.DrawRect(cellRect, paint);
                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = SKColors.Black;
                    canvas.DrawRect(cellRect, paint); // Draw border
                }
            }
        }
    }

    private Dictionary<DateTime, int> GenerateHeatMapData(List<PlayerStatusData> statuses, DateTime startTime, int rows)
    {
        var intervals = new Dictionary<DateTime, int>();

        for (var time = startTime; time < startTime.AddDays(rows); time = time.AddMinutes(5))
        {
            intervals[time] = -1;
        }

        foreach (var status in statuses)
        {
            var statusData = status.StatusData; 

            for (int day = 0; day < rows; day++)
            {
                string dayName = Enum.GetName(typeof(DayOfWeek), (DayOfWeek)(((int)DayOfWeek.Monday + day) % 7)).ToLower();
                if (statusData.TryGetValue(dayName, out var dayData))
                {
                    var daySlots = dayData;
                    
                    foreach (var slot in daySlots)
                    {
                        string[] time = slot.Key.Split(':');
                        int hours = int.Parse(time[0]);
                        int minutes = int.Parse(time[1]);
                        
                        DateTime interval = startTime.AddDays(day).AddHours(hours).AddMinutes((minutes / 5) * 5);

                        if (slot.Value.OnlineStatus == (byte)TornBot.Entities.TornPlayer.PlayerOnlineStatus.Online)
                        {
                            if (intervals[interval] == -1)
                                intervals[interval] = 1;
                            else
                                intervals[interval]++;
                        }
                        else if (intervals[interval] == -1)
                        {
                            // We must have some valid data for that timeslot, so mark it as 0 people online
                            intervals[interval] = 0;
                        }
                        
                    }
                }
            }

        }

        return intervals;
    }

    private void DrawHeatmapCells(SKCanvas canvas, Dictionary<DateTime, int> heatmapData, int maxOnlinePlayers)
    {
        DateTime startDate = heatmapData.Keys.Min();
        foreach (var kvp in heatmapData)
        {
            DateTime time = kvp.Key;
            int onlineCount = kvp.Value;

            int dayIndex = (int)(time - startDate).TotalDays % 7;
            int timeSlotIndex = (time.Hour * 12) + (time.Minute / 5);

            SKColor color = GetHeatmapColor(onlineCount, maxOnlinePlayers);
            SKRect cellRect = new SKRect(
                HeaderWidth + BorderWidth + timeSlotIndex * SlotWidth, 
                HeaderHeight + BorderWidth + InfoHeight + dayIndex * SlotHeight, 
                HeaderWidth + BorderWidth + (timeSlotIndex + 1) * SlotWidth, 
                HeaderHeight + BorderWidth + InfoHeight + (dayIndex + 1) * SlotHeight
            );

            var paint = new SKPaint { Color = color, Style = SKPaintStyle.Fill };
            canvas.DrawRect(cellRect, paint);
            paint.Style = SKPaintStyle.Stroke;
            paint.Color = SKColors.Black;
            canvas.DrawRect(cellRect, paint); // Draw border
        }
    }

    private SKColor GetHeatmapColor(int onlineCount, int maxOnlinePlayers)
    {
        // We have no data for this point
        if (onlineCount == -1)
            return SKColors.White;
        
        if (onlineCount == 0)
            return new SKColor(0, 255, 0);

        float ratio = (float)onlineCount / maxOnlinePlayers;
        int red = (int)(255 * ratio);
        int green = (int)(255 * (1 - ratio));

        return new SKColor((byte)red, (byte)green, 0);
    }

    private int GetTimeSlotIndex(string time)
    {
        TimeSpan timeOfDay = TimeSpan.Parse(time);
        return (timeOfDay.Hours * 12) + (timeOfDay.Minutes / 5);
    }

    private SKColor GetColorForStatus(byte status)
    {
        return status switch
        {
            1 => SKColors.Tomato,
            2 => SKColors.Orange,
            3 => SKColors.Green,
            _ => SKColors.Gray
        };
    }

    private void DrawBorders(SKCanvas canvas, int width, int height)
    {
        var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        // Draw outer border
        canvas.DrawRect(0, 0, width - 1, height - 1, paint);
    }
}

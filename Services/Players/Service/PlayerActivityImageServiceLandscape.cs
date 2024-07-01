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

using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json.Linq;
using TornBot.Services.Players.Database.Entities;

namespace TornBot.Services.Players.Service;

public class PlayerActivityImageServiceLandscape : IPlayerActivityImageService
{
    private static readonly int SlotWidth = 5;
    private static readonly int SlotHeight = 20;
    private static readonly int HeaderHeight = 20;
    private static readonly int HeaderWidth = 100;
    private static readonly int BorderWidth = 20; // Width of the outer border
    private static readonly int InfoHeight = 50; // Height for player info and week dates

    public Stream GenerateStatusImage(PlayerStatus playerStatus)
    {
        int rows = 7; // Days in a week
        int columns = 288; // 24 hours * 12 slots per hour (5-minute intervals)
        int width = columns * SlotWidth + HeaderWidth + BorderWidth * 2;
        int height = rows * SlotHeight + HeaderHeight + BorderWidth * 2 + InfoHeight;
        Bitmap bitmap = new Bitmap(width, height);

        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.White); // Background color
                
            DrawPlayerInfo(graphics, playerStatus);
            DrawHeaders(graphics);
            DrawGridLines(graphics, columns, rows);
            DrawStatusCells(graphics, playerStatus, columns, rows);
            DrawBorders(graphics, width, height);
        }

        MemoryStream memoryStream = new MemoryStream();
        bitmap.Save(memoryStream, ImageFormat.Png);
        memoryStream.Position = 0; // Reset stream position for reading
        return memoryStream;
    }
    
    private void DrawPlayerInfo(Graphics graphics, PlayerStatus playerStatus)
    {
        // Draw Player ID
        graphics.DrawString($"Player ID: {playerStatus.PlayerId}", new Font("Arial", 14), Brushes.Black, new PointF(BorderWidth, BorderWidth));

        // Draw Week Dates
        DateTime weekStarting = playerStatus.WeekStarting;
        DateTime weekEnding = weekStarting.AddDays(6);
        graphics.DrawString($"Week: {weekStarting:yyyy-MM-dd} 00:00 to {weekEnding:yyyy-MM-dd} 23:55", new Font("Arial", 14), Brushes.Black, new PointF(BorderWidth, BorderWidth + 20));
    }
    
    private void DrawHeaders(Graphics graphics)
    {
        // Draw time headers
        for (int timeSlot = 0; timeSlot < 288; timeSlot += 12)
        {
            TimeSpan time = TimeSpan.FromMinutes(timeSlot * 5);
            graphics.DrawString(time.ToString(@"hh\:mm"), new Font("Arial", 10), Brushes.Black, new PointF(HeaderWidth + BorderWidth + timeSlot * SlotWidth, BorderWidth + InfoHeight));
        }

        // Draw day headers
        string[] daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        for (int day = 0; day < daysOfWeek.Length; day++)
        {
            graphics.DrawString(daysOfWeek[day], new Font("Arial", 10), Brushes.Black, new PointF(BorderWidth, HeaderHeight + BorderWidth + InfoHeight + day * SlotHeight));
        }
    }

    private void DrawGridLines(Graphics graphics, int columns, int rows)
    {
        // Draw vertical grid lines
        for (int col = 0; col <= columns; col++)
        {
            int x = HeaderWidth + BorderWidth + col * SlotWidth;
            graphics.DrawLine(Pens.Black, x, HeaderHeight + BorderWidth + InfoHeight, x, HeaderHeight + BorderWidth + InfoHeight + rows * SlotHeight);
        }

        // Draw horizontal grid lines
        for (int row = 0; row <= rows; row++)
        {
            int y = HeaderHeight + BorderWidth + InfoHeight + row * SlotHeight;
            graphics.DrawLine(Pens.Black, HeaderWidth + BorderWidth, y, HeaderWidth + BorderWidth + columns * SlotWidth, y);
        }
    }

    private void DrawStatusCells(Graphics graphics, PlayerStatus playerStatus, int columns, int rows)
    {
        var statusData = JObject.Parse(playerStatus.StatusLog);

        for (int day = 0; day < rows; day++)
        {
            string dayName = Enum.GetName(typeof(DayOfWeek), (DayOfWeek)(((int)DayOfWeek.Monday + day) % 7)).ToLower();
            if (statusData.TryGetValue(dayName, out var dayData))
            {
                var daySlots = (JObject)dayData;

                foreach (var slot in daySlots.Properties())
                {
                    int timeSlotIndex = GetTimeSlotIndex(slot.Name);
                    Color color = GetColorForStatus(slot.Value);
                    Rectangle cellRect = new Rectangle(HeaderWidth + BorderWidth + timeSlotIndex * SlotWidth, HeaderHeight + BorderWidth + InfoHeight + day * SlotHeight, SlotWidth, SlotHeight);
                    graphics.FillRectangle(new SolidBrush(color), cellRect);
                    graphics.DrawRectangle(Pens.Black, cellRect); // Draw border
                }
            }
        }
    }

    private int GetTimeSlotIndex(string time)
    {
        TimeSpan timeOfDay = TimeSpan.Parse(time);
        return (timeOfDay.Hours * 12) + (timeOfDay.Minutes / 5);
    }

    private Color GetColorForStatus(JToken status)
    {
        int onlineStatus = status["OnlineStatus"].Value<int>();

        return onlineStatus switch
        {
            1 => Color.Tomato,
            2 => Color.Orange,
            3 => Color.Green,
            _ => Color.Gray
        };
    }

    private void DrawBorders(Graphics graphics, int width, int height)
    {
        // Draw outer border
        graphics.DrawRectangle(Pens.Black, 0, 0, width - 1, height - 1);
    }
}

﻿//
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

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace TornBot
{
    internal class Config
    {
        /// <summary>
        /// Your bot's token.
        /// </summary>
        [JsonProperty("Token")]
        internal string Token = "Discord Bot Token";

        /// <summary>
        /// Your bot's prefix
        /// </summary>
        [JsonProperty("Prefix")]
        internal string Prefix = "!";

        /// <summary>
        /// Your Discord test guild
        /// </summary>
        [JsonProperty("TestGuild")]
        internal string TestGuild = "Guild ID";


        // <sumary>
        // Torn API key
        // </summary>
        [JsonProperty("TornApiKey")]
        internal string TornApiKey = "Torn API Key";


        // <sumary>
        // Torn API key
        // </summary>
        [JsonProperty("TornstatsApiKey")]
        internal string TornstatsApiKey = "TornStats API";

        [JsonProperty("StocksChannelId")]
        internal string StocksChannelId = "Channel ID";

        /// <summary>
        /// Loads config from a JSON file.
        /// </summary>
        /// <param name="path">Path to your config file.</param>
        /// <returns></returns>
        public static Config LoadFromFile(string path)
        {
            using (var sr = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            }
        }

        /// <summary>
        /// Saves config to a JSON file.
        /// </summary>
        /// <param name="path"></param>
        public void SaveToFile(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                sw.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }

        public static IConfigurationRoot LoadConfig()
        {
            if (!File.Exists("config.json"))
            {
                new Config().SaveToFile("config.json");

                PrintHeader();

                Console.BackgroundColor = ConsoleColor.Yellow;
                WriteCenter("WARNING", 3);
                Console.ResetColor();
                WriteCenter("Please fill in the config.json that was generated.", 1);
                WriteCenter("Press any key to exit..", 1);
                Console.SetCursorPosition(0, 0);
                Console.ReadKey();

                //Die
                Environment.Exit(0);
            }

            var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("config.json")
            .Build();

            return config;
        }

        private static void PrintHeader()
        {
            //Tell the user to fill out the new config.json
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;

            WriteCenter("  ______                 ____        __ ", 2);
            WriteCenter(" /_  __/___  _________  / __ )____  / /_");
            WriteCenter("  / / / __ \\/ ___/ __ \\/ __  / __ \\/ __/");
            WriteCenter(" / / / /_/ / /  / / / / /_/ / /_/ / /_  ");
            WriteCenter("/_/  \\____/_/  /_/ /_/_____/\\____/\\__/  ");
            WriteCenter("                                        ");
        }

        private static void WriteCenter(string value, int skipline = 0)
        {
            for (int i = 0; i < skipline; i++)
                Console.WriteLine();

            Console.SetCursorPosition((Console.WindowWidth - value.Length) / 2, Console.CursorTop);
            Console.WriteLine(value);
        }

    }
}

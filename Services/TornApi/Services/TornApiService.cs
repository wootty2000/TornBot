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

using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TornBot.Exceptions;
using TornBot.Services.Database;
using TornBot.Services.Database.Entities;

namespace TornBot.Services.TornApi.Services
{
    public class TornApiService
    {
        private string baseUrl = "https://api.torn.com/";

        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _serviceProvider;

        private const byte AccessLevelPublic   = 0b00000001;
        private const byte AccessLevelMinimal  = 0b00000010;
        private const byte AccessLevelLimited  = 0b00000100;
        private const byte AccessLevelFull     = 0b00001000;
        private const byte AccessLevelFaction  = 0b00010000;
        private const byte AccessLevelOutsider = 0b10000000;

        // These are used when storing what access a key can provide
        // eg, a Minimal level key also allows for Public key endpoints
        private enum KeyAccessLevel
        {
            /* 0b00000001 */ Public = AccessLevelPublic,
            /* 0b00000011 */ Minimal = AccessLevelMinimal | AccessLevelPublic ,           
            /* 0b00000111 */ Limited = AccessLevelLimited | AccessLevelMinimal | AccessLevelPublic,
            /* 0b00001111 */ Full = AccessLevelFull | AccessLevelLimited | AccessLevelMinimal | AccessLevelPublic, 
            /* 0b00000001 */ FactionPublic = AccessLevelPublic, //Faction public does not need a faction enabled key
            /* 0b00010011 */ FactionMinimal = AccessLevelFaction | AccessLevelMinimal | AccessLevelPublic,
            /* 0b00010111 */ FactionLimited = AccessLevelFaction | AccessLevelLimited | AccessLevelMinimal | AccessLevelPublic,   
            /* 0b00011111 */ FactionFull =    AccessLevelFaction | AccessLevelFull | AccessLevelLimited | AccessLevelMinimal | AccessLevelPublic, // Nothing needs faction full
            /* 0b10000001 */ OutsiderPublic = AccessLevelOutsider | AccessLevelPublic,
            /* 0b10000011 */ OutsiderMinimal = AccessLevelOutsider | AccessLevelMinimal | AccessLevelPublic,
            /* 0b10000111 */ OutsiderLimited = AccessLevelOutsider | AccessLevelLimited | AccessLevelMinimal | AccessLevelPublic,
            /* 0b10001111 */ OutsiderFull = AccessLevelOutsider | AccessLevelFull | AccessLevelLimited | AccessLevelMinimal | AccessLevelPublic
        }

        // These are the access levels that calling modules want to use
        public enum AccessLevel
        {
            /* 0b00000001 */ Public = AccessLevelPublic, 
            /* 0b00000010 */ Minimal = AccessLevelMinimal,                               
            /* 0b00000100 */ Limited = AccessLevelLimited,
            /* 0b00001000 */ Full = AccessLevelFull,
            /* 0b00000001 */ FactionPublic = AccessLevelPublic, // Faction public does not need a faction enabled key
            /* 0b00010010 */ FactionMinimal = AccessLevelFaction | AccessLevelMinimal,
            /* 0b00010100 */ FactionLimited = AccessLevelFaction | AccessLevelLimited,
            /* 0b00011000 */ FactionFull = AccessLevelFaction | AccessLevelFull, // Nothing needs faction full
            /* 0b10000001 */ OutsiderPublic = AccessLevelOutsider | AccessLevelPublic,
            /* 0b10000010 */ OutsiderMinimal = AccessLevelOutsider | AccessLevelMinimal,
            /* 0b10000100 */ OutsiderLimited = AccessLevelOutsider | AccessLevelLimited,
            /* 0b10001000 */ OutsiderFull = AccessLevelOutsider | AccessLevelFull
        }

        
        
        public TornApiService(IConfigurationRoot config, IServiceProvider serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }

        
        /// <summary>
        /// Adds the supplied Api key to the database
        /// 2 API calls are made to Torn.
        /// The first checks what permissions the key has and the second to see who owns it
        /// </summary>
        /// <param name="apiKey">Api Key to use</param>
        /// <returns>void</returns>
        /// <exception cref="ApiCallFailureException">The inner Exception should contain further information</exception>
        /// <exception cref="UnknownException"></exception>
        public void AddApiKey(string apiKey)
        {
            DatabaseContext database = _serviceProvider.GetRequiredService<DatabaseContext>(); 
            
            try
            {
                TornBot.Entities.KeyInfo apiKeyInfo = GetApiKeyInfo(apiKey);
                TornBot.Entities.TornPlayer tornPlayer = GetPlayer(0, apiKey);

                TornBot.Services.Database.Entities.TornPlayer? dbTornPlayer =
                    database.TornPlayers.FirstOrDefault(s => s.Id == tornPlayer.Id);

                if (dbTornPlayer != null)
                {
                    dbTornPlayer.ParseTornPlayer(tornPlayer);
                    database.TornPlayers.Update(dbTornPlayer);
                    database.SaveChanges();
                    //TODO - log updated TornPlayer via AddApiKey
                }
                else
                {
                    database.TornPlayers.Add(new TornPlayer(tornPlayer));
                    database.SaveChanges();
                    //TODO - log added new TornPlayer via AddApiKey
                }

                //add api key with info to database
                UInt32 homeFactionId = _config.GetValue<UInt32>("TornFactionId"); //get faction id

                KeyAccessLevel keyAccessLevel = apiKeyInfo.TornAccessLevel switch
                {
                    1 => KeyAccessLevel.Public,
                    2 => KeyAccessLevel.Minimal,
                    3 => KeyAccessLevel.Limited,
                    4 => KeyAccessLevel.Full,
                    _ => throw new ApiKeyAccessLevelNotSupportedException(
                        "The supplied key access level is not supported")
                };

                //Factions -> Applications needs a minimum of Faction enabled Minimal key
                if (apiKeyInfo.SelectionFaction.Contains("applications"))
                {
                    keyAccessLevel = (KeyAccessLevel)(AccessLevelFaction | (byte)keyAccessLevel);
                }

                if (tornPlayer.Faction.Id != homeFactionId) //it is outside api key
                {
                    keyAccessLevel = (KeyAccessLevel)(AccessLevelOutsider | (byte)keyAccessLevel);
                }

                TornBot.Services.Database.Entities.ApiKeys? dbApiKeys =
                    database.ApiKeys.Where(s => s.PlayerId == tornPlayer.Id).FirstOrDefault();
                bool newApiKey = false;

                if (dbApiKeys == null) //add new api key
                {
                    dbApiKeys = new TornBot.Services.Database.Entities.ApiKeys();
                    newApiKey = true;

                    dbApiKeys.PlayerId = tornPlayer.Id;
                    dbApiKeys.TornStatsApiKey = "";
                }

                dbApiKeys.FactionId = tornPlayer.Faction.Id;

                dbApiKeys.ApiKey = apiKey;
                dbApiKeys.AccessLevel = (byte)keyAccessLevel;
                dbApiKeys.TornApiAddedTimestamp = DateTime.UtcNow;
                dbApiKeys.TornLastUsed = null;

                if (newApiKey)
                {
                    database.ApiKeys.Add(dbApiKeys);
                    database.SaveChanges();
                    //TODO - log new key added
                }
                else
                {
                    database.ApiKeys.Update(dbApiKeys);
                    database.SaveChanges();
                    //TODO - log key updated
                }
            }
            catch (ApiCallFailureException e)
            {
                throw;
            }
            catch (Exception e)
            {
                //TODO - Log something bad happened
                throw new UnknownException("Something went wrong adding api key to the system", e);
            }
        }
        
        /// <summary>
        /// Attempts to get the next Api key for the required Access Level
        /// </summary>
        /// <param name="accessLevel">Minimum required access level, based on AccessLevel enum</param>
        /// <returns>string</returns>
        /// <exception cref="NoMoreKeysAvailableException"></exception>
        /// <exception cref="UnknownException">Unknown error occured. Check the inner exception</exception>
        public string GetNextKey(AccessLevel accessLevel)
        {
            DatabaseContext dbContext = _serviceProvider.GetRequiredService<DatabaseContext>();
            TornBot.Services.Database.Entities.ApiKeys? dbApiKeys;

            try
            {
                //If we are not asked for an outsider key, do not offer outsider keys
                if(((byte)accessLevel & 0b1000000) == 0)
                {
                    dbApiKeys = dbContext.ApiKeys //get the api key that hasnt been used for the longest
                        .Where(s => 
                            s.ApiKey != "" &&
                            ((s.AccessLevel & (byte)accessLevel) == (byte)accessLevel) && 
                            ((s.AccessLevel & 0b10000000) == 0 )
                        )
                        .OrderBy(s => s.TornLastUsed)
                        .FirstOrDefault();
                }
                else
                {
                    dbApiKeys = dbContext.ApiKeys //get the api key that hasnt been used for the longest
                        .Where(s => (s.AccessLevel & (byte)accessLevel) == (byte)accessLevel)
                        .OrderBy(s => s.TornLastUsed)
                        .FirstOrDefault();
                }
                
                if (dbApiKeys != null)
                {
                    Console.WriteLine("Db api key used: " + dbApiKeys.ApiKey);

                    dbApiKeys.TornLastUsed = DateTime.UtcNow;
                    
                    dbContext.ApiKeys.Update(dbApiKeys); //updates LastUsed 
                    dbContext.SaveChanges();
                    
                    return dbApiKeys.ApiKey;
                }
                else
                {
                    // TODO - Log we ran out of keys
                    throw new NoMoreKeysAvailableException("No valid key found");
                }
            }
            catch (NoMoreKeysAvailableException e)
            {
                throw;
            }
            catch (Exception e)
            {
                // TODO - You can log the exception or handle it as needed
                throw new UnknownException("Error fetching TornStats API key from DB", e);
            }
        }

        /// <summary>
        /// Makes the actual GET request to Torn's API
        /// </summary>
        /// <param name="endpoint">String of the endpoint including all parameters (inc Api Key)</param>
        /// <returns>String - the raw response from the API</returns>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        private string MakeApiRequest(string endpoint)
        {
            HttpClient client = new HttpClient();
            string url = baseUrl + endpoint;

            JsonElement codeElement;
            try
            {
                string response = client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;

                //Try to XML serialise the response string and use it to check for error status / error codes
                JsonElement jsonResponse = JsonSerializer.Deserialize<dynamic>(response);
                
                //If there is no 'error' property in the XML, return the original response string
                if (!jsonResponse.TryGetProperty("error", out JsonElement errorElement)) 
                    return response;
                
                //If we got here, there was an error property.
                //Check for code. If it's not provider, or it is but it's not a number, return the original string
                if (!errorElement.TryGetProperty("code", out codeElement) || codeElement.ValueKind != JsonValueKind.Number) 
                    return response;
            }
            catch (Exception e)
            {
                //TODO - log this properly. We probably want the ApiKey to be passed in for logging who's key went wrong
                Console.WriteLine("Error in rest call to Torn: " + e.Message);
                throw new ApiCallFailureException("Error is rest call to Torn", e);
            }    
            
            //If we got here, there was an error code from Torn's API. Return the most appropriate exception
            throw codeElement.GetInt16() switch
            {
                2 => new ApiCallFailureException("Supplied API key is invalid", new InvalidKeyException()),
                8 => new ApiCallFailureException("Server IP has been temporarily blocked", new IPBlockedException()),
                9 => new ApiCallFailureException("Torn API is currently unavailable", new ApiNotAvailableException()),
                13 => new ApiCallFailureException("Supplied API key has been suspended due to owner inactivity", new ApiKeyOwnerInactiveException()),
                _ => new ApiCallFailureException("Unknown Torn API Error", new UnknownException(String.Format("Torn API Error: {0}", codeElement.GetInt16().ToString())))
            };
            
        }

        /// <summary>
        /// Attempt to get the Player's information from Torn's API
        /// Will use the next available API key from the key store
        /// </summary>
        /// <param name="id">Torn Player id</param>
        /// <returns>TornBot.Entities.TornPlayer object</returns>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornBot.Entities.TornPlayer GetPlayer(UInt32 id)
        {
            //Loop until we get a valid response or run out of API keys or a other API call failure
            while (true)
            {
                string key;

                try
                {
                    key = GetNextKey(AccessLevel.Public);

                    return GetPlayer(id, key);
                }
                catch (ApiCallFailureException e)
                {
                    if (e.InnerException is not null)
                    {
                        if (e.InnerException is InvalidKeyException)
                        {
                            //TODO mark the key as invalid in the key store
                            continue;
                        } 
                        else if (e.InnerException is ApiKeyOwnerInactiveException)
                        {
                            //TODO mark the key as owner inactive in the key store
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new ApiCallFailureException("Error in Torn API GetPlayer(UInt32 id)", e);
                }
            }
        }

        /// <summary>
        /// Attempts to gets the Faction's general information from Torn's API
        /// </summary>
        /// <param name="id">Torn Player id</param>
        /// <param name="apiKey">Api Key to use</param>
        /// <returns>TornBot.Entities.TornPlayer object of the player</returns>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornBot.Entities.TornPlayer GetPlayer(UInt32 id, string apiKey)
        {
            string url = String.Format("user/{0}?key={1}", id.ToString(), apiKey);

            try
            {
                string apiResponse = MakeApiRequest(url);

                TornApi.Entities.User user = JsonSerializer.Deserialize<TornApi.Entities.User>(apiResponse);
                TornBot.Entities.TornPlayer tornPlayer = user.ToTornPlayer();

                return tornPlayer;
            }
            catch (ApiCallFailureException)
            {
                throw;
            }
            catch (Exception e)
            {
                //TODO log this correctly
                throw new ApiCallFailureException("Error deserializing Torn player data", e);
            }
        }

        /// <summary>
        /// Attempts to gets the Faction's general information from Torn's API
        /// Will use the next available API key from the key store
        /// </summary>
        /// <param name="id">Torn Faction id</param>
        /// <returns>TornApi.Entities.Faction object of the faction</returns>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornApi.Entities.Faction GetFaction(UInt32 id)
        {
            //Loop until we get a valid response or run out of API keys or a other API call failure
            while (true)
            {
                string key;
                
                try
                {
                    key = GetNextKey(AccessLevel.FactionPublic);
                    
                    return GetFaction(id, key);
                }
                catch (ApiCallFailureException e)
                {
                    if (e.InnerException is not null)
                    {
                        if (e.InnerException is InvalidKeyException)
                        {
                            //TODO mark the key as invalid in the key store
                            continue;
                        } 
                        else if (e.InnerException is ApiKeyOwnerInactiveException)
                        {
                            //TODO mark the key as owner inactive in the key store
                            continue;
                        }
                        else
                        {
                            throw;
                        }

                    }
                }
                catch (Exception e)
                {
                    throw new ApiCallFailureException("Error in Torn API GetPlayer(UInt32 id)", e);
                }
            }
        }

        /// <summary>
        /// Attempts to gets the Faction's general information from Torn's API
        /// </summary>
        /// <param name="id">Torn Player id</param>
        /// <param name="apiKey">Api Key to use</param>
        /// <returns>TornApi.Entities.Faction object of the faction</returns>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornApi.Entities.Faction GetFaction(UInt32 id, string key)
        {
            string url = String.Format("faction/{0}?selections=&key={1}", id.ToString(), key);

            try
            {    
                string apiResponse = MakeApiRequest(url);

                TornApi.Entities.Faction faction = JsonSerializer.Deserialize<TornApi.Entities.Faction>(apiResponse);
                return faction;
            }
            catch (ApiCallFailureException)
            {
                throw;
            }
            catch (Exception e)
            {
                //TODO log this correctly
                throw new ApiCallFailureException("Error deserializing torn player data", e);
            }
        }

        /// <summary>
        /// Attempts to get Stock data
        /// </summary>
        /// <returns>TornApi.Entities.StockResponse</returns>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornApi.Entities.StockResponse GetStocks()
        {
            //Loop until we get a valid response or run out of API keys or a other API call failure
            while (true)
            {
                string key;
                
                try
                {
                    key = GetNextKey(AccessLevel.Public);
                    
                    return GetStocks(key);
                }
                catch (ApiCallFailureException e)
                {
                    if (e.InnerException is not null)
                    {
                        if (e.InnerException is InvalidKeyException)
                        {
                            //TODO mark the key as invalid in the key store
                            continue;
                        } 
                        else if (e.InnerException is ApiKeyOwnerInactiveException)
                        {
                            //TODO mark the key as owner inactive in the key store
                            continue;
                        }
                        else
                        {
                            throw;
                        }

                    }
                }
                catch (Exception e)
                {
                    throw new ApiCallFailureException("Error in Torn API GetPlayer(UInt32 id)", e);
                }
            }
        }
        
        /// <summary>
        /// Attempts to get Stock data
        /// </summary>
        /// <param name="apiKey">Api Key to use</param>
        /// <returns>TornApi.Entities.StockResponse</returns>
        /// <exception cref="ApiCallFailureException"></exception>
        public TornApi.Entities.StockResponse GetStocks(string apiKey)
        {
            string url = String.Format("torn/?selections=stocks&key={0}", apiKey);

            try
            {        
                string apiResponse = MakeApiRequest(url);

                TornApi.Entities.StockResponse stocks = JsonSerializer.Deserialize<TornApi.Entities.StockResponse>(apiResponse);
                return stocks;
            }
            catch (ApiCallFailureException)
            {
                throw;
            }
            catch (Exception e)
            {
                //TODO log this correctly
                throw new ApiCallFailureException("Error deserializing torn player data", e);
            }
        }

        /// <summary>
        /// Attempts to get revive status of a player
        /// </summary>
        /// <param name="id">Torn Player id</param>
        /// <returns>TornBot.Entities.ReviveStatus</returns>
        /// <exception cref="ApiNotAvailableException">API system is currently unavailable</exception>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception
        public TornBot.Entities.ReviveStatus GetReviveStatus(UInt32 id)
        {
            //Loop until we get a valid response or run out of API keys or a other API call failure
            while (true)
            {
                //We ideally want to use an external API key so it doesnt return friends/faction
                try
                {
                    string key = GetNextKey(AccessLevel.OutsiderPublic);
                    return GetReviveStatus(id, key);
                }
                catch (ApiCallFailureException e)
                {
                    if (e.InnerException is not null)
                    {
                        if (e.InnerException is InvalidKeyException)
                        {
                            //TODO mark the key as invalid in the key store
                            continue;
                        } 
                        else if (e.InnerException is ApiKeyOwnerInactiveException)
                        {
                            //TODO mark the key as owner inactive in the key store
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new ApiCallFailureException("Error in Torn API GetReviveStatus(UInt32 id)", e);
                }
            }
        }

        /// <summary>
        /// Attempts to gets the Torn player's revive status
        /// </summary>
        /// <param name="id">Torn Player id</param>
        /// <param name="apiKey">Api Key to use</param>
        /// <returns>TornBot.Entities.ReviveStatus object</returns>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornBot.Entities.ReviveStatus GetReviveStatus(UInt32 id, string apiKey)
        {
            string url = String.Format("user/{0}?key={1}", id.ToString(), apiKey);

            try
            {    
                string apiResponse = MakeApiRequest(url);
                
                TornBot.Entities.ReviveStatus reviveStatus = JsonSerializer.Deserialize<TornApi.Entities.User>(apiResponse).ToReviveStatus();
                return reviveStatus;
            }
            catch (ApiCallFailureException)
            {
                throw;
            }
            catch (Exception e)
            {
                //TODO log this correctly
                throw new ApiCallFailureException("Error deserializing revive status data", e);
            }
        }

        /// <summary>
        /// Attempts to gets the Api Key Info from the supplied Api Key
        /// </summary>
        /// <param name="apiKey">Api Key to use</param>
        /// <returns>TornBot.Entities.KeyInfo object of the Api Key Info</returns>
        /// <exception cref="ApiCallFailureException">Something went wrong and the inner exception has more details</exception>
        public TornBot.Entities.KeyInfo GetApiKeyInfo(string apiKey)
        {
            string url = String.Format("key/?selections=info&key={0}", apiKey);

            try
            {
                string apiResponse = MakeApiRequest(url);
                TornApi.Entities.Key key = JsonSerializer.Deserialize<TornApi.Entities.Key>(apiResponse);
                
                return key.ToKeyInfo(apiKey);
            }
            catch (ApiCallFailureException)
            {
                throw;
            }
            catch (Exception e)
            {
                //TODO log this correctly
                throw new ApiCallFailureException("Error deserializing api Key info data", e);
            }
        }
    }
}

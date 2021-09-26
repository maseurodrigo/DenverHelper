using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using DenverHelper.Data;
using DenverHelper.Data.Functions;
using DenverHelper.Data.MySQL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DenverHelper.Modules
{
    [Summary("Soccer Discord Commands")]
    public class SoccerComms : InteractiveBase
    {
        // Getting all services through constructor param with AddSingleton()
        private BotData botData { get; }
        private static readonly Color embedsColor = new Color(139, 195, 74);
        private SoccerComms(BotData _BotData) => this.botData = _BotData;

        [Command("team")]
        [Alias("squad")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get all team members from a particular soccer team")]
        public async Task getTeamMembers([Remainder][Summary("Team name")] String _teamName) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedsColor);
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLAPIKeys.checkAPIKeyExists(conn, Context.Guild.Id)) {
                // Get JSON data of the given team from API
                String APIKey = await MySQLAPIKeys.getAPIKey(conn, Context.Guild.Id);
                JObject jsonData_Team = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPISoccerTeams(APIKey, 4, null, _teamName.Trim().ToLower(), null, null, null));
                try {
                    // Store team data (id, name and logo)
                    JToken teamID = jsonData_Team["api"]["teams"][0]["team_id"];
                    JToken teamName = jsonData_Team["api"]["teams"][0]["name"];
                    JToken teamLogo = jsonData_Team["api"]["teams"][0]["logo"];
                    JObject jsonData_Squad = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPISoccerTeams(APIKey, 1, (int)teamID, null, null, null, DateTime.Now.Year));
                    // Fill all variables within JSON data
                    StringBuilder allGoalkeepers = new StringBuilder(),
                        allDefenders = new StringBuilder(),
                        allMidfielders = new StringBuilder(),
                        allAttackers = new StringBuilder();
                    // Loop through all team members
                    foreach (var player in jsonData_Squad["api"]["players"]) {
                        JToken playerName = ((JObject)player)["player_name"];
                        JToken playerPosition = ((JObject)player)["position"];
                        // Fill sb's within players positions
                        switch ((String)playerPosition) {
                            case "Goalkeeper":
                                allGoalkeepers.AppendLine((String)playerName);
                                break;
                            case "Defender":
                                allDefenders.AppendLine((String)playerName);
                                break;
                            case "Midfielder":
                                allMidfielders.AppendLine((String)playerName);
                                break;
                            case "Attacker":
                                allAttackers.AppendLine((String)playerName);
                                break;
                        }
                    }
                    // Build out the reply
                    replyEmbed.Title = (String)teamName;
                    replyEmbed.ThumbnailUrl = (String)teamLogo;
                    replyEmbed.AddField("🙌​​​​ Goalkeepers", allGoalkeepers.ToString(), true);
                    replyEmbed.AddField("​🦶​​ Defenders", allDefenders.ToString(), true);
                    replyEmbed.AddField("​🔥​​ ​Midfielders", allMidfielders.ToString(), true);
                    replyEmbed.AddField("​👟​​​ Strikers", allAttackers.ToString(), true);
                } catch (NullReferenceException) {
                    replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key";
                } catch (WebException) {
                    replyEmbed.Description = "My apologies, I honestly don't know this team...";
                } catch (JsonReaderException) {
                    replyEmbed.Description = "I couldn't get results for this command";
                }
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }

        [Command("matches")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get scheduled matches for a particular soccer team")]
        public async Task getTeamGames([Summary("Next/Last match")] String _typeMatch, [Remainder][Summary("Team name")] String _teamName) {
            // Initialize empty string builder for reply
            StringBuilder strBuilder = new StringBuilder();
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedsColor);
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLAPIKeys.checkAPIKeyExists(conn, Context.Guild.Id)) {
                // Get JSON data of the given team next matches from API
                String APIKey = await MySQLAPIKeys.getAPIKey(conn, Context.Guild.Id);
                JObject jsonData_Team = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPISoccerTeams(APIKey, 4, null, _teamName.Trim().ToLower(), null, null, null));
                try {
                    List<String> typeOptions = new List<String>() { "NEXT", "LAST" };
                    // Checking if "typeMatch" param its valid
                    if (typeOptions.Contains(_typeMatch.ToUpper())) {
                        // Waiting for an input of a valid numeric value
                        await ReplyAsync("Number of matches ?");
                        int countMatches;
                        SocketMessage response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));
                        if (response != null) {
                            if (int.TryParse(response.Content.Trim(), out countMatches)) {
                                if(countMatches > 25) {
                                    // Invalid emoji reaction
                                    await response.AddReactionAsync(new Emoji("👎"));
                                    replyEmbed.Description = "Invalid numeric value";
                                } else {
                                    // Valid emoji reaction
                                    await response.AddReactionAsync(new Emoji("👍"));
                                    // Trigger typing state on current channel
                                    await Context.Channel.TriggerTypingAsync();
                                    // Store team data (id, name and logo)
                                    JToken teamID = jsonData_Team["api"]["teams"][0]["team_id"];
                                    JToken teamName = jsonData_Team["api"]["teams"][0]["name"];
                                    JToken teamLogo = jsonData_Team["api"]["teams"][0]["logo"];
                                    if (_typeMatch.ToUpper().Equals(typeOptions[0])) {
                                        // Next matches
                                        JObject jsonData_Matches = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPISoccerTeams(APIKey, 2, (int)teamID, null, countMatches, null, null));
                                        // Loop through all team matches
                                        foreach (var match in jsonData_Matches["api"]["fixtures"]) {
                                            String _tmpAgainst = ((JObject)match)["homeTeam"]["team_id"].ToString() == (String)teamID ? 
                                                ((JObject)match)["awayTeam"]["team_name"].ToString() : ((JObject)match)["homeTeam"]["team_name"].ToString();
                                            JToken competition = ((JObject)match)["league"]["name"];
                                            JToken stadium = ((JObject)match)["venue"];
                                            // Fill StringBuilders with match data
                                            strBuilder.AppendLine($"🆚{ new String(' ', 2) }**{ _tmpAgainst }** on { (String.IsNullOrWhiteSpace((String)stadium) ? String.Concat("-") : (String)stadium) } for { (String)competition }");
                                            strBuilder.AppendLine();
                                        }
                                        // Build out the reply
                                        replyEmbed.Title = $"Next matches of { (String)teamName }";
                                        replyEmbed.ThumbnailUrl = (String)teamLogo;
                                        replyEmbed.Description = strBuilder.ToString();
                                    } else {
                                        // Last matches
                                        JObject jsonData_Matches = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPISoccerTeams(APIKey, 5, (int)teamID, null, countMatches, null, null));
                                        // Loop through all team matches
                                        foreach (var match in jsonData_Matches["api"]["fixtures"]) {
                                            KeyValuePair<bool, String> _tmpAgainst = ((JObject)match)["homeTeam"]["team_id"].ToString() == (String)teamID ?
                                                            new KeyValuePair<bool, String>(true, ((JObject)match)["awayTeam"]["team_name"].ToString()) : 
                                                            new KeyValuePair<bool, String>(false, ((JObject)match)["homeTeam"]["team_name"].ToString());
                                            JToken competition = ((JObject)match)["league"]["name"];
                                            JToken goalsHomeT = ((JObject)match)["goalsHomeTeam"];
                                            JToken goalsAwayT = ((JObject)match)["goalsAwayTeam"];
                                            JToken fullTimeResult = ((JObject)match)["score"]["fulltime"];
                                            String resultState = String.Empty;
                                            // Different results possibilites
                                            if (_tmpAgainst.Key) {
                                                if (Convert.ToInt32((String)goalsHomeT) > Convert.ToInt32((String)goalsAwayT)) resultState = "win";
                                                else if (Convert.ToInt32((String)goalsHomeT) < Convert.ToInt32((String)goalsAwayT)) resultState = "lose";
                                                else resultState = "draw";
                                            } else {
                                                if (Convert.ToInt32((String)goalsHomeT) > Convert.ToInt32((String)goalsAwayT)) resultState = "lose";
                                                else if (Convert.ToInt32((String)goalsHomeT) < Convert.ToInt32((String)goalsAwayT)) resultState = "win";
                                                else resultState = "draw";
                                            }
                                            // Fill StringBuilders with match data
                                            strBuilder.AppendLine($"🆚{ new String(' ', 2) }**{ _tmpAgainst.Value }** with a { resultState } of { (String)fullTimeResult } for { (String)competition }");
                                            strBuilder.AppendLine();
                                        }
                                        // Build out the reply
                                        replyEmbed.Title = $"Last matches of { (String)teamName }";
                                        replyEmbed.ThumbnailUrl = (String)teamLogo;
                                        replyEmbed.Description = strBuilder.ToString();
                                    }
                                }
                            } else {
                                // Invalid emoji reaction
                                await response.AddReactionAsync(new Emoji("👎"));
                                replyEmbed.Description = "Invalid numeric value";
                            }
                        } else await ReplyAsync($"{ Context.User.Mention } you didnt reply before the timeout"); // response timeout
                    } else replyEmbed.Description = "Sorry, but I need a 'next' or 'last' parameter";
                } catch(NullReferenceException) {
                    replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key";
                } catch (WebException) {
                    replyEmbed.Description = "My apologies, I honestly don't know this team...";
                } catch (JsonReaderException) {
                    replyEmbed.Description = "I couldn't get results for this command";
                }
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }

        [Command("tip")]
        [Alias("predict")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get predictions for the next scheduled matche(s) of a particular soccer team")]
        public async Task getTeamGamesPredict([Remainder][Summary("Team name")] String _teamName) {
            // Initialize empty string builder for reply
            StringBuilder strBuilder = new StringBuilder();
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedsColor);
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLAPIKeys.checkAPIKeyExists(conn, Context.Guild.Id)) {
                // Get JSON data of the given team next matches from API
                String APIKey = await MySQLAPIKeys.getAPIKey(conn, Context.Guild.Id);
                JObject jsonData_Team = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPISoccerTeams(APIKey, 4, null, _teamName.Trim().ToLower(), null, null, null));
                try {
                    // Waiting for an input of a valid numeric value
                    await ReplyAsync("Number of matches ?");
                    int countMatches;
                    SocketMessage response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));
                    if (response != null) {
                        if (int.TryParse(response.Content.Trim(), out countMatches)) {
                            if(countMatches > 25) {
                                // Invalid emoji reaction
                                await response.AddReactionAsync(new Emoji("👎"));
                                replyEmbed.Description = "Invalid numeric value";
                            } else {
                                // Valid emoji reaction
                                await response.AddReactionAsync(new Emoji("👍"));
                                // Trigger typing state on current channel
                                await Context.Channel.TriggerTypingAsync();
                                // Store team data (id, name and logo)
                                JToken teamID = jsonData_Team["api"]["teams"][0]["team_id"];
                                JToken teamName = jsonData_Team["api"]["teams"][0]["name"];
                                JToken teamLogo = jsonData_Team["api"]["teams"][0]["logo"];
                                JObject jsonData_Game = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPISoccerTeams(APIKey, 2, (int)teamID, null, countMatches, null, null));
                                // Loop through all team matches
                                foreach (var match in jsonData_Game["api"]["fixtures"]) {
                                    // Current match ID
                                    int _tmpMatchID = Convert.ToInt32(((JObject)match)["fixture_id"].ToString());
                                    // JSON data from current match
                                    JObject jsonData_Predict = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPISoccerTeams(APIKey, 3, null, String.Empty, null, _tmpMatchID, null));
                                    string _tmpAgainst = ((JObject)match)["homeTeam"]["team_id"].ToString() == (String)teamID ?
                                                    ((JObject)match)["awayTeam"]["team_name"].ToString() : ((JObject)match)["homeTeam"]["team_name"].ToString();
                                    string _tmpPredict = jsonData_Predict["api"]["predictions"][0]["advice"].ToString();
                                    // Fill StringBuilders with match data
                                    strBuilder.AppendLine($"🆚{ new String(' ', 2) }**{ _tmpAgainst }** with a prediction of **{ _tmpPredict }**");
                                    strBuilder.AppendLine();
                                }
                                // Build out the reply
                                replyEmbed.Title = $"Next matches of { (String)teamName }";
                                replyEmbed.ThumbnailUrl = (String)teamLogo;
                                replyEmbed.Description = strBuilder.ToString();
                            }
                        } else {
                            // Invalid emoji reaction
                            await response.AddReactionAsync(new Emoji("👎"));
                            replyEmbed.Description = "Invalid numeric value";
                        }
                    } else await ReplyAsync($"{ Context.User.Mention } you didnt reply before the timeout"); // response timeout
                } catch(NullReferenceException) {
                    replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key";
                } catch (WebException) {
                    replyEmbed.Description = "My apologies, I honestly don't know this team...";
                } catch (JsonReaderException) {
                    replyEmbed.Description = "I couldn't get results for this command";
                }
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }

        // Local player's data
        public class PlayerData {
            public String playerName;
            public String playerPosition;
            public String playerNation;
            public int playerAge;
            public String playerHeight;
            public String playerWeight;
            public PlayerData(String _playerName, String _playerPosition, String _playerNation,
                int _playerAge, String _playerHeight, String _playerWeight) {
                this.playerName = _playerName;
                this.playerPosition = _playerPosition;
                this.playerNation = _playerNation;
                this.playerAge = _playerAge;
                this.playerHeight = _playerHeight;
                this.playerWeight = _playerWeight;
            }
        }

        [Command("player")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get informations relating to the given soccer player")]
        public async Task getPlayerData([Remainder][Summary("Player name")] String _player) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedsColor);
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLAPIKeys.checkAPIKeyExists(conn, Context.Guild.Id)) {
                // Get JSON data of the given city from APIs
                String APIKey = await MySQLAPIKeys.getAPIKey(conn, Context.Guild.Id);
                JObject tmpPlayerData = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPISoccerPlayers(APIKey, 1, null, _player.Trim().ToLower()));
                try {
                    // Embed layout reply
                    EmbedBuilder listPlayersEmbed = new EmbedBuilder();
                    listPlayersEmbed.WithColor(embedsColor);
                    Dictionary<int, PlayerData> listPlayers = new Dictionary<int, PlayerData>();
                    // Check if the results are too many 
                    JToken totalResults = tmpPlayerData["api"]["results"];
                    if ((int)totalResults > 20) listPlayersEmbed.Description = "You need to be more specific with the player's name";
                    else {
                        // Fill all variables within JSON data
                        StringBuilder allPlayers = new StringBuilder();
                        // Loop through all players with similar names
                        foreach (var player in tmpPlayerData["api"]["players"]) {
                            JToken playerID = ((JObject)player)["player_id"];
                            JToken playerName = ((JObject)player)["player_name"];
                            JToken playerPosition = ((JObject)player)["position"];
                            JToken playerNation = ((JObject)player)["nationality"];
                            JToken playerAge = ((JObject)player)["age"];
                            JToken playerHeight = ((JObject)player)["height"];
                            JToken playerWeight = ((JObject)player)["weight"];
                            allPlayers.AppendLine($"`{ (String)playerID }`: **{ (String)playerName }** ({ (String)playerNation })");
                            listPlayers.Add((int)playerID, new PlayerData((String)playerName, (String)playerPosition, (String)playerNation,
                                (int)playerAge, (String)playerHeight, (String)playerWeight));
                        }
                        // Build out the reply
                        listPlayersEmbed.AddField($"Some { CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_player.Trim().ToLower()) }'s", allPlayers.ToString(), true);
                        // Reply with the embed
                        await ReplyAsync(null, false, listPlayersEmbed.Build());
                        int userPlayerID;
                        // Waiting for an input of a valid numeric value
                        SocketMessage response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(20));
                        if (int.TryParse(response.Content.Trim(), out userPlayerID)) {
                            if (listPlayers.ContainsKey(userPlayerID)) {
                                // Get all player's statistics
                                JObject tmpPlayerStatsData = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPISoccerPlayers(APIKey, 3, userPlayerID, String.Empty));
                                JToken playerTeam = tmpPlayerStatsData["api"]["players"][0]["team_name"];
                                JToken playerInjured = tmpPlayerStatsData["api"]["players"][0]["injured"];
                                JToken playerNumber = tmpPlayerStatsData["api"]["players"][0]["number"];
                                JToken playerRating = tmpPlayerStatsData["api"]["players"][0]["rating"];
                                int totalGoals = 0, totalGames = 0;
                                // Loop all competitions data that current player have played on
                                foreach (var player in tmpPlayerStatsData["api"]["players"]) {
                                    totalGoals += (int)((JObject)player)["goals"]["total"];
                                    totalGames += (int)((JObject)player)["games"]["appearences"];
                                }
                                // Instance current player
                                PlayerData currentPlayer = listPlayers[userPlayerID];
                                // Build out the reply
                                replyEmbed.Title = $"**{ currentPlayer.playerName }**";
                                replyEmbed.AddField($"Name", currentPlayer.playerName, true);
                                replyEmbed.AddField($"Nationality", currentPlayer.playerNation, true);
                                replyEmbed.AddField($"Age", currentPlayer.playerAge, true);
                                replyEmbed.AddField($"Height", currentPlayer.playerHeight, true);
                                replyEmbed.AddField($"Weight", currentPlayer.playerWeight, true);
                                if(playerTeam.Type != JTokenType.Null && playerTeam.Type != JTokenType.Undefined) 
                                    replyEmbed.AddField($"Current Team", (String)playerTeam, true);
                                replyEmbed.AddField($"Position", currentPlayer.playerPosition, true);
                                if (playerInjured.Type != JTokenType.Null && playerInjured.Type != JTokenType.Undefined) 
                                    replyEmbed.AddField($"Injured", (String)playerInjured, true);
                                if (playerNumber.Type != JTokenType.Null && playerNumber.Type != JTokenType.Undefined) 
                                    replyEmbed.AddField($"Number", (String)playerNumber, true);
                                if (playerRating.Type != JTokenType.Null && playerRating.Type != JTokenType.Undefined) 
                                    replyEmbed.AddField($"Rating", (String)playerRating, true);
                                replyEmbed.AddField($"Total Goals", totalGoals, true);
                                replyEmbed.AddField($"Total Games", totalGames, true);
                            } else replyEmbed.Description = "Sorry boss, I don't know nothing about this player";
                        } else {
                            // Invalid emoji reaction
                            await response.AddReactionAsync(new Emoji("👎"));
                            replyEmbed.Description = "Invalid numeric value";
                        }
                    }
                } catch (NullReferenceException) {
                    replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key";
                } catch (WebException) {
                    replyEmbed.Description = "Sorry boss, I think this is not a player...";
                } catch (JsonReaderException) {
                    replyEmbed.Description = "I couldn't get results for this command";
                } catch (ArgumentException) {
                    replyEmbed.Description = "I don't have complete data on this player";
                } catch (Discord.Net.HttpException excep) {
                    replyEmbed.Description = excep.Message;
                }
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }

        [Command("goals")]
        [Alias("highlights")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get the latest goals and highlights video from a match of a particular soccer team")]
        public async Task getTeamGamesGoals([Remainder][Summary("Team name")] String _teamName) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedsColor);
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLAPIKeys.checkAPIKeyExists(conn, Context.Guild.Id)) {
                String APIKey = await MySQLAPIKeys.getAPIKey(conn, Context.Guild.Id);
                try {
                    // Get JSON data of the given team last matches from API
                    JArray jsonData_Goals = (JArray)JsonConvert.DeserializeObject(await APIsFunctions.getAPISoccerGoals(APIKey));
                    // Loop through all matches returned
                    foreach (JToken match in jsonData_Goals) {
                        JObject currentMatch = match.ToObject<JObject>();
                        // If matches titles contains team name
                        if (currentMatch["title"].ToString().ToLower().Contains(_teamName.ToLower())) {
                            MatchCollection urlRegex = Regex.Matches((String)currentMatch["embed"], @"(http|https)\://[a-zA-Z0-9-.]+.[a-zA-Z]{2,3}(/\S*)?'");
                            // Title -> Match teams
                            replyEmbed.WithTitle((String)currentMatch["title"]);
                            // URL -> Embed URL
                            replyEmbed.WithUrl(urlRegex[0].Value.ToString().Remove(urlRegex[0].Value.ToString().Length - 1));
                            replyEmbed.WithDescription((String)currentMatch["date"]);
                            replyEmbed.WithThumbnailUrl((String)currentMatch["thumbnail"]);
                            // Reply with the embed
                            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
                        }
                    }
                } catch (NullReferenceException) {
                    replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key";
                } catch (WebException) {
                    replyEmbed.Description = "My apologies, I don't have results for this team...";
                } catch (JsonReaderException) {
                    replyEmbed.Description = "I couldn't get results for this command";
                } catch (Discord.Net.HttpException excep) {
                    replyEmbed.Description = excep.Message;
                }
            } else {
                replyEmbed.Description = "API key for this server not found on DB";
                // Reply with the embed
                await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
            }
            // Close local connection
            await conn.closeConnection();
        }
    }
}
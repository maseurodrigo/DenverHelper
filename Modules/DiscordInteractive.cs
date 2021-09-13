using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DiscordDenver.Data.Functions;
using DiscordDenver.Data.MySQL;

namespace DiscordDenver.Modules
{
    [Summary("Interactive Data Commands")]
    public class DiscordInteractive : InteractiveBase
    {
        private readonly InteractiveService _interactiveService;
        private MySQLConnect mySQLConnect;
        private DiscordInteractive(MySQLConnect _MySQLConnect) { this.mySQLConnect = _MySQLConnect; }

        [Command("convert", RunMode = RunMode.Async)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get the value in the given cryptocurrency for the indicated dollar value")]
        public async Task convCryptoPrice([Remainder][Summary("Crypto coin")] String _coin) {
            // Initialize empty string builder for reply
            var strBuilder = new StringBuilder();
            // Embed layout reply
            var replyEmbed = new EmbedBuilder();
            // Add some options to the embed (like color and title)
            replyEmbed.WithColor(new Color(33, 150, 243));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            if (await MySQLAPIKeys.checkAPIKeyExists(mySQLConnect.myConn, Context.Guild.Id)) {
                try {
                    // Waiting for an input of a valid numeric value
                    await ReplyAsync("Amount of dollars ?");
                    int dollars;
                    var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));
                    if (response != null) {
                        if (int.TryParse(response.Content.Trim(), out dollars)) {
                            // Valid emoji reaction
                            await response.AddReactionAsync(new Emoji("👍"));
                            // Trigger typing state on current channel
                            await Context.Channel.TriggerTypingAsync();
                            // Get JSON data of the given crypto name from API
                            String APIKey = await MySQLAPIKeys.getAPIKey(mySQLConnect.myConn, Context.Guild.Id);
                            object jsonData = JsonConvert.DeserializeObject<Object>(APIsFunctions.getCryptoData(APIKey, _coin));
                            // Crypto token object
                            JToken token = ((JObject)jsonData)["symbol"];
                            if (token.Type == JTokenType.Null || token.Type == JTokenType.Undefined) {
                                replyEmbed.Description = "I think this doesn't match any cryptocurrency name...";
                            } else {
                                // Store crypto data (name, price)
                                JToken fullName = ((JObject)jsonData)["name"];
                                JToken currentPrice = ((JObject)jsonData)["market_data"]["current_price"]["usd"];
                                // Build out the reply
                                replyEmbed.Title = $"Dollars to { (String)fullName }";
                                double convCrypto = dollars / Convert.ToDouble(currentPrice.ToString());
                                strBuilder.AppendLine($"🔄{ new String(' ', 3) }You will get **{ convCrypto.ToString("0.0000") }** { (String)fullName } ({ (String)token }) for { dollars } dollars");
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
                } catch (ArgumentNullException) {
                    replyEmbed.Description = "I think this doesn't match any cryptocurrency name...";
                } catch (JsonReaderException) {
                    replyEmbed.Description = "I couldn't get results for this command";
                }
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("matches", RunMode = RunMode.Async)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get scheduled matches for a particular football team")]
        public async Task getTeamGames([Summary("Next/Last match")] String _typeMatch, [Remainder][Summary("Team name")] String _teamName) {
            // Initialize empty string builder for reply
            var strBuilder = new StringBuilder();
            // Embed layout reply
            var replyEmbed = new EmbedBuilder();
            // Add some options to the embed (like color and title)
            replyEmbed.WithColor(new Color(139, 195, 74));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            if (await MySQLAPIKeys.checkAPIKeyExists(mySQLConnect.myConn, Context.Guild.Id)) {
                // Get JSON data of the given team next matches from API
                String APIKey = await MySQLAPIKeys.getAPIKey(mySQLConnect.myConn, Context.Guild.Id);
                object jsonData_Team = JsonConvert.DeserializeObject<Object>(await APIsFunctions.getAPIFootballTeams(APIKey, 4, null, _teamName.Trim().ToLower(), null, null, null));
                try {
                    List<String> typeOptions = new List<String>() { "NEXT", "LAST" };
                    // Checking if "typeMatch" param its valid
                    if (typeOptions.Contains(_typeMatch.ToUpper())) {
                        // Waiting for an input of a valid numeric value
                        await ReplyAsync("Number of matches ?");
                        int countMatches;
                        var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));
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
                                    JToken teamID = ((JObject)jsonData_Team)["api"]["teams"][0]["team_id"];
                                    JToken teamName = ((JObject)jsonData_Team)["api"]["teams"][0]["name"];
                                    JToken teamLogo = ((JObject)jsonData_Team)["api"]["teams"][0]["logo"];
                                    if (_typeMatch.ToUpper().Equals(typeOptions[0])) {
                                        // Next matches
                                        object jsonData_Matches = JsonConvert.DeserializeObject<Object>(await APIsFunctions.getAPIFootballTeams(APIKey, 2, (int)teamID, null, countMatches, null, null));
                                        // Loop through all team matches
                                        foreach (var match in ((JObject)jsonData_Matches)["api"]["fixtures"]) {
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
                                        object jsonData_Matches = JsonConvert.DeserializeObject<Object>(await APIsFunctions.getAPIFootballTeams(APIKey, 5, (int)teamID, null, countMatches, null, null));
                                        // Loop through all team matches
                                        foreach (var match in ((JObject)jsonData_Matches)["api"]["fixtures"]) {
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
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("tip", RunMode = RunMode.Async)]
        [Alias("predict")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get predictions for the next scheduled matche(s) of a particular football team")]
        public async Task getTeamGamesPredict([Remainder][Summary("Team name")] String _teamName) {
            // Initialize empty string builder for reply
            var strBuilder = new StringBuilder();
            // Embed layout reply
            var replyEmbed = new EmbedBuilder();
            // Add some options to the embed (like color and title)
            replyEmbed.WithColor(new Color(139, 195, 74));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            if (await MySQLAPIKeys.checkAPIKeyExists(mySQLConnect.myConn, Context.Guild.Id)) {
                // Get JSON data of the given team next matches from API
                String APIKey = await MySQLAPIKeys.getAPIKey(mySQLConnect.myConn, Context.Guild.Id);
                object jsonData_Team = JsonConvert.DeserializeObject<Object>(await APIsFunctions.getAPIFootballTeams(APIKey, 4, null, _teamName.Trim().ToLower(), null, null, null));
                try {
                    // Waiting for an input of a valid numeric value
                    await ReplyAsync("Number of matches ?");
                    int countMatches;
                    var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));
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
                                JToken teamID = ((JObject)jsonData_Team)["api"]["teams"][0]["team_id"];
                                JToken teamName = ((JObject)jsonData_Team)["api"]["teams"][0]["name"];
                                JToken teamLogo = ((JObject)jsonData_Team)["api"]["teams"][0]["logo"];
                                object jsonData_Game = JsonConvert.DeserializeObject<Object>(await APIsFunctions.getAPIFootballTeams(APIKey, 2, (int)teamID, null, countMatches, null, null));
                                // Loop through all team matches
                                foreach (var match in ((JObject)jsonData_Game)["api"]["fixtures"]) {
                                    // Current match ID
                                    int _tmpMatchID = Convert.ToInt32(((JObject)match)["fixture_id"].ToString());
                                    // JSON data from current match
                                    object jsonData_Predict = JsonConvert.DeserializeObject<Object>(await APIsFunctions.getAPIFootballTeams(APIKey, 3, null, String.Empty, null, _tmpMatchID, null));
                                    string _tmpAgainst = ((JObject)match)["homeTeam"]["team_id"].ToString() == (String)teamID ?
                                                    ((JObject)match)["awayTeam"]["team_name"].ToString() : ((JObject)match)["homeTeam"]["team_name"].ToString();
                                    string _tmpPredict = ((JObject)jsonData_Predict)["api"]["predictions"][0]["advice"].ToString();
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
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
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
        [Command("player", RunMode = RunMode.Async)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get informations relating to the given football player")]
        public async Task getPlayerData([Remainder][Summary("Player name")] String _player) {
            // Embed layout reply
            var replyEmbed = new EmbedBuilder();
            // Add some options to the embed (like color and title)
            Color embedColor = new Color(77, 182, 172);
            replyEmbed.WithColor(embedColor);
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            if (await MySQLAPIKeys.checkAPIKeyExists(mySQLConnect.myConn, Context.Guild.Id)) {
                // Get JSON data of the given city from APIs
                String APIKey = await MySQLAPIKeys.getAPIKey(mySQLConnect.myConn, Context.Guild.Id);
                object tmpPlayerData = JsonConvert.DeserializeObject<Object>(await APIsFunctions.getAPIFootballPlayers(APIKey, 1, null, _player.Trim().ToLower(), null));
                try {
                    // Embed layout reply
                    var listPlayersEmbed = new EmbedBuilder();
                    // Add some options to the embed (like color and title)
                    listPlayersEmbed.WithColor(embedColor);
                    Dictionary<int, PlayerData> listPlayers = new Dictionary<int, PlayerData>();
                    // Check if the results are too many 
                    JToken totalResults = ((JObject)tmpPlayerData)["api"]["results"];
                    if ((int)totalResults > 20) listPlayersEmbed.Description = "You need to be more specific with the player's name";
                    else {
                        // Fill all variables within JSON data
                        StringBuilder allPlayers = new StringBuilder();
                        // Loop through all players with similar names
                        foreach (var player in ((JObject)tmpPlayerData)["api"]["players"]) {
                            JToken playerID = ((JObject)player)["player_id"];
                            JToken playerName = ((JObject)player)["player_name"];
                            JToken playerPosition = ((JObject)player)["position"];
                            JToken playerNation = ((JObject)player)["nationality"];
                            JToken playerAge = ((JObject)player)["age"];
                            JToken playerHeight = ((JObject)player)["height"];
                            JToken playerWeight = ((JObject)player)["weight"];
                            allPlayers.AppendLine($"{ (String)playerID }: **{ (String)playerName }** ({ (String)playerNation })");
                            listPlayers.Add((int)playerID, new PlayerData((String)playerName, (String)playerPosition, (String)playerNation,
                                (int)playerAge, (String)playerHeight, (String)playerWeight));
                        }
                        // Build out the reply
                        listPlayersEmbed.AddField($"Some { CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_player.Trim().ToLower()) }'s", allPlayers.ToString(), true);
                        // Reply with the embed
                        await ReplyAsync(null, false, listPlayersEmbed.Build());
                        int userPlayerID;
                        // Waiting for an input of a valid numeric value
                        var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(20));
                        if (int.TryParse(response.Content.Trim(), out userPlayerID)) {
                            if (listPlayers.ContainsKey(userPlayerID)) {
                                // Get all player's statistics
                                object tmpPlayerStatsData = JsonConvert.DeserializeObject<Object>(await APIsFunctions.getAPIFootballPlayers(APIKey, 3, userPlayerID, String.Empty, null));
                                JToken playerTeam = ((JObject)tmpPlayerStatsData)["api"]["players"][0]["team_name"];
                                JToken playerInjured = ((JObject)tmpPlayerStatsData)["api"]["players"][0]["injured"];
                                JToken playerNumber = ((JObject)tmpPlayerStatsData)["api"]["players"][0]["number"];
                                JToken playerRating = ((JObject)tmpPlayerStatsData)["api"]["players"][0]["rating"];
                                int totalGoals = 0, totalGames = 0;
                                // Loop all competitions data that current player have played on
                                foreach (var player in ((JObject)tmpPlayerStatsData)["api"]["players"]) {
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
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }
    }
}
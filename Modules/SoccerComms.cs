using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using DenverHelper.Data;
using DenverHelper.Data.MySQL;
using DenverHelper.Data.JSON.Soccer;
using Newtonsoft.Json;

namespace DenverHelper.Modules
{
    [Summary("Soccer Discord Commands")]
    public class SoccerComms : InteractiveBase
    {
        // Getting all services through constructor param with AddSingleton()
        //private BotData botData { get; }
        //private static readonly Color embedsColor = new Color(139, 195, 74);
        //private SoccerComms(BotData _BotData) => botData = _BotData;

        /*[Command("matches")]
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
                Team teamSearchData = TeamGetData.FromJson(await TeamClass.GetSoccerTeam(APIKey, _teamName.Trim().ToLower()));
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
                                    if (_typeMatch.ToUpper().Equals(typeOptions.First())) {
                                        // Next matches
                                        TeamMatches matchesData = MatchesGetData.FromJson(await NextMatchesClass.GetSoccerTeamMatches(APIKey, teamSearchData.Api.Teams.First().TeamId, countMatches));
                                        // Loop through all team matches
                                        foreach (FixtureMatches match in matchesData.Api.Fixtures) {
                                            String _tmpAgainst = match.HomeTeam.TeamId == teamSearchData.Api.Teams.First().TeamId ? match.AwayTeam.TeamName : match.HomeTeam.TeamName;
                                            // Fill StringBuilders with match data
                                            strBuilder.AppendLine($"🆚{ new String(' ', 2) }**{ _tmpAgainst }** on { (String.IsNullOrWhiteSpace(match.Venue) ? String.Concat("-") : match.Venue) } for **{ match.League.Name }**");
                                            strBuilder.AppendLine();
                                        }
                                        // Build out the reply
                                        replyEmbed.Title = $"Next matches of { teamSearchData.Api.Teams.First().Name }";
                                        replyEmbed.WithThumbnailUrl(teamSearchData.Api.Teams.First().Logo.AbsoluteUri);
                                        replyEmbed.Description = strBuilder.ToString();
                                    } else {
                                        // Last matches
                                        TeamMatches matchesData = MatchesGetData.FromJson(await LastMatchesClass.GetSoccerTeamMatches(APIKey, teamSearchData.Api.Teams.First().TeamId, countMatches));
                                        // Loop through all team matches
                                        foreach (FixtureMatches match in matchesData.Api.Fixtures) {
                                            KeyValuePair<bool, String> _tmpAgainst = match.HomeTeam.TeamId == teamSearchData.Api.Teams.First().TeamId ? 
                                                new KeyValuePair<bool, String>(true, match.AwayTeam.TeamName) : new KeyValuePair<bool, String>(false, match.HomeTeam.TeamName);
                                            String resultState = String.Empty;
                                            // Different results possibilites
                                            if (_tmpAgainst.Key) {
                                                if (match.GoalsHomeTeam > match.GoalsAwayTeam) resultState = "win";
                                                else if (match.GoalsHomeTeam < match.GoalsAwayTeam) resultState = "lose";
                                                else resultState = "draw";
                                            } else {
                                                if (match.GoalsHomeTeam > match.GoalsAwayTeam) resultState = "lose";
                                                else if (match.GoalsHomeTeam < match.GoalsAwayTeam) resultState = "win";
                                                else resultState = "draw";
                                            }
                                            // Fill StringBuilders with match data
                                            strBuilder.AppendLine($"🆚{ new String(' ', 2) }**{ _tmpAgainst.Value }** with a { resultState } of { match.Score.Fulltime } for **{ match.League.Name }**");
                                            strBuilder.AppendLine();
                                        }
                                        // Build out the reply
                                        replyEmbed.Title = $"Next matches of { teamSearchData.Api.Teams.First().Name }";
                                        replyEmbed.WithThumbnailUrl(teamSearchData.Api.Teams.First().Logo.AbsoluteUri);
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
                } 
                catch(NullReferenceException) { replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key"; } 
                catch (WebException) { replyEmbed.Description = "My apologies, I honestly don't know this team..."; } 
                catch (JsonReaderException) { replyEmbed.Description = "I couldn't get results for this command"; }
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }*/

        /*[Command("tip")]
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
                Team teamSearchData = TeamGetData.FromJson(await TeamClass.GetSoccerTeam(APIKey, _teamName.Trim().ToLower()));
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
                                TeamMatches matchesData = MatchesGetData.FromJson(await NextMatchesClass.GetSoccerTeamMatches(APIKey, teamSearchData.Api.Teams.First().TeamId, countMatches));
                                // Loop through all team matches
                                foreach (FixtureMatches match in matchesData.Api.Fixtures) {
                                    // JSON data from current match
                                    MatchTips matchTipData = MatchTipsGetData.FromJson(await MatchTipsClass.GetSoccerMatchTips(APIKey, match.FixtureId));
                                    String _tmpAgainst = match.HomeTeam.TeamId == teamSearchData.Api.Teams.First().TeamId ? match.AwayTeam.TeamName : match.HomeTeam.TeamName;
                                    // Fill StringBuilders with match data
                                    strBuilder.AppendLine($"🆚{ new String(' ', 2) }**{ _tmpAgainst }** with a prediction of **{ matchTipData.Api.Predictions.First().Advice }**");
                                    strBuilder.AppendLine();
                                }
                                // Build out the reply
                                replyEmbed.Title = $"Next matches of { teamSearchData.Api.Teams.First().Name }";
                                replyEmbed.WithThumbnailUrl(teamSearchData.Api.Teams.First().Logo.AbsoluteUri);
                                replyEmbed.Description = strBuilder.ToString();
                            }
                        } else {
                            // Invalid emoji reaction
                            await response.AddReactionAsync(new Emoji("👎"));
                            replyEmbed.Description = "Invalid numeric value";
                        }
                    } else await ReplyAsync($"{ Context.User.Mention } you didnt reply before the timeout"); // response timeout
                } 
                catch(NullReferenceException) { replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key"; } 
                catch (WebException) { replyEmbed.Description = "My apologies, I honestly don't know this team..."; } 
                catch (JsonReaderException) { replyEmbed.Description = "I couldn't get results for this command"; }
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }*/

        /*[Command("goals")]
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
                    List<GoalsData> goalsData = GoalsGetData.FromJson(await GoalsClass.GetSoccerGoals(APIKey));
                    // Loop through all matches returned
                    foreach (GoalsData match in goalsData) {
                        // If matches titles contains team name
                        if (match.Title.ToLower().Contains(_teamName.ToLower())) {
                            String matchEmbed = String.Empty;
                            if (String.IsNullOrWhiteSpace(match.Videos.First().Embed)) { matchEmbed = match.Embed; }
                            else { matchEmbed = match.Videos.First().Embed; }
                            MatchCollection urlRegex = Regex.Matches(matchEmbed, @"(http|https)\://[a-zA-Z0-9-.]+.[a-zA-Z]{2,3}(/\S*)?'");
                            // Title -> Match teams
                            replyEmbed.WithTitle(match.Title);
                            // URL -> Embed URL
                            replyEmbed.WithUrl(urlRegex.First().Value.ToString().Remove(urlRegex.First().Value.ToString().Length - 1));
                            replyEmbed.WithDescription(match.Date.ToString("g"));
                            replyEmbed.WithThumbnailUrl(match.Thumbnail.AbsoluteUri);
                            replyEmbed.WithFooter(footer => { footer.WithText("ScoreBat"); footer.WithIconUrl("https://bit.ly/39KwWa0"); });
                            // Reply with the embed
                            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
                        }
                    }
                } 
                catch (NullReferenceException) { replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key"; } 
                catch (WebException) { replyEmbed.Description = "My apologies, I don't have results for this team..."; } 
                catch (JsonReaderException) { replyEmbed.Description = "I couldn't get results for this command"; } 
                catch (Discord.Net.HttpException excep) { replyEmbed.Description = excep.Message; }
            } else {
                replyEmbed.Description = "API key for this server not found on DB";
                // Reply with the embed
                await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
            }
            // Close local connection
            await conn.closeConnection();
        }*/
    }
}
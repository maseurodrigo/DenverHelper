using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DenverHelper.Data;
using DenverHelper.Data.Functions;
using DenverHelper.Data.MySQL;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DenverHelper.Modules
{
    [Summary("NBA Discord Commands")]
    public class NBAComms : ModuleBase<SocketCommandContext>
    {
        // Getting all services through constructor param with AddSingleton()
        private BotData botData { get; }
        private readonly Color embedsColor = new Color(161, 136, 127);
        private NBAComms(BotData _BotData) => this.botData = _BotData;

        [Command("nba_team")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get data from an NBA team")]
        public async Task getNBATeam([Summary("Team Nick")] String _teamNick) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedsColor);
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLAPIKeys.checkAPIKeyExists(conn, Context.Guild.Id)) {
                // Get JSON data of NBA team from API
                String APIKey = await MySQLAPIKeys.getAPIKey(conn, Context.Guild.Id);
                JObject tmpTeamData = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPINBATeam(APIKey, 1, _teamNick.Trim().ToLower(), null));
                try {
                    // Store team data (id, name and logo)
                    JToken teamID = tmpTeamData["api"]["teams"][0]["teamId"];
                    JToken teamName = tmpTeamData["api"]["teams"][0]["fullName"];
                    JToken teamLogo = tmpTeamData["api"]["teams"][0]["logo"];
                    // Build out the reply
                    replyEmbed.Title = (String)teamName;
                    replyEmbed.ThumbnailUrl = (String)teamLogo;
                    replyEmbed.AddField("City", tmpTeamData["api"]["teams"][0]["city"], true);
                    replyEmbed.AddField("Abbreviation", tmpTeamData["api"]["teams"][0]["shortName"], false);
                    replyEmbed.AddField("🏆 Leagues", new String('_', 8), false);
                    // Fill all variables within JSON data
                    JObject teamLeagues = (JObject)tmpTeamData["api"]["teams"][0]["leagues"];
                    // Parse all team leagues to JToken list
                    IList<JToken> listLeagues = JObject.Parse(teamLeagues.ToString());
                    // Loop through all team leagues
                    for (var league=0; league < listLeagues.Count; league++) {
                        String leagueName = ((JProperty)listLeagues[league]).Name;
                        replyEmbed.AddField(leagueName, $"{ teamLeagues[leagueName]["confName"] } ({ teamLeagues[leagueName]["divName"] })", true);
                    }
                    replyEmbed.AddField("🏀 Players", new String('_', 8), false);
                    JObject jsonData_Squad = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPINBATeam(APIKey, 2, String.Empty, (int)teamID));
                    // Loop through all team members
                    foreach (JToken player in jsonData_Squad["api"]["players"]) {
                        try {
                            JToken playerFName = ((JObject)player)["firstName"];
                            JToken playerLName = ((JObject)player)["lastName"];
                            if(playerLName.Type == JTokenType.Null || playerLName.Type == JTokenType.Undefined) {
                                replyEmbed.AddField((String)playerFName, new String('-', 1), true);
                            } else replyEmbed.AddField((String)playerFName, (String)playerLName, true);
                        } catch (ArgumentException) { }
                    }
                } catch (NullReferenceException) {
                    replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key";
                } catch (WebException) {
                    replyEmbed.Description = "My apologies, I honestly don't know this team...";
                } catch (JsonReaderException) {
                    replyEmbed.Description = "I couldn't get results for this command";
                } catch (ArgumentException) {
                    replyEmbed.Description = "I don't have complete data on this team";
                }
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }

        [Command("nba_player")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get informations relating to the given NBA player")]
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
                JObject tmpPlayerData = (JObject)JsonConvert.DeserializeObject(await APIsFunctions.getAPINBAPlayer(APIKey, _player.Trim().ToLower()));
                try {
                    JToken playerFName = tmpPlayerData["api"]["players"][0]["firstName"];
                    JToken playerLName = tmpPlayerData["api"]["players"][0]["lastName"];
                    JToken playerCountry = tmpPlayerData["api"]["players"][0]["country"];
                    JToken playerBirthDay = tmpPlayerData["api"]["players"][0]["dateOfBirth"];
                    JToken mtrsHeight = tmpPlayerData["api"]["players"][0]["heightInMeters"];
                    JToken kgsWeight = tmpPlayerData["api"]["players"][0]["weightInKilograms"];
                    JToken startOnNBA = tmpPlayerData["api"]["players"][0]["startNba"];
                    JToken proYears = tmpPlayerData["api"]["players"][0]["yearsPro"];
                    // Build out the reply
                    replyEmbed.Title = $"**{ (String)playerFName } { (String)playerLName }**";
                    replyEmbed.AddField($"First Name", (String)playerFName, true);
                    replyEmbed.AddField($"Last Name", (String)playerLName, true);
                    replyEmbed.AddField($"Country", (String)playerCountry, true);
                    replyEmbed.AddField($"Date of Birth", (String)playerBirthDay, true);
                    replyEmbed.AddField($"Height", (String)mtrsHeight, true);
                    replyEmbed.AddField($"Weight", (String)kgsWeight, true);
                    if (startOnNBA.Type != JTokenType.Null && startOnNBA.Type != JTokenType.Undefined)
                        replyEmbed.AddField($"Start on NBA", (String)startOnNBA, true);
                    if (proYears.Type != JTokenType.Null && proYears.Type != JTokenType.Undefined)
                        replyEmbed.AddField($"Years on NBA", (String)proYears, true);
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
    }
}
using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DiscordDenver.Data.Functions;
using DiscordDenver.Data.MySQL;

namespace DiscordDenver.Modules
{
    [Summary("Data Commands")]
    public class DiscordCommData : ModuleBase<SocketCommandContext>
    {
        // Getting all commands through constructor param with AddSingleton()
        private readonly CommandService commandService;
        private MySQLConnect mySQLConnect;
        private DiscordCommData(CommandService _commandService, MySQLConnect _MySQLConnect) { 
            this.commandService = _commandService;
            this.mySQLConnect = _MySQLConnect;
        }

        [Command("price")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get updated data related to the price of the given cryptocurrency")]
        public async Task getCryptoPrices([Remainder][Summary("Crypto coin")] String _coin) {
            // Initialize empty string builder for reply
            var strBuilder = new StringBuilder();
            // Embed layout reply
            var replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(33, 150, 243));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            if (await MySQLAPIKeys.checkAPIKeyExists(mySQLConnect.myConn, Context.Guild.Id)) {
                try {
                    // Get JSON data of the given crypto name from API
                    String APIKey = await MySQLAPIKeys.getAPIKey(mySQLConnect.myConn, Context.Guild.Id);
                    object jsonData = JsonConvert.DeserializeObject<Object>(APIsFunctions.getCryptoData(APIKey, _coin.Trim().ToLower()));
                    // Crypto token object
                    JToken token = ((JObject)jsonData)["symbol"];
                    if (token.Type == JTokenType.Null || token.Type == JTokenType.Undefined) {
                        replyEmbed.Description = "I think this doesn't match any cryptocurrency name...";
                    } else {
                        // Store crypto data (name, price, change on 24h and 7d)
                        JToken fullName = ((JObject)jsonData)["name"];
                        JToken currentPrice = ((JObject)jsonData)["market_data"]["current_price"]["usd"];
                        JToken last24Hours = ((JObject)jsonData)["market_data"]["price_change_percentage_24h"];
                        JToken last7Days = ((JObject)jsonData)["market_data"]["price_change_percentage_7d"];
                        // Build out the reply
                        replyEmbed.Title = $"Price of { (String)fullName }";
                        strBuilder.AppendLine($"💵{ new String(' ', 3) }Current Price: **{ Convert.ToDouble(currentPrice.ToString()).ToString("0.00") } $**");
                        strBuilder.AppendLine();
                        double tmpLast24Hours = Convert.ToDouble(last24Hours.ToString());
                        String icon24Hours = tmpLast24Hours >= 0 ? "📈" : "📉";
                        strBuilder.AppendLine($"{ icon24Hours }{ new String(' ', 3) }24H Change: { tmpLast24Hours.ToString("0.00") } %");
                        strBuilder.AppendLine();
                        double tmpLast7Days = Convert.ToDouble(last7Days.ToString());
                        String icon7Days = tmpLast7Days >= 0 ? "📈" : "📉";
                        strBuilder.AppendLine($"{ icon7Days }{ new String(' ', 3) }7D Change: { tmpLast7Days.ToString("0.00") } %");
                        replyEmbed.Description = strBuilder.ToString();
                    }
                } catch (NullReferenceException) {
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

        [Command("city")]
        [Alias("weather", "temperature")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get the timezone and weather conditions of the given city")]
        public async Task getCityData([Remainder][Summary("City name")] String _city) {
            // Initialize empty string builder for reply
            var strBuilder = new StringBuilder();
            // Embed layout reply
            var replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(0, 150, 136));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            if (await MySQLAPIKeys.checkAPIKeyExists(mySQLConnect.myConn, Context.Guild.Id)) {
                // Get JSON data of the given city from APIs
                String APIKey = await MySQLAPIKeys.getAPIKey(mySQLConnect.myConn, Context.Guild.Id);
                object tmpTimeZoneData = JsonConvert.DeserializeObject<Object>(APIsFunctions.getTimeZoneData(APIKey, _city.Trim().ToLower()));
                object tmpWeatherData = JsonConvert.DeserializeObject<Object>(APIsFunctions.getWeatherData(APIKey, _city.Trim().ToLower()));
                try {
                    // Build out the reply
                    replyEmbed.Title = $"Now in { CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_city.ToLower()) }...";
                    JToken localTime = ((JObject)tmpTimeZoneData)["location"]["localtime"];
                    DateTime localTimeParsed;
                    if (DateTime.TryParse((String)localTime, out localTimeParsed)) {
                        strBuilder.AppendLine($"⏲{ new String(' ', 3) }Current Time: **{ localTimeParsed.ToString("HH:mm") }** hours");
                        strBuilder.AppendLine();
                    }
                    JToken weatherCondition = ((JObject)tmpWeatherData)["current"]["condition"]["text"];
                    strBuilder.AppendLine($"⛅{ new String(' ', 3) }Current Weather: **{ (String)weatherCondition }**");
                    strBuilder.AppendLine();
                    JToken weatherTempC = ((JObject)tmpWeatherData)["current"]["temp_c"];
                    strBuilder.AppendLine($"🌡{ new String(' ', 3) }Current Temperature: **{ (String)weatherTempC }**c");
                    replyEmbed.Description = strBuilder.ToString();
                } catch (NullReferenceException) {
                    replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key";
                } catch (WebException) {
                    replyEmbed.Description = "Sorry boss, I think this is not a city...";
                } catch (JsonReaderException) {
                    replyEmbed.Description = "I couldn't get results for this command";
                }
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("team")]
        [Alias("squad")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get all team members from a particular football team")]
        public async Task getTeamMembers([Remainder][Summary("Team name")] String _teamName) {
            // Embed layout reply
            var replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(139, 195, 74));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            if (await MySQLAPIKeys.checkAPIKeyExists(mySQLConnect.myConn, Context.Guild.Id)) {
                // Get JSON data of the given team from API
                String APIKey = await MySQLAPIKeys.getAPIKey(mySQLConnect.myConn, Context.Guild.Id);
                object jsonData_Team = JsonConvert.DeserializeObject<Object>(await APIsFunctions.getAPIFootballTeams(APIKey, 4, null, _teamName.Trim().ToLower(), null, null, null));
                try {
                    // Store team data (id, name and logo)
                    JToken teamID = ((JObject)jsonData_Team)["api"]["teams"][0]["team_id"];
                    JToken teamName = ((JObject)jsonData_Team)["api"]["teams"][0]["name"];
                    JToken teamLogo = ((JObject)jsonData_Team)["api"]["teams"][0]["logo"];
                    object jsonData_Squad = JsonConvert.DeserializeObject<Object>(await APIsFunctions.getAPIFootballTeams(APIKey, 1, (int)teamID, null, null, null, DateTime.Now.Year));
                    // Fill all variables within JSON data
                    StringBuilder allGoalkeepers = new StringBuilder(),
                        allDefenders = new StringBuilder(),
                        allMidfielders = new StringBuilder(),
                        allAttackers = new StringBuilder();
                    // Loop through all team members
                    foreach (var player in ((JObject)jsonData_Squad)["api"]["players"]) {
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
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }
    }
}
using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Addons.Interactive;
using DiscordDenver.Data;
using DiscordDenver.Data.Functions;
using DiscordDenver.Data.MySQL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordDenver.Modules
{
    [Summary("Crypto Discord Commands")]
    public class CryptoComms : InteractiveBase
    {
        // Getting all services through constructor param with AddSingleton()
        private BotData botData { get; }
        private CryptoComms(BotData _BotData) => this.botData = _BotData;

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
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLAPIKeys.checkAPIKeyExists(conn, Context.Guild.Id)) {
                try {
                    // Get JSON data of the given crypto name from API
                    String APIKey = await MySQLAPIKeys.getAPIKey(conn, Context.Guild.Id);
                    object jsonData = JsonConvert.DeserializeObject<Object>(await APIsFunctions.getCryptoData(APIKey, _coin.Trim().ToLower()));
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
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }

        [Command("convert")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get the value in the given cryptocurrency for the indicated dollar value")]
        public async Task convCryptoPrice([Remainder][Summary("Crypto coin")] String _coin) {
            // Initialize empty string builder for reply
            var strBuilder = new StringBuilder();
            // Embed layout reply
            var replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(33, 150, 243));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLAPIKeys.checkAPIKeyExists(conn, Context.Guild.Id)) {
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
                            String APIKey = await MySQLAPIKeys.getAPIKey(conn, Context.Guild.Id);
                            object jsonData = JsonConvert.DeserializeObject<Object>(await APIsFunctions.getCryptoData(APIKey, _coin));
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
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }
    }
}
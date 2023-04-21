using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Addons.Interactive;
using DenverHelper.Data;
using DenverHelper.Data.JSON;
using DenverHelper.Data.MySQL;
using Newtonsoft.Json;

namespace DenverHelper.Modules
{
    [Summary("Crypto Discord Commands")]
    public class CryptoComms : InteractiveBase
    {
        // Getting all services through constructor param with AddSingleton()
        //private BotData botData { get; }
        //private CryptoComms(BotData _BotData) => botData = _BotData;

        /*[Command("price")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get updated data related to the price of the given cryptocurrency")]
        public async Task getCryptoPrices([Remainder][Summary("Crypto coin")] String _coin) {
            // Initialize empty string builder for reply
            StringBuilder strBuilder = new StringBuilder();
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(33, 150, 243));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLAPIKeys.checkAPIKeyExists(conn, Context.Guild.Id)) {
                try {
                    // Get JSON data of the given crypto name from API
                    String APIKey = await MySQLAPIKeys.getAPIKey(conn, Context.Guild.Id);
                    CryptoData cryptoData = CryptoGetData.FromJson(await CryptoClass.GetCryptoData(APIKey, _coin.Trim().ToLower()));
                    if (String.IsNullOrWhiteSpace(cryptoData.Symbol)) {
                        replyEmbed.Description = "I think this doesn't match any cryptocurrency name...";
                    } else {
                        // Build out the reply
                        replyEmbed.Title = $"Price of { cryptoData.Name }";
                        strBuilder.AppendLine($"💵{ new String(' ', 3) }Current Price: **{ Convert.ToDouble(cryptoData.Market.CurrentPrice.Usd).ToString("0.00") } $**");
                        strBuilder.AppendLine();
                        double tmpLast24Hours = Convert.ToDouble(cryptoData.Market.PriceChangePercentage24H);
                        String icon24Hours = tmpLast24Hours >= 0 ? "📈" : "📉";
                        strBuilder.AppendLine($"{ icon24Hours }{ new String(' ', 3) }24H Change: { tmpLast24Hours.ToString("0.00") } %");
                        strBuilder.AppendLine();
                        double tmpLast7Days = Convert.ToDouble(cryptoData.Market.PriceChangePercentage7D);
                        String icon7Days = tmpLast7Days >= 0 ? "📈" : "📉";
                        strBuilder.AppendLine($"{ icon7Days }{ new String(' ', 3) }7D Change: { tmpLast7Days.ToString("0.00") } %");
                        replyEmbed.Description = strBuilder.ToString();
                        replyEmbed.WithThumbnailUrl(cryptoData.Image.Large.AbsoluteUri);
                    }
                } 
                catch (NullReferenceException) { replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key"; } 
                catch (ArgumentNullException) { replyEmbed.Description = "I think this doesn't match any cryptocurrency name..."; } 
                catch (JsonReaderException) { replyEmbed.Description = "I couldn't get results for this command"; }
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }*/
    }
}
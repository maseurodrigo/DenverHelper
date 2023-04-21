using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using DenverHelper.Data;
using DenverHelper.Data.MySQL;
using DenverHelper.Data.JSON;

namespace DenverHelper.Modules
{
    [Summary("Weather Discord Commands")]
    public class WeatherComms : ModuleBase<SocketCommandContext>
    {
        // Getting all services through constructor param with AddSingleton()
        //private BotData botData { get; }
        //private WeatherComms(BotData _BotData) => botData = _BotData;

        /*[Command("city")]
        [Alias("weather", "temperature")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get the timezone and weather conditions of the given city")]
        public async Task getCityData([Remainder][Summary("City name")] String _city) {
            // Initialize empty string builder for reply
            StringBuilder strBuilder = new StringBuilder();
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(0, 150, 136));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLAPIKeys.checkAPIKeyExists(conn, Context.Guild.Id)) {
                // Get JSON data of the given city from APIs
                String APIKey = await MySQLAPIKeys.getAPIKey(conn, Context.Guild.Id), tmpCity = _city.Trim().ToLower();
                Weather weatherData = WeatherGetData.FromJson(await WeatherClass.GetWeatherData(APIKey, tmpCity));
                try {
                    // Build out the reply
                    replyEmbed.Title = $"Now in { weatherData.Location.Name }...";
                    strBuilder.AppendLine($"⏲{ new String(' ', 3) }Current Time: **{ weatherData.Location.Localtime.ToString("t") }**");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine($"⛅{ new String(' ', 3) }Current Weather: **{ weatherData.Current.Condition.Text }**");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine($"🌡{ new String(' ', 3) }Current Temperature: **{ weatherData.Current.TempC }c**");
                    replyEmbed.Description = strBuilder.ToString();
                    replyEmbed.WithThumbnailUrl(new UriBuilder(weatherData.Current.Condition.Icon.TrimStart('/')).Uri.AbsoluteUri);
                } 
                catch (NullReferenceException) { replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key"; } 
                catch (WebException) { replyEmbed.Description = "Sorry boss, I think this is not a city..."; } 
                catch (JsonReaderException) { replyEmbed.Description = "I couldn't get results for this command"; }
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }*/
    }
}
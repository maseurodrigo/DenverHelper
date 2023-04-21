using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DenverHelper.Data;
using DenverHelper.Data.JSON.NBA;
using DenverHelper.Data.MySQL;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace DenverHelper.Modules
{
    [Summary("NBA Discord Commands")]
    public class NBAComms : ModuleBase<SocketCommandContext>
    {
        // Getting all services through constructor param with AddSingleton()
        //private BotData botData { get; }
        //private readonly Color embedsColor = new Color(161, 136, 127);
        //private NBAComms(BotData _BotData) => botData = _BotData;

        /*[Command("nba_team")]
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
                Team nbaTeam = TeamGetData.FromJson(await TeamClass.GetNBATeam(APIKey, _teamNick.Trim().ToLower()));
                try {
                    // Build out the reply
                    replyEmbed.WithTitle(nbaTeam.Api.Teams.First().FullName);
                    replyEmbed.WithThumbnailUrl(nbaTeam.Api.Teams.First().Logo.AbsoluteUri);
                    replyEmbed.AddField("City", nbaTeam.Api.Teams.First().City, true);
                    replyEmbed.AddField("Abbreviation", nbaTeam.Api.Teams.First().ShortName, false);
                    // Loop through all team leagues
                    replyEmbed.AddField("\u200b", "🏆 **Leagues**", false);
                    replyEmbed.AddField("Standard", $"{ nbaTeam.Api.Teams.First().Leagues.Standard.ConfName } ({ nbaTeam.Api.Teams.First().Leagues.Standard.DivName })", true);
                    replyEmbed.AddField("Vegas", $"{ nbaTeam.Api.Teams.First().Leagues.Vegas.ConfName } ({ nbaTeam.Api.Teams.First().Leagues.Vegas.DivName })", true);
                    replyEmbed.AddField("Utah", $"{ nbaTeam.Api.Teams.First().Leagues.Utah.ConfName } ({ nbaTeam.Api.Teams.First().Leagues.Utah.DivName })", true);
                    replyEmbed.AddField("Sacramento", $"{ nbaTeam.Api.Teams.First().Leagues.Sacramento.ConfName } ({ nbaTeam.Api.Teams.First().Leagues.Sacramento.DivName })", true);
                    // Loop through all team players
                    TeamPlayers nbaTeamPlayers = TeamPlayersGetData.FromJson(await TeamPlayersClass.GetNBATeamPlayers(APIKey, nbaTeam.Api.Teams.First().TeamId));
                    replyEmbed.AddField("\u200b", "🏀 **Players**", false);
                    // Loop through all team members
                    foreach (TeamPlayersData player in nbaTeamPlayers.Api.Players) {
                        try {
                            if(String.IsNullOrWhiteSpace(player.LastName)) replyEmbed.AddField(player.FirstName, "\u200b", true);
                            else replyEmbed.AddField(player.FirstName, player.LastName, true);
                        } catch (ArgumentException) { }
                    }
                } 
                catch (NullReferenceException) { replyEmbed.Description = "My apologies, but it looks like there are invalid parameter(s) or an invalid API key"; } 
                catch (WebException) { replyEmbed.Description = "My apologies, I honestly don't know this team..."; } 
                catch (JsonReaderException) { replyEmbed.Description = "I couldn't get results for this command"; } 
                catch (ArgumentException) { replyEmbed.Description = "I don't have complete data on this team"; }
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }*/
    }
}
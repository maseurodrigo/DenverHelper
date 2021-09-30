using System;
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
        private BotData botData { get; }
        private readonly Color embedsColor = new Color(161, 136, 127);
        private NBAComms(BotData _BotData) => botData = _BotData;

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
                Team nbaTeam = TeamGetData.FromJson(await TeamClass.GetAPINBATeam(APIKey, _teamNick.Trim().ToLower()));
                try {
                    // Build out the reply
                    replyEmbed.WithTitle(nbaTeam.Api.Teams[0].FullName);
                    replyEmbed.WithThumbnailUrl(nbaTeam.Api.Teams[0].Logo.AbsoluteUri);
                    replyEmbed.AddField("City", nbaTeam.Api.Teams[0].City, true);
                    replyEmbed.AddField("Abbreviation", nbaTeam.Api.Teams[0].ShortName, false);
                    // Loop through all team leagues
                    replyEmbed.AddField("🏆 Leagues", new String('_', 8), false);
                    replyEmbed.AddField("Standard", $"{ nbaTeam.Api.Teams[0].Leagues.Standard.ConfName } ({ nbaTeam.Api.Teams[0].Leagues.Standard.DivName })", true);
                    replyEmbed.AddField("Vegas", $"{ nbaTeam.Api.Teams[0].Leagues.Vegas.ConfName } ({ nbaTeam.Api.Teams[0].Leagues.Vegas.DivName })", true);
                    replyEmbed.AddField("Utah", $"{ nbaTeam.Api.Teams[0].Leagues.Utah.ConfName } ({ nbaTeam.Api.Teams[0].Leagues.Utah.DivName })", true);
                    replyEmbed.AddField("Sacramento", $"{ nbaTeam.Api.Teams[0].Leagues.Sacramento.ConfName } ({ nbaTeam.Api.Teams[0].Leagues.Sacramento.DivName })", true);
                    // Loop through all team players
                    TeamPlayers nbaTeamPlayers = TeamPlayersGetData.FromJson(await TeamPlayersClass.GetAPINBATeam(APIKey, nbaTeam.Api.Teams[0].TeamId));
                    replyEmbed.AddField("🏀 Players", new String('_', 8), false);
                    // Loop through all team members
                    foreach (TeamPlayersData player in nbaTeamPlayers.Api.Players) {
                        try {
                            if(String.IsNullOrWhiteSpace(player.LastName)) replyEmbed.AddField(player.FirstName, new String('-', 1), true);
                            else replyEmbed.AddField(player.FirstName, player.LastName, true);
                        } catch (ArgumentException) { }
                    }
                    replyEmbed.WithFooter(footer => { footer.WithText("API-Basketball"); footer.WithIconUrl("https://bit.ly/3ogprjM"); });
                    replyEmbed.WithCurrentTimestamp();
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
                Player nbaPlayer = PlayerGetData.FromJson(await PlayerClass.GetAPINBAPlayer(APIKey, _player.Trim().ToLower()));
                try {
                    // Build out the reply
                    replyEmbed.Title = $"**{ nbaPlayer.Api.Players[0].FirstName } { nbaPlayer.Api.Players[0].LastName }**";
                    replyEmbed.AddField($"First Name", nbaPlayer.Api.Players[0].FirstName, true);
                    replyEmbed.AddField($"Last Name", nbaPlayer.Api.Players[0].LastName, true);
                    replyEmbed.AddField($"Country", nbaPlayer.Api.Players[0].Country, true);
                    replyEmbed.AddField($"Date of Birth", nbaPlayer.Api.Players[0].DateOfBirth.ToString("d"), true);
                    replyEmbed.AddField($"Height", nbaPlayer.Api.Players[0].HeightInMeters, true);
                    replyEmbed.AddField($"Weight", nbaPlayer.Api.Players[0].WeightInKilograms, true);
                    replyEmbed.AddField($"Start on NBA", nbaPlayer.Api.Players[0].StartNba, true);                    
                    replyEmbed.AddField($"Years on NBA", nbaPlayer.Api.Players[0].YearsPro, true);
                    replyEmbed.WithFooter(footer => { footer.WithText("API-Basketball"); footer.WithIconUrl("https://bit.ly/3ogprjM"); });
                    replyEmbed.WithCurrentTimestamp();
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
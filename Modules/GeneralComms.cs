using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DenverHelper.Data;
using DenverHelper.Data.MySQL;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;

namespace DenverHelper.Modules
{
    [Summary("General Discord Commands")]
    public class GeneralComms : InteractiveBase
    {
        // Getting all services through constructor param with AddSingleton()
        private readonly CommandService commandService;
        private BotData botData { get; }
        private GiveawayList giveawayList { get; set; }
        private GeneralComms(CommandService _commandService, BotData _BotData, GiveawayList _GiveawayList) {
            commandService = _commandService;
            botData = _BotData;
            giveawayList = _GiveawayList;
        }

        [Command("help")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task getHelp() {
            // List of all available commands
            List<CommandInfo> allCommands = commandService.Commands.OrderBy(comm => comm.Name).ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();
            EmbedBuilder embedAdminBuilder = new EmbedBuilder();
            // Instance context user data
            SocketGuildUser contextUser = Context.User as SocketGuildUser;
            GuildPermissions userGuildPerms = contextUser.GuildPermissions;
            // List of commands to exclude from help list
            Dictionary<String, bool> commsToExclude = new Dictionary<String, bool>() {
                { "conn", false }, { "help", false }, { "addkey", true }, { "delkey", true }
            };
            foreach (CommandInfo command in allCommands) {
                if (commsToExclude.ContainsKey(command.Name)) {
                    if (commsToExclude[command.Name]) {
                        // Get the command Summary attribute information
                        String embedFieldText = command.Summary ?? "No description available";
                        embedAdminBuilder.AddField($"`{ command.Name.ToLower() }`", embedFieldText);
                    }
                } else {
                    // Get the command Summary attribute information
                    String embedFieldText = command.Summary ?? "No description available";
                    embedBuilder.AddField($"`{ command.Name.ToLower() }`", embedFieldText);
                }
            }
            // Reply with the embed
            await ReplyAsync(null, false, embedBuilder.Build());
            // DM reply with the admin's embed
            if (userGuildPerms.Administrator) {
                embedAdminBuilder.Title = "Exclusive admin commands";
                await contextUser.SendMessageAsync(null, false, embedAdminBuilder.Build());
            }
        }

        [Command("conn")]
        [Alias("connection", "status")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get the current status of the bot connection")]
        public async Task getConnection() {
            // Retrieve bot connection status
            EmbedBuilder embedBuilder = new EmbedBuilder();
            ConnectionState currentConn = Context.Client.ConnectionState;
            embedBuilder.AddField("Login", Context.Client.LoginState, true);
            embedBuilder.AddField("Connection", currentConn, true);
            if (currentConn.Equals(ConnectionState.Connected)) {
                embedBuilder.AddField("Latency", $"{ Context.Client.Latency } ms", true);
                /*MySQLConnect conn = new MySQLConnect(botData);
                await conn.newConnection();     // Open local connection
                embedBuilder.AddField("MySQLConnect", conn.myConn.State, false);
                await conn.closeConnection();   // Close local connection*/
            }
            // Reply with the embed
            await ReplyAsync(null, false, embedBuilder.Build());
        }

        [Command("poll")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Create a new poll with, custom or not, answers using emojis")]
        public async Task newPoll([Remainder][Summary("Full poll query")] String _fullArgs) {
            EmbedBuilder pollEmbed = new EmbedBuilder();
            pollEmbed.WithColor(new Color(144, 164, 174));
            // Verify if exists question and answers
            String strQuestion = _fullArgs.Split('|').First().Trim();
            // EmbedAuthor as EmbedTitle
            EmbedAuthorBuilder pollTitle = new EmbedAuthorBuilder();
            pollTitle.Name = $"📋 { strQuestion.ToUpper() }";
            pollEmbed.WithAuthor(pollTitle);
            if (_fullArgs.Contains('|')) {
                String strAnswers = _fullArgs.Split('|')[1].Trim();
                if (String.IsNullOrWhiteSpace(strAnswers)) {
                    // Default Yes/No poll
                    List<IEmote> emojiCodes = new List<IEmote>() { new Emoji("👍"), new Emoji("👎") };
                    // Get the poll option text
                    pollEmbed.AddField("Yes", emojiCodes.First(), true);
                    pollEmbed.AddField("No", emojiCodes[1], true);
                    // Create the poll in the channel
                    IUserMessage sent = await ReplyAsync(null, false, pollEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
                    // Add reactions to the poll.
                    await sent.AddReactionsAsync(emojiCodes.ToArray());
                } else {
                    try {
                        // Poll with custom options
                        String[] arrayAnswers = strAnswers.Split(',');
                        List<IEmote> emojiCodes = new List<IEmote>();
                        foreach (String option in arrayAnswers) {
                            // Get the poll option reaction
                            String emojiCode = option.Split('(', ')')[1].Trim();
                            // Get the poll option text (with fields)
                            pollEmbed.AddField(option.Split('(').First().Trim(), emojiCode, true);
                            if (emojiCode.Contains(':')) {
                                // If the reaction is a custom emoji, get the emoji code from the Guild
                                String customEmojiName = emojiCode.Split(':')[1];
                                emojiCodes.Add(Context.Guild.Emotes.First(x => x.Name == customEmojiName));
                            } else emojiCodes.Add(new Emoji(emojiCode));
                        }
                        // Create the poll in the channel
                        IUserMessage sent = await ReplyAsync(null, false, pollEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
                        // Add reactions to the poll.
                        await sent.AddReactionsAsync(emojiCodes.ToArray());
                    } catch (HttpException excep) {
                        pollEmbed.Description = $"{ excep.HttpCode }: { excep.Message }";
                        await ReplyAsync(null, false, pollEmbed.Build());
                    } catch (IndexOutOfRangeException) {
                        pollEmbed.Description = "I'm sorry, but i don't know that identifier";
                        await ReplyAsync(null, false, pollEmbed.Build());
                    }
                }
            } else {
                // Default Yes/No poll
                List<IEmote> emojiCodes = new List<IEmote>() { new Emoji("👍"), new Emoji("👎") };
                // Get the poll option text
                pollEmbed.AddField("Yes", emojiCodes.First(), true);
                pollEmbed.AddField("No", emojiCodes[1], true);
                // Create the poll in the channel
                IUserMessage sent = await ReplyAsync(null, false, pollEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
                // Add reactions to the poll.
                await sent.AddReactionsAsync(emojiCodes.ToArray());
            }
        }

        [Command("giveaway")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [Summary("Create a new giveaway with custom params.")]
        public async Task newGiveaway() {
            Color embColor = new Color(144, 164, 174);
            // Start and end giveaway embeds
            EmbedBuilder giveStartEmbed = new EmbedBuilder();
            giveStartEmbed.WithColor(embColor);
            EmbedBuilder giveEndEmbed = new EmbedBuilder();
            giveEndEmbed.WithColor(embColor);
            // Check if theres already an active giveaway on the server
            if (!giveawayList.Giveaway.ContainsKey(Context.Guild.Id)) {
                // Instance context user data
                SocketGuildUser contextUser = Context.User as SocketGuildUser;
                GuildPermissions userGuildPerms = contextUser.GuildPermissions;
                GiveawayData localGiveaway = new GiveawayData();
                int tmpReplyData;
                // Waiting for an input of a valid string
                await ReplyAsync("Giveaway title ?");
                SocketMessage respTitle = await NextMessageAsync(true, true, TimeSpan.FromSeconds(15));
                if (respTitle != null) {
                    localGiveaway.Title = respTitle.ToString();
                    // Waiting for an input of a valid numeric value
                    await ReplyAsync("Number of parameters ?");
                    SocketMessage respParams = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));
                    if (respParams != null) {
                        if (int.TryParse(respParams.Content.Trim(), out tmpReplyData)) {
                            localGiveaway.Params = tmpReplyData;
                            await ReplyAsync("Number of seconds ?");
                            SocketMessage respTimer = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));
                            if (respTimer != null) {
                                if (int.TryParse(respTimer.Content.Trim(), out tmpReplyData)) {
                                    localGiveaway.Timer = tmpReplyData;
                                    await ReplyAsync("Number of winners ?");
                                    SocketMessage respWinners = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));
                                    if (respWinners != null) {
                                        if (int.TryParse(respWinners.Content.Trim(), out tmpReplyData)) {
                                            localGiveaway.Winners = tmpReplyData;
                                            localGiveaway.Active = true;
                                            giveawayList.Giveaway.Add(Context.Guild.Id, localGiveaway);
                                            // Filling start giveaway embed
                                            EmbedAuthorBuilder giveTitle = new EmbedAuthorBuilder();
                                            giveTitle.Name = $"🎉 { localGiveaway.Title } Giveaway";
                                            giveStartEmbed.WithAuthor(giveTitle);
                                            giveStartEmbed.AddField("Param(s)", localGiveaway.Params, true);
                                            giveStartEmbed.AddField("Time", String.Concat(localGiveaway.Timer, "secs."), true);
                                            giveStartEmbed.AddField("Winner(s)", localGiveaway.Winners, true);
                                            StringBuilder strEntrie = new StringBuilder();
                                            strEntrie.Append(botData.BotPrefix);
                                            strEntrie.Append("givejoin");
                                            for (int i = 0; i < localGiveaway.Params; i++) {
                                                if (i.Equals(0)) strEntrie.Append(String.Concat(" ", localGiveaway.Title));
                                                else strEntrie.Append(String.Concat("_", localGiveaway.Title));
                                            }
                                            giveStartEmbed.AddField("Entry Example", strEntrie, false);
                                            EmbedFooterBuilder giveFooter = new EmbedFooterBuilder();
                                            giveFooter.IconUrl = contextUser.GetAvatarUrl();
                                            giveFooter.Text = $"Started by { contextUser.Username }";
                                            giveStartEmbed.WithFooter(giveFooter);
                                            await ReplyAsync(null, false, giveStartEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
                                            try {
                                                // Suspend current thread waiting for entries
                                                Thread.Sleep(localGiveaway.Timer * 1000);
                                                // Giveaway winners list
                                                Dictionary<ulong, String> listWinners = new Dictionary<ulong, String>();
                                                // Check if the number of entries is greater than the winners
                                                if (giveawayList.Giveaway[Context.Guild.Id].ListUsers.Count > localGiveaway.Winners) {
                                                    // Registered user ids list
                                                    List<ulong> listEntries = new List<ulong>(giveawayList.Giveaway[Context.Guild.Id].ListUsers.Keys);
                                                    for(int i = 0; i < localGiveaway.Winners; i++) {
                                                        List<ulong> shuffleEntries = listEntries.OrderBy(id => new Random().Next()).ToList();
                                                        listWinners.Add(shuffleEntries[0], giveawayList.Giveaway[Context.Guild.Id].ListUsers[shuffleEntries[0]]);
                                                        listEntries.Remove(shuffleEntries[0]);
                                                    }
                                                } else listWinners = giveawayList.Giveaway[Context.Guild.Id].ListUsers;
                                                giveawayList.Giveaway.Remove(Context.Guild.Id);
                                                giveEndEmbed.Title = "Giveaway finished";
                                                await ReplyAsync(null, false, giveEndEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
                                                // Send list of winners to the creator
                                                EmbedBuilder giveWinnersEmbed = new EmbedBuilder();
                                                giveWinnersEmbed.WithColor(embColor);
                                                EmbedAuthorBuilder giveWinnersTitle = new EmbedAuthorBuilder();
                                                giveWinnersTitle.Name = $"🎉 { localGiveaway.Title } Giveaway Winner(s)";
                                                giveWinnersEmbed.WithAuthor(giveWinnersTitle);
                                                foreach (KeyValuePair<ulong, String> entry in listWinners) 
                                                    giveWinnersEmbed.AddField(entry.Key.ToString(), entry.Value, false);
                                                // DM reply with the creator embed
                                                if (userGuildPerms.Administrator) 
                                                    await contextUser.SendMessageAsync(null, false, giveWinnersEmbed.Build());
                                            } catch (ArgumentOutOfRangeException excep) {
                                                giveEndEmbed.Title = excep.Message;
                                                await ReplyAsync(null, false, giveEndEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [Command("givejoin")]
        [Alias("giveawayjoin")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Join current giveaway")]
        public async Task joinGiveaway([Remainder][Summary("Full giveaway query")] String _fullArgs) {
            // Check active giveaway for current server
            if (giveawayList.Giveaway.ContainsKey(Context.Guild.Id) && giveawayList.Giveaway[Context.Guild.Id].Active is true) {
                // Validate user entry data
                if (_fullArgs.Contains('_')) {
                    if (_fullArgs.Split('_').Count().Equals(giveawayList.Giveaway[Context.Guild.Id].Params)) {
                        // Already registered
                        if (giveawayList.Giveaway[Context.Guild.Id].ListUsers.ContainsKey(Context.User.Id)) 
                            await Context.Message.AddReactionAsync(new Emoji("🤙"));
                        else {
                            // New entry
                            giveawayList.Giveaway[Context.Guild.Id].ListUsers.Add(Context.User.Id, _fullArgs);
                            await Context.Message.AddReactionAsync(new Emoji("👍"));
                        }
                    } else { await Context.Message.AddReactionAsync(new Emoji("👎")); }
                } else await Context.Message.AddReactionAsync(new Emoji("👎"));
            } else await Context.Message.AddReactionAsync(new Emoji("❌"));
        }
    }
}
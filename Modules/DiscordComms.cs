using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using DiscordDenver.Data;
using DiscordDenver.Data.MySQL;

namespace DiscordDenver.Modules
{
    [Summary("Logical Commands")]
    public class DiscordComms : ModuleBase<SocketCommandContext>
    {
        // Getting all commands through constructor param with AddSingleton()
        private readonly CommandService commandService;
        private BotData botData;
        private DiscordComms(CommandService _commandService, BotData _BotData) {
            this.commandService = _commandService;
            this.botData = _BotData;
        }

        [Command("help")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task getHelp() {
            // List of all available commands
            List<CommandInfo> allCommands = commandService.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();
            EmbedBuilder embedAdminBuilder = new EmbedBuilder();
            // Instance context user data
            SocketGuildUser contextUser = Context.User as SocketGuildUser;
            GuildPermissions userGuildPerms = contextUser.GuildPermissions;
            // List of commands to exclude from help list
            Dictionary<String, bool> commsToExclude = new Dictionary<String, bool>() {
                { "help", false }, { "addkey", true }, { "delkey", true }
            };
            foreach (CommandInfo command in allCommands) {
                if (commsToExclude.ContainsKey(command.Name)) {
                    if (commsToExclude[command.Name]) {
                        // Get the command Summary attribute information
                        String embedFieldText = command.Summary ?? "No description available\n";
                        embedAdminBuilder.AddField(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(command.Name.ToLower()), embedFieldText);
                    }
                } else {
                    // Get the command Summary attribute information
                    String embedFieldText = command.Summary ?? "No description available\n";
                    embedBuilder.AddField(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(command.Name.ToLower()), embedFieldText);
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
            if (currentConn.Equals(ConnectionState.Connected))
                embedBuilder.AddField("Latency", $"{ Context.Client.Latency } ms", true);
            // Reply with the embed
            await ReplyAsync(null, false, embedBuilder.Build());
        }

        [Command("addkey")]
        [Alias("add_key", "newkey", "new_key")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Set new API key to be assigned to the current server")]
        public async Task setNewAPIKey([Summary("API Key")] String _apiKey) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(144, 164, 174));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLAPIKeys.checkAPIKeyExists(conn, Context.Guild.Id)) {
                await MySQLAPIKeys.changeAPIKey(conn, Context.Guild.Id, _apiKey.Trim(), Context.User.Id);
                replyEmbed.Description = "API key renewed";
            } else {
                await MySQLAPIKeys.newAPIKey(conn, Context.Guild.Id, _apiKey.Trim(), Context.User.Id);
                replyEmbed.Description = "API key set";
            }
            // Close local connection
            await conn.closeConnection();
            // Delete user's message
            await Context.Message.DeleteAsync();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("delkey")]
        [Alias("removekey", "remove_key", "deletekey", "delete_key")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Delete API key thats assigned to the current server")]
        public async Task removeAPIKey() {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(144, 164, 174));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLAPIKeys.checkAPIKeyExists(conn, Context.Guild.Id)) {
                await MySQLAPIKeys.deleteAPIKey(conn, Context.Guild.Id);
                replyEmbed.Description = "API key removed";
            } else replyEmbed.Description = "API key for this server not found on DB";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("comms")]
        [Alias("servercomms", "server_comms", "totalcomms", "total_comms")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get all comms added to the current server")]
        public async Task getTotalServerComms() {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(186, 104, 200));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            int totalComms = await MySQLComms.getTotalComms(conn, Context.Guild.Id);
            replyEmbed.Description = $"There are { totalComms } commands for this server";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("searchcomm")]
        [Alias("search_comm", "checkcomm", "check_comm")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get a list of similar comms found for the current server")]
        public async Task searchServerComms([Summary("Server Comm")] String _serverComm) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(186, 104, 200));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            // List of all available commands
            MySQLConnect conn = new MySQLConnect(botData);
            Dictionary<String, String> allCommands = await MySQLComms.getCommsList(conn, Context.Guild.Id, _serverComm.Trim());
            foreach (KeyValuePair<String, String> command in allCommands) {
                // Get the command Summary attribute information
                replyEmbed.AddField(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(command.Key.ToLower()), command.Value);
            }
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("comm")]
        [Alias("getcomm", "get_comm", "datacomm", "data_comm")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get the associated data for this server from the given comm")]
        public async Task getDataComm([Summary("Command")] String _comm) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(186, 104, 200));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLComms.checkCommExists(conn, Context.Guild.Id, _comm.Trim())) {
                String dataComm = await MySQLComms.getCommData(conn, Context.Guild.Id, _comm.Trim());
                replyEmbed.AddField(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_comm.Trim().ToLower()), dataComm);
            } else replyEmbed.Description = "Command not found for this server";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("addcomm")]
        [Alias("add_comm", "newcomm", "new_comm")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Set a new comm to be assigned to the current server")]
        public async Task setNewComm([Summary("Command")] String _comm, [Remainder][Summary("Command data")] String _commData) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(186, 104, 200));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLComms.checkCommExists(conn, Context.Guild.Id, _comm.Trim())) {
                await MySQLComms.changeCommand(conn, Context.Guild.Id, _comm.Trim(), _commData.Trim(), Context.User.Id);
                replyEmbed.Description = "Command renewed";
            } else {
                await MySQLComms.newCommand(conn, Context.Guild.Id, Context.Channel.Id, _comm.Trim(), _commData.Trim(), Context.User.Id);
                replyEmbed.Description = "Command added";
            }
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("delcomm")]
        [Alias("removecomm", "remove_comm", "deletecomm", "delete_comm")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Delete comm thats assigned to the current server")]
        public async Task removeComm([Summary("Command")] String _comm) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(186, 104, 200));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLComms.checkCommExists(conn, Context.Guild.Id, _comm.Trim())) {
                await MySQLComms.deleteCommand(conn, Context.Guild.Id, _comm.Trim());
                replyEmbed.Description = "Command removed";
            } else replyEmbed.Description = "Command not found for this server";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("poll")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Create a new poll with, custom or not, answers using emojis")]
        public async Task newPoll([Remainder][Summary("Full poll query")] String _fullArgs) {
            if (!(Context.Message.Channel is IDMChannel)) {
                EmbedBuilder pollEmbed = new EmbedBuilder();
                // Verify if exists question and answers
                String strQuestion = _fullArgs.Split('|')[0].Trim();
                if (_fullArgs.Contains('|')) {
                    String strAnswers = _fullArgs.Split('|')[1].Trim();
                    // Define embed title with question
                    pollEmbed.Title = strQuestion.ToUpper();
                    if (String.IsNullOrWhiteSpace(strAnswers)) {
                        // Default Yes/No poll
                        List<IEmote> emojiCodes = new List<IEmote>() { new Emoji("👍"), new Emoji("👎") };
                        // Get the poll option text
                        pollEmbed.AddField("Yes", emojiCodes[0], true);
                        pollEmbed.AddField("No", emojiCodes[1], true);
                        // Create the poll in the channel
                        IUserMessage sent = await ReplyAsync(null, false, pollEmbed.Build());
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
                                pollEmbed.AddField(option.Split('(')[0].Trim(), emojiCode, true);
                                if (emojiCode.Contains(':')) {
                                    // If the reaction is a custom emoji, get the emoji code from the Guild
                                    String customEmojiName = emojiCode.Split(':')[1];
                                    emojiCodes.Add(Context.Guild.Emotes.First(x => x.Name == customEmojiName));
                                } else emojiCodes.Add(new Emoji(emojiCode));
                            }
                            // Create the poll in the channel
                            IUserMessage sent = await ReplyAsync(null, false, pollEmbed.Build());
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
                    // Define embed title with question
                    pollEmbed.Title = strQuestion.ToUpper();
                    List<IEmote> emojiCodes = new List<IEmote>() { new Emoji("👍"), new Emoji("👎") };
                    // Get the poll option text
                    pollEmbed.AddField("Yes", emojiCodes[0], true);
                    pollEmbed.AddField("No", emojiCodes[1], true);
                    // Create the poll in the channel
                    IUserMessage sent = await ReplyAsync(null, false, pollEmbed.Build());
                    // Add reactions to the poll.
                    await sent.AddReactionsAsync(emojiCodes.ToArray());
                }
            }
        }

        [Command("dmuser")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Send a private message to a unique server member")]
        public async Task dmUser(IGuildUser _user, [Remainder][Summary("DM message")] String _message) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(220, 231, 117));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            try {
                // Exclude message author and bots
                if (!Context.User.Id.Equals(_user.Id) && !_user.IsBot) await _user.SendMessageAsync(_message);
                replyEmbed.Description = $"DM sent to { (String.IsNullOrWhiteSpace(_user.Nickname) ? _user.Username : _user.Nickname) }";
            } catch (HttpException) {
                replyEmbed.Description = "I'm sorry, but an error occurred during the operation";
            }
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("dmrole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Send a private message for users with a given role assigned")]
        public async Task dmRole(IRole _role, [Remainder][Summary("DM message")] String _message) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(220, 231, 117));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            int dmsSuccess = 0, dmsError = 0;
            // Loop through all (cached) server users
            foreach (SocketGuildUser serverUser in Context.Guild.Users) {
                try {
                    // Exclude message author and bots
                    if (!Context.User.Id.Equals(serverUser.Id) && !serverUser.IsBot 
                    && serverUser.Roles.Contains(_role)) {
                        await serverUser.SendMessageAsync(_message);
                        dmsSuccess++;
                    }
                } catch (HttpException) {
                    // Counter of users which DM could not be sent
                    dmsError++;
                }
            }
            // Reply with the embed
            replyEmbed.Description = $"{ dmsSuccess } DM(s) were sent and { dmsError } were not!";
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("dmall", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Send a private message to all server members")]
        public async Task dmAll([Remainder][Summary("DM message")] String _message) {
            // Embed layout reply
            Color embedColor = new Color(220, 231, 117);
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedColor);
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            // Alert for servers with large number of users
            if (Context.Guild.Users.Count >= 100) {
                EmbedBuilder usersEmbed = new EmbedBuilder();
                usersEmbed.WithColor(embedColor);
                usersEmbed.Description = "This operation will take some time, when it's finished I'll post the results here";
                await ReplyAsync(null, false, usersEmbed.Build());
            }
            int dmsSuccess = 0, dmsError = 0;
            // Loop through all (cached) server users
            foreach (SocketGuildUser serverUser in Context.Guild.Users) {
                try {
                    // Exclude message author and bots
                    if (!Context.User.Id.Equals(serverUser.Id) && !serverUser.IsBot) {
                        await serverUser.SendMessageAsync(_message);
                        dmsSuccess++;
                    }
                } catch (HttpException) {
                    // Counter of users which DM could not be sent
                    dmsError++;
                }
            }
            // Reply with the embed
            replyEmbed.AddField($"DMs sent", dmsSuccess, true);
            replyEmbed.AddField($"Unsent DMs", dmsError, true);
            await ReplyAsync(null, false, replyEmbed.Build());
        }

        [Command("random")]
        [Alias("rand")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Generate random data for the information provided")]
        public async Task randomData([Remainder][Summary("Data to randomize")] String _message) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(new Color(144, 164, 174));
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            try {
                if (_message.Contains(",")) {
                    // Get all parameters to randomize
                    String[] arrayData = _message.Split(",");
                    replyEmbed.Description = $"{ arrayData[new Random().Next(0, arrayData.Length)].Trim() } was the chosen!";
                }
            } catch (ArgumentOutOfRangeException) {
                replyEmbed.Description = "I'm sorry, but an error occurred during the random operation";
            }
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build());
        }
    }
}
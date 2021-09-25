using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;

namespace DenverHelper.Modules
{
    [Summary("General Discord Commands")]
    public class GeneralComms : ModuleBase<SocketCommandContext>
    {
        // Getting all services through constructor param with AddSingleton()
        private readonly CommandService commandService;
        private GeneralComms(CommandService _commandService) => this.commandService = _commandService;

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
            if (currentConn.Equals(ConnectionState.Connected))
                embedBuilder.AddField("Latency", $"{ Context.Client.Latency } ms", true);
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
            String strQuestion = _fullArgs.Split('|')[0].Trim();
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
                    pollEmbed.AddField("Yes", emojiCodes[0], true);
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
                            pollEmbed.AddField(option.Split('(')[0].Trim(), emojiCode, true);
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
                pollEmbed.AddField("Yes", emojiCodes[0], true);
                pollEmbed.AddField("No", emojiCodes[1], true);
                // Create the poll in the channel
                IUserMessage sent = await ReplyAsync(null, false, pollEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
                // Add reactions to the poll.
                await sent.AddReactionsAsync(emojiCodes.ToArray());
            }
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
                String[] arrayData = null;
                char[] splitOpts = new char[] { ',', '|', '/' };
                // Get all parameters to randomize
                if (_message.IndexOfAny(splitOpts) != -1) 
                    arrayData = _message.Split(_message[_message.IndexOfAny(splitOpts)]);
                replyEmbed.Description = $"`{ arrayData[new Random().Next(0, arrayData.Length)].Trim() }` was the chosen!";
            } catch (ArgumentOutOfRangeException) {
                replyEmbed.Description = "I'm sorry, but an error occurred during the random operation";
            }
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }
    }
}
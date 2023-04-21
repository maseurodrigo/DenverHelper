using System;
using System.Threading.Tasks;
using DenverHelper.Data;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Whetstone.ChatGPT;

namespace DenverHelper.Services
{
    public class CommHandler
    {
        // Discord default vars.
        public static DiscordSocketClient discordClient { get; set; }
        public static CommandService discordCommands { get; set; }
        public static InteractiveService discordInteractive { get; set; }
        public static IServiceProvider discordService { get; set; }
        public static IChatGPTClient chatGptClient { get; set; }
        public static BotData botData { get; set; }
        public static GiveawayList giveawayList { get; set; }

        public CommHandler(DiscordSocketClient _discordClient, CommandService _discordCommands, InteractiveService _discordInteractive,
            IServiceProvider _discordService, IChatGPTClient _chatGptClient, BotData _botData, GiveawayList _giveawayList) {
            discordClient = _discordClient;
            discordCommands = _discordCommands;
            discordInteractive = _discordInteractive;
            discordService = _discordService;
            chatGptClient = _chatGptClient;
            botData = _botData;
            giveawayList = _giveawayList;
            // DiscordSocketClient functions
            discordClient.MessageReceived += client_NewCommandReceived;
            discordClient.Log += botLogEvents;
        }

        private async Task client_NewCommandReceived(SocketMessage message) {
            // Block commands through DMs
            if (message.Channel is SocketDMChannel) { return; }
            // Block messages from verified bots
            if (message.Author.IsBot) { return; }
            else {
                int argPos = 0;
                SocketUserMessage discMessage = message as SocketUserMessage;
                // Detect whether the entered text will be associated with a command
                if (discMessage.HasStringPrefix(botData.BotPrefix, ref argPos)) {
                    SocketCommandContext mssgContext = new SocketCommandContext(discordClient, discMessage);
                    await discordCommands.ExecuteAsync(mssgContext, argPos, discordService);
                }
            }
        }

        private async Task botLogEvents(LogMessage arg) {
            await Task.Factory.StartNew(() => { Console.WriteLine(arg.ToString()); });
        }
    }
}
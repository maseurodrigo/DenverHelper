using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordDenver.Data;
using DiscordDenver.Data.MySQL;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DiscordDenver
{
    class Program
    {
        public static void Main(string[] args) => new Program().BotKeepAsync().GetAwaiter().GetResult();

        // Discord default vars.
        private DiscordSocketClient discordClient;
        private CommandService discordCommands;
        private InteractiveService discordInteractive;
        private IServiceProvider discordService;
        private APIsData localAPIsData;
        private MySQLConnect localMySQLConnect;

        public async Task BotKeepAsync() {
            try {
                this.localAPIsData = JsonConvert.DeserializeObject<APIsData>(File.ReadAllText(@"APIsData.json"));
                this.localMySQLConnect = new MySQLConnect(localAPIsData.MySQLConnect.ServerID, localAPIsData.MySQLConnect.UserID, 
                    localAPIsData.MySQLConnect.UserPW, localAPIsData.MySQLConnect.Database);
                await localMySQLConnect.newConnection(); // GCloud new connection
                this.discordClient = new DiscordSocketClient(new DiscordSocketConfig() { LogLevel = LogSeverity.Info, AlwaysDownloadUsers = true });
                this.discordCommands = new CommandService(new CommandServiceConfig() { LogLevel = LogSeverity.Info, CaseSensitiveCommands = false });
                this.discordInteractive = new InteractiveService(discordClient);
                this.discordService = new ServiceCollection()
                    .AddSingleton(discordClient)
                    .AddSingleton(discordCommands)
                    .AddSingleton(discordInteractive)
                    .AddSingleton(localMySQLConnect)
                    .AddSingleton(localAPIsData)
                    .BuildServiceProvider();
                await this.discordCommands.AddModulesAsync(Assembly.GetEntryAssembly(), discordService);
                // Create connection between bot and discord server (API)
                await discordClient.LoginAsync(TokenType.Bot, localAPIsData.BotToken);
                await discordClient.StartAsync();
                await discordClient.SetStatusAsync(UserStatus.Online); // Setting online status
                await discordClient.SetGameAsync($"{ localAPIsData.BotPrefix }help", null, ActivityType.Listening); // Listening status
                // Enable discord bot feedback regarding bot commands in text channels
                discordClient.MessageReceived += client_NewCommandReceived;
                discordClient.Log += botLogEvents;
                // Block task untill the program is closed
                await Task.Delay(-1);
            } catch (FileNotFoundException excep) {
                Console.WriteLine(excep.Message);
            }
        }

        private async Task CurrentDomain_ProcessExit(object sender, EventArgs e) {
            // Disconnection bot from the discord server (API)
            await discordClient.StopAsync();
            await discordClient.LogoutAsync();
        }

        private Task botLogEvents(LogMessage arg) {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        private async Task client_NewCommandReceived(SocketMessage message) {
            // Block commands through DMs
            if (message.Channel is SocketDMChannel) return;
            // Block messages from verified bots
            if (message.Author.IsBot) return;
            else {
                int argPos = 0;
                SocketUserMessage discMessage = message as SocketUserMessage;
                // Detect whether the entered text will be associated with a command
                if (discMessage.HasStringPrefix(localAPIsData.BotPrefix, ref argPos)) {
                    SocketCommandContext mssgContext = new SocketCommandContext(discordClient, discMessage);
                    await discordCommands.ExecuteAsync(mssgContext, argPos, discordService);
                }
            }
        }
    }
}
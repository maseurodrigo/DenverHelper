using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DenverHelper.Data;
using DenverHelper.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Whetstone.ChatGPT;

namespace DenverHelper
{
    class Program
    {
        public static async Task Main(string[] args) => await BotKeepAsync();

        public static async Task BotKeepAsync() {
            // Discord client configurations
            DiscordSocketClient discordClient = new DiscordSocketClient(new DiscordSocketConfig() {
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info
            });
            ServiceCollection discordService = new ServiceCollection();
            CommandServiceConfig serviceConfig = new CommandServiceConfig() {
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false,
                IgnoreExtraArgs = true,
                LogLevel = LogSeverity.Info
            };
            try {
                // Load up from JSON file all bot required data
                BotData botData = JsonConvert.DeserializeObject<BotData>(File.ReadAllText(@"BotData.json"));
                // Setup ChatGptClient with API key
                IChatGPTClient chatGptClient = new ChatGPTClient(botData.CGPTKey);
                GiveawayList giveawayList = new GiveawayList();
                giveawayList.Giveaway = new Dictionary<ulong, GiveawayData>();
                discordService
                    .AddSingleton(discordClient)
                    .AddSingleton(new CommandService(serviceConfig))
                    .AddSingleton(new InteractiveService(discordClient))
                    .AddSingleton<BotService>()
                    .AddSingleton(chatGptClient)
                    .AddSingleton(botData)
                    .AddSingleton(giveawayList)
                    .AddSingleton<CommHandler>();
            } catch (FileNotFoundException excep) { Console.WriteLine(excep.Message); }
            ServiceProvider serviceProvider = discordService.BuildServiceProvider();
            serviceProvider.GetRequiredService<CommHandler>();
            // Start discord bot connection asynchronous
            await serviceProvider.GetRequiredService<BotService>().StartAsync();
            // Block task untill the program is closed
            await Task.Delay(-1);
        }
    }
}
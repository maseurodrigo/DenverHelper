﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using DenverHelper.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Whetstone.ChatGPT;

namespace DenverHelper.Services
{
    public class BotService
    {
        public static IServiceProvider discordService { get; set; }
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService discordCommands;
        public static IChatGPTClient chatGptClient;
        private readonly BotData botData;

        public BotService(DiscordSocketClient _discordClient, CommandService _discordCommands, IServiceProvider _discordService, IChatGPTClient _chatGptClient, BotData _botData) {
            discordService = _discordService;
            discordClient = _discordClient;
            discordCommands = _discordCommands;
            chatGptClient = _chatGptClient;
            botData = _botData;
        }

        // Create connection between bot and discord server
        public async Task<ConnectionState> StartAsync() {
            // Load all commands modules found (System.AppDomain.ExecuteAssembly)
            await discordCommands.AddModulesAsync(Assembly.GetEntryAssembly(), discordService);
            await discordClient.LoginAsync(TokenType.Bot, botData.BotToken);
            await discordClient.StartAsync();
            // Setting online status
            await discordClient.SetStatusAsync(UserStatus.Online);
            // Listening status
            await discordClient.SetGameAsync($"{ botData.BotPrefix }help", null, ActivityType.Listening);
            return discordClient.ConnectionState;
        }
    }
}
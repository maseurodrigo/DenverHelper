using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using DiscordDenver.Data;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Victoria;
using Victoria.EventArgs;

namespace DiscordDenver.Services
{
    public class CommHandler
    {
        // Discord default vars.
        public static DiscordSocketClient discordClient { get; set; }
        public static CommandService discordCommands { get; set; }
        public static InteractiveService discordInteractive { get; set; }
        public static IServiceProvider discordService { get; set; }
        public static LavaNode lavaNode { get; set; }
        public static BotData botData { get; set; }
        private ConcurrentDictionary<ulong, CancellationTokenSource> lavaPlayerDisconnects { get; set; }

        public CommHandler(DiscordSocketClient _discordClient, CommandService _discordCommands, InteractiveService _discordInteractive,
            IServiceProvider _discordService, LavaNode _lavaNode, BotData _botData) {
            discordClient = _discordClient;
            discordCommands = _discordCommands;
            discordInteractive = _discordInteractive;
            discordService = _discordService;
            lavaNode = _lavaNode;
            botData = _botData;
            lavaPlayerDisconnects = new ConcurrentDictionary<ulong, CancellationTokenSource>();
            discordClient.Ready += client_Ready;
            // Enable discord bot feedback regarding bot commands in text channels
            discordClient.MessageReceived += client_NewCommandReceived;
            discordClient.UserVoiceStateUpdated += client_UserVoiceStateUpdated;
            discordClient.Log += botLogEvents;
            lavaNode.OnTrackStarted += lavaClient_OnTrackStarted;
            lavaNode.OnTrackEnded += lavaClient_OnTrackEnded;
        }

        private async Task<bool> client_Ready() {
            // When discordClient its ready connect victoria client
            if (!lavaNode.IsConnected) await lavaNode.ConnectAsync();
            return lavaNode.IsConnected;
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
                if (discMessage.HasStringPrefix(botData.BotPrefix, ref argPos)) {
                    SocketCommandContext mssgContext = new SocketCommandContext(discordClient, discMessage);
                    await discordCommands.ExecuteAsync(mssgContext, argPos, discordService);
                }
            }
        }

        private async Task client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3) {
            // LavaNode leaves vchannel when last user leaves that specific channel
            if (lavaNode.IsConnected 
                && arg2.VoiceChannel.Users.Count.Equals(1) 
                && !arg1.Id.Equals(discordClient.CurrentUser.Id)) 
                await lavaNode.LeaveAsync(lavaNode.GetPlayer(arg2.VoiceChannel.Guild).VoiceChannel);
        }

        private Task botLogEvents(LogMessage arg) {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        private Task lavaClient_OnTrackStarted(TrackStartEventArgs arg) {
            if (!lavaPlayerDisconnects.TryGetValue(arg.Player.VoiceChannel.Id, out var value)) 
                return Task.CompletedTask;
            if (value.IsCancellationRequested) return Task.CompletedTask;
            value.Cancel(true);
            return Task.CompletedTask;
        }

        private async Task lavaClient_OnTrackEnded(TrackEndedEventArgs arg) {
            LavaPlayer player = arg.Player;
            // If queue end disconnect (5s)
            if (!player.Queue.TryDequeue(out var queueable)) {
                _ = InitiateDisconnectAsync(arg.Player, TimeSpan.FromSeconds(10));
                return;
            }
            // If next item its an invalid track
            if (!(queueable is LavaTrack track)) {
                await player.SkipAsync();
                return;
            }
            await arg.Player.PlayAsync(track);
            EmbedBuilder embedTrack = new EmbedBuilder();
            embedTrack.Color = new Color(244, 67, 54);
            embedTrack.Title = "Now Playing...";
            embedTrack.AddField("Name", track.Title, true);
            embedTrack.AddField("Author", track.Author, false);
            embedTrack.AddField("Duration", track.Duration, true);
            embedTrack.ThumbnailUrl = await track.FetchArtworkAsync();
            await arg.Player.TextChannel.SendMessageAsync(null, false, embedTrack.Build());
        }

        private async Task InitiateDisconnectAsync(LavaPlayer player, TimeSpan timeSpan) {
            if (!lavaPlayerDisconnects.TryGetValue(player.VoiceChannel.Id, out var value)) {
                value = new CancellationTokenSource();
                lavaPlayerDisconnects.TryAdd(player.VoiceChannel.Id, value);
            } else if (value.IsCancellationRequested) {
                // New track/playlist added to queue
                lavaPlayerDisconnects.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), value);
                value = lavaPlayerDisconnects[player.VoiceChannel.Id];
            }
            // TimeSpan before disconnects
            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled) return;
            await lavaNode.LeaveAsync(player.VoiceChannel);
        }
    }
}
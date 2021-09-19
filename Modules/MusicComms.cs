using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Discord.Commands;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace DiscordDenver.Modules
{
    [Summary("Music Discord Commands")]
    public class MusicComms : ModuleBase<SocketCommandContext>
    {
        // Getting all services through constructor param with AddSingleton()
        private readonly LavaNode lavaNode;
        private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);
        private static readonly Color embedsColor = new Color(244, 67, 54);
        public MusicComms(LavaNode _lavaNode) => this.lavaNode = _lavaNode;

        [Command("lavanode")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get the current status of the lavanode connection")]
        public async Task getConnection() {
            // Retrieve bot connection status
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.Description = $"LavaNode it's `{ new String(lavaNode.IsConnected ? "connected" : "not connected") }`";
            // Reply with the embed
            await ReplyAsync(null, false, embedBuilder.Build());
        }

        [Command("play")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Connect the bot to the current voice channel and start a music or playlist")]
        public async Task playAsync([Remainder][Summary("Youtube query")] String _ytQuery) {
            // Invalid URLs
            if (_ytQuery.StartsWith("https") || _ytQuery.StartsWith("http")) {
                if (!HttpUtility.ParseQueryString(new Uri(_ytQuery).Query).HasKeys()) { return; }
            }
            // Lava client it's not present on any voice channel
            if (!lavaNode.HasPlayer(Context.Guild)) {
                // User it's not present on any voice channel
                IVoiceState voiceState = Context.User as IVoiceState;
                if (voiceState.VoiceChannel is null) {
                    EmbedBuilder userVChannel = new EmbedBuilder();
                    userVChannel.Color = embedsColor;
                    userVChannel.Description = $"I can't play anything in narnia, please join a voice channel";
                    await ReplyAsync(null, false, userVChannel.Build(), null, null, new MessageReference(Context.Message.Id));
                    return;
                }
                try {
                    await lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                } catch (Exception excep) {
                    await ReplyAsync(excep.Message);
                }
            }
            String[] _ytQueries = _ytQuery.Split(' ');
            foreach (String query in _ytQueries) {
                SearchResponse searchResp = await lavaNode.SearchYouTubeAsync(query);
                // Couldnt find anything for the query param given
                if (searchResp.Status is SearchStatus.LoadFailed || searchResp.Status is SearchStatus.NoMatches) {
                    EmbedBuilder noMatches = new EmbedBuilder();
                    noMatches.Color = embedsColor;
                    noMatches.Description = $"I didn't find anything about `{ query }` on mars but I can look it up in narnia";
                    await ReplyAsync(null, false, noMatches.Build(), null, null, new MessageReference(Context.Message.Id));
                    return;
                }
                // Assign LavaPlayer to the current discord server
                LavaPlayer currentPlayer = lavaNode.GetPlayer(Context.Guild);
                // If LavaPlayer its active (playing)
                if (currentPlayer.PlayerState is PlayerState.Playing || currentPlayer.PlayerState is PlayerState.Paused) {
                    if (!String.IsNullOrWhiteSpace(searchResp.Playlist.Name)) {
                        foreach (var track in searchResp.Tracks) currentPlayer.Queue.Enqueue(track);
                        // Enqueued track embed
                        EmbedBuilder inQueue = new EmbedBuilder();
                        inQueue.Color = embedsColor;
                        inQueue.Description = $"Currently { searchResp.Tracks.Count } tracks in queue";
                        await ReplyAsync(null, false, inQueue.Build());
                    } else {
                        // With track name -> elemAt(0)
                        LavaTrack track = searchResp.Tracks.ElementAt(0);
                        currentPlayer.Queue.Enqueue(track);
                        // Enqueued track embed
                        EmbedBuilder enQueued = new EmbedBuilder();
                        enQueued.Color = embedsColor;
                        enQueued.Description = $"Enqueued: `{ track.Title }`";
                        await ReplyAsync(null, false, enQueued.Build());
                    }
                } else {
                    // When LavaPlayer its idle trigger a PlayAsync
                    LavaTrack track = searchResp.Tracks.ElementAt(0);
                    await currentPlayer.PlayAsync(track);
                    // Next track embed details
                    EmbedBuilder embedTrack = new EmbedBuilder();
                    embedTrack.Color = embedsColor;
                    embedTrack.Title = "Now Playing...";
                    embedTrack.AddField("Name", track.Title, true);
                    embedTrack.AddField("Author", track.Author, false);
                    embedTrack.AddField("Duration", track.Duration, true);
                    embedTrack.ThumbnailUrl = await track.FetchArtworkAsync();
                    await ReplyAsync(null, false, embedTrack.Build(), null, null, new MessageReference(Context.Message.Id));
                }
            }
        }

        /*[Command("skip")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Skip to next track in queue")]
        public async Task skipNextTrack() {
            // User nor bot it's not present on any voice channel
            IVoiceState voiceState = Context.User as IVoiceState;
            if (voiceState.VoiceChannel is null || !lavaNode.HasPlayer(Context.Guild)) { return; }
            LavaPlayer currentPlayer = lavaNode.GetPlayer(Context.Guild);
            // Current user in a different vchannel than bot
            if (!voiceState.VoiceChannel.Equals(currentPlayer.VoiceChannel)) { return; }
            // No next track queued
            if (currentPlayer.Queue.Count.Equals(0)) { return; }
            await currentPlayer.SkipAsync(TimeSpan.FromSeconds(2));
            // Next track embed details
            LavaTrack currenttrack = currentPlayer.Track;
            EmbedBuilder embedTrack = new EmbedBuilder();
            embedTrack.Color = embedsColor;
            embedTrack.Title = "Playing now...";
            embedTrack.AddField("Name", currenttrack.Title, true);
            embedTrack.AddField("Author", currenttrack.Author, false);
            embedTrack.AddField("Duration", currenttrack.Duration, true);
            embedTrack.ThumbnailUrl = await currenttrack.FetchArtworkAsync();
            await ReplyAsync(null, false, embedTrack.Build(), null, null, new MessageReference(Context.Message.Id));
        }*/

        [Command("pause")]
        [Summary("Pause current discord player")]
        public async Task pausePlayer() {
            // User nor bot it's not present on any voice channel
            IVoiceState voiceState = Context.User as IVoiceState;
            if (voiceState.VoiceChannel is null || !lavaNode.HasPlayer(Context.Guild)) { return; }
            LavaPlayer currentPlayer = lavaNode.GetPlayer(Context.Guild);
            // Current user in a different vchannel than bot
            if (!voiceState.VoiceChannel.Equals(currentPlayer.VoiceChannel)) { return; }
            // Player its already paused/stopped
            if (currentPlayer.PlayerState.Equals(PlayerState.Paused) || 
                currentPlayer.PlayerState.Equals(PlayerState.Stopped)) { return; }
            await currentPlayer.PauseAsync();
        }

        [Command("resume")]
        [Summary("Resume current discord player")]
        public async Task resumePlayer() {
            // User nor bot it's not present on any voice channel
            IVoiceState voiceState = Context.User as IVoiceState;
            if (voiceState.VoiceChannel is null || !lavaNode.HasPlayer(Context.Guild)) { return; }
            LavaPlayer currentPlayer = lavaNode.GetPlayer(Context.Guild);
            // Current user in a different vchannel than bot
            if (!voiceState.VoiceChannel.Equals(currentPlayer.VoiceChannel)) { return; }
            // Player its already paused/stopped
            if (currentPlayer.PlayerState.Equals(PlayerState.Playing)) { return; }
            await currentPlayer.ResumeAsync();
        }

        [Command("stop")]
        [Summary("Stop discord player and finish the current queue")]
        public async Task stopPlayer() {
            // User nor bot it's not present on any voice channel
            IVoiceState voiceState = Context.User as IVoiceState;
            if (voiceState.VoiceChannel is null || !lavaNode.HasPlayer(Context.Guild)) { return; }
            LavaPlayer currentPlayer = lavaNode.GetPlayer(Context.Guild);
            // Current user in a different vchannel than bot
            if (!voiceState.VoiceChannel.Equals(currentPlayer.VoiceChannel)) { return; }
            // Player its already stopped
            if (currentPlayer.PlayerState.Equals(PlayerState.Stopped)) { return; }
            await currentPlayer.StopAsync();
            currentPlayer.Queue.Clear();
        }

        [Command("queue")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("List all the tracks that are currently on queue")]
        public async Task listQueue() {
            // User nor bot it's not present on any voice channel
            IVoiceState voiceState = Context.User as IVoiceState;
            if (voiceState.VoiceChannel is null || !lavaNode.HasPlayer(Context.Guild)) { return; }
            // Current user in a different vchannel than bot
            LavaPlayer currentPlayer = lavaNode.GetPlayer(Context.Guild);
            if (!voiceState.VoiceChannel.Equals(currentPlayer.VoiceChannel)) { return; }
            // Queue tracks list embed
            EmbedBuilder embedQueueTracks = new EmbedBuilder();
            embedQueueTracks.Color = embedsColor;
            embedQueueTracks.Title = $"Tracks in queue: `{ currentPlayer.Queue.Count }`";
            // Loop through all track in queue
            foreach (LavaTrack track in currentPlayer.Queue) 
                embedQueueTracks.AddField(track.Title, track.Author, false);
            await ReplyAsync(null, false, embedQueueTracks.Build(), null, null, new MessageReference(Context.Message.Id));
        }

        [Command("lyrics")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Searching for current track lyrics")]
        public async Task getTrackLyrics() {
            // Current track lyrics embed
            EmbedBuilder embedLyrics = new EmbedBuilder();
            embedLyrics.Color = embedsColor;
            try {
                if (!lavaNode.TryGetPlayer(Context.Guild, out var currentPlayer)) { return; }
                if (!currentPlayer.PlayerState.Equals(PlayerState.Playing)) { return; }
                String lyrics = await currentPlayer.Track.FetchLyricsFromGeniusAsync();
                if (String.IsNullOrWhiteSpace(lyrics)) { return; }
                String[] splitLyrics = lyrics.Split('\n');
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var line in splitLyrics) {
                    if (Range.Contains(stringBuilder.Length)) {
                        embedLyrics.Title = currentPlayer.Track.Title;
                        embedLyrics.Description = $"```{stringBuilder}```";
                        await ReplyAsync(null, false, embedLyrics.Build(), null, null, new MessageReference(Context.Message.Id));
                        stringBuilder.Clear();
                    } else stringBuilder.AppendLine(line);
                }
                embedLyrics.Title = currentPlayer.Track.Title;
                embedLyrics.Description = $"```{stringBuilder}```";
            } catch (HttpRequestException) {
                embedLyrics.Description = "Don't ask me how, but i didn't find anything for this track";
            } catch (ArgumentOutOfRangeException) {
                embedLyrics.Description = "Sorry boss, but the lyrics for this track are a little weird and i can't present it";
            } catch (IndexOutOfRangeException excep) {
                await ReplyAsync(excep.Message);
            } finally {
                await ReplyAsync(null, false, embedLyrics.Build(), null, null, new MessageReference(Context.Message.Id));
            }
        }
    }
}
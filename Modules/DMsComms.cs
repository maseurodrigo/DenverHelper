using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Discord.Addons.Interactive;

namespace DenverHelper.Modules
{
    [Summary("DMs Discord Commands")]
    public class DMsComms : InteractiveBase
    {
        private static readonly Color embedsColor = new Color(220, 231, 117);

        [Command("dmrole")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Send a private message for users with a given role assigned")]
        public async Task dmRole(IRole _role, [Remainder][Summary("DM message")] String _message) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedsColor);
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
                } catch (HttpException excep) {
                    // Counter of users which DM could not be sent
                    if (excep.DiscordCode.Equals(50007)) dmsError++;
                }
            }
            // Reply with the embed
            replyEmbed.Description = $"`{ dmsSuccess }` DM(s) were sent and `{ dmsError }` were not!";
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }

        [Command("dmall")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Send a private message to all server members")]
        public async Task dmAll([Remainder][Summary("DM message")] String _message) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedsColor);
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            // Alert for servers with large number of users
            if (Context.Guild.Users.Count >= 100) {
                EmbedBuilder usersEmbed = new EmbedBuilder();
                usersEmbed.WithColor(embedsColor);
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
                } catch (HttpException excep) {
                    // Counter of users which DM could not be sent
                    if (excep.DiscordCode.Equals(50007)) dmsError++;
                }
            }
            // Reply with the embed
            replyEmbed.AddField($"DMs sent", $"`{ dmsSuccess }`", true);
            replyEmbed.AddField($"Unsent DMs", $"`{ dmsError }`", true);
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }
    }
}
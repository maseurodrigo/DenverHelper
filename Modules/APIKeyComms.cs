using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DenverHelper.Data;
using DenverHelper.Data.MySQL;

namespace DenverHelper.Modules
{
    [Summary("API Key Discord Commands")]
    public class APIKeyComms : ModuleBase<SocketCommandContext>
    {
        // Getting all services through constructor param with AddSingleton()
        //private BotData botData { get; }
        //private APIKeyComms(BotData _BotData) => botData = _BotData;

        /*[Command("addkey")]
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
        }*/

        /*[Command("delkey")]
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
        }*/
    }
}
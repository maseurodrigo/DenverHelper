using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DenverHelper.Data;
using DenverHelper.Data.MySQL;

namespace DenverHelper.Modules
{
    [Summary("Server Discord Commands")]
    public class ServerComms : ModuleBase<SocketCommandContext>
    {
        // Getting all services through constructor param with AddSingleton()
        //private BotData botData { get; }
        //private static readonly Color embedsColor = new Color(149, 117, 205);
        //private ServerComms(BotData _BotData) => botData = _BotData;

        /*[Command("comms")]
        [Alias("servercomms", "server_comms", "totalcomms", "total_comms")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get all comms added to the current server")]
        public async Task getTotalServerComms() {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedsColor);
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            int totalComms = await MySQLComms.getTotalComms(conn, Context.Guild.Id);
            replyEmbed.Description = $"There are `{ totalComms }` commands for this server";
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }*/

        /*[Command("comm")]
        [Alias("getcomm", "get_comm", "datacomm", "data_comm")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Get the associated data for this server from the given comm")]
        public async Task getDataComm([Summary("Command")] String _comm) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedsColor);
            // Trigger typing state on current channel
            await Context.Channel.TriggerTypingAsync();
            MySQLConnect conn = new MySQLConnect(botData);
            if (await MySQLComms.checkCommExists(conn, Context.Guild.Id, _comm.Trim())) {
                String dataComm = await MySQLComms.getCommData(conn, Context.Guild.Id, _comm.Trim());
                if (Uri.IsWellFormedUriString(dataComm, UriKind.Absolute)) {
                    replyEmbed.WithTitle(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_comm.Trim().ToLower()));
                    replyEmbed.WithDescription(dataComm);
                    replyEmbed.WithImageUrl(dataComm);
                } else replyEmbed.AddField(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_comm.Trim().ToLower()), dataComm);
            } else {
                // Get a list of similar comms found
                Dictionary<String, String> allCommands = await MySQLComms.getCommsList(conn, Context.Guild.Id, _comm.Trim());
                foreach (KeyValuePair<String, String> command in allCommands) {
                    // Get the command Summary attribute information
                    replyEmbed.AddField(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(command.Key.ToLower()), command.Value);
                }
            }
            // Close local connection
            await conn.closeConnection();
            // Reply with the embed
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }*/

        /*[Command("addcomm")]
        [Alias("add_comm", "newcomm", "new_comm")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Set a new comm to be assigned to the current server")]
        public async Task setNewComm([Summary("Command")] String _comm, [Remainder][Summary("Command data")] String _commData) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedsColor);
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
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }*/

        /*[Command("delcomm")]
        [Alias("removecomm", "remove_comm", "deletecomm", "delete_comm")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Delete comm thats assigned to the current server")]
        public async Task removeComm([Summary("Command")] String _comm) {
            // Embed layout reply
            EmbedBuilder replyEmbed = new EmbedBuilder();
            replyEmbed.WithColor(embedsColor);
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
            await ReplyAsync(null, false, replyEmbed.Build(), null, null, new MessageReference(Context.Message.Id));
        }*/
    }
}
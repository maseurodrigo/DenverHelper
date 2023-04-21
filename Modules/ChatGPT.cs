using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Addons.Interactive;
using Whetstone.ChatGPT;
using Whetstone.ChatGPT.Models;

namespace DenverHelper.Modules
{
    [Summary("ChatGPT Discord Integration")]
    public class ChatGPT : InteractiveBase
    {
        private IChatGPTClient chatGptClient { get; }
        private ChatGPT(IChatGPTClient _chatGptClient) { chatGptClient = _chatGptClient; }
        
        [Command("cgpt")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Q&A with an AI bot")]
        public async Task startCGPT([Remainder][Summary("AI Query")] String _message) {
            try {
                EmbedBuilder embedBuilder = new EmbedBuilder();
                // ChatGPTCompletion request constructor
                ChatGPTCompletionRequest gptRequest = new ChatGPTCompletionRequest { Model = ChatGPT35Models.Davinci003, MaxTokens = 120, Prompt = _message };
                ChatGPTCompletionResponse response = await chatGptClient.CreateCompletionAsync(gptRequest);
                if (response is not null) {
                    embedBuilder.AddField(_message, Format.Italics(response.GetCompletionText().Trim()));
                    // Reply with the embed
                    await ReplyAsync(null, false, embedBuilder.Build(), null, null, new MessageReference(Context.Message.Id));
                }
            } catch(ChatGPTException excep) { Console.WriteLine(excep.Message); }
        }
    }
}

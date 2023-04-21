using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DenverHelper.Data
{
    public class BotData {
        [JsonProperty("DiscordBotPrefix")] public String BotPrefix { get; set; }
        [JsonProperty("DiscordBotToken")] public String BotToken { get; set; }
        [JsonProperty("ChatGPTApiKey")] public String CGPTKey { get; set; }
        [JsonProperty("MySQLConnect")] public MySQLConnectData MySQLConnect { get; set; }
    }

    public class MySQLConnectData {
        [JsonProperty("Server")] public String Server { get; set; }
        [JsonProperty("UserName")] public String UserName { get; set; }
        [JsonProperty("UserPWord")] public String UserPWord { get; set; }
        [JsonProperty("Database")] public String Database { get; set; }
    }

    public class GiveawayList { public Dictionary<ulong, GiveawayData> Giveaway { get; set; } }

    public class GiveawayData {
        public String Title;
        public int Params, Timer, Winners;
        public bool Active;
        public Dictionary<ulong, String> ListUsers = new Dictionary<ulong, String>();
    }
}
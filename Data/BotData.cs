using System;
using Newtonsoft.Json;

namespace DiscordDenver.Data
{
    public class BotData {
        [JsonProperty("DiscordBotPrefix")] public String BotPrefix { get; set; }
        [JsonProperty("DiscordBotToken")] public String BotToken { get; set; }
        [JsonProperty("MySQLConnect")] public MySQLConnectData MySQLConnect { get; set; }
    }

    public class MySQLConnectData {
        [JsonProperty("Server")] public String ServerID { get; set; }
        [JsonProperty("UserID")] public String UserID { get; set; }
        [JsonProperty("UserPW")] public String UserPW { get; set; }
        [JsonProperty("Database")] public String Database { get; set; }
    }
}
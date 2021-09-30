using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DenverHelper.Data.JSON.Soccer
{
    public partial class PlayerClass {
        public static async Task<String> GetAPISoccerPlayers(String _RapidAPIKey, int? _idPlayer) {
            // Get and return data of a player's statistics through its ID
            RestClient restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/players/player/{ _idPlayer }");
            RestRequest reqst = new RestRequest(Method.GET);
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
            IRestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class Player { [JsonProperty("api")] public APIPlayer Api { get; set; } }

    public partial class APIPlayer {
        [JsonProperty("results")] public long Results { get; set; }
        [JsonProperty("players")] public List<PlayerData> Players { get; set; }
    }

    public partial class PlayerData {
        [JsonProperty("player_id", Required = Required.Always)] public int PlayerId { get; set; }
        [JsonProperty("player_name", Required = Required.Always)] public String PlayerName { get; set; }
        [JsonProperty("number", NullValueHandling = NullValueHandling.Ignore)] public int? Number { get; set; }
        [JsonProperty("position", Required = Required.Always)] public String Position { get; set; }
        [JsonProperty("age", Required = Required.Always)] public int Age { get; set; }
        [JsonProperty("nationality", Required = Required.Always)] public String Nationality { get; set; }
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)] public String Height { get; set; }
        [JsonProperty("weight", NullValueHandling = NullValueHandling.Ignore)] public String Weight { get; set; }
        [JsonProperty("rating", NullValueHandling = NullValueHandling.Ignore)] public String Rating { get; set; }
        [JsonProperty("team_name", NullValueHandling = NullValueHandling.Ignore)] public String TeamName { get; set; }
        [JsonProperty("goals")] public PlayerGoals Goals { get; set; }
        [JsonProperty("games")] public PlayerGames Games { get; set; }
    }

    public partial class PlayerGoals {[JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)] public int Total { get; set; } }

    public partial class PlayerGames { [JsonProperty("appearences", NullValueHandling = NullValueHandling.Ignore)] public int Appearences { get; set; } }

    public partial class PlayerGetData {
        public static Player FromJson(String json) => JsonConvert.DeserializeObject<Player>(json, Converter.Settings);
    }
}
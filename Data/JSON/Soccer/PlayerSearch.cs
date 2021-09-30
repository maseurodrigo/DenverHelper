using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DenverHelper.Data.JSON.Soccer
{
    public partial class PlayerSearchClass {
        public static async Task<String> GetAPISoccerPlayers(String _RapidAPIKey, String _playerName) {
            // Get and return data of a player through its name
            RestClient restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/players/search/{ _playerName }");
            RestRequest reqst = new RestRequest(Method.GET);
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
            IRestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class PlayerSearch { [JsonProperty("api")] public ApiPlayerSearch Api { get; set; } }

    public partial class ApiPlayerSearch {
        [JsonProperty("results")] public long Results { get; set; }
        [JsonProperty("players")] public List<PlayerSearchData> Players { get; set; }
    }

    public partial class PlayerSearchData {
        [JsonProperty("player_id", Required = Required.Always)] public int PlayerId { get; set; }
        [JsonProperty("player_name", Required = Required.Always)] public String PlayerName { get; set; }
        [JsonProperty("nationality", NullValueHandling = NullValueHandling.Ignore)] public String Nationality { get; set; }
    }

    public partial class PlayerSearchGetData {
        public static PlayerSearch FromJson(String json) => JsonConvert.DeserializeObject<PlayerSearch>(json, Converter.Settings);
    }
}
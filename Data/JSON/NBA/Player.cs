using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DenverHelper.Data.JSON.NBA
{
    public partial class PlayerClass {
        public static async Task<String> GetAPINBAPlayer(String _RapidAPIKey, String _playerName) {
            // Get and return NBA player data through its name
            RestClient restClient = new RestClient($"https://api-nba-v1.p.rapidapi.com/players/firstName/{ _playerName }");
            RestRequest reqst = new RestRequest(Method.GET);
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "api-nba-v1.p.rapidapi.com");
            IRestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class Player { [JsonProperty("api")] public APIPlayer Api { get; set; } }

    public partial class APIPlayer {
        [JsonProperty("status")] public long Status { get; set; }
        [JsonProperty("players")] public List<PlayerData> Players { get; set; }
    }

    public partial class PlayerData {
        [JsonProperty("firstName", Required = Required.Always)] public String FirstName { get; set; }
        [JsonProperty("lastName", NullValueHandling = NullValueHandling.Ignore)] public String LastName { get; set; }
        [JsonProperty("teamId", Required = Required.Always)] public int TeamId { get; set; }
        [JsonProperty("yearsPro", NullValueHandling = NullValueHandling.Ignore)] public int? YearsPro { get; set; }
        [JsonProperty("country")] public String Country { get; set; }
        [JsonProperty("playerId", Required = Required.Always)] public int PlayerId { get; set; }
        [JsonProperty("dateOfBirth")] public DateTimeOffset DateOfBirth { get; set; }
        [JsonProperty("startNba", NullValueHandling = NullValueHandling.Ignore)] public int? StartNba { get; set; }
        [JsonProperty("heightInMeters", NullValueHandling = NullValueHandling.Ignore)] public String HeightInMeters { get; set; }
        [JsonProperty("weightInKilograms", NullValueHandling = NullValueHandling.Ignore)] public String WeightInKilograms { get; set; }
    }

    public partial class PlayerGetData {
        public static Player FromJson(String json) => JsonConvert.DeserializeObject<Player>(json, Converter.Settings);
    }
}
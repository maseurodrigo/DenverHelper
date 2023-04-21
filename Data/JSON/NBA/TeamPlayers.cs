using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DenverHelper.Data.JSON.NBA
{
    public partial class TeamPlayersClass {
        public static async Task<String> GetNBATeamPlayers(String _RapidAPIKey, int? _teamID) {
            // Get and return NBA team players through its ID
            RestClient restClient = new RestClient($"https://api-nba-v1.p.rapidapi.com/players/teamId/{ _teamID }");
            RestRequest reqst = new RestRequest();
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "api-nba-v1.p.rapidapi.com");
            RestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class TeamPlayers { [JsonProperty("api")] public APITeamPlayers Api { get; set; } }

    public partial class APITeamPlayers {
        [JsonProperty("status")] public long Status { get; set; }
        [JsonProperty("players")] public List<TeamPlayersData> Players { get; set; }
    }

    public partial class TeamPlayersData {
        [JsonProperty("firstName", Required = Required.Always)] public String FirstName { get; set; }
        [JsonProperty("lastName", NullValueHandling = NullValueHandling.Ignore)] public String LastName { get; set; }
    }

    public partial class TeamPlayersGetData {
        public static TeamPlayers FromJson(String json) => JsonConvert.DeserializeObject<TeamPlayers>(json, Converter.Settings);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DenverHelper.Data.JSON.Soccer
{
    public partial class TeamSquadClass {
        public static async Task<String> GetAPISoccerTeams(String _RapidAPIKey, int? _idTeam, int? _season) {
            // Get and return team members of given team
            RestClient restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/players/squad/{ _idTeam }/{ _season }");
            RestRequest reqst = new RestRequest(Method.GET);
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
            IRestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class TeamSquad { [JsonProperty("api")] public ApiTeamSquad Api { get; set; } }

    public partial class ApiTeamSquad {
        [JsonProperty("results")] public long Results { get; set; }
        [JsonProperty("players")] public List<TeamSquadPlayer> Players { get; set; }
    }

    public partial class TeamSquadPlayer {
        [JsonProperty("player_id", Required = Required.Always)] public int PlayerId { get; set; }
        [JsonProperty("player_name", Required = Required.Always)] public String PlayerName { get; set; }
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)] public String Position { get; set; }
    }

    public partial class TeamSquadGetData {
        public static TeamSquad FromJson(String json) => JsonConvert.DeserializeObject<TeamSquad>(json, Converter.Settings);
    }
}
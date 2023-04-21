using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DenverHelper.Data.JSON.Soccer
{
    public partial class TeamClass {
        public static async Task<String> GetSoccerTeam(String _RapidAPIKey, String _teamName) {
            // Get and return data of a team through its name
            RestClient restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/teams/search/{ _teamName }");
            RestRequest reqst = new RestRequest();
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
            RestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class Team { [JsonProperty("api")] public ApiTeam Api { get; set; } }

    public partial class ApiTeam {
        [JsonProperty("results")] public long Results { get; set; }
        [JsonProperty("teams")] public List<TeamData> Teams { get; set; }
    }

    public partial class TeamData {
        [JsonProperty("team_id")] public int TeamId { get; set; }
        [JsonProperty("name", Required = Required.Always)] public String Name { get; set; }
        [JsonProperty("logo", NullValueHandling = NullValueHandling.Ignore)] public Uri Logo { get; set; }
    }

    public partial class TeamGetData {
        public static Team FromJson(String json) => JsonConvert.DeserializeObject<Team>(json, Converter.Settings);
    }
}
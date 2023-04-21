using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DenverHelper.Data.JSON.NBA
{
    public partial class TeamClass {
        public static async Task<String> GetNBATeam(String _RapidAPIKey, String _teamName) {
            // Get and return NBA team data
            RestClient restClient = new RestClient($"https://api-nba-v1.p.rapidapi.com/teams/nickName/{ _teamName }");
            RestRequest reqst = new RestRequest();
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "api-nba-v1.p.rapidapi.com");
            RestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class Team { [JsonProperty("api")] public APITeam Api { get; set; } }

    public partial class APITeam {
        [JsonProperty("status")] public long Status { get; set; }
        [JsonProperty("teams")] public List<TeamData> Teams { get; set; }
    }

    public partial class TeamData {
        [JsonProperty("city")] public String City { get; set; }
        [JsonProperty("fullName", Required = Required.Always)] public String FullName { get; set; }
        [JsonProperty("teamId", Required = Required.Always)] public int TeamId { get; set; }
        [JsonProperty("logo", NullValueHandling = NullValueHandling.Ignore)] public Uri Logo { get; set; }
        [JsonProperty("shortName", Required = Required.Always)] public String ShortName { get; set; }
        [JsonProperty("leagues")] public Leagues Leagues { get; set; }
    }

    public partial class Leagues {
        [JsonProperty("standard", NullValueHandling = NullValueHandling.Ignore)] public LeagueData Standard { get; set; }
        [JsonProperty("vegas", NullValueHandling = NullValueHandling.Ignore)] public LeagueData Vegas { get; set; }
        [JsonProperty("utah", NullValueHandling = NullValueHandling.Ignore)] public LeagueData Utah { get; set; }
        [JsonProperty("sacramento", NullValueHandling = NullValueHandling.Ignore)] public LeagueData Sacramento { get; set; }
    }

    public partial class LeagueData {
        [JsonProperty("confName")] public String ConfName { get; set; }
        [JsonProperty("divName")] public String DivName { get; set; }
    }

    public partial class TeamGetData {
        public static Team FromJson(String json) => JsonConvert.DeserializeObject<Team>(json, Converter.Settings);
    }
}
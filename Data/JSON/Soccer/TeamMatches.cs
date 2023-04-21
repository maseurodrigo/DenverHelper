using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DenverHelper.Data.JSON.Soccer
{
    public partial class NextMatchesClass {
        public static async Task<String> GetSoccerTeamMatches(String _RapidAPIKey, int? _idTeam, int? _numGames) {
            // Get and return next games of given team
            RestClient restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/fixtures/team/{ _idTeam }/next/{ _numGames }");
            RestRequest reqst = new RestRequest();
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
            RestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class LastMatchesClass {
        public static async Task<String> GetSoccerTeamMatches(String _RapidAPIKey, int? _idTeam, int? _numGames) {
            // Get and return last games of given team
            RestClient restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/fixtures/team/{ _idTeam }/last/{ _numGames }");
            RestRequest reqst = new RestRequest();
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
            RestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class TeamMatches { [JsonProperty("api")] public ApiMatches Api { get; set; } }

    public partial class ApiMatches {
        [JsonProperty("results")] public long Results { get; set; }
        [JsonProperty("fixtures")] public List<FixtureMatches> Fixtures { get; set; }
    }

    public partial class FixtureMatches {
        [JsonProperty("fixture_id")] public int FixtureId { get; set; }
        [JsonProperty("league", NullValueHandling = NullValueHandling.Ignore)] public LeagueMatches League { get; set; }
        [JsonProperty("venue", NullValueHandling = NullValueHandling.Ignore)] public String Venue { get; set; }
        [JsonProperty("homeTeam")] public TeamMatchesData HomeTeam { get; set; }
        [JsonProperty("awayTeam")] public TeamMatchesData AwayTeam { get; set; }
        [JsonProperty("goalsHomeTeam", NullValueHandling = NullValueHandling.Ignore)] public int? GoalsHomeTeam { get; set; }
        [JsonProperty("goalsAwayTeam", NullValueHandling = NullValueHandling.Ignore)] public int? GoalsAwayTeam { get; set; }
        [JsonProperty("score")] public ScoreMatches Score { get; set; }
    }

    public partial class TeamMatchesData {
        [JsonProperty("team_id", Required = Required.Always)] public int TeamId { get; set; }
        [JsonProperty("team_name", Required = Required.Always)] public String TeamName { get; set; }
    }

    public partial class LeagueMatches { [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)] public String Name { get; set; } }

    public partial class ScoreMatches { [JsonProperty("fulltime", NullValueHandling = NullValueHandling.Ignore)] public String Fulltime { get; set; } }

    public partial class MatchesGetData {
        public static TeamMatches FromJson(String json) => JsonConvert.DeserializeObject<TeamMatches>(json, Converter.Settings);
    }
}
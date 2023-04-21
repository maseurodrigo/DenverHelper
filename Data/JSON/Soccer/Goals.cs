using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DenverHelper.Data.JSON.Soccer
{
    public partial class GoalsClass {
        public static async Task<String> GetSoccerGoals(String _RapidAPIKey) {
            // Get and return the latest goals and highlights video from the given team name
            RestClient restClient = new RestClient("https://free-football-soccer-videos1.p.rapidapi.com/v1/");
            RestRequest reqst = new RestRequest();
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "free-football-soccer-videos1.p.rapidapi.com");
            RestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class GoalsData {
        [JsonProperty("title", Required = Required.Always)] public String Title { get; set; }
        [JsonProperty("embed", Required = Required.Always)] public String Embed { get; set; }
        [JsonProperty("thumbnail")] public Uri Thumbnail { get; set; }
        [JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)] public DateTimeOffset Date { get; set; }
        [JsonProperty("videos")] public List<Video> Videos { get; set; }
    }

    public partial class Video { [JsonProperty("embed", NullValueHandling = NullValueHandling.Ignore)] public String Embed { get; set; } }

    public partial class GoalsGetData {
        public static List<GoalsData> FromJson(String json) => JsonConvert.DeserializeObject<List<GoalsData>>(json, Converter.Settings);
    }
}
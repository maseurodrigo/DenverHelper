using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DenverHelper.Data.JSON.Soccer
{
    public partial class MatchTipsClass {
        public static async Task<String> GetSoccerMatchTips(String _RapidAPIKey, int? _idMatch) {
            // Get and return next game predict of given team
            RestClient restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/predictions/{ _idMatch }");
            RestRequest reqst = new RestRequest();
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
            RestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class MatchTips { [JsonProperty("api")] public ApiMatchTips Api { get; set; } }

    public partial class ApiMatchTips {
        [JsonProperty("results")] public long Results { get; set; }
        [JsonProperty("predictions")] public List<PredictionMatch> Predictions { get; set; }
    }

    public partial class PredictionMatch { [JsonProperty("advice", NullValueHandling = NullValueHandling.Ignore)] public String Advice { get; set; } }

    public partial class MatchTipsGetData {
        public static MatchTips FromJson(String json) => JsonConvert.DeserializeObject<MatchTips>(json, Converter.Settings);
    }
}
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DenverHelper.Data.JSON
{
    public partial class WeatherClass {
        public static async Task<String> GetWeatherData(String _WeatherAPIKey, String _city) {
            // Get and return weather data (given city name)
            RestClient restClient = new RestClient($"https://weatherapi-com.p.rapidapi.com/current.json?q={ _city }");
            RestRequest reqst = new RestRequest();
            reqst.AddHeader("x-rapidapi-host", "weatherapi-com.p.rapidapi.com");
            reqst.AddHeader("x-rapidapi-key", _WeatherAPIKey);
            RestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class Weather {
        [JsonProperty("location")] public TimezoneData Location { get; set; }
        [JsonProperty("current")] public WeatherData Current { get; set; }
    }

    public partial class WeatherData {
        [JsonProperty("temp_c", Required = Required.Always)] public long TempC { get; set; }
        [JsonProperty("condition", NullValueHandling = NullValueHandling.Ignore)] public Condition Condition { get; set; }
    }

    public partial class Condition {
        [JsonProperty("text")] public String Text { get; set; }
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)] public String Icon { get; set; }
    }

    public partial class TimezoneData {
        [JsonProperty("name", Required = Required.Always)] public String Name { get; set; }
        [JsonProperty("localtime", Required = Required.Always)] public DateTimeOffset Localtime { get; set; }
    }

    public partial class WeatherGetData {
        public static Weather FromJson(String json) => JsonConvert.DeserializeObject<Weather>(json, Converter.Settings);
    }
}
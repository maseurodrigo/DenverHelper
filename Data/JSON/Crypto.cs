using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DenverHelper.Data.JSON
{
    public partial class CryptoClass {
        public static async Task<String> GetCryptoData(String _CoinGeckoKey, String _crypto) {
            // Get and return crypto data by given coin name
            RestClient restClient = new RestClient($"https://coingecko.p.rapidapi.com/coins/{ _crypto }?market_data=true");
            RestRequest reqst = new RestRequest();
            reqst.AddHeader("x-rapidapi-host", "coingecko.p.rapidapi.com");
            reqst.AddHeader("x-rapidapi-key", _CoinGeckoKey);
            RestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }

    public partial class CryptoData {
        [JsonProperty("symbol", Required = Required.Always)] public String Symbol { get; set; }
        [JsonProperty("name", Required = Required.Always)] public String Name { get; set; }
        [JsonProperty("image")] public ImageData Image { get; set; }
        [JsonProperty("market_data")] public MarketData Market { get; set; }
    }

    public partial class ImageData { [JsonProperty("large", NullValueHandling = NullValueHandling.Ignore)] public Uri Large { get; set; } }

    public partial class MarketData {
        [JsonProperty("current_price")] public CurrentPrice CurrentPrice { get; set; }
        [JsonProperty("price_change_percentage_24h", NullValueHandling = NullValueHandling.Ignore)] public double PriceChangePercentage24H { get; set; }
        [JsonProperty("price_change_percentage_7d", NullValueHandling = NullValueHandling.Ignore)] public double PriceChangePercentage7D { get; set; }
    }

    public partial class CurrentPrice { [JsonProperty("usd")] public double Usd { get; set; } }

    public partial class CryptoGetData {
        public static CryptoData FromJson(String json) => JsonConvert.DeserializeObject<CryptoData>(json, Converter.Settings);
    }
}
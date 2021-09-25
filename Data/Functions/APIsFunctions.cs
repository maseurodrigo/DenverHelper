using System;
using System.Threading.Tasks;
using RestSharp;

namespace DenverHelper.Data.Functions
{
    public class APIsFunctions 
    {
        public static async Task<String> getWeatherAPIData(String _WeatherAPIKey, int _apiCall, String _city) {
            RestClient restClient = new RestClient();
            switch (_apiCall) {
                case 1:
                    // Get and return timezones data (given city name)
                    restClient = new RestClient($"https://weatherapi-com.p.rapidapi.com/timezone.json?q={ _city }");
                    break;
                case 2:
                    // Get and return weather data (given city name)
                    restClient = new RestClient($"https://weatherapi-com.p.rapidapi.com/current.json?q={ _city }");
                    break;
            }
            RestRequest reqst = new RestRequest(Method.GET);
            reqst.AddHeader("x-rapidapi-host", "weatherapi-com.p.rapidapi.com");
            reqst.AddHeader("x-rapidapi-key", _WeatherAPIKey);
            IRestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }

        public static async Task<String> getCryptoData(String _CoinGeckoKey, String _crypto) {
            // Get and return crypto data by given coin name
            RestClient restClient = new RestClient($"https://coingecko.p.rapidapi.com/coins/{ _crypto }?market_data=true");
            RestRequest reqst = new RestRequest(Method.GET);
            reqst.AddHeader("x-rapidapi-host", "coingecko.p.rapidapi.com");
            reqst.AddHeader("x-rapidapi-key", _CoinGeckoKey);
            IRestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }

        public static async Task<String> getAPISoccerTeams(String _RapidAPIKey, int _apiCall, int? _idTeam, String _teamName, int? _numGames, int? _idMatch, int? _season) {
            RestClient restClient = new RestClient();
            switch (_apiCall) {
                case 1:
                    // Get and return team members of given team
                    restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/players/squad/{ _idTeam }/{ _season }");
                    break;
                case 2:
                    // Get and return next games of given team
                    restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/fixtures/team/{ _idTeam }/next/{ _numGames }");
                    break;
                case 3:
                    // Get and return next game predict of given team
                    restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/predictions/{ _idMatch }");
                    break;
                case 4:
                    // Get and return data of a team through its name
                    restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/teams/search/{ _teamName }");
                    break;
                case 5:
                    // Get and return last games of given team
                    restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/fixtures/team/{ _idTeam }/last/{ _numGames }");
                    break;
            }
            RestRequest reqst = new RestRequest(Method.GET);
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
            IRestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }

        public static async Task<String> getAPISoccerPlayers(String _RapidAPIKey, int _apiCall, int? _idPlayer, String _playerName) {
            RestClient restClient = new RestClient();
            switch (_apiCall) {
                case 1:
                    // Get and return data of a player through its name
                    restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/players/search/{ _playerName }");
                    break;
                case 2:
                    // Get and return data of a player's trophies through its ID
                    restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/trophies/player/{ _idPlayer }");
                    break;
                case 3:
                    // Get and return data of a player's statistics through its ID
                    restClient = new RestClient($"https://api-football-v1.p.rapidapi.com/v2/players/player/{ _idPlayer }");
                    break;
            }
            RestRequest reqst = new RestRequest(Method.GET);
            reqst.AddHeader("x-rapidapi-key", _RapidAPIKey);
            reqst.AddHeader("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
            IRestResponse respJSON = await restClient.ExecuteAsync(reqst);
            return respJSON.Content;
        }
    }
}
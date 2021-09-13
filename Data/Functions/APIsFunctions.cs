using System;
using System.Threading.Tasks;
using RestSharp;

namespace DiscordDenver.Data.Functions
{
    public class APIsFunctions 
    {
        public static String getTimeZoneData(String _WeatherAPIKey, String _city) {
            // Get and return timezones data (given city name)
            RestClient restClient = new RestClient($"https://weatherapi-com.p.rapidapi.com/timezone.json?q={ _city }");
            RestRequest reqst = new RestRequest(Method.GET);
            reqst.AddHeader("x-rapidapi-host", "weatherapi-com.p.rapidapi.com");
            reqst.AddHeader("x-rapidapi-key", _WeatherAPIKey);
            IRestResponse response = restClient.Execute(reqst);
            return response.Content;
        }

        public static String getWeatherData(String _WeatherAPIKey, String _city) {
            // Get and return weather data (given city name)
            RestClient restClient = new RestClient($"https://weatherapi-com.p.rapidapi.com/current.json?q={ _city }");
            RestRequest reqst = new RestRequest(Method.GET);
            reqst.AddHeader("x-rapidapi-host", "weatherapi-com.p.rapidapi.com");
            reqst.AddHeader("x-rapidapi-key", _WeatherAPIKey);
            IRestResponse response = restClient.Execute(reqst);
            return response.Content;
        }

        public static String getCryptoData(String _CoinGeckoKey, String _crypto) {
            // Get and return crypto data by given coin name
            RestClient restClient = new RestClient($"https://coingecko.p.rapidapi.com/coins/{ _crypto }?market_data=true");
            RestRequest reqst = new RestRequest(Method.GET);
            reqst.AddHeader("x-rapidapi-host", "coingecko.p.rapidapi.com");
            reqst.AddHeader("x-rapidapi-key", _CoinGeckoKey);
            IRestResponse dynamJSON = restClient.Execute(reqst);
            return dynamJSON.Content;
        }

        public static async Task<String> getAPIFootballTeams(String _RapidAPIKey, int _method, int? _idTeam, String _teamName, int? _numGames, int? _idMatch, int? _season) {
            RestClient restClient = new RestClient();
            switch (_method) {
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
            IRestResponse dynamJSON = await restClient.ExecuteAsync(reqst);
            return dynamJSON.Content;
        }

        public static async Task<String> getAPIFootballPlayers(String _RapidAPIKey, int _method, int? _idPlayer, String _playerName, int? _season) {
            RestClient restClient = new RestClient();
            switch (_method) {
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
            IRestResponse dynamJSON = await restClient.ExecuteAsync(reqst);
            return dynamJSON.Content;
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApolloWorldCup.Library
{
    public class WorldCupApi
    {
        private string _urlTodayMatches = "http://worldcup.sfg.io/matches/today";
        private string _urlTomorrowMatches = "http://worldcup.sfg.io/matches/tomorrow";
        private string _urlYesterdayMatches = "http://worldcup.sfg.io/matches?start_date={0}&end_date={1}&by_date=ASC";
        private string _urlCurrentMatch = "http://worldcup.sfg.io/matches/current";
        private string _urlAllTeams = "https://worldcup.sfg.io/teams/results";
        private string _urlAllFuturesMatches = "https://worldcup.sfg.io/matches?start_date={0}&end_date=2018-08-01&by_date=ASC";
        private HttpClient _client;

        

        public WorldCupApi()
        {
            _client = new HttpClient();
        }

        public async Task<List<WorldCupMatch>> GetCurrentMatchAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _urlCurrentMatch);

            var response = await _client.SendAsync(request);

            return JsonConvert.DeserializeObject<List<WorldCupMatch>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<List<WorldCupMatch>> GetTodayMatches()
        {
            try
            {

                var request = new HttpRequestMessage(HttpMethod.Get, _urlTodayMatches);

                var response = await _client.SendAsync(request);

                var str = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<WorldCupMatch>>(str);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{ex.Message} - {ex.InnerException}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return new List<WorldCupMatch>();
        }

        public async Task<List<WorldCupMatch>> GetTomorrowMatches()
        {
            try
            {

                var request = new HttpRequestMessage(HttpMethod.Get, _urlTomorrowMatches);

                var response = await _client.SendAsync(request);

                var str = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<WorldCupMatch>>(str);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{ex.Message} - {ex.InnerException}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return new List<WorldCupMatch>();
        }

        public async Task<List<WorldCupMatch>> GetYesterdayMatches()
        {
            try
            {

                var request = new HttpRequestMessage(HttpMethod.Get, string.Format(_urlYesterdayMatches, DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"),  DateTime.Today.ToString("yyyy-MM-dd")));

                var response = await _client.SendAsync(request);

                var str = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<WorldCupMatch>>(str);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{ex.Message} - {ex.InnerException}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return new List<WorldCupMatch>();
        }

        public async Task<List<WorldCupMatch>> GetFuturesMatches()
        {
            try
            {

                var request = new HttpRequestMessage(HttpMethod.Get, string.Format(_urlAllFuturesMatches, DateTime.UtcNow.ToString("yyyy-MM-dd")));

                var response = await _client.SendAsync(request);

                var str = await response.Content.ReadAsStringAsync();

                var matches = JsonConvert.DeserializeObject<List<WorldCupMatch>>(str);

                return matches.ToList();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{ex.Message} - {ex.InnerException}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return new List<WorldCupMatch>();
        }

        public async Task<List<WorldCupTeam>> GetRemainingTeams()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, string.Format(_urlAllFuturesMatches, DateTime.UtcNow.ToString("yyyy-MM-dd")));

                var response = await _client.SendAsync(request);

                var str = await response.Content.ReadAsStringAsync();

                var matches = JsonConvert.DeserializeObject<List<WorldCupMatch>>(str);

                var teamIds = new List<string>();

                foreach(var match in matches)
                {
                    if(!teamIds.Any(t => match.AwayTeam.Code == t))
                    {
                        teamIds.Add(match.AwayTeam.Code);
                    }

                    if (!teamIds.Any(t => match.HomeTeam.Code == t))
                    {
                        teamIds.Add(match.HomeTeam.Code);
                    }
                }

                if(teamIds.Any())
                {
                    var requestTeam = new HttpRequestMessage(HttpMethod.Get, _urlAllTeams);

                    var responseTeam = await _client.SendAsync(requestTeam);

                    var strTeam = await responseTeam.Content.ReadAsStringAsync();

                    var teams =  JsonConvert.DeserializeObject<List<WorldCupTeam>>(strTeam);

                    return teams.Where(t => teamIds.Contains(t.FifaCode)).ToList();
                }

                return new List<WorldCupTeam>();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{ex.Message} - {ex.InnerException}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return new List<WorldCupTeam>();
        }
    }
}

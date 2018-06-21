using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApolloWorldCup
{
    public class WorldCupApi
    {
        private string _urlTodayMatches = "http://worldcup.sfg.io/matches/today";
        private string _urlTomorrowMatches = "http://worldcup.sfg.io/matches/tomorrow";
        private string _urlCurrentMatch = "http://worldcup.sfg.io/matches/current";
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
    }
}

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
        private string _urlCurrentMatch = "http://worldcup.sfg.io/matches/current";
        private int _timeBetweenCall = 10;
        private HttpClient _client;
        private Thread _thread;
        private Action<WorldCupMatch> _callbackStateMatch;
        private Action<WorldCupTeam, WorldCupTeamEvent> _callbackEvent;

        private List<WorldCupMatch> _previousMatches;

        public WorldCupApi()
        {
            _client = new HttpClient();
            _previousMatches = new List<WorldCupMatch>();
        }

        public void Start(Action<WorldCupMatch> callbackStateMatch, Action<WorldCupTeam, WorldCupTeamEvent> callbackEvent, int timeBetweenCall = 10)
        {
            _callbackStateMatch = callbackStateMatch;
            _callbackEvent = callbackEvent;
            _timeBetweenCall = timeBetweenCall;
            _thread = new Thread(Run);
            _thread.Start();
        }

        public void Stop()
        {
            if (_thread != null && _thread.IsAlive)
            {
                _thread.Abort();
            }
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

            return _previousMatches;
        }

        private void Run()
        {
            while(true)
            {
                var matches = GetTodayMatches().Result.OrderBy(m => DateTime.Parse(m.DateTime)).ToList();

                foreach(var match in matches)
                {
                    var previousMatch = _previousMatches.Where(m => m.Id == match.Id).FirstOrDefault();
                    if (previousMatch == null)
                    {
                        _callbackStateMatch(match);
                    }
                    else
                    {
                        var awayEvents = match.AwayTeamEvents.Select(e => e.Id).Except(previousMatch.AwayTeamEvents.Select(e => e.Id));

                        if (awayEvents.Any())
                        {
                            foreach (var id in awayEvents)
                            {
                                var e = match.AwayTeamEvents.First(a => a.Id == id);
                                _callbackEvent(match.AwayTeam, e);
                            }
                        }

                        var homeEvents = match.HomeTeamEvents.Select(e => e.Id).Except(previousMatch.HomeTeamEvents.Select(e => e.Id));

                        if (homeEvents.Any())
                        {
                            foreach (var id in homeEvents)
                            {
                                var e = match.HomeTeamEvents.First(a => a.Id == id);
                                _callbackEvent(match.HomeTeam, e);
                            }
                        }

                        if (previousMatch.Status != match.Status)
                        {
                            _callbackStateMatch(match);
                        }
                        else if ((previousMatch.Time == "half-time" || match.Time == "half-time") && previousMatch.Time != match.Time)
                        {
                            _callbackStateMatch(match);
                        }
                    }
                }

                _previousMatches = matches;

                Thread.Sleep(_timeBetweenCall);
            }
        }
    }
}

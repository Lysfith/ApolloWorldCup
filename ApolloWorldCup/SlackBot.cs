using ApolloWorldCup.Library;
using log4net;
using log4net.Core;
using Slack.Webhooks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace ApolloWorldCup
{
    public class SlackBot
    {
        public static string BOT_RUNNING = "Démarrage du bot";
        public static string BOT_STOPPING = "Arrêt du bot";

        public static string MATCH_OTHER = "{0} :flag-{1}: - :flag-{2}: {3} ({4})";
        public static string MATCH_FUTURE = "*{0} :* {1} :flag-{2}: - :flag-{3}: {4} :watch:{5}";
        public static string MATCH_FUTURE_TBD_1 = "*{0} :* {1} :flag-{2}: - Non déterminé :watch:{3}";
        public static string MATCH_FUTURE_TBD_2 = "*{0} :* Non déterminé - :flag-{1}: {2} :watch:{3}";
        public static string MATCH_FUTURE_TBD_3 = "*{0} :* Non déterminé - Non déterminé :watch:{1}";
        public static string MATCH_PAUSE = "{0} :flag-{1}: *{2}* | *{3}* :flag-{4}: {5} (Mi-temps)";
        public static string MATCH_START = "{0} :flag-{1}: *{2}* | *{3}* :flag-{4}: {5} (En cours - {6})";
        public static string MATCH_END_DRAW = "Le match *{0}* :flag-{2}: - :flag-{3}: *{4}* s'est terminé par une égalité ({5} - {6})";
        public static string MATCH_END_VICTORY = "Victoire de *{0}* :flag-{1}: face à *{2}* :flag-{3}: ({4} - {5})";

        public static string EVENT_YELLOW_CARD = ":carton_jaune: *Carton jaune* pour *{0}* de *{1}* :flag-{2}: à la *{3}*";
        public static string EVENT_RED_CARD = ":carton_rouge: *Carton rouge* pour *{0}* de *{1}* :flag-{2}: à la *{3}*";
        public static string EVENT_PENALTY = "*Penalty* en faveur de *{0}* :flag-{1}: tiré par *{2}* à la *{3}*";
        public static string EVENT_GOAL = ":but: *{0}* a marqué pour *{1}* :flag-{2}: à la *{3}*";

        public static string TEAM_STATS = ":flag-{1}: *{0}* : W *{2}* | D *{3}* | L *{4}* | Buts totaux *{5}* | Buts pris *{6}*";


        private Dictionary<string, Action> _commands;
        private SlackApi _api;
        private WorldCupApi _wcApi;
        private string _channelId;
        private string _token;
        private Thread _threadBot;
        private Thread _threadWc;
        private bool _running;
        private SlackMessageApi _firstMessage;
        private SlackMessageApi _lastMessage;
        private DateTime _start;

        private List<WorldCupMatch> _previousMatches;
        private string _channel = "#sport";
        private CultureInfo[] _cultures;
        private Action _onClose;

        private ILog _logger;

        public SlackBot(SlackApi api, WorldCupApi wcApi, string channelId, string token, ILog logger, Action onClose)
        {
            _logger = logger;
            _api = api;
            _wcApi = wcApi;
            _channelId = channelId;
            _token = token;
            _start = DateTime.Now;
            _previousMatches = new List<WorldCupMatch>();
            _onClose = onClose;

            _cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            _commands = new Dictionary<string, Action>()
            {
                { Constants.CMD_PRONO, () => { _api.SendMessage(channelId, "https://fr.pronocontest.com/contest/3087-apollocup?page=1#/ranking/general", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_SLITHER, () => { _api.SendMessage(channelId, "http://slither.io/", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_ALEXIS, () => { _api.SendMessage(channelId, "https://image.ibb.co/gZCMnJ/money.jpg", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_YAYA, () => { _api.SendMessage(channelId, "https://media.giphy.com/media/3oKGz8CjdhZx1OCDV6/source.gif", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_UPTIME, () => { _api.SendMessage(channelId, $"Bot démarré depuis le {_start.ToString("dd/MM/yy HH:mm:ss")}", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_ROLL, () => { _api.SendMessage(channelId, "https://www.youtube.com/watch?v=dQw4w9WgXcQ", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_PUDDY, () => { _api.SendMessage(channelId, "https://www.youtube.com/watch?v=KyucG76N9PY", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_HORSE, () => { _api.SendMessage(channelId, "https://www.youtube.com/watch?v=OWFBqiUgspg", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_POIREAU, () => { _api.SendMessage(channelId, "https://www.youtube.com/watch?v=jdL-K9EgSwE", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_STARS, () => { _api.SendMessage(channelId, "https://www.youtube.com/watch?v=cl4ySbLvdEM", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_CHICKEN, () => { _api.SendMessage(channelId, "https://www.youtube.com/watch?v=rA9Ood3-peg", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_TAUPE, () => { _api.SendMessage(channelId, "https://www.youtube.com/watch?v=24pUKRQt7fk", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_FROG, () => { _api.SendMessage(channelId, "https://www.youtube.com/watch?v=k85mRPqvMbE", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_GITHUB, () => { _api.SendMessage(channelId, "Vous pouvez contribuer ici : https://github.com/ApolloSSC/ApolloWorldCup", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_SCRIBE, () => { _api.SendMessage(channelId, "Vous savez, moi je ne crois pas qu’il y ait de bonne ou de mauvaise situation. Moi, si je devais résumer ma vie aujourd’hui avec vous, je dirais que c’est d’abord des rencontres. Des gens qui m’ont tendu la main, peut-être à un moment où je ne pouvais pas, où j’étais seul chez moi. Et c’est assez curieux de se dire que les hasards, les rencontres forgent une destinée... Parce que quand on a le goût de la chose, quand on a le goût de la chose bien faite, le beau geste, parfois on ne trouve pas l’interlocuteur en face je dirais, le miroir qui vous aide à avancer. Alors ça n’est pas mon cas, comme je disais là, puisque moi au contraire, j’ai pu : et je dis merci à la vie, je lui dis merci, je chante la vie, je danse la vie... je ne suis qu’amour ! Et finalement, quand beaucoup de gens aujourd’hui me disent « Mais comment fais-tu pour avoir cette humanité ? », et bien je leur réponds très simplement, je leur dis que c’est ce goût de l’amour ce goût donc qui m’a poussé aujourd’hui à entreprendre une construction mécanique, mais demain qui sait ? Peut-être simplement à me mettre au service de la communauté, à faire le don, le don de soi... ", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger); } },
                { Constants.CMD_STOP, () => {
                        _api.SendMessage(channelId, $"Demande d'arrêt du bot...", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger);
                        _onClose?.Invoke();
                    }
                },
                { Constants.CMD_TODAY, () => {
                        _api.SendMessage(channelId, $"Récupération des matchs de la journée...", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger);
                        var matches = _wcApi.GetTodayMatches().Result.OrderBy(m => DateTime.Parse(m.DateTime)).ToList();

                        PostMatchStateChange(matches);
                    }
                },
                { Constants.CMD_TOMORROW, () => {
                        _api.SendMessage(channelId, $"Récupération des matchs de demain...", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger);
                        var matches = _wcApi.GetTomorrowMatches().Result.OrderBy(m => DateTime.Parse(m.DateTime)).ToList();

                        PostMatchStateChange(matches);
                    }
                },
                 { Constants.CMD_YESTERDAY, () => {
                        _api.SendMessage(channelId, $"Récupération des matchs d'hier...", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger);
                        var matches = _wcApi.GetYesterdayMatches().Result.OrderBy(m => DateTime.Parse(m.DateTime)).ToList();

                        PostMatchStateChange(matches);
                    }
                },
                { Constants.CMD_FUTURES, () => {
                        _api.SendMessage(channelId, $"Récupération des prochains matchs...", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger);
                        var matches = _wcApi.GetFuturesMatches().Result.OrderBy(m => DateTime.Parse(m.DateTime)).ToList();

                        PostMatchStateChange(matches);
                    }
                },
                 { Constants.CMD_TEAMS, () => {
                        var teams = _wcApi.GetRemainingTeams().Result;

                        PostOnTeamStat(teams);
                    }
                },
                 { Constants.CMD_LIST, () => {
                        _api.SendMessage(channelId, string.Join(", ", _commands.Keys), Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup", _logger);
                    }
                },
            };

        }

        public void Start()
        {
            _running = true;

            PostStartBot();

            _threadBot = new Thread(Run);
            _threadBot.Start();

            _threadWc = new Thread(RunWc);
            _threadWc.Start();
        }

        public void Stop()
        {
            _running = false;

            PostStopBot();
        }

        public void Run()
        {
            while (_running)
            {
                try
                {
                    List<SlackMessageApi> messages = null;

                    if (_lastMessage == null)
                    {
                        messages = _api.GetMessagesFromChannel(_token, _channelId, 100).Result.ToList();
                    }
                    else
                    {
                        messages = _api.GetMessagesFromChannel(_token, _channelId, 100, _lastMessage.TimeStamp).Result.ToList();
                    }

                    if (_lastMessage == null)
                    {
                        _lastMessage = messages.Any() ? messages.FirstOrDefault() : null;
                    }
                    else
                    {
                        _lastMessage = messages.Any() ? messages.FirstOrDefault() : _lastMessage;

                        if (messages.Any())
                        {
                            foreach (var message in messages)
                            {
                                ExecuteCommand(message.Text);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                Thread.Sleep(1000);
            }
        }

        public void ExecuteCommand(string commandAsked)
        {
            if (string.IsNullOrEmpty(commandAsked))
            {
                return;
            }

            var parseLine = commandAsked.Split(' ');
            var commandStr = parseLine[0].ToLowerInvariant();

            var command = _commands.Keys.Where(c => c == commandStr).FirstOrDefault();

            if (_commands.ContainsKey(commandStr))
            {
                _commands[commandStr].Invoke();
            }
        }


        private void RunWc()
        {
            while (_running)
            {
                try
                {
                    var matches = _wcApi.GetTodayMatches().Result.OrderBy(m => DateTime.Parse(m.DateTime)).ToList();

                    if (!matches.Any())
                    {
                        matches = _previousMatches;
                    }

                    foreach (var match in matches)
                    {
                        var previousMatch = _previousMatches.Where(m => m.Id == match.Id).FirstOrDefault();
                        if (previousMatch == null)
                        {
                            PostMatchStateChange(new List<WorldCupMatch>() { match });
                        }
                        else
                        {
                            var awayEvents = match.AwayTeamEvents.Select(e => e.Id).Except(previousMatch.AwayTeamEvents.Select(e => e.Id));

                            if (awayEvents.Any())
                            {
                                foreach (var id in awayEvents)
                                {
                                    var e = match.AwayTeamEvents.First(a => a.Id == id);
                                    PostOnEvent(match, match.AwayTeam, e);
                                }
                            }

                            var homeEvents = match.HomeTeamEvents.Select(e => e.Id).Except(previousMatch.HomeTeamEvents.Select(e => e.Id));

                            if (homeEvents.Any())
                            {
                                foreach (var id in homeEvents)
                                {
                                    var e = match.HomeTeamEvents.First(a => a.Id == id);
                                    PostOnEvent(match, match.HomeTeam, e);
                                }
                            }

                            if (previousMatch.Status != match.Status)
                            {
                                PostMatchStateChange(new List<WorldCupMatch>() { match });
                            }
                            else if ((previousMatch.Time == "half-time" || match.Time == "half-time") && previousMatch.Time != match.Time)
                            {
                                PostMatchStateChange(new List<WorldCupMatch>() { match });
                            }
                        }
                    }

                    _previousMatches = matches;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                Thread.Sleep(20000);
            }
        }


        public void PostStartBot()
        {
            _logger.Info(BOT_RUNNING);
            _api.SendMessage(_channel, BOT_RUNNING, Emoji.Ghost, "Apollo WorldCup", _logger);
        }

        public void PostStopBot()
        {
            _logger.Info(BOT_STOPPING);
            _api.SendMessage(_channel, BOT_STOPPING, Emoji.Ghost, "Apollo WorldCup", _logger);

        }

        public void PostMatchStateChange(List<WorldCupMatch> matchStates)
        {
            var messageAll = "";

            if(!matchStates.Any())
            {
                _logger.Info("Nope !");
                _api.SendMessage(_channel, "https://i.giphy.com/media/12XMGIWtrHBl5e/giphy.gif", Emoji.Ghost, "Apollo WorldCup", _logger);

                return;
            }

            foreach (var matchState in matchStates)
            {
                var date = DateTime.Parse(matchState.DateTime);
                var message = "";

                switch (matchState.Status)
                {
                    case "future":
                        var stageName = "";

                        switch (matchState.StageName)
                        {
                            case "Round of 16":
                                stageName = "1/8";
                                break;
                            case "Quarter-finals":
                                stageName = "1/4";
                                break;
                            case "Semi-finals":
                                stageName = "1/2";
                                break;
                            case "Play-off for third place":
                                stageName = "Petite finale";
                                break;
                            case "Final":
                                stageName = "Finale";
                                break;
                        }

                        if (matchState.HomeTeam.Code != "TBD" && matchState.AwayTeam.Code != "TBD")
                        {
                            message = string.Format(
                                MATCH_FUTURE,
                                stageName,
                                matchState.HomeTeam.Country,
                                GetCountryCode(matchState.HomeTeam.Country),
                                GetCountryCode(matchState.AwayTeam.Country),
                                matchState.AwayTeam.Country,
                                date.ToString(Constants.DATETIME_FORMAT)
                                );
                        }
                        else if (matchState.HomeTeam.Code != "TBD")
                        {
                            message = string.Format(
                                MATCH_FUTURE_TBD_1,
                                stageName,
                                matchState.HomeTeam.Country,
                                GetCountryCode(matchState.HomeTeam.Country),
                                date.ToString(Constants.DATETIME_FORMAT)
                                );
                        }
                        else if (matchState.AwayTeam.Code != "TBD")
                        {
                            message = string.Format(
                                MATCH_FUTURE_TBD_2,
                                stageName,
                                GetCountryCode(matchState.AwayTeam.Country),
                                matchState.AwayTeam.Country,
                                date.ToString(Constants.DATETIME_FORMAT)
                                );
                        }
                        else
                        {
                            message = string.Format(
                                MATCH_FUTURE_TBD_3,
                                stageName,
                                date.ToString(Constants.DATETIME_FORMAT)
                                );
                        }
                        break;
                    case "in progress":
                        if (matchState.Time == "half-time")
                        {
                            message = string.Format(
                                MATCH_PAUSE,
                                matchState.HomeTeam.Country,
                                GetCountryCode(matchState.HomeTeam.Country),
                                matchState.HomeTeam.Goals,
                                matchState.AwayTeam.Goals,
                                GetCountryCode(matchState.AwayTeam.Country),
                                matchState.AwayTeam.Country
                                );
                        }
                        else
                        {
                            message = string.Format(
                                MATCH_START,
                                matchState.HomeTeam.Country,
                                GetCountryCode(matchState.HomeTeam.Country),
                                matchState.HomeTeam.Goals,
                                matchState.AwayTeam.Goals,
                                GetCountryCode(matchState.AwayTeam.Country),
                                matchState.AwayTeam.Country,
                                matchState.Time
                                );
                        }
                        break;
                    case "completed":
                        if (matchState.Winner == "Draw")
                        {
                            message = string.Format(
                                MATCH_END_DRAW,
                                matchState.HomeTeam.Country,
                                GetCountryCode(matchState.HomeTeam.Country),
                                GetCountryCode(matchState.AwayTeam.Country),
                                matchState.AwayTeam.Country,
                                matchState.AwayTeam.Goals,
                                matchState.AwayTeam.Goals
                                );
                        }
                        else
                        {
                            var looser = matchState.Winner == matchState.HomeTeam.Country ? matchState.AwayTeam.Country : matchState.HomeTeam.Country;
                            var scoreWinner = matchState.Winner == matchState.HomeTeam.Country ? matchState.HomeTeam.Goals : matchState.AwayTeam.Goals;
                            var scoreLooser = matchState.Winner == matchState.HomeTeam.Country ? matchState.AwayTeam.Goals : matchState.HomeTeam.Goals;

                            message = string.Format(
                                MATCH_END_VICTORY,
                                matchState.Winner,
                                GetCountryCode(matchState.Winner),
                                looser,
                                GetCountryCode(looser),
                                scoreWinner,
                                scoreLooser
                                );
                        }
                        break;
                    default:
                        message = string.Format(
                          MATCH_OTHER,
                          matchState.HomeTeam.Country,
                          GetCountryCode(matchState.HomeTeam.Country),
                          GetCountryCode(matchState.AwayTeam.Country),
                          matchState.AwayTeam.Country,
                          matchState.Status
                          );
                        break;
                }

                messageAll += message + "\n";

                if (matchState.Status != "future")
                {
                    if (matchState.HomeTeamEvents.Any() || matchState.AwayTeamEvents.Any())
                    {
                        var team = matchState.HomeTeam;

                        messageAll += string.Format("    *Stats* {0} :flag-{1}: :\n", team.Country, GetCountryCode(team.Country));

                        var events = matchState.HomeTeamEvents.GroupBy(e => e.Type);

                        messageAll += "        ";

                        var listMessages = new List<string>();

                        foreach (var e in events)
                        {
                            if (e.Key == "red-card")
                            {
                                listMessages.Add($"*{e.Count()}x* :carton_rouge: ({string.Join(", ", e.Select(u => u.Player))})");
                            }
                            else if (e.Key == "yellow-card")
                            {
                                listMessages.Add($"*{e.Count()}x* :carton_jaune: ({string.Join(", ", e.Select(u => u.Player))})");
                            }
                            else if (e.Key == "goal-penalty")
                            {
                                listMessages.Add($"*{e.Count()}x* Penalty ({string.Join(", ", e.Select(u => u.Player))})");
                            }
                            else if (e.Key == "goal")
                            {
                                listMessages.Add($"*{e.Count()}x* :but: ({string.Join(", ", e.Select(u => u.Player))})");
                            }
                        }

                        messageAll += string.Join(", ", listMessages) +  "\n";

                        //=========================
                        listMessages.Clear();

                        team = matchState.AwayTeam;

                        messageAll += string.Format("    *Stats* {0} :flag-{1}: :\n", team.Country, GetCountryCode(team.Country));

                        events = matchState.AwayTeamEvents.GroupBy(e => e.Type);

                        messageAll += "        ";

                        foreach (var e in events)
                        {
                            if (e.Key == "red-card")
                            {
                                listMessages.Add($"*{e.Count()}x* :carton_rouge: ({string.Join(", ", e.Select(u => u.Player))})");
                            }
                            else if (e.Key == "yellow-card")
                            {
                                listMessages.Add($"*{e.Count()}x* :carton_jaune: ({string.Join(", ", e.Select(u => u.Player))})");
                            }
                            else if (e.Key == "goal-penalty")
                            {
                                listMessages.Add($"*{e.Count()}x* Penalty ({string.Join(", ", e.Select(u => u.Player))})");
                            }
                            else if (e.Key == "goal")
                            {
                                listMessages.Add($"*{e.Count()}x* :but: ({string.Join(", ", e.Select(u => u.Player))})");
                            }
                        }

                        messageAll += string.Join(", ", listMessages) + "\n";
                    }
                }

                messageAll += "\n";
            }

            _logger.Info(messageAll);
            _api.SendMessage(_channel, messageAll, Emoji.Ghost, "Apollo WorldCup", _logger);
        }

        public void PostOnEvent(WorldCupMatch match, WorldCupTeam team, WorldCupTeamEvent e)
        {
            var message = "";

            switch (e.Type)
            {
                case "red-card":
                    message = string.Format(EVENT_RED_CARD, e.Player, team.Country, GetCountryCode(team.Country), e.Time);
                    break;
                case "yellow-card":
                    message = string.Format(EVENT_YELLOW_CARD, e.Player, team.Country, GetCountryCode(team.Country), e.Time);
                    break;
                case "goal-penalty":
                    message = string.Format(EVENT_PENALTY, team.Country, GetCountryCode(team.Country), e.Player, e.Time);
                    break;
                case "goal":
                    message = string.Format(EVENT_GOAL, e.Player, team.Country, GetCountryCode(team.Country), e.Time);
                    break;
                default:
                    return;
            }

            _logger.Info(message);
            _api.SendMessage(_channel, message, Emoji.Ghost, "Apollo WorldCup", _logger);

            if (e.Type == "goal")
            {
                PostMatchStateChange(new List<WorldCupMatch>() { match });
            }
        }

        public void PostOnTeamStat(List<WorldCupTeam> teams)
        {
            var messageAll = "";
            foreach (var team in teams)
            {
                var message = string.Format(TEAM_STATS, team.Country, GetCountryCode(team.Country), team.Wins, team.Draws, team.Losses, team.GoalsFor, team.GoalsAgainst);

                messageAll += message + "\n";
            }

            _logger.Info(messageAll);
            _api.SendMessage(_channel, messageAll, Emoji.Ghost, "Apollo WorldCup", _logger);
        }

        private string GetCountryCode(string s)
        {
            try
            {
                switch (s)
                {
                    case "England":
                        return "England";
                }

                var cinfo = _cultures.FirstOrDefault(culture => s.Contains((new RegionInfo(culture.LCID).EnglishName)));

                if (cinfo == null)
                {
                    return $"pas trouvé ({s})";
                }

                var rinfo = new RegionInfo(cinfo.LCID);

                return rinfo.TwoLetterISORegionName;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return "";
            }
        }
    }
}

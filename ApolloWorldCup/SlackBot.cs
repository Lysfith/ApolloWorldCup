﻿using Slack.Webhooks;
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

        public static string MATCH_FUTURE = "{0} :flag-{1}: *{2}* | *{3}* :flag-{4}: {5} (Commence à {6})";
        public static string MATCH_PAUSE = "{0} :flag-{1}: *{2}* | *{3}* :flag-{4}: {5} (Mi-temps)";
        public static string MATCH_START = "{0} :flag-{1}: *{2}* | *{3}* :flag-{4}: {5} (En cours - {6})";
        public static string MATCH_END_DRAW = "Le match *{0}* :flag-{2}: - :flag-{3}: *{4}* s'est terminé par une égalité ({5} - {6})";
        public static string MATCH_END_VICTORY = "Victoire de *{0}* :flag-{1}: face à *{2}* :flag-{3}: ({4} - {5})";

        public static string EVENT_YELLOW_CARD = "*Carton jaune* pour *{0}* de *{1}* :flag-{2}: à la *{3}*";
        public static string EVENT_RED_CARD = "*Carton rouge* pour *{0}* de *{1}* :flag-{2}: à la *{3}*";
        public static string EVENT_PENALTY = "*Penalty* en faveur de *{0}* :flag-{1}: tiré par *{2}* à la *{3}*";
        public static string EVENT_GOAL = "*{0}* a marqué pour *{1}* :flag-{2}: à la *{3}*";


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

        public SlackBot(SlackApi api, WorldCupApi wcApi, string channelId, string token)
        {
            _api = api;
            _wcApi = wcApi;
            _channelId = channelId;
            _token = token;
            _start = DateTime.Now;
            _previousMatches = new List<WorldCupMatch>();

            _commands = new Dictionary<string, Action>()
            {
                { "!prono", () => { _api.SendMessage(channelId, "https://fr.pronocontest.com/contest/3087-apollocup?page=1#/ranking/general", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup"); } },
                { "!slither", () => { _api.SendMessage(channelId, "http://slither.io/", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup"); } },
                { "!alexis", () => { _api.SendMessage(channelId, "Monnnnnneeeetttt", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup"); } },
                { "!yaya", () => { _api.SendMessage(channelId, "Zoom zoom", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup"); } },
                { "!uptime", () => { _api.SendMessage(channelId, $"Bot démarré depuis le {_start.ToString("dd/MM/yy HH:mm:ss")}", Slack.Webhooks.Emoji.Ghost, "ApolloWorldCup"); } },
                { "!today", () => {
                        var matches = _wcApi.GetTodayMatches().Result.OrderBy(m => DateTime.Parse(m.DateTime)).ToList();

                        foreach (var match in matches)
                        {
                            PostMatchStateChange(match);
                        }
                    }
                },
                { "!tomorrow", () => {
                        var matches = _wcApi.GetTomorrowMatches().Result.OrderBy(m => DateTime.Parse(m.DateTime)).ToList();

                        foreach (var match in matches)
                        {
                            PostMatchStateChange(match);
                        }
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
                            var parseLine = message.Text.Split(" ");
                            var commandStr = parseLine[0].ToLowerInvariant();

                            var command = _commands.Keys.Where(c => c == commandStr).FirstOrDefault();

                            if (_commands.ContainsKey(commandStr))
                            {
                                _commands[commandStr].Invoke();
                            }
                        }
                    }
                }

                Thread.Sleep(10000);
            }
        }


        private void RunWc()
        {
            while (_running)
            {
                var matches = _wcApi.GetTodayMatches().Result.OrderBy(m => DateTime.Parse(m.DateTime)).ToList();

                if(!matches.Any())
                {
                    matches = _previousMatches;
                }

                foreach (var match in matches)
                {
                    var previousMatch = _previousMatches.Where(m => m.Id == match.Id).FirstOrDefault();
                    if (previousMatch == null)
                    {
                        PostMatchStateChange(match);
                    }
                    else
                    {
                        var awayEvents = match.AwayTeamEvents.Select(e => e.Id).Except(previousMatch.AwayTeamEvents.Select(e => e.Id));

                        if (awayEvents.Any())
                        {
                            foreach (var id in awayEvents)
                            {
                                var e = match.AwayTeamEvents.First(a => a.Id == id);
                                PostOnEvent(match.AwayTeam, e);
                            }
                        }

                        var homeEvents = match.HomeTeamEvents.Select(e => e.Id).Except(previousMatch.HomeTeamEvents.Select(e => e.Id));

                        if (homeEvents.Any())
                        {
                            foreach (var id in homeEvents)
                            {
                                var e = match.HomeTeamEvents.First(a => a.Id == id);
                                PostOnEvent(match.HomeTeam, e);
                            }
                        }

                        if (previousMatch.Status != match.Status)
                        {
                            PostMatchStateChange(match);
                        }
                        else if ((previousMatch.Time == "half-time" || match.Time == "half-time") && previousMatch.Time != match.Time)
                        {
                            PostMatchStateChange(match);
                        }
                    }
                }

                _previousMatches = matches;

                Thread.Sleep(10000);
            }
        }


        public void PostStartBot()
        {
            Console.WriteLine(BOT_RUNNING);

            _api.SendMessage("#sport", BOT_RUNNING, Emoji.Ghost, "Apollo WorldCup");
        }

        public void PostStopBot()
        {
            Console.WriteLine(BOT_STOPPING);

            _api.SendMessage("#sport", BOT_STOPPING, Emoji.Ghost, "Apollo WorldCup");
        }

        public void PostMatchStateChange(WorldCupMatch matchState)
        {
            var date = DateTime.Parse(matchState.DateTime);
            var message = "";

            switch (matchState.Status)
            {
                case "future":
                    message = string.Format(
                        MATCH_FUTURE,
                        matchState.HomeTeam.Country,
                        GetCountryCode(matchState.HomeTeam.Country),
                        matchState.HomeTeam.Goals, 
                        matchState.AwayTeam.Goals,
                        GetCountryCode(matchState.AwayTeam.Country),
                        matchState.AwayTeam.Country, 
                        date.ToLocalTime().ToShortTimeString()
                        );
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
                            matchState.AwayTeam.Country,
                            GetCountryCode(matchState.AwayTeam.Country),
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
                    return;
            }

            Console.WriteLine(message);

            _api.SendMessage("#sport", message, Emoji.Ghost, "Apollo WorldCup");
        }

        public void PostOnEvent(WorldCupTeam team, WorldCupTeamEvent e)
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

            var slackMessage = new SlackMessage
            {
                Channel = "#sport",
                Text = message,
                IconEmoji = Emoji.Ghost,
                Username = "ApolloWorldCup"
            };

            Console.WriteLine(message);

            _api.SendMessage("#sport", message, Emoji.Ghost, "Apollo WorldCup");
        }

        private string GetCountryCode(string s) {
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            var cinfo = cultures.FirstOrDefault(culture => (new RegionInfo(culture.LCID).EnglishName) == s);
            var rinfo = new RegionInfo(cinfo.LCID);

            return rinfo.TwoLetterISORegionName;
        }
    }
}

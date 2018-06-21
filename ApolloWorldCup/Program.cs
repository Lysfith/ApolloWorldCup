using Slack.Webhooks;
using System;
using System.Linq;

namespace ApolloWorldCup
{
    class Program
    {
        public static string BOT_RUNNING = "Démarrage du bot";
        public static string BOT_STOPPING = "Arrêt du bot";

        public static string MATCH_FUTURE = "{0} *{1}* | *{2}* {3} (Commence à {4})";
        public static string MATCH_PAUSE = "{0} *{1}* | *{2}* {3} (Mi-temps)";
        public static string MATCH_START = "{0} *{1}* | *{2}* {3} (En cours - {4})";
        public static string MATCH_END_DRAW = "Le match *{0}* - *{1}* s'est terminé par une égalité ({2} - {3})";
        public static string MATCH_END_VICTORY = "Victoire de *{0}* face à *{1}* ({2} - {3})";

        public static string EVENT_YELLOW_CARD = "*Carton jaune* pour *{0}* de *{1}* à la *{2}*";
        public static string EVENT_RED_CARD = "*Carton rouge* pour *{0}* de *{1}* à la *{2}*";
        public static string EVENT_PENALTY = "*Penalty* en faveur de *{0}* tiré par *{1}* à la *{2}*";
        public static string EVENT_GOAL = "*{0}* a marqué pour *{1}* à la *{2}*";

        public static SlackApi _slackApi;
        public static WorldCupApi _wcApi;
        public static bool _enableSlackApi = false;

        public static string _channelId = "CAZAYAE1G";
        public static string _tokenSlack = "xoxp-301409525076-303733381060-384153991315-e03a5853186123adcbe4fd9ff71ec387";

        static void Main(string[] args)
        {
            string webhook = null;
            if (args != null)
            {
                if (args.Length > 0)
                {
                    webhook = args[0];
                    _enableSlackApi = true;
                }
                if (args.Length > 0)
                {
                    webhook = args[0];
                    _enableSlackApi = true;
                }
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Start");
            Console.ForegroundColor = ConsoleColor.White;

            _wcApi = new WorldCupApi();

            _slackApi = new SlackApi(webhook);
            var messages = _slackApi.GetMessagesFromChannel(_tokenSlack, _channelId, 30, null).Result;
            var lastMessage = _slackApi.GetMessagesFromChannel(_tokenSlack, _channelId, 30, t.ElementAt(1).TimeStamp).Result;

            PostStartBot();

            _wcApi.Start(PostMatchStateChange, PostOnEvent);

            Console.ReadKey();

            _wcApi.Stop();

            PostStopBot();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("End");
        }

        public static void PostStartBot()
        {
            Console.WriteLine(BOT_RUNNING);

            if (_enableSlackApi)
            {
                _slackApi.SendMessage("#sport", BOT_RUNNING, Emoji.Ghost, "Apollo WorldCup");
            }
        }

        public static void PostStopBot()
        {
            Console.WriteLine(BOT_STOPPING);

            if (_enableSlackApi)
            {
                _slackApi.SendMessage("#sport", BOT_STOPPING, Emoji.Ghost, "Apollo WorldCup");
            }
        }

        public static void PostMatchStateChange(WorldCupMatch matchState)
        {
            var date = DateTime.Parse(matchState.DateTime);
            var message = "";

            switch (matchState.Status)
            {
                case "future":
                    message = string.Format(MATCH_FUTURE, matchState.HomeTeam.Country, matchState.HomeTeam.Goals, matchState.AwayTeam.Goals, matchState.AwayTeam.Country, date.ToLocalTime().ToShortTimeString());
                    break;
                case "in progress":
                    if (matchState.Time == "half-time")
                    {
                        message = string.Format(MATCH_PAUSE, matchState.HomeTeam.Country, matchState.HomeTeam.Goals, matchState.AwayTeam.Goals, matchState.AwayTeam.Country);
                    }
                    else
                    {
                        message = string.Format(MATCH_START, matchState.HomeTeam.Country, matchState.HomeTeam.Goals, matchState.AwayTeam.Goals, matchState.AwayTeam.Country, matchState.Time);
                    }
                    break;
                case "completed":
                    if (matchState.Winner == "Draw")
                    {
                        message = string.Format(MATCH_END_DRAW, matchState.HomeTeam.Country, matchState.AwayTeam.Country, matchState.AwayTeam.Goals, matchState.AwayTeam.Goals);
                    }
                    else
                    {
                        var looser = matchState.Winner == matchState.HomeTeam.Country ? matchState.AwayTeam.Country : matchState.HomeTeam.Country;
                        var scoreWinner = matchState.Winner == matchState.HomeTeam.Country ? matchState.HomeTeam.Goals : matchState.AwayTeam.Goals;
                        var scoreLooser = matchState.Winner == matchState.HomeTeam.Country ? matchState.AwayTeam.Goals : matchState.HomeTeam.Goals;

                        message = string.Format(MATCH_END_VICTORY, matchState.Winner, looser, scoreWinner, scoreLooser);
                    }
                    break;
                default:
                    return;
            }

            Console.WriteLine(message);

            if (_enableSlackApi)
            {
                _slackApi.SendMessage("#sport", message, Emoji.Ghost, "Apollo WorldCup");
            }
        }

        public static void PostOnEvent(WorldCupTeam team, WorldCupTeamEvent e)
        {
            var message = "";

            switch(e.Type)
            {
                case "red-card":
                    message = string.Format(EVENT_YELLOW_CARD, e.Player, team.Country, e.Time);
                    break;
                case "yellow-card":
                    message = string.Format(EVENT_RED_CARD, e.Player, team.Country, e.Time);
                    break;
                case "goal-penalty":
                    message = string.Format(EVENT_PENALTY, team.Country, e.Player, e.Time);
                    break;
                case "goal":
                    message = string.Format(EVENT_GOAL, e.Player, team.Country, e.Time);
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

            if (_enableSlackApi)
            {
                _slackApi.SendMessage("#sport", message, Emoji.Ghost, "Apollo WorldCup");
            }
        }
    }
}

using Slack.Webhooks;
using System;

namespace ApolloWorldCup
{
    class Program
    {
        public static SlackClient _slackClient;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Start");
            Console.ForegroundColor = ConsoleColor.White;

            var wcApi = new WorldCupApi();

            _slackClient = new SlackClient("#WEBHOOK_URL#");

            PostStartBot();

            wcApi.Start(PostMatchStateChange, PostOnEvent);

            Console.ReadKey();

            wcApi.Stop();

            PostStopBot();
        }

        public static void PostStartBot()
        {
            var slackMessage = new SlackMessage
            {
                Channel = "#sport",
                Text = $"Démarrage du bot",
                IconEmoji = Emoji.Ghost,
                Username = "Apollo WorldCup"
            };

            Console.WriteLine($"Démarrage du bot");

            _slackClient.Post(slackMessage);
        }

        public static void PostStopBot()
        {
            var slackMessage = new SlackMessage
            {
                Channel = "#sport",
                Text = $"Arrêt du bot",
                IconEmoji = Emoji.Ghost,
                Username = "Apollo WorldCup"
            };

            Console.WriteLine($"Arrêt du bot");

            _slackClient.Post(slackMessage);
        }

        public static void PostMatchStateChange(WorldCupMatch matchState)
        {
            var date = DateTime.Parse(matchState.DateTime);
            var message = "";

            switch (matchState.Status)
            {
                case "future":
                    message = $"{matchState.HomeTeam.Country} *{matchState.HomeTeam.Goals}* | *{matchState.AwayTeam.Goals}* {matchState.AwayTeam.Country} (Commence à {date.ToLocalTime().ToShortTimeString()})";
                    break;
                case "in progress":
                    if (matchState.Time == "half-time")
                    {
                        message = $"{matchState.HomeTeam.Country} *{matchState.HomeTeam.Goals}* | *{matchState.AwayTeam.Goals}* {matchState.AwayTeam.Country} (Mi-temps)";
                    }
                    else
                    {
                        message = $"{matchState.HomeTeam.Country} *{matchState.HomeTeam.Goals}* | *{matchState.AwayTeam.Goals}* {matchState.AwayTeam.Country} (En cours - {matchState.Time})";
                    }
                    break;
                case "completed":
                    if (matchState.Winner == "Draw")
                    {
                        message = $"Le match *{matchState.HomeTeam.Country}* - *{matchState.AwayTeam.Country}* s'est terminé par une égalité ({matchState.AwayTeam.Goals} - {matchState.AwayTeam.Goals})";
                    }
                    else
                    {
                        var looser = matchState.Winner == matchState.HomeTeam.Country ? matchState.AwayTeam.Country : matchState.HomeTeam.Country;
                        var scoreWinner = matchState.Winner == matchState.HomeTeam.Country ? matchState.HomeTeam.Goals : matchState.AwayTeam.Goals;
                        var scoreLooser = matchState.Winner == matchState.HomeTeam.Country ? matchState.AwayTeam.Goals : matchState.HomeTeam.Goals;

                        message = $"Victoire de *{matchState.Winner}* face à *{looser}* ({scoreWinner} - {scoreLooser})";
                    }
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

            _slackClient.Post(slackMessage);
        }

        public static void PostOnEvent(WorldCupTeam team, WorldCupTeamEvent e)
        {
            var message = "";

            switch(e.Type)
            {
                case "red-card":
                    message = $"*Carton rouge* pour *{e.Player}* de *{team.Country}* à la *{e.Time}*";
                    break;
                case "yellow-card":
                    message = $"*Carton jaune* pour *{e.Player}* de *{team.Country}* à la *{e.Time}*";
                    break;
                case "goal-penalty":
                    message = $"*Penalty* en faveur de *{team.Country}* tiré par *{e.Player}* à la *{e.Time}*";
                    break;
                case "goal":
                    message = $"*{e.Player}* a marqué pour *{team.Country}* à la *{e.Time}*";
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

            _slackClient.Post(slackMessage);
        }
    }
}

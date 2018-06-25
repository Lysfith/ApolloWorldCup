using Slack.Webhooks;
using System;
using System.Linq;

namespace ApolloWorldCup
{
    class Program
    {
        public static SlackApi _slackApi;
        public static SlackBot _slackBot;
        public static WorldCupApi _wcApi;
        public static bool _enableSlackApi = false;

        public static string _channelId = "CAZAYAE1G";

        static void Main(string[] args)
        {
            string webhook = null;
            string token = null;
            if (args != null)
            {
                if (args.Length > 0)
                {
                    webhook = args[0];
                    _enableSlackApi = true;
                }
                if (args.Length > 1)
                {
                    token = args[1];
                }
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Start");
            Console.ForegroundColor = ConsoleColor.White;

            _wcApi = new WorldCupApi();
            _slackApi = new SlackApi(webhook);
        
            _slackBot = new SlackBot(_slackApi, _wcApi, _channelId, token);
            _slackBot.Start();

            Console.ReadKey();

            _slackBot.Stop();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("End");
        }

    }
}

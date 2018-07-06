using ApolloWorldCup;
using ApolloWorldCup.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApolloWorldCupLauncher
{
    class Program
    {
        public static SlackApi _slackApi;
        public static SlackBot _slackBot;
        public static bool _enableSlackApi = false;
        public static bool _running = true;

        public static string _channelId = "CAZAYAE1G";

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                string webhook = null;
                string token = null;
                string channelId = _channelId;
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
                    if (args.Length > 2)
                    {
                        channelId = args[2];
                    }
                }

                _slackApi = new SlackApi(webhook);

                _slackBot = new SlackBot(_slackApi, channelId, token, () => _running = false);
                _slackBot.Start();


                while (_running)
                {
                    Thread.Sleep(1000);
                }

                _slackBot.Stop();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

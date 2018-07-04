using Slack.Webhooks;
using System;
using System.Linq;
using log4net.Config;
using log4net;
using System.Reflection;
using System.IO;
using System.Threading;

namespace ApolloWorldCup
{
    class Program
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static SlackApi _slackApi;
        public static SlackBot _slackBot;
        public static WorldCupApi _wcApi;
        public static PronoApi _pronoApi;
        public static bool _enableSlackApi = false;
        public static bool _running = true;

        public static string _channelId = "CAZAYAE1G";

        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

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

                _log.Info("Start");


                _wcApi = new WorldCupApi();
                _slackApi = new SlackApi(webhook);

                _slackBot = new SlackBot(_slackApi, _wcApi, channelId, token, _log, () => _running = false);
                _slackBot.Start();

#if DEBUG
                string command = Constants.CMD_YESTERDAY;
                _slackBot.ExecuteCommand(command);
#endif

                while (_running)
                {
                    Thread.Sleep(1000);
                }

                _slackBot.Stop();
                
                _log.Info("End");

            }
            catch (Exception e) {
                _log.Error(e);
            }
        }

    }
}

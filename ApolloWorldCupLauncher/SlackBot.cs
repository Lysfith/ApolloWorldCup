using ApolloWorldCup;
using ApolloWorldCup.Library;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace ApolloWorldCupLauncher
{
    public class SlackBot
    {
        private Dictionary<string, System.Action> _commands;
        private string _channelId;
        private SlackApi _api;
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
        private System.Action _onClose;

        public SlackBot(SlackApi api, string channelId, string token, System.Action onClose)
        {
            _channelId = channelId;
            _token = token;
            _start = DateTime.Now;
            _onClose = onClose;
            _api = api;

            _cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            _commands = new Dictionary<string, System.Action>()
            {
                { "!start", () => {
                        // Create a new task definition and assign properties
                        var task = TaskService.Instance.FindTask("ApolloWorldCup");

                        if(task.State != TaskState.Running)
                        {
                            task.Run();
                        }
                    }
                }
            };

        }

        public void Start()
        {
            _running = true;

            _threadBot = new Thread(Run);
            _threadBot.Start();
        }

        public void Stop()
        {
            _running = false;
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
                    Console.WriteLine(ex.Message);
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
    }
}

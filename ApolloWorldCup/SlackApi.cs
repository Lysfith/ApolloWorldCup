using Slack.Webhooks;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloWorldCup
{
    public class SlackApi
    {
        public const string WEBHOOK_URL = "#WEBHOOK_URL#";

        public SlackClient _client;

        public SlackApi(string webhook = null)
        {
            if (string.IsNullOrEmpty(webhook))
            {
                _client = new SlackClient(WEBHOOK_URL);
            }
            else
            {
                _client = new SlackClient(webhook);
            }
        }

        public void SendMessage(string channel, string text, Emoji icon, string username)
        {
            var slackMessage = new SlackMessage
            {
                Channel = channel,
                Text = text,
                IconEmoji = icon,
                Username = username
            };

            _client.Post(slackMessage);
        }
    }
}

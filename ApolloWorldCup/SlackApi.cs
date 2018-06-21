using Slack.Webhooks;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloWorldCup
{
    public class SlackApi
    {
        public const string WEBHOOK_URL = "https://hooks.slack.com/services/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";

        public SlackClient _client;

        public SlackApi()
        {
            _client = new SlackClient(WEBHOOK_URL);
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

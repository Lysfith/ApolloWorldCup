using log4net;
using Newtonsoft.Json;
using Slack.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApolloWorldCup
{
    public class SlackApi
    {
        public const string WEBHOOK_URL = "#WEBHOOK_URL#";

        public SlackClient _client;
        public HttpClient _clientHttp;

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

            _clientHttp = new HttpClient();
        }

        public void SendMessage(string channel, string text, Emoji icon, string username, ILog logger)
        {
            try { 
                var slackMessage = new SlackMessage
                {
                    Channel = channel,
                    Text = text,
                    IconEmoji = icon,
                    Username = username
                };

                _client.Post(slackMessage);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public async Task<IEnumerable<SlackMessageApi>> GetMessagesFromChannel(string token, string channelId, int count, string oldest = null)
        {
            string url = $"https://slack.com/api/channels.history?token={token}&channel={channelId}&count={count}&oldest={oldest}";

            if(oldest != null)
            {
                url += $"&oldest={oldest}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _clientHttp.SendAsync(request);

            var result =  JsonConvert.DeserializeObject<SlackResultApi>(await response.Content.ReadAsStringAsync());

            return result.Messages.Where(m => m.User != null).ToList();
        }
    }
}

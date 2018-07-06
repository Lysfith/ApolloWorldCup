using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ApolloWorldCup.Library
{
    public class PronoApi
    {
        private string _url = "https://fr.pronocontest.com/contest/3087-apollocup?page=1#/ranking/general";
        private string _urlLogin = "https://fr.pronocontest.com/login";
        private HttpClient _client;

        

        public PronoApi()
        {
            _client = new HttpClient();
        }

        public async Task LoginAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _urlLogin);
            var response = await _client.SendAsync(request);

            var html = await response.Content.ReadAsStringAsync();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var metas = htmlDoc.DocumentNode.SelectNodes("//meta");
            var meta = metas.Where(m => m.GetAttributeValue("id", null) != null).FirstOrDefault();

            var token = meta.GetAttributeValue("content", null);

            var request2 = new HttpRequestMessage(HttpMethod.Post, _urlLogin);
            var str = "{ \"email\": \"" + "julien.plateau@apollossc.com" + "\", \"password\": \"" + "Palouf1789" + "\", \"remember\": 1, \"return\": \"\", \"_token\": \"" + token + "\" }";
            request2.Content = new StringContent(str, Encoding.UTF8, "application/json");
            request2.Headers.Add("set-cookie", "XSRF-TOKEN=AtoqUt4S0Abtf1Pg4DwXzSbLmmFUVYGRdYHYu8tR; expires=Thu, 28-Jun-2018 17:28:56 GMT; Max-Age=7200; path=/; domain=.pronocontest.com");

            var response2 = await _client.SendAsync(request2);

            var html2 = await response.Content.ReadAsStringAsync();
        }

        public async Task<IEnumerable<string>> GetRankAsync()
        {
            //await LoginAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, _url);
            request.Headers.Add("set-cookie", "XSRF-TOKEN=AtoqUt4S0Abtf1Pg4DwXzSbLmmFUVYGRdYHYu8tR; expires=Thu, 28-Jun-2018 17:28:56 GMT; Max-Age=7200; path=/; domain=.pronocontest.com");
            request.Headers.Add("set-cookie", "session=FZ59CEB87uvTzfRayio0wCKrwZbFJwf4gBaVbNq0; expires=Thu, 28-Jun-2018 17:28:56 GMT; Max-Age=7200; path=/; domain=.pronocontest.com; HttpOnly");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var html = await response.Content.ReadAsStringAsync();

                var source = WebUtility.HtmlDecode(html);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(source);

                var tbody = htmlDoc.DocumentNode.SelectSingleNode("//tbody");

                //var xml = XDocument.Parse(source);

                //var tbody = xml.Descendants("tbody").FirstOrDefault();
                //var trs = tbody.Descendants("tr").ToList();

                var ranks = new List<string>();
                //int cpt = 1;
                //foreach(var tr in trs)
                //{
                //    var tds = tr.Descendants("td").ToList();

                //    ranks.Add($"{cpt} - {tds[1].Attribute("data-sort-value")}");
                //}

                return ranks;
            }

            return Enumerable.Empty<string>();
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ApolloWorldCup.Library
{
    [DataContract]
    public class SlackMessageApi
    {
        //{"type":"message","user":"UA3869VNF","text":"pffff nimp","client_msg_id":"8e0d0397-8750-4a70-adeb-e10ebae5c71b","ts":"1529515134.000594"}

        [DataMember(Name = "client_msg_id")]
        public string Id { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "user")]
        public string User { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "ts")]
        public string TimeStamp { get; set; }
    }
}

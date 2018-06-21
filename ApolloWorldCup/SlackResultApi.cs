using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ApolloWorldCup
{
    [DataContract]
    public class SlackResultApi
    {
        //{"ok":true,"messages":[],"has_more":true}

        [DataMember(Name = "ok")]
        public bool Ok { get; set; }

        [DataMember(Name = "messages")]
        public List<SlackMessageApi> Messages { get; set; }
    }
}

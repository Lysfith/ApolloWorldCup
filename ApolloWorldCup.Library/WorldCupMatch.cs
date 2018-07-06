using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ApolloWorldCup.Library
{
    [DataContract]
    public class WorldCupMatch
    {
        [DataMember(Name = "fifa_id")]
        public long Id { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "time")]
        public string Time { get; set; }

        [DataMember(Name = "datetime")]
        public string DateTime { get; set; }

        [DataMember(Name = "winner")]
        public string Winner { get; set; }

        [DataMember(Name = "stage_name")]
        public string StageName { get; set; }

        [DataMember(Name = "home_team")]
        public WorldCupTeam HomeTeam { get; set; }

        [DataMember(Name = "away_team")]
        public WorldCupTeam AwayTeam { get; set; }

        [DataMember(Name = "home_team_events")]
        public List<WorldCupTeamEvent> HomeTeamEvents { get; set; }

        [DataMember(Name = "away_team_events")]
        public List<WorldCupTeamEvent> AwayTeamEvents { get; set; }
    }

    [DataContract]
    public class WorldCupTeam
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "fifa_code")]
        public string FifaCode { get; set; }

        [DataMember(Name = "goals")]
        public int Goals { get; set; }

        [DataMember(Name = "wins")]
        public int Wins { get; set; }

        [DataMember(Name = "draws")]
        public int Draws { get; set; }

        [DataMember(Name = "losses")]
        public int Losses { get; set; }

        [DataMember(Name = "goals_for")]
        public int GoalsFor { get; set; }

        [DataMember(Name = "goals_against")]
        public int GoalsAgainst { get; set; }
    }

    [DataContract]
    public class WorldCupTeamEvent
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "type_of_event")]
        public string Type { get; set; }

        [DataMember(Name = "player")]
        public string Player { get; set; }

        [DataMember(Name = "time")]
        public string Time { get; set; }
    }
}

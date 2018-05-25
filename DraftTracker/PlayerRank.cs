using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftTracker
{
    [JsonObject(Title = "Icons")]
    public class RankIcons
    {
        public string headline { get; set; }
        public string injury { get; set; }
        public string suspension { get; set; }
    }
    [JsonObject(Title = "Player")]
    public class RankPlayer
    {
        public string firstname { get; set; }
        public string photo { get; set; }
        public int eligible_for_offense_and_defense { get; set; }
        public string position { get; set; }
        public object auction_value { get; set; }
        public Icons icons { get; set; }
        public string lastname { get; set; }
        public int? age { get; set; }
        public string elias_id { get; set; }
        public string id { get; set; }
        public string bye_week { get; set; }
        public string pro_status { get; set; }
        public string jersey { get; set; }
        public string fullname { get; set; }
        public int rank { get; set; }
        public string pro_team { get; set; }
    }

    public class Rankings
    {
        public string source { get; set; }
        public List<RankPlayer> players { get; set; }
        public string type { get; set; }
        public int updated { get; set; }
    }
    [JsonObject(Title = "Body")]
    public class RankBody
    {
        public Rankings rankings { get; set; }
    }

    //used to deserialize json returned from 
    //http://api.cbssports.com/fantasy/players/rankings?version=3.0&SPORT=football&type=overall&response_format=json
    [JsonObject(Title = "RootObject")]
    public class PlayerRank
    {
        public RankBody body { get; set; }
        public string uriAlias { get; set; }
        public string statusMessage { get; set; }
        public string uri { get; set; }
        public int statusCode { get; set; }
    }
}

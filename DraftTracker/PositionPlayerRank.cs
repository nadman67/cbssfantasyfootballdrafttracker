using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftTracker
{






    public class Position
    {
        public string abbr { get; set; }
        public List<RankPlayer> players { get; set; }
    }
    [JsonObject(Title = "Rankings")]
    public class PPRRankings
    {
        public string source { get; set; }
        public List<Position> positions { get; set; }
        public string type { get; set; }
        public int updated { get; set; }
    }
    [JsonObject(Title = "Body")]
    public class PPRBody
    {
        public PPRRankings rankings { get; set; }
    }
    //used to deserialize json returned from 
    //http://api.cbssports.com/fantasy/players/rankings?version=3.0&SPORT=football&response_format=json
    [JsonObject(Title = "RootObject")]
    public class PositionPlayerRank
    {
        public PPRBody body { get; set; }
        public string uriAlias { get; set; }
        public string statusMessage { get; set; }
        public string uri { get; set; }
        public int statusCode { get; set; }
    }
}

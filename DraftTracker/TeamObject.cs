using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftTracker
{
    public class Owner
    {
        public int commissioner { get; set; }
        public string name { get; set; }
        public string id { get; set; }
    }
    [JsonObject(Title = "Team")]
    public class LogoTeam
    {
        public string long_abbr { get; set; }
        public int logged_in_team { get; set; }
        public string short_name { get; set; }
        public string division { get; set; }
        public string name { get; set; }
        public string logo { get; set; }
        public string abbr { get; set; }
        public List<Owner> owners { get; set; }
        public string id { get; set; }
    }
    [JsonObject(Title = "Body")]
    public class TeamBody
    {
        public List<Team> teams { get; set; }
    }
    //used to deserialize json returned from 
    //http://[leaguename].football.cbssports.com/api/league/teams?version=3.0&response_format=json&access_token=[token]&content_type=all
    [JsonObject(Title = "RootObject")]
    public class TeamObject
    {
        public TeamBody body { get; set; }
        public string uriAlias { get; set; }
        public string statusMessage { get; set; }
        public string uri { get; set; }
        public int statusCode { get; set; }
    }
}

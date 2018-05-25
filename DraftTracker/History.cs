using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftTracker
{
    public class Icons
    {
        public string headline { get; set; }
    }

    public class Player
    {
        public string firstname { get; set; }
        public int on_waivers { get; set; }
        public string photo { get; set; }
        public int eligible_for_offense_and_defense { get; set; }
        public string position { get; set; }
        public Icons icons { get; set; }
        public string lastname { get; set; }
        public int age { get; set; }
        public int is_locked { get; set; }
        public string elias_id { get; set; }
        public string opponent { get; set; }
        public int owned_by_team_id { get; set; }
        public string gametime { get; set; }
        public string profile_link { get; set; }
        public string id { get; set; }
        public string bye_week { get; set; }
        public string pro_status { get; set; }
        public object on_waivers_until { get; set; }
        public string profile_url { get; set; }
        public string jersey { get; set; }
        public string fullname { get; set; }
        public string roster_position { get; set; }
        public string pro_team { get; set; }
    }


    [JsonObject(Title = "Pick")]
    public class HistoryPick
    {
        public string overall_pick { get; set; }
        public int round { get; set; }
        public int round_pick { get; set; }
        public Player player { get; set; }
        public Team team { get; set; }
    }

    public class DraftResults
    {
        public string draft_type { get; set; }
        public string last_pick { get; set; }
        public string state { get; set; }
        public List<HistoryPick> picks { get; set; }
    }
    [JsonObject(Title = "Body")]
    public class ResultBody
    {
        public DraftResults draft_results { get; set; }
    }

    [JsonObject(Title = "RootObject")]
    public class History
    {
        public ResultBody body { get; set; }
        public string uriAlias { get; set; }
        public string statusMessage { get; set; }
        public string uri { get; set; }
        public int statusCode { get; set; }
    }
}

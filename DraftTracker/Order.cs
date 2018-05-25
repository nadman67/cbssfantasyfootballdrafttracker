using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftTracker
{
    public class Team
    {
        public string logo { get; set; }
        public string long_abbr { get; set; }
        public string abbr { get; set; }
        public string short_name { get; set; }
        public string division { get; set; }
        public string name { get; set; }
        public string id { get; set; }


    }

    public class Pick
    {
        public int number { get; set; }
        public int round { get; set; }
        public Team team { get; set; }
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool pickmade { get; set; }
    }

    public class DraftOrder
    {
        public List<Pick> picks { get; set; }
    }

    public class Body
    {
        public DraftOrder draft_order { get; set; }
    }

    [JsonObject(Title = "RootObject")]
    public class Order
    {
        public Body body { get; set; }
        public string uriAlias { get; set; }
        public string statusMessage { get; set; }
        public string uri { get; set; }
        public int statusCode { get; set; }
    }
}

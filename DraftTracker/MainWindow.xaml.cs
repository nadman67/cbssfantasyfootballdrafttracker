using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Configuration;

namespace DraftTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //class variables for draft picks and rankings
        List<Pick> _picks;
        List<HistoryPick> _draftPicks = new List<HistoryPick>();
        List<RankPlayer> _overallRankings = new List<RankPlayer>();
        List<RankPlayer> _qbRankings = new List<RankPlayer>();
        List<RankPlayer> _rbRankings = new List<RankPlayer>();
        List<RankPlayer> _wrRankings = new List<RankPlayer>();
        List<RankPlayer> _teRankings = new List<RankPlayer>();
        List<RankPlayer> _kRankings = new List<RankPlayer>();
        List<RankPlayer> _dstRankings = new List<RankPlayer>();
        Dictionary<string, string> _logoDictionary = new Dictionary<string, string>();
        //use fiddler to capture traffic after login, tokens expire 
        //public static string token = "U2FsdGVkX19syk3KN8GEzlkjZxmD-2K6p2VdIh_0AkaIkKUBADyaJUJe4vUYu1J6S5MpR1bod3DONWUM83eEwnn1wfa_VQ2mbOosjSLCPhdx_BK_yq8qdeMSveroouHRYt-hDpA3UJuSMfAz8C8azA";
        public static string token =  ConfigurationManager.AppSettings["token"];// "U2FsdGVkX19tgLKaZ_uBd-80D32AU8s30FgwrYyTaerC2fOqYFm7QbGFKKYWEjwjaJKp0C0P4wvIuwnnyC1IjMA_Z3b5w5Ej2Fenk_Nwm2RUWzdKurZMUdpmv5TNyjoo1crDBw8Z8GUPXRMazH5LJA";
        public static string leagueName = ConfigurationManager.AppSettings["leaguename"];// "18927-h2h"; //debug
        //lists to iterate over for rankBrowser html display
        List<string> _rankingList = new List<string>(7) { "", "", "", "", "", "", "" };
        int rankIncrementer = 0;//increments for display rollover
        int timer = 0;//timer int for clock 
        List<string> _resultList = new List<string>(14) { "AwaitingStart", "", "", "", "", "", "", "", "", "", "", "", "", "" };
        int _numberOfrounds = 14;//set number of rounds of draft
        int resultIncrementer = 0;
        //class variable to keep track of clock 
        string _teamPicking = "";
        DispatcherTimer dispatchTimer;
        DispatcherTimer rankingTimer;
        DispatcherTimer resultTimer;
        public MainWindow()
        {
            InitializeComponent();
            GetDraftOrder(leagueName, token);

            _overallRankings = this.GetOverallRankings();
            _qbRankings = this.GetPositionRanking("QB");
            _rbRankings = this.GetPositionRanking("RB");
            _wrRankings = this.GetPositionRanking("WR");
            _teRankings = this.GetPositionRanking("TE");
            _kRankings = this.GetPositionRanking("K");
            _dstRankings = this.GetPositionRanking("DST");

            //initialize logo dictionary
            this.GetLogos(leagueName, token);
            //getdraftresults
            //initialize to get first team picking
            _draftPicks = GetDraftPicks(leagueName, token);
            _teamPicking = _picks.Where(p => !_draftPicks.Any(p2 => Convert.ToInt64(p2.overall_pick) == p.number)).OrderBy(y => y.number).First().team.name;

            //main ticker 
            dispatchTimer = new DispatcherTimer();
            dispatchTimer.Tick += new EventHandler(dipatcherTimer_Tick);
            dispatchTimer.Interval = new TimeSpan(0, 0, 1);
            dispatchTimer.Start();

            //ticker that updates ranking browser display
            rankingTimer = new DispatcherTimer();
            rankingTimer.Tick += new EventHandler(rankingTimer_Tick);
            rankingTimer.Interval = new TimeSpan(0, 0, 5);
            rankingTimer.Start();

            //ticker that updates result browser display
            resultTimer = new DispatcherTimer();
            resultTimer.Tick += new EventHandler(resultTimer_Tick);
            resultTimer.Interval = new TimeSpan(0, 0, 5);
            resultTimer.Start();

        }



        private void resultTimer_Tick(object sender, EventArgs e)
        {
            if (_resultList[resultIncrementer] == "")//don't display if nothing
            {
                resultIncrementer = 0;
            };
            resultBrowser.NavigateToString(_resultList[resultIncrementer % _numberOfrounds]);//
            resultIncrementer++;
        }

        private void rankingTimer_Tick(object sender, EventArgs e)
        {
            rankBrowser.NavigateToString(_rankingList[rankIncrementer % 7]);//7 represents the number of position rank lists
            rankIncrementer++;
        }

        private void dipatcherTimer_Tick(object sender, EventArgs e)
        {
            _draftPicks = GetDraftPicks(leagueName, token);
            string teamPicking = _picks.Where(p => !_draftPicks.Any(p2 => Convert.ToInt64(p2.overall_pick) == p.number)).OrderBy(y => y.number).First().team.name;
            //compare if a new team is picking, if so, increment timer, if not reset timer
            if (_teamPicking == teamPicking)
            {
                timer++;
            }
            else
            {
                _teamPicking = teamPicking;
                timer = 0;
            }
            
            
            BuildResultHtml(_draftPicks);

            //get the draft picks of the team "ON THE CLOCK" and create html for display
            IEnumerable<HistoryPick> teamPicks = _draftPicks.Where(x => x.team.name == teamPicking).OrderBy(y=>y.player.position);
            string htmlTeamString = GetTeamHTML(_draftPicks, teamPicking, timer);
            
            //remove drafted players from ranking lists and create html for display
            _overallRankings = _overallRankings.Where(x => !_draftPicks.Any(dp => dp.player.id == x.id)).OrderBy(y=>y.rank).Take(20).ToList();
            string overAllHtml = GetRankHtml(_overallRankings, "Overall");
            _rankingList[0] = overAllHtml;

            _qbRankings = _qbRankings.Where(x => !_draftPicks.Any(dp => dp.player.id == x.id)).OrderBy(y => y.rank).Take(20).ToList();
            string qbHtml = GetRankHtml(_qbRankings, "QB");
            _rankingList[1] = qbHtml;


            _rbRankings = _rbRankings.Where(x => !_draftPicks.Any(dp => dp.player.id == x.id)).OrderBy(y => y.rank).Take(20).ToList();
            string rbHtml = GetRankHtml(_rbRankings, "RB");
            _rankingList[2] = rbHtml;

            _wrRankings = _wrRankings.Where(x => !_draftPicks.Any(dp => dp.player.id == x.id)).OrderBy(y => y.rank).Take(20).ToList();
            string wrHtml = GetRankHtml(_wrRankings, "WR");
            _rankingList[3] = wrHtml;

            _teRankings = _teRankings.Where(x => !_draftPicks.Any(dp => dp.player.id == x.id)).OrderBy(y => y.rank).Take(20).ToList();
            string teHtml = GetRankHtml(_teRankings, "TE");
            _rankingList[4] = teHtml;

            _kRankings = _kRankings.Where(x => !_draftPicks.Any(dp => dp.player.id == x.id)).OrderBy(y => y.rank).Take(20).ToList();
            string kHtml = GetRankHtml(_kRankings, "K");
            _rankingList[5] = kHtml;

            _dstRankings = _dstRankings.Where(x => !_draftPicks.Any(dp => dp.player.id == x.id)).OrderBy(y => y.rank).Take(20).ToList();
            string dstHtml = GetRankHtml(_dstRankings, "DST");
            _rankingList[6] = dstHtml;

            //get the last ten draft picks and display on browser control
            string last10 = GetLastTenHtml(_draftPicks);
            lastTenBrowser.NavigateToString(last10);
            TeamWebBrowser.NavigateToString(htmlTeamString);
        }



        private string GetTeamHTML(List<HistoryPick> draftPicks, string team, int timer)
        {
            //difference css if pick has taken longer than two minutes, changes border to yellow
            string titleClass = "title";
            if (timer > 120)
            {
                titleClass = "warningtitle";
            }
            //for timer
            TimeSpan span = new TimeSpan(0, 0, timer);
            string timeSpan = span.ToString(@"mm\:ss");
            //get team picks by position
            IEnumerable<HistoryPick> teamPicks = draftPicks.Where(x => x.team.name == team).OrderBy(y => y.player.position);
            string html = "<html>";
            html += "<div class='" + titleClass + "'>ON THE CLOCK:                  " + timeSpan + "</div>";
            html += "<div id='fantasyPageHeaderDynamic'><div class='clearfix'><div class='fantasyPageTeamHeader'><div class='logoCover'>" +
                    "<div class='logo'><img src = '" + _logoDictionary[team] + "' width='36' height='36' border='0' /></div><div class='editLogoCover'>&nbsp;</div></div>" +
                    "<div class='teaminfo'><div class='teamName'>" + team + "</div><div class='teamOwnersAndSettings'></div></div></div></div></div>";
            html += "<table class='data' style='width:100%;'>" + this.GetStyle() + 
                "<thead><tr class='subtitle'><td colspan = '13' > Players </td></tr>";
            html += "<tr class='label'><td>QB</td></tr>";
            html += GetPlayerHtml(teamPicks, "QB");
            html += "<tr class='label'><td>RB</td></tr>";
            html += GetPlayerHtml(teamPicks, "RB");
            html += "<tr class='label'><td>WR-TE</td></tr>";
            html += GetPlayerHtml(teamPicks, "WR-TE");
            html += "<tr class='label'><td>K</td></tr>";
            html += GetPlayerHtml(teamPicks, "K");
            html += "<tr class='label'><td>DST</td></tr>";
            html += GetPlayerHtml(teamPicks, "DST");
            html += "</thead></table>";
            html += "</html>";
            return html;
        }

        private string GetPlayerHtml(IEnumerable<HistoryPick> teamPicks, string pos)
        {
            //loop through picks and create table rows
            string html = "";
            foreach (var pick in teamPicks.Where(x => x.player.roster_position == pos))
            {
                html += "<tr>";
                html += "<td align='left' class='playerPosition'></td><td align='left' class='playerLink'>" + pick.player.fullname + " <span class='playerPositionAndTeam'>" + pick.player.roster_position + " | " + pick.player.pro_team + " </span></td><td style='width: 118px;'>" + pick.player.bye_week + "</td>";

                html += "</tr>";
            }
            return html;
        }
        
        private string GetLastTenHtml(List<HistoryPick> draftPicks)
        {
            string returnHTML = "<html>" +
            GetStyle();
            returnHTML += "<table class='data borderTop' style='width: 100%; '><tbody>";
            returnHTML += "<tr class='subtitle'><td colspan='3'>Last Ten Selections</td></tr>";
            //get last ten picks in reverse order, loop through them and create table rows
            foreach (var pic in _draftPicks.Skip(Math.Max(0, _draftPicks.Count() - 10)).Reverse())
            {
                returnHTML += "<tr align='right' valign='top'><td align='left'>" + pic.overall_pick + "</td><td align='left'>" + pic.team.name + "</td><td align='left' class='playerLink'>" + pic.player.fullname + " <span class='playerPositionAndTeam'>" + pic.player.position + " | " + pic.player.pro_team + " </span></td></tr>";
            }
            returnHTML += "</html>";
            return returnHTML;
        }
        private void BuildResultHtml(List<HistoryPick> draftPicks)
        {
            //lists of picks by round
           List<List<HistoryPick>> byRound = new List<List<HistoryPick>>();
            for (int i = 1; i <= _numberOfrounds; i++)
            {
                byRound.Add(_draftPicks.Where(x => x.round == i).ToList());
            }
            //iterate through each round and create html for display
            foreach (var roundPicks in byRound)
            {
                //if no picks, don't create html
                if (roundPicks.Count > 0)
                {
                    string html = this.GetStyle(); 

                    html += "<table class='data borderTop' style='width: 100 %; '>";
                    html += "<tr class='subtitle'><td colspan=3>Round " + roundPicks.First().round + "</td></tr>";
                    foreach (var pic in roundPicks)
                    {
                        html += "<tr align='right' valign='top'><td align='left'>" + pic.round_pick + "</td><td align='left'>" + pic.team.name + "</td><td align='left' class='playerLink'>" + pic.player.fullname + " <span class='playerPositionAndTeam'>" + pic.player.position + " | " + pic.player.pro_team + " </span></td></tr>";
                    }
                    html += "</table>";
                    _resultList[roundPicks.First().round - 1] = html;
                }
                
            }

            
        }

        //css styles for display
        private string GetStyle()
        {
            return "<style>span.playerPositionAndTeam{font-family:proxima-nova,'Helvetica','Arial',sans-serif;font-weight:400;font-style:normal;font-size:11px;white-space:nowrap;display:inline-block}" + 
                                 "td.playerLink{font-family:proxima-nova,'Helvetica','Arial',sans-serif;font-weight:700;font-style:normal;font-size:19px;color:#232323}"  +
                                 "tr.subtitle td{color:#232323;padding:10px;font-family:proxima-nova,'Helvetica','Arial',sans-serif;font-weight:400;font-style:normal;font-size:18px;background-color:#e6e7e8;text-transform:uppercase}" +
                                 "div.title {color:#ffffff;padding:10px;font-family:proxima-nova,'Helvetica','Arial',sans-serif;font-weight:800;font-style:normal;font-size:24px;background-color:#0026ff;text-transform:uppercase}" +
                                 "div.warningtitle {color:#ffffff;padding:10px;font-family:proxima-nova,'Helvetica','Arial',sans-serif;font-weight:800;font-style:normal;font-size:24px;background-color:#f4f000;text-transform:uppercase}" +
                                 "table.data tr.label th,table.data tr.label td,table.data tr.superheader th,table.data tr.superheader td{font-family:proxima-nova,'Helvetica','Arial',sans-serif;font-weight:700;font-style:normal;font-size:12px;background-color:#f5f5f6;color:#232323;padding:7px 5px;text-transform:uppercase}" +
                                 "table.data tr.title{background-color:#e6e7e8;border:0}"+
                                 ".fantasyPageTeamHeader .logo{float:left;margin:20px 20px 0 20px}" +
                                 ".fantasyPageTeamHeader .teaminfo .teamName{font-family:proxima-nova,'Helvetica','Arial',sans-serif;font-weight:800;font-style:normal;font-size:25px;color:#2362c6;line-height:25px}" +
                                 ".fantasyPageTeamHeader .teaminfo .teamOwnersAndSettings .teamOwners{font-family:proxima-nova,'Helvetica','Arial',sans-serif;font-weight:500;font-style:normal;font-size:15px;color:#000000;line-height:25px}" +
                                 "</style>";
            
        }

        private string GetRankHtml(List<RankPlayer> rankList, string pos)
        {
            string returnHTML = "<html>" +
                GetStyle();
            returnHTML += "<table class='data' style='height: 48px; width: 346px; '><tbody>";
            returnHTML += "<tr class='subtitle'><td colspan='2'>Best " + pos + " Available</td></tr>";
            returnHTML += "<tr class='label'><td style='width: 1000px; '>PLAYER</td><td style='width: 10px; '>BYE</td></tr>";
            foreach (var rank in rankList)
            {
                returnHTML += "<tr>";
                returnHTML += "<td align='left' class='playerLink'>" + rank.fullname + " <span class='playerPositionAndTeam'>" + rank.position + " | " + rank.pro_team + " </span></td><td style='width: 118px;'>" + rank.bye_week + "</td>";

                returnHTML += "</tr>";
            }
            returnHTML += "</html>";
            return returnHTML;
        }

        private void GetLogos(string league, string token)
        {
            //prefetch logo url
            string content = GetApiRequest("http://" + league + ".football.cbssports.com/api/league/teams?version=3.0&response_format=json&access_token=" + token + "&content_type=all");
            TeamObject teams = JsonConvert.DeserializeObject<TeamObject>(content);
            foreach (var t in teams.body.teams)
            {
                _logoDictionary.Add(t.name, t.logo);
            }
        }
        private void GetDraftOrder(string league, string token)
        {
            string content = GetApiRequest("http://" + league + ".football.cbssports.com/api/league/draft/order?version=3.0&response_format=json&access_token=" + token + "&content_type=all");
            Order order = JsonConvert.DeserializeObject<Order>(content);
            _picks = order.body.draft_order.picks;
        }
        private List<HistoryPick> GetDraftPicks(string league, string token)
        {
            string content = GetApiRequest("http://" + league + ".football.cbssports.com/api/league/draft/results?version=3.0&response_format=json&access_token=" + token + "&content_type=all");
            History history = JsonConvert.DeserializeObject<History>(content);
            //if we've got new picks, add them to class variable
            if (history.body.draft_results.picks != null && history.body.draft_results.picks.Count != _draftPicks.Count)
            {
                var newPicks = history.body.draft_results.picks.Where(h => !_draftPicks.Any(dp => dp.overall_pick == h.overall_pick));
                _draftPicks.AddRange(newPicks);
                
            }
            return _draftPicks;
        }
        private List<RankPlayer> GetOverallRankings()
        {
            string content = GetApiRequest("http://api.cbssports.com/fantasy/players/rankings?version=3.0&SPORT=football&type=overall&response_format=json");
            PlayerRank ranks = JsonConvert.DeserializeObject<PlayerRank>(content);
            return ranks.body.rankings.players;
        }
        private List<RankPlayer> GetPositionRanking(string v)
        {
            string content = GetApiRequest("http://api.cbssports.com/fantasy/players/rankings?version=3.0&SPORT=football&response_format=json");
            PositionPlayerRank ranks = JsonConvert.DeserializeObject<PositionPlayerRank>(content);
            return ranks.body.rankings.positions.Where(x => x.abbr == v).First().players;
             
        }
        private string GetApiRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            
            WebResponse response = request.GetResponse();
            var stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            return sr.ReadToEnd();


        }

        //used to eliminate scroll bars on webbrowser
        private void TeamWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            string script = "document.body.style.overflow ='hidden'";
            WebBrowser wb = (WebBrowser)sender;
            wb.InvokeScript("execScript", new Object[] { script, "JavaScript" });
        }
    }
}

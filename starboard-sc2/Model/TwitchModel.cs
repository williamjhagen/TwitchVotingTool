using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
namespace Starboard.Model
{
    using Starboard.MVVM;

    public class TwitchModel : ObservableObject, ICloneable
    {
        private string channelName = "";
        private string username = "TheHamsterBot";
        private string oauthCode = "oauth:5zi35qpdyv0htl68tpam90rfxcahjz";
        private string server = "irc.twitch.tv";
        private int port = 6667;
        private string player1 = string.Empty;
        private string player2 = string.Empty;
        private string player1Score = "50";
        private string player2Score = "50";
        private cIRC Irc;
        private bool validChannelName = false;
        private string connectionText = "Connection Status: Waiting for User Input";
        private string statusColor ="Red";
        #region properties
        
       // public string Player1
       // {
       //     get { return player1 != string.Empty ? string.Format("{0} %{1}", player1, player1Score) : string.Empty; }
       // }
       // public string Player2
       // {
       //     get { return player2 != string.Empty ? string.Format("{0} %{1}", player2, player2Score) : string.Empty; }
       // }
       public String ConnectionText
        {
            get { return connectionText; }
            set
            {
                connectionText = value;
                RaisePropertyChanged("ConnectionText");
            }
        }
        public string StatusColor
        {
            get { return statusColor; }
            set
            {
                statusColor = value;
                RaisePropertyChanged("StatusColor");
            }
        }
        public string Player1Score
        {
            get { return player1Score; }
            set
            {
                player1Score = value;
                RaisePropertyChanged("Player1Score");
            }
        }
        public string Player2Score
        {
            get { return player2Score; }
            set
            {
                player2Score = value;
                RaisePropertyChanged("Player2Score");
            }
        }
        public bool ValidChannelName
        {
            get { return validChannelName; }
            set
            {
                validChannelName = value;
                RaisePropertyChanged("ValidChannelName");
            }
        }
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
            }
        }

        public string OAuthCode
        {
            get { return oauthCode; }
            //set { oauthCode = value; }
        }

        public string Server
        {
            get { return server; }
        }

        public int Port
        {
            get { return port; }
        }

        public string ChannelName
        {
            get { return channelName; }
            set {
                channelName = value;
                RaisePropertyChanged("ChannelName");
            }
        }

        #endregion

        public void InitIrc(string player1, string player2)
        {
            if (ValidateString(ChannelName))
            {
                UpdateStatus("Connection Status: Attempting to connect to Twitch", "Yellow");
                this.player1 = string.Format("!vote {0}", player1);
                this.player2 = string.Format("!vote {0}", player2);
                Irc = new cIRC(Server, Port, Username, ChannelName, OAuthCode, player1.ToLower(), player2.ToLower(), UpdateInfo, UpdateStatus);
            }
            else
            {
                UpdateStatus("Connection Status: Invalid channel name, waiting for user input", "Red");
                ValidChannelName = false;
            }
        }

        private bool ValidateString(string input)
        {
            if (input == string.Empty) return false;
            if (input[0] == '_') return false;
            if (input.Length < 4 || input.Length > 25) return false;
            foreach(char c in input)
            {
                if (!(
                    (c > 47 && c < 58)
                    || (c > 64 && c < 91)
                    || (c > 96 && c < 123)
                    || (c == ' ') 
                    || (c == '_')
                    )) return false;
            }
            return true;
        }

        public void UpdateInfo(string p1value, string p2value)
        {
                Player1Score =  p1value;
                Player2Score = p2value;
        }
        public void UpdateStatus(string text, string color)
        {
            ConnectionText = text;
            StatusColor = color;
        }
        public void DeInitIrc()
        {
            if (Irc == null) return;
            Irc.Kill();
            Irc = null;
            player1 = string.Empty;
            player2 = string.Empty;
            player1Score = "50";
            player2Score = "50";
        }

        public object Clone()
        {
            TwitchModel twitch = new TwitchModel();
            twitch.ChannelName = this.channelName;
            //twitch.OAuthCode = this.oauthCode;
            //twitch.Username = this.username;

            return twitch;
        }
    }


}

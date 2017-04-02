using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace System.Net {
	#region Delegates
	public delegate void CommandReceived(string IrcCommand);
	public delegate void TopicSet(string IrcChannel, string IrcTopic);
	public delegate void TopicOwner(string IrcChannel, string IrcUser, string TopicDate);
	public delegate void NamesList(string UserNames);
	public delegate void ServerMessage(string ServerMessage);
	public delegate void Join(string IrcChannel, string IrcUser);
	public delegate void Part(string IrcChannel, string IrcUser);
	public delegate void Mode(string IrcChannel, string IrcUser, string UserMode);
	public delegate void NickChange(string UserOldNick, string UserNewNick);
	public delegate void Kick(string IrcChannel, string UserKicker, string UserKicked, string KickMessage);
	public delegate void Quit(string UserQuit, string QuitMessage);
    public delegate void UpdateInfo(string value1, string value2);
    #endregion

    public class IRC {
        #region Events
        public event CommandReceived eventReceiving;
        public event TopicSet eventTopicSet;
        public event TopicOwner eventTopicOwner;
        public event NamesList eventNamesList;
        public event ServerMessage eventServerMessage;
        public event Join eventJoin;
        public event Part eventPart;
        public event Mode eventMode;
        public event NickChange eventNickChange;
        public event Kick eventKick;
        public event Quit eventQuit;
        #endregion

        #region Private Variables
        private string ircServer;
        private int ircPort;
        private string ircNick;
        private string ircUser;
        private string ircRealName;
        private string ircChannel;
        private string ircPass;
        private bool isInvisible;
        private TcpClient ircConnection;
        private NetworkStream ircStream;
        private StreamWriter ircWriter;
        private StreamReader ircReader;
        public Thread IrcThread;
        private UpdateInfo relay;
        #endregion

        #region Properties
        public string IrcPass {
            get { return this.ircPass; }
            set { this.ircPass = value; }
        }
        public string IrcServer {
            get { return this.ircServer; }
            set { this.ircServer = value; }
        } /* IrcServer */

        public int IrcPort {
            get { return this.ircPort; }
            set { this.ircPort = value; }
        } /* IrcPort */

        public string IrcNick {
            get { return this.ircNick; }
            set { this.ircNick = value; }
        } /* IrcNick */

        public string IrcUser {
            get { return this.ircUser; }
            set { this.ircUser = value; }
        } /* IrcUser */

        public string IrcRealName {
            get { return this.ircRealName; }
            set { this.ircRealName = value; }
        } /* IrcRealName */

        public string IrcChannel {
            get { return this.ircChannel; }
            set { this.ircChannel = value; }
        } /* IrcChannel */

        public bool IsInvisble {
            get { return this.isInvisible; }
            set { this.isInvisible = value; }
        } /* IsInvisible */

        public TcpClient IrcConnection {
            get { return this.ircConnection; }
            set { this.ircConnection = value; }
        } /* IrcConnection */

        public NetworkStream IrcStream {
            get { return this.ircStream; }
            set { this.ircStream = value; }
        } /* IrcStream */

        public StreamWriter IrcWriter {
            get { return this.ircWriter; }
            set { this.ircWriter = value; }
        } /* IrcWriter */

        public StreamReader IrcReader {
            get { return this.ircReader; }
            set { this.ircReader = value; }
        } /* IrcReader */
        #endregion

        #region Constructor
        public IRC(string IrcNick, string IrcChannel, string IrcPass, UpdateInfo relay) {
            this.IrcNick = IrcNick;
            this.IrcUser = System.Environment.MachineName;
            this.IrcRealName = "TheHamsterBot";
            this.IrcChannel = IrcChannel; 
            this.IsInvisble = false;
            this.IrcPass = IrcPass;
            this.relay = relay;
        } /* IRC */
        #endregion
        void CreateConnection()
        {
        }
        #region Public Methods
        public void Connect(string IrcServer, int IrcPort) {
            //no longer necessary after Twitch updated their API
            //GetConnectionIp(out IrcServer,out IrcPort, IrcChannel);
            if (IrcServer == null)
            {
                relay("Connection Status: Invalid channel name, waiting for user input", "Red");
                return;
            }
            this.IrcPort = IrcPort;
            this.IrcServer = IrcServer;

            try
            {
                this.IrcConnection = new TcpClient(this.IrcServer, this.IrcPort);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                relay("Connection Status: Server did not respond, check channel name, and try again.", "Red");
                return;
            }
            this.IrcStream = this.IrcConnection.GetStream();
            this.IrcReader = new StreamReader(this.IrcStream);
            this.IrcWriter = new StreamWriter(this.IrcStream);

            // Authenticate our user
            this.ircWriter.WriteLine(String.Format("PASS {0}", this.IrcPass));
            this.IrcWriter.Flush();
            this.IrcWriter.WriteLine(String.Format("NICK {0}", this.IrcNick));
            this.IrcWriter.Flush();
            this.IrcWriter.WriteLine(String.Format("JOIN {0}", (IrcChannel[0] == '#' ? IrcChannel : String.Format("{0}{1}", "#", IrcChannel))));
            this.IrcWriter.Flush();

            IrcThread = new Thread(IrcLoop);
            IrcThread.IsBackground = true;
            IrcThread.Start();
            relay(string.Format("Connection Status: Connected to channel - {0}", IrcChannel), "Green");

        } /* Connect */
        #endregion
        private void GetConnectionIp(out string server, out int port, string channel)
        {
            WebRequest wrGETURL;
            string sURL = string.Format("http://api.twitch.tv/api/channels/{0}/chat_properties", channel);
            wrGETURL = WebRequest.Create(sURL);

            Stream objStream;
            try {
                objStream = wrGETURL.GetResponse().GetResponseStream();
            }
            catch
            {
                throw new WebException();
            }

            StreamReader objReader = new StreamReader(objStream);

            string html = objReader.ReadToEnd();
            int ipStart = html.IndexOf(":[\"") + 3;
            string sub = html.Substring(ipStart, html.IndexOf(",", ipStart) - ipStart - 1);
            int seperator = sub.IndexOf(":");
            server = sub.Substring(0, seperator);
            Int32.TryParse(sub.Substring(seperator+1), out port);
 
        }
        public void KillIrcLoop()
        {
          
            if (IrcThread.IsAlive)
            {
                IrcThread.Abort();
                this.IrcWriter.Close();
                this.IrcReader.Close();
                this.IrcConnection.Close();
                relay("Connection Status: Waiting for User Input", "Red");
            }
        }
		private void IrcLoop()
        {
            // Listen for commands
            while (true)
            {
                string ircCommand;
                while ((ircCommand = this.IrcReader.ReadLine()) != null)
                {
                    if (eventReceiving != null) { this.eventReceiving(ircCommand); }

                    string[] commandParts = new string[ircCommand.Split(' ').Length];
                    commandParts = ircCommand.Split(' ');
                    if (commandParts[0].Substring(0, 1) == ":")
                    {
                        commandParts[0] = commandParts[0].Remove(0, 1);
                    }

                    if (commandParts[0] == this.IrcServer)
                    {
                        // Server message
                        switch (commandParts[1])
                        {
                            case "332": this.IrcTopic(commandParts); break;
                            case "333": this.IrcTopicOwner(commandParts); break;
                            case "353": this.IrcNamesList(commandParts); break;
                            case "366": /*this.IrcEndNamesList(commandParts);*/ break;
                            case "372": /*this.IrcMOTD(commandParts);*/ break;
                            case "376": /*this.IrcEndMOTD(commandParts);*/ break;
                            default: this.IrcServerMessage(commandParts); break;
                        }
                    }
                    else if (commandParts[0] == "PING")
                    {
                        // Server PING, send PONG back
                        this.IrcPing(commandParts);
                    }
                    else
                    {
                        // Normal message
                        string commandAction = commandParts[1];
                        switch (commandAction)
                        {
                            case "JOIN": this.IrcJoin(commandParts); break;
                            case "PART": this.IrcPart(commandParts); break;
                            case "MODE": this.IrcMode(commandParts); break;
                            case "NICK": this.IrcNickChange(commandParts); break;
                            case "KICK": this.IrcKick(commandParts); break;
                            case "QUIT": this.IrcQuit(commandParts); break;
                        }
                    }
                }
            }
            this.IrcWriter.Close();
            this.IrcReader.Close();
            this.IrcConnection.Close();
        }
		#region Private Methods
		#region Server Messages
		private void IrcTopic(string[] IrcCommand) {
			string IrcChannel = IrcCommand[3];
			string IrcTopic = "";
			for (int intI = 4; intI < IrcCommand.Length; intI++) {
				IrcTopic += IrcCommand[intI] + " ";
			}
			if (eventTopicSet != null) { this.eventTopicSet(IrcChannel, IrcTopic.Remove(0, 1).Trim()); }
		} /* IrcTopic */
		
		private void IrcTopicOwner(string[] IrcCommand) {
			string IrcChannel = IrcCommand[3];
			string IrcUser = IrcCommand[4].Split('!')[0];
			string TopicDate = IrcCommand[5];
			if (eventTopicOwner != null) { this.eventTopicOwner(IrcChannel, IrcUser, TopicDate); }
		} /* IrcTopicOwner */
		
		private void IrcNamesList(string[] IrcCommand) {
		  string UserNames = "";
			for (int intI = 5; intI < IrcCommand.Length; intI++) {
				UserNames += IrcCommand[intI] + " ";
			}
			if (eventNamesList != null) { this.eventNamesList(UserNames.Remove(0, 1).Trim()); }
		} /* IrcNamesList */
		
		private void IrcServerMessage(string[] IrcCommand) {
            Console.WriteLine(IrcCommand[0]);
			string ServerMessage = "";
			for (int intI = 1; intI < IrcCommand.Length; intI++) {
				ServerMessage += IrcCommand[intI] + " ";
			}
			if (eventServerMessage != null) { this.eventServerMessage(ServerMessage.Trim()); }
		} /* IrcServerMessage */
		#endregion
		
		#region Ping
		private void IrcPing(string[] IrcCommand) {
			string PingHash = "";
			for (int intI = 1; intI < IrcCommand.Length; intI++) {
				PingHash += IrcCommand[intI] + " ";
			}
			this.IrcWriter.WriteLine("PONG " + PingHash);
			this.IrcWriter.Flush();
		} /* IrcPing */
		#endregion
		
		#region User Messages
		private void IrcJoin(string[] IrcCommand) {
			string IrcChannel = IrcCommand[2];
			string IrcUser = IrcCommand[0].Split('!')[0];
			if (eventJoin != null) { this.eventJoin(IrcChannel.Remove(0, 1), IrcUser); }
		} /* IrcJoin */
		
		private void IrcPart(string[] IrcCommand) {
			string IrcChannel = IrcCommand[2];
			string IrcUser = IrcCommand[0].Split('!')[0];
			if (eventPart != null) { this.eventPart(IrcChannel, IrcUser); }
		} /* IrcPart */
		
		private void IrcMode(string[] IrcCommand) {
			string IrcChannel = IrcCommand[2];
			string IrcUser = IrcCommand[0].Split('!')[0];
			string UserMode = "";
			for (int intI = 3; intI < IrcCommand.Length; intI++) {
				UserMode += IrcCommand[intI] + " ";
			}
			if (UserMode.Substring(0, 1) == ":") {
				UserMode = UserMode.Remove(0, 1);
			}
			if (eventMode != null) { this.eventMode(IrcChannel, IrcUser, UserMode.Trim()); }
		} /* IrcMode */
		
		private void IrcNickChange(string[] IrcCommand) {
			string UserOldNick = IrcCommand[0].Split('!')[0];
			string UserNewNick = IrcCommand[2].Remove(0, 1);
			if (eventNickChange != null) { this.eventNickChange(UserOldNick, UserNewNick); }
		} /* IrcNickChange */
		
		private void IrcKick(string[] IrcCommand) {
			string UserKicker = IrcCommand[0].Split('!')[0];
			string UserKicked = IrcCommand[3];
			string IrcChannel = IrcCommand[2];
			string KickMessage = "";
			for (int intI = 4; intI < IrcCommand.Length; intI++) {
				KickMessage += IrcCommand[intI] + " ";
			}
			if (eventKick != null) { this.eventKick(IrcChannel, UserKicker, UserKicked, KickMessage.Remove(0, 1).Trim()); }
		} /* IrcKick */
		
		private void IrcQuit(string[] IrcCommand) {
			string UserQuit = IrcCommand[0].Split('!')[0];
			string QuitMessage = "";
			for (int intI = 2; intI < IrcCommand.Length; intI++) {
				QuitMessage += IrcCommand[intI] + " ";
			}
			if (eventQuit != null) { this.eventQuit(UserQuit, QuitMessage.Remove(0, 1).Trim()); }

		} /* IrcQuit */
		#endregion
		#endregion
	} /* IRC */
} /* System.Net */
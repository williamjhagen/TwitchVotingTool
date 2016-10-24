using System;
using System.Net;

namespace Starboard.Model
{
	public class cIRC {
		private IRC IrcObject;
        private int p1Score;
        private int p2Score;
        private string p1;
        private string p2;
        private UpdateInfo relayScore;
        public cIRC(string IrcServer, int IrcPort, string IrcUser, string IrcChan, string IrcPass, string p1, string p2, UpdateInfo relayScore, UpdateInfo relayStatus) {
            //IrcObject = new IRC("CumpsD", "#mypreciousss");
            this.p1 = p1;
            this.p2 = p2;
            this.relayScore = relayScore;
            IrcObject = new IRC(IrcUser, IrcChan, IrcPass, relayStatus);
			// Assign events
			IrcObject.eventReceiving += new CommandReceived(IrcCommandReceived);
			IrcObject.eventTopicSet += new TopicSet(IrcTopicSet);
			IrcObject.eventTopicOwner += new TopicOwner(IrcTopicOwner);
			IrcObject.eventNamesList += new NamesList(IrcNamesList);
			IrcObject.eventServerMessage += new ServerMessage(IrcServerMessage);
			IrcObject.eventJoin += new Join(IrcJoin);
			IrcObject.eventPart += new Part(IrcPart);
			IrcObject.eventMode += new Mode(IrcMode);
			IrcObject.eventNickChange += new NickChange(IrcNickChange);
			IrcObject.eventKick += new Kick(IrcKick);
			IrcObject.eventQuit += new Quit(IrcQuit);

            // Connect to server
            //IrcObject.Connect("efnet.xs4all.nl", 6667);	
			IrcObject.Connect(IrcServer, IrcPort);	
		} /* cIRC */
		
        public void Kill()
        {
            relayScore("50", "50");
            IrcObject.KillIrcLoop();
        }

		private void IrcCommandReceived(string IrcCommand) {
            Console.WriteLine(IrcCommand);
            int indexP1 = IrcCommand.ToLower().IndexOf(p1);
            int indexP2 = IrcCommand.ToLower().IndexOf(p2);
            if (indexP1 != -1)
            {
                ++p1Score;
                relayScore(P1Percent.ToString(), P2Percent.ToString());
            }
            else if (indexP2 != -1)
            {
                ++p2Score;
                relayScore(P1Percent.ToString(), P2Percent.ToString());
            }
        } /* IrcCommandReceived */
		
        public double Total
        {
            get { return p1Score + p2Score; }
        }

        public int P1Percent
        {
            get { return (int)Math.Round((p1Score / Total) * 100); }
        }

        public int P2Percent
        {
            get { return (int)Math.Round((p2Score / Total) * 100); }
        }

		private void IrcTopicSet(string IrcChan, string IrcTopic) {
			Console.WriteLine(String.Format("Topic of {0} is: {1}", IrcChan, IrcTopic));
		} /* IrcTopicSet */
		
		private void IrcTopicOwner(string IrcChan, string IrcUser, string TopicDate) {
			Console.WriteLine(String.Format("Topic of {0} set by {1} on {2} (unixtime)", IrcChan, IrcUser, TopicDate));
		} /* IrcTopicSet */
		
		private void IrcNamesList(string UserNames) {
			Console.WriteLine(String.Format("Names List: {0}", UserNames));
		} /* IrcNamesList */
		
		private void IrcServerMessage(string ServerMessage) {
			//Console.WriteLine(String.Format("Server Message: {0}", ServerMessage));
		} /* IrcNamesList */
		
		private void IrcJoin(string IrcChan, string IrcUser) {
			Console.WriteLine(String.Format("{0} joins {1}", IrcUser, IrcChan));
			IrcObject.IrcWriter.WriteLine(String.Format("NOTICE {0} :Hello {0}, welcome to {1}!", IrcUser, IrcChan));
			IrcObject.IrcWriter.Flush ();	
		} /* IrcJoin */
		
		private void IrcPart(string IrcChan, string IrcUser) {
			Console.WriteLine(String.Format("{0} parts {1}", IrcUser, IrcChan));
		} /* IrcPart */
		
		private void IrcMode(string IrcChan, string IrcUser, string UserMode) {
			if (IrcUser != IrcChan) {
				Console.WriteLine(String.Format("{0} sets {1} in {2}", IrcUser, UserMode, IrcChan));
			}
		} /* IrcMode */
		
		private void IrcNickChange(string UserOldNick, string UserNewNick) {
			Console.WriteLine(String.Format("{0} changes nick to {1}", UserOldNick, UserNewNick));
		} /* IrcNickChange */
		
		private void IrcKick(string IrcChannel, string UserKicker, string UserKicked, string KickMessage) {
			Console.WriteLine(String.Format("{0} kicks {1} out {2} ({3})", UserKicker, UserKicked, IrcChannel, KickMessage));
		} /* IrcKick */
		
		private void IrcQuit(string UserQuit, string QuitMessage) {
			Console.WriteLine(String.Format("{0} has quit IRC ({1})", UserQuit, QuitMessage));
		} /* IrcQuit */
	} /* cIRC */
} /* cIRC */
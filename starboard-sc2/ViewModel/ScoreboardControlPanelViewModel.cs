// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScoreboardControlPanelViewModel.cs" company="Starboard">
//   Copyright © 2011 All Rights Reserved
// </copyright>
// <summary>
//   View model for the main scoreboard control view, which allows the user to
//   change the player information, show the scoreboard, and all other critical updates.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Starboard.ViewModel
{
    using System.Windows.Input;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Controls;
    using Starboard.Model;
    using Starboard.MVVM;

    /// <summary>
    /// The scoreboard control panel view model.
    /// </summary>
    public class ScoreboardControlPanelViewModel : ObservableObject
    {
        #region Constants and Fields

        /// <summary> Reference to the active scoreboard view model. </summary>
        private readonly ScoreboardControlViewModel scoreboard;

        /// <summary> The backing field for the swap players command. </summary>
        private ICommand swapPlayersCommand;

        /// <summary> The backing field for the toggle announcement command. </summary>
        private ICommand toggleAnnouncementCommand;

        /// <summary> The backing field for the toggle scoreboard visible command. </summary>
        private ICommand toggleScoreboardVisibleCommand;

        /// <summary> The backing field for the toggle subbar command. </summary>
        private ICommand toggleSubbarCommand;

        /// <summary> The backing field for the connect to twitch chat command </summary>
        private ICommand connectToTwitchCommand;

        /// <summary> The backing field for the connect to twitch chat command </summary>
        private ICommand disconnectFromTwitchCommand;


        #endregion

        #region Constructors and Destructors

        /// <summary> Initializes a new instance of the <see cref="ScoreboardControlPanelViewModel"/> class. </summary>
        /// <param name="scoreboard"> The scoreboard view model. </param>
        public ScoreboardControlPanelViewModel(ScoreboardControlViewModel scoreboard)
        {
            this.scoreboard = scoreboard;
        }

        #endregion

        #region Public Properties

        /// <summary> Gets the active scoreboard view model. </summary>
        public ScoreboardControlViewModel Scoreboard
        {
            get
            {
                return this.scoreboard;
            }
        }
        
        /// <summary> initiates IRC connection </summary>
        public ICommand ConnectToTwitchCommand
        {
            get
            {
                return this.connectToTwitchCommand ?? (this.connectToTwitchCommand = new RelayCommand(this.ConnectToTwitch));
            }
        }
        /// <summary> initiates IRC connection </summary>
        public ICommand DisconnectFromTwitchCommand
        {
            get
            {
                return this.disconnectFromTwitchCommand ?? (this.disconnectFromTwitchCommand = new RelayCommand(this.DisconnectFromTwitch));
            }
        }
        /// <summary> Gets a command to swap the players on the scoreboard. </summary>
        public ICommand SwapPlayersCommand
        {
            get
            {
                return this.swapPlayersCommand ?? (this.swapPlayersCommand = new RelayCommand(this.SwapPlayers));
            }
        }

        /// <summary> Gets a command to toggle the announcement display. </summary>
        public ICommand ToggleAnnouncementCommand
        {
            get
            {
                return this.toggleAnnouncementCommand
                       ?? (this.toggleAnnouncementCommand = new RelayCommand(this.ToggleAnnouncement));
            }
        }

        /// <summary> Gets a command to toggle whether the scoreboard is visible. </summary>
        public ICommand ToggleScoreboardVisibleCommand
        {
            get
            {
                return this.toggleScoreboardVisibleCommand
                       ?? (this.toggleScoreboardVisibleCommand = new RelayCommand(this.ToggleScoreboardVisible));
            }
        }

        /// <summary> Gets a command to toggle whether the subbar is visible. </summary>
        public ICommand ToggleSubbarCommand
        {
            get
            {
                return this.toggleSubbarCommand ?? (this.toggleSubbarCommand = new RelayCommand(this.ToggleSubbar));
            }
        }

        #endregion

        #region Public Methods
        
        public void ConnectToTwitch()
        {
            this.scoreboard.Twitch.InitIrc(Scoreboard.Player1.Name, Scoreboard.Player2.Name);
        }
        public void DisconnectFromTwitch()
        {
            this.scoreboard.Twitch.DeInitIrc();
        }

        /// <summary> Swaps the position of the two players, flipping left and right sides. </summary>
        public void SwapPlayers()
        {
            var player2 = (Player)this.Scoreboard.Player1.Clone();
            var player1 = (Player)this.Scoreboard.Player2.Clone();

            this.Scoreboard.Player1.Name = player1.Name;
            this.Scoreboard.Player1.Color = player1.Color;
            this.Scoreboard.Player1.Score = player1.Score;

            this.Scoreboard.Player2.Name = player2.Name;
            this.Scoreboard.Player2.Color = player2.Color;
            this.Scoreboard.Player2.Score = player2.Score;

            string temp = this.Scoreboard.Twitch.Player2Score;
            this.Scoreboard.Twitch.Player2Score = this.Scoreboard.Twitch.Player1Score;
            this.Scoreboard.Twitch.Player1Score = temp;
        }

        /// <summary> Toggles the visibility of the announcement board. </summary>
        public void ToggleAnnouncement()
        {
            this.Scoreboard.IsAnnouncementShowing = !this.Scoreboard.IsAnnouncementShowing;
        }

        /// <summary> Toggles the visibility of the scoreboard. </summary>
        public void ToggleScoreboardVisible()
        {
            var display = MainWindowViewModel.DisplayWindow;

            if (display.IsVisible)
            {
                display.Hide();
            }
            else
            {
                display.Show();
            }
        }

        /// <summary> Toggles whether the subbar is visible. </summary>
        public void ToggleSubbar()
        {
            this.Scoreboard.IsSubbarShowing = !this.Scoreboard.IsSubbarShowing;
        }

        #endregion
    }
}
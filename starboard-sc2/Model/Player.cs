﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Player.cs" company="Starboard">
//   Copyright © 2011 All Rights Reserved
// </copyright>
// <summary>
//   Model for a Player, containing the player's name, color, race and score.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Starboard.Model
{
    using System;
    using System.Windows.Input;
    using System.Windows.Media;
    using Starboard.MVVM;

    /// <summary> Model for a Player, containing the player's name, color, race and score.</summary>
    public class Player : ObservableObject, ICloneable
    {
        /// <summary> Backing for the ResetCommand command. </summary>
        private ICommand resetCommand;

        /// <summary> The player's name, defaulting to "Player". </summary>
        private string name = string.Empty;

        /// <summary> The player's color, default to "Unknown" </summary>
        private string color = "Black";

        /// <summary> The player's race, defaulting to terran. </summary>
        private Race race = Race.Unknown;

        /// <summary> The current score, starting at 0. </summary>
        private int score;

        /// <summary> Gets a command which resets the player. </summary>
        public ICommand ResetCommand
        {
            get
            {
                return this.resetCommand ?? (this.resetCommand = new RelayCommand(this.Reset));
            }
        }

        /// <summary> Gets or sets the name of the player. </summary>
        public string Name
        {
            get 
            { 
                return this.name; 
            }

            set
            {
                this.name = value;
                RaisePropertyChanged("Name");
            }
        }

        /// <summary> Gets or sets the color of the player. </summary>
        public string Color
        {
            get
            {
                return this.color;
            }

            set
            {
                this.color = value;
                RaisePropertyChanged("Color");
            }
        }

        /// <summary> Gets or sets the race of the player. </summary>
        public Race Race
        {
            get
            {
                return this.race;
            }

            set
            {
                this.race = value;
                RaisePropertyChanged("Race");
            }
        }

        /// <summary> Gets or sets the player's current score. </summary>
        public int Score
        {
            get
            {
                return this.score;
            }

            set
            {
                this.score = value < 0 ? 0 : value;
                RaisePropertyChanged("Score");
            }
        }

        /// <summary> Resets the player back to default status. </summary>
        public void Reset()
        {
            this.Name = string.Empty;
            this.Color = "#000";
            this.Race = Race.Unknown;
            this.Score = 0;
        }

        public object Clone()
        {
            Player player = new Player();
            player.Color = this.Color;
            player.Name = this.Name;
            player.Race = this.Race;
            player.Score = this.Score;

            return player;
        }
    }
}

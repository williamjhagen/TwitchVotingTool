﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScoreboardControl.xaml.cs" company="Starboard">
//   Copyright © 2011 All Rights Reserved
// </copyright>
// <summary>
//   Main control displaying the scoreboard. Has built-in logic for handling transitions and animations.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Starboard.View
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    using Starboard.Converters;
    using Starboard.Model;
    using Starboard.MVVM;
    using Starboard.ViewModel;

    /// <summary>
    /// Main control displaying the scoreboard. Has built-in logic for handling transitions and animations.
    /// </summary>
    public partial class ScoreboardControl
    {
        // These fields only get set, this hides the resharper warning. It's just holding a weak reference.
        #region Constants and Fields

        /// <summary> Reference to the timer handling announcement animations and transitions. </summary>
        private Timer currentAnnouncementTimer;

        /// <summary> Reference to the timer handling subbar animations and transitions. </summary>
        private Timer currentSubbarTimer;

        // ReSharper disable UnaccessedField.Local
        
        /// <summary> Used to keep a reference to the property observer used for player one. </summary>
        private PropertyObserver<Player> playerOneObserver;

        /// <summary> Used to keep a reference to the property observer used for player two. </summary>
        private PropertyObserver<Player> playerTwoObserver;
        
        // ReSharper restore UnaccessedField.Local

        /// <summary> Index of the last announcement item displayed. </summary>
        private int previousAnnouncementIndex;

        /// <summary> Index of the last subbar item displayed. </summary>
        private int previousSubbarIndex;

        #endregion

        #region Constructors and Destructors

        /// <summary> Initializes a new instance of the <see cref = "ScoreboardControl" /> class. </summary>
        public ScoreboardControl()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.DataContextChanged += this.ScoreboardDataContextChanged;
        }

        /// <summary> Finalizes an instance of the <see cref="ScoreboardControl"/> class. </summary>
        ~ScoreboardControl()
        {
            if (this.currentSubbarTimer == null)
            {
                return;
            }

            this.currentSubbarTimer.Stop();
            this.currentSubbarTimer.Dispose();
        }

        #endregion

        #region Properties

        /// <summary> Gets the viewmodel used for the control </summary>
        private ScoreboardControlViewModel ViewModel
        {
            get
            {
                return (ScoreboardControlViewModel)this.DataContext;
            }
        }

        #endregion

        #region Methods

        /// <summary> Handles logic when the number of announcement messages has changed. Starts/stops appropriate timers automatically. </summary>
        /// <param name="sender"> The sender.  </param>
        /// <param name="e"> The event arguments.  </param>
        private void AnnouncementTextCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var s = (ObservableCollection<TimedText>)sender;

            if (s.Count == 0)
            {
                if (this.currentAnnouncementTimer != null)
                {
                    this.currentAnnouncementTimer.Stop();
                    this.currentAnnouncementTimer.Dispose();
                }

                this.ChangeAnnouncementText(null);
                this.ViewModel.IsAnnouncementShowing = false;
            }
            else if (s.Count == 1)
            {
                if (this.currentAnnouncementTimer != null)
                {
                    this.currentAnnouncementTimer.Stop();
                    this.currentAnnouncementTimer.Dispose();
                }

                this.ChangeAnnouncementText(new Binding("Text") { Source = s[0] });
            }
            else if (s.Count > 1)
            {
                if (this.currentAnnouncementTimer == null)
                {
                    var el = s[0];

                    var time = el.Time * 1000;

                    this.currentAnnouncementTimer = new Timer(time) { AutoReset = false };
                    this.currentAnnouncementTimer.Elapsed += this.AnnouncementTimerElapsed;
                    this.currentAnnouncementTimer.Start();
                }

                // Else the timer will take care of the updating.
            }

            if (e == null)
            {
                // Incase the arguments are not passed in.
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldStartingIndex == this.previousAnnouncementIndex)
                {
                    if (this.currentAnnouncementTimer != null)
                    {
                        this.currentAnnouncementTimer.Stop();
                        this.currentAnnouncementTimer.Dispose();
                    }

                    // Go back 1 so we get the next element.
                    this.previousAnnouncementIndex--;

                    this.AnnouncementTimerElapsed(sender, EventArgs.Empty);
                }
            }
        }

        /// <summary> Finds the next announcement and setups the proper binding when the announcement timer has elapsed. </summary>
        /// <param name="sender"> The sender.  </param>
        /// <param name="e"> The event arguments.  </param>
        private void AnnouncementTimerElapsed(object sender, EventArgs e)
        {
            // Just incase we call this manually.
            if (this.currentAnnouncementTimer != null)
            {
                this.currentAnnouncementTimer.Dispose();
            }

            this.Dispatcher.BeginInvoke(
                (Action)delegate
                    {
                        if (this.ViewModel.AnnouncementText.Count == 0)
                        {
                            // Probably shouldn't get to this point if this is the case. Just incase...
                            return;
                        }

                        int currentIndex = (this.previousAnnouncementIndex + 1) % this.ViewModel.AnnouncementText.Count;

                        var textField = this.ViewModel.AnnouncementText[currentIndex];

                        this.previousAnnouncementIndex = currentIndex;

                        this.ChangeAnnouncementText(new Binding("Text") { Source = textField });

                        if (this.ViewModel.AnnouncementText.Count < 2)
                        {
                            // No need to restart a timer or do anything.
                            return;
                        }

                        var time = textField.Time * 1000;

                        this.currentAnnouncementTimer = new Timer(time) { AutoReset = false };
                        this.currentAnnouncementTimer.Elapsed += this.AnnouncementTimerElapsed;
                        this.currentAnnouncementTimer.Start();
                    });
        }

        /// <summary> Changes the object which the announcement text is bound, and animates the transition. </summary>
        /// <param name="newBinding"> The new binding. </param>
        private void ChangeAnnouncementText(BindingBase newBinding)
        {
            // Fade text away.
            this.Dispatcher.BeginInvoke(
                (Action)
                (() =>
                 this.txtAnnouncement.BeginAnimation(
                     OpacityProperty, new DoubleAnimation(1.0, 0.0, new Duration(new TimeSpan(0, 0, 0, 0, 300))))));

            var timer = new Timer(800);
            timer.Elapsed += (sender, e) =>
                {
                    this.Dispatcher.BeginInvoke(
                        (Action)delegate
                            {
                                if (newBinding == null)
                                {
                                    this.txtAnnouncement.ClearValue(TextBlock.TextProperty);
                                }
                                else
                                {
                                    this.txtAnnouncement.SetBinding(TextBlock.TextProperty, newBinding);
                                }

                                this.txtAnnouncement.BeginAnimation(
                                    OpacityProperty, 
                                    new DoubleAnimation(0.0, 1.0, new Duration(new TimeSpan(0, 0, 0, 0, 300))));
                            });
                    timer.Dispose();
                };
            timer.Start();
        }

        /// <summary> Changes the matchup text to the bind specified. </summary>
        /// <param name="newBinding"> The path of the new binding.  </param>
        private void ChangeMatchupText(BindingBase newBinding)
        {
            // Fade text away.
            this.Dispatcher.BeginInvoke(
                (Action)
                (() =>
                 this.txtStatus.BeginAnimation(
                     OpacityProperty, new DoubleAnimation(1.0, 0.0, new Duration(new TimeSpan(0, 0, 0, 0, 250))))));

            var timer = new Timer(250);
            timer.Elapsed += (sender, e) =>
                {
                    this.Dispatcher.BeginInvoke(
                        (Action)delegate
                            {
                                if (newBinding == null)
                                {
                                    this.txtStatus.ClearValue(TextBlock.TextProperty);
                                }
                                else
                                {
                                    this.txtStatus.SetBinding(TextBlock.TextProperty, newBinding);
                                }

                                this.txtStatus.BeginAnimation(
                                    OpacityProperty, 
                                    new DoubleAnimation(0.0, 1.0, new Duration(new TimeSpan(0, 0, 0, 0, 300))));
                            });
                    timer.Dispose();
                };
            timer.Start();
        }

        /// <summary> Creates and starts a proper ColorAnimation when the color changes for Player 1. </summary>
        /// <param name="player"> The player.  </param>
        private void Player1ColorChanged(Player player)
        {
            var converter = new PlayerColorConverter();
            var color = (Color)converter.Convert(player.Color, typeof(string), null, null);
            var anim = new ColorAnimation(color, new TimeSpan(0, 0, 0, 0, 500));
            this.player1Color.BeginAnimation(GradientStop.ColorProperty, anim);
        }

        /// <summary> Creates and starts a proper ColorAnimation when the color changes for Player 2. </summary>
        /// <param name="player"> The player.  </param>
        private void Player2ColorChanged(Player player)
        {
            var converter = new PlayerColorConverter();
            var color = (Color)converter.Convert(player.Color, typeof(string), null, null);
            var anim = new ColorAnimation(color, new TimeSpan(0, 0, 0, 0, 500));
            this.player2Color.BeginAnimation(GradientStop.ColorProperty, anim);
        }

        /// <summary> Implements property observers when a new viewmodel is applied. </summary>
        /// <param name="sender"> The sender.  </param>
        /// <param name="e"> The event arguments.  </param>
        private void ScoreboardDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewmodel = e.NewValue as ScoreboardControlViewModel;

            if (viewmodel == null)
            {
                return;
            }

            this.DataContext = viewmodel;

            // Unregistering handlers aren't necessary thanks to weak references.
            this.playerOneObserver = new PropertyObserver<Player>(viewmodel.Player1).RegisterHandler(
                n => n.Color, this.Player1ColorChanged);
            this.playerTwoObserver = new PropertyObserver<Player>(viewmodel.Player2).RegisterHandler(
                n => n.Color, this.Player2ColorChanged);

            this.Player1ColorChanged(viewmodel.Player1);
            this.Player2ColorChanged(viewmodel.Player2);

            this.previousSubbarIndex = 0;
            viewmodel.SubbarText.CollectionChanged += this.SubbarTextCollectionChanged;

            this.SubbarTextCollectionChanged(viewmodel.SubbarText, null);

            this.previousAnnouncementIndex = 0;
            viewmodel.AnnouncementText.CollectionChanged += this.AnnouncementTextCollectionChanged;

            this.AnnouncementTextCollectionChanged(viewmodel.AnnouncementText, null);
        }

        /// <summary> Handles logic when the number of subbar messages has changed. Starts/stops appropriate timers automatically. </summary>
        /// <param name="sender"> The sender.  </param>
        /// <param name="e"> The event arguments.  </param>
        private void SubbarTextCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var s = (ObservableCollection<TimedText>)sender;

            if (s.Count == 0)
            {
                if (this.currentSubbarTimer != null)
                {
                    this.currentSubbarTimer.Stop();
                    this.currentSubbarTimer.Dispose();
                }

                this.ChangeMatchupText(null);
                this.ViewModel.IsSubbarShowing = false;
            }
            else if (s.Count == 1)
            {
                if (this.currentSubbarTimer != null)
                {
                    this.currentSubbarTimer.Stop();
                    this.currentSubbarTimer.Dispose();
                }

                this.ViewModel.IsSubbarShowing = true;
                this.ChangeMatchupText(new Binding("Text") { Source = s[0] });
            }
            else if (s.Count > 1)
            {
                if (this.currentSubbarTimer == null)
                {
                    var el = s[0];

                    var time = el.Time * 1000;

                    this.currentSubbarTimer = new Timer(time) { AutoReset = false };
                    this.currentSubbarTimer.Elapsed += this.SubbarTimerElapsed;
                    this.currentSubbarTimer.Start();
                }

                // Else the timer will take care of the updating.
            }

            if (e == null)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldStartingIndex == this.previousSubbarIndex)
                {
                    if (this.currentSubbarTimer != null)
                    {
                        this.currentSubbarTimer.Stop();
                        this.currentSubbarTimer.Dispose();
                    }

                    // Go back 1 so we get the next element.
                    this.previousSubbarIndex--;

                    this.SubbarTimerElapsed(sender, EventArgs.Empty);
                }
            }
        }

        /// <summary> Increments the subbar text when the given timer elapses, and disposes of the timer object. </summary>
        /// <param name="sender"> The sender.  </param>
        /// <param name="e"> The event arguments.  </param>
        private void SubbarTimerElapsed(object sender, EventArgs e)
        {
            // Just incase we call this manually.
            if (this.currentSubbarTimer != null)
            {
                this.currentSubbarTimer.Stop();
                this.currentSubbarTimer.Dispose();
            }

            this.Dispatcher.BeginInvoke(
                (Action)delegate
                    {
                        if (this.ViewModel.SubbarText.Count == 0)
                        {
                            // Probably shouldn't get to this point if this is the case. Just incase...
                            return;
                        }

                        int currentIndex = (this.previousSubbarIndex + 1) % this.ViewModel.SubbarText.Count;

                        var textField = this.ViewModel.SubbarText[currentIndex];

                        this.previousSubbarIndex = currentIndex;

                        this.ChangeMatchupText(new Binding("Text") { Source = textField });

                        if (this.ViewModel.SubbarText.Count < 2)
                        {
                            // No need to restart a timer or do anything.
                            return;
                        }

                        var time = textField.Time * 1000;

                        this.currentSubbarTimer = new Timer(time) { AutoReset = false };
                        this.currentSubbarTimer.Elapsed += this.SubbarTimerElapsed;
                        this.currentSubbarTimer.Start();
                    });
        }

        #endregion
    }
}
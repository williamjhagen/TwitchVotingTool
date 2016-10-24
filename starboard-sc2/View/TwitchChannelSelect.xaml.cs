using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Starboard.View
{

    using System;
    using System.Collections.ObjectModel;
    using System.Windows;

    using Starboard.Model;
    /// <summary>
    /// Interaction logic for TwitchChannelSelect.xaml
    /// </summary>
    public partial class TwitchChannelSelect
    {
        private string channelName = "";


        public TwitchChannelSelect()
        {
            InitializeComponent();
            box.TextChanged += TwitchDataContextChanged;
        }
        #region Public Properties

        /// <summary> Gets the view model stored in the current DataContext. </summary>
        public ObservableCollection<TwitchChannelSelect> ViewModel
        {
            get
            {
                return this.DataContext as ObservableCollection<TwitchChannelSelect>;
            }
        }

        #endregion
        public string ChannelName
        {
            get { return channelName; }
            set {
                if (ValidateString(value))
                    channelName = (value[0] == '#' ? value : value.Insert(0, "#"));
            }
        }

        #region Methods

        private void TwitchDataContextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = this.ViewModel;

            Console.WriteLine("hello");
            if (vm == null)
            {
                return;
            }

        }

        private bool ValidateString(string input)
        {
            if (input[0] == '_') return false;
            if (input.Length < 4 || input.Length > 25) return false;
            foreach (char c in input)
            {
                if (!(
                    (c > 47 && c < 58)
                    || (c > 64 && c < 91)
                    || (c > 96 && c < 123)
                    || (c == ' ')
                    ))
                    return false;
            }
            return true;
        }
        #endregion
    }
}

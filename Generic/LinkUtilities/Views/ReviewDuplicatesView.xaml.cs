using KNARZhelper;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace LinkUtilities
{
    /// <summary>
    /// Interaction logic for ReviewDuplicatesView.xaml
    /// </summary>
    public partial class ReviewDuplicatesView : UserControl
    {
        public ReviewDuplicatesView(LinkUtilities plugin, IEnumerable<Game> games)
        {
            try
            {
                InitializeComponent();
                ((ReviewDuplicatesViewModel)DataContext).InitializeView(games);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing ReviewDuplicatesView", true);
            }
        }
    }
}
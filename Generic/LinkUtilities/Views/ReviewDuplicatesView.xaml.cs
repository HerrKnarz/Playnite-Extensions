using KNARZhelper;
using System;
using System.Windows.Controls;

namespace LinkUtilities
{
    /// <summary>
    ///     Interaction logic for ReviewDuplicatesView.xaml
    /// </summary>
    public partial class ReviewDuplicatesView : UserControl
    {
        public ReviewDuplicatesView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing ReviewDuplicatesView", true);
            }
        }
    }
}
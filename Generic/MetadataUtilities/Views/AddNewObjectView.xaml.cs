using KNARZhelper;
using System;
using System.Windows.Controls;

namespace MetadataUtilities.Views
{
    /// <summary>
    ///     Interaction logic for AddNewObject.xaml
    /// </summary>
    public partial class AddNewObjectView : UserControl
    {
        public AddNewObjectView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Add new object dialog", true);
            }
        }
    }
}
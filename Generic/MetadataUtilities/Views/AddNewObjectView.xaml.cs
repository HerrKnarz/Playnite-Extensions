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
        public AddNewObjectView(MetadataUtilities plugin)
        {
            try
            {
                InitializeComponent();
                ((AddNewObjectViewModel)DataContext).Plugin = plugin;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Add new object dialog", true);
            }
        }
    }
}
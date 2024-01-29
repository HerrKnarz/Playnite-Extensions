using KNARZhelper;
using MetadataUtilities.Models;
using System;
using System.Windows.Controls;

namespace MetadataUtilities.Views
{
    /// <summary>
    ///     Interaction logic for AddNewObject.xaml
    /// </summary>
    public partial class AddNewObjectView : UserControl
    {
        public AddNewObjectView(MetadataUtilities plugin, MetadataListObject item)
        {
            try
            {
                InitializeComponent();
                ((AddNewObjectViewModel)DataContext).Plugin = plugin;
                ((AddNewObjectViewModel)DataContext).NewObject = item;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Add new object dialog", true);
            }
        }
    }
}
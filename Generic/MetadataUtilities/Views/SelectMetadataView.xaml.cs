using KNARZhelper;
using MetadataUtilities.Models;
using System;
using System.Windows.Controls;

namespace MetadataUtilities.Views
{
    /// <summary>
    ///     Interaction logic for SelectMetadataView.xaml
    /// </summary>
    public partial class SelectMetadataView : UserControl
    {
        public SelectMetadataView(MetadataUtilities plugin, MetadataListObjects items)
        {
            try
            {
                InitializeComponent();
                ((SelectMetadataViewModel)DataContext).MetadataListObjects = items;
                ((SelectMetadataViewModel)DataContext).Plugin = plugin;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Select Metadata dialog", true);
            }
        }
    }
}
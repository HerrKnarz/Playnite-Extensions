using KNARZhelper;
using System;
using System.Windows.Controls;

namespace AdvancedMetadataTools
{
    /// <summary>
    ///     Interaction logic for MetadataManager.xaml
    /// </summary>
    public partial class MetadataManagerView : UserControl
    {
        public MetadataManagerView(AdvancedMetadataTools plugin)
        {
            try
            {
                InitializeComponent();
                ((MetadataManagerViewModel)DataContext).Plugin = plugin;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing MetadataManager", true);
            }
        }
    }
}
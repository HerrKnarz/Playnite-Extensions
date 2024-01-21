using AdvancedMetadataTools.Models;
using KNARZhelper;
using System;
using System.Windows.Controls;

namespace AdvancedMetadataTools.Views
{
    /// <summary>
    ///     Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MergeDialogView : UserControl
    {
        public MergeDialogView(AdvancedMetadataTools plugin, MetadataListObjects items)
        {
            try
            {
                InitializeComponent();
                ((MergeDialogViewModel)DataContext).MetadataListObjects = items;
                ((MergeDialogViewModel)DataContext).Plugin = plugin;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing MetadataManager", true);
            }
        }
    }
}
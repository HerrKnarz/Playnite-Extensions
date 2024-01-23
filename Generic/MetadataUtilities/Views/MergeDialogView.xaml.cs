using KNARZhelper;
using MetadataUtilities.Models;
using System;
using System.Windows.Controls;

namespace MetadataUtilities.Views
{
    /// <summary>
    ///     Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MergeDialogView : UserControl
    {
        public MergeDialogView(MetadataUtilities plugin, MetadataListObjects items)
        {
            try
            {
                InitializeComponent();
                ((MergeDialogViewModel)DataContext).MetadataListObjects = items;
                ((MergeDialogViewModel)DataContext).Plugin = plugin;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Metadata Editor", true);
            }
        }
    }
}
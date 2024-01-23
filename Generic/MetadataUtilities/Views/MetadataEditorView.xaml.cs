using KNARZhelper;
using System;
using System.Windows.Controls;

namespace MetadataUtilities
{
    /// <summary>
    ///     Interaction logic for MetadataEditor.xaml
    /// </summary>
    public partial class MetadataEditorView : UserControl
    {
        public MetadataEditorView(MetadataUtilities plugin)
        {
            try
            {
                InitializeComponent();
                ((MetadataEditorViewModel)DataContext).Plugin = plugin;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Metadata Editor", true);
            }
        }
    }
}
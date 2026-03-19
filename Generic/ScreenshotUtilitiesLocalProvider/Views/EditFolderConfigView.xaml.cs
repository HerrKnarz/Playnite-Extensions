using KNARZhelper;
using System;

namespace ScreenshotUtilitiesLocalProvider.Views
{
    /// <summary>
    /// Interaction logic for EditFolderConfigView.xaml
    /// </summary>
    public partial class EditFolderConfigView
    {
        public EditFolderConfigView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Edit Folder Config View", true);
            }
        }
    }
}

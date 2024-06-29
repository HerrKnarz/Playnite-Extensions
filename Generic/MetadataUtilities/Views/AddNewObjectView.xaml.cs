using KNARZhelper;
using System;

namespace MetadataUtilities.Views
{
    /// <summary>
    /// Interaction logic for AddNewObject.xaml
    /// </summary>
    public partial class AddNewObjectView
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
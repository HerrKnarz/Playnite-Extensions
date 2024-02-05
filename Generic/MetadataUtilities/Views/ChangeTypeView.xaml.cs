using KNARZhelper;
using System;

namespace MetadataUtilities.Views
{
    /// <summary>
    ///     Interaction logic for ChangeTypeView.xaml
    /// </summary>
    public partial class ChangeTypeView
    {
        public ChangeTypeView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Change Type dialog", true);
            }
        }
    }
}
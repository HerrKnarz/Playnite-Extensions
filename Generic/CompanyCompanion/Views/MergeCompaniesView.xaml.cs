using KNARZhelper;
using System;
using System.Windows.Controls;

namespace CompanyCompanion
{
    /// <summary>
    /// Interaction logic for MergeCompaniesView.xaml
    /// </summary>
    public partial class MergeCompaniesView : UserControl
    {
        public MergeCompaniesView(CompanyCompanion plugin)
        {
            try
            {
                InitializeComponent();
                ((MergeCompaniesViewModel)DataContext).Plugin = plugin;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing MergeCompaniesView", true);
            }
        }
    }
}
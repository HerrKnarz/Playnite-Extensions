using System;
using System.Windows.Controls;

namespace CompanyCompanion
{
    /// <summary>
    /// Interaction logic for MergeCompaniesView.xaml
    /// </summary>
    public partial class MergeCompaniesView : UserControl
    {
        public MergeCompaniesView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception E)
            {
                Log.Error(E, "Error during initializing MergeCompaniesView", true);
            }
        }
    }
}

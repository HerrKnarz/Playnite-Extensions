using System;
using System.Windows.Controls;

namespace CompanyCompanion
{
    /// <summary>
    /// Interaction logic for MergeCompaniesView.xaml
    /// </summary>
    public partial class MergeCompaniesView : UserControl
    {
        public MergeCompanies MergeCompanies { get; set; }
        private readonly CompanyCompanion plugin;

        public MergeCompaniesView()
        {
            InitializeComponent();
        }

        public MergeCompaniesView(CompanyCompanion plugin)
        {
            try
            {
                this.plugin = plugin;

                MergeCompanies = new MergeCompanies();

                MergeCompanies.FindMatches();

                InitializeComponent();
                DataContext = this;
            }
            catch (Exception E)
            {
                Log.Error(E, "Error during initializing MergeCompaniesView", true);
            }
        }
    }
}

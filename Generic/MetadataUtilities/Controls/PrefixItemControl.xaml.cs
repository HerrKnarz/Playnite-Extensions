using MetadataUtilities.ViewModels;
using Playnite.SDK.Models;
using System;

namespace MetadataUtilities.Controls
{
    /// <summary>
    /// Interaction logic for PrefixItemControl.xaml
    /// </summary>
    public partial class PrefixItemControl
    {
        public PrefixItemControl(MetadataUtilities plugin)
        {
            InitializeComponent();
            DataContext = new PrefixItemControlViewModel(plugin);
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            base.GameContextChanged(oldContext, newContext);

            if (!(DataContext is PrefixItemControlViewModel viewModel))
            {
                return;
            }

            if (viewModel.GameId == (newContext?.Id ?? Guid.Empty))
            {
                viewModel.RefreshData();

                return;
            }

            viewModel.GameId = newContext?.Id ?? Guid.Empty;
        }
    }
}

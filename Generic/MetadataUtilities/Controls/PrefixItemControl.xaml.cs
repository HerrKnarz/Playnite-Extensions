using KNARZhelper.Enum;
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
        public PrefixItemControl(MetadataUtilities plugin, FieldType type)
        {
            InitializeComponent();
            DataContext = new PrefixItemControlViewModel(plugin, type, Icon);
        }

        public string Icon { get; set; }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            base.GameContextChanged(oldContext, newContext);

            RefreshData();
        }

        public void RefreshData()
        {
            if (!(DataContext is PrefixItemControlViewModel viewModel))
            {
                return;
            }

            if (viewModel.GameId == (GameContext?.Id ?? Guid.Empty))
            {
                viewModel.RefreshData();
                return;
            }

            viewModel.GameId = GameContext?.Id ?? Guid.Empty;
        }
    }
}

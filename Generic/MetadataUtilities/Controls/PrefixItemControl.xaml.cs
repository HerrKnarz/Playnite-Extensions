using KNARZhelper;
using KNARZhelper.Enum;
using MetadataUtilities.ViewModels;
using Playnite.SDK.Models;
using System;
using System.Windows;

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
            DataContext = new PrefixItemControlViewModel(plugin, type);
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            base.GameContextChanged(oldContext, newContext);

            RefreshData();
        }

        public void RefreshData()
        {
            try
            {
                if (!(DataContext is PrefixItemControlViewModel viewModel))
                {
                    return;
                }

                if (Parent is FrameworkElement parentElement && parentElement.Tag is string tag)
                {
                    viewModel.DefaultIcon = tag;
                }

                if (viewModel.GameId == (GameContext?.Id ?? Guid.Empty))
                {
                    viewModel.RefreshData();
                    return;
                }

                viewModel.GameId = GameContext?.Id ?? Guid.Empty;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}

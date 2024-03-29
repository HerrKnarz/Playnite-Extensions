﻿using KNARZhelper;
using System;

namespace MetadataUtilities.Views
{
    /// <summary>
    ///     Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MergeDialogView
    {
        public MergeDialogView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Metadata Editor", true);
            }
        }
    }
}
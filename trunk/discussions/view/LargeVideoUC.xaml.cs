﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for SourceUC.xaml
    /// </summary>
    public partial class LargeVideoUC : UserControl
    {
        public LargeVideoUC()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            BotLaunch();
        }

        private void Hyperlink_TouchDown_1(object sender, TouchEventArgs e)
        {
            BotLaunch();
        }

        public void BotLaunch()
        {
            System.Diagnostics.Process.Start(linkTarget.Text);
        }
    }
}
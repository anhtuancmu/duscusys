using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for CtxMenuWrapper.xaml
    /// </summary>
    public partial class CtxMenuWrapper : ContextMenu
    {
        SurfaceTextBox textBox
        {
            get { return (SurfaceTextBox)PlacementTarget; }
        }

        public CtxMenuWrapper()
        {       
            InitializeComponent();
        }

        private void Cut_OnClick(object sender, RoutedEventArgs e)
        {            
            textBox.Cut();
        }

        private void Copy_OnClick(object sender, RoutedEventArgs e)
        {
            textBox.Copy();
        }

        private void Paste_OnClick(object sender, RoutedEventArgs e)
        {
            textBox.Paste();
        }

        private void Select_All_OnClick(object sender, RoutedEventArgs e)
        {
            textBox.SelectAll();
        }

        private void CtxMenuWrapper_OnOpened(object sender, RoutedEventArgs e)
        {
            // Only allow copy/cut if something is selected to copy/cut. 
            if (textBox.SelectedText == "")
                Copy.IsEnabled = Cut.IsEnabled = false;
            else
                Copy.IsEnabled = Cut.IsEnabled = true;

            // Only allow paste if there is text on the clipboard to paste. 
            if (Clipboard.ContainsText())
                Paste.IsEnabled = true;
            else
                Paste.IsEnabled = false;
        }
    }
}

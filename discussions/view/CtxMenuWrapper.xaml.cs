using System.Windows;
using System.Windows.Controls;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions.view
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
            if (string.IsNullOrWhiteSpace(textBox.SelectedText))
                return;

            Clipboard.SetText(textBox.SelectedText);
            if (textBox.SelectionStart >= 0 && textBox.SelectionLength >= 1)
            {
                var newText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);
                textBox.Text = newText;
            }
        }

        private void Copy_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox.SelectedText))
                return;

            Clipboard.SetText(textBox.SelectedText);
        }

        private void Paste_OnClick(object sender, RoutedEventArgs e)
        {
            var textToPaste = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(textToPaste))
                return;

            if (textBox.SelectionStart >= 0 && textBox.SelectionLength >= 1)
            {
                var text = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);
                textBox.Text = text.Insert(textBox.SelectionStart, textToPaste);
            }
            else
                textBox.Text = textBox.Text.Insert(textBox.CaretIndex, textToPaste);
        }

        private void Select_All_OnClick(object sender, RoutedEventArgs e)
        {
            textBox.SelectAll();
        }

        private void CtxMenuWrapper_OnOpened(object sender, RoutedEventArgs e)
        {
            textBox.SelectsAllOnFocus = false;
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

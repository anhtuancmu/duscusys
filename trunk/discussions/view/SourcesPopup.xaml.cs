using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Discussions.DbModel;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for SourcesPopup.xaml
    /// </summary>
    public partial class SourcesPopup : Popup
    {
        public delegate void EditRefList(RichText rt, bool readOnly);

        private EditRefList _sourcesEditor = null;
        private RichText _richText;

        public SourcesPopup()
        {
            InitializeComponent();
        }

        public void SetModel(RichText richText, EditRefList sourcesEditor)
        {
            _richText = richText;
            _sourcesEditor = sourcesEditor;
            if (richText.Source != null)
                SetModel(richText.Source, sourcesEditor != null);
        }

        private void SetModel(IEnumerable<Source> refs, bool showEditLink)
        {
            int i = 1;
            string sources = "";
            foreach (Source src in refs)
            {
                sources += string.Format("{0}. {1}\n", i++, src.Text);
            }
            txtBxSources.Text = sources;
            if (!showEditLink)
                lblEdit.Visibility = Visibility.Hidden;
            else
                lblEdit.Visibility = Visibility.Visible;
        }

        private void Popup_LostFocus(object sender, RoutedEventArgs e)
        {
            IsOpen = false;
        }

        private void btnEdit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_sourcesEditor != null)
            {
                _sourcesEditor(_richText, false);
            }
        }

        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            IsOpen = false;
        }

        private void Popup_MouseMove(object sender, MouseEventArgs e)
        {
        }
    }
}
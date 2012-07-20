using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace Discussions
{
    /// <summary>
    /// Represents a bindable rich editing control which operates on System.Windows.Documents.FlowDocument objects. 
    /// AFA: The binding target has been changed to use a DependencyPropery "Text". The document property will still be operable
    /// </summary>
    public class BoundRichTextBox : RichTextBox
    {
        private static DependencyProperty SaveAsProperty =
            DependencyProperty.Register("SaveAs", typeof(string), typeof(BoundRichTextBox));

        private static DependencyProperty LoadedAsProperty =
            DependencyProperty.Register("LoadedAs", typeof(string), typeof(BoundRichTextBox));

        private static DependencyProperty IsFormattedProperty =
            DependencyProperty.Register("IsFormatted", typeof(bool), typeof(BoundRichTextBox), new PropertyMetadata(false));

        private static DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(BoundRichTextBox));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundRichTextBox"/> class.
        /// </summary>
        public BoundRichTextBox() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundRichTextBox"/> class.
        /// </summary>
        /// <param title="document">A <see cref="T:System.Windows.Documents.FlowDocument"></see> to be added as the initial contents of the new <see cref="T:System.Windows.Controls.BoundRichTextBox"></see>.</param>
        public BoundRichTextBox(FlowDocument document) : base(document) 
        {
            IsFormatted = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized"></see> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized"></see> is set to true internally.
        /// </summary>
        /// <param title="e">The <see cref="T:System.Windows.RoutedEventArgs"></see> that contains the event data.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Hook up to get notified when TextProperty changes.
            var descriptor = DependencyPropertyDescriptor.FromProperty(TextProperty, typeof(TextBox));
            descriptor.AddValueChanged(this, delegate
                {
                    if (RTBContent != Text)
                        RTBContent = Text == null ? string.Empty : Text;
                });

            TextChanged += delegate
            {
                if (Text != RTBContent)
                    Text = RTBContent;
            };
        }

        #region Text Support Routines

        public bool IsFormatted
        {
            get { return (bool)GetValue(IsFormattedProperty); }
            set { SetValue(IsFormattedProperty, value); }
        }

        public string SaveAs
        {
            get { return (string)GetValue(SaveAsProperty); }
            set { SetValue(SaveAsProperty, value); }
        }

        public string LoadedAs
        {
            get { return (string)GetValue(LoadedAsProperty); }
            set { SetValue(LoadedAsProperty, value); }
        }

        public string TextOnly
        {
            get { return new TextRange(Document.ContentStart, Document.ContentEnd).Text; }
        }

        public void ClearFormatting()
        {
            RTBContent = TextOnly;
        }

        public string RTBContent
        {
            get
            {
                TextRange tr = new TextRange(Document.ContentStart, Document.ContentEnd);
                using (var ms = new MemoryStream())
                {
                    tr.Save(ms, IsFormatted ? DataFormats.Rtf : DataFormats.Text);
                    return ASCIIEncoding.Default.GetString(ms.ToArray());
                }
            }
            set
            {
                TextRange selection = new TextRange(Document.ContentStart, Document.ContentEnd);
                SetRangeContent(selection, value);

                if ((string)GetValue(TextBox.TextProperty) != value)
                    SetValue(TextBox.TextProperty, value);
            }
        }

        private string SetRangeContent(TextRange selection, string content)
        {
            string loadType = DataFormats.Rtf;
            IsFormatted = true;
            content = content == "" ? " " : content;
            try
            {
                using (MemoryStream ms = new MemoryStream(ASCIIEncoding.Default.GetBytes(content)))
                {
                    if (content.StartsWith("<Section"))
                    {
                        if (selection.CanLoad(DataFormats.Xaml))
                        {
                            selection.Load(ms, DataFormats.Xaml);
                            loadType = DataFormats.Xaml;
                        }
                    }
                    else if (content.StartsWith("PK"))
                    {
                        if (selection.CanLoad(DataFormats.XamlPackage))
                        {
                            selection.Load(ms, DataFormats.XamlPackage);
                            loadType = DataFormats.XamlPackage;
                        }
                    }
                    else if (content.StartsWith("{\\rtf"))
                    {
                        if (selection.CanLoad(DataFormats.Rtf))
                        {
                            selection.Load(ms, DataFormats.Rtf);
                            loadType = DataFormats.Rtf;
                        }
                    }
                    else
                    {
                        if (selection.CanLoad(DataFormats.Text))
                        {
                            selection.Load(ms, DataFormats.Text);
                            loadType = DataFormats.Text;
                            IsFormatted = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return loadType;
        }

        #endregion Text Support Routines
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Discussions.webkit_host;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for SourceUC.xaml
    /// </summary>
    public partial class SourceUC : UserControl
    {
        public static readonly DependencyProperty PermitsEditProperty =
          DependencyProperty.Register("PermitsEdit", typeof(bool),
          typeof(SourceUC), new FrameworkPropertyMetadata(true, OnPermitsEditChanged));

        public bool PermitsEdit
        {
            get { return (bool)GetValue(PermitsEditProperty); }
            set { SetValue(PermitsEditProperty, value); }
        }

        private static void OnPermitsEditChanged(DependencyObject source,
        DependencyPropertyChangedEventArgs e)
        {
            SourceUC control = source as SourceUC;
            bool permits = (bool)e.NewValue;
            if (!permits)
                control.btnRemoveComment.Visibility = Visibility.Hidden;
        }

        public static readonly DependencyProperty TruncateUrlsProperty =
          DependencyProperty.Register("TruncateUrls", typeof(bool),
          typeof(SourceUC), new FrameworkPropertyMetadata(true, OnTruncChanged));

        public bool TruncateUrls
        {
            get { return (bool)GetValue(TruncateUrlsProperty); }
            set { SetValue(TruncateUrlsProperty, value); }
        }

        private static void OnTruncChanged(DependencyObject source,
        DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly RoutedEvent SourceRemovedEvent = EventManager.RegisterRoutedEvent(
          "SourceRemoved", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SourceUC));

        // Provide CLR accessors for the event
        public event RoutedEventHandler SourceRemoved
        {
            add { AddHandler(SourceRemovedEvent, value); }
            remove { RemoveHandler(SourceRemovedEvent, value); }
        }

        public string TruncatedLink
        {
            get
            {
                var src = DataContext as Source;
                if (src == null)
                    return "";

                if (!TruncateUrls)
                    return src.Text;

                return truncateUrl(src.Text);
            }

            set
            {
                var src = DataContext as Source;
                if (src != null)
                    return;

                src.Text = value;
            }
        }

        public SourceUC()
        {
            InitializeComponent();

            linkTarget.DataContext = this;
        }

        string truncateUrl(string url)
        {
            var truncated = url.Substring(0, Math.Min(url.Length, 40));
            if(truncated!=url)
                truncated = truncated.Insert(truncated.Length,"...");
            return truncated;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext == null)
                return;

            //order!
            {
                RaiseEvent(new RoutedEventArgs(SourceRemovedEvent));
                var src = (Source)DataContext;
                src.RichText = null;
            }
        }

        private void Hyperlink_TouchDown_1(object sender, TouchEventArgs e)
        {
            Launch();
        }

        private void Launch()
        {
            try
            {
                var src = DataContext as Source;
                if (src != null)
                {
                    //var browser = new BrowserWindow(src.Text);
                    //browser.ShowDialog();

                    //System.Diagnostics.Process.Start(src.Text);    

                    Utils.ReportMediaOpened(StEvent.SourceOpened, src.RichText.ArgPoint);
            
                    var browser = new WebKitFrm(src.Text);
                    browser.ShowDialog();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            Launch();
        }   
     
        public int SrcNumber 
        {
            set
            {
                number.Text = value.ToString();
            }
        }
    }
}

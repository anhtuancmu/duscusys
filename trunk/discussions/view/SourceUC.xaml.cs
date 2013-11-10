using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Discussions.DbModel;
using Discussions.model;
using Discussions.webkit_host;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for SourceUC.xaml
    /// </summary>
    public partial class SourceUC : UserControl
    {
        public static readonly DependencyProperty PermitsEditProperty =
            DependencyProperty.Register("PermitsEdit", typeof (bool),
                                        typeof (SourceUC), new FrameworkPropertyMetadata(true, OnPermitsEditChanged));

        public bool PermitsEdit
        {
            get { return (bool) GetValue(PermitsEditProperty); }
            set { SetValue(PermitsEditProperty, value); }
        }

        private static void OnPermitsEditChanged(DependencyObject source,
                                                 DependencyPropertyChangedEventArgs e)
        {
            SourceUC control = source as SourceUC;
            bool permits = (bool) e.NewValue;
            if (!permits)
                control.btnRemoveComment.Visibility = Visibility.Hidden;
        }

        public static readonly DependencyProperty TruncateUrlsProperty =
            DependencyProperty.Register("TruncateUrls", typeof (bool),
                                        typeof (SourceUC), new FrameworkPropertyMetadata(true, OnTruncChanged));

        public bool TruncateUrls
        {
            get { return (bool) GetValue(TruncateUrlsProperty); }
            set { SetValue(TruncateUrlsProperty, value); }
        }

        private static void OnTruncChanged(DependencyObject source,
                                           DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly RoutedEvent SourceRemovedEvent = EventManager.RegisterRoutedEvent(
            "SourceRemoved", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (SourceUC));

        // Provide CLR accessors for the event
        public event RoutedEventHandler SourceRemoved
        {
            add { AddHandler(SourceRemovedEvent, value); }
            remove { RemoveHandler(SourceRemovedEvent, value); }
        }

        public static readonly RoutedEvent SourceViewEvent = EventManager.RegisterRoutedEvent(
            "SourceView", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (SourceUC));

        public event RoutedEventHandler SourceView
        {
            add { AddHandler(SourceViewEvent, value); }
            remove { RemoveHandler(SourceViewEvent, value); }
        }

        public static readonly RoutedEvent SourceUpDownEvent = EventManager.RegisterRoutedEvent(
            "SourceUpDown", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (SourceUC));

        public event RoutedEventHandler SourceUpDown
        {
            add { AddHandler(SourceUpDownEvent, value); }
            remove { RemoveHandler(SourceUpDownEvent, value); }
        }

        public static readonly DependencyProperty CanReorderProperty =
            DependencyProperty.Register("CanReorder", typeof (bool),
                                        typeof (SourceUC), new FrameworkPropertyMetadata(false, OnCanReorderChanged));

        public bool CanReorder
        {
            get { return (bool) GetValue(CanReorderProperty); }
            set { SetValue(CanReorderProperty, value); }
        }

        private static void OnCanReorderChanged(DependencyObject source,
                                                DependencyPropertyChangedEventArgs e)
        {
            SourceUC control = source as SourceUC;
            bool can = (bool) e.NewValue;
            if (!can)
                control.btnReposition.Visibility = Visibility.Collapsed;
            else
                control.btnReposition.Visibility = Visibility.Visible;
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

        private string truncateUrl(string url)
        {
            var truncated = url.Substring(0, Math.Min(url.Length, 40));
            if (truncated != url)
                truncated = truncated.Insert(truncated.Length, "...");
            return truncated;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext == null)
                return;

            RaiseEvent(new RoutedEventArgs(SourceRemovedEvent));
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

                    RaiseEvent(new RoutedEventArgs(SourceViewEvent));

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
            set { number.Text = value.ToString(); }
        }

        private void btnReposition_Click_1(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SourceUpDownEvent));
        }
    }
}
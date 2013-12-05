using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DistributedEditor
{
    /// <summary>
    /// Interaction logic for TextUC.xaml
    /// </summary>
    public partial class TextUC : UserControl
    {
        public delegate void TextChanged(string text);

        public TextChanged textChanged;

        public static readonly RoutedEvent VdTextDeleteEvent = EventManager.RegisterRoutedEvent(
            "VdTextDelete", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (TextUC));

        // Provide CLR accessors for the event
        public event RoutedEventHandler VdTextDelete
        {
            add { AddHandler(VdTextDeleteEvent, value); }
            remove { RemoveHandler(VdTextDeleteEvent, value); }
        }

        public static readonly RoutedEvent TextShapeCopyEvent = EventManager.RegisterRoutedEvent(
            "TextShapeCopy", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (TextUC));

        // Provide CLR accessors for the event
        public event RoutedEventHandler TextShapeCopy
        {
            add { AddHandler(TextShapeCopyEvent, value); }
            remove { RemoveHandler(TextShapeCopyEvent, value); }
        }

        public static readonly RoutedEvent TextShapePasteEvent = EventManager.RegisterRoutedEvent(
            "TextShapePaste", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (TextUC));

        // Provide CLR accessors for the event
        public event RoutedEventHandler TextShapePaste
        {
            add { AddHandler(TextShapePasteEvent, value); }
            remove { RemoveHandler(TextShapePasteEvent, value); }
        }

        private VdText _vdText;

        public TextUC(VdText vdText)
        {
            InitializeComponent();

            _vdText = vdText;

            field.Tag = vdText;
            txtLabel.Tag = vdText;

            RemoveFocus();
        }

        private void SurfaceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void field_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(VdTextDeleteEvent, _vdText);
                RaiseEvent(newEventArgs);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.C)
                {
                    RoutedEventArgs newEventArgs = new RoutedEventArgs(TextUC.TextShapeCopyEvent, null);
                    RaiseEvent(newEventArgs);
                }
                else if (e.Key == Key.V)
                {
                    RoutedEventArgs newEventArgs = new RoutedEventArgs(TextUC.TextShapePasteEvent, null);
                    RaiseEvent(newEventArgs);
                }
            }

            beginTextChangedInvoke();
        }

        private void beginTextChangedInvoke()
        {
            this.Dispatcher.BeginInvoke(
                new Action(() =>
                    {
                        if (textChanged != null)
                            textChanged(field.Text);
                    }),
                DispatcherPriority.Background);
        }

        private void ToggleEditability()
        {
            if (txtLabel.Visibility == Visibility.Visible)
                SetFocus();
            else
                RemoveFocus();
        }

        private void field_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void txtLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void txtLabel_TouchDown(object sender, TouchEventArgs e)
        {
        }

        public void RemoveFocus()
        {
            //Keyboard.ClearFocus();
            txtLabel.Visibility = Visibility.Visible;
            field.Visibility = Visibility.Collapsed;
            field.IsReadOnly = true;
            handle.BorderBrush = Brushes.Transparent;
        }

        public void SetFocus()
        {
            txtLabel.Visibility = Visibility.Collapsed;
            field.Visibility = Visibility.Visible;
            field.IsReadOnly = false;
            handle.BorderBrush = new SolidColorBrush((Color) FindResource("TextToolOverlay"));
            this.Dispatcher.BeginInvoke(new Action(() => Keyboard.Focus(field)),
                                        DispatcherPriority.Background);
        }

        private void handleMouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void handleTouchDown(object sender, TouchEventArgs e)
        {
        }

        public double GetFieldActualWidth()
        {
            return field.ActualWidth;
        }

        public double GetFieldActualHeight()
        {
            return field.ActualHeight;
        }
    }
}
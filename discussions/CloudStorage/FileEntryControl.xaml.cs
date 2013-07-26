﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Discussions;

namespace CloudStorage
{
    public partial class FileEntryControl : UserControl
    {
        private readonly MultiClickRecognizer _mediaDoubleClick;

        public static readonly RoutedEvent RequestViewEvent = EventManager.RegisterRoutedEvent(
            "RequestView", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (FileEntryControl));

        public event RoutedEventHandler RequestView
        {
            add { AddHandler(RequestViewEvent, value); }
            remove { RemoveHandler(RequestViewEvent, value); }
        }

        public static readonly RoutedEvent CustSelectionEvent = EventManager.RegisterRoutedEvent(
            "CustSelection", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (FileEntryControl));

        public event RoutedEventHandler CustSelection
        {
            add { AddHandler(CustSelectionEvent, value); }
            remove { RemoveHandler(CustSelectionEvent, value); }
        }

        public FileEntryControl()
        {
            InitializeComponent();

            _mediaDoubleClick = new MultiClickRecognizer(badgeDoubleTap, null, onSingleClick);
        }

        private void badgeDoubleTap(object sender, InputEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RequestViewEvent));
        }

        private void FileEntryControl_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            _mediaDoubleClick.Click(sender, e);
            e.Handled = true;
        }

        private void FileEntryControl_PreviewTouchDown_1(object sender, TouchEventArgs e)
        {
            _mediaDoubleClick.Click(sender, e);
            e.Handled = true;
        }

        private void onSingleClick(object sender, InputEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CustSelectionEvent));
        }
    }
}
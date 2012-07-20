using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Controls;

namespace Discussions
{
    class RichTextBoxFormatBarManagerStatic : DependencyObject
    {
          #region Members

        private global::System.Windows.Controls.RichTextBox _richTextBox;
        private IRichTextBoxFormatBar _toolbar;

        #endregion //Members

        #region Properties

        #region FormatBar

        public static readonly DependencyProperty FormatBarProperty = DependencyProperty.RegisterAttached("FormatBar", typeof(IRichTextBoxFormatBar), typeof(Microsoft.Windows.Controls.RichTextBox),
            new PropertyMetadata(null, OnFormatBarPropertyChanged));
        public static void SetFormatBar(UIElement element, IRichTextBoxFormatBar value)
        {
            element.SetValue(FormatBarProperty, value);
        }
        public static IRichTextBoxFormatBar GetFormatBar(UIElement element)
        {
            return (IRichTextBoxFormatBar)element.GetValue(FormatBarProperty);
        }

        private static void OnFormatBarPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            global::System.Windows.Controls.RichTextBox rtb = d as global::System.Windows.Controls.RichTextBox;
            if (rtb == null)
                throw new Exception("A FormatBar can only be applied to a RichTextBox.");

            RichTextBoxFormatBarManagerStatic manager = new RichTextBoxFormatBarManagerStatic();
            manager.AttachFormatBarToRichtextBox(rtb, e.NewValue as IRichTextBoxFormatBar);
        }

        #endregion //FormatBar

        #endregion //Properties

        #region Event Handlers

        void RichTextBox_MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //this fixes the bug when applying text transformations the text would lose it's highlight.  That was because the RichTextBox was losing focus
            //so we just give it focus again and it seems to do the trick of re-highlighting it.
            if (!_richTextBox.IsFocused && !_richTextBox.Selection.IsEmpty)
                _richTextBox.Focus();
        }

        #endregion //Event Handlers

        #region Methods

        /// <summary>
        /// Attaches a FormatBar to a RichtextBox
        /// </summary>
        /// <param name="richTextBox">The RichtextBox to attach to.</param>
        /// <param name="formatBar">The Formatbar to attach.</param>
        private void AttachFormatBarToRichtextBox(global::System.Windows.Controls.RichTextBox richTextBox, IRichTextBoxFormatBar formatBar)
        {
            _richTextBox = richTextBox;
            //we cannot use the PreviewMouseLeftButtonUp event because of selection bugs.
            //we cannot use the MouseLeftButtonUp event because it is handled by the RichTextBox and does not bubble up to here, so we must
            //add a hander to the MouseUpEvent using the Addhandler syntax, and specify to listen for handled events too.
            _richTextBox.AddHandler(Mouse.MouseUpEvent, new MouseButtonEventHandler(RichTextBox_MouseButtonUp), true);
            _richTextBox.TextChanged += RichTextBox_TextChanged;

            formatBar.Target = _richTextBox;
            _toolbar = formatBar;
        }

        /// <summary>
        /// Shows the FormatBar
        /// </summary>
        void ShowAdorner()
        {
        }

        /// <summary>
        /// Positions the FormatBar so that is does not go outside the bounds of the RichTextBox or covers the selected text
        /// </summary>
        /// <param name="adorningEditor"></param>
        private void PositionFormatBar(Control adorningEditor)
        {
        }

        /// <summary>
        /// Ensures that the IRichTextFormatBar is in the adorner layer.
        /// </summary>
        /// <returns>True if the IRichTextFormatBar is in the adorner layer, else false.</returns>
        bool VerifyAdornerLayer()
        {
            return true;
        }

        /// <summary>
        /// Hides the IRichTextFormatBar that is in the adornor layer.
        /// </summary>
        void HideAdorner()
        {
        }

        #endregion //Methods
    }
}

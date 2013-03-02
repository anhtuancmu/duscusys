using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MS.Internal;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Discussions.d_editor
{
    class NumberedTickBar : TickBar
    {
        protected override void OnRender(DrawingContext dc)
        {
            Size size = new Size(base.ActualWidth, base.ActualHeight);
            int tickCount = (int)((this.Maximum - this.Minimum) / this.TickFrequency) + 1;
            if ((this.Maximum - this.Minimum) % this.TickFrequency == 0)
                tickCount -= 1;
            Double tickFrequencySize;
            // Calculate tick's setting
            tickFrequencySize = (size.Height * this.TickFrequency / (this.Maximum - this.Minimum));
            string text = "";
            FormattedText formattedText = null;
            double num = this.Maximum - this.Minimum;
            int i = 0;
            // Draw each tick text
            for (i = 0; i <= tickCount; i++)
            {
                text = Convert.ToString(Convert.ToInt32(this.Minimum + this.TickFrequency * i), 10);

                formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 14, Brushes.Black);
                dc.DrawText(formattedText, new Point(3, tickFrequencySize * (tickCount - i)));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EventGen.timeline
{
    class TimeScale
    {
        //distance between second ticks at lowest scale
        //const double SECOND_TICK_DISTANCE = 180;
        const double SECOND_TICK_DISTANCE = 260;

        public const double TIMELINE_HEIGHT = 200;
        
        //position in scene to relative time
        public static TimeSpan PositionToTime(double pos, double zoom)
        {
            double seconds = zoom * pos / SECOND_TICK_DISTANCE;
            return TimeSpan.FromSeconds(seconds);
        }

        //relative time in canvas to position 
        public static double TimeToPosition(TimeSpan ts, double zoom)
        {
            return SECOND_TICK_DISTANCE * ts.TotalSeconds / zoom;
        }

        //zoom shows how many times current scale is larger than lowest, [1,+Inf)        
        public static void drawTicks(Canvas scene, double zoom, TimeSpan range, double tickLevelY)
        {
            if (zoom < 1)
                throw new ArgumentOutOfRangeException();

            //remove previous ticks and text
            UIElement[] copy = new UIElement[scene.Children.Count];
            scene.Children.CopyTo(copy,0);
            foreach(var c in copy)
            {
                if (c is Line || c is TextBlock)
                    scene.Children.Remove(c); 
            }

            //add new ticks and text
            if (zoom < 3)
            {
                var numSecondTicks = Math.Round(range.TotalSeconds) + 1;
                for (int i = 0; i < numSecondTicks; i++)
                {
                    addTick(scene, i, zoom, tickLevelY);
                }
            }
            else if (zoom < 25)
            {
                var num3SecondTicks = Math.Round(range.TotalSeconds / 3) + 1;
                for (int i = 0; i < num3SecondTicks; i++)
                {
                    addTick(scene, 3 * i, zoom, tickLevelY);
                }
            }
            else if (zoom < 100)
            {
                var numMinuteTicks = Math.Round(range.TotalSeconds / 30) + 1;
                for (int i = 0; i < numMinuteTicks; i++)
                {
                    addTick(scene, 30 * i, zoom, tickLevelY);
                }
            }
            else
            {
                var numMinuteTicks = Math.Round(range.TotalSeconds / 60) + 1;
                for (int i = 0; i < numMinuteTicks; i++)
                {
                    addTick(scene, 60 * i, zoom, tickLevelY);
                }
            }
        }

        static void addTick(Canvas scene, int totalSeconds, double zoom, double tickLevelY)
        {
            var tick = new Line();
            tick.Stroke = new SolidColorBrush(Colors.Azure);
            tick.StrokeThickness = 0.2;
            tick.X1 = totalSeconds * SECOND_TICK_DISTANCE / zoom;
            tick.Y1 = 0;
            tick.X2 = tick.X1;
            tick.Y2 = tickLevelY;
            scene.Children.Add(tick);

            var label = new TextBlock(new Run(TimeSpan.FromSeconds(totalSeconds).ToString("mm\\:ss")));
            label.Foreground = tick.Stroke;
            scene.Children.Add(label);
            Canvas.SetLeft(label, tick.X1);
            Canvas.SetTop(label, tick.Y2);
        }
    }
}

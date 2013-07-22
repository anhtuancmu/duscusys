using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Discussions.Metro
{
    public class MetroFiller
    {
        public delegate void Launcher();

        public static void AddTiles(StackPanel metroStackPanel, Dictionary<string, Launcher> tiles)
        {
            var tileWrapPanel = new WrapPanel();
            tileWrapPanel.Orientation = Orientation.Horizontal;
            tileWrapPanel.Margin = new Thickness(0, 0, 20, 0);

            tileWrapPanel.Height = (210*3) + (6*3);

            foreach (var k in tiles.Keys)
            {
                var newTile = new TopicTile();
                newTile.TileTxtBlck.Text = k;
                newTile.Margin = new Thickness(0, 0, 6, 6);
                newTile.LaunchDel = tiles[k];
                tileWrapPanel.Children.Add(newTile);
            }

            metroStackPanel.Children.Add(tileWrapPanel);
        }
    }
}
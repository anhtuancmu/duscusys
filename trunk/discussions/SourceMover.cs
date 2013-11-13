using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using Discussions.DbModel;
using Discussions.view;
using Microsoft.Surface.Presentation.Input;

namespace Discussions
{
    public class SourceMover
    {
        //swaps current source with its neighbour
        public bool swapWithNeib(bool withTopNeib, Source current)
        {
            if (current == null)
                return false;

            //there is no neighbour, nothing to do
            if (current.RichText.Source.Count <= 1)
                return false;

            //ensure strict ordering
            var orderNr = 0;
            foreach (var s in current.RichText.Source.OrderBy(s => s.OrderNumber))
            {
                s.OrderNumber = orderNr++;
            }

            if (withTopNeib)
            {
                Source topNeib = null;
                foreach (var s in current.RichText.Source.OrderBy(s => s.OrderNumber))
                {
                    if (s == current)
                        break;
                    else
                        topNeib = s;
                }
                //current source is topmost, nothing to do 
                if (topNeib == null)
                    return false;

                var tmp = topNeib.OrderNumber;
                topNeib.OrderNumber = current.OrderNumber;
                current.OrderNumber = tmp;

                return true;
            }
            else
            {
                Source botNeib = null;
                foreach (var s in current.RichText.Source.OrderBy(s => s.OrderNumber).Reverse())
                {
                    if (s == current)
                        break;
                    else
                        botNeib = s;
                }
                //current source is bottommost, nothing to do 
                if (botNeib == null)
                    return false;

                var tmp = botNeib.OrderNumber;
                botNeib.OrderNumber = current.OrderNumber;
                current.OrderNumber = tmp;

                return true;
            }
        }

        public Popup srcPopup;

        public SourceMover(Popup srcPopup)
        {
            this.srcPopup = srcPopup;
        }

        public Source _srcToReposition = null;

        public void onSourceUpDown(object sender, RoutedEventArgs e)
        {
            try
            {
                _srcToReposition = ((SourceUC) e.OriginalSource).DataContext as Source;
                srcPopup.IsOpen = true;
                HwndSource hwndSource =
                    (HwndSource) PresentationSource.FromVisual((Visual) VisualTreeHelper.GetParent(srcPopup.Child));
                hwndSource.EnableSurfaceInput();
            }
            catch (Exception)
            {
                _srcToReposition = null;
            }
        }
    }
}
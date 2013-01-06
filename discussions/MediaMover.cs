using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using Discussions.DbModel;
using Microsoft.Surface.Presentation.Input;

namespace Discussions
{
    public class MediaMover
    {
        //swaps current source with its neighbour
        public bool swapWithNeib(bool withTopNeib, Attachment current)
        {
            if (current == null)
                return false;

            IEnumerable<Attachment> media = null;

            //there is no neighbour, nothing to do
            if (current.ArgPoint != null)
            {
                media = current.ArgPoint.Attachment;
                if (current.ArgPoint.Attachment.Count <= 1)
                    return false;
            }
            else
            {
                if (current.Discussion == null)
                    return false;

                media = current.Discussion.Attachment;
                if (current.Discussion.Attachment.Count <= 1)
                    return false;
            }

            //ensure strong ordering
            var orderNr = 0;
            foreach (var att in media.OrderBy(at0 => at0.OrderNumber))
            {
                att.OrderNumber = orderNr++;
            }

            if (withTopNeib)
            {
                Attachment topNeib = null;
                foreach (var a in media.OrderBy(a0 => a0.OrderNumber))
                {
                    if (a == current)
                        break;
                    else
                        topNeib = a;
                }
                //current attachment is topmost, nothing to do 
                if (topNeib == null)
                    return false;

                var tmp = topNeib.OrderNumber;
                topNeib.OrderNumber = current.OrderNumber;
                current.OrderNumber = tmp;

                return true;
            }
            else
            {
                Attachment botNeib = null;
                foreach (var a in media.OrderBy(a0 => a0.OrderNumber).Reverse())
                {
                    if (a == current)
                        break;
                    else
                        botNeib = a;
                }
                //current attachment is bottommost, nothing to do 
                if (botNeib == null)
                    return false;

                var tmp = botNeib.OrderNumber;
                botNeib.OrderNumber = current.OrderNumber;
                current.OrderNumber = tmp;

                return true;
            }
        }

        public Popup medPopup;

        public MediaMover(Popup medPopup)
        {
            this.medPopup = medPopup;
        }

        public Attachment _attachmentToReposition = null;

        public void onAttachmentUpDown(object sender, RoutedEventArgs e)
        {
            try
            {
                _attachmentToReposition = ((FrameworkElement) e.OriginalSource).DataContext as Attachment;
                medPopup.IsOpen = true;
                HwndSource hwndSource =
                    (HwndSource) PresentationSource.FromVisual((Visual) VisualTreeHelper.GetParent(medPopup.Child));
                hwndSource.EnableSurfaceInput();
            }
            catch (Exception)
            {
                _attachmentToReposition = null;
            }
        }
    }
}
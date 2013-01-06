using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Discussions.model;
using LoginEngine;

namespace Discussions.stats
{
    public class EventViewModel
    {
        public string dateTime { get; set; }
        public string userName { get; set; }
        public SolidColorBrush userColor { get; set; }
        public string evt { get; set; }
        public string devType { get; set; }

        public EventViewModel(StEvent eventCode, int userId, DateTime stamp, DeviceType device)
        {
            dateTime = stamp.ToString();

            var usr = DbCtx.Get().Person.FirstOrDefault(p0 => p0.Id == userId);
            if (usr != null)
            {
                userName = usr.Name;
                userColor = new SolidColorBrush(Utils2.IntToColor(usr.Color));
            }
            else
            {
                userName = "SYSTEM";
                userColor = new SolidColorBrush(Colors.Aqua);
            }

            switch (eventCode)
            {
                case StEvent.RecordingStarted:
                    evt = "enabled event recording";
                    break;
                case StEvent.RecordingStopped:
                    evt = "disabled event recording";
                    break;
                case StEvent.BadgeCreated:
                    evt = "created badge";
                    break;
                case StEvent.BadgeEdited:
                    evt = "edited badge";
                    break;
                case StEvent.BadgeMoved:
                    evt = "moved badge";
                    break;
                case StEvent.BadgeZoomIn:
                    evt = "zoomed in badge";
                    break;
                case StEvent.ClusterCreated:
                    evt = "created cluster";
                    break;
                case StEvent.ClusterDeleted:
                    evt = "deleted cluster";
                    break;
                case StEvent.ClusterIn:
                    evt = "cluster-in";
                    break;
                case StEvent.ClusterOut:
                    evt = "cluster-out";
                    break;
                case StEvent.ClusterMoved:
                    evt = "moved cluster";
                    break;
                case StEvent.LinkCreated:
                    evt = "created link";
                    break;
                case StEvent.LinkRemoved:
                    evt = "removed link";
                    break;
                case StEvent.FreeDrawingCreated:
                    evt = "created free drawing";
                    break;
                case StEvent.FreeDrawingRemoved:
                    evt = "removed free drawing";
                    break;
                case StEvent.FreeDrawingResize:
                    evt = "resized free drawing";
                    break;
                case StEvent.FreeDrawingMoved:
                    evt = "moved free drawing";
                    break;
                case StEvent.SceneZoomedIn:
                    evt = "zoomed in scene";
                    break;
                case StEvent.SceneZoomedOut:
                    evt = "zoomed out scene";
                    break;
                case StEvent.ArgPointTopicChanged:
                    evt = "point transfer";
                    break;
                case StEvent.SourceAdded:
                    evt = "added source";
                    break;
                case StEvent.SourceRemoved:
                    evt = "removed source";
                    break;
                case StEvent.ImageAdded:
                    evt = "added image file";
                    break;
                case StEvent.ImageUrlAdded:
                    evt = "added link to image";
                    break;
                case StEvent.PdfAdded:
                    evt = "added PDF";
                    break;
                case StEvent.PdfUrlAdded:
                    evt = "added link to PDF";
                    break;
                case StEvent.YoutubeAdded:
                    evt = "added Youtube video";
                    break;
                case StEvent.ScreenshotAdded:
                    evt = "added screenshot";
                    break;
                case StEvent.MediaRemoved:
                    evt = "removed media";
                    break;
                case StEvent.CommentAdded:
                    evt = "added comment";
                    break;
                case StEvent.CommentRemoved:
                    evt = "removed comment";
                    break;
                case StEvent.ImageOpened:
                    evt = "opened image";
                    break;
                case StEvent.VideoOpened:
                    evt = "opened video";
                    break;
                case StEvent.ScreenshotOpened:
                    evt = "opened screenshot";
                    break;
                case StEvent.PdfOpened:
                    evt = "opened PDF";
                    break;
                case StEvent.SourceOpened:
                    evt = "opened source";
                    break;
                default:
                    throw new NotSupportedException();
            }

            switch (device)
            {
                case DeviceType.Android:
                    devType = "App";
                    break;
                case DeviceType.Wpf:
                    devType = "Windows";
                    break;
                case DeviceType.Sticky:
                    devType = "Sticky";
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
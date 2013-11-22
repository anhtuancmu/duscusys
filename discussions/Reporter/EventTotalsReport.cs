using System;
using System.Collections.Generic;
using Discussions.DbModel.model;

namespace Reporter
{
    public class EventTotalsReport
    {
        public int TotalRecordingStarted;
        public int TotalRecordingStopped;

        public int TotalBadgeCreated;
        public int TotalBadgeEdited;
        public int TotalBadgeMoved;
        public int TotalBadgeZoomIn;

        public int TotalClusterCreated;
        public int TotalClusterDeleted;
        public int TotalClusterIn;
        public int TotalClusterOut;
        public int TotalClusterMoved;
        public int TotalClusterTitlesAdded;
        public int TotalClusterTitlesEdited;
        public int TotalClusterTitlesRemoved;

        public int TotalLinkCreated;
        public int TotalLinkRemoved;

        public int TotalFreeDrawingCreated;
        public int TotalFreeDrawingRemoved;
        public int TotalFreeDrawingResize;
        public int TotalFreeDrawingMoved;

        public int TotalSceneZoomedIn;
        public int TotalSceneZoomedOut;

        public int TotalArgPointTopicChanged;

        public int TotalSourceAdded;
        public int TotalSourceRemoved;

        public int TotalImageAdded;
        public int TotalImageUrlAdded;

        public int TotalPdfAdded;
        public int TotalPdfUrlAdded;

        public int TotalYoutubeAdded;

        public int TotalScreenshotAdded;

        public int TotalMediaRemoved;

        public int TotalCommentAdded;
        public int TotalCommentRemoved;

        public int TotalImageOpened;
        public int TotalVideoOpened;
        public int TotalScreenshotOpened;
        public int TotalPdfOpened;
        public int TotalSourceOpened;

        public int TotalLaserEnabled;

        private List<int> countedEventIds = new List<int>();

        public void CountEvent(StEvent ev, int eventId)
        {
            if (countedEventIds.Contains(eventId))
                return;

            countedEventIds.Add(eventId);

            switch (ev)
            {
                case StEvent.RecordingStarted:
                    TotalRecordingStarted++;
                    break;
                case StEvent.RecordingStopped:
                    TotalRecordingStopped++;
                    break;
                case StEvent.BadgeCreated:
                    TotalBadgeCreated++;
                    break;
                case StEvent.BadgeEdited:
                    TotalBadgeEdited++;
                    break;
                case StEvent.BadgeMoved:
                    TotalBadgeMoved++;
                    break;
                case StEvent.BadgeZoomIn:
                    TotalBadgeZoomIn++;
                    break;
                case StEvent.ClusterCreated:
                    TotalClusterCreated++;
                    break;
                case StEvent.ClusterDeleted:
                    TotalClusterDeleted++;
                    break;
                case StEvent.ClusterIn:
                    TotalClusterIn++;
                    break;
                case StEvent.ClusterOut:
                    TotalClusterOut++;
                    break;
                case StEvent.ClusterMoved:
                    TotalClusterMoved++;
                    break;
                case StEvent.ClusterTitleAdded:
                    TotalClusterTitlesAdded++;
                    break;
                case StEvent.ClusterTitleEdited:
                    TotalClusterTitlesEdited++;
                    break;
                case StEvent.ClusterTitleRemoved:
                    TotalClusterTitlesRemoved++;
                    break;
                case StEvent.LinkCreated:
                    TotalLinkCreated++;
                    break;
                case StEvent.LinkRemoved:
                    TotalLinkRemoved++;
                    break;
                case StEvent.FreeDrawingCreated:
                    TotalFreeDrawingCreated++;
                    break;
                case StEvent.FreeDrawingRemoved:
                    TotalFreeDrawingRemoved++;
                    break;
                case StEvent.FreeDrawingResize:
                    TotalFreeDrawingResize++;
                    break;
                case StEvent.FreeDrawingMoved:
                    TotalFreeDrawingMoved++;
                    break;
                case StEvent.SceneZoomedIn:
                    TotalSceneZoomedIn++;
                    break;
                case StEvent.SceneZoomedOut:
                    TotalSceneZoomedOut++;
                    break;
                case StEvent.ArgPointTopicChanged:
                    TotalArgPointTopicChanged++;
                    break;
                case StEvent.SourceAdded:
                    TotalSourceAdded++;
                    break;
                case StEvent.SourceRemoved:
                    TotalSourceRemoved++;
                    break;
                case StEvent.ImageAdded:
                    TotalImageAdded++;
                    break;
                case StEvent.ImageUrlAdded:
                    TotalImageUrlAdded++;
                    break;
                case StEvent.PdfAdded:
                    TotalPdfAdded++;
                    break;
                case StEvent.PdfUrlAdded:
                    TotalPdfUrlAdded++;
                    break;
                case StEvent.YoutubeAdded:
                    TotalYoutubeAdded++;
                    break;
                case StEvent.ScreenshotAdded:
                    TotalScreenshotAdded++;
                    break;
                case StEvent.MediaRemoved:
                    TotalMediaRemoved++;
                    break;
                case StEvent.CommentAdded:
                    TotalCommentAdded++;
                    break;
                case StEvent.CommentRemoved:
                    TotalCommentRemoved++;
                    break;
                case StEvent.ImageOpened:
                    TotalImageOpened++;
                    break;
                case StEvent.VideoOpened:
                    TotalVideoOpened++;
                    break;
                case StEvent.ScreenshotOpened:
                    TotalScreenshotOpened++;
                    break;
                case StEvent.PdfOpened:
                    TotalPdfOpened++;
                    break;
                case StEvent.SourceOpened:
                    TotalSourceOpened++;
                    break;
                case StEvent.LaserEnabled:
                    TotalLaserEnabled++;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
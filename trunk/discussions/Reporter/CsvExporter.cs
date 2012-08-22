using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Reporter
{
    public class CsvExporter
    {
        public static void Export(string reportPathName,
                                  TopicReport topicReport1, ReportParameters params1,
                                  EventTotalsReport eventTotals1,

                                  TopicReport topicReport2, ReportParameters params2,
                                  EventTotalsReport eventTotals2)
        {
            System.IO.File.WriteAllText(reportPathName, Export(topicReport1, params1, eventTotals1,
                                                               topicReport2, params2, eventTotals2));
        }
      
        public static string Export(TopicReport topicReport1, ReportParameters params1,
                                    EventTotalsReport eventTotals1,

                                    TopicReport topicReport2, ReportParameters params2,
                                    EventTotalsReport eventTotals2)
        {
            //write header
            var sb = new StringBuilder();
            sb.Append("SessionName;");
            sb.Append("TopicName;");
            sb.Append("CumulativeDuration;");
            sb.Append("NumClusteredBadges;");
            sb.Append("NumClusters;");
            sb.Append("NumComments;");
            sb.Append("NumLinks;");
            sb.Append("NumMediaAttachments;");
            sb.Append("NumPoints;");
            sb.Append("NumPointsWithDescription;");
            sb.Append("NumSources;");
            sb.Append("TotalRecordingStarted;");
            sb.Append("TotalRecordingStopped;");
            sb.Append("TotalBadgeCreated;");
            sb.Append("TotalBadgeEdited;");
            sb.Append("TotalBadgeMoved;");
            sb.Append("TotalBadgeZoomIn;");
            sb.Append("TotalClusterCreated;");
            sb.Append("TotalClusterDeleted;");
            sb.Append("TotalClusterIn;");
            sb.Append("TotalClusterOut;");
            sb.Append("TotalClusterMoved;");
            sb.Append("TotalLinkCreated;");
            sb.Append("TotalLinkRemoved;");
            sb.Append("TotalFreeDrawingCreated;");
            sb.Append("TotalFreeDrawingRemoved;");
            sb.Append("TotalFreeDrawingResize;");
            sb.Append("TotalFreeDrawingMoved;");
            sb.Append("TotalSceneZoomedIn;");
            sb.Append("TotalSceneZoomedOut;");
            sb.Append("TotalArgPointTopicChanged;");
            sb.Append("TotalSourceAdded;");
            sb.Append("TotalSourceRemoved;");
            sb.Append("TotalImageAdded;");
            sb.Append("TotalImageUrlAdded;");
            sb.Append("TotalPdfAdded;");
            sb.Append("TotalPdfUrlAdded;");
            sb.Append("TotalYoutubeAdded;");
            sb.Append("TotalScreenshotAdded;");
            sb.Append("TotalMediaRemoved;");
            sb.Append("TotalCommentAdded;");
            sb.Append("TotalCommentRemoved;");
            sb.Append("TotalImageOpened;");
            sb.Append("TotalVideoOpened;");
            sb.Append("TotalScreenshotOpened;");
            sb.Append("TotalPdfOpened;");
            sb.AppendLine("TotalSourceOpened");

            //user event report
            //sb.AppendLine("TotalArgPointTopicChanged");
            //sb.AppendLine("TotalBadgeCreated");
            //sb.AppendLine("TotalBadgeEdited;");
            //sb.AppendLine("TotalBadgeMoved;");
            //sb.AppendLine("TotalBadgeZoomIn;");
            //sb.AppendLine("TotalClusterCreated;");
            //sb.AppendLine("TotalClusterDeleted;");
            //sb.AppendLine("TotalClusterIn;");
            //sb.AppendLine("TotalClusterMoved;");
            //sb.AppendLine("TotalClusterOut;");
            //sb.AppendLine("TotalCommentAdded;");
            //sb.AppendLine("TotalCommentRemoved;");
            //sb.AppendLine("TotalFreeDrawingCreated;");
            //sb.AppendLine("TotalFreeDrawingMoved;");
            //sb.AppendLine("TotalFreeDrawingRemoved;");
            //sb.AppendLine("TotalFreeDrawingResize;");
            //sb.AppendLine("TotalImageAdded;");
            //sb.AppendLine("TotalImageOpened;");
            //sb.AppendLine("TotalImageUrlAdded;");
            //sb.AppendLine("TotalLinkCreated;");
            //sb.AppendLine("TotalLinkRemoved;");
            //sb.AppendLine("TotalMediaRemoved;");
            //sb.AppendLine("TotalPdfAdded;");
            //sb.AppendLine("TotalPdfOpened;");
            //sb.AppendLine("TotalPdfUrlAdded;");
            //sb.AppendLine("TotalSourceAdded;");
            //sb.AppendLine("TotalSourceOpened;");
            //sb.AppendLine("TotalSourceRemoved;");
            //sb.AppendLine("TotalVideoOpened;");
            //sb.AppendLine("TotalYoutubeAdded;");
            //sb.AppendLine("TotalRecordingStarted;");
            //sb.AppendLine("TotalRecordingStopped;");
            //sb.AppendLine("TotalSceneZoomedIn;");
            //sb.AppendLine("TotalSceneZoomedOut;");
            //sb.AppendLine("TotalScreenshotAdded;");
            //sb.AppendLine("TotalScreenshotOpened;");
            //---------------------------------------

            AddSessionTopicRow(sb, topicReport1, params1, eventTotals1);

            if(topicReport2!=null && params2!=null && eventTotals1!=null)
                AddSessionTopicRow(sb, topicReport2, params2, eventTotals2);

            return sb.ToString();
        }

        static void AddSessionTopicRow(StringBuilder sb, TopicReport topicReport1,
                                       ReportParameters params1, 
                                       EventTotalsReport eventTotals1)
        {
            sb.Append("\"" + params1.session.Name + "\"");
            sb.Append(";");
            sb.Append("\"" + params1.topic.Name + "\"");
            sb.Append(";");
            sb.Append(TimeSpan.FromSeconds(topicReport1.cumulativeDuration).ToString());
            sb.Append(";");
            sb.Append(topicReport1.numClusteredBadges.ToString());
            sb.Append(";");
            sb.Append(topicReport1.numClusters.ToString());
            sb.Append(";");
            sb.Append(topicReport1.numComments.ToString());
            sb.Append(";");
            sb.Append(topicReport1.numLinks.ToString());
            sb.Append(";");
            sb.Append(topicReport1.numMediaAttachments.ToString());
            sb.Append(";");
            sb.Append(topicReport1.numPoints.ToString());
            sb.Append(";");
            sb.Append(topicReport1.numPointsWithDescription.ToString());
            sb.Append(";");
            sb.Append(topicReport1.numSources.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalRecordingStarted.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalRecordingStopped.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalBadgeCreated.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalBadgeEdited.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalBadgeMoved.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalBadgeZoomIn.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalClusterCreated.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalClusterDeleted.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalClusterIn.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalClusterOut.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalClusterMoved.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalLinkCreated.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalLinkRemoved.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalFreeDrawingCreated.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalFreeDrawingRemoved.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalFreeDrawingResize.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalFreeDrawingMoved.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalSceneZoomedIn.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalSceneZoomedOut.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalArgPointTopicChanged.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalSourceAdded.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalSourceRemoved.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalImageAdded.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalImageUrlAdded.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalPdfAdded.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalPdfUrlAdded.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalYoutubeAdded.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalRecordingStarted.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalRecordingStopped.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalScreenshotAdded.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalMediaRemoved.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalCommentAdded.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalCommentRemoved.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalImageOpened.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalVideoOpened.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalScreenshotOpened.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalPdfOpened.ToString());
            sb.Append(";");
            sb.AppendLine(eventTotals1.TotalSourceOpened.ToString());
            sb.Append(";");

            //sb.AppendLine(eventUserReport.TotalArgPointTopicChanged.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalBadgeCreated.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalBadgeEdited.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalBadgeMoved.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalBadgeZoomIn.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalClusterCreated.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalClusterDeleted.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalClusterIn.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalClusterMoved.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalClusterOut.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalCommentAdded.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalCommentRemoved.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalFreeDrawingCreated.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalFreeDrawingMoved.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalFreeDrawingRemoved.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalFreeDrawingResize.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalImageAdded.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalImageOpened.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalImageUrlAdded.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalLinkCreated.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalLinkRemoved.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalMediaRemoved.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalPdfAdded.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalPdfOpened.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalPdfUrlAdded.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalSourceAdded.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalSourceOpened.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalSourceRemoved.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalVideoOpened.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalYoutubeAdded.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalSceneZoomedIn.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalSceneZoomedOut.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalScreenshotAdded.ToString());
            //sb.Append(";");
            //sb.AppendLine(eventUserReport.TotalScreenshotOpened.ToString());
        }
    }
}

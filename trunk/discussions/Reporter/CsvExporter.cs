using System;
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

        private static string Export(TopicReport topicReport1, ReportParameters params1,
                                     EventTotalsReport eventTotals1,
                                     TopicReport topicReport2, ReportParameters params2,
                                     EventTotalsReport eventTotals2)
        {
            //write header
            var sb = new StringBuilder();
            sb.Append("SessionName;");
            sb.Append("TopicName;");
            sb.Append("NumUsers;");
            sb.Append("NumImages;");
            sb.Append("NumScreenshots;");
            sb.Append("NumPDFs;");
            sb.Append("NumVideos;");
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
            sb.Append("TotalClusterTitlesAdded;");
            sb.Append("TotalClusterTitlesEdited;");
            sb.Append("TotalClusterTitlesRemoved;");
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
            sb.AppendLine("TotalSourceOpened;");
            sb.AppendLine("TotalLaserEnabled;");

            //first line
            AddSessionTopicRow(sb, topicReport1, params1, eventTotals1);

            //second line
            if (topicReport2 != null && params2 != null && eventTotals2 != null)
                AddSessionTopicRow(sb, topicReport2, params2, eventTotals2);

            return sb.ToString();
        }

        private static void AddSessionTopicRow(StringBuilder sb, TopicReport topicReport1,
                                               ReportParameters params1,
                                               EventTotalsReport eventTotals1)
        {
            sb.Append("\"" + params1.session.Name + "\"");
            sb.Append(";");
            sb.Append("\"" + params1.topic.Name + "\"");
            sb.Append(";");
            sb.Append(params1.sessionTopicUsers.Count());
            sb.Append(";");
            sb.Append(topicReport1.numImages);
            sb.Append(";");
            sb.Append(topicReport1.numScreenshots);
            sb.Append(";");
            sb.Append(topicReport1.numPDFs);
            sb.Append(";");
            sb.Append(topicReport1.numYoutubes);
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
            sb.Append(eventTotals1.TotalClusterTitlesAdded.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalClusterTitlesEdited.ToString());
            sb.Append(";");
            sb.Append(eventTotals1.TotalClusterTitlesRemoved.ToString());
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
            sb.Append(eventTotals1.TotalLaserEnabled.ToString() + ";");
        }
    }
}
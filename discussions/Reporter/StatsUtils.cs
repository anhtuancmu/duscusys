using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Reporter
{
    public class StatsUtils
    {
        public static List<int> Union(List<int> lst1, List<int> lst2)
        {
            var res = new List<int>();
            foreach (var i in lst1)
                if (!res.Contains(i))
                    res.Add(i);
            foreach (var i in lst2)
                if (!res.Contains(i))
                    res.Add(i);
            return res;
        }

        public static TextBlock GetEventTotals(EventTotalsReport eTotals)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<event totals>");

            sb.Append("no. arg.point topic changed ");
            sb.AppendLine(eTotals.TotalArgPointTopicChanged.ToString());

            sb.Append("no. badge created ");
            sb.AppendLine(eTotals.TotalBadgeCreated.ToString());

            sb.Append("no. badge edited ");
            sb.AppendLine(eTotals.TotalBadgeEdited.ToString());

            sb.Append("no. badge moved ");
            sb.AppendLine(eTotals.TotalBadgeMoved.ToString());

            sb.Append("no. badge zoom in ");
            sb.AppendLine(eTotals.TotalBadgeZoomIn.ToString());

            sb.Append("no. cluster created ");
            sb.AppendLine(eTotals.TotalClusterCreated.ToString());

            sb.Append("no. cluster deleted ");
            sb.AppendLine(eTotals.TotalClusterDeleted.ToString());

            sb.Append("no. cluster-in ");
            sb.AppendLine(eTotals.TotalClusterIn.ToString());

            sb.Append("no. cluster moved ");
            sb.AppendLine(eTotals.TotalClusterMoved.ToString());

            sb.Append("no. cluster-out ");
            sb.AppendLine(eTotals.TotalClusterOut.ToString());

            sb.Append("no. cluster titles added ");
            sb.AppendLine(eTotals.TotalClusterTitlesAdded.ToString());

            sb.Append("no. cluster titles edited ");
            sb.AppendLine(eTotals.TotalClusterTitlesEdited.ToString());

            sb.Append("no. cluster titles removed ");
            sb.AppendLine(eTotals.TotalClusterTitlesRemoved.ToString());

            sb.Append("no. comment added ");
            sb.AppendLine(eTotals.TotalCommentAdded.ToString());

            sb.Append("no. comment removed ");
            sb.AppendLine(eTotals.TotalCommentRemoved.ToString());

            sb.Append("no. free drawing created ");
            sb.AppendLine(eTotals.TotalFreeDrawingCreated.ToString());

            sb.Append("no. free drawing moved ");
            sb.AppendLine(eTotals.TotalFreeDrawingMoved.ToString());

            sb.Append("no. free drawing removed ");
            sb.AppendLine(eTotals.TotalFreeDrawingRemoved.ToString());

            sb.Append("no. free drawing resize ");
            sb.AppendLine(eTotals.TotalFreeDrawingResize.ToString());

            sb.Append("no. image added ");
            sb.AppendLine(eTotals.TotalImageAdded.ToString());

            sb.Append("no. image opened ");
            sb.AppendLine(eTotals.TotalImageOpened.ToString());

            sb.Append("no. image url added ");
            sb.AppendLine(eTotals.TotalImageUrlAdded.ToString());

            sb.Append("no. link created ");
            sb.AppendLine(eTotals.TotalLinkCreated.ToString());

            sb.Append("no. link removed ");
            sb.AppendLine(eTotals.TotalLinkRemoved.ToString());

            sb.Append("no. media removed ");
            sb.AppendLine(eTotals.TotalMediaRemoved.ToString());

            sb.Append("no. PDF added ");
            sb.AppendLine(eTotals.TotalPdfAdded.ToString());

            sb.Append("no. PDF opened ");
            sb.AppendLine(eTotals.TotalPdfOpened.ToString());

            sb.Append("no. PDF url added ");
            sb.AppendLine(eTotals.TotalPdfUrlAdded.ToString());

            sb.Append("no. source added ");
            sb.AppendLine(eTotals.TotalSourceAdded.ToString());

            sb.Append("no. source opened ");
            sb.AppendLine(eTotals.TotalSourceOpened.ToString());

            sb.Append("no. source removed ");
            sb.AppendLine(eTotals.TotalSourceRemoved.ToString());

            sb.Append("no. video opened ");
            sb.AppendLine(eTotals.TotalVideoOpened.ToString());

            sb.Append("no. video added ");
            sb.AppendLine(eTotals.TotalYoutubeAdded.ToString());

            sb.Append("no. recording started ");
            sb.AppendLine(eTotals.TotalRecordingStarted.ToString());

            sb.Append("no. recording stopped ");
            sb.AppendLine(eTotals.TotalRecordingStopped.ToString());

            sb.Append("no. scene zoom in ");
            sb.AppendLine(eTotals.TotalSceneZoomedIn.ToString());

            sb.Append("no. scene zoom out ");
            sb.AppendLine(eTotals.TotalSceneZoomedOut.ToString());

            sb.Append("no. screenshot added ");
            sb.AppendLine(eTotals.TotalScreenshotAdded.ToString());

            sb.Append("no. screenshot opened ");
            sb.AppendLine(eTotals.TotalScreenshotOpened.ToString());

            sb.Append("no. laser enabled ");
            sb.AppendLine(eTotals.TotalLaserEnabled.ToString());

            return WrapText(sb.ToString());
        }

        public static TextBlock WrapText(string txt)
        {
            var res = new TextBlock {Text = txt};
            return res;
        }
    }
}
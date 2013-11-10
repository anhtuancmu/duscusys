using System;
using System.Data;
using System.Windows.Threading;
using Discussions.DbModel;
using Discussions.rt;
using Microsoft.Surface.Presentation.Controls;
using System.Linq;

namespace Discussions
{
    public class CommentDismissalRecognizer
    {
        private const int ReadingTime = 1;//s

        DispatcherTimer _commentReader;

        private ArgPoint _recentPoint;

        private readonly SurfaceScrollViewer _scrollViewer;

        private readonly Action<ArgPoint> _onDismiss;

        public CommentDismissalRecognizer(SurfaceScrollViewer scrollViewer, Action<ArgPoint> onDismiss)
        {
            _scrollViewer = scrollViewer;
            _onDismiss = onDismiss;
        }

        void StartTimer()
        {
            if (_commentReader == null)
            {
                _commentReader = new DispatcherTimer { Interval = TimeSpan.FromSeconds(ReadingTime) };
                _commentReader.Tick += _commentReader_Tick;
            }

            if (!_commentReader.IsEnabled)
                _commentReader.Start();
        }

        void StopTimer()
        {
            if (_commentReader != null)
                _commentReader.Stop();
        }

        private void _commentReader_Tick(object sender, EventArgs e)
        {
            StopTimer();

            //if point is new and has not been saved
            if (_recentPoint == null || _recentPoint.Topic == null)
                return;

            _onDismiss(_recentPoint);

            _recentPoint = null;//only single notification per arg.point during reading corridor
        }

        static bool IsTheBadgeAtBottom(SurfaceScrollViewer scroller)
        {
            var maxVOffset = scroller.ExtentHeight - scroller.ViewportHeight;
            const int threshold = 10;
            return Math.Abs(scroller.VerticalOffset - maxVOffset) < threshold;
        }

        public void CheckScrollState()
        {
            if (_recentPoint == null)
            {
                StopTimer();
                return;
            }
            Console.WriteLine(DateTime.Now);

            if (IsTheBadgeAtBottom(_scrollViewer))
                StartTimer();
            else
                StopTimer();
        }

        /// <summary>
        /// If for example topic changes, or arg.point changes, we reset any possible existing timing
        /// </summary>
        public void Reset(ArgPoint ap)
        {
            StopTimer();
            _recentPoint = ap;
        }

        //////////////////////////////////////////////////////////

        #region helpers

        public static string FormatNumUnreadComments(int numUnread) 
        {
            if (numUnread > 0)
                return string.Format("Feedback  +{0}", numUnread);
            return "Feedback";
        }

        //all the changes will be cached locally without refreshing/redropping the context         
        public static void pushDismissal(ArgPoint ap, DiscCtx ctx)
        {
            var persId = SessionInfo.Get().person.Id;

            var apId = ap.Id;
            ap = ctx.ArgPoint.Single(ap0 => ap0.Id == apId);

            foreach (var c in ap.Comment)
            {
                //skip "new comment"
                if(c.Person == null)
                    continue;                
                
                var existing = c.ReadEntry.FirstOrDefault(re => re.Person.Id == persId);               
                if (existing == null)
                {
                    var entry = new CommentPersonReadEntry
                        {
                            Comment = c,
                            Person = ctx.Person.Single(p => p.Id == persId)
                        };
                    ctx.AddToCommentPersonReadEntry(entry);
                }                
            }

            try
            {
                ctx.SaveChanges();
            }
            catch
            {                
            }

            UISharedRTClient.Instance.clienRt.SendCommentsRead(SessionInfo.Get().person.Id,
                                                              ap.Topic.Id,
                                                              ap.Id);
        }

        #endregion

    }
}
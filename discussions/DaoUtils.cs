using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Text;
using Discussions.ctx;
using Discussions.DbModel.model;
using Discussions.model;
using Discussions.DbModel;
using System.Windows.Media;
using Discussions.stats;
using System.Linq;
using Discussions.view;

namespace Discussions
{
    public class DaoUtils
    {
        public static string MODER_SUBNAME = "moder";

        public static void DeleteDiscussion(Discussion d)
        {
            if (!Ctors.DiscussionExists(d))
                return;

            var ctx = PublicBoardCtx.Get();

            //delete attachments 
            var attachments = new List<Attachment>();
            foreach (var a in d.Attachment)
                attachments.Add(a);
            foreach (var a in attachments)
                ctx.DeleteObject(a);

            //delete background             
            if (d.Background != null)
                d.Background = null;

            foreach (var t in d.Topic)
            {
                t.Person.Clear();
                t.ArgPoint.Clear();
            }
            d.Topic.Clear();

            d.GeneralSide.Clear();

            ctx.DeleteObject(d);

            ctx.SaveChanges();
        }

        public static Person PersonSingleton(Person template, out bool prevExists)
        {
            Person prev = PublicBoardCtx.Get().Person.FirstOrDefault(p0 => p0.Name == template.Name &&
                                                                         p0.Email == template.Email);
            if (prev != null)
            {
                prevExists = true;
                return prev;
            }
            else
            {
                prevExists = false;
                return template;
            }
        }

        public static void EnsureModerExists()
        {
            var ctx = PublicBoardCtx.Get();

            var q = from p in ctx.Person
                    where p.Name.IndexOf(MODER_SUBNAME) != -1
                    select p;

            if (q.Count() == 0)
            {
                ctx.AddToPerson(Ctors.NewPerson("moderator", "moder-mail"));
                ctx.SaveChanges();
            }
        }

        public static void deletePersonAndPoints(Person p)
        {
            var ctx = PublicBoardCtx.Get();

            //remove the point from topic 
            foreach (var pt in p.ArgPoint)
            {
                if (pt.Topic != null)
                    pt.Topic.ArgPoint.Remove(pt);
                pt.Topic = null;
            }
            p.ArgPoint.Clear();

            foreach (var s in p.Screenshot.ToList())
            {
                s.Discussion = null;
                s.Person = null;
                ctx.Annotation.DeleteObject(s);
            }

            //remove the speaker from all topics
            p.Topic.Clear();

            p.Screenshot.Clear();

            p.GeneralSide.Clear();

            //delete the person
            try
            {
                ctx.Person.DeleteObject(p);
            }
            catch (Exception)
            {
                //if person doesn't exist, ignore 
            }

            ctx.SaveChanges();
        }

        public static ArgPoint NewPoint(Topic t, int orderNumber)
        {
            if (t == null)
                return null;

            //create new point 
            ArgPoint pt = new ArgPoint();
            pt.Point = "Your point here";
            pt.RecentlyEnteredSource = "Your source here";
            pt.RecentlyEnteredMediaUrl = "Your media link here";
            DaoUtils.EnsurePtDescriptionExists(pt);

            pt.Description.Text = NEW_DESCRIPTION;
            pt.Topic = PrivateCenterCtx.Get().Topic.FirstOrDefault(t0 => t0.Id == t.Id);
            int selfId = SessionInfo.Get().person.Id;
            var pers = PrivateCenterCtx.Get().Person.FirstOrDefault(p0 => p0.Id == selfId);
            pt.Person = pers;
            pt.SharedToPublic = true;
            pt.SideCode = DaoUtils.GetGeneralSide(SessionInfo.Get().person,
                                                  SessionInfo.Get().discussion);
            pt.OrderNumber = orderNumber;

            return pt;
        }

        public static void DeleteArgPoint(DiscCtx ctx, ArgPoint p)
        {
            p.Topic = null;
            p.Person = null;
            p.Comment.Clear();
            p.Attachment.Clear();
            p.Description = null;

            try
            {
                ctx.DeleteObject(p);
            }
            catch (Exception)
            {
                //if person doesn't exist, ignore 
            }
        }

        public static void UnattachPoint(ArgPoint p)
        {
            p.Topic = null;
            p.Person = null;
        }

        public static void removePersonsAndTopic(Topic t)
        {
            if (t == null)
                return;

            var ctx = PublicBoardCtx.Get();
            t.Person.Clear();
            t.ArgPoint.Clear();
            t.Discussion = null;
            ctx.SaveChanges();
        }

        public static void SetGeneralSide(Person p, Discussion d, int side)
        {
            if (p == null || d == null)
                return;

            var q = from genSide in PublicBoardCtx.Get().GeneralSide
                    where genSide.Discussion.Id == d.Id && genSide.Person.Id == p.Id
                    select genSide;
            if (q.Count() > 0)
                q.First().Side = side;
            else
                PublicBoardCtx.Get().AddToGeneralSide(Ctors.NewGenSide(p, d, side));
        }

        public static int GetGeneralSide(Person p, Discussion d)
        {
            if (p == null || d == null)
                return -1;

            var q = from genSide in PublicBoardCtx.Get().GeneralSide
                    where genSide.Discussion.Id == d.Id && genSide.Person.Id == p.Id
                    select genSide;

            if (q.Count() > 0)
                return q.First().Side;
            else
                return -1;
        }

        public static void AddSource(string newSrc, RichText richText)
        {
            if (richText == null)
                return;

            Source src = new Source();
            src.Text = newSrc;
            richText.Source.Add(src);
        }

        public static void AddSource(RichText r)
        {
            if (r == null)
                return;

            var dlg = new SourceDialog();
            dlg.ShowDialog();

            if (dlg.Source == null)
                return;

            DaoUtils.AddSource(dlg.Source, r);
        }

        public static void EnsureBgExists(Discussion discussion)
        {
            if (discussion == null)
                return;

            if (discussion.Background == null)
                discussion.Background = new RichText();
        }

        public static void EnsurePtDescriptionExists(ArgPoint point)
        {
            if (point == null)
                return;

            if (point.Description == null)
                point.Description = new RichText();
        }

        public static IEnumerable<Annotation> GetAnnotations(Discussion d)
        {
            var q = from ans in PublicBoardCtx.Get().Annotation
                    where ans.Discussion.Id == d.Id
                    select ans;
            return q;
        }

        public static void RemoveAnnotation(Annotation a)
        {
            a.Person = null;
            a.Discussion = null;

            PublicBoardCtx.Get().Annotation.DeleteObject(a);
        }

        //used for coloring shapes in graphics editor after users
        public static Color UserIdToColor(int id)
        {
            Person p = PublicBoardCtx.Get().Person.FirstOrDefault(p0 => p0.Id == id);
            if (p == null)
                return Colors.AliceBlue;
            else
                return Utils.IntToColor(p.Color);
        }

        public static void UnpublishPoint(ArgPoint ap)
        {
            ap.SharedToPublic = false;
        }

        public static List<EventViewModel> GetRecentEvents()
        {
            var res = new List<EventViewModel>();

            var events = (from s in PublicBoardCtx.Get().StatsEvent
                          orderby s.Time descending
                          select s).Take(10);

            foreach (var e in events.ToArray().Reverse())
                res.Add(new EventViewModel((StEvent)e.Event, e.UserId, e.Time, (DeviceType)e.DeviceType));
          
            return res;
        }

        private const string NewComment = NEW_COMMENT;

        static void RemovePlaceholders(ArgPoint ap)
        {
            if (ap == null)
                return;

            var placeholderComment =
                   ap.Comment.FirstOrDefault(c0 => c0.Text == NewComment || string.IsNullOrWhiteSpace(c0.Text));
            if (placeholderComment != null)
                placeholderComment.ArgPoint.Comment.Remove(placeholderComment);
        }

        public static bool IsPlaceholder(Comment c)
        {
            return (c.Text == NewComment || string.IsNullOrWhiteSpace(c.Text));
        }

        public static Comment EnsureCommentPlaceholderExists(ArgPoint ap)
        {
            if (ap == null)
                return null;
            
            RemovePlaceholders(ap);

            bool needNewPlaceholder = false;
            if (ap.Comment.Count == 0)
            {
                needNewPlaceholder = true;
            }
            else
            {
                var placeholderComment =
                    ap.Comment.FirstOrDefault(c0 => c0.Text == NewComment || string.IsNullOrWhiteSpace(c0.Text));
                needNewPlaceholder = (placeholderComment == null);
                //if (placeholderComment != null)
                //{
                //    placeholderComment.Person = null;
                //    placeholderComment.Text = DaoUtils.NEW_COMMENT; //in case of comment was whitespace  
                //}
            }

            if (needNewPlaceholder)
            {
                var c = new Comment();
                c.Text = NewComment;
                ap.Comment.Add(c);
                return c;
            }

            return null;
        }

        //returns true if changes owner 
        public static bool InjectAuthorOfComment(Comment c, Person commentAuthor)
        {
            bool changed = false;
            if (c != null)
            {
                if ((c.Person == null && commentAuthor != null) ||
                    (c.Person != null && commentAuthor == null))
                {
                    changed = true;
                }
                else
                    changed = c.Person.Id != commentAuthor.Id;

                c.Person = commentAuthor;
            }

            return changed;
        }

        public const string NEW_COMMENT = "New comment";
        public const string NEW_POINT_NAME = "Your point here";
        public const string NEW_DESCRIPTION = "Description";

        public static MediaData CreateMediaData(byte[] data)
        {
            var res = new MediaData();
            res.Data = data;
            return res;
        }

        public static void RemoveDuplicateComments(ArgPoint ap)
        {
            var emptyComments = ap.Comment.Where(c0 => c0.Text == NEW_COMMENT);
            if (emptyComments.Count() > 1)
            {
                for (int i = 0; i < emptyComments.Count() - 1; i++)
                    ap.Comment.Remove(emptyComments.ElementAt(i));
            }
        }

        public static ArgPoint clonePoint(DiscCtx ctx, ArgPoint ap, Topic topic, Person owner, String name)
        {
            var top = ctx.Topic.FirstOrDefault(t0 => t0.Id == topic.Id);
            if (top == null)
                return null;

            var ownPoints = top.ArgPoint.Where(p0 => p0.Person.Id == owner.Id);
            int orderNr = 1;
            foreach (var pt in ownPoints)
            {
                if (pt.OrderNumber > orderNr)
                    orderNr = pt.OrderNumber;
            }

            var pointCopy = DaoUtils.NewPoint(top, orderNr + 1);
            pointCopy.Point = name;
            pointCopy.Description.Text = ap.Description.Text;

            foreach (var src in ap.Description.Source)
            {
                var newSrc = new Source();
                newSrc.Text = src.Text;
                pointCopy.Description.Source.Add(newSrc);
            }

            foreach (var cmt in ap.Comment)
            {
                if (cmt.Person == null)
                    continue;

                var comment = new Comment();
                comment.Text = cmt.Text;
                var commentPersonId = cmt.Person.Id;
                comment.Person = ctx.Person.FirstOrDefault(p0 => p0.Id == commentPersonId);
                pointCopy.Comment.Add(comment);
            }

            var ownId = SessionInfo.Get().person.Id;
            var self = ctx.Person.FirstOrDefault(p0 => p0.Id == ownId);
            foreach (var media in ap.Attachment)
            {
                var attach = new Attachment();
                attach.ArgPoint = pointCopy;
                attach.Format = media.Format;
                attach.Link = media.Link;
                attach.Name = media.Name;
                attach.Title = media.Title;
                attach.VideoEmbedURL = media.VideoEmbedURL;
                attach.VideoLinkURL = media.VideoLinkURL;
                attach.VideoThumbURL = media.VideoThumbURL;
                attach.OrderNumber = media.OrderNumber;

                if (media.Thumb != null)
                    attach.Thumb = (byte[])media.Thumb.Clone();

                if (media.MediaData != null && media.MediaData.Data != null)
                {
                    var mediaClone = new MediaData();
                    mediaClone.Data = (byte[])media.MediaData.Data.Clone();
                    attach.MediaData = mediaClone;
                }

                attach.Person = self;
            }

            pointCopy.Person = self;

            pointCopy.Topic = top;

            return pointCopy;
        }

        //moder online & session running > ok
        public static bool DashboardsAvailable(DiscCtx ctx)
        {
            return true;
        }

        public static bool ArgPointInTopic(int apId, int topicId)
        {
            var ap = PublicBoardCtx.Get().ArgPoint.FirstOrDefault(p0 => p0.Id == apId);
            if (ap == null)
                return false;

            if (ap.Topic == null)
                return false;

            return ap.Topic.Id == topicId;
        }

        public static IEnumerable<Person> Participants(Topic topic, Session session)
        {
            var topId = topic.Id;
            return session.Person.Where(p => p.Topic.Any(t => t.Id == topId));
        }

        public static IEnumerable<ArgPoint> ArgPointsOf(Person pers, Discussion d, Topic t)
        {
            return pers.ArgPoint.Where(ap => ap.Topic != null && ap.Topic.Id == t.Id);
        }

        #region comment notifications

        //we use separate context from one used by private board, not to interfere with it
        public static IEnumerable<NewCommentsFrom> NumCommentsUnreadBy(DiscCtx ctx, int ArgPointId)
        {
            var res = new List<NewCommentsFrom>();

            //new point that hasn't been saved
            if (ArgPointId == 0)
                return res;

            var selfId = SessionInfo.Get().person.Id;

            var ap = ctx.ArgPoint.FirstOrDefault(ap0 => ap0.Id == ArgPointId);

            foreach (var c in ap.Comment)
            {
                if (c.Person == null)
                    continue;

                //skip own comment
                if (c.Person.Id == selfId)
                    continue;

                //if self is not in number of those who read the comment
                if (!c.ReadEntry.Any(re => re.Person.Id == selfId))
                {
                    var bin = res.Find(ncf => ncf.Person.Id == c.Person.Id);
                    if (bin == null)
                    {
                        bin = new NewCommentsFrom(c.Person);
                        res.Add(bin);
                    }
                    bin.NumNewComments++;
                }
            }

            return res;
        }
        
        public static List<int> SubsetOfPersonsWithDots(DiscCtx ctx, int[] personIds, int topicId)
        {
            var topic = ctx.Topic.FirstOrDefault(t => t.Id == topicId);
            if (topic == null)
                return null;

            var res = new List<int>();

            foreach (var ap in topic.ArgPoint)
            {
                if (!personIds.Contains(ap.Person.Id))
                    continue;                

                if (NumCommentsUnreadBy(ctx, ap.Id).Total() > 0)
                    if (!res.Contains(ap.Person.Id))
                    {
                        res.Add(ap.Person.Id);
                    }
            }

            return res;
        }
       
        public static string RecentCommentReadBy(DiscCtx ctx, int argPointId)
        {
            if (argPointId == 0)
                return "";

            var selfId = SessionInfo.Get().person.Id;

            var ap = ctx.ArgPoint.FirstOrDefault(ap0 => ap0.Id == argPointId);
            if (ap == null)
                return "";

            //var recentComment = ap.Comment.LastOrDefault(c => c.Person != null && c.Person.Id == selfId);
            //use recent comment by anyone
            var recentComment = ap.Comment.LastOrDefault(c => c.Person != null);
            if (recentComment == null || recentComment.Person==null)
                return "";

            var res = new StringBuilder(string.Format("\"{0}\" seen by ", 
                                            SummaryTextConvertor.ShortenLine(recentComment.Text, 15)
                                           )
                                       );
            var atLeastOneReader = false;
            var unreadUsers = new List<int>(ap.Topic.Person.Where(p => p.Id != selfId).Select(p => p.Id));          
            foreach (var readEntry in recentComment.ReadEntry)
            {
                unreadUsers.Remove(readEntry.Person.Id);

                if (readEntry.Person.Id != selfId && recentComment.Person.Id != readEntry.Person.Id)
                {
                    if (atLeastOneReader)
                    {
                        res.Append(", ");
                    }
                    atLeastOneReader = true;

                    res.Append(readEntry.Person.Name);    
                }
            }

            if (unreadUsers.Count == 0)
            {
                return string.Format("\"{0}\" seen by all", SummaryTextConvertor.ShortenLine(recentComment.Text, 15) );
            }

            if (atLeastOneReader)
                return res.ToString();

            return "";
        }
       
        /// <summary>
        /// Own comment cannot be new. 
        /// Placeholder cannot be new. 
        /// Comment of other user is new if unread by us.
        /// </summary>
        public static bool IsCommentNewForUs(DiscCtx ctx, int commentId)
        {
            //new comment that hasn't been saved
            if (commentId == 0)
                return false;

            var selfId = SessionInfo.Get().person.Id;

            var c = ctx.Comment.FirstOrDefault(c0 => c0.Id == commentId);
            if (c == null)
                return false;

            if (c.Person == null)
                return false;

            if (c.Person.Id == selfId)
                return false;

            //if self is not in number of those who read the comment, the comment is new for us
            return c.ReadEntry.All(re => re.Person.Id != selfId);            
        }
        #endregion
    }
}
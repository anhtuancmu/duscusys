using System.Collections.Generic;
using System.Linq;
using Discussions;
using Discussions.DbModel;
using TdsSvc.Model;

namespace TdsSvc
{
    public class TdsService : ITdsService
    {
        public void AddComment(SComment comment)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var argPoint = ctx.ArgPoint.FirstOrDefault(ap => ap.Id == comment.ArgPointId);
                if (argPoint == null)
                    return;

                var person = ctx.Person.FirstOrDefault(ap => ap.Id == comment.PersonId);
                if (person == null)
                    return;

                var newComment = new Comment {ArgPoint = argPoint, Person = person, Text = comment.Text};

                argPoint.Comment.Add(newComment);

                ctx.SaveChanges();
            }
        }

        public void ChangeCommentText(int commentId, string newText)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var dbComment = ctx.Comment.FirstOrDefault(c => c.Id == commentId);
                if (dbComment == null)
                    return;

                if (newText != dbComment.Text)
                {
                    dbComment.Text = newText;
                    ctx.SaveChanges();
                }
            }
        }
        public void RemoveComment(int commentId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var dbComment = ctx.Comment.FirstOrDefault(c => c.Id == commentId);
                if (dbComment == null)
                    return;

                ctx.DeleteObject(dbComment);
                ctx.SaveChanges();
            }
        }

        public List<SComment> GetCommentsInArgPoint(int pointId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                return ctx.Comment.Where(c => c.ArgPoint.Id == pointId).Select(c => new SComment(c)).ToList();
            }
        }

        public List<SNewCommentsFrom> NumCommentsUnreadBy(int pointId, int callerId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                return DAL.Helper.NumCommentsUnreadBy(ctx, pointId, callerId);
            }                           
        }

        public List<int> SubsetOfPersonsWithDots(List<int> personIds, int topicId, int callerId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                return DAL.Helper.SubsetOfPersonsWithDots(ctx, personIds, topicId, callerId);
            } 
        }

        public SArgPoint GetArgPoint(int pointId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var point = ctx.ArgPoint.FirstOrDefault(ap => ap.Id == pointId);
                if (point == null)
                    return null;

                return point.ToServiceEntity();
            } 
        }

        public List<SArgPoint> GetArgPointsInTopic(int topicId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var points = ctx.ArgPoint.Where(ap => ap.Topic.Id==topicId);
                return points.Select(p => p.ToServiceEntity()).ToList();                
            }
        }

        public List<SSource> GetSourcesInArgPoint(int pointId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var point = ctx.ArgPoint.FirstOrDefault(ap => ap.Id == pointId);
                if (point == null || point.Description==null)
                    return new List<SSource>();

                return point.Description.Source
                    .OrderBy(s => s.OrderNumber)
                    .Select(s=>new SSource(s))
                    .ToList();                
            }
        }

        public void AddSourceArgPoint(int pointId, string text, int callerId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var point = ctx.ArgPoint.FirstOrDefault(ap => ap.Id == pointId);
                if (point == null)
                    return;

                var lastSrc = point.Description.Source.OrderBy(s => s.OrderNumber).LastOrDefault();

                int orderNumber = lastSrc != null ? lastSrc.OrderNumber + 1 : 0;

                var source = new Source
                {
                    OrderNumber = orderNumber, 
                    RichText = null, 
                    Text = text
                };
                point.Description.Source.Add(source);

                ctx.SaveChanges();
            }
        }

        public bool MoveSourceUp(int sourceId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var res = DAL.Helper.SwapSourceWithNeib(ctx, true, sourceId);
                ctx.SaveChanges();
                return res;
            }
        }

        public bool MoveSourceDown(int sourceId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var res = DAL.Helper.SwapSourceWithNeib(ctx, false, sourceId);
                ctx.SaveChanges();
                return res;
            }
        }

        public List<SOrderInfo> GetSourcesOrder(int pointId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var point = ctx.ArgPoint.FirstOrDefault(ap => ap.Id == pointId);
                if (point == null)
                    return new List<SOrderInfo>();

                return point.Description.Source
                    .OrderBy(c => c.OrderNumber)
                    .Select(c => new SOrderInfo(c.Id, c.OrderNumber))
                    .ToList();                 
            }
        }

        public void AddAttachmentToPoint(int pointId, SInAttachment attachment, int callerId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var point = ctx.ArgPoint.FirstOrDefault(ap => ap.Id == pointId);
                if (point == null)
                    return;

                DAL.Helper.AddAttachment(ctx, argPoint:point, 
                                        discussion:null, personWithAvatar:null, 
                                        attachment:attachment,
                                        callerId:callerId);

                ctx.SaveChanges();
            }            
        }

        public void AddAttachmentToDiscussion(int discussionId, SInAttachment attachment, int callerId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var discussion = ctx.Discussion.FirstOrDefault(d => d.Id == discussionId);
                if (discussion == null)
                    return;

                DAL.Helper.AddAttachment(ctx, argPoint: null,
                                        discussion: discussion, personWithAvatar: null,
                                        attachment: attachment,
                                        callerId: callerId);

                ctx.SaveChanges();
            }
        }

        public void AddAttachmentToOwnAvatar(SInAttachment attachment, int callerId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var avatarOwner = ctx.Person.FirstOrDefault(p => p.Id == callerId);
                if (avatarOwner == null)
                    return;

                DAL.Helper.AddAttachment(ctx, argPoint: null,
                                        discussion: null, personWithAvatar: avatarOwner,
                                        attachment: attachment,
                                        callerId: callerId);

                ctx.SaveChanges();
            }
        }

        public bool MoveAttachmentUp(int attachmentId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var res = DAL.Helper.SwapAttachmentWithNeib(ctx, true, attachmentId);
                ctx.SaveChanges();
                return res;
            }
        }

        public bool MoveAttachmentDown(int attachmentId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var res = DAL.Helper.SwapAttachmentWithNeib(ctx, false, attachmentId);
                ctx.SaveChanges();
                return res;
            }
        }

        public List<SOutAttachment> GetAttachmentsInArgPoint(int pointId, bool includeMediaData)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var point = ctx.ArgPoint.FirstOrDefault(ap => ap.Id == pointId);
                if (point == null)
                    return new List<SOutAttachment>();

                return point.Attachment
                        .Where(at => at.ArgPoint.Id == pointId)
                        .Select(at => new SOutAttachment(at, includeMediaData))
                        .ToList();
            }
        }

        public List<SOrderInfo> GetAttachmentsOrder(int pointId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var point = ctx.ArgPoint.FirstOrDefault(ap => ap.Id == pointId);
                if (point == null)
                    return new List<SOrderInfo>();

                return point.Attachment
                    .Where(at => at.ArgPoint.Id == pointId)
                    .Select(at => new SOrderInfo(at.Id, at.OrderNumber))
                    .ToList();
            }
        }

        public void RemoveAttachment(int attachmentId)
        {
            using (var ctx = new DiscCtx(ConfigManager.ConnStr))
            {
                var attachment = ctx.Attachment.FirstOrDefault(at => at.Id == attachmentId);
                if (attachment == null)
                    return;
                
                if (attachment.MediaData != null)                
                    ctx.DeleteObject(attachment.MediaData);                 
                ctx.DeleteObject(attachment);

                ctx.SaveChanges();
            }
        }
    }
}

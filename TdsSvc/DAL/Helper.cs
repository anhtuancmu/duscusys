using System.Collections.Generic;
using System.Linq;
using Discussions.DbModel;
using TdsSvc.Model;

namespace TdsSvc.DAL
{
    public static class Helper
    {
        public static List<SNewCommentsFrom> NumCommentsUnreadBy(DiscCtx ctx, int argPointId, int callerId)
        {
            var res = new List<SNewCommentsFrom>();

            //new point that hasn't been saved
            if (argPointId == 0)
                return res;

            var ap = ctx.ArgPoint.FirstOrDefault(ap0 => ap0.Id == argPointId);
            if(ap==null)


            foreach (var c in ap.Comment)
            {
                if (c.Person == null)
                    continue;

                //skip own comment
                if (c.Person.Id == callerId)
                    continue;

                //if self is not in number of those who read the comment
                if (!c.ReadEntry.Any(re => re.Person.Id == callerId))
                {
                    var bin = res.Find(ncf => ncf.PersonId == c.Person.Id);
                    if (bin == null)
                    {
                        bin = new SNewCommentsFrom {PersonId = c.Person.Id, PersonName = c.Person.Name};
                        res.Add(bin);
                    }
                    bin.NumNewComments++;
                }
            }

            return res;
        }

        public static List<int> SubsetOfPersonsWithDots(DiscCtx ctx, List<int> personIds, int topicId, int callerId)
        {
            var topic = ctx.Topic.FirstOrDefault(t => t.Id == topicId);
            if (topic == null)
                return new List<int>();

            var res = new List<int>();
            
            foreach (var ap in topic.ArgPoint)
            {
                if (!personIds.Contains(ap.Person.Id))
                    continue;

                if (NumCommentsUnreadBy(ctx, ap.Id, callerId).Total() > 0)
                    if (!res.Contains(ap.Person.Id))
                    {
                        res.Add(ap.Person.Id);
                    }
            }

            return res;
        }

        public static bool SwapSourceWithNeib(DiscCtx ctx, bool withTopNeib, int sourceId)
        {
            var src = ctx.Source.FirstOrDefault(s => s.Id == sourceId);
            if (src == null)
                return false;

            return SwapSourceWithNeib(withTopNeib, src);
        }

        //swaps current source with its neighbour
        static bool SwapSourceWithNeib(bool withTopNeib, Source current)
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

        public static bool SwapAttachmentWithNeib(DiscCtx ctx, bool withTopNeib, int attachmentId)
        {
            var att = ctx.Attachment.FirstOrDefault(s => s.Id == attachmentId);
            if (att == null)
                return false;

            return SwapAttachmentWithNeib(withTopNeib, att);
        }

        //swaps the attachment with its neighbour
        public static bool SwapAttachmentWithNeib(bool withTopNeib, Attachment current)
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

        public static void AddAttachment(DiscCtx ctx, 
                                         ArgPoint argPoint, Discussion discussion, Person personWithAvatar,
                                         SInAttachment attachment,
                                         int callerId)
        {
            var caller = ctx.Person.FirstOrDefault(p => p.Id == callerId);
            if (caller == null)
                return;
                                    
            var dbMediaData = new MediaData {Data = attachment.MediaData};

            var dbAttachent = attachment.ToDbEntity(ctx);
            dbAttachent.MediaData = dbMediaData;

            //switch 
            {
                dbAttachent.Discussion = discussion;
                dbAttachent.PersonWithAvatar = personWithAvatar;
                dbAttachent.ArgPoint = argPoint;
            }

            dbAttachent.Person = caller;

            argPoint.Attachment.Add(dbAttachent);            
        }
    }
}
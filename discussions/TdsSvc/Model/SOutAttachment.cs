using System.Linq;
using System.Runtime.Serialization;
using Discussions.DbModel;

namespace TdsSvc.Model
{
    [DataContract]
    public class SOutAttachment
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Format { get; set; }

        [DataMember]
        public string VideoThumbURL { get; set; }

        [DataMember]
        public string VideoEmbedURL { get; set; }

        [DataMember]
        public string VideoLinkURL { get; set; }

        [DataMember]
        public string Link { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public byte[] Thumb { get; set; }

        [DataMember]
        public int OrderNumber { get; set; }

        [DataMember]
        public int? ArgPointId { get; set; }

        [DataMember]
        public int PersonId { get; set; }

        [DataMember]
        public int? DiscussionId { get; set; }

        [DataMember]
        public int? PersonWithAvatarId { get; set; }

        [DataMember]
        public int MediaDataId { get; set; }

        /// <summary>
        /// Only !=null if media data has been requested to be included 
        /// </summary>
        [DataMember]
        public byte[] MediaData { get; set; }

        public SOutAttachment()
        {            
        }

        public SOutAttachment(Attachment attachment, bool includeMediaData)
        {
            Id = attachment.Id;
            Name = attachment.Name;
            Format = attachment.Format;
            VideoThumbURL = attachment.VideoThumbURL;
            VideoEmbedURL = attachment.VideoEmbedURL;
            VideoLinkURL = attachment.VideoLinkURL;
            Link = attachment.Link;
            Title = attachment.Title;
            Thumb = attachment.Thumb;
            OrderNumber = attachment.OrderNumber;
            ArgPointId = attachment.ArgPoint != null ? attachment.ArgPoint.Id : (int?)null;
            PersonId = attachment.Person.Id;
            DiscussionId = attachment.Discussion != null ? attachment.Discussion.Id : (int?)null;
            PersonWithAvatarId = attachment.PersonWithAvatar != null ? attachment.PersonWithAvatar.Id : (int?)null;
            MediaDataId = attachment.MediaData.Id;
            MediaData = includeMediaData ? attachment.MediaData.Data : null;
        }

        public Attachment ToDbEntity(DiscCtx ctx)
        {
            return new Attachment 
            {
                Name = this.Name,
                Format = this.Format,
                VideoThumbURL = this.VideoThumbURL,
                VideoEmbedURL = this.VideoEmbedURL,
                VideoLinkURL = this.VideoLinkURL,
                Link = this.Link,
                Title = this.Title,
                Thumb = this.Thumb,
                OrderNumber = this.OrderNumber,
                ArgPoint = ArgPointId!=null ? ctx.ArgPoint.FirstOrDefault(ap => ap.Id == ArgPointId) : null,
                Person = ctx.Person.FirstOrDefault(p => p.Id == PersonId),
                Discussion = DiscussionId != null ? ctx.Discussion.FirstOrDefault(d => d.Id == DiscussionId) : null,
                PersonWithAvatar = PersonWithAvatarId != null ? ctx.Person.FirstOrDefault(p => p.Id == PersonWithAvatarId) : null,
                MediaData = ctx.MediaDataSet.FirstOrDefault(m=>m.Id==MediaDataId)
            };
        }
    }
}

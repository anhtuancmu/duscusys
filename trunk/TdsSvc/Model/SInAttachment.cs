using System.Linq;
using System.Runtime.Serialization;
using Discussions.DbModel;

namespace TdsSvc.Model
{
    [DataContract]
    public class SInAttachment
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
        public byte[] MediaData { get; set; }

        public SInAttachment()
        {            
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
            };
        }
    }
}

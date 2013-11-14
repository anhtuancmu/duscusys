using Discussions.TdsSvcRef;

namespace Discussions.ViewModel
{
    public class AttachmentViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Format { get; set; }

        public string VideoThumbURL { get; set; }

        public string VideoEmbedURL { get; set; }

        public string VideoLinkURL { get; set; }

        public string Link { get; set; }

        public string Title { get; set; }

        public byte[] Thumb { get; set; }

        public int OrderNumber { get; set; }

        public byte[] MediaData { get; set; }

        public AttachmentViewModel()
        {            
        }

        public AttachmentViewModel(SOutAttachment attachment)
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
            MediaData = attachment.MediaData;
        }
    }
}
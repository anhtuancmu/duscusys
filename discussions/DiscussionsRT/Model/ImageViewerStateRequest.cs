using System.Collections.Generic;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public class ImageViewerStateRequest
    {
        public int OwnerId;
        public int TopicId;
        public int ImageAttachmentId;

        public static ImageViewerStateRequest Read(Dictionary<byte, object> par)
        {
            var res = new ImageViewerStateRequest
                {
                    OwnerId = (int) par[(byte) DiscussionParamKey.ShapeOwnerId],
                    TopicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId],
                    ImageAttachmentId = (int) par[(byte) DiscussionParamKey.AttachmentId]
                };

            return res;
        }
        
        public static Dictionary<byte, object> Write(int ownerId,
                                                     int topicId,
                                                     int imageAttachmentId)
        {                       
            var res = new Dictionary<byte, object>
                {
                    {(byte) DiscussionParamKey.ShapeOwnerId, ownerId},
                    {(byte) DiscussionParamKey.ChangedTopicId, topicId},
                    {(byte) DiscussionParamKey.AttachmentId, imageAttachmentId}
                };

            return res;
        }

        public Dictionary<byte, object> ToDict()
        {
            var res = new Dictionary<byte, object>
                {
                    {(byte) DiscussionParamKey.ShapeOwnerId, this.OwnerId},
                    {(byte) DiscussionParamKey.ChangedTopicId, this.TopicId},
                    {(byte) DiscussionParamKey.AttachmentId, this.ImageAttachmentId}
                };
            return res;
        }
    }
}
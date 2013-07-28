using System.Collections.Generic;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public class ImageViewerMatrix
    {
        public double M11;
        public double M12;
        public double M21;
        public double M22;
        public double OffsetX;
        public double OffsetY;

        public int OwnerId;
        public int TopicId;
        public int ImageAttachmentId;

        public static ImageViewerMatrix Read(Dictionary<byte, object> par)
        {
            var res = new ImageViewerMatrix
                {
                    M11 = (double) par[(byte) DiscussionParamKey.M11Key],
                    M12 = (double) par[(byte) DiscussionParamKey.M12Key],
                    M21 = (double) par[(byte) DiscussionParamKey.M21Key],
                    M22 = (double) par[(byte) DiscussionParamKey.M22Key],
                    OffsetX = (double) par[(byte) DiscussionParamKey.OffsetXKey],
                    OffsetY = (double) par[(byte) DiscussionParamKey.OffsetYKey],

                    OwnerId = (int) par[(byte) DiscussionParamKey.ShapeOwnerId],
                    TopicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId],
                    ImageAttachmentId = (int) par[(byte) DiscussionParamKey.AttachmentId]
                };

            return res;
        }
        
        public static Dictionary<byte, object> Write(System.Windows.Media.Matrix matrix, 
                                                     int ownerId,
                                                     int topicId,
                                                     int imageAttachmentId)
        {
           
            
            var res = new Dictionary<byte, object>
                {
                    {(byte) DiscussionParamKey.M11Key, matrix.M11},
                    {(byte) DiscussionParamKey.M12Key, matrix.M12},
                    {(byte) DiscussionParamKey.M21Key, matrix.M21},
                    {(byte) DiscussionParamKey.M22Key, matrix.M22},
                    {(byte) DiscussionParamKey.OffsetXKey, matrix.OffsetX},
                    {(byte) DiscussionParamKey.OffsetYKey, matrix.OffsetY},
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
                    {(byte) DiscussionParamKey.M11Key, this.M11},
                    {(byte) DiscussionParamKey.M12Key, this.M12},
                    {(byte) DiscussionParamKey.M21Key, this.M21},
                    {(byte) DiscussionParamKey.M22Key, this.M22},
                    {(byte) DiscussionParamKey.OffsetXKey, this.OffsetX},
                    {(byte) DiscussionParamKey.OffsetYKey, this.OffsetY},
                    {(byte) DiscussionParamKey.ShapeOwnerId, this.OwnerId},
                    {(byte) DiscussionParamKey.ChangedTopicId, this.TopicId},
                    {(byte) DiscussionParamKey.AttachmentId, this.ImageAttachmentId}
                };
            return res;
        }
    }
}
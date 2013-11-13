using System.Collections.Generic;
using System.ServiceModel;
using TdsSvc.Model;

namespace TdsSvc
{
    [ServiceContract]
    public interface ITdsService
    {
        #region arg points

        [OperationContract]
        SArgPoint GetArgPoint(int pointId);

        [OperationContract]
        List<SArgPoint> GetArgPointsInTopic(int topicId);

        #endregion


        #region comments
        [OperationContract]
        void AddComment(SComment comment);

        [OperationContract]
        void ChangeCommentText(int commentId, string newText);

        [OperationContract]
        void RemoveComment(int commentId);

        [OperationContract]
        List<SNewCommentsFrom> NumCommentsUnreadBy(int pointId, int callerId);

        [OperationContract]
        List<int> SubsetOfPersonsWithDots(List<int> personIds, int topicId, int callerId);

        [OperationContract]
        List<SComment> GetCommentsInArgPoint(int pointId);
        #endregion


        #region sources

        [OperationContract]
        List<SSource> GetSourcesInArgPoint(int pointId);

        [OperationContract]
        void AddSourceArgPoint(int pointId, string text, int callerId);

        [OperationContract]
        bool MoveSourceUp(int sourceId);

        [OperationContract]
        bool MoveSourceDown(int sourceId);

        [OperationContract]
        List<SOrderInfo> GetSourcesOrder(int pointId);
        #endregion


        #region media 

        [OperationContract]
        void AddAttachmentToPoint(int pointId, SInAttachment attachment, int callerId);

        [OperationContract]
        void AddAttachmentToDiscussion(int discussionId, SInAttachment attachment, int callerId);

        [OperationContract]
        void AddAttachmentToOwnAvatar(SInAttachment attachment, int callerId);

        [OperationContract]
        void RemoveAttachment(int attachmentId);

        [OperationContract]
        bool MoveAttachmentUp(int attachmentId);

        [OperationContract]
        bool MoveAttachmentDown(int attachmentId);

        [OperationContract]
        List<SOrderInfo> GetAttachmentsOrder(int pointId);

        [OperationContract]
        List<SOutAttachment> GetAttachmentsInArgPoint(int pointId, bool includeMediaData);
        #endregion
    }
}

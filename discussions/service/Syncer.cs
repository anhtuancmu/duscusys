using System;
using System.Linq;
using Discussions.TdsSvcRef;
using Discussions.ViewModel;

namespace Discussions.service
{
    public static class Syncer
    {     
        public static ArgPointViewModel[] LoadPointsInTopicLite(TdsSvcRef.TdsServiceClient proxy, int topicId)
        {
            if (proxy == null)
                return new ArgPointViewModel[]{};

            return proxy.GetArgPointsInTopic(topicId).Select(ap => new ArgPointViewModel(ap)).ToArray();
        }

        public static void LoadPointFully(TdsSvcRef.TdsServiceClient proxy, ArgPointViewModel argPoint)
        {
            if(argPoint.Id==0)
                throw new ArgumentException("argPoint.Id==0");

            if (proxy == null || argPoint.ChildrenLoaded)
                return;

            SOutAttachment[] attachments = proxy.GetAttachmentsInArgPoint(argPoint.Id, includeMediaData:true);
            argPoint.Attachments.Clear();
            foreach (var att in attachments)
                argPoint.Attachments.Add(new AttachmentViewModel(att));

            SSource[] sources = proxy.GetSourcesInArgPoint(argPoint.Id);
            argPoint.Sources.Clear();
            foreach (var src in sources)
                argPoint.Sources.Add(new SourceViewModel(src));

            argPoint.ChildrenLoaded = true; 
        }
    }
}
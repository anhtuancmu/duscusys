using System;
using System.Linq;
using Discussions.TdsSvcRef;
using Discussions.util;
using Discussions.ViewModel;

namespace Discussions.service
{
    //public static class SyncExtensions
    //{     
    //    public static ArgPointViewModel[] LoadPointsInTopicLite(this TdsSvcRef.TdsServiceClient proxy, int topicId)
    //    {
    //        if (proxy == null)
    //            return new ArgPointViewModel[]{};

    //        return proxy.GetArgPointsInTopic(topicId, SessionInfo.Get().person.Id)
    //            .Select(ap => new ArgPointViewModel(ap))
    //            .ToArray();
    //    }

    //    public static void LoadPointFully(this TdsSvcRef.TdsServiceClient proxy, ArgPointViewModel argPoint)
    //    {
    //        if(argPoint.Id==0)
    //            throw new ArgumentException("argPoint.Id==0");

    //        if (proxy == null || argPoint.ChildrenLoaded)
    //            return;

    //        LoadSources(proxy, argPoint);

    //        LoadAttachments(proxy, argPoint);

    //        argPoint.ChildrenLoaded = true; 
    //    }

    //    public static void LoadSources(this TdsSvcRef.TdsServiceClient proxy, ArgPointViewModel argPoint)
    //    {
    //        if (proxy == null)
    //            return;

    //        SSource[] svcSources = proxy.GetSourcesInArgPoint(argPoint.Id);

    //        //add
    //        foreach (var svcSrc in svcSources)
    //            if(argPoint.Sources.FirstOrDefault(s=>s.Id==svcSrc.Id)==null)
    //                argPoint.Sources.Add(new SourceViewModel(svcSrc));

    //        //remove 
    //        foreach (var vmSrc in argPoint.Sources)
    //            if (svcSources.FirstOrDefault(s => s.Id == vmSrc.Id) == null)
    //                argPoint.Sources.Remove(vmSrc);
    //    }

    //    public static void LoadAttachments(this TdsSvcRef.TdsServiceClient proxy, ArgPointViewModel argPoint)
    //    {
    //        if (proxy == null)
    //            return;

    //        SOutAttachment[] svcAttachments = proxy.GetAttachmentsInArgPoint(argPoint.Id, includeMediaData: true);  
        
    //        //add
    //        foreach (var att in svcAttachments)
    //            if (argPoint.Attachments.FirstOrDefault(a => a.Id == att.Id) == null)
    //                argPoint.Attachments.Add(new AttachmentViewModel(att));

    //        //remove 
    //        foreach (var vmAtt in argPoint.Attachments)
    //            if (svcAttachments.FirstOrDefault(s => s.Id == vmAtt.Id) == null)
    //                argPoint.Attachments.Remove(vmAtt);
    //    }

    //    public static void UpdateSourcesOrder(this TdsSvcRef.TdsServiceClient proxy, ArgPointViewModel argPoint)
    //    {
    //        if (proxy == null || !argPoint.ChildrenLoaded)
    //            return;

    //        SOrderInfo[] orderInfo = proxy.GetSourcesOrder(argPoint.Id);
    //        foreach (var sOrderInfo in orderInfo)
    //        {
    //            SourceViewModel src = argPoint.Sources.FirstOrDefault(s => s.Id == sOrderInfo.Id);
    //            if (src != null)
    //                src.OrderNumber = sOrderInfo.OrderNumber;
    //        }
            
    //        ObservableCollectionSorter.Sort(argPoint.Sources, (s1,s2) => s1.OrderNumber.CompareTo(s2.OrderNumber));
    //    }

    //    public static void UpdateAttachmentsOrder(this TdsSvcRef.TdsServiceClient proxy, ArgPointViewModel argPoint)
    //    {
    //        if (proxy == null || !argPoint.ChildrenLoaded)
    //            return;

    //        SOrderInfo[] orderInfo = proxy.GetAttachmentsOrder(argPoint.Id);
    //        foreach (var sOrderInfo in orderInfo)
    //        {
    //            AttachmentViewModel attachment = argPoint.Attachments.FirstOrDefault(s => s.Id == sOrderInfo.Id);
    //            if (attachment != null)
    //                attachment.OrderNumber = sOrderInfo.OrderNumber;
    //        }

    //        ObservableCollectionSorter.Sort(argPoint.Attachments, (a1, a2) => a1.OrderNumber.CompareTo(a2.OrderNumber));
    //    }

    //    public static void UpdateLocalReadCounts(this TdsSvcRef.TdsServiceClient proxy, ArgPointViewModel argPoint)
    //    {
    //        if (proxy == null)
    //            return;

    //        SArgPoint sPoint = proxy.GetArgPoint(argPoint.Id, SessionInfo.Get().person.Id);
    //        argPoint.NumUnreadComments = sPoint.NumUnreadComments;
    //    }
    //}
}
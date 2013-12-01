using System.Runtime.Serialization;
using Discussions.DbModel;

namespace TdsSvc.Model
{
    [DataContract]
    public class SArgPoint
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Point { get; set; }

        [DataMember]
        public int SideCode { get; set; }

        [DataMember]
        public bool SharedToPublic { get; set; }

        [DataMember]
        public string RecentlyEnteredSource { get; set; }

        [DataMember]
        public string RecentlyEnteredMediaUrl { get; set; }

        [DataMember]
        public int OrderNumber { get; set; }

        [DataMember]
        public int PersonId { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int NumUnreadComments { get; set; }

        void Init(ArgPoint ap)
        {
            Id = ap.Id;
            Point = ap.Point;
            SideCode = ap.SideCode;
            SharedToPublic = ap.SharedToPublic;
            RecentlyEnteredSource = ap.RecentlyEnteredSource;
            RecentlyEnteredMediaUrl = ap.RecentlyEnteredMediaUrl;
            OrderNumber = ap.OrderNumber;
            PersonId = ap.Person.Id;
            Description = ap.Description.Text;
        }

        public SArgPoint(ArgPoint ap)
        {
            Init(ap);
        }

        public SArgPoint(ArgPoint ap, DiscCtx ctx, int callerId)
        {
            Init(ap);
            NumUnreadComments = DAL.Helper.NumUnreadComments(ctx, ap.Id, callerId);            
        }
    }
}
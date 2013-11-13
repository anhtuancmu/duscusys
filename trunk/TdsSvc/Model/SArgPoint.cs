using System.Runtime.Serialization;

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
    }
}
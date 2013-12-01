using System.Runtime.Serialization;

namespace TdsSvc.Model
{
    /// <summary>
    /// When order of sources, attachments, or other orderable entities changes, clients can request 
    /// a collection of objects of this class instead of a heavier custom collection.
    /// </summary>
    [DataContract]
    public class SOrderInfo
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int OrderNumber { get; set; }

        public SOrderInfo(int id, int orderNumber)
        {
            Id = id;
            OrderNumber = orderNumber;
        }
    }
}
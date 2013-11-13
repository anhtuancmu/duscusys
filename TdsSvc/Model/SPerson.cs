using System.Runtime.Serialization;

namespace TdsSvc.Model
{
    [DataContract]
    public class SPerson
    {
        [DataMember]
        public int Id { get; set; }
    }
}
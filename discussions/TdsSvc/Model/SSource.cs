using System.Runtime.Serialization;
using Discussions.DbModel;

namespace TdsSvc.Model
{
    [DataContract]
    public class SSource
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public int OrderNumber { get; set; }

        public SSource()
        {            
        }

        public SSource(Source s)
        {
            Id = s.Id;
            Text = s.Text;
            OrderNumber = s.OrderNumber;
        }
    }
}
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TdsSvc.Model
{
    [DataContract]
    public class SCommentReadInfo
    {
        [DataMember]
        public SComment Comment { get; set; }

        [DataMember]
        public List<SPerson> PersonsWhoRead { get; set; }

        [DataMember]
        public bool EveryoneInTopicRead { get; set; }
    }
}
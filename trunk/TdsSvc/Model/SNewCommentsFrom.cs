using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TdsSvc.Model
{
    [DataContract]
    public class SNewCommentsFrom
    {
        [DataMember]
        public int NumNewComments { get; set; }

        [DataMember]
        public int PersonId { get; set; }

        [DataMember]
        public string PersonName { get; set; }


        public override string ToString()
        {
            return string.Format("{0}  +{1} comment(s)", PersonName, NumNewComments);
        }
    }

    public static class NewCommentsFromExt
    {
        public static int Total(this IEnumerable<SNewCommentsFrom> bins)
        {
            return bins.Sum(bin => bin.NumNewComments);
        }
    }
}
using System.Runtime.Serialization;
using Discussions.DbModel;

namespace TdsSvc.Model
{
    [DataContract]
    public class SComment
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public int ArgPointId { get; set; }

        [DataMember]
        public int PersonId { get; set; }

        public SComment()
        {            
        }

        public SComment(Comment comment)
        {
            Id = comment.Id;
            Text = comment.Text;
            ArgPointId = comment.ArgPoint.Id;
            PersonId = comment.Person.Id;
        }
    }
}
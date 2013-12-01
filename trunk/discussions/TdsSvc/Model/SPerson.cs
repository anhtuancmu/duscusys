using System.Runtime.Serialization;
using Discussions.DbModel;

namespace TdsSvc.Model
{
    [DataContract]
    public class SPerson
    {
        [DataMember]
        public int Id { get; set; }


        [DataMember]
        public string Name { get; set; }


        [DataMember]
        public string Email { get; set; }


        [DataMember]
        public int Color { get; set; }


        [DataMember]
        public bool Online { get; set; }


        [DataMember]
        public int? SeatId { get; set; }


        [DataMember]
        public int OnlineDevType { get; set; }


        [DataMember]
        public int? SessionId { get; set; }


        [DataMember]
        public int? AvatarAttachmentId { get; set; }

        public SPerson(Person person)
        {
            Id = person.Id;
            Name = person.Name;
            Email = person.Email;
            Color = person.Color;
            Online = person.Online;
            SeatId = person.SeatId;
            OnlineDevType = person.OnlineDevType;
            SessionId = person.SessionId;
            AvatarAttachmentId = person.AvatarAttachment!=null ? person.AvatarAttachment.Id : (int?)null;
        }
    }
}
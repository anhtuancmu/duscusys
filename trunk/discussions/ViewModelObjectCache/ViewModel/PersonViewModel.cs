using Discussions.TdsSvcRef;

namespace Discussions.ViewModel
{
    public class PersonViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public int Color { get; set; }

        public bool Online { get; set; }

        public int? SeatId { get; set; }

        public int OnlineDevType { get; set; }

        public int? SessionId { get; set; }

        public int? AvatarAttachmentId { get; set; }

        public PersonViewModel(SPerson person)
        {
            Id = person.Id;
            Name = person.Name;
            Email = person.Email;
            Color = person.Color;
            Online = person.Online;
            SeatId = person.SeatId;
            OnlineDevType = person.OnlineDevType;
            SessionId = person.SessionId;
            AvatarAttachmentId = person.AvatarAttachmentId;
        }
    }
}
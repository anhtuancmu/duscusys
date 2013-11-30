using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Badass.ViewModel;

namespace Badass.Model
{
    public class Event
    {
        public Event()
        {
            Subcodes = new List<Subcode>();
        }

        public Event(int id, int timestamp, int userId, string code, string note)
        {
            Id = id;
            Timestamp = timestamp;
            UserId = userId;
            Code = code;
            Note = note;

            Subcodes = new List<Subcode>();
            SubcodesIds = new List<string>();
        }

        public void FixupReferences()
        {
            foreach (var subcodeId in SubcodesIds)
            {
                var subcode = WorkingSession.LoadedProject.Subcodes
                  .First(s => s.Name == subcodeId);
                AddSubcode(subcode);
            }
        }

        public int Id { get; set; }

        /// <summary>
        /// Number of seconds elapsed since the start of the timeline
        /// </summary>
        public int Timestamp { get; set; }

        public int UserId { get; set; }

        public string Code { get; set; }

        public string Note { get; set; }

        [XmlIgnore]
        public List<Subcode> Subcodes { get; set; }

        public List<string> SubcodesIds { get; set; }

        public void AddSubcode(Subcode subcode)
        {
            if(Subcodes.FirstOrDefault(sc=>sc.Name==subcode.Name)==null)
                Subcodes.Add(subcode);

            if(!SubcodesIds.Contains(subcode.Name))
                SubcodesIds.Add(subcode.Name);
        }

        public void RemoveSubcode(Subcode subcode)
        {
            Subcodes.Remove(subcode);

            SubcodesIds.Remove(subcode.Name);
        }
    }
}
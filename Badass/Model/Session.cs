using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Badass.ViewModel;

namespace Badass.Model
{
    public class Session
    {
        public Session()
        {
            Events = new List<Event>();
        }

        public Session(string name, string type)
        {
            EventsIds = new List<int>();
            Events = new List<Event>();
            Name = name;
            Type = type; 
        }
        public void FixupReferences()
        {
            foreach (var subcodeId in EventsIds)
            {
                var subcode = WorkingSession.LoadedProject.GetSubcode(subcodeId);
                AddSubcode(subcode);
            }
        }

        #region primary key
        public string Name { get; set; }
        
        public string Type { get; set; }
        #endregion

        public string FirstVideoFile { get; set; }
        public string SecondVideoFile { get; set; }
        public string AudioFile { get; set; }

        public List<int> EventsIds { get; set; }

        [XmlIgnore]
        public List<Event> Events { get; set; }

        public Event GetEvent(int eventId)
        {
            return Events.FirstOrDefault(e => e.Id == eventId);
        }

        public void AddEvent(Event ev)
        {
            if (!EventsIds.Contains(ev.Id))
                EventsIds.Add(ev.Id);

            if(Events.FirstOrDefault(e=>e.Id==ev.Id)==null)
                Events.Add(ev);
        }

        public void RemoveEvent(Event ev)
        {            
            EventsIds.Remove(ev.Id);
            
            Events.Remove(ev);
        }
    }
}
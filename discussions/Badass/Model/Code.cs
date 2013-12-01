using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Badass.ViewModel;

namespace Badass.Model
{
    /// <summary>
    /// This is a type of event, not event instance.
    /// </summary>
    public class Code
    {
        public Code()
        {
            AvailableSubcodes = new List<Subcode>();
        }

        public Code(string name)
        {
            Name = name;

            AvailableSubcodes = new List<Subcode>();
            AvailableSubcodeIds = new List<string>();
        }

        public void FixupReferences()
        {
            foreach (var subcodeId in AvailableSubcodeIds)
            {
                var subcode = WorkingSession.LoadedProject.GetSubcode(subcodeId); 
                AddSubcode(subcode);
            }
        }

        public string Name { get; set; }

        public Hotkey Shortcut { get; set; }

        public List<string> AvailableSubcodeIds { get; set; }

        [XmlIgnore]
        public List<Subcode> AvailableSubcodes { get; set; }

        public void AddSubcode(Subcode subcode)
        {
            if (AvailableSubcodes.FirstOrDefault(sc => sc.Name == subcode.Name) == null)
                AvailableSubcodes.Add(subcode);

            if (!AvailableSubcodeIds.Contains(subcode.Name))
                AvailableSubcodeIds.Add(subcode.Name);
        }

        public void RemoveSubcode(Subcode subcode)
        {
            AvailableSubcodes.Remove(subcode);

            AvailableSubcodeIds.Remove(subcode.Name);
        }
    }
}
namespace Badass.Model
{
    public class Subcode
    {
        public Subcode()
        {            
        }

        public Subcode(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public Hotkey Shortcut { get; set; }  
    }
}
using System.Windows.Input;

namespace Badass.Model
{
    public class Hotkey
    {
        public Hotkey()
        {
            Key = Key.None;
        }

        public bool Shift { get; set; }
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public System.Windows.Input.Key  Key { get; set; }     
    }
}
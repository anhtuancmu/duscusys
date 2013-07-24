using System;
using System.ComponentModel;

namespace DistributedEditor
{
    public class InpModeMgr : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public ShapeInputMode _inpMode = ShapeInputMode.ManipulationExpected;

        public ShapeInputMode Mode
        {
            get { return _inpMode; }
            set
            {
                if (value != _inpMode)
                {
                    _inpMode = value;
                    NotifyPropertyChanged("Mode");
                    NotifyPropertyChanged("ReadableMode");
                }
            }
        }

        public string ReadableMode
        {
            get
            {
                switch (_inpMode)
                {
                    case ShapeInputMode.CreationExpected:
                        return "Draw the new shape";
                    case ShapeInputMode.CursorApprovalExpected:
                        return "Taking cursor...";
                    case ShapeInputMode.LinkedObj1Expected:
                        return "Click first object to link";
                    case ShapeInputMode.LinkedObj2Expected:
                        return "Click second object to link";
                    case ShapeInputMode.Manipulating:
                        return "Manipulating...";
                    case ShapeInputMode.ManipulationExpected:
                        return "Manipulate existing shapes";
                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }
}
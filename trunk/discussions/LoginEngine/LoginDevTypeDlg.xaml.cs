using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AbstractionLayer;
using Discussions.DbModel;
using Discussions.DbModel.model;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for LoginPerson.xaml
    /// </summary>
    public partial class LoginDevTypeDlg : PortableWindow

    {
        public DeviceType SelectedDeviceType;

        public bool BackClicked = false;

        public const string DEV_TYPE_ANDROID = "Android";
        public const string DEV_TYPE_WPF = "WPF";
        public const string DEV_TYPE_STICKY = "Sticky";

        private ObservableCollection<string> _devTypes = null;

        public ObservableCollection<string> DevTypes
        {
            get
            {
                if (_devTypes == null)
                {
                    _devTypes = new ObservableCollection<string>();
                    _devTypes.Add(DEV_TYPE_ANDROID);
                    _devTypes.Add(DEV_TYPE_WPF);
                    _devTypes.Add(DEV_TYPE_STICKY);
                }
                return _devTypes;
            }
            set { _devTypes = value; }
        }

        public LoginDevTypeDlg(Person selectedPerson)
        {
            InitializeComponent();

            DataContext = this;

            SkinManager.ChangeSkin("Blue2Skin.xaml", this.Resources);

            if (selectedPerson != null)
                decorations.SetGreetingName(selectedPerson.Name);
        }

        private void lstBxDevType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((string) e.AddedItems[0])
            {
                case DEV_TYPE_ANDROID:
                    SelectedDeviceType = DeviceType.Android;
                    break;
                case DEV_TYPE_WPF:
                    SelectedDeviceType = DeviceType.Wpf;
                    break;
                case DEV_TYPE_STICKY:
                    SelectedDeviceType = DeviceType.Sticky;
                    break;
                default:
                    throw new NotSupportedException();
            }

            Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            BackClicked = true;
            Close();
        }
    }
}
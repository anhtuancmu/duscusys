using System.ComponentModel;
using System.Runtime.CompilerServices;
using Discussions.view;

namespace Discussions.bots
{
    public sealed class BotsViewModel: INotifyPropertyChanged
    {
        private EnlargeOpenCommentsCloseBot _enlargeOpenCommentsCloseBot;
        private EnlargeOpenAttachmentCloseBot _enlargeOpenAttachmentCloseBot;
        private EnlargeOpenSourceCloseBot _enlargeOpenSourceCloseBot;

        private bool _enlargeOpenCommentsCloseBotEnabled;
        private bool _enlargeOpenAttachmentCloseBotEnabled;
        private bool _enlargeOpenSourceCloseBotEnabled;

        public BotsViewModel(PublicCenter publicCenter)
        {
            _publicCenter = publicCenter;
        }

        private readonly PublicCenter _publicCenter;
     
        public bool EnlargeOpenCommentsCloseBotEnabled
        {
            get { return _enlargeOpenCommentsCloseBotEnabled; }
            set
            {
                if (value == _enlargeOpenCommentsCloseBotEnabled)
                    return;

                if (value && _enlargeOpenCommentsCloseBot==null)
                {
                    _enlargeOpenCommentsCloseBot = new EnlargeOpenCommentsCloseBot(_publicCenter);
                }
                else if (!value && _enlargeOpenCommentsCloseBot != null)
                {
                    _enlargeOpenCommentsCloseBot.Dispose();
                    _enlargeOpenCommentsCloseBot = null;
                }

                _enlargeOpenCommentsCloseBotEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool EnlargeOpenAttachmentCloseBotEnabled
        {
            get { return _enlargeOpenAttachmentCloseBotEnabled; }
            set
            {
                if (value == _enlargeOpenAttachmentCloseBotEnabled)
                    return;

                if (value && _enlargeOpenAttachmentCloseBot == null)
                {
                    _enlargeOpenAttachmentCloseBot = new EnlargeOpenAttachmentCloseBot(_publicCenter);
                }
                else if (!value && _enlargeOpenAttachmentCloseBot != null)
                {
                    _enlargeOpenAttachmentCloseBot.Dispose();
                    _enlargeOpenAttachmentCloseBot = null;
                }

                _enlargeOpenAttachmentCloseBotEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool EnlargeOpenSourceCloseBotEnabled
        {
            get { return _enlargeOpenSourceCloseBotEnabled; }
            set
            {
                if (value == _enlargeOpenSourceCloseBotEnabled)
                    return;

                if (value && _enlargeOpenSourceCloseBot == null)
                {
                    _enlargeOpenSourceCloseBot = new EnlargeOpenSourceCloseBot(_publicCenter);
                }
                else if (!value && _enlargeOpenSourceCloseBot != null)
                {
                    _enlargeOpenSourceCloseBot.Dispose();
                    _enlargeOpenSourceCloseBot = null;
                }

                _enlargeOpenSourceCloseBotEnabled = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

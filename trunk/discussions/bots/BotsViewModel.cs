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
        private DEditorBot _dEditorBot;
        private SuperBot _superBot;

        private bool _enlargeOpenCommentsCloseBotEnabled;
        private bool _enlargeOpenAttachmentCloseBotEnabled;
        private bool _enlargeOpenSourceCloseBotEnabled;
        private bool _superBotEnabled;
        private bool _dEditorBotEnabled;

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

        public bool SuperBotEnabled
        {
            get { return _superBotEnabled; }
            set
            {
                if (value == _superBotEnabled)
                    return;

                if (value && _superBot == null)
                {
                    _superBot = new SuperBot(_publicCenter);
                }
                else if (!value && _superBot != null)
                {
                    _superBot.Dispose();
                    _superBot = null;
                }

                _superBotEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool DEditorBotEnabled
        {
            get { return _dEditorBotEnabled; }
            set
            {
                if (value == _dEditorBotEnabled)
                    return;

                if (value && _dEditorBot == null && _publicCenter.Ctx!=null)
                {
                    _dEditorBot = new DEditorBot(_publicCenter.Ctx.SceneMgr);
                }
                else if (!value && _dEditorBot != null)
                {
                    _dEditorBot.Dispose();
                    _dEditorBot = null;
                }

                _dEditorBotEnabled = value;
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

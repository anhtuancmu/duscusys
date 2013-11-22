using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Discussions.model;
using Reporter.Annotations;

namespace Reporter
{
    public class ExportEventSelectorVM : INotifyPropertyChanged
    {
        private bool _argPointTopicChanged;
        private bool _noBadgeCreated;
        private bool _noBadgeEdited;
        private bool _noBadgeMoved;
        private bool _noBadgeZoomIn;
        private bool _noClusterCreated;
        private bool _noClusterDeleted;
        private bool _noClusterIn;
        private bool _noClusterMoved;
        private bool _noClusterOut;
        private bool _noCommentsAdded;
        private bool _noCommentsRemoved;
        private bool _noFreeDrawingCreated;
        private bool _noFreeDrawingMoved;
        private bool _noFreeDrawingRemoved;
        private bool _noFreeDrawingResize;
        private bool _noImageAdded;
        private bool _noImageOpened;
        private bool _noImageUrlAdded;
        private bool _noLinksCreated;
        private bool _noLinksRemoved;
        private bool _noMediaRemoved;
        private bool _noPdfRemoved;
        private bool _noPdfAdded;
        private bool _noPdfOpened;
        private bool _noPdfUrlAdded;
        private bool _noSourcesAdded;
        private bool _noSourcesOpened;
        private bool _noSourcesRemoved;
        private bool _noVideoOpened;
        private bool _noVideoAdded;
        private bool _noSceneZoomIn;
        private bool _noSceneZoomOut;
        private bool _noScreenshotsAdded;
        private bool _noScreenshotsOpened;

        public ExportEventSelectorVM()
        {
            CheckAllCommand = new RelayCommand<int?>(CheckAll);

            UncheckAllCommand = new RelayCommand<int?>(UncheckAll);
        }

        private void UncheckAll(int? i)
        {
            ArgPointTopicChanged = false;
            NoBadgeCreated = false;
            NoBadgeEdited = false;
            NoBadgeMoved = false;
            NoBadgeZoomIn = false;
            NoClusterCreated = false;
            NoClusterDeleted = false;
            NoClusterIn = false;
            NoClusterMoved = false;
            NoClusterOut = false;
            NoCommentsAdded = false;
            NoCommentsRemoved = false;
            NoFreeDrawingCreated = false;
            NoFreeDrawingMoved = false;
            NoFreeDrawingRemoved = false;
            NoFreeDrawingResize = false;
            NoImageAdded = false;
            NoImageOpened = false;
            NoImageUrlAdded = false;
            NoLinksCreated = false;
            NoLinksRemoved = false;
            NoMediaRemoved = false;
            NoPdfRemoved = false;
            NoPdfAdded = false;
            NoPdfOpened = false;
            NoPdfUrlAdded = false;
            NoSourcesAdded = false;
            NoSourcesOpened = false;
            NoSourcesRemoved = false;
            NoVideoOpened = false;
            NoVideoAdded = false;
            NoSceneZoomIn = false;
            NoSceneZoomOut = false;
            NoScreenshotsAdded = false;
            NoScreenshotsOpened = false;
        }

        private void CheckAll(int? i)
        {
            ArgPointTopicChanged = true;
            NoBadgeCreated = true;
            NoBadgeEdited = true;
            NoBadgeMoved = true;
            NoBadgeZoomIn = true;
            NoClusterCreated = true;
            NoClusterDeleted = true;
            NoClusterIn = true;
            NoClusterMoved = true;
            NoClusterOut = true;
            NoCommentsAdded = true;
            NoCommentsRemoved = true;
            NoFreeDrawingCreated = true;
            NoFreeDrawingMoved = true;
            NoFreeDrawingRemoved = true;
            NoFreeDrawingResize = true;
            NoImageAdded = true;
            NoImageOpened = true;
            NoImageUrlAdded = true;
            NoLinksCreated = true;
            NoLinksRemoved = true;
            NoMediaRemoved = true;
            NoPdfRemoved = true;
            NoPdfAdded = true;
            NoPdfOpened = true;
            NoPdfUrlAdded = true;
            NoSourcesAdded = true;
            NoSourcesOpened = true;
            NoSourcesRemoved = true;
            NoVideoOpened = true;
            NoVideoAdded = true;
            NoSceneZoomIn = true;
            NoSceneZoomOut = true;
            NoScreenshotsAdded = true;
            NoScreenshotsOpened = true;
        }

        public bool ArgPointTopicChanged
        {
            get { return _argPointTopicChanged; }
            set
            {
                if (value.Equals(_argPointTopicChanged)) return;
                _argPointTopicChanged = value;
                OnPropertyChanged();
            }
        }

        public bool NoBadgeCreated
        {
            get { return _noBadgeCreated; }
            set
            {
                if (value.Equals(_noBadgeCreated)) return;
                _noBadgeCreated = value;
                OnPropertyChanged();
            }
        }

        public bool NoBadgeEdited
        {
            get { return _noBadgeEdited; }
            set
            {
                if (value.Equals(_noBadgeEdited)) return;
                _noBadgeEdited = value;
                OnPropertyChanged();
            }
        }

        public bool NoBadgeMoved
        {
            get { return _noBadgeMoved; }
            set
            {
                if (value.Equals(_noBadgeMoved)) return;
                _noBadgeMoved = value;
                OnPropertyChanged();
            }
        }

        public bool NoBadgeZoomIn
        {
            get { return _noBadgeZoomIn; }
            set
            {
                if (value.Equals(_noBadgeZoomIn)) return;
                _noBadgeZoomIn = value;
                OnPropertyChanged();
            }
        }

        public bool NoClusterCreated
        {
            get { return _noClusterCreated; }
            set
            {
                if (value.Equals(_noClusterCreated)) return;
                _noClusterCreated = value;
                OnPropertyChanged();
            }
        }

        public bool NoClusterDeleted
        {
            get { return _noClusterDeleted; }
            set
            {
                if (value.Equals(_noClusterDeleted)) return;
                _noClusterDeleted = value;
                OnPropertyChanged();
            }
        }

        public bool NoClusterIn
        {
            get { return _noClusterIn; }
            set
            {
                if (value.Equals(_noClusterIn)) return;
                _noClusterIn = value;
                OnPropertyChanged();
            }
        }

        public bool NoClusterMoved
        {
            get { return _noClusterMoved; }
            set
            {
                if (value.Equals(_noClusterMoved)) return;
                _noClusterMoved = value;
                OnPropertyChanged();
            }
        }

        public bool NoClusterOut
        {
            get { return _noClusterOut; }
            set
            {
                if (value.Equals(_noClusterOut)) return;
                _noClusterOut = value;
                OnPropertyChanged();
            }
        }

        public bool NoCommentsAdded
        {
            get { return _noCommentsAdded; }
            set
            {
                if (value.Equals(_noCommentsAdded)) return;
                _noCommentsAdded = value;
                OnPropertyChanged();
            }
        }

        public bool NoCommentsRemoved
        {
            get { return _noCommentsRemoved; }
            set
            {
                if (value.Equals(_noCommentsRemoved)) return;
                _noCommentsRemoved = value;
                OnPropertyChanged();
            }
        }

        public bool NoFreeDrawingCreated
        {
            get { return _noFreeDrawingCreated; }
            set
            {
                if (value.Equals(_noFreeDrawingCreated)) return;
                _noFreeDrawingCreated = value;
                OnPropertyChanged();
            }
        }

        public bool NoFreeDrawingMoved
        {
            get { return _noFreeDrawingMoved; }
            set
            {
                if (value.Equals(_noFreeDrawingMoved)) return;
                _noFreeDrawingMoved = value;
                OnPropertyChanged();
            }
        }

        public bool NoFreeDrawingRemoved
        {
            get { return _noFreeDrawingRemoved; }
            set
            {
                if (value.Equals(_noFreeDrawingRemoved)) return;
                _noFreeDrawingRemoved = value;
                OnPropertyChanged();
            }
        }

        public bool NoFreeDrawingResize
        {
            get { return _noFreeDrawingResize; }
            set
            {
                if (value.Equals(_noFreeDrawingResize)) return;
                _noFreeDrawingResize = value;
                OnPropertyChanged();
            }
        }

        public bool NoImageAdded
        {
            get { return _noImageAdded; }
            set
            {
                if (value.Equals(_noImageAdded)) return;
                _noImageAdded = value;
                OnPropertyChanged();
            }
        }

        public bool NoImageOpened
        {
            get { return _noImageOpened; }
            set
            {
                if (value.Equals(_noImageOpened)) return;
                _noImageOpened = value;
                OnPropertyChanged();
            }
        }

        public bool NoImageUrlAdded
        {
            get { return _noImageUrlAdded; }
            set
            {
                if (value.Equals(_noImageUrlAdded)) return;
                _noImageUrlAdded = value;
                OnPropertyChanged();
            }
        }

        public bool NoLinksCreated
        {
            get { return _noLinksCreated; }
            set
            {
                if (value.Equals(_noLinksCreated)) return;
                _noLinksCreated = value;
                OnPropertyChanged();
            }
        }

        public bool NoLinksRemoved
        {
            get { return _noLinksRemoved; }
            set
            {
                if (value.Equals(_noLinksRemoved)) return;
                _noLinksRemoved = value;
                OnPropertyChanged();
            }
        }

        public bool NoMediaRemoved
        {
            get { return _noMediaRemoved; }
            set
            {
                if (value.Equals(_noMediaRemoved)) return;
                _noMediaRemoved = value;
                OnPropertyChanged();
            }
        }

        public bool NoPdfRemoved
        {
            get { return _noPdfRemoved; }
            set
            {
                if (value.Equals(_noPdfRemoved)) return;
                _noPdfRemoved = value;
                OnPropertyChanged();
            }
        }

        public bool NoPdfAdded
        {
            get { return _noPdfAdded; }
            set
            {
                if (value.Equals(_noPdfAdded)) return;
                _noPdfAdded = value;
                OnPropertyChanged();
            }
        }

        public bool NoPdfOpened
        {
            get { return _noPdfOpened; }
            set
            {
                if (value.Equals(_noPdfOpened)) return;
                _noPdfOpened = value;
                OnPropertyChanged();
            }
        }

        public bool NoPdfUrlAdded
        {
            get { return _noPdfUrlAdded; }
            set
            {
                if (value.Equals(_noPdfUrlAdded)) return;
                _noPdfUrlAdded = value;
                OnPropertyChanged();
            }
        }

        public bool NoSourcesAdded
        {
            get { return _noSourcesAdded; }
            set
            {
                if (value.Equals(_noSourcesAdded)) return;
                _noSourcesAdded = value;
                OnPropertyChanged();
            }
        }

        public bool NoSourcesOpened
        {
            get { return _noSourcesOpened; }
            set
            {
                if (value.Equals(_noSourcesOpened)) return;
                _noSourcesOpened = value;
                OnPropertyChanged();
            }
        }

        public bool NoSourcesRemoved
        {
            get { return _noSourcesRemoved; }
            set
            {
                if (value.Equals(_noSourcesRemoved)) return;
                _noSourcesRemoved = value;
                OnPropertyChanged();
            }
        }

        public bool NoVideoOpened
        {
            get { return _noVideoOpened; }
            set
            {
                if (value.Equals(_noVideoOpened)) return;
                _noVideoOpened = value;
                OnPropertyChanged();
            }
        }

        public bool NoVideoAdded
        {
            get { return _noVideoAdded; }
            set
            {
                if (value.Equals(_noVideoAdded)) return;
                _noVideoAdded = value;
                OnPropertyChanged();
            }
        }

        public bool NoSceneZoomIn
        {
            get { return _noSceneZoomIn; }
            set
            {
                if (value.Equals(_noSceneZoomIn)) return;
                _noSceneZoomIn = value;
                OnPropertyChanged();
            }
        }

        public bool NoSceneZoomOut
        {
            get { return _noSceneZoomOut; }
            set
            {
                if (value.Equals(_noSceneZoomOut)) return;
                _noSceneZoomOut = value;
                OnPropertyChanged();
            }
        }

        public bool NoScreenshotsAdded
        {
            get { return _noScreenshotsAdded; }
            set
            {
                if (value.Equals(_noScreenshotsAdded)) return;
                _noScreenshotsAdded = value;
                OnPropertyChanged();
            }
        }

        public bool NoScreenshotsOpened
        {
            get { return _noScreenshotsOpened; }
            set
            {
                if (value.Equals(_noScreenshotsOpened)) return;
                _noScreenshotsOpened = value;
                OnPropertyChanged();
            }
        }

        public bool EventExported(StEvent eventCode)
        {
            switch (eventCode)
            {
                case StEvent.RecordingStarted:
                    return true;

                case StEvent.RecordingStopped:
                    return true;

                case StEvent.BadgeCreated:
                    return NoBadgeCreated;

                case StEvent.BadgeEdited:
                    return NoBadgeEdited;

                case StEvent.BadgeMoved:
                    return NoBadgeMoved;

                case StEvent.BadgeZoomIn:
                    return NoBadgeZoomIn;

                case StEvent.ClusterCreated:
                    return NoClusterCreated;

                case StEvent.ClusterDeleted:
                    return NoClusterDeleted;

                case StEvent.ClusterIn:
                    return NoClusterIn;

                case StEvent.ClusterOut:
                    return NoClusterOut;

                case StEvent.ClusterMoved:
                    return NoClusterMoved;

                case StEvent.LinkCreated:
                    return NoLinksCreated;

                case StEvent.LinkRemoved:
                    return NoLinksRemoved;

                case StEvent.FreeDrawingCreated:
                    return NoFreeDrawingCreated;

                case StEvent.FreeDrawingRemoved:
                    return NoFreeDrawingRemoved;

                case StEvent.FreeDrawingResize:
                    return NoFreeDrawingResize;

                case StEvent.FreeDrawingMoved:
                    return NoFreeDrawingMoved;

                case StEvent.SceneZoomedIn:
                    return NoSceneZoomIn;

                case StEvent.SceneZoomedOut:
                    return NoSceneZoomOut;

                case StEvent.ArgPointTopicChanged:
                    return ArgPointTopicChanged;

                case StEvent.SourceAdded:
                    return NoSourcesAdded;

                case StEvent.SourceRemoved:
                    return NoSourcesRemoved;

                case StEvent.ImageAdded:
                    return NoImageAdded;

                case StEvent.ImageUrlAdded:
                    return NoImageUrlAdded;

                case StEvent.PdfAdded:
                    return NoPdfAdded;

                case StEvent.PdfUrlAdded:
                    return NoPdfUrlAdded;

                case StEvent.YoutubeAdded:
                    return NoVideoAdded;

                case StEvent.ScreenshotAdded:
                    return NoScreenshotsAdded;

                case StEvent.MediaRemoved:
                    return NoMediaRemoved;

                case StEvent.CommentAdded:
                    return NoCommentsAdded;

                case StEvent.CommentRemoved:
                    return NoCommentsRemoved;

                case StEvent.ImageOpened:
                    return NoImageOpened;

                case StEvent.VideoOpened:
                    return NoVideoOpened;

                case StEvent.ScreenshotOpened:
                    return NoScreenshotsOpened;

                case StEvent.PdfOpened:
                    return NoPdfOpened;

                case StEvent.SourceOpened:
                    return NoSourcesOpened;

                case StEvent.LocalIgnorableEvent:
                    return true;

                default:
                    throw new ArgumentOutOfRangeException("eventCode");
            }
        }

        public RelayCommand<int?> CheckAllCommand
        {
            get;
            set;
        }

        public RelayCommand<int?> UncheckAllCommand
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
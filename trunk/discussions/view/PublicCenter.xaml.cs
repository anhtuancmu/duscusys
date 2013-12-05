using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Input.Manipulations;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AbstractionLayer;
using Discussions.ctx;
using Discussions.DbModel;
using Discussions.DbModel.model;
using Discussions.model;
using Discussions.rt;
using Discussions.RTModel.Model;
using DistributedEditor;
using LoginEngine;
using Microsoft.Surface.Presentation.Controls.TouchVisualizations;
using Size = System.Windows.Size;

namespace Discussions.view
{
    public partial class PublicCenter : PortableWindow 
    {
        private Discussion _discussion;

        public Discussion discussion
        {
            get { return _discussion; }
            set { _discussion = value; }
        }

        public Topic CurrentTopic
        {
            get { return topicNavPanel.selectedTopic; }
        }

        //only used for final screenshot 
        private int _topicId = -1;
        private int _discussionId = -1;

        private const double MIN_ZOOM = 0.5;
        private const double MAX_ZOOM = 5;

        private double _zoomFactor = 1.0;

        public double ZoomFactor
        {
            get { return _zoomFactor; }
            set
            {
                if (value != _zoomFactor)
                {
                    var zoomIn = value > _zoomFactor;
                    var zoomOut = value < _zoomFactor;

                    _zoomFactor = value;

                    zoomBySlider(_zoomFactor, zoomIn, zoomOut);
                }
            }
        }


        private UISharedRTClient _sharedClient;

        private ZoomSeriesAnalyser _zoomSeries;

        private EditorWndCtx editCtx;
        public EditorWndCtx Ctx
        {
            get { return editCtx; }
        }

        private LaserPointerWndCtx _laserPointerWndCtx;

        private bool _shapesVisibile = false;

        private Main.OnDiscFrmClosing _closing;

        private DispatcherTimer stopWatchTimer = null;

        public PublicCenter(UISharedRTClient sharedClient,
                            Main.OnDiscFrmClosing closing,
                            int screenshotTopicId, int screenshotDiscussionId)
        {
            this._discussion = SessionInfo.Get().discussion;
            _sharedClient = sharedClient;
            _closing = closing;
            _topicId = screenshotTopicId;
            _discussionId = screenshotDiscussionId;

            InitializeComponent();

            if (CurrentTopic != null)
                ExplanationModeMediator.Inst.CurrentTopicId = CurrentTopic.Id;
            else
                ExplanationModeMediator.Inst.CurrentTopicId = null;

            topicNavPanel.topicChanged += topicSelectionChanged;
            topicNavPanel.discussion = SessionInfo.Get().discussion;
            topicNavPanel.topicAnimate += TopicAnimate;

            btnExplanationMode.DataContext = ExplanationModeMediator.Inst;
            btnLaserPointer.DataContext = ExplanationModeMediator.Inst;

            localAccount.DataContext = SessionInfo.Get().person;

            avaBar.Init(_sharedClient);
            avaBar.paletteOwnerChanged += PaletteOwnerChanged;

            SetListeners(_sharedClient, true);

            _zoomSeries = new ZoomSeriesAnalyser(OnSeriesEnd);

            TouchVisualizer.SetShowsVisualizations(laserScene, false);

            if (SessionInfo.Get().person.Name.StartsWith(DaoUtils.MODER_SUBNAME))
            {
                startStopWatch();
            }

            //  scene.Height = 0.6 * System.Windows.SystemParameters.PrimaryScreenHeight;
            //  inkCanv.Height = scene.Height;

            //var scaleTr = Matrix.Identity;
            //scaleTr.Translate(0, -scene.Height / 2);
            //scaleTr.ScaleAt(2,2, 
            //                0.5, 
            //                0.5);
            //SetWorkingAreaTransform(scaleTr, false, false, false, false);  
        }

        void Inst_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LasersEnabled")
            {
                ToggleLaserPointer();
            
                if (ExplanationModeMediator.Inst.LasersEnabled)
                {
                    ResetZoomAndPanning();
                    blockWorkingAreaTransforms = true;
                }
                else
                {
                    blockWorkingAreaTransforms = false;
                } 
            }
        }

        void ResetZoomAndPanning()
        {
            SetWorkingAreaTransform(Matrix.Identity, false, false, false, false);
            
            //reset slider            
            zoomSlider.Value = 1.0;
        }

        private void frmLoaded(object sender, RoutedEventArgs args)
        {
        }

        private void OnSeriesEnd(ZoomDirection direction)
        {
            var ev = direction == ZoomDirection.In ? StEvent.SceneZoomedIn : StEvent.SceneZoomedOut;
            UISharedRTClient.Instance.clienRt.SendStatsEvent(ev,
                                                             SessionInfo.Get().person.Id,
                                                             SessionInfo.Get().discussion.Id,
                                                             topicNavPanel.selectedTopic.Id,
                                                             DeviceType.Wpf);
        }

        private void topicSelectionChanged(SelectionChangedEventArgs e)
        {
            if (_laserPointerWndCtx != null)
                _laserPointerWndCtx.Dispose();             
            _laserPointerWndCtx = new LaserPointerWndCtx(laserScene, 
                                                         CurrentTopic.Id, 
                                                         LaserPointerTargetSurface.PublicBoard);
            ExplanationModeMediator.Inst.LasersEnabled = false;

            if (CurrentTopic != null)
                ExplanationModeMediator.Inst.CurrentTopicId = CurrentTopic.Id;
            else
                ExplanationModeMediator.Inst.CurrentTopicId = null;

            CleanupEditCtx();

            if (topicNavPanel.selectedTopic == null)
                return;

            CreateEditCtx();

            //to update text 
            onStopWatch(null, null);
        }

        private void btnViewResults_Click(object sender, RoutedEventArgs e)
        {
            PublicBoardCtx.SaveChangesIgnoreConflicts();
            new ResultViewer(discussion, null).Show();
            Close();
        }

        //private void Window_ManipulationStarting(object sender, Manipulation2DStartedEventArgs e)
        //{
        //    //e.Handled = true;
        //    //e.ManipulationContainer = this;            
        //}

        private void Window_ManipulationDelta(object sender, Manipulation2DDeltaEventArgs e)
        {
            //e.Handled = true;
            unsolved_ManipulationDelta(sender, e);
        }

        private void unsolved_ManipulationDelta(object sender, Manipulation2DDeltaEventArgs e)
        {
            Canvas element = scene;

            var matrix = GetUnsolvedTransform();

            //if (e.Delta.ScaleX < 1)
            //    if (matrix.M11 <= 1.0 || matrix.M22 <= 1.0)
            //        return;

            //matrix.ScaleAt(e.DeltaManipulation.Scale.X,
            //               e.DeltaManipulation.Scale.Y,
            //               e.ManipulationOrigin.X - this.ActualWidth / 2,
            //               e.ManipulationOrigin.Y - this.ActualHeight / 2);

            var xScale = (e.Delta.ScaleX + 2)/3;

            var finalFact = matrix.M11*xScale;
            if (finalFact > MAX_ZOOM || finalFact < MIN_ZOOM)
                return;

            matrix.ScaleAt(xScale,
                           xScale,
                           e.OriginX - this.ActualWidth/2,
                           e.OriginY - this.ActualHeight/2);

            //matrix.RotateAt(e.DeltaManipulation.Rotation,
            //                e.ManipulationOrigin.X,
            //                e.ManipulationOrigin.Y);

            matrix.Translate(e.Delta.TranslationX,
                             e.Delta.TranslationY);

            updateZoomFactor(matrix.M11);

            SetWorkingAreaTransform(matrix, e.Delta.TranslationX < 0,
                                    e.Delta.TranslationY < 0,
                                    e.Delta.ScaleX > 1,
                                    e.Delta.ScaleX < 1);
        }

        private void SetWorkingAreaTransform(Matrix m, bool left, bool top, bool zoomIn, bool zoomOut)
        {
            if (blockWorkingAreaTransforms)
                return;

            var mt = new MatrixTransform(m);

            scene.RenderTransform = mt;
            inkCanv.RenderTransform = mt;

            if (zoomIn || zoomOut)
                _zoomSeries.SubmitStep(zoomIn ? ZoomDirection.In : ZoomDirection.Out);
        }

        private void unsolved_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            //e.Handled = true;
        }

        private Matrix GetUnsolvedTransform()
        {
            var transformation = scene.RenderTransform
                                 as MatrixTransform;
            var matrix = transformation == null
                             ? Matrix.Identity
                             : transformation.Matrix;
            return matrix;
        }

        private void unsolved_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Canvas element = scene;

            var matrix = GetUnsolvedTransform();

            var mousePos = e.GetPosition(this);
            double factor = e.Delta > 0 ? 1.1 : 0.9;

            var finalFact = matrix.M11*factor;
            if (finalFact > MAX_ZOOM || finalFact < MIN_ZOOM)
                return;

            matrix.ScaleAt(factor,
                           factor,
                           mousePos.X - System.Windows.SystemParameters.PrimaryScreenWidth/2,
                           mousePos.Y - 0.5*System.Windows.SystemParameters.PrimaryScreenHeight);

            updateZoomFactor(matrix.M11);

            SetWorkingAreaTransform(matrix, false, false, factor > 1, factor < 1);
        }

        private void updateZoomFactor(double val)
        {
            _zoomFactor = val;
            zoomSlider.Value = val;
        }

        private void zoomBySlider(double finalFactor, bool zoomIn, bool zoomOut)
        {
            Canvas element = scene;

            var matrix = GetUnsolvedTransform();

            var stepFactor = finalFactor/matrix.M11;
            matrix.ScaleAt(stepFactor,
                           stepFactor,
                           0,
                           0);

            SetWorkingAreaTransform(matrix, false, false, zoomIn, zoomOut);
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            unsolved_MouseWheel(sender, e);
        }

        private double PrevX, PrevY;

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePos = e.GetPosition(this);

            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) !=
                System.Windows.Forms.Keys.None)
            {
                Canvas element = scene;

                var matrix = GetUnsolvedTransform();

                if (PrevX > 0 && PrevY > 0)
                {
                    matrix.Translate(mousePos.X - PrevX, mousePos.Y - PrevY);
                }

                SetWorkingAreaTransform(matrix, mousePos.X - PrevX < 0, mousePos.Y - PrevY < 0, false, false);
            }

            PrevX = mousePos.X;
            PrevY = mousePos.Y;
        }

        private void SetListeners(UISharedRTClient sharedClient, bool doSet)
        {
            var clienRt = sharedClient.clienRt;
            if (clienRt == null)
                return;


            //explanation mode functions
            if (doSet)
            {
                clienRt.onBadgeViewRequest += __badgeViewEvent;
                clienRt.onSourceViewRequest += __sourceView;
                WebkitBrowserWindow.userRequestedClosing += onLocalSourceViewerClosed;
                ExplanationModeMediator.Inst.CloseReq += onImgViewerClosed;
                ExplanationModeMediator.Inst.OpenReq += onImgViewerOpened;

                ExplanationModeMediator.Inst.PropertyChanged += Inst_PropertyChanged;
            }
            else
            {
                clienRt.onBadgeViewRequest -= __badgeViewEvent;
                clienRt.onSourceViewRequest -= __sourceView;
                WebkitBrowserWindow.userRequestedClosing -= onLocalSourceViewerClosed;
                ExplanationModeMediator.Inst.CloseReq -= onImgViewerClosed;
                ExplanationModeMediator.Inst.OpenReq -= onImgViewerOpened;

                ExplanationModeMediator.Inst.PropertyChanged -= Inst_PropertyChanged;
            }
        }

        private void ForgetDBDiscussionState()
        {
            //forget cached state
            PublicBoardCtx.DropContext();
            _discussion = SessionInfo.Get().discussion;
            DataContext = this;
        }

        private void CleanupEditCtx()
        {
            if (editCtx != null)
            {
                editCtx.ZoomManipulator.Delta -= Window_ManipulationDelta;

                editCtx.CleanupScene();
                editCtx = null;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetListeners(_sharedClient, false);
            avaBar.Deinit();
            CleanupEditCtx();
            cleanupStopWatch();

            if (_laserPointerWndCtx != null)
                _laserPointerWndCtx.Dispose();

            if (_closing != null)
                _closing();
        }

        private void btnGoPrivate_Click(object sender, RoutedEventArgs e)
        {
            var wnd = DiscWindows.Get();
            if (wnd.privateDiscBoard != null)
            {
                wnd.privateDiscBoard.Activate();
                return;
            }

            wnd.privateDiscBoard = new PrivateCenter3(_sharedClient, () => { wnd.privateDiscBoard = null; });
            wnd.privateDiscBoard.Show();

            // Close();
        }

        private void btnToggleShapes_Click(object sender, RoutedEventArgs e)
        {
            if (_shapesVisibile)
                ToggleShapes(false);
            else
                ToggleShapes(true);
        }

        private void ToggleShapes(bool shVisible)
        {
            if (editCtx == null)
                CreateEditCtx();

            if (shVisible)
            {
                editCtx.ShapesVisility(true);
            }
            else
            {
                editCtx.ShapesVisility(false);
            }
            //modeInfoTip.Visibility = shVisible ? Visibility.Visible : Visibility.Hidden;
            this._shapesVisibile = shVisible; //save to preserve selected option between topics 
        }

        private void CreateEditCtx()
        {
            CleanupEditCtx();

            avaBar.SelectCurrentUser();

            editCtx = new EditorWndCtx(scene,
                                       inkCanv,
                                       palette,
                                       inkPalette,
                                       this, //surface window for focus fix                                      
                                       _topicId != -1 ? _topicId : CurrentTopic.Id,
                                       _discussionId != -1 ? _discussionId : CurrentTopic.Discussion.Id,
                                       _shapesVisibile);

            editCtx.ZoomManipulator.Delta += Window_ManipulationDelta;

            DataContext = this;
            _sharedClient.clienRt.SendInitialSceneLoadRequest(_topicId != -1 ? _topicId : CurrentTopic.Id);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                btnToggleShapes.IsChecked = !btnToggleShapes.IsChecked;
                btnToggleShapes_Click(null, null);
            }
            else if (e.Key == Key.Delete && _shapesVisibile)
            {
                editCtx.RemoveShape(palette.GetOwnerId());
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.C)
                {
                    if (editCtx != null)
                        editCtx.CopySelected();
                }
                else if (e.Key == Key.V)
                {
                    if (editCtx != null)
                        editCtx.PasteSelected();
                    Keyboard.Focus(this);
                }
            }
            else if (e.Key == Key.Left)
            {
                topicNavPanel.SelectTopic(false);
            }
            else if (e.Key == Key.Right)
            {
                topicNavPanel.SelectTopic(true);
            }
            else if (e.Key == Key.Enter)
            {
                //if (graphicsCtx != null)
                //    graphicsCtx.FinishFreeDrawing();
            }
        }

        private void ensureOwnerSelected()
        {
            if (avaBar.lstBxPlayers.SelectedIndex == -1)
                avaBar.lstBxPlayers.SelectedIndex = 0;
        }

        private void btnDiscInfo_Click(object sender, RoutedEventArgs e)
        {
            if (discussion == null)
                return;

            var diz = new DiscussionInfoZoom(discussion);
            diz.ShowDialog();
        }

        private void btnToMainMenuClick(object sender, RoutedEventArgs e)
        {
            if (DiscWindows.Get().mainWnd != null)
                DiscWindows.Get().mainWnd.Activate();

            ///Close();
        }

        #region topic listbox 

        private void TopicAnimate(bool hide)
        {
            if (hide)
            {
                var s = (Storyboard) FindResource("HideTopicsStoryboard");
                s.Begin();
            }
            else
            {
                var s = (Storyboard) FindResource("ShowTopicsStoryboard");
                s.Begin();
            }
        }

        #endregion topic listbox

        private void PaletteOwnerChanged(int owner)
        {
            if (owner != -1)
            {
                if(editCtx!=null)
                    editCtx.SceneMgr.FinishFreeDrawing();

                palette._ownerId = owner;
                palette.bdr.BorderBrush = new SolidColorBrush(DaoUtils.UserIdToColor(owner));
                inkPalette.bdr.BorderBrush = new SolidColorBrush(DaoUtils.UserIdToColor(owner));
            }
        }

        private void btnHome_Click_1(object sender, RoutedEventArgs e)
        {
            DiscWindows.Get().mainWnd.Activate();
            //this.Close();
        }

        private DateTime _recentStopWatchTick;
        private bool _firstTick = true;

        private void onStopWatch(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            if (_firstTick)
            {
                _firstTick = false;
                updateStopWatch(CurrentTopic, TimeSpan.FromSeconds(1)); //assume timer ticks in 1 second first time
            }
            else
            {
                updateStopWatch(CurrentTopic, now.Subtract(_recentStopWatchTick));
            }
            _recentStopWatchTick = now;
        }

        private void updateStopWatch(Topic displayedTopic, TimeSpan passedSinceLastUpdate)
        {
            var discId = SessionInfo.Get().discussion.Id;
            var freshDisc = PublicBoardCtx.Get().Discussion.FirstOrDefault(d0 => d0.Id == discId);
            bool needSave = false;
            foreach (var topic in freshDisc.Topic)
            {
                if (!topic.Running)
                {
                    if (topic.Id == displayedTopic.Id)
                        stopWatch.Text = TimeSpan.FromSeconds((double) topic.CumulativeDuration).ToString();
                    continue;
                }

                needSave = true;
                topic.CumulativeDuration += passedSinceLastUpdate.Seconds;

                if (topic.Id == displayedTopic.Id)
                    stopWatch.Text = TimeSpan.FromSeconds((double) topic.CumulativeDuration).ToString();
            }
            if (needSave)
                PublicBoardCtx.Get().SaveChanges();
        }

        private void startStopWatch()
        {
            if (stopWatchTimer != null)
                return;

            stopWatchTimer = new DispatcherTimer();
            stopWatchTimer.Interval = TimeSpan.FromSeconds(1);
            stopWatchTimer.Tick += onStopWatch;
            stopWatchTimer.Start();
        }

        private void cleanupStopWatch()
        {
            if (stopWatchTimer == null)
                return;

            stopWatchTimer.Stop();
            stopWatchTimer.Tick -= onStopWatch;
            stopWatchTimer = null;
            _firstTick = true;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {          
            CreateEditCtx();
        }

        #region large badge view management

        private LargeBadgeView _lbv = null;
        private bool blockWorkingAreaTransforms = false;

        private bool IsExplanationModeEnabled()
        {
            return ExplanationModeMediator.Inst.ExplanationModeEnabled;           
        }

        private bool IsEditingCommentInLargeBadgeView()
        {
            return (_lbv != null && _lbv.IsEditingComment);
        }

        private void __badgeViewEvent(BadgeViewMessage bv)
        {
            if (!IsExplanationModeEnabled())
                return;

            if (IsEditingCommentInLargeBadgeView())
            {
                _lbv.MissedCloseRequest = true;
                return;
            }

            if (bv.doExpand)
            {
                ShowLargeBadgeView(PublicBoardCtx.Get().ArgPoint.FirstOrDefault(ap0 => ap0.Id == bv.argPointId));
            }
            else
            {
                HideLargeBadgeView();
            }
        }

        private void LargeRequest(object sender, RoutedEventArgs e)
        {
            var badge = e.OriginalSource as Badge4;
            if (badge == null)
                return;

            //block the badge movement
            if (editCtx != null)
                editCtx.SceneMgr.CancelManipulation();
            ShowLargeBadgeView(badge.DataContext as ArgPoint);

            UISharedRTClient.Instance.clienRt.SendStatsEvent(StEvent.BadgeZoomIn,
                                                             SessionInfo.Get().person.Id,
                                                             SessionInfo.Get().discussion.Id,
                                                             topicNavPanel.selectedTopic.Id,
                                                             DeviceType.Wpf);
            UISharedRTClient.Instance.clienRt.SendBadgeViewRequest((badge.DataContext as ArgPoint).Id, true);
        }

        private void ShrinkRequest(object sender, RoutedEventArgs e)
        {
            HideLargeBadgeView();
            UISharedRTClient.Instance.clienRt.SendBadgeViewRequest(-1, false);
        }

        private void ShowLargeBadgeView(ArgPoint ap)
        {
            if (_lbv != null)
                return;

            scene.IsHitTestVisible = false;
            blockWorkingAreaTransforms = true;

            _lbv = new LargeBadgeView();
            var ArgPointId = ap.Id;
            DbCtx.DropContext();//it can become stale while modal view was closed.
            _lbv.DataContext = DbCtx.Get().ArgPoint.FirstOrDefault(p0 => p0.Id == ArgPointId);
            _lbv.SetRt(UISharedRTClient.Instance);

            //mainGrid.Children.Add(_lbv);
            int indexOfLaserScene = mainGrid.Children.IndexOf(laserScene);
            mainGrid.Children.Insert(indexOfLaserScene, _lbv);

            ResizeLargeBadgeView();

            _lbv.HorizontalAlignment = HorizontalAlignment.Center;
            _lbv.VerticalAlignment = VerticalAlignment.Center;
        }

        void ResizeLargeBadgeView()
        {
            if (_lbv == null)
                return;

             Size sz = GetLargeBadgeViewSize();
            _lbv.Height = sz.Height;
            _lbv.Width = sz.Width;
        }

        System.Windows.Size GetLargeBadgeViewSize()
        {
            double height = mainGrid.ActualHeight - 200;
            if (height < 100)
                height = 100;
            if (height > 700)
                height = 700;

            double width = mainGrid.ActualWidth - 200;
            if (width < 200)
                width = 200;
            if (width > 850)
                width = 850;

            return new System.Windows.Size(width, height);
        }

        private void PublicCenter_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeLargeBadgeView();
        }

        private void HideLargeBadgeView()
        {
            if (_lbv == null)
                return;

            scene.IsHitTestVisible = true;
            blockWorkingAreaTransforms = false;

            mainGrid.Children.Remove(_lbv);
            _lbv = null;
        }

        #endregion

        #region explanation mode browser

        //some source already opened locally, notify all other clients    
        private void sourceView(object sender, RoutedEventArgs e)
        {
            var sourceUC = e.OriginalSource as FrameworkElement;
            if (sourceUC == null)
                return;

            var src = sourceUC.DataContext as Source;
            var att = sourceUC.DataContext as Attachment;

            if (src != null)
                UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(SyncMsgType.SourceView, src.Id, true);
            else if (att != null)
                UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(SyncMsgType.YoutubeView, att.Id, true);
        }

        //local source viewer is about to close 
        private void onLocalSourceViewerClosed()
        {
            UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(SyncMsgType.SourceView, -1, false);
        }

        private void onImgViewerClosed(int attachId)
        {
            UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(SyncMsgType.ImageView, attachId, false);
        }

        private void onImgViewerOpened(int attachId)
        {
            UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(SyncMsgType.ImageView, attachId, true);
        }

        //message from remote client
        private void __sourceView(ExplanationModeSyncMsg sm)
        {
            if (!ExplanationModeMediator.Inst.ExplanationModeEnabled)
                return;

            if (sm.doExpand)
            {
                switch (sm.syncMsgType)
                {
                    case SyncMsgType.SourceView:
                        var src = PublicBoardCtx.Get().Source.FirstOrDefault(s0 => s0.Id == sm.viewObjectId);
                        var browser = new WebkitBrowserWindow(src.Text, CurrentTopic != null ? CurrentTopic.Id : (int?)null);
                        browser.Show();
                        break;
                    case SyncMsgType.YoutubeView:
                        var attach = PublicBoardCtx.Get().Attachment.FirstOrDefault(a0 => a0.Id == sm.viewObjectId);
                        var embedUrl = AttachmentToVideoConvertor.AttachToYtInfo(attach).EmbedUrl;
                        browser = new WebkitBrowserWindow(embedUrl, CurrentTopic != null ? CurrentTopic.Id : (int?)null);
                        browser.Show();
                        break;
                    case SyncMsgType.ImageView:
                        attach = PublicBoardCtx.Get().Attachment.FirstOrDefault(a0 => a0.Id == sm.viewObjectId);
                        if (attach != null && !ExplanationModeMediator.Inst.IsViewerOpened(attach.Id))
                            AttachmentManager.RunViewer(attach);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                switch (sm.syncMsgType)
                {
                    case SyncMsgType.SourceView:
                        WebkitBrowserWindow.EnsureInstanceClosed();
                        break;
                    case SyncMsgType.ImageView:
                        ExplanationModeMediator.Inst.EnsureInstanceClosed(sm.viewObjectId);
                        break;
                    case SyncMsgType.YoutubeView:
                        WebkitBrowserWindow.EnsureInstanceClosed();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        #endregion explanation mode browser

        #region screenshot of final scene

        private TaskCompletionSource<ScreenshoReports> finalSceneTcs = null;


        public struct ScreenshoReports
        {
            public Dictionary<int, string> ShapeIdsToShapes;
            public Dictionary<int, string> ArgPointIdsToBadges;
        }

        public Task<ScreenshoReports> FinalSceneScreenshots()
        {
            finalSceneTcs = new TaskCompletionSource<ScreenshoReports>();
            _sharedClient.clienRt.loadingDoneEvent += shapeLoadingDone;

            ToggleShapes(true);

            return finalSceneTcs.Task;
        }

        public static Rect TransformRectByDpi(Rect rect, int dpi)
        {
            return new Rect(rect.X*dpi/96,
                            rect.Y*dpi/96,
                            rect.Width*dpi/96,
                            rect.Height*dpi/96);
        }

        private async void shapeLoadingDone()
        {
            _sharedClient.clienRt.loadingDoneEvent -= shapeLoadingDone;

            //even though all d-editor objects are loaded, their visuals on canvas are created asynchronously
            await Utils.Delay(3000);

            //add main screen
            var dpi = 200;
            int maxWidth  = (int)this.ActualWidth;
            int maxHeight = (int)this.ActualHeight;
            var shapes = editCtx.getMgr().Doc.GetShapes();
            foreach (var subpartSh in shapes)
            {
                try
                {
                    var bounds = subpartSh.ReportingBoundsProvider();
                    if (bounds.X + bounds.Width > maxWidth)
                        maxWidth = (int) (bounds.X + bounds.Width);
                    if (bounds.Y + bounds.Height > maxHeight)
                        maxHeight = (int) (bounds.Y + bounds.Height);
                }
                catch
                {
                    //some shapes don't support bounds provider
                }
            }
            const int margin = 20;
            scene.Background = null;
            scene.InvalidateVisual();
            await Utils.Delay(50);

            var screen = Screenshot.Take(scene,
                                        new System.Windows.Size(maxWidth + margin, maxHeight + margin),
                                        dpi);
            var linksAndClusters = new Dictionary<int, string> {{-1, screen}};

            //add links and clusters
            var bmp = new Bitmap(screen);
            var subparts = editCtx.getMgr().Doc.GetShapes().Where(sh => sh.ShapeCode() == VdShapeType.Cluster ||
                                                                        sh.ShapeCode() == VdShapeType.ClusterLink);
            foreach (var subpartSh in subparts)
            {
                var bounds = subpartSh.ReportingBoundsProvider();
                var subpartScreen = Screenshot.TakeSubImage(bmp,
                                                            TransformRectByDpi(bounds, dpi));
                linksAndClusters.Add(subpartSh.Id(), subpartScreen);
            }

            //badges
            var badgeScreenshots = new Dictionary<int, string>();
            var badges = editCtx.getMgr().Doc.GetShapes().Where(sh => sh.ShapeCode() == VdShapeType.Badge);
            foreach (var badge in badges)
            {
                var bounds = badge.ReportingBoundsProvider();
                var subpartScreen = Screenshot.TakeSubImage(bmp,
                                                            TransformRectByDpi(bounds, dpi));
                badgeScreenshots.Add(((VdBadge)badge).ArgPtId, subpartScreen);
            }

            finalSceneTcs.SetResult(new ScreenshoReports
                {
                    ArgPointIdsToBadges = badgeScreenshots, 
                    ShapeIdsToShapes = linksAndClusters
                });
            finalSceneTcs = null;
        }

        #endregion

        private void btnResetZoomAndPanning_Click_1(object sender, RoutedEventArgs e)
        {
            ResetZoomAndPanning();
        }

        private void btnReport_Click_1(object sender, RoutedEventArgs e)
        {
            if (CurrentTopic == null)
                return;
            HtmlReportBrowsing.startHtml5TopicReport(CurrentTopic.Id);
        }

        #region laser cursors
   
        public void ToggleLaserPointer()
        {
            var expl = ExplanationModeMediator.Inst;

            bool enableLasers = expl.LasersEnabled &&
                                !expl.WebkitOpen &&
                                !expl.ImageViewerOpen;

            if (enableLasers && _laserPointerWndCtx == null)
            {
                _laserPointerWndCtx = new LaserPointerWndCtx(scene, CurrentTopic.Id,
                    LaserPointerTargetSurface.PublicBoard);
            }

            if (_laserPointerWndCtx != null)
                _laserPointerWndCtx.LocalLazerEnabled = enableLasers;

            if (editCtx != null)
                editCtx.SetListeners(!enableLasers);
        }

        #endregion
    }
}
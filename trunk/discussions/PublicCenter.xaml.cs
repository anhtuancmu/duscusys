﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Input;
using Discussions.model;
using System.Windows.Ink;
using System.Diagnostics;
using System.Timers;
using DiscussionsClientRT;
using Discussions.RTModel;
using System.Windows.Threading;
using Discussions.RTModel.Model;
using Discussions.DbModel;
using Discussions.rt;
using System.IO;
using System.Data.Objects;
using Discussions.VectorEditor;
using System.Windows.Media.Animation;
using DistributedEditor;
using System.Windows.Input.Manipulations;
using System.ComponentModel;
using LoginEngine;
using Discussions.webkit_host;
using System.Threading.Tasks;


namespace Discussions
{  
    public partial class PublicCenter : SurfaceWindow
    {
        Discussion _discussion;
        public Discussion discussion 
        {
            get 
            { 
                return _discussion; 
            }
            set
            {
                _discussion = value;
            }
        }

        public Topic CurrentTopic
        {
            get
            {
                return topicNavPanel.selectedTopic;
            }
        }

        //only used for final screenshot 
        int _topicId = -1;
        int _discussionId = -1;

        const double MIN_ZOOM = 0.5;
        const double MAX_ZOOM = 5;

        double _zoomFactor = 1.0;
        public double ZoomFactor
        {
            get
            {
                return _zoomFactor;
            }
            set
            {
                if (value != _zoomFactor)
                {
                    var zoomIn  = value > _zoomFactor;
                    var zoomOut = value < _zoomFactor;

                    _zoomFactor = value;

                    zoomBySlider(_zoomFactor, zoomIn, zoomOut);
                }
            }
        }

 
        UISharedRTClient _sharedClient;

        ZoomSeriesAnalyser _zoomSeries;

        EditorWndCtx editCtx;
        public EditorWndCtx Ctx
        {
            get
            {
                return editCtx;
            }
        }

        bool shapesVisibile = false;

        Discussions.Main.OnDiscFrmClosing _closing;

        DispatcherTimer stopWatchTimer = null;
        
        public PublicCenter(UISharedRTClient sharedClient,
                            Discussions.Main.OnDiscFrmClosing closing, 
                            int screenshotTopicId, int screenshotDiscussionId)
        {
            this._discussion = SessionInfo.Get().discussion;
            _sharedClient = sharedClient;
            _closing = closing;
            _topicId = screenshotTopicId;
            _discussionId = screenshotDiscussionId; 

            InitializeComponent();

            topicNavPanel.topicChanged += topicSelectionChanged;
            topicNavPanel.discussion = SessionInfo.Get().discussion;
            topicNavPanel.topicAnimate += TopicAnimate;

            localAccount.DataContext = SessionInfo.Get().person;        

            avaBar.Init(_sharedClient);
            avaBar.paletteOwnerChanged += PaletteOwnerChanged;
           
            SetListeners(_sharedClient, true);
            
            _zoomSeries = new ZoomSeriesAnalyser(OnSeriesEnd);

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

        void frmLoaded(object sender, RoutedEventArgs args)
        {                        
        }

        void OnSeriesEnd(ZoomDirection direction)
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
            CleanupEditCtx();
              
            if (topicNavPanel.selectedTopic == null)
                return;

            CreateEditCtx();

            //to update text 
            onStopWatch(null, null);
        }

        private void btnViewResults_Click(object sender, RoutedEventArgs e)
        {
            CtxSingleton.SaveChangesIgnoreConflicts();           
            new ResultViewer(discussion,null).Show();
            Close();
        }

        //private void SurfaceWindow_ManipulationStarting(object sender, Manipulation2DStartedEventArgs e)
        //{
        //    //e.Handled = true;
        //    //e.ManipulationContainer = this;            
        //}

        private void SurfaceWindow_ManipulationDelta(object sender, Manipulation2DDeltaEventArgs e)
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

            var xScale = (e.Delta.ScaleX + 2) / 3;
            
            var finalFact = matrix.M11 * xScale;
            if (finalFact > MAX_ZOOM || finalFact < MIN_ZOOM)         
                return;
            
            matrix.ScaleAt(xScale,
                           xScale,
                           e.OriginX - this.ActualWidth / 2,
                           e.OriginY - this.ActualHeight / 2);

            //matrix.RotateAt(e.DeltaManipulation.Rotation,
            //                e.ManipulationOrigin.X,
            //                e.ManipulationOrigin.Y);

            matrix.Translate(e.Delta.TranslationX,
                             e.Delta.TranslationY);

            updateZoomFactor(matrix.M11);

            SetWorkingAreaTransform(matrix, e.Delta.TranslationX  < 0,
                                            e.Delta.TranslationY  < 0,
                                            e.Delta.ScaleX > 1,
                                            e.Delta.ScaleX < 1);            
        }

        void SetWorkingAreaTransform(Matrix m, bool left, bool top, bool zoomIn, bool zoomOut)
        {
            if (blockWorkingAreaTransforms)
                return;           
            
            var mt = new MatrixTransform(m);

            //var unsolved2Wnd = scene.TransformToVisual(mainGrid);
            
            //var topLeft = unsolved2Wnd.Transform(new Point(0,0));
            //if (!zoomIn && topLeft.X > 0 && !left)
            //    return;

            //var bottomRight = unsolved2Wnd.Transform(new Point(scene.ActualWidth, scene.ActualHeight));
            //if (!zoomIn && bottomRight.X < mainGrid.ActualWidth && left)
            //    return;

            //if (!zoomIn && topLeft.Y > scene.ActualHeight && !top)
            //    return;

            //if (!zoomIn && bottomRight.Y < scene.ActualHeight && top)
            //    return;

            scene.RenderTransform   = mt;            
            inkCanv.RenderTransform = mt;

            if (zoomIn || zoomOut)
                _zoomSeries.SubmitStep(zoomIn ? ZoomDirection.In : ZoomDirection.Out);
        }

        private void unsolved_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            //e.Handled = true;
        }

        Matrix GetUnsolvedTransform()
        {
            var transformation = scene.RenderTransform
                                                 as MatrixTransform;
            var matrix = transformation == null ? Matrix.Identity :
                                           transformation.Matrix;
            return matrix;
        }        

        private void unsolved_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Canvas element = scene;

            var matrix = GetUnsolvedTransform();

            Point mousePos = e.GetPosition(this); 
            double factor = e.Delta > 0 ? 1.1 : 0.9;

            var finalFact = matrix.M11 * factor;
            if (finalFact > MAX_ZOOM || finalFact < MIN_ZOOM)
                return;

            matrix.ScaleAt(factor,
                           factor,
                           mousePos.X - System.Windows.SystemParameters.PrimaryScreenWidth/2,
                           mousePos.Y - 0.5*System.Windows.SystemParameters.PrimaryScreenHeight);

            updateZoomFactor(matrix.M11);

            SetWorkingAreaTransform(matrix, false, false, factor > 1, factor < 1);
        }

        void updateZoomFactor(double val)
        {
            _zoomFactor = val;
            zoomSlider.Value = val;
        }

        void zoomBySlider(double finalFactor, bool zoomIn, bool zoomOut)
        {
            Canvas element = scene;

            var matrix = GetUnsolvedTransform();

            var stepFactor = finalFactor / matrix.M11;         
            matrix.ScaleAt(stepFactor,
                           stepFactor,
                           0,
                           0);

            SetWorkingAreaTransform(matrix, false, false, zoomIn, zoomOut);
        }

        private void SurfaceWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            unsolved_MouseWheel(sender, e);
        }

        double PrevX, PrevY;
        private void SurfaceWindow_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);

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

        void SetListeners(UISharedRTClient sharedClient, bool doSet)
        {
            var clienRt = sharedClient.clienRt;
            if (clienRt == null)
                return;
    
           

            //explanation mode functions
            if (doSet)
            {
                clienRt.onBadgeViewRequest += __badgeViewEvent;
                clienRt.onSourceViewRequest += __sourceView;
                WebKitFrm.userRequestedClosing += onLocalSourceViewerClosed;         
                ExplanationModeMediator.Inst.CloseReq += onImgViewerClosed;
                ExplanationModeMediator.Inst.OpenReq  += onImgViewerOpened;
            }
            else
            {
                clienRt.onBadgeViewRequest -= __badgeViewEvent;
                clienRt.onSourceViewRequest -= __sourceView;
                WebKitFrm.userRequestedClosing -= onLocalSourceViewerClosed;
                ExplanationModeMediator.Inst.CloseReq -= onImgViewerClosed;
                ExplanationModeMediator.Inst.OpenReq  -= onImgViewerOpened;                
            }
        }

        void ForgetDBDiscussionState()
        {
            //forget cached state
            CtxSingleton.DropContext();
            _discussion = SessionInfo.Get().discussion;
            DataContext = this;            
        }

        void CleanupEditCtx()
        {
            if (editCtx != null)
            {                
                editCtx.ZoomManipulator.Delta -= SurfaceWindow_ManipulationDelta;
                
                editCtx.CleanupScene();              
                editCtx = null;               
            }
        }

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetListeners(_sharedClient, false);            
            avaBar.Deinit();
            CleanupEditCtx();
            cleanupStopWatch();
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

        void btnToggleShapes_Click(object sender, RoutedEventArgs e)
        {
            if (shapesVisibile)
                ToggleShapes(false);
            else
                ToggleShapes(true);
        }

        void ToggleShapes(bool shVisible)
        {
            if (shVisible)
            {
                editCtx.ShapesVisility(true);   
            }
            else
            {
                editCtx.ShapesVisility(false); 
            }
            modeInfoTip.Visibility = shVisible ? Visibility.Visible : Visibility.Hidden;   
            this.shapesVisibile = shVisible; //save to preserve selected option between topics 
        }

        void CreateEditCtx()
        {
            CleanupEditCtx();

            avaBar.SelectCurrentUser();

            editCtx = new EditorWndCtx(scene,
                                        inkCanv,
                                        palette,
                                        inkPalette,
                                        this,//surface window for focus fix                                      
                                        _topicId != -1 ? _topicId : CurrentTopic.Id,
                                        _discussionId != -1 ? _discussionId : CurrentTopic.Discussion.Id,
                                        shapesVisibile);

            editCtx.ZoomManipulator.Delta += SurfaceWindow_ManipulationDelta;

            DataContext = this;
            _sharedClient.clienRt.SendInitialSceneLoadRequest(_topicId != -1 ? _topicId : CurrentTopic.Id);        
        }
  
        private void SurfaceWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                btnToggleShapes.IsChecked = !btnToggleShapes.IsChecked;
                btnToggleShapes_Click(null, null);               
            }            
            else if (e.Key == Key.Delete && shapesVisibile)
            {
                editCtx.RemoveShape(palette.GetOwnerId());
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.C)
                {
                    if(editCtx!=null)
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

        void ensureOwnerSelected()
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
        void TopicAnimate(bool hide)
        {
            if (hide)
            {
                var s = (Storyboard)FindResource("HideTopicsStoryboard");
                s.Begin();                    
            }
            else
            {
                var s = (Storyboard)FindResource("ShowTopicsStoryboard");
                s.Begin();                    
            }
        }
        #endregion topic listbox

        void PaletteOwnerChanged(int owner)
        {
            if (owner != -1)
            {
                palette._ownerId = owner;
                palette.bdr.BorderBrush = new SolidColorBrush(DaoUtils.UserIdToColor(owner));
                inkPalette.bdr.BorderBrush = new SolidColorBrush(DaoUtils.UserIdToColor(owner));    
            }
        }

        private void SurfaceWindow_TouchDown_1(object sender, TouchEventArgs e)
        {
            //stop promoting to mouse event
            //e.Handled = true;
        }

        private void SurfaceWindow_TouchMove_1(object sender, TouchEventArgs e)
        {
            //stop promoting to mouse event
           // e.Handled = true;
        }

        private void SurfaceWindow_TouchUp_1(object sender, TouchEventArgs e)
        {
            //stop promoting to mouse event
         //   e.Handled = true;
        }

        private void btnHome_Click_1(object sender, RoutedEventArgs e)
        {
            DiscWindows.Get().mainWnd.Activate();
            //this.Close();
        }

        DateTime _recentStopWatchTick;
        bool _firstTick = true; 
        void onStopWatch(object sender, EventArgs e)
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

        void updateStopWatch(Topic displayedTopic, TimeSpan passedSinceLastUpdate)
        {
           var discId = SessionInfo.Get().discussion.Id;
           var freshDisc = CtxSingleton.Get().Discussion.FirstOrDefault(d0=>d0.Id==discId);
           bool needSave = false;
           foreach (var topic in freshDisc.Topic)
           {
               if (!topic.Running)
               {
                   if (topic.Id == displayedTopic.Id)
                       stopWatch.Text = TimeSpan.FromSeconds((double)topic.CumulativeDuration).ToString(); 
                   continue;
               }

               needSave = true;
               topic.CumulativeDuration += passedSinceLastUpdate.Seconds;

               if (topic.Id == displayedTopic.Id)
                   stopWatch.Text = TimeSpan.FromSeconds((double)topic.CumulativeDuration).ToString();               
           }
           if (needSave)
              CtxSingleton.Get().SaveChanges();                     
        }

        void startStopWatch()
        {
            if (stopWatchTimer != null)
                return;

            stopWatchTimer = new DispatcherTimer();
            stopWatchTimer.Interval = TimeSpan.FromSeconds(1);
            stopWatchTimer.Tick += onStopWatch;
            stopWatchTimer.Start();
        }

        void cleanupStopWatch()
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
        LargeBadgeView _lbv = null;
        bool blockWorkingAreaTransforms = false;

        void __badgeViewEvent(BadgeViewMessage bv)
        {         
            if(!btnExplanationMode.IsChecked.HasValue || !btnExplanationMode.IsChecked.Value)
                return;

            if (bv.doExpand)
            {
                ShowLargeBadgeView(CtxSingleton.Get().ArgPoint.FirstOrDefault(ap0 => ap0.Id == bv.argPointId));
            }
            else
            {
                HideLargeBadgeView();
            }
        }
        void LargeRequest(object sender, RoutedEventArgs e)
        {
            var badge = e.OriginalSource as Badge4;
            if (badge == null)
                return;
           
            ShowLargeBadgeView(badge.DataContext as ArgPoint);
            UISharedRTClient.Instance.clienRt.SendStatsEvent(StEvent.BadgeZoomIn,
                                                            SessionInfo.Get().person.Id,
                                                            SessionInfo.Get().discussion.Id,
                                                            topicNavPanel.selectedTopic.Id,
                                                            DeviceType.Wpf);
            UISharedRTClient.Instance.clienRt.SendBadgeViewRequest((badge.DataContext as ArgPoint).Id, true);
        }
        void ShrinkRequest(object sender, RoutedEventArgs e)
        {
            HideLargeBadgeView();
            UISharedRTClient.Instance.clienRt.SendBadgeViewRequest(-1, false);
        }
        void ShowLargeBadgeView(ArgPoint ap)
        {
            if (_lbv != null)
                return;

            scene.IsHitTestVisible = false;
            blockWorkingAreaTransforms = true;

            _lbv = new LargeBadgeView();
             var ArgPointId = ap.Id;
             _lbv.DataContext = DbCtx.Get().ArgPoint.FirstOrDefault(p0 => p0.Id == ArgPointId);
            _lbv.SetRt(UISharedRTClient.Instance);
            mainGrid.Children.Add(_lbv);
            Grid.SetRowSpan(_lbv, 2);
            _lbv.HorizontalAlignment = HorizontalAlignment.Center;
            _lbv.VerticalAlignment   = VerticalAlignment.Center;
        }
        void HideLargeBadgeView()
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
        void sourceView(object sender, RoutedEventArgs e)
        {
            var sourceUC = e.OriginalSource as FrameworkElement;
            if (sourceUC == null)
                return;

            var src   = sourceUC.DataContext as Source;
            var att = sourceUC.DataContext as Attachment;

            if (src != null)
                UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(SyncMsgType.SourceView, src.Id, true); 
            else if(att!=null)
                UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(SyncMsgType.YoutubeView, att.Id, true);                         
        }
        
        //local source viewer is about to close 
        void onLocalSourceViewerClosed()
        {
            UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(SyncMsgType.SourceView, -1, false);           
        }

        void onImgViewerClosed(int attachId)
        {
            UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(SyncMsgType.ImageView, attachId, false);    
        }

        void onImgViewerOpened(int attachId)
        {
            UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(SyncMsgType.ImageView, attachId, true);     
        }

        //message from remote client
        void __sourceView(ExplanationModeSyncMsg sm)
        {
            if (!btnExplanationMode.IsChecked.HasValue || !btnExplanationMode.IsChecked.Value)
                return;

            if (sm.doExpand)
            {
                switch (sm.syncMsgType)
                {
                    case SyncMsgType.SourceView:
                        var src = CtxSingleton.Get().Source.FirstOrDefault(s0 => s0.Id == sm.viewObjectId);
                        var browser = new WebKitFrm(src.Text);
                        browser.Show();
                        break;
                    case SyncMsgType.YoutubeView:
                        var attach = CtxSingleton.Get().Attachment.FirstOrDefault(a0 => a0.Id == sm.viewObjectId);
                        var embedUrl = AttachmentToVideoConvertor.AttachToYtInfo(attach).EmbedUrl;
                        browser = new WebKitFrm(embedUrl);
                        browser.Show();
                        break;
                    case SyncMsgType.ImageView:
                        attach = CtxSingleton.Get().Attachment.FirstOrDefault(a0 => a0.Id == sm.viewObjectId);
                        if(attach!=null && !ExplanationModeMediator.Inst.IsViewerOpened(attach.Id))
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
                        WebKitFrm.EnsureInstanceClosed();
                        break;
                    case SyncMsgType.ImageView:
                        ExplanationModeMediator.Inst.EnsureInstanceClosed(sm.viewObjectId);
                        break;
                    case SyncMsgType.YoutubeView:
                        WebKitFrm.EnsureInstanceClosed();
                        break;
                    default:
                        throw new NotImplementedException();
                }    
            }
        }
        #endregion explanation mode browser

        #region screenshot of final scene
        TaskCompletionSource<string> finalSceneTcs = null;
        public Task<string> FinalSceneScreenshot()
        {
            ToggleShapes(true);

            finalSceneTcs = new TaskCompletionSource<string>();
            _sharedClient.clienRt.loadingDoneEvent += shapeLoadingDone;
            return finalSceneTcs.Task;
        }
        async void shapeLoadingDone()
        {
            _sharedClient.clienRt.loadingDoneEvent -= shapeLoadingDone;
            
            //even though all d-editor objects are loaded, their visuals on canvas are created asynchronously
            await Utils.Delay(3000);

            finalSceneTcs.SetResult(Screenshot.Take(scene, 200));
            finalSceneTcs = null;
        }
        #endregion
    }
}

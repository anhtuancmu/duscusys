using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Shapes;
using Discussions;
using Discussions.ctx;
using Discussions.DbModel.model;
using Discussions.d_editor;
using Discussions.model;
using Discussions.rt;
using Discussions.RTModel.Model;

namespace DistributedEditor
{
    //coordination of drawing tools and controls/visibility, management of 
    //drawing state, invoking methods on document
    public class SceneManager : IDisposable
    {
        private readonly Canvas _scene;
        private readonly Palette _palette;

        private readonly DistributedInkCanvas _ink;
        private readonly InkPalette _inkPalette;

        private readonly InpModeMgr _modeMgr = new InpModeMgr();

        public InpModeMgr ModeMgr
        {
            get { return _modeMgr; }
        }

        private VdDocument _doc;

        public VdDocument Doc
        {
            get { return _doc; }
        }

        private readonly UISharedRTClient _rt = UISharedRTClient.Instance;

        private CursorApprovalData _cursorApproval;

        private LinkCreationRecord _linkCreation;

        private ICaptionHost _hostAwaitingCaption;

        public SceneManager(Canvas scene, DistributedInkCanvas ink, Palette palette, InkPalette inkPalette,
                            int topicId, int discussionId, bool shapeVisibility)
        {
            _scene = scene;
            _palette = palette;

            _ink = ink;
            _inkPalette = inkPalette;

            //non-NaN palette coords
            Canvas.SetLeft(_palette, 200);
            Canvas.SetTop(_palette, 200);

            _doc = new VdDocument(palette, scene, ShapePostCtor, topicId, discussionId, shapeVisibility);

            inkPalette.Init(FinishFreeDrawing, ink);

            setListeners(true);
        }

        public void Dispose()
        {
            setListeners(false);
        }

        public void FinishFreeDrawing()
        {
            if (_ink.IsHitTestVisible)
            {
                _palette.ResetOvers();
                FreeDrawingMode(false);
            }
        }

        private void FreeDrawingMode(bool doEnter)
        {
            if (doEnter)
            {
                _ink.IsHitTestVisible = true;
                _inkPalette.Visibility = Visibility.Visible;
                _palette.Visibility = Visibility.Hidden;

                //set drawing attributes of current palette owner                
                var da = _ink.DefaultDrawingAttributes.Clone();
                da.Color = DaoUtils.UserIdToColor(_palette.GetOwnerId());
                _ink.DefaultDrawingAttributes = da;
            }
            else
            {
                _ink.IsHitTestVisible = false;
                _inkPalette.Visibility = Visibility.Hidden;
                _palette.Visibility = Visibility.Visible;
                _modeMgr.Mode = ShapeInputMode.ManipulationExpected;

                //create free form shape if we have locally created strokes            
                var ownColor = DaoUtils.UserIdToColor(_palette.GetOwnerId());

                //while we have been drawing, somebody else could draw and still hasn't finalized drawing, so ink canvas can contain
                //strokes from multiple authors
                var ownStrokes = _ink.Strokes.Where(st => st.DrawingAttributes.Color == ownColor);
                if (ownStrokes.Count() > 0)
                {
                    //don't take cursor for free draw
                    var freeFrmSh =
                        (VdFreeForm) _doc.BeginCreateShape(VdShapeType.FreeForm, 0, 0, false, DocTools.TAG_UNDEFINED);
                    freeFrmSh.locallyJustCreated = true; //enable one-time stroke send
                    var ownStrokeCollection = new StrokeCollection(ownStrokes);
                    freeFrmSh.extractGeomtry(ownStrokeCollection, ownStrokeCollection.GetBounds());

                    //but set focus
                    _doc.VolatileCtx.UnfocusAll();
                    freeFrmSh.SetFocus();

                    //send state update to other clients
                    SendSyncState(freeFrmSh);

                    //remove own strokes
                    var notOwnStrokes = _ink.Strokes.Where(st => st.DrawingAttributes.Color != ownColor);
                    _ink.Strokes = new StrokeCollection(notOwnStrokes);

                    TryEndHostCaption(freeFrmSh, CaptionType.FreeDraw);
                }

                //update ink on other clients
                sendLocalInk();
            }
        }

        private void BeginHostCaption(ICaptionHost host, CaptionType type)
        {
            _hostAwaitingCaption = host;

            RemovePreviousCaption(host, type);
        }

        private void RemovePreviousCaption(ICaptionHost cluster, CaptionType type)
        {
            if (_hostAwaitingCaption.CapMgr().FreeDraw != null)
                _doc.BeginRemoveSingleShape(_hostAwaitingCaption.CapMgr().FreeDraw.Id());
            if (_hostAwaitingCaption.CapMgr().text != null)
                _doc.BeginRemoveSingleShape(_hostAwaitingCaption.CapMgr().text.Id());         
        }

        private void TryEndHostCaption(IVdShape caption, CaptionType type)
        {
            //inject caption
            if (_hostAwaitingCaption == null)
                return;

            RemovePreviousCaption(_hostAwaitingCaption, type);

            if (caption is VdFreeForm)
            {
                _hostAwaitingCaption.CapMgr().FreeDraw = (VdFreeForm)caption;                

                //initial resize of free form
                _hostAwaitingCaption.CapMgr().InitialResizeOfFreeForm();

                //send resized free form 
                SendSyncState(_hostAwaitingCaption.CapMgr().FreeDraw);
                
                //cluster + title stats event
                _rt.clienRt.SendStatsEvent(StEvent.ClusterTitleAdded,
                                            _palette.GetOwnerId(),
                                            _doc.DiscussionId,
                                            _doc.TopicId,
                                            DeviceType.Wpf);              
            }
            else if (caption is VdText)
            {
                _hostAwaitingCaption.CapMgr().text = (VdText)caption;

                //increase caption font
                //for(int i = 0; i<6; ++i)
                //    _hostAwaitingCaption.CapMgr().text.ScaleInPlace(true);

                SendSyncState(_hostAwaitingCaption.CapMgr().text);

                //cluster + title stats event
                _rt.clienRt.SendStatsEvent(StEvent.ClusterTitleAdded,
                                            _palette.GetOwnerId(),
                                            _doc.DiscussionId,
                                            _doc.TopicId,
                                            DeviceType.Wpf);     
            }
            else
                throw new NotSupportedException();

            //update first time after build
            _hostAwaitingCaption.CapMgr().UpdateRelatives();

            //send state of cluster to attach captions on other clients 
            SendSyncState(_hostAwaitingCaption);

            _hostAwaitingCaption = null;
        }

        //we have editing permission if either shape is free or if shape is cursored by us
        private bool editingPermission(IVdShape sh)
        {
            var noPermission = sh.GetCursor() != null && sh.GetCursor().OwnerId != _palette.GetOwnerId();
            return !noPermission;
        }

        //we are removing locally focused shape (not cursored by others), if any. 
        //the shape either must have our cursor or be cursor-free 
        public void RemoveShape(int owner)
        {
            var sh = _doc.VolatileCtx.LocalFocus;
            if (sh == null)
            {
                MessageDlg.Show("Please select shape to remove",
                                "No shape selected",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            if (!editingPermission(sh))
            {
                MessageDlg.Show("Cannot remove shape under user cursor",
                                "No permission",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            //badges and clusters cannot be removed with palette
            if (sh.ShapeCode() == VdShapeType.Badge || sh.ShapeCode() == VdShapeType.Cluster)
                return;

            _doc.BeginRemoveSingleShape(sh.Id());
        }

        //if we have editing permission for all own shapes, we can clear them all
        public void RemoveOwnShapes(int owner)
        {
            var ownShapes =
                _doc.GetShapes().Where(sh => sh.InitialOwner() == owner && sh.ShapeCode() != VdShapeType.Badge);
            foreach (var s in ownShapes)
                if (!editingPermission(s))
                {
                    MessageDlg.Show("To delete your shapes, wait until all your shapes are free of user cursors",
                                    "No permission",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

            _doc.BeginClearShapesOfOwner();
        }

        public void EnterShapeCreationMode(VdShapeType shapeType, int shapeTag)
        {
            if (shapeType == VdShapeType.None)
            {
                _linkCreation.end1 = _linkCreation.end2 = null;
                _modeMgr.Mode = ShapeInputMode.ManipulationExpected;
                return;
            }

            if (_doc.VolatileCtx.LocalCursor != null)
            {
                _palette.ResetOvers();
                _modeMgr.Mode = ShapeInputMode.ManipulationExpected;
                return;
            }

            switch (shapeType)
            {
                case VdShapeType.ClusterLink:
                    _modeMgr.Mode = ShapeInputMode.LinkedObj1Expected;
                    _linkCreation.linkId = -1; //reset Id of previously locally created link
                    _linkCreation.headType = (LinkHeadType) shapeTag;
                    break;
                case VdShapeType.FreeForm:
                    _modeMgr.Mode = ShapeInputMode.CreationExpected;
                    FreeDrawingMode(true);
                    break;
                default:
                    _modeMgr.Mode = ShapeInputMode.CreationExpected;
                    break;
            }
        }

        private void CreateManipulate(VdShapeType shapeType,
                                      double startX, double startY)
        {
            if (shapeType == 0)
                return;

            var sh = _doc.BeginCreateShape(shapeType, startX, startY, true, DocTools.TAG_UNDEFINED);

            if (shapeType == VdShapeType.FreeForm || shapeType == VdShapeType.Text)
                TryEndHostCaption(sh, shapeType == VdShapeType.FreeForm ? CaptionType.FreeDraw : CaptionType.Text);

            CaptureAndStartManip(sh, new Point(startX, startY), null, null);
            _modeMgr.Mode = ShapeInputMode.Manipulating;
        }

        private void GetLinkables(Point pos, TouchDevice d)
        {
            Shape resizeNode = null;
            var sh = DocTools.DetectSelectedShape(_doc, pos, d, out resizeNode) as IVdShape;
            var end = DocTools.RequestLinkable(sh);
            if (end == null)
            {
                MessageDlg.Show("Can only link badges and clusters",
                                "Tip",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                _palette.ResetTool();
                _modeMgr.Mode = ShapeInputMode.ManipulationExpected;
                return;
            }

            if (_linkCreation.end1 == null)
            {
                _linkCreation.end1 = end;
            }
            else if (end == _linkCreation.end1)
            {
                _palette.ResetTool();
                _modeMgr.Mode = ShapeInputMode.ManipulationExpected;
                MessageDlg.Show("Cannot link object with itself",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else
                _linkCreation.end2 = end;
        }

        public bool InpDeviceDown(Point pos, TouchDevice touchDev)
        {
            DocTools.UnfocusAll(_doc.GetShapes().Where(sh => !sh.IsManipulated()));
            switch (_modeMgr.Mode)
            {
                case ShapeInputMode.CreationExpected:
                    _modeMgr.Mode = ShapeInputMode.ManipulationExpected;
                    CreateManipulate(_palette.shapeType, pos.X, pos.Y);
                    if (_palette != null)
                        _palette.ResetOvers();
                    return true;
                case ShapeInputMode.LinkedObj1Expected:
                    GetLinkables(pos, touchDev);
                    if (_linkCreation.end1 != null)
                        ModeMgr.Mode = ShapeInputMode.LinkedObj2Expected;
                    return true;
                case ShapeInputMode.LinkedObj2Expected:
                    GetLinkables(pos, touchDev);
                    if (_linkCreation.end1 != null && _linkCreation.end2 != null)
                    {
                        _linkCreation.linkId = _doc.BeginCreateLink(_linkCreation.end1.GetId(), _linkCreation.end2.GetId(),
                                                                   _linkCreation.headType);
                        _linkCreation.end1 = null;
                        _linkCreation.end2 = null;
                        ModeMgr.Mode = ShapeInputMode.ManipulationExpected;

                        if (_palette != null)
                            _palette.ResetOvers();
                    }
                    return true;
                case ShapeInputMode.ManipulationExpected:
                    //no current touch points on shapes (maybe touch points over empty space)

                    Shape resizeNode = null;
                    var underContact = DocTools.DetectSelectedShape(_doc, pos, touchDev, out resizeNode) as IVdShape;
                    if (underContact == null)
                        return false;

                    var shapeFree = underContact.GetCursor() == null;
                    var shapeLockedByUs = false;
                    if (!shapeFree)
                        shapeLockedByUs = underContact.GetCursor().OwnerId == _palette.GetOwnerId();

                    //this shape free and we don't have cursors
                    if (shapeFree && _doc.VolatileCtx.LocalCursor == null)
                    {
                        //shape free, try lock it and schedule cursor approval continuation                        
                        {
                            _cursorApproval.resizeNode = resizeNode;
                            _cursorApproval.pos = pos;
                            _cursorApproval.td = touchDev;
                            _modeMgr.Mode = ShapeInputMode.CursorApprovalExpected;
                        }

                        //take new local cursor
                        _doc.VolatileCtx.BeginTakeShapeWithLocalCursor(underContact.Id());
                    }
                    else if (shapeLockedByUs)
                    {
                        CaptureAndStartManip(underContact, pos, resizeNode, touchDev);
                    }
                    return true;
                case ShapeInputMode.Manipulating:
                    return true;
                case ShapeInputMode.CursorApprovalExpected:
                    return true;
                default:
                    throw new NotSupportedException();
            }
        }

        private void CaptureAndStartManip(IVdShape sh, Point pt, object sender, TouchDevice td)
        {
            sh.StartManip(pt, sender);
            sh.UnderlyingControl().CaptureMouse();
            if (td != null)
                sh.UnderlyingControl().CaptureTouch(td);
            Console.WriteLine("Scene Mgr : CaptureAndStartManip");
        }

        private void ReleaseCaptureAndFinishManip(IVdShape sh)
        {
            sh.UnderlyingControl().ReleaseMouseCapture();
            sh.UnderlyingControl().ReleaseAllTouchCaptures();
            sh.FinishManip();

            //if the moved shape is a caption of caption host, save caption host to save new relative position of caption
            var capHost = DocTools.GetCaptionHost(_doc.GetShapes(), sh);
            if (capHost != null)
            {
                capHost.CapMgr().UpdateRelatives();
                SendSyncState(capHost);
            }
        }

        //used by large badge view mode
        public void CancelManipulation()
        {
            _doc.VolatileCtx.skipNextAquiredCursor = true;
            InpDeviceUp(new Point(0, 0));
        }

        public void InpDeviceUp(Point pos)
        {
            if (_modeMgr.Mode == ShapeInputMode.Manipulating)
            {
                _modeMgr.Mode = ShapeInputMode.ManipulationExpected;
            }
            else if (_modeMgr.Mode == ShapeInputMode.CursorApprovalExpected)
            {
                _modeMgr.Mode = ShapeInputMode.ManipulationExpected;
            }
          
            var lc = _doc.VolatileCtx.LocalCursor;
            if (lc != null)
            {
                //cluster created event ?
                var clust = lc as VdCluster;

                //if this finish manip completes initial cluster drawing, don't generate move event
                var supressClusterMoveEvent = false;

                if (clust != null && !clust.ClusterCreated)
                {
                    supressClusterMoveEvent = true;
                    _rt.clienRt.SendStatsEvent(StEvent.ClusterCreated,
                                               _palette.GetOwnerId(),
                                               _doc.DiscussionId,
                                               _doc.TopicId,
                                               DeviceType.Wpf);
                }

                ReleaseCaptureAndFinishManip(lc);
                _doc.VolatileCtx.BeginFreeCursor(supressClusterMoveEvent);
            }
            else
            {
                _doc.VolatileCtx.NotifyPointUpEvent();
            }
        }

        private void localCursorChanged(CursorMgr mgr)
        {
            if (mgr.LocalCursor == null)
                return;

            if (mgr.skipNextAquiredCursor)
            {
                mgr.skipNextAquiredCursor = false;
                InpDeviceUp(new Point(0,0));
                return;
            }

            //local cursor approval
            switch (_modeMgr.Mode)
            {
                case ShapeInputMode.CursorApprovalExpected:
                    _modeMgr.Mode = ShapeInputMode.Manipulating;
                    CaptureAndStartManip(mgr.LocalCursor, _cursorApproval.pos, _cursorApproval.resizeNode,
                                         _cursorApproval.td);
                    if (mgr.LocalCursor!=null)
                        mgr.LocalCursor.SetFocus();
                    break;
            }
        }

        public void InpDeviceMove(Point pos)
        {
            if (_modeMgr.Mode == ShapeInputMode.Manipulating && _doc.VolatileCtx.LocalCursor != null)
                BeginApplyPoint(_doc.VolatileCtx.LocalCursor, pos.X, pos.Y);
        }

        #region multitouch manipulations

        private void ShapePostCtor(IVdShape sh, VdShapeType shapeType)
        {
            var ctl = sh.UnderlyingControl();
            ctl.IsManipulationEnabled = true;
            ctl.ManipulationStarting += ManipulationStarting;
            ctl.ManipulationDelta += ManipulationDelta;
            ctl.ManipulationCompleted += ManipulationCompleted;

            switch (shapeType)
            {
                case VdShapeType.Text:
                    ((VdText) sh).onChanged += onTextChanged;
                    ((VdText)sh).onEdited += onTextEdited;
                    break;
                case VdShapeType.Cluster:
                    ((ICaptionHost) sh).InitCaptions(CaptionCreationRequested);
                    break;
                case VdShapeType.ClusterLink:
                    ((ICaptionHost) sh).InitCaptions(CaptionCreationRequested);

                    //if the link was created locally, send its state 
                    if (sh.Id() == _linkCreation.linkId)
                    {
                        SendSyncState(sh);
                        _linkCreation.linkId = -1;
                    }
                    break;
            }
        }

        private void CaptionCreationRequested(CaptionType type, ICaptionHost host)
        {
            BeginHostCaption(host, type);
            switch (type)
            {
                case CaptionType.FreeDraw:
                    EnterShapeCreationMode(VdShapeType.FreeForm, -1);
                    break;
                case CaptionType.Text:
                    //emulate text creation              
                    _palette.shapeType = VdShapeType.Text;
                    EnterShapeCreationMode(VdShapeType.Text, -1);

                    var clickLocation = host.capOrgProvider();
                    InpDeviceDown(new Point(clickLocation.X, clickLocation.Y), null);
                    break;
            }
        }

        private void ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = _scene;
            e.Handled = true;

            var underContact = (IVdShape) ((FrameworkElement) sender).Tag;

            underContact.ManipulationStarting(sender, e);
            Console.WriteLine("Scene Mgr : ManipulationStarting");
        }

        private void ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (_doc.clusterRebuildPending)
                return;

            var localCurs = _doc.VolatileCtx.LocalCursor;
            if (localCurs == null)
                return;

            if (localCurs.ShapeCode() == VdShapeType.ClusterLink)
                return;

            if (e.DeltaManipulation.Scale.X != 1.0 || e.DeltaManipulation.Scale.Y != 1.0)
                _doc.VolatileCtx.ResizeDetected();
            if (e.DeltaManipulation.Translation.X != 0.0 || e.DeltaManipulation.Translation.Y != 0.0)
                _doc.VolatileCtx.MovementDetected();

            localCurs.ManipulationDelta(sender, e);
            SendSyncState(localCurs);
            NotifyClusterableMoved(localCurs);

            if (localCurs.ShapeCode() == VdShapeType.Cluster)
                updateHostCaptions((VdCluster) localCurs);
        }

        private void ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var localCurs = _doc.VolatileCtx.LocalCursor;
            if (localCurs != null)
            {
                localCurs.ManipulationCompleted(sender, e);
            }
        }

        private void onTextChanged(VdText text)
        {
            SendSyncState(text);
        }

        private void onTextEdited(VdText text)
        {
            var hostCluster = Doc.GetShapes()
                .Where(sh => sh.ShapeCode() == VdShapeType.Cluster)
                .FirstOrDefault(cl => cl.GetState(_doc.TopicId).ints[0] == text.Id());

            if (hostCluster != null)
            {
                _rt.clienRt.SendStatsEvent(StEvent.ClusterTitleEdited,
                                           _palette.GetOwnerId(),
                                           _doc.DiscussionId,
                                           _doc.TopicId,
                                           DeviceType.Wpf);       
            }
        }

        private void SendSyncState(IVdShape sh)
        {
            _rt.clienRt.SendSyncState(sh.Id(), sh.GetState(_doc.TopicId));
        }

        private void updateHostCaptions(ICaptionHost capHost)
        {
            capHost.CapMgr().SetBounds();
        }

        #endregion multitouch manipulations

        public void BeginApplyPoint(IVdShape sh,
                                    double X,
                                    double Y)
        {
            if (_doc.clusterRebuildPending)
                return;

            //apply point locally
            switch (sh.ApplyCurrentPoint(new Point(X, Y)))
            {
                case PointApplyResult.None:
                    break;
                case PointApplyResult.Move:
                    _doc.VolatileCtx.MovementDetected();
                    break;
                case PointApplyResult.MoveResize:
                    _doc.VolatileCtx.MovementDetected();
                    _doc.VolatileCtx.ResizeDetected();
                    break;
                case PointApplyResult.Resize:
                    _doc.VolatileCtx.ResizeDetected();
                    break;
                default:
                    throw new NotSupportedException();
            }

            SendSyncState(sh);

            switch (sh.ShapeCode())
            {
                case VdShapeType.Badge:
                    NotifyClusterableMoved(sh);
                    break;
                case VdShapeType.Cluster:
                case VdShapeType.ClusterLink:
                    updateHostCaptions((ICaptionHost) sh);
                    break;
            }
        }

        private void NotifyClusterableMoved(IVdShape sh)
        {
            if (sh.ShapeCode() == VdShapeType.Badge)
            {
                var clustShapes = _doc.GetShapes().Where(sh0 => sh0.ShapeCode() == VdShapeType.Cluster);
                //todo: optimization possible
                foreach (var clust in clustShapes)
                {
                    ((VdCluster) clust).ClusterableMoved(((VdBadge) sh).GetClusterable());
                }
            }
        }

        private void syncStateEvent(ShapeState state)
        {
            if (state.topicId != _doc.TopicId)
                return;

            PlaySyncStateEvent(state);
        }

        private void PlaySyncStateEvent(ShapeState state)
        {
            var sh = _doc.IdToShape(state.shapeId);
            if (sh == null)
            {
                //error of initialization (not all existing shapes were loaded to local station)
                return;
            }

            sh.ApplyState(state);
        }

        private void ReloadBadgeContexts()
        {
            BadgesCtx.DropContext();
            DocTools.ToggleBadgeContexts(BadgesCtx.Get(), _doc.GetShapes().Where(sh => sh.ShapeCode() == VdShapeType.Badge));
        }

        private void argPointChanged(int ArgPointId, int topicId, PointChangedType change, int personId)
        {
            if (topicId != _doc.TopicId)
                return;

            switch (change)
            {
                case PointChangedType.Created:
                    //we process create event for respective shape
                    break;
                case PointChangedType.Modified:
                    ReloadBadgeContexts();
                    break;
                case PointChangedType.Deleted:
                    //we process delete event for respective shape
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void OnCommentRead(CommentsReadEvent ev)
        {
            if (ev.TopicId == _doc.TopicId && ev.PersonId == SessionInfo.Get().person.Id)
            {
                //if a comment is read by us in the topic of scene, update the badge to trigger notifications dot
                ReloadBadgeContexts();
            }
        }

        private void inkStateEvent(InkMessage ink)
        {
            if (ink.topicId != _doc.TopicId)
                return;

            PlayInkEvent(ink);
        }

        private void PlayInkEvent(InkMessage ink)
        {
            if (!_doc.ShapeVisibility)
                return;

            var s = new MemoryStream();
            s.Write(ink.inkData, 0, ink.inkData.Length);
            s.Position = 0;

            _ink.Strokes.Add(new StrokeCollection(s));
        }

        private void sendLocalInk()
        {
            using (var s = new MemoryStream())
            {
                _ink.Strokes.Save(s, true);
                UISharedRTClient.Instance.clienRt.SendInkRequest(_palette.GetOwnerId(), _doc.TopicId, s.ToArray());
            }
        }

        #region photon events

        public void setListeners(bool doSet)
        {
            if (doSet)
            {
                _doc.VolatileCtx.localCursorChanged += localCursorChanged;
                //_ink.OnInkChanged += OnLocalInkChanged;
            }
            else
            {
                // _ink.OnInkChanged -= OnLocalInkChanged;
                _doc.VolatileCtx.localCursorChanged -= localCursorChanged;
            }

            if (doSet)
                _rt.clienRt.syncStateEvent += syncStateEvent;
            else
                _rt.clienRt.syncStateEvent -= syncStateEvent;

            if (doSet)
                _rt.clienRt.argPointChanged += argPointChanged;
            else
                _rt.clienRt.argPointChanged -= argPointChanged;

            if (doSet)
                _rt.clienRt.inkEvent += inkStateEvent;
            else
                _rt.clienRt.inkEvent -= inkStateEvent;

            if (doSet)
                _rt.clienRt.onCommentRead += OnCommentRead;
            else
                _rt.clienRt.onCommentRead -= OnCommentRead;            
        }

        #endregion photon events
    }
}
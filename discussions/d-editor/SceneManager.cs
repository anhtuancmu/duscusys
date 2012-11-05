using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using Discussions;
using Discussions.d_editor;
using Discussions.model;
using Discussions.rt;
using Discussions.RTModel.Model;
using LoginEngine;
using Microsoft.Surface.Presentation.Controls;

namespace DistributedEditor
{     
    //coordination of drawing tools and controls/visibility, management of 
    //drawing state, invoking methods on document
    public class SceneManager : IDisposable
    {
        Canvas _scene;
        Palette _palette;

        DistributedInkCanvas _ink;
        InkPalette _inkPalette;

        InpModeMgr _modeMgr = new InpModeMgr();
        public InpModeMgr ModeMgr
        {
            get
            {
                return _modeMgr;
            }
        }

        VdDocument _doc;
        public VdDocument Doc
        {
            get
            {
                return _doc;
            }
        }

        UISharedRTClient _rt = UISharedRTClient.Instance;

        CursorApprovalData cursorApproval;

        LinkCreationRecord linkCreation;

        ICaptionHost hostAwaitingCaption = null;

        bool touchDevice = false;

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
            if(_ink.IsHitTestVisible)
            {            
                _palette.ResetOvers();
                FreeDrawingMode(false);
            }
        }

        void FreeDrawingMode(bool doEnter)
        {
            if (doEnter)
            {
                _ink.IsHitTestVisible  = true;
                _inkPalette.Visibility = Visibility.Visible;
                _palette.Visibility    = Visibility.Hidden;

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
                    var freeFrmSh = (VdFreeForm)_doc.BeginCreateShape(VdShapeType.FreeForm, 0, 0, false, DocTools.TAG_UNDEFINED);
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

        void BeginHostCaption(ICaptionHost host, CaptionType type)
        {
            hostAwaitingCaption = host;

            RemovePreviousCaption(host, type);
        }
        void RemovePreviousCaption(ICaptionHost cluster, CaptionType type)
        {
            if (hostAwaitingCaption.CapMgr().FreeDraw != null)
                _doc.BeginRemoveSingleShape(hostAwaitingCaption.CapMgr().FreeDraw.Id());
            if (hostAwaitingCaption.CapMgr().text != null)
                _doc.BeginRemoveSingleShape(hostAwaitingCaption.CapMgr().text.Id());
        }

        void TryEndHostCaption(IVdShape caption, CaptionType type)
        {
            //inject caption
            if (hostAwaitingCaption == null)
                return;

            RemovePreviousCaption(hostAwaitingCaption, type);

            if (caption is VdFreeForm)
            {
                hostAwaitingCaption.CapMgr().FreeDraw = (VdFreeForm)caption;
                
                //initial resize of free form
                hostAwaitingCaption.CapMgr().InitialResizeOfFreeForm();

                //send resized free form 
                SendSyncState(hostAwaitingCaption.CapMgr().FreeDraw);
            }
            else if (caption is VdText)
            {
                hostAwaitingCaption.CapMgr().text = (VdText)caption;
                SendSyncState(hostAwaitingCaption.CapMgr().text);
            }
            else
                throw new NotSupportedException();

            //update first time after build
            hostAwaitingCaption.CapMgr().UpdateRelatives();

            //send state of cluster to attach captions on other clients 
            SendSyncState(hostAwaitingCaption);
               
            hostAwaitingCaption = null;            
        }

        //we have editing permission if either shape is free or if shape is cursored by us
        bool editingPermission(IVdShape sh)
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
                MessageBox.Show("Please select shape to remove",
                                "No shape selected",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            if (!editingPermission(sh))
            {
                MessageBox.Show("Cannot remove shape under user cursor",
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
            var ownShapes = _doc.GetShapes().Where(sh => sh.InitialOwner() == owner && sh.ShapeCode() != VdShapeType.Badge);
            foreach (var s in ownShapes)
                if (!editingPermission(s))
                {
                    MessageBox.Show("To delete your shapes, wait until all your shapes are free of user cursors",
                                    "No permission",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

            _doc.BeginClearShapesOfOwner();
        }

        public void EnterShapeCreationMode(VdShapeType shapeType, int shapeTag)
        {
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
                    linkCreation.headType = (LinkHeadType)shapeTag;
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

        void CreateManipulate(VdShapeType shapeType,
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

        void GetLinkables(Point pos, TouchDevice d)
        {
            Shape resizeNode = null;
            var sh = DocTools.DetectSelectedShape(_doc, pos, d, out resizeNode) as IVdShape;
            var end = DocTools.RequestLinkable(sh);
            if (end == null)
            {
                MessageBox.Show("Badges or clusters are accepted as link endpoints",
                                "Tip",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            if (linkCreation.end1 == null)
            {
                linkCreation.end1 = end;
            }
            else if (end == linkCreation.end1)
            {
                MessageBox.Show("Cannot link object with itself",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else
                linkCreation.end2 = end;
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
                    if (linkCreation.end1 != null)
                        ModeMgr.Mode = ShapeInputMode.LinkedObj2Expected;
                    return true;   
                case ShapeInputMode.LinkedObj2Expected:
                    GetLinkables(pos, touchDev);
                    if (linkCreation.end1 != null && linkCreation.end2 != null)
                    {
                        _doc.BeginCreateLink(linkCreation.end1.GetId(), linkCreation.end2.GetId(), linkCreation.headType);
                        linkCreation.end1 = null;
                        linkCreation.end2 = null;
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
                            cursorApproval.resizeNode = resizeNode;
                            cursorApproval.pos = pos;
                            cursorApproval.td = touchDev;
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

        void CaptureAndStartManip(IVdShape sh, Point pt, object sender, TouchDevice td)
        {
            touchDevice = td != null;

            sh.StartManip(pt, sender);
            sh.UnderlyingControl().CaptureMouse();
            if (td != null)
                sh.UnderlyingControl().CaptureTouch(td);
            Console.WriteLine("Scene Mgr : CaptureAndStartManip");
        }

        void ReleaseCaptureAndFinishManip(IVdShape sh)
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

        void localCursorChanged(CursorMgr mgr)
        {
            if (mgr.LocalCursor == null)
                return;

            //local cursor approval
            switch (_modeMgr.Mode)
            {
                case ShapeInputMode.CursorApprovalExpected:
                    _modeMgr.Mode = ShapeInputMode.Manipulating;
                    CaptureAndStartManip(mgr.LocalCursor, cursorApproval.pos, cursorApproval.resizeNode, cursorApproval.td);
                    mgr.LocalCursor.SetFocus();
                    break;                
            }
        }

        public void InpDeviceMove(Point pos)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;

            if (_modeMgr.Mode == ShapeInputMode.Manipulating && _doc.VolatileCtx.LocalCursor != null)
                BeginApplyPoint(_doc.VolatileCtx.LocalCursor, pos.X, pos.Y);
        }

        #region multitouch manipulations

        void ShapePostCtor(IVdShape sh, VdShapeType shapeType)
        {
            var ctl = sh.UnderlyingControl();
            ctl.IsManipulationEnabled = true;
            ctl.ManipulationStarting  += ManipulationStarting;
            ctl.ManipulationDelta     += ManipulationDelta;
            ctl.ManipulationCompleted += ManipulationCompleted;
          
            switch (shapeType)
            {
                case VdShapeType.Text:
                    ((VdText)sh).onChanged += onTextChanged;
                    break;
                case VdShapeType.Cluster:
                case VdShapeType.ClusterLink:
                    ((ICaptionHost)sh).InitCaptions(CaptionCreationRequested);
                    break;
            }           
        }

        void CaptionCreationRequested(CaptionType type, ICaptionHost host)
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
        
        void ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = _scene;
            e.Handled = true;

            var underContact = (IVdShape)((FrameworkElement)sender).Tag;

            underContact.ManipulationStarting(sender, e);
            Console.WriteLine("Scene Mgr : ManipulationStarting");
        }

        void ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if  (_doc.clusterRebuildPending)
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
                updateHostCaptions((VdCluster)localCurs);
        }
        void ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var localCurs = _doc.VolatileCtx.LocalCursor;
            if (localCurs != null)
            {
                localCurs.ManipulationCompleted(sender, e);
            }
        }
        void onTextChanged(VdText text)
        {          
            SendSyncState(text);
        }

        void SendSyncState(IVdShape sh)
        {
            _rt.clienRt.SendSyncState(sh.Id(), sh.GetState(_doc.TopicId));
        }

        void updateHostCaptions(ICaptionHost capHost)
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
                    updateHostCaptions((ICaptionHost)sh);
                    break;
            }        
        }

        void NotifyClusterableMoved(IVdShape sh)
        {
            if (sh.ShapeCode() == VdShapeType.Badge)
            {
                var clustShapes = _doc.GetShapes().Where(sh0 => sh0.ShapeCode() == VdShapeType.Cluster);
                //todo: optimization possible
                foreach (var clust in clustShapes)
                {
                    ((VdCluster)clust).ClusterableMoved(((VdBadge)sh).GetClusterable());
                }
            }
        }

        void syncStateEvent(ShapeState state)
        {
            if (state.topicId != _doc.TopicId)
                return;

            PlaySyncStateEvent(state);
        }

        void PlaySyncStateEvent(ShapeState state)
        {
            var sh = _doc.IdToShape(state.shapeId);
            if (sh == null)
            {
                //error of initialization (not all existing shapes were loaded to local station)
                return;
            }

            sh.ApplyState(state);
        }

        void ReloadBadgeContexts()
        {
            DbCtx.DropContext();
            DocTools.ToggleBadgeContexts(_doc.GetShapes().Where(sh => sh.ShapeCode() == VdShapeType.Badge));
        }

        void argPointChanged(int ArgPointId, int topicId, PointChangedType change)
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

        void inkStateEvent(InkMessage ink)
        {
            if (ink.topicId != _doc.TopicId)
                return;

            PlayInkEvent(ink);
        }

        void PlayInkEvent(InkMessage ink)
        {
            if (!_doc.ShapeVisibility)
                return;
            
            var s = new MemoryStream();
            s.Write(ink.inkData, 0, ink.inkData.Length);
            s.Position = 0;

            _ink.Strokes.Add(new StrokeCollection(s));
        }

        //void OnLocalInkChanged()
        //{
        //    sendLocalInk();
        //}

        void sendLocalInk()
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
        }
        #endregion photon events
    }
}

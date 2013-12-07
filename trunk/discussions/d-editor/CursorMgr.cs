using System;
using System.Linq;
using Discussions.DbModel.model;
using Discussions.rt;
using Discussions.RTModel.Model;

namespace DistributedEditor
{
    /*
         each shape can be free or can have one cursor
         any user can lock only one shape at a time 
         local owner of palette can change any time 
         when palette owner changes, cursor of previous palette owner, if any, is freed.
         each client can have any palette owner at any time. 
         the same palette owner on 2 or more machines is ok. 
     */

    public class CursorMgr : IDisposable
    {
        private VdDocument _doc;

        private UISharedRTClient _rt = UISharedRTClient.Instance;

        //owner selected currently in palette
        private IPaletteOwner _palette;

        public delegate void LocalCursorChanged(CursorMgr mgr);

        public LocalCursorChanged localCursorChanged;

        //has movement been detected during recent cursor ownership period ? 
        private bool _movementDetected = false;

        //if we are in large badge view mode, we skip the next cursor 
        public bool skipNextAquiredCursor;

        public void MovementDetected()
        {
            _movementDetected = true;
        }

        //has movement been detected during recent cursor ownership period ? 
        private bool _resizeDetected = false;

        public void ResizeDetected()
        {
            _resizeDetected = true;
        }

        public CursorMgr(VdDocument doc, IPaletteOwner palette)
        {
            _doc = doc;
            _palette = palette;

            setListeners(true);
        }

        public void Dispose()
        {
            setListeners(false);
        }

        //if local user (selected in palette) has cursor (only one), then it returns shape of the cursor 
        private IVdShape _localCursorShape;

        public IVdShape LocalCursor
        {
            get
            {
                if (_localCursorShape != null &&
                    _localCursorShape.GetCursor() != null &&
                    _localCursorShape.GetCursor().OwnerId == _palette.GetOwnerId() &&
                    _doc.Contains(_localCursorShape))
                    return _localCursorShape;
                else
                {
                    _localCursorShape = null;
                    return null;
                }
            }
            set { _localCursorShape = value; }
        }

        public IVdShape LocalFocus
        {
            get { return _doc.GetShapes().FirstOrDefault(sh => sh.IsFocused()); }
        }

        public void UnfocusAll()
        {
            var lf = LocalFocus;
            if (lf != null)
                lf.RemoveFocus();
        }

        /// <summary>
        /// call this when 
        /// 1. user removes contact points so that local cursor needs to be removed
        /// 2. palette owner changes and we invalidate previous cursor
        /// </summary>
        /// <param name="ownId"></param>
        public void BeginFreeCursor(bool supressClusterMoveEvent)
        {
            //if currently we hold cursor on shape, remove it
            var ownedSh = DocTools.CursorOwnerToShape(_palette.GetOwnerId(), _doc.GetShapes());
            if (ownedSh != null && ownedSh.GetCursor() != null)
            {
                _rt.clienRt.SendCursorRequest(false, _palette.GetOwnerId(), ownedSh.Id(), _doc.TopicId);

                if (_movementDetected)
                {
                    if (ownedSh is VdBadge)
                    {
                        _rt.clienRt.SendStatsEvent(StEvent.BadgeMoved,
                                                   ownedSh.GetCursor().OwnerId,
                                                   _doc.DiscussionId,
                                                   _doc.TopicId,
                                                   DeviceType.Wpf);
                    }
                    else if (!supressClusterMoveEvent && ownedSh is VdCluster)
                    {
                        _rt.clienRt.SendStatsEvent(StEvent.ClusterMoved,
                                                   ownedSh.GetCursor().OwnerId,
                                                   _doc.DiscussionId,
                                                   _doc.TopicId,
                                                   DeviceType.Wpf);
                    }
                    else if (ownedSh is VdFreeForm)
                    {
                        _rt.clienRt.SendStatsEvent(StEvent.FreeDrawingMoved,
                                                   ownedSh.GetCursor().OwnerId,
                                                   _doc.DiscussionId,
                                                   _doc.TopicId,
                                                   DeviceType.Wpf);
                    }
                }

                if (_resizeDetected)
                {
                    if (ownedSh is VdFreeForm)
                    {
                        _rt.clienRt.SendStatsEvent(StEvent.FreeDrawingResize,
                                                   ownedSh.GetCursor().OwnerId,
                                                   _doc.DiscussionId,
                                                   _doc.TopicId,
                                                   DeviceType.Wpf);
                    }
                }

                _localCursorShape = null;
                if (localCursorChanged != null)
                    localCursorChanged(this);
            }

            _movementDetected = false;
            _resizeDetected = false;
        }

        public void PlayFreeCursor(int ownId, int shapeId)
        {
            var freedShape = _doc.IdToShape(shapeId);
            if (freedShape != null)
                freedShape.UnsetCursor();
        }

        private bool cursorCaptureExpected = false;
        private bool pointUpEventDuringCursorCapture = false;

        public void NotifyPointUpEvent()
        {
            if (cursorCaptureExpected)
                pointUpEventDuringCursorCapture = true;
        }

        //the method is not called for just locally created shape. we don't know Id of the shape, 
        //but any new shape is automatically locked after its owner
        public void BeginTakeShapeWithLocalCursor(int shapeId)
        {
            cursorCaptureExpected = true;
            _rt.clienRt.SendCursorRequest(true, _palette.GetOwnerId(), shapeId, _doc.TopicId);
        }

        public void PlayTakeCursor(int owner, int shapeId)
        {
            cursorCaptureExpected = false;

            //if point up event is pending, and this cursor capture is our,  
            //we want to cancel cursor capture. locally it was not set, 
            //send cancellation to server 
            if (owner == _palette.GetOwnerId() && pointUpEventDuringCursorCapture)
            {
                pointUpEventDuringCursorCapture = false;
                _rt.clienRt.SendCursorRequest(false, owner, shapeId, _doc.TopicId);
                return;
            }

            var sh = _doc.IdToShape(shapeId);
            if (sh == null)
            {
                //error of initialization (not all existing shapes were loaded to local station)
                return;
            }
            if (sh.GetCursor() != null)
                return;

            sh.SetCursor(new Cursor(owner));

            //if owner is local, save local cursor
            if (owner == _palette.GetOwnerId())
            {
                _localCursorShape = sh;
                if (localCursorChanged != null)
                    localCursorChanged(this);
            }
        }

        private void cursorEvent(CursorEvent ev)
        {
            if (ev.topicId != _doc.TopicId)
                return;

            if (ev.doSet)
                PlayTakeCursor(ev.ownerId, ev.shapeId);
            else
                PlayFreeCursor(ev.ownerId, ev.shapeId);
        }

        #region photon events

        public void setListeners(bool doSet)
        {
            if (doSet)
                _rt.clienRt.cursorEvent += cursorEvent;
            else
                _rt.clienRt.cursorEvent -= cursorEvent;
        }

        #endregion photon events
    }
}
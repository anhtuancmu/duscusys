using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CustomCursor;
using Discussions;
using Discussions.RTModel.Model;
using Discussions.rt;

namespace DistributedEditor
{
    /// <summary>
    /// Lifetime: public board open, one topic is selected.
    /// </summary>
    public class LaserPointerWndCtx : IDisposable
    {
        private readonly Canvas _ptrCanvas;
        private readonly int _topicId;

        private bool _listenersSet;

        private readonly LaserCursorManager _laserCursorMgr;

        private readonly UISharedRTClient _rt = UISharedRTClient.Instance;

        private System.Windows.Point _recentSentPoint;

        private bool _localLazerEnabled;
        public bool LocalLazerEnabled
        {
            get { return _localLazerEnabled; }
            set
            {
                if (value == _localLazerEnabled)
                    return;
                _localLazerEnabled = value;

                if (_localLazerEnabled)
                {
                    _laserCursorMgr.AttachLaserPointer(LocalLaserPointer.UserId,
                                                       Utils.IntToColor(SessionInfo.Get().person.Color));
                    _laserCursorMgr.DisableStandardCursor(_ptrCanvas);
                    SetListeners(true);
                }
                else
                {
                    _laserCursorMgr.DetachLaserPointer(LocalLaserPointer.UserId);
                    _laserCursorMgr.EnableStandardCursor(_ptrCanvas);
                    _rt.clienRt.SendDetachLaserPointer(LocalLaserPointer);

                    SetListeners(false);
                }
            }
        }

        public LaserPointerWndCtx(Canvas ptrCanvas, int topicId)
        {
            _ptrCanvas = ptrCanvas;
            _topicId = topicId;

            _laserCursorMgr = new LaserCursorManager(ptrCanvas);
                   
            SetPhotonListeners(true);
        }

        public void Dispose()
        {
            LocalLazerEnabled = false;
            SetPhotonListeners(false);           
            _laserCursorMgr.Dispose();
        }

        private LaserPointer _localLaserPointer;
        private LaserPointer LocalLaserPointer
        {
            get
            {
                if (_localLaserPointer == null)
                {
                    _localLaserPointer = new LaserPointer
                        {
                            Color = SessionInfo.Get().person.Color,
                            TopicId = _topicId,
                            UserId = SessionInfo.Get().person.Id
                        };
                }
                return _localLaserPointer;
            }
        }

        void SetPhotonListeners(bool doSet)
        {
            if (doSet)
            {
                _rt.clienRt.onAttachLaserPointer += OnAttachLaserPointer;
                _rt.clienRt.onDetachLaserPointer += OnDetachLaserPointer;
                _rt.clienRt.onLaserPointerMoved  += OnLaserPointerMoved;
            }
            else
            {
                _rt.clienRt.onAttachLaserPointer -= OnAttachLaserPointer;
                _rt.clienRt.onDetachLaserPointer -= OnDetachLaserPointer;
                _rt.clienRt.onLaserPointerMoved  -= OnLaserPointerMoved;
            }
        }

        #region photon listeners
        private void OnLaserPointerMoved(LaserPointer ptr)
        {
            if (ptr.TopicId != _topicId)
                return;

            _laserCursorMgr.UpdatePointerLocation(ptr.UserId, new System.Windows.Point(ptr.X, ptr.Y));
        }

        private void OnDetachLaserPointer(LaserPointer ptr)
        {
            if (ptr.TopicId != _topicId)
                return;
            
            _laserCursorMgr.DetachLaserPointer(ptr.UserId);
        }

        private void OnAttachLaserPointer(LaserPointer ptr)
        {
            if (ptr.TopicId != _topicId)
                return;

            _laserCursorMgr.AttachLaserPointer(ptr.UserId, Utils.IntToColor(ptr.Color));
            _laserCursorMgr.UpdatePointerLocation(ptr.UserId, new System.Windows.Point(ptr.X, ptr.Y));
        }
        #endregion

        void SetListeners(bool doSet)
        {
            if (doSet == _listenersSet)
                return;
            _listenersSet = doSet;

            if (doSet)
                _ptrCanvas.TouchDown += ptrCanv_TouchDown;
            else
                _ptrCanvas.TouchDown -= ptrCanv_TouchDown;

            if (doSet)
                _ptrCanvas.TouchMove += ptrWnd_TouchMove;
            else
                _ptrCanvas.TouchMove -= ptrWnd_TouchMove;

            if (doSet)
                _ptrCanvas.TouchUp += ptrCanv_TouchUp;
            else
                _ptrCanvas.TouchUp -= ptrCanv_TouchUp;

            if (doSet)
                _ptrCanvas.MouseMove += ptrCanvasOnMouseMove;
            else
                _ptrCanvas.MouseMove -= ptrCanvasOnMouseMove;

            if (doSet)
                _ptrCanvas.MouseEnter += ptrCanvasOnMouseEnter;
            else
                _ptrCanvas.MouseEnter -= ptrCanvasOnMouseEnter;

            if (doSet)
                _ptrCanvas.MouseLeave += ptrCanvasOnMouseLeave;
            else
                _ptrCanvas.MouseLeave -= ptrCanvasOnMouseLeave;
        }

        void UpdateLocalPosition(System.Windows.Point p)
        {
            LocalLaserPointer.X = p.X;
            LocalLaserPointer.Y = p.Y;
        }

        private void ShowUpdatedLocalPosition()
        {
            _laserCursorMgr.UpdatePointerLocation(LocalLaserPointer.UserId,
                                                  new System.Windows.Point(LocalLaserPointer.X, LocalLaserPointer.Y)
                                                  );
        }

        void HandleDetach(InputEventArgs e)
        {
            if (e.Handled)
                return;

            //local
            _laserCursorMgr.DetachLaserPointer(LocalLaserPointer.UserId);

            //remote
            _rt.clienRt.SendDetachLaserPointer(LocalLaserPointer);

            e.Handled = true;
        }

        private void CheckSendLaserPointerMoved()
        {
            const int pointerMovingThreshold = 5;
            if (Math.Abs(LocalLaserPointer.X - _recentSentPoint.X) > pointerMovingThreshold ||
                Math.Abs(LocalLaserPointer.Y - _recentSentPoint.Y) > pointerMovingThreshold)
            {
                _recentSentPoint = new Point(LocalLaserPointer.X, LocalLaserPointer.Y);
                _rt.clienRt.SendLaserPointerMoved(LocalLaserPointer);
            }
        }

        void HandleAttach(Point p, InputEventArgs e)
        {
            if (e.Handled)
                return;

            //local
            _laserCursorMgr.AttachLaserPointer(LocalLaserPointer.UserId, 
                                               Utils.IntToColor(LocalLaserPointer.Color));
            UpdateLocalPosition(p);
            ShowUpdatedLocalPosition();

            //remote 
            _rt.clienRt.SendAttachLaserPointer(LocalLaserPointer);

            e.Handled = true;
        }

        void HandleMove(Point p, InputEventArgs e)
        {
            if (e.Handled)
                return;

            //local
            UpdateLocalPosition(p);
            ShowUpdatedLocalPosition();

            //remote
            CheckSendLaserPointerMoved();

            e.Handled = true;
        }

        #region handlers 
        private void ptrCanvasOnMouseLeave(object sender, MouseEventArgs e)
        {
            HandleDetach(e);
        }

        private void ptrCanvasOnMouseEnter(object sender, MouseEventArgs e)
        {
            HandleAttach(e.GetPosition(_ptrCanvas), e);
        }

        private void ptrCanvasOnMouseMove(object sender, MouseEventArgs e)
        {
            HandleMove(e.GetPosition(_ptrCanvas), e);
        }

        private void ptrCanv_TouchDown(object sender, TouchEventArgs e)
        {
            HandleAttach(e.GetTouchPoint(_ptrCanvas).Position, e);          
        }

        private void ptrWnd_TouchMove(object sender, TouchEventArgs e)
        {
            HandleMove(e.GetTouchPoint(_ptrCanvas).Position, e);
        }

        private void ptrCanv_TouchUp(object sender, TouchEventArgs e)
        {
            HandleDetach(e);
        }
        #endregion
    }
}
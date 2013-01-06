using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Input.Manipulations;

namespace DistributedEditor
{
    public class EditorWndCtx
    {
        private const TouchDevice NULL_TOUCH_DEVICE = null;

        private SceneManager mgr;

        public SceneManager SceneMgr
        {
            get { return mgr; }
        }

        private Canvas _canv;
        private DistributedInkCanvas _inkCanv;
        private Palette _palette;
        private InkPalette _inkPalette;

        private SurfaceWindow _keyboardWnd;

        private enum AnnotationMode
        {
            NoAnnotation,
            NewBeingEdited,
            EditingExisting
        };

        //makes manipulation starting event come after StartManip for point-oriented handling
        //DispatcherTimer poinManipDeferrer;

        private ManipulationProcessor2D _zoomManipProc;

        public ManipulationProcessor2D ZoomManipulator
        {
            get { return _zoomManipProc; }
        }

        private Dictionary<int, Manipulator2D> _manipulators = new Dictionary<int, Manipulator2D>();

        public EditorWndCtx(Canvas canv,
                            DistributedInkCanvas inkCanv,
                            Palette palette,
                            InkPalette inkPalette,
                            SurfaceWindow keyboardWnd,
                            int topicId,
                            int discussionId,
                            bool shapesVisibility)
        {
            _canv = canv;
            _inkCanv = inkCanv;
            _palette = palette;
            _inkPalette = inkPalette;
            _keyboardWnd = keyboardWnd;

            _zoomManipProc = new ManipulationProcessor2D(Manipulations2D.All);

            mgr = new SceneManager(canv, inkCanv, palette, inkPalette, topicId, discussionId, shapesVisibility);

            SetListeners(true);

            //poinManipDeferrer = new DispatcherTimer();
            //poinManipDeferrer.Interval = TimeSpan.FromMilliseconds(260);
            //poinManipDeferrer.Tick += manipDeferrerTick;
        }

        #region Timestamp

        private long Timestamp
        {
            get
            {
                // Get timestamp in 100-nanosecond units               
                double nanosecondsPerTick = 1000000000.0/System.Diagnostics.Stopwatch.Frequency;
                return (long) (System.Diagnostics.Stopwatch.GetTimestamp()/nanosecondsPerTick/100.0);
            }
        }

        #endregion

        public void CleanupScene()
        {
            this.SetListeners(false);

            if (mgr.Doc != null)
            {
                mgr.Doc.DetachFromCanvas();
                mgr.Doc.setListeners(false);
            }

            _inkCanv.Strokes.Clear();
            mgr.FinishFreeDrawing();

            mgr.setListeners(false);
            mgr = null;

            _palette.ResetOvers();
        }

        public void ShapesVisility(bool visible)
        {
            if (visible)
            {
                SceneMgr.Doc.ShowShapes();
                _palette.Visibility = Visibility.Visible;
                _palette.IsHitTestVisible = true;
            }
            else
            {
                _inkCanv.Strokes.Clear();
                SceneMgr.FinishFreeDrawing();
                SceneMgr.Doc.HideShapes();
                _palette.Visibility = Visibility.Hidden;
                _palette.IsHitTestVisible = false;
                _palette.ResetOvers();
            }
        }

        public SceneManager getMgr()
        {
            return mgr;
        }

        public void SetListeners(bool doSet)
        {
            if (delCanv_DeleteText == null)
                delCanv_DeleteText = new RoutedEventHandler(canv_DeleteText);

            if (delTextShapeCopy == null)
                delTextShapeCopy = new RoutedEventHandler(textShapeCopy);

            if (delTextShapePaste == null)
                delTextShapePaste = new RoutedEventHandler(textShapePaste);

            //if(doSet)
            //    UISharedRTClient.Instance.clienRt.onStatsEvent += OnStatsEvent;
            //else
            //    UISharedRTClient.Instance.clienRt.onStatsEvent -= OnStatsEvent;   


            if (doSet)
            {
                _palette.toolSelected += this.ToolSelected;
                _palette.removeShape += this.RemoveShape;
                _palette.noTool += this.NoTool;
                _palette.reset += this.Reset;
            }
            else
            {
                _palette.toolSelected -= this.ToolSelected;
                _palette.removeShape -= this.RemoveShape;
                _palette.noTool -= this.NoTool;
                _palette.reset -= this.Reset;
            }

            if (doSet)
            {
                _canv.AddHandler(TextUC.VdTextDeleteEvent, delCanv_DeleteText);
                //_canv.AddHandler(TextUC.TextShapeCopyEvent, delTextShapeCopy);
                //_canv.AddHandler(TextUC.TextShapePasteEvent, delTextShapePaste);

                _canv.TouchDown += canv_TouchDown;
                _canv.TouchMove += canv_TouchMove;
                _canv.TouchUp += canv_TouchUp;

                _keyboardWnd.TouchDown += wnd_TouchDown;
                _keyboardWnd.TouchMove += wnd_TouchMove;
                _keyboardWnd.TouchUp += wnd_TouchUp;

                _canv.MouseDown += canv_MouseLeftButtonDown;
                _canv.MouseMove += canv_MouseMove;
                _canv.MouseUp += canv_MouseUp;

                _canv.MouseWheel += canv_Wheel;
            }
            else
            {
                _canv.RemoveHandler(TextUC.VdTextDeleteEvent, delCanv_DeleteText);
                //_canv.RemoveHandler(TextUC.TextShapeCopyEvent, delTextShapeCopy);
                //_canv.RemoveHandler(TextUC.TextShapePasteEvent, delTextShapePaste);                

                _canv.MouseDown -= canv_MouseLeftButtonDown;
                _canv.MouseMove -= canv_MouseMove;
                _canv.MouseUp -= canv_MouseUp;

                _keyboardWnd.TouchDown -= wnd_TouchDown;
                _keyboardWnd.TouchMove -= wnd_TouchMove;
                _keyboardWnd.TouchUp -= wnd_TouchUp;

                _canv.TouchDown -= canv_TouchDown;
                _canv.TouchMove -= canv_TouchMove;
                _canv.TouchUp -= canv_TouchUp;

                _canv.MouseWheel -= canv_Wheel;
            }
        }

        //now only used for public board. we are in free form drawing, and switching to read-only mode

        private Delegate delCanv_DeleteText = null;

        private void canv_DeleteText(object sender, RoutedEventArgs e)
        {
            mgr.RemoveShape(-1);
        }

        private Delegate delTextShapeCopy = null;

        private void textShapeCopy(object sender, RoutedEventArgs e)
        {
            //// mgr.CopySelected();
        }

        private Delegate delTextShapePaste = null;

        private void textShapePaste(object sender, RoutedEventArgs e)
        {
            /// mgr.PasteCopied();
        }

        public void RemoveShape(int owner)
        {
            mgr.RemoveShape(owner);
        }

        private void NoTool(int owner)
        {
            //// checkInjectNewOwner(owner);

            /// mgr.ExitShapeCreationMode();
        }

        private void TryAddManipulator(TouchDevice td)
        {
            if (_manipulators.ContainsKey(td.Id))
                return;

            var pos = td.GetPosition(_keyboardWnd);
            _manipulators.Add(td.Id, new Manipulator2D(td.Id, (float) pos.X, (float) pos.Y));

            var pivot = new ManipulationPivot2D();
            if (_manipulators.Count > 1)
            {
                pivot.X = (_manipulators.Values.First().X + _manipulators.Values.Last().X)/2;
                pivot.Y = (_manipulators.Values.First().Y + _manipulators.Values.Last().Y)/2;
            }
            else
            {
                pivot.X = (float) pos.X;
                pivot.Y = (float) pos.Y;
            }
            pivot.Radius = (float) 1.0;
            _zoomManipProc.Pivot = pivot;
        }

        private void TryRemoveManipulator(TouchDevice td)
        {
            if (!_manipulators.ContainsKey(td.Id))
                return;

            _manipulators.Remove(td.Id);
        }

        #region mouse

        private void canv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sceneProcessedAction)
                return;

            mgr.InpDeviceDown(Mouse.GetPosition(_canv), NULL_TOUCH_DEVICE);
            //e.Handled = true; 
        }

        private void canv_MouseMove(object sender, MouseEventArgs e)
        {
            if (sceneProcessedAction)
                return;
            mgr.InpDeviceMove(Mouse.GetPosition(_canv));
            //e.Handled = true; 
        }

        private void canv_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sceneProcessedAction)
                return;
            mgr.InpDeviceUp(Mouse.GetPosition(_canv));
            Keyboard.Focus(_keyboardWnd);
            //e.Handled = true; 
        }

        private void ToolSelected(VdShapeType shape, int shapeTag, int owner)
        {
            mgr.EnterShapeCreationMode(shape, shapeTag);
        }

        #endregion

        #region touch   

        private bool sceneProcessedAction = false;

        private void wnd_TouchDown(object sender, TouchEventArgs e)
        {
            TryAddManipulator(e.TouchDevice);
            _zoomManipProc.ProcessManipulators(this.Timestamp, _manipulators.Values);
        }

        private void canv_TouchDown(object sender, TouchEventArgs e)
        {
            if (mgr.InpDeviceDown(e.TouchDevice.GetPosition(_canv), e.TouchDevice))
            {
                // e.Handled = true;
                sceneProcessedAction = true;
            }
        }

        private void wnd_TouchMove(object sender, TouchEventArgs e)
        {
            //any touch events from ink canvas are ignored, as they are drawing events, InkCanvasSelectionAdorner, MS Internal
            if (!sceneProcessedAction && !(e.OriginalSource is Adorner))
            {
                //force update by remove/add
                TryRemoveManipulator(e.TouchDevice);
                TryAddManipulator(e.TouchDevice);
                _zoomManipProc.ProcessManipulators(this.Timestamp, _manipulators.Values);
            }
        }

        private void canv_TouchMove(object sender, TouchEventArgs e)
        {
            mgr.InpDeviceMove(e.TouchDevice.GetPosition(_canv));
            //e.Handled = true; 
        }

        private void wnd_TouchUp(object sender, TouchEventArgs e)
        {
            //TryRemoveManipulator(e.TouchDevice);
            _manipulators.Clear();
            _zoomManipProc.ProcessManipulators(this.Timestamp, _manipulators.Values);
        }

        private void canv_TouchUp(object sender, TouchEventArgs e)
        {
            mgr.InpDeviceUp(e.TouchDevice.GetPosition(_canv));
            Keyboard.Focus(_keyboardWnd);
            //  e.Handled = true;
            sceneProcessedAction = false;
        }

        public void CopySelected()
        {
            /// mgr.CopySelected();
        }

        public void PasteSelected()
        {
            //// mgr.PasteCopied();
        }

        #endregion

        private void canv_Wheel(object sender, MouseWheelEventArgs e)
        {
            /// mgr.ScaleInPlace(e.Delta > 0);
            //e.Handled = true;
        }

        private void Reset(int owner)
        {
            mgr.RemoveOwnShapes(owner);
        }

        //this is only to capture commentAdded notification to set notification visuals 
        //void OnStatsEvent(StEvent e, int userId, int discussionId, int topicId, DeviceType devType)
        //{
        //    if(e==StEvent.)
        //    ShowRecentEvent(new EventViewModel(e, userId, DateTime.Now, devType));
        //}
    }
}
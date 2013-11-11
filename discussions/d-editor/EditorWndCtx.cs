using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using AbstractionLayer;
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

        private readonly Canvas _canv;
        private readonly DistributedInkCanvas _inkCanv;
        private readonly Palette _palette;
        private InkPalette _inkPalette;


        private readonly PortableWindow _keyboardWnd;

        private bool _listenersSet;

        //makes manipulation starting event come after StartManip for point-oriented handling
        //DispatcherTimer poinManipDeferrer;

        private readonly ManipulationProcessor2D _zoomManipProc;
        public ManipulationProcessor2D ZoomManipulator
        {
            get { return _zoomManipProc; }
        }

        private readonly Dictionary<int, Manipulator2D> _manipulators = new Dictionary<int, Manipulator2D>();        


        public EditorWndCtx(Canvas canv,
                            DistributedInkCanvas inkCanv,
                            Palette palette,
                            InkPalette inkPalette,
                            PortableWindow keyboardWnd,
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
            if (doSet == _listenersSet)
                return;
            _listenersSet = doSet;

            if (delCanv_DeleteText == null)
                delCanv_DeleteText = new RoutedEventHandler(canv_DeleteText);

            if (delTextShapeCopy == null)
                delTextShapeCopy = new RoutedEventHandler(textShapeCopy);

            if (delTextShapePaste == null)
                delTextShapePaste = new RoutedEventHandler(textShapePaste);
           
            if (doSet)
            {
                _palette.toolSelected += this.ToolSelected;
                _palette.removeShape += this.RemoveShape;
                _palette.reset += this.Reset;
            }
            else
            {
                _palette.toolSelected -= this.ToolSelected;
                _palette.removeShape -= this.RemoveShape;
                _palette.reset -= this.Reset;
            }

            if (doSet)
            {
                _canv.AddHandler(TextUC.VdTextDeleteEvent, delCanv_DeleteText);
                //_canv.AddHandler(TextUC.TextShapeCopyEvent, delTextShapeCopy);
                //_canv.AddHandler(TextUC.TextShapePasteEvent, delTextShapePaste);

                _canv.TouchDown += drawingCanv_TouchDown;
                _canv.TouchMove += drawingCanv_TouchMove;
                _canv.TouchUp += drawingCanv_TouchUp;

                _keyboardWnd.TouchDown += drawingWnd_TouchDown;
                _keyboardWnd.TouchMove += drawingWnd_TouchMove;
                _keyboardWnd.TouchUp += drawingWnd_TouchUp;

                _canv.MouseDown += drawingCanv_MouseLeftButtonDown;
                _canv.MouseMove += drawingCanv_MouseMove;
                _canv.MouseUp += drawingCanv_MouseUp;

                _canv.MouseWheel += drawingCanv_Wheel;
            }
            else
            {
                _canv.RemoveHandler(TextUC.VdTextDeleteEvent, delCanv_DeleteText);
                //_canv.RemoveHandler(TextUC.TextShapeCopyEvent, delTextShapeCopy);
                //_canv.RemoveHandler(TextUC.TextShapePasteEvent, delTextShapePaste);                

                _canv.MouseDown -= drawingCanv_MouseLeftButtonDown;
                _canv.MouseMove -= drawingCanv_MouseMove;
                _canv.MouseUp -= drawingCanv_MouseUp;

                _keyboardWnd.TouchDown -= drawingWnd_TouchDown;
                _keyboardWnd.TouchMove -= drawingWnd_TouchMove;
                _keyboardWnd.TouchUp -= drawingWnd_TouchUp;

                _canv.TouchDown -= drawingCanv_TouchDown;
                _canv.TouchMove -= drawingCanv_TouchMove;
                _canv.TouchUp -= drawingCanv_TouchUp;

                _canv.MouseWheel -= drawingCanv_Wheel;
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
            // mgr.CopySelected();
        }

        private Delegate delTextShapePaste = null;

        private void textShapePaste(object sender, RoutedEventArgs e)
        {
            // mgr.PasteCopied();
        }

        public void RemoveShape(int owner)
        {
            mgr.RemoveShape(owner);
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

        private void drawingCanv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sceneProcessedAction)
                return;

            mgr.InpDeviceDown(Mouse.GetPosition(_canv), NULL_TOUCH_DEVICE);
        }

        private void drawingCanv_MouseMove(object sender, MouseEventArgs e)
        {
            if (sceneProcessedAction)
                return;
            mgr.InpDeviceMove(Mouse.GetPosition(_canv));
        }

        private void drawingCanv_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sceneProcessedAction)
                return;
            mgr.InpDeviceUp(Mouse.GetPosition(_canv));
            Keyboard.Focus(_keyboardWnd);
        }

        private void ToolSelected(VdShapeType shape, int shapeTag, int owner)
        {
            mgr.EnterShapeCreationMode(shape, shapeTag);
        }

        #endregion

        #region touch   

        private bool sceneProcessedAction;

        private void drawingWnd_TouchDown(object sender, TouchEventArgs e)
        {
            TryAddManipulator(e.TouchDevice);
            _zoomManipProc.ProcessManipulators(this.Timestamp, _manipulators.Values);
        }

        private void drawingCanv_TouchDown(object sender, TouchEventArgs e)
        {
            if (mgr.InpDeviceDown(e.TouchDevice.GetPosition(_canv), e.TouchDevice))
            {
                sceneProcessedAction = true;
            }
        }

        private void drawingWnd_TouchMove(object sender, TouchEventArgs e)
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

        private void drawingCanv_TouchMove(object sender, TouchEventArgs e)
        {
            mgr.InpDeviceMove(e.TouchDevice.GetPosition(_canv));
        }

        private void drawingWnd_TouchUp(object sender, TouchEventArgs e)
        {
            //TryRemoveManipulator(e.TouchDevice);
            _manipulators.Clear();
            _zoomManipProc.ProcessManipulators(this.Timestamp, _manipulators.Values);
        }

        private void drawingCanv_TouchUp(object sender, TouchEventArgs e)
        {
            mgr.InpDeviceUp(e.TouchDevice.GetPosition(_canv));
            Keyboard.Focus(_keyboardWnd);
            sceneProcessedAction = false;
        }

        public void CopySelected()
        {
            // mgr.CopySelected();
        }

        public void PasteSelected()
        {
            // mgr.PasteCopied();
        }

        #endregion

        private void drawingCanv_Wheel(object sender, MouseWheelEventArgs e)
        {
            // mgr.ScaleInPlace(e.Delta > 0);
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
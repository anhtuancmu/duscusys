using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.IO;
using Discussions.DbModel;
using Discussions;
using System.Windows.Ink;

namespace DistributedEditor
{
    public class EditorWndCtx 
    {
        const TouchDevice NULL_TOUCH_DEVICE = null;

        SceneManager mgr;
        public SceneManager SceneMgr
        {
            get
            {
                return mgr;
            }
        }

        Canvas _canv;
        DistributedInkCanvas _inkCanv;
        Palette _palette;
        InkPalette _inkPalette;

        SurfaceWindow _keyboardWnd;

        ContactTimer touchTimer;

        enum AnnotationMode { NoAnnotation, NewBeingEdited, EditingExisting };

        public EditorWndCtx(Canvas canv, 
                            DistributedInkCanvas inkCanv,
                            Palette palette,
                            InkPalette inkPalette,                     
                            SurfaceWindow keyboardWnd,
                            int topicId,
                            int discussionId)
        {
            _canv = canv;
            _inkCanv = inkCanv;
            _palette = palette;
            _inkPalette = inkPalette;
            _keyboardWnd = keyboardWnd;

            mgr = new SceneManager(canv, inkCanv, palette, inkPalette,  topicId, discussionId);
         
            touchTimer = new ContactTimer(DevDownAsMouse, 0.05, false);

            SetListeners(true); 
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

            if(doSet)
            {
                //_canv.AddHandler(TextUC.VdTextDeleteEvent, delCanv_DeleteText);
                //_canv.AddHandler(TextUC.TextShapeCopyEvent, delTextShapeCopy);
                //_canv.AddHandler(TextUC.TextShapePasteEvent, delTextShapePaste);

                _canv.TouchDown += canv_TouchDown;
                _canv.TouchMove += canv_TouchMove;
                _canv.TouchUp   += canv_TouchUp;
                
                _canv.MouseDown += canv_MouseLeftButtonDown;
                _canv.MouseMove += canv_MouseMove;
                _canv.MouseUp   += canv_MouseUp;

                _canv.MouseWheel += canv_Wheel;
            }
            else
            {
                //_canv.RemoveHandler(TextUC.VdTextDeleteEvent, delCanv_DeleteText);
                //_canv.RemoveHandler(TextUC.TextShapeCopyEvent, delTextShapeCopy);
                //_canv.RemoveHandler(TextUC.TextShapePasteEvent, delTextShapePaste);                

                _canv.MouseDown -= canv_MouseLeftButtonDown;
                _canv.MouseMove -= canv_MouseMove;
                _canv.MouseUp   -= canv_MouseUp;

                _canv.TouchDown -= canv_TouchDown;
                _canv.TouchMove -= canv_TouchMove;
                _canv.TouchUp   -= canv_TouchUp;

                _canv.MouseWheel -= canv_Wheel;
            }
        }

        #region API
        //screenshot==null for no bg
        public void CreateAnnotation(int canvWidth, int canvHeight)
        {
            //currentAnnotation = null;

            //mgr.Doc.DetachFromCanvas();
            //doc = new VdDocument(_palette, _canv);
            //mgr = new SceneManager(_canv, doc, _palette);                             

            //_canv.Width = canvWidth;
            //_canv.Height = canvHeight;
          
            //annotationMode = AnnotationMode.NewBeingEdited;
        }

        //public void LoadAnnotation(Annotation a)
        //{
        //    currentAnnotation = a;
        //    edtCtxBgPathName = null;

        //    doc.DetachFromCanvas(_canv);

        //    doc = new VdDocument(_canv, this, _drawStatsOper);
        //    mgr = new SceneManager(_canv, doc, _lblText, _ownerOfNewShapes, false, _palette);
        //    doc.mgr = mgr;
        //    doc.clustMgr = mgr.GetClustMgr();

        //    ReregisterClusterables(_clusterablesProvider());

        //    doc.Read(a.VectGraphics);

        //    _canv.Width = a.CanvWidth;
        //    _canv.Height = a.CanvHeight;

        //    if (a.Bg!=null)
        //    {
        //        BitmapImage bg = MiniAttachmentManager.LoadImageFromBlob(a.Bg);                          
        //         _canv.Background = new ImageBrush(bg);
        //    }

        //    doc.UnselectAll();            

        //    annotationMode = AnnotationMode.EditingExisting;
        //}

        //const double THUMBNAIL_DPI = 50;
        //public Annotation Save(Visual visualForThumb)
        //{
        //    //new annotation
        //    if (annotationMode == AnnotationMode.NewBeingEdited)
        //    {
        //        var id = new InpDialog("Annotation name?","<annotation>");
        //        id.ShowDialog();
        //        if (id.Answer == null)
        //            return null;
                
        //        BusyWndSingleton.Show("Uploading annotations, please wait...");
        //        try
        //        {                    
        //            currentAnnotation = DaoUtils.UploadAnnotation(SessionInfo.Get().person,
        //                                                          SessionInfo.Get().discussion,
        //                                                          edtCtxBgPathName,
        //                                                          _newAnnotCanvWidth, _newAnnotCanvHeight,
        //                                                          id.Answer,
        //                                                          Screenshot.Take(visualForThumb, THUMBNAIL_DPI),
        //                                                          doc);
        //            CtxSingleton.Get().SaveChanges();
        //        }
        //        finally
        //        {
        //            BusyWndSingleton.Hide();                
        //        }
        //        annotationMode = AnnotationMode.EditingExisting;
        //    }
        //    else if (annotationMode == AnnotationMode.EditingExisting)
        //    {
        //        //save changes in existing annotation 
        //        BusyWndSingleton.Show("Saving changes, please wait...");
        //        try
        //        {
        //            DaoUtils.UpdateAnnotation(currentAnnotation, Screenshot.Take(visualForThumb, THUMBNAIL_DPI), doc);
        //            CtxSingleton.Get().SaveChanges();
        //        }
        //        finally
        //        {
        //            BusyWndSingleton.Hide();                   
        //        }
        //    }

        //    return currentAnnotation;
        //}

        //now only used for public board. we are in free form drawing, and switching to read-only mode
        public void ToReadOnlyMode()
        {
            if (_inkCanv.Visibility == Visibility.Visible)
            {
                _inkCanv.Strokes.Clear();
                switchDrawingMode(false);                              
            }
        }
        #endregion 

        Delegate delCanv_DeleteText = null;
        void canv_DeleteText(object sender, RoutedEventArgs e)
        {
           //// mgr.DeleteRecentlySelectedShape();
        }

        Delegate delTextShapeCopy = null;
        void textShapeCopy(object sender, RoutedEventArgs e)
        {
           //// mgr.CopySelected();
        }

        Delegate delTextShapePaste = null;
        void textShapePaste(object sender, RoutedEventArgs e)
        {
           /// mgr.PasteCopied();
        }

        public void RemoveShape(int owner)
        {          
            mgr.RemoveShape(owner);
        }

        void NoTool(int owner)
        {
           //// checkInjectNewOwner(owner);
            
           /// mgr.ExitShapeCreationMode();
        }
       
        #region mouse
        private void canv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (recentTouchEventClose() || e.Handled)
               return;

            touchTimer.Stop();
            touchTimer.Run();           
        }

        void DevDownAsMouse(object sender, EventArgs e)
        {
            touchTimer.Stop();
            mgr.InpDeviceDown(Mouse.GetPosition(_canv), NULL_TOUCH_DEVICE);            
        }

        private void canv_MouseMove(object sender, MouseEventArgs e)
        {
            if (recentTouchEventClose() || e.Handled)
               return;
              
            mgr.InpDeviceMove(Mouse.GetPosition(_canv));            
        }

        private void canv_MouseUp(object sender, MouseButtonEventArgs e)
        {
            touchTimer.Stop();
            if (recentTouchEventClose())
                return;

            mgr.InpDeviceUp(Mouse.GetPosition(_canv));
            Keyboard.Focus(_keyboardWnd);            
        }

        void ToolSelected(VdShapeType shape, int owner)
        {
            mgr.EnterShapeCreationMode(shape);
        }
        #endregion

        #region touch
        DateTime recentTouchEvent = DateTime.Now;
        bool recentTouchEventClose()
        {                   
            var res = DateTime.Now.Subtract(recentTouchEvent).TotalMilliseconds < 500;
            return res;
        }

        private void canv_TouchDown(object sender, TouchEventArgs e)
        {
            if (e.Handled)
                return;
            recentTouchEvent = DateTime.Now;
            touchTimer.Stop();
            mgr.InpDeviceDown(e.TouchDevice.GetPosition(_canv), e.TouchDevice);            
        }

        private void canv_TouchMove(object sender, TouchEventArgs e)
        {
            if (e.Handled)
                return;
            recentTouchEvent = DateTime.Now;
            mgr.InpDeviceMove(e.TouchDevice.GetPosition(_canv));            
        }

        private void canv_TouchUp(object sender, TouchEventArgs e)
        {
            if (e.Handled)
                return;
            recentTouchEvent = DateTime.Now;
            mgr.InpDeviceUp(e.TouchDevice.GetPosition(_canv));
            Keyboard.Focus(_keyboardWnd);            
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

        void switchDrawingMode(bool toDrawing)
        {
            if (toDrawing)
            {
                _inkCanv.Strokes.Clear();
                _inkCanv.Visibility = Visibility.Visible;
                _inkCanv.IsHitTestVisible = true;
            }
            else
            {
                _inkCanv.Visibility = Visibility.Hidden;
                _inkCanv.IsHitTestVisible = false;
            }
        }

        private void canv_Wheel(object sender, MouseWheelEventArgs e)
        {
           /// mgr.ScaleInPlace(e.Delta > 0);
            e.Handled = true;
        }

        void Reset(int owner)
        {
            mgr.RemoveOwnShapes(owner);
        }
    }
}

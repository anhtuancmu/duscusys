using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Surface.Presentation.Controls.TouchVisualizations;
using System.Linq;

namespace CustomCursor
{
    public class LaserCursorManager : IDisposable 
    {
        private readonly Canvas _visualizationCanvas;
        private readonly List<LaserPointerUC> _attachedPointers = new List<LaserPointerUC>();

        public LaserCursorManager(Canvas visualizationCanvas)
        {
            _visualizationCanvas = visualizationCanvas;
        }

        public void Dispose()
        {
            foreach (var ptr in _attachedPointers)
                _visualizationCanvas.Children.Remove(ptr);
            _attachedPointers.Clear();
        }

        public void DisableStandardCursor(FrameworkElement elem)
        {
            elem.Cursor = Cursors.None;
        }

        public void EnableStandardCursor(FrameworkElement elem)
        {
            elem.Cursor = Cursors.Arrow;
        }

        public void SetTouchVisualizationColors(FrameworkElement elem,  Color clr)
        {          
            TouchVisualizer.SetVisualizationColor1(elem, clr);
            TouchVisualizer.SetVisualizationColor2(elem, clr);
            TouchVisualizer.SetVisualizationColor3(elem, Colors.White);
        }

        public void SetLaserColor(int pointerId, Color clr)
        {
            var ptrToUpdate = _attachedPointers.FirstOrDefault(ptr => (int)ptr.Tag == pointerId);
            if (ptrToUpdate != null)
            {
                ptrToUpdate.SetColor(clr); 
            }
        }

        public void AttachLaserPointer(int pointerId, Color clr)
        {
            if (_attachedPointers.FirstOrDefault(ptr => (int)ptr.Tag == pointerId) == null)
            {
                var newPtr = new LaserPointerUC {Tag = pointerId, Width = 20, Height = 20};
                Panel.SetZIndex(newPtr, int.MaxValue);                
                newPtr.SetColor(clr);
                _attachedPointers.Add(newPtr);
                _visualizationCanvas.Children.Add(newPtr);
            }
        }

        public void DetachLaserPointer(int pointerId)
        {
            var ptrToRemove = _attachedPointers.FirstOrDefault(ptr => (int)ptr.Tag == pointerId);
            if (ptrToRemove!=null)
            {
                _attachedPointers.Remove(ptrToRemove);
                _visualizationCanvas.Children.Remove(ptrToRemove);
            }
        }

        public void UpdatePointerLocation(int pointerId, Point pointerCenter)
        {
            var ptrToUpdate = _attachedPointers.FirstOrDefault(ptr => (int)ptr.Tag == pointerId);
            if (ptrToUpdate != null)
            {
                ptrToUpdate.SetValue(Canvas.LeftProperty, pointerCenter.X - 0.5 * ptrToUpdate.ActualWidth);
                ptrToUpdate.SetValue(Canvas.TopProperty, pointerCenter.Y - 0.5 * ptrToUpdate.ActualHeight);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Presentation.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Discussions
{
    public class DragCursorState
    {
        public DragCursorState(ContentControl draggedItem, //ScatterViewItem or ListViewItem
                               FrameworkElement DragSrc,
                               ObservableCollection<object> srcCollection)
        {
            this.draggedItem = draggedItem;
            this.DragSrc = DragSrc;
            this.srcCollection = srcCollection;
        }
        public object draggedItem; //ScatterViewItem or ListBoxItem
        public object DragSrc; //ScatterView or BadgeFolder
        public ObservableCollection<object> srcCollection;

        //filled during target changed
        public enum OperationType { None, MoveToGroup, MoveFromGroup, MergeWith, ResolveAgreement };
        public OperationType Operation;
        public object currentTarget;
        public void SetOperation(OperationType Operation, object currentTarget)
        {
            this.Operation = Operation;
            this.currentTarget = currentTarget;
        }
    }
}

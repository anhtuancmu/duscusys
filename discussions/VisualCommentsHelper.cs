using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;
using Discussions.DbModel;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions
{
    class VisualCommentsHelper
    {
        ItemContainerGenerator _gen = null;
        Comment _placeholder = null;

        public VisualCommentsHelper(Dispatcher disp, ItemContainerGenerator gen, Comment placeholder)
        {
            _gen = gen;
            _placeholder = placeholder;

            disp.BeginInvoke(new Action(DeferredFocusSet),
                            System.Windows.Threading.DispatcherPriority.Background, null);
        }

        void DeferredFocusSet()
        {
            var newItem = _gen.ContainerFromItem(_placeholder);
            var commentText = Utils.FindChild<SurfaceTextBox>(newItem);
            if (commentText != null)
                commentText.Focus();
        }
    }
}

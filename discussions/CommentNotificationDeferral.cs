using System;
using System.Windows.Controls;
using System.Windows.Threading;
using Discussions.DbModel;
using Discussions.view;

namespace Discussions
{
    /// <summary>
    /// ArgPoint.Comment contains collection of arg.point's comments. In both private and public board 
    /// lists of comments bind to ArgPoint.Comment. In order not to intervene that and not to add separate 
    /// view model for Comment, we control individual per comment notifications from this class. As items controls
    /// can have not all comment items initialized (after DataContent change), the update is deferred. 
    /// </summary>
    class CommentNotificationDeferral
    {
        private readonly Dispatcher _disp;
        private readonly DiscCtx _ctx;
        private readonly ItemsControl _commentItems;

        public CommentNotificationDeferral(Dispatcher disp, DiscCtx ctx, ItemsControl commentItems)
        {
            _disp = disp;
            _ctx = ctx;
            _commentItems = commentItems;

            _disp.BeginInvoke(new Action(InjectCommentNotifications),
                              System.Windows.Threading.DispatcherPriority.Background);
        }     

        void InjectCommentNotifications()
        {
            for (int i = 0; i < _commentItems.Items.Count; ++i)
            {
                var item = _commentItems.ItemContainerGenerator.ContainerFromIndex(i);
                var commentUC = Utils.FindChild<CommentUC>(item);
                if (commentUC != null)
                {
                    var comment = (Comment)commentUC.DataContext;
                    if (comment != null)
                    {
                        commentUC.SetNotification(false); 
                        if (DaoUtils.IsCommentNewForUs(_ctx, comment.Id))
                            commentUC.SetNotification(true);
                    }
                }                
            }
        }
    }
}

using System.Windows;

namespace Discussions
{
    public class CommentEditabilityChanged : RoutedEventArgs
    {
        public CommentEditabilityChanged(RoutedEvent re, bool IsBeingEdited) :
            base(re)
        {
            this.IsBeingEdited = IsBeingEdited;
        }

        public bool IsBeingEdited { get; set; }
    }
}
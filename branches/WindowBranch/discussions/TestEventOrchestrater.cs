using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Discussions
{
    public class TestEventOrchestrater
    {
        private enum TestAction
        {
            FocusedAndEdited,
            Submitted,
            Removed,
            None
        };        

        private static TestEventOrchestrater _inst;

        public static TestEventOrchestrater Inst
        {
            get
            {
                if (_inst == null)
                    _inst = new TestEventOrchestrater();
                return _inst;
            }
        }

        private DispatcherTimer _timer;

        private EditableBadge _editBadge;

        private PrivateCenter3 _privCenter;

        private TestAction _prevAction;


        public void Start(EditableBadge editBadge, PrivateCenter3 privCenter)
        {
            _editBadge = editBadge;
            _privCenter = privCenter;
            _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(2)};
            _timer.Tick += TimerOnTick;
            _timer.Start();
        }        

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            switch (_prevAction)
            {
                case TestAction.FocusedAndEdited:
                    Submit();
                    break;
                case TestAction.Submitted:
                    Remove();
                    break;
                case TestAction.None:
                    FocusAndEnter();
                    break;
                case TestAction.Removed:
                    FocusAndEnter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void Submit()
        {
            _privCenter.btnSave_Click(null,null);

            _prevAction = TestAction.Submitted;            
        }

        void FocusAndEnter()
        {
            var comment = GetCommentTextBox();
            if (comment == null)
                return;

            comment.Focus();

            var rnd = new Random();
            comment.Text = "comment" + rnd.Next(100);

            _prevAction = TestAction.FocusedAndEdited;  
        }

        void Remove()
        {
            var lastContainer = _editBadge.TestGetCommentsControl().ItemContainerGenerator.ContainerFromIndex(
              _editBadge.TestGetCommentsControl().Items.Count - 3);

            if (lastContainer == null)
                return;

            var commentUc = Utils.FindChild<CommentUC>(lastContainer);

            commentUc.btnRemoveComment_Click(null, null);

            _privCenter.btnSave_Click(null, null);

            _prevAction = TestAction.Removed;
        }

        TextBox GetCommentTextBox()
        {
            var lastContainer = _editBadge.TestGetCommentsControl().ItemContainerGenerator.ContainerFromIndex(
              _editBadge.TestGetCommentsControl().Items.Count - 1);

            if (lastContainer == null)
                return null;
            
            var txtBox = Utils.FindChild<TextBox>(lastContainer);
            return txtBox;
        }
    }
}
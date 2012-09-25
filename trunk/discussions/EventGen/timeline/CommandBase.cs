using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventGen.timeline
{
    enum CommandDoneState { Done, NotDone }

    public class CommandBase : ICommand
    {
        CommandDoneState _doneState = CommandDoneState.NotDone;

        public virtual void ToUndone()
        {
            if (_doneState == CommandDoneState.NotDone)
                throw new NotSupportedException("command already undone");

            _doneState = CommandDoneState.NotDone;
        }

        public virtual void ToDone()
        {
            if (_doneState == CommandDoneState.Done)
                throw new NotSupportedException("command already done");

            _doneState = CommandDoneState.Done;
        }
    }
}

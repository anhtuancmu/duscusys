using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventGen.timeline
{
    //keeps history of timeline commands and manages undo/redo operations
    public class CommandManager
    {
        static CommandManager _inst = null;
        public static CommandManager Instance
        {
            get
            {
                if (_inst == null)
                    _inst = new CommandManager();
                return _inst;
            }
        }
                      
        public const int HistoryLength = 25;
        List<ICommand> _history = new List<ICommand>(HistoryLength);
        List<ICommand> _undoHistory = new List<ICommand>(HistoryLength);

        public void RegisterDoneCommand(ICommand cmd)
        {
            registerCommand(cmd, _history);           
        }

        void registerUndoneCommand(ICommand cmd)
        {
            registerCommand(cmd, _undoHistory);
        }

        void registerCommand(ICommand cmd, List<ICommand>  hist)
        {
            if (hist.Count() == HistoryLength)
                hist.RemoveAt(0);//remove oldest
            hist.Add(cmd);
        }

        ICommand popRecentCommand(List<ICommand> hist)
        {
            if (hist.Count() > 0)
            {
                var recentCmd = hist.Last();
                hist.RemoveAt(hist.Count() - 1);
                return recentCmd;
            }
            return null;
        }

        public void Undo()
        {
            var recentCmd = popRecentCommand(_history);
            if (recentCmd!=null)
            {
                recentCmd.ToUndone();
                registerUndoneCommand(recentCmd);
            }
        }

        public void Redo()
        {
            var recentUndoneCmd = popRecentCommand(_undoHistory);
            if (recentUndoneCmd != null)
            {
                recentUndoneCmd.ToDone();
                RegisterDoneCommand(recentUndoneCmd);
            }
        }
    }
}

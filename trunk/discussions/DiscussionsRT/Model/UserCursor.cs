using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discussions.RTModel.Model
{
    public enum CursorInputState
    {
        None,
        Mouse,
        Touch
    }

    public class UserCursor
    {
        public string Name { get; set; }
        public CursorInputState State { get; set; }

        //optional fields
        public int usrId;
        public double x;
        public double y;

        public UserCursor(string Name)
        {
            this.Name = Name;
            State = CursorInputState.None;
        }

        public UserCursor(string Name, CursorInputState State)
        {
            this.Name = Name;
            this.State = State;
        }

        static UserCursor emptryCursor = new UserCursor("", CursorInputState.None);
        public static UserCursor EmptyCursor()
        {
            return emptryCursor;
        }
    }
}

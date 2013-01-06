using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;
using Discussions.model;

namespace Discussions
{
    public class LoginResult
    {
        public Discussion discussion;
        public Person person;
        public DeviceType devType; //defined only for event generator login
        public Session session; //only experimental login
    }
}
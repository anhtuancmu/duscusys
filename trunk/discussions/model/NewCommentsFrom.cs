using System.Collections.Generic;
using Discussions.DbModel;
using System.Linq;

namespace Discussions.model
{
    public class NewCommentsFrom
    {
        private Person _person;
        private int _numNewComments;

        public NewCommentsFrom(Person person)
        {
            this._person = person;
        }

        public int NumNewComments
        {
            get { return _numNewComments; }
            set { _numNewComments = value; }
        }

        public Person Person
        {
            get { return _person; }
            set { _person = value; }
        }

        public override string ToString()
        {            
            return string.Format("{0}  +{1} comment(s)", Person.Name, NumNewComments);
        }
    }

    public static class NewCommentsFromExt
    {
        public static int Total(this IEnumerable<NewCommentsFrom> bins)
        {
            return bins.Sum(bin => bin.NumNewComments);
        }
    }
}
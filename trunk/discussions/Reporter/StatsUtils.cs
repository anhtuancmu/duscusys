using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reporter
{
    public class StatsUtils
    {
        public static List<int> Union(List<int> lst1, List<int> lst2)
        {
            var res = new List<int>();
            foreach (var i in lst1)
                if (!res.Contains(i))
                    res.Add(i);
            foreach (var i in lst2)
                if (!res.Contains(i))
                    res.Add(i);
            return res; 
        }
    }
}

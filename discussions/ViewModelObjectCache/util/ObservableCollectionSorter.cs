using System;
using System.Collections.ObjectModel;

namespace Discussions.util
{
    /// <summary>
    /// Sorts ObservableCollection without Clear(), Add(), or creating another collection
    /// </summary>
    public class ObservableCollectionSorter
    {
        public static void Sort<T>(ObservableCollection<T> collection, Comparison<T> cmp)
        {          
            for (int write = 0; write < collection.Count; write++)
                for (int sort = 0; sort < collection.Count - 1; sort++)
                    if (cmp(collection[sort], collection[sort + 1]) > 0)
                        collection.Move(sort, sort + 1);
        }
    }
}

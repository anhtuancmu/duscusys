using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedEditor
{
    public class ShapeIdGenerator
    {
        List<int> existingIds = new  List<int>();

        Random _rnd = new Random();

        static ShapeIdGenerator _inst = null;
        public static ShapeIdGenerator Instance
        {
            get
            {
                if (_inst == null)
                    _inst = new ShapeIdGenerator();
                return _inst;
            }
        }

        public int NextId(int owner)
        {
            int id;
            do
            {
                id = _rnd.Next(int.MaxValue - 1000); //-1000 for badge ids                
            } while (existingIds.Contains(id));
            return id;
        }

        //when our own shape is created, correct low bound of Id generator
        public void CorrectLowBound(int owner, int existingId)
        {
            existingIds.Add(existingId);
        }
    }
}

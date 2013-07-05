using System.Collections.Generic;
using System.Windows;

namespace DistributedEditor
{
    public class ClientLinkable
    {
        private readonly int _id;
        private readonly List<VdClusterLink> _edges = new List<VdClusterLink>();

        public delegate Rect BoundsProvider();

        private BoundsProvider _boundsProvider;

        public ClientLinkable(int id, BoundsProvider boundsProvider)
        {
            _id = id;
            _boundsProvider = boundsProvider;
        }

        public int GetId()
        {
            return _id;
        }

        public IEnumerable<VdClusterLink> GetEdges()
        {
            return _edges;
        }

        public void AddEdge(VdClusterLink next)
        {
            _edges.Add(next);
        }

        public void RemoveEdge(VdClusterLink next)
        {
            _edges.Remove(next);
        }

        public bool HasEdge(VdClusterLink next)
        {
            return _edges.Contains(next);
        }

        public bool HasAdjacent(ClientLinkable next)
        {
            foreach (var e in _edges)
                if (e.GetLinkable1() == next || e.GetLinkable2() == next)
                    return true;
            return false;
        }

        public Rect GetBounds()
        {
            return _boundsProvider();
        }

        public Point GetCenter()
        {
            var rect = _boundsProvider();
            return new Point(rect.X + rect.Width/2, rect.Y + rect.Height/2);
        }

        public void InvalidateLinks()
        {
            foreach (var link in GetEdges())
                link.NotifyLinkableMoved();
        }
    }
}
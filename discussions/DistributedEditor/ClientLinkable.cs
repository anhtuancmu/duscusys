﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DistributedEditor
{
    public class ClientLinkable
    {
        readonly int _id;
        List<VdClusterLink> _edges = new List<VdClusterLink>();

        public delegate Rect BoundsProvider();
        BoundsProvider _boundsProvider;

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
            return new Point(rect.X + rect.Width/2, rect. Y + rect.Height/2);            
        }

        public void InvalidateLinks()
        {
            foreach (var link in GetEdges())
                link.NotifyLinkableMoved();
        }
    }
}

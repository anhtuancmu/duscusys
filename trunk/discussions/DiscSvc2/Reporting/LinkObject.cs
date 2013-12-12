using System;

namespace DiscSvc.Reporting
{
    public enum LinkObject
    {
        ArgPoint,
        Cluster,
        Link,
        Comment
    }

    public enum EmitType
    {
        Ref,
        Anchor, 
        None
    }

    public struct DiscLink
    {
        public int Id;
        public LinkObject LinkObject;
        public EmitType EmitType; 

        string Ref(string name)
        {
            switch (LinkObject)
            {
                case LinkObject.ArgPoint:
                    return string.Format(@"<a href=""#argPoint{0}"">{1}</a>", Id, name);
                case LinkObject.Cluster:
                    return string.Format(@"<a href=""#cluster{0}"">{1}</a>", Id, name);
                case LinkObject.Link:
                    return string.Format(@"<a href=""#link{0}"">{1}</a>", Id, name);
                case LinkObject.Comment:
                    return string.Format(@"<a href=""#comment{0}"">{1}</a>", Id, name);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        string Anch(string name)
        {
            switch (LinkObject)
            {
                case LinkObject.ArgPoint:
                    return string.Format(@"<a id=""argPoint{0}"">{1}</a>", Id, name);
                case LinkObject.Cluster:
                    return string.Format(@"<a id=""cluster{0}"">{1}</a>", Id, name);
                case LinkObject.Link:
                    return string.Format(@"<a id=""link{0}"">{1}</a>", Id, name);
                case LinkObject.Comment:
                    return string.Format(@"<a id=""comment{0}"">{1}</a>", Id, name);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string Emit(string name)
        {
            switch (EmitType)
            {
                case EmitType.Ref:
                    return Ref(name);
                case EmitType.Anchor:
                    return Anch(name);
                case EmitType.None:
                    return name;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
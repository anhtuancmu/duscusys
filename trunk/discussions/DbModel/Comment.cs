//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Discussions.DbModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class Comment
    {
        public Comment()
        {
            this.ReadEntry = new HashSet<CommentPersonReadEntry>();
        }
    
        public int Id { get; set; }
        public string Text { get; set; }
    
        public virtual ArgPoint ArgPoint { get; set; }
        public virtual Person Person { get; set; }
        public virtual ICollection<CommentPersonReadEntry> ReadEntry { get; set; }
    }
}
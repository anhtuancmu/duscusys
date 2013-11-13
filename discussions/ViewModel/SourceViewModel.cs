using Discussions.TdsSvcRef;

namespace Discussions.ViewModel
{
    public class SourceViewModel
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public int OrderNumber{get;set;}

        public SourceViewModel()
        {            
        }

        public SourceViewModel(SSource source)
        {
            Id = source.Id;
            Text = source.Text;
            OrderNumber = source.OrderNumber;
        }
    }
}
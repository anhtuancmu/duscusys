using System.Windows.Forms;

namespace Discussions.pdf_reader
{
    public partial class PdfWinFrmControl : UserControl
    {
        public PdfWinFrmControl()
        {
            InitializeComponent();
            this.axAcroPDF2.RegionChanged += axAcroPDF2_RegionChanged;
        }

        void axAcroPDF2_RegionChanged(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        public string PdfPathName
        {
            set { this.axAcroPDF2.LoadFile(value); }
        }
    }
}
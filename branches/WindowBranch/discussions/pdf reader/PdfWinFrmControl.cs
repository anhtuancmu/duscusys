using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Discussions.pdf_reader
{
    public partial class PdfWinFrmControl : UserControl
    {
        public PdfWinFrmControl()
        {
            InitializeComponent();
        }

        public string PdfPathName
        {
            set { this.axAcroPDF2.LoadFile(value); }
        }
    }
}
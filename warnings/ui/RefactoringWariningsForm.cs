using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace warnings.ui
{
    public partial class RefactoringWariningsForm : Form
    {
        public RefactoringWariningsForm()
        {
            InitializeComponent();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.AddRefactoringWarning(new string[]{"1", "2", "3", "4"});
        }

    }
}

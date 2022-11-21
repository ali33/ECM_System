using System;
using System.Windows.Forms;

namespace Ecm.Mvvm
{
    internal partial class ConfirmDialog : Form
    {
        public ConfirmDialog()
        {
            InitializeComponent();
        }

        public ConfirmDialog(string message, string compareValue)
        {
            InitializeComponent();
            _compareValue = compareValue;
            lblMessage.Text = message;
        }

        private void TXTValueTextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = txtValue.Text == _compareValue;
        }

        private readonly string _compareValue;
    }
}

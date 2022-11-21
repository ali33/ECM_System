using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Ecm.CaptureCustomAddIn.View
{
    public partial class BaseActionForm : UserControl
    {
        public BaseActionForm(System.Windows.Controls.UserControl view)
        {
            InitializeComponent();
            elementHost1.Child = view;
        }
    }
}

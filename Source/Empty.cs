using System.Windows.Forms;

namespace Lokad.CodeDsl
{
    public partial class Empty : Form
    {
        public Empty()
        {
            this.Hide();
            this.ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;

            InitializeComponent();
        }
    }
}

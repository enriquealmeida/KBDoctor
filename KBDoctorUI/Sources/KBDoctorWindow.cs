using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Concepto.Packages.KBDoctor.Sources
{
    public partial class KBDoctorWindow : Form
    {
        public KBDoctorWindow()
        {
            InitializeComponent();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
           
        }

        public void Navigate(String address)
        {
            if (String.IsNullOrEmpty(address)) return;
            if (address.Equals("about:blank")) return;
            try
            {
                webBrowser1.Navigate(new Uri(address));
            }
            catch (Exception e)
            {
                KBDoctorOutput.InternalError(e.Message, e);
                return;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Packages;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common;
using Artech.Common.Framework.Selection;
using Artech.Common.Helpers.Dates;
using Artech.FrameworkDE;
using Artech.Udm.Framework;
using System.Runtime.InteropServices;
using Artech.Architecture.Common;
using Artech.Common.Framework.Commands;

namespace Concepto.Packages.KBDoctor.Sources
{
    [Guid("37fc47d7-03d7-4389-9017-b8b711870599")]
    public partial class KBDoctorToolWindow : AbstractToolWindow, ISelectionListener, ISelectionContainer
    {
        public static Guid guid = typeof(KBDoctorToolWindow).GUID;

        public KBDoctorToolWindow()
        {
            InitializeComponent();
            UIServices.TrackSelection.Subscribe(Guid.NewGuid(), this);
        }

        public bool OnSelectChange(ISelectionContainer pSC)
        {
            return true;
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Navigate("");
        }

        public void Clear()
        {
            this.Clear("");
        }

        public void Clear(string add)
        {
            this.Navigate("");
        }

        public void Navigate(string address)
        {
            if (!(String.IsNullOrEmpty(address)))
            {
                if (!address.Equals("about:blank"))
                {
                    webBrowser2.Navigate(new Uri(address));
                }
            }
        }



        private void webBrowser2_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string kbPath;
            IDictionary<string, string> parms = new Dictionary<string, string>();

            if (UriHelper.Parse(e.Url.ToString(), out kbPath, parms))
            {
                CommandKey cmdKey = CommandKey.Empty;
                object[] cmdParams = null;
                if (parms.ContainsKey(UriHelper.CommandKey))
                {
                    string[] commandKey = parms[UriHelper.CommandKey].Split(';');
                    if (commandKey.Length == 2)
                    {
                        cmdKey.Package = new Guid(commandKey[0]);
                        cmdKey.Name = commandKey[1];
                        cmdParams = new object[1];
                        cmdParams[0] = parms;
                    }
                    else
                    {
                        if (commandKey.Length == 1)
                        {
                            cmdKey.Package = Guid.Empty;
                            cmdKey.Name = commandKey[0];
                            cmdParams = new object[1];
                            cmdParams[0] = parms;
                        }
                    }
                }
                else
                {
                    /*cmdKey = CommandKeys..Core.OpenKnowledgeBase;
                    cmdParams = new object[1] { url };*/
                }

                ICommandDispatcherService service = UIServices.CommandDispatcher;
                if (service != null)
                    service.Dispatch(cmdKey, new CommandData(cmdParams));
            }
        }

        private void webBrowser2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
    }
}

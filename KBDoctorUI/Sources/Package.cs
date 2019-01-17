using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Artech.Architecture.BL.Framework.Packages;
using Artech.Architecture.Common.Events;
using Artech.Architecture.Common.Packages;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Controls;
using Artech.Architecture.UI.Framework.Packages;
using Concepto.Packages.KBDoctor.Sources;
using Microsoft.Practices.CompositeUI.EventBroker;

namespace Concepto.Packages.KBDoctor
{
   [Guid("fa2c542d-cd46-4df2-9317-bd5899a536eb")]
   public class Package : AbstractPackageUI,  IGxPackageBL
    {

        private KBDoctorToolWindow myKBDoctorWindow;

        public static Guid guid = typeof(Package).GUID;

      public override string Name
      {
         get { return "KBDoctor"; }
      }

      //  public KBDoctorToolWindow KBDoctorTW { get; internal set; }

        public override void Initialize(IGxServiceProvider services)
        {
         base.Initialize(services);
            AddCommandTarget(new CommandManager());
        }

        /*     [EventSubscription(ArchitectureEvents.BeforeSaveKBObject)]
             private void OnBeforeSaveKBObject(object sender, KBObjectEventArgs args)
             {
                 CommonServices.Output.SelectOutput("General");
             }
             */
        public override IToolWindow CreateToolWindow(Guid toolWindowId)
        {
            bool flag = toolWindowId.Equals(KBDoctorToolWindow.guid);
            IToolWindow result;
            if (flag)
            {
                bool flag2 = this.myKBDoctorWindow == null;
                if (flag2)
                {
                    this.myKBDoctorWindow = new KBDoctorToolWindow();
                }
                result = this.myKBDoctorWindow;
            }
            else
            {
                result = base.CreateToolWindow(toolWindowId);
            }
            return result;
        }

        private void LoadToolWindows()
        {
            this.AddToolWindow<KBDoctorToolWindow>();
        }

        public override void Close()
        {
            base.Close();
        }

    }
}

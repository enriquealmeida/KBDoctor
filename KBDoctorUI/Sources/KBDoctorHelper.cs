using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Artech.Architecture.Common.Services;
using Artech.Architecture.Common.Packages;
using Artech.Architecture.UI.Framework.Services;

using KBDoctor = Concepto.Packages.KBDoctor;
using Artech.Genexus.Common;
using System.IO;
using Concepto.Packages.KBDoctor.Sources;
using System.Threading;
using Artech.Architecture.UI.Framework.Controls;

namespace Concepto.Packages.KBDoctor
{
    public static class KBDoctorHelper
    {
        public static IOutputService SelectOutput()
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            return output;
        }

        public static void ShowKBDoctorResults(string outputFile)
        {
            try
            {
                UIServices.ToolWindows.ShowToolWindow(KBDoctorToolWindow.guid);
                UIServices.ToolWindows.FocusToolWindow(KBDoctorToolWindow.guid);
                IToolWindow ikbdtw;
                if (UIServices.ToolWindows.TryGet(KBDoctorToolWindow.guid, out ikbdtw))
                {
                    KBDoctorToolWindow kbdtw = (KBDoctorToolWindow)ikbdtw;
                    kbdtw.Navigate(outputFile);
                    return;
                }

                if (Package.CurrentKBDoctorWindow != null)
                {
                    Package.CurrentKBDoctorWindow.Navigate(outputFile);
                    return;
                }
            }
            catch (Exception e)
            {
                KBDoctorOutput.Warning("Could not open KBDoctor tool window: " + e.Message);
            }

            try
            {
                UIServices.StartPage.OpenPage(outputFile, "KBDoctor", null);
                UIServices.ToolWindows.FocusToolWindow(UIServices.StartPage.ToolWindow.Id);
            }
            catch (Exception e)
            {
                KBDoctorOutput.Error("Error trying to show results. Output file: " + outputFile + ". " + e.Message);
            }
        }

        public static string SpcDirectory(IKBService kbserv )
        {
            GxModel gxModel = kbserv.CurrentKB.DesignModel.Environment.TargetModel.GetAs<GxModel>();
            return kbserv.CurrentKB.Location + string.Format(@"\GXSPC{0:D3}\", gxModel.Model.Id);
        }

        public static string ObjComparerDirectory(IKBService kbserv)
        {
            GxModel gxModel = kbserv.CurrentKB.DesignModel.Environment.TargetModel.GetAs<GxModel>();
            string dir = Path.Combine(SpcDirectory(kbserv), "ObjComparer");
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch (Exception e) { Console.WriteLine(e.Message); }

            return dir;
        }

        public static string NvgComparerDirectory(IKBService kbserv)
        {
            GxModel gxModel = kbserv.CurrentKB.DesignModel.Environment.TargetModel.GetAs<GxModel>();
            string dir = Path.Combine(SpcDirectory(kbserv), "NvgComparer");
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            return dir;
        }
    }
}

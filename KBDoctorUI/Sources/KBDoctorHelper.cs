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
            KBDoctorOutput.Message("ShowKBDoctorResults: requested file '" + outputFile + "'. Exists=" + File.Exists(outputFile).ToString());
            if (File.Exists(outputFile))
            {
                FileInfo fileInfo = new FileInfo(outputFile);
                KBDoctorOutput.Message("ShowKBDoctorResults: file length=" + fileInfo.Length.ToString() + " bytes.");
            }

            try
            {
                KBDoctorOutput.Message("ShowKBDoctorResults: ShowToolWindow " + KBDoctorToolWindow.guid.ToString());
                UIServices.ToolWindows.ShowToolWindow(KBDoctorToolWindow.guid);
                KBDoctorOutput.Message("ShowKBDoctorResults: FocusToolWindow " + KBDoctorToolWindow.guid.ToString());
                UIServices.ToolWindows.FocusToolWindow(KBDoctorToolWindow.guid);
                IToolWindow ikbdtw;
                if (UIServices.ToolWindows.TryGet(KBDoctorToolWindow.guid, out ikbdtw))
                {
                    KBDoctorOutput.Message("ShowKBDoctorResults: TryGet returned " + ikbdtw.GetType().FullName);
                    KBDoctorToolWindow kbdtw = ikbdtw as KBDoctorToolWindow;
                    if (kbdtw == null)
                    {
                        KBDoctorOutput.Warning("ShowKBDoctorResults: ToolWindow instance is not KBDoctorToolWindow.");
                    }
                    else
                    {
                        KBDoctorOutput.Message("ShowKBDoctorResults: navigating KBDoctor tool window.");
                        kbdtw.Navigate(outputFile);
                        KBDoctorOutput.Message("ShowKBDoctorResults: navigation requested.");
                        UIServices.ToolWindows.FocusToolWindow(KBDoctorToolWindow.guid);
                        KBDoctorOutput.Message("ShowKBDoctorResults: focus requested after navigation.");
                        return;
                    }
                }
                else
                {
                    KBDoctorOutput.Warning("ShowKBDoctorResults: ToolWindows.TryGet returned false.");
                }

                if (Package.CurrentKBDoctorWindow != null)
                {
                    KBDoctorOutput.Message("ShowKBDoctorResults: navigating Package.CurrentKBDoctorWindow.");
                    Package.CurrentKBDoctorWindow.Navigate(outputFile);
                    KBDoctorOutput.Message("ShowKBDoctorResults: current window navigation requested.");
                    UIServices.ToolWindows.FocusToolWindow(KBDoctorToolWindow.guid);
                    KBDoctorOutput.Message("ShowKBDoctorResults: focus requested after current window navigation.");
                    return;
                }
                else
                {
                    KBDoctorOutput.Warning("ShowKBDoctorResults: Package.CurrentKBDoctorWindow is null.");
                }
            }
            catch (Exception e)
            {
                KBDoctorOutput.Warning("Could not open KBDoctor tool window: " + e.Message);
                KBDoctorOutput.Warning("ShowKBDoctorResults stack: " + e.StackTrace);
            }

            try
            {
                KBDoctorOutput.Warning("ShowKBDoctorResults: falling back to Start Page.");
                UIServices.StartPage.OpenPage(outputFile, "KBDoctor", null);
                UIServices.ToolWindows.FocusToolWindow(UIServices.StartPage.ToolWindow.Id);
            }
            catch (Exception e)
            {
                KBDoctorOutput.Error("Error trying to show results. Output file: " + outputFile + ". " + e.Message);
                KBDoctorOutput.Error("ShowKBDoctorResults StartPage stack: " + e.StackTrace);
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Artech.Architecture.Common.Packages;
using Artech.Architecture.UI.Framework.Services;

using KBDoctor = Concepto.Packages.KBDoctor;
using Artech.Genexus.Common;
using System.IO;

namespace Concepto.Packages.KBDoctor
{
    public static class KBDoctorHelper
    {
        public static void ShowKBDoctorResults(string outputFile)
        {
            UIServices.StartPage.OpenPage(outputFile, "KBDoctor", null);
            UIServices.ToolWindows.FocusToolWindow(UIServices.StartPage.ToolWindow.Id);

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
            catch (Exception e) { }

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
            catch (Exception e) { }
            return dir;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Artech.Architecture.Common.Packages;
using Artech.Architecture.UI.Framework.Services;

using KBDoctor = Concepto.Packages.KBDoctor;
using Artech.Genexus.Common;
using System.IO;
using Concepto.Packages.KBDoctor.Sources;
using System.Threading;

namespace Concepto.Packages.KBDoctor
{
    public static class KBDoctorHelper
    {
        public static void ShowKBDoctorResults(string outputFile)
        {
            Thread t = new Thread(() => CreateWindow(outputFile));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        public static void CreateWindow(string outputFile)
        {
            KBDoctorWindow sw = new KBDoctorWindow();
            sw.Navigate(outputFile);
            DialogResult dr = sw.ShowDialog();
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

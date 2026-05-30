using System;
using System.Text;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    static class KBDoctorReport
    {
        public static void Run(string title, string[] headers, Action<KBDoctorXMLWriter> writeRows)
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;

            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);
                KBDoctorOutput.StartSection(title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                try
                {
                    writer.AddHeader(title);
                    writer.AddTableHeader(headers);
                    writeRows(writer);
                    writer.AddFooter();
                }
                finally
                {
                    writer.Close();
                }

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                KBDoctorOutput.EndSection(title, true);
            }
            catch
            {
                KBDoctorOutput.EndSection(title, false);
            }
        }
    }
}

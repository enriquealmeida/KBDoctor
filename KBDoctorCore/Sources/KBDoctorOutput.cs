using System;
using Artech.Architecture.Common.Location;
using Artech.Architecture.Common.Services;
using Artech.Common.Diagnostics;

namespace Concepto.Packages.KBDoctor
{
    public  class KBDoctorOutput
    {

        public static  void StartSection(string section)
        {
            IOutputService output = NewKBDoctorOutput();
            output.StartSection(section);
        }

        public static void Error(string v)
        {
            IOutputService output = NewKBDoctorOutput();
            output.AddErrorLine("KBDoctor", v);
        }

        public static void InternalError(string v, Exception e)
        {
            IOutputService output = NewKBDoctorOutput();
            output.AddErrorLine("KBDoctor", "Internal error: " + v + " " + e.Message);
            output.AddErrorLine("KBDoctor", e.StackTrace);
        }

        public static void EndSection(string section)
        {
            IOutputService output = NewKBDoctorOutput();
            output.EndSection(section, true);
        }

        public static void EndSection(string section, bool success)
        {
            IOutputService output = NewKBDoctorOutput();
            output.EndSection(section, success);
        }

        public static void Message(string v)
        {
            IOutputService output = NewKBDoctorOutput();
            output.AddLine("KBDoctor", v);
        }


        public static void Warning(string v)
        {
            IOutputService output = NewKBDoctorOutput();
            output.AddWarningLine("KBDoctor", v);
        }

        public static void OutputError(OutputError oe)
        {
            IOutputService output = NewKBDoctorOutput();
            output.Add("KBDoctor", oe);
        }

        private static IOutputService NewKBDoctorOutput()
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            return output;
        }
    }
}


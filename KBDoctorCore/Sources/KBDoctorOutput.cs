using System;
using Artech.Architecture.Common.Location;
using Artech.Architecture.Common.Services;

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
            output.AddErrorLine(v);
        }

        public static void InternalError(string v, Exception e)
        {
            IOutputService output = NewKBDoctorOutput();
            output.AddErrorLine("Internal error: + v " + e.Message  );
            output.AddErrorLine(e.StackTrace);
        }

        public static void EndSection(string section)
        {
            IOutputService output = NewKBDoctorOutput();
            output.EndSection(section, true);
        }


        public static void Message(string v)
        {
            IOutputService output = NewKBDoctorOutput();
            output.AddLine(v);
        }


        public static void Warning(string v)
        {
            IOutputService output = NewKBDoctorOutput();
            output.AddWarningLine(v);
        }

        private static IOutputService NewKBDoctorOutput()
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");
            return output;
        }
    }
}
 
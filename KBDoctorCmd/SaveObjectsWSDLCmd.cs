using System;
using System.Diagnostics;
using Artech.Architecture.Common.Services;
using Artech.MsBuild.Common;
using Concepto.Packages.KBDoctorCore.Sources;

namespace KBDoctorCmd
{
    public class SaveObjectsWSDLCmd : ArtechTask
    {
        public override bool Execute()
        {
            bool isSuccess = true;
            Stopwatch watch = null;
            OutputSubscribe();
            IOutputService output = CommonServices.Output;
            output.StartSection("Save Objects WSDL");
            try
            {
                watch = new Stopwatch();
                watch.Start();

                if (KB == null)
                {
                    output.AddErrorLine("No hay ninguna KB abierta en el contexto actual.");
                    isSuccess = false;
                }
                else
                {
                    CommonServices.Output.AddLine(string.Format(KB.Name, KB.Location));
                    API.SaveObjectsWSDL(KB, output);
                }

            }
            catch (Exception e)
            {
                output.AddErrorLine(e.Message);
                isSuccess = false;
            }
            finally
            {
                output.EndSection("Save Objects WSDL", isSuccess);
                OutputUnsubscribe();
            }
            return isSuccess;
        }
    }
}

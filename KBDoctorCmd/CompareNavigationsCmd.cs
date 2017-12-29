using System;
using System.Diagnostics;
using System.Globalization;

using Artech.Architecture.Common.Services;
using Artech.MsBuild.Common;
using Concepto.Packages.KBDoctorCore.Sources;

namespace KBDoctorCmd
{
    public class CompareNavigationsCmd : ArtechTask
    {
        public override bool Execute()
        {
            bool isSuccess = true;
            Stopwatch watch = null;
            OutputSubscribe();
            IOutputService output = CommonServices.Output;
            try
            {
                watch = new Stopwatch();
                watch.Start();

                if (KB == null)
                {
                    output.AddErrorLine("No hay ninguna KB abierta en el contexto actual, asegúrese de incluír la tarea OpenKnowledgeBase antes de ejecutar la comparación de navegaciones.");
                    isSuccess = false;
                }
                else
                {
                    CommonServices.Output.AddLine(string.Format(KB.Name, KB.Location));
                    isSuccess = API.CompareNavigations(KB, output);
                }

            }
            catch (Exception e)
            {
                output.AddErrorLine(e.Message);
                isSuccess = false;
            }
            finally
            {
                output.EndSection("Compare navigations", isSuccess);
                OutputUnsubscribe();
            }
            return isSuccess;
        }
    }
}

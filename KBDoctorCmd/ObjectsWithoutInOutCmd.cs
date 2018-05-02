using System;
using System.Diagnostics;


using Artech.Architecture.Common.Services;
using Artech.MsBuild.Common;
using Concepto.Packages.KBDoctorCore.Sources;


namespace KBDoctorCmd
{
    public class ObjectsWithoutInOutCmd : ArtechTask
    {
        public override bool Execute()
        {
            bool isSuccess = true;
            Stopwatch watch = null;
            OutputSubscribe();
            IOutputService output = CommonServices.Output;
            output.StartSection("Objects Without InOut ");
            try
            {
                watch = new Stopwatch();
                watch.Start();

                if (KB == null)
                {
                    output.AddErrorLine("No hay ninguna KB abierta en el contexto actual, asegúrese de incluír la tarea OpenKnowledgeBase antes de ejecutar la limpieza de variables.");
                    isSuccess = false;
                }
                else
                {
                    CommonServices.Output.AddLine(string.Format(KB.Name, KB.Location));
                    API.ObjectsWithoutINOUT(KB, output);
                }
            }
            catch (Exception e)
            {
                output.AddErrorLine(e.Message);
                isSuccess = false;
            }
            finally
            {
                output.EndSection("Objects Without InOut ", isSuccess);
                OutputUnsubscribe();
            }
            return isSuccess;
        }
    }
}

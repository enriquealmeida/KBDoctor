using System;
using System.Diagnostics;


using Artech.Architecture.Common.Services;
using Artech.MsBuild.Common;
using Concepto.Packages.KBDoctorCore.Sources;


namespace KBDoctorCmd
{
    public class CleanAllKBObjectVariablesCmd : ArtechTask
    {
        public override bool Execute()
        {
            bool isSuccess = true;
            Stopwatch watch = null;
            OutputSubscribe();
            IOutputService output = CommonServices.Output;
            output.StartSection("Clean all variables ");
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
                    API.CleanAllKBObjectVariables(KB, output);
                }
            }
            catch (Exception e)
            {
                output.AddErrorLine(e.Message);
                isSuccess = false;
            }
            finally
            {
                output.EndSection("Clean all variables ", isSuccess);
                OutputUnsubscribe();
            }
            return isSuccess;
        }
    }
}

using System;
using System.Diagnostics;
using Artech.Architecture.Common.Services;
using Artech.MsBuild.Common;
using Concepto.Packages.KBDoctorCore.Sources;

namespace KBDoctorCmd
{
    public class CheckBldObjectsCmd : ArtechTask
    {
        public override bool Execute()
        {
            bool isSuccess = true;
            Stopwatch watch = null;
            OutputSubscribe();
            IOutputService output = CommonServices.Output;
            output.StartSection("Check bld objects in KB");
            try
            {
                watch = new Stopwatch();
                watch.Start();

                if (KB == null)
                {
                    output.AddErrorLine("No hay ninguna KB abierta en el contexto actual. Ejecute una tarea OpenKB antes.");
                    isSuccess = false;
                }
                else
                {
                    output.AddLine("KBDoctor",string.Format(KB.Name, KB.Location));
                    API.CheckBldObjects(KB);
                }

            }
            catch (Exception e)
            {
                output.AddErrorLine(e.Message);
                isSuccess = false;
            }
            finally
            {
                output.EndSection("Check bld objects in KB", isSuccess);
                OutputUnsubscribe();
            }
            return isSuccess;
        }
    }
}

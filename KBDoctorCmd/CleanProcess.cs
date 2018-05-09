using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.MsBuild.Common;
using Concepto.Packages.KBDoctorCore.Sources;


namespace KBDoctorCmd
{
    public class CleanProcess : ArtechTask
    {
        private string m_Objects;

        public string Objects
        {
            get
            {
                return this.m_Objects;
            }
            set
            {
                this.m_Objects = value;
            }
        }

        public override bool Execute()
        {
            bool isSuccess = true;
            Stopwatch watch = null;
            OutputSubscribe();
            IOutputService output = CommonServices.Output;
            output.StartSection("Clean process");
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
                    CommonServices.Output.AddLine(Objects);
                    //API.PreProcessPendingObjects(KB, output, CodigoGX.GetObjects(base.KB.DesignModel, this.Objects));   
                }
            }
            catch (Exception e)
            {
                output.AddErrorLine(e.Message);
                isSuccess = false;
            }
            finally
            {
                output.EndSection("Clean process", isSuccess);
                OutputUnsubscribe();
            }
            return isSuccess;
        }
    }
}

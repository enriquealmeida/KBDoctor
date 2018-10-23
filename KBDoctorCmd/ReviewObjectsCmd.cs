using System;
using System.Collections.Generic;
using System.Diagnostics;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.MsBuild.Common;
using Concepto.Packages.KBDoctorCore.Sources;


namespace KBDoctorCmd
{
    public class ReviewObjectsCmd : ArtechTask
    {
        private string m_DateFrom;

        public string DateFrom
        {
            get
            {
                return this.m_DateFrom;
            }
            set
            {
                this.m_DateFrom = value;
            }
        }

        public override bool Execute()
        {
            bool isSuccess = true;
            Stopwatch watch = null;
            OutputSubscribe();
            IOutputService output = CommonServices.Output;
            output.StartSection("Review objects");
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
                    output.AddLine("KBDoctor",DateFrom);
                    List<KBObject> objects = new List<KBObject>();
                    DateTime dt;
                    if (DateFrom != null)
                    { 
                        dt = DateTime.ParseExact(DateFrom, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        DateTime ayer = DateTime.Today.AddDays(-1);
                        dt = ayer;
                    }

                    foreach (KBObject obj in KB.DesignModel.Objects.GetAll())
                    {
                        if(DateTime.Compare(obj.Timestamp, dt) >= 0)
                        {
                            objects.Add(obj);
                        }
                    }

                    output.AddLine("Review objects from " + dt.ToString());
                    List<string[]> lines = new List<string[]>();
                    API.PreProcessPendingObjects(KB, output, objects, out lines);
                    lines.Clear();
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

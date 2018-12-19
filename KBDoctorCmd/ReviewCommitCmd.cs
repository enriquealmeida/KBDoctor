using System;
using System.Collections.Generic;
using System.Diagnostics;
using Artech.Architecture.BL.Framework.Services;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.Common.Services.TeamDevData.Client;
using Artech.MsBuild.Common;
using Concepto.Packages.KBDoctorCore.Sources;
using ITeamDevClientService = Artech.Architecture.Common.Services.ITeamDevClientService;

namespace KBDoctorCmd
{
    public class ReviewCommitCmd : ArtechTask
    {
        private string m_DateFrom;
        private string m_DateTo;
        private string m_ServerUser;
        private string m_ServerPassword;

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

        public string DateTo
        {
            get
            {
                return this.m_DateTo;
            }
            set
            {
                this.m_DateTo = value;
            }
        }

        public string ServerUser
        {
            get
            {
                return this.m_ServerUser;
            }
            set
            {
                this.m_ServerUser = value;
            }
        }

        public string ServerPassword
        {
            get
            {
                return this.m_ServerPassword;
            }
            set
            {
                this.m_ServerPassword = value;
            }
        }

        public override bool Execute()
        {
            bool isSuccess = true;
            Stopwatch watch = null;
            OutputSubscribe();
            IOutputService output = CommonServices.Output;
            output.StartSection("Review commits");
            try
            {
                watch = new Stopwatch();
                watch.Start();

                if (KB == null)
                {
                    output.AddErrorLine("No hay ninguna KB abierta en el contexto actual, asegúrese de incluír la tarea OpenKnowledgeBase antes de ejecutar revisión de commits.");
                    isSuccess = false;
                }
                else
                {
                    List<KBObject> objects = new List<KBObject>();
                    DateTime From;
                    DateTime To;
                    if (DateFrom != null)
                    {
                        From = DateTime.ParseExact(DateFrom, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        DateTime yest = DateTime.Today.AddDays(-1);
                        From = yest;
                    }

                    if (DateTo != null)
                    {
                        To = DateTime.ParseExact(DateTo, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        DateTime today = DateTime.Today;
                        To = today;
                    }

                    output.AddLine("Reviewing commits from " + From.ToString() + " to " + To.ToString());

                    string querystring = Utility.GetQueryStringFromToDate(From, To);
                    TeamDevelopmentData tdd = new TeamDevelopmentData(KB.DesignModel);
                    tdd.User = ServerUser;
                    tdd.Password = ServerPassword;
                    
                    List<IKBVersionRevision> revisions_list = BLServices.TeamDevClient.GetRevisions(tdd, KB.DesignModel.KBVersion.Id, KB.DesignModel.KBVersion.Name, querystring, 1);
                    isSuccess = API.ReivewCommits(KB, revisions_list);

                }
            }
            catch (Exception e)
            {
                output.AddErrorLine(e.Message);
                isSuccess = false;
            }
            finally
            {
                output.EndSection("Review commits", isSuccess);
                OutputUnsubscribe();
            }
            return isSuccess;
        }
    }
}

using Artech.Architecture.Common.Objects;
//using Artech.Architecture.Common.Resolvers;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Concepto.Packages.KBDoctorCore.Sources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Artech.Genexus.Common.Properties.TRN;

namespace Concepto.Packages.KBDoctor.Sources
{
    public partial class ResponsiveSmooth : Form
    {
        public ResponsiveSmooth()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;

            WebPanelReference masterRef = (WebPanelReference)kbModel.GetPropertyValue("idMASTER_PAGE");
            ThemeWebReference themeReference = (ThemeWebReference)kbModel.GetPropertyValue(Properties.MODEL.DefaultTheme);
            string modelWebDF = kbModel.GetPropertyValueString(Properties.MODEL.WebFormDefaults_DisplayName);

            //    output.AddWarningLine(masterRef.GetName(kbModel));

            foreach (KBObject obj in Transaction.GetAll(kbModel))
            {
                CambioResponsive(obj);
            }

            foreach (KBObject obj in WebPanel.GetAll(kbModel))
            {
                CambioResponsive(obj);
            }

            void CambioResponsive(KBObject obj)
            {

                if (CambioWebDF(obj))
                    obj.Save();
                output.AddLine(obj.Name);
            }






            bool CambioWebDF(KBObject obj)
            {
                bool isChanged = false;

                string webDF = obj.GetPropertyValueString(Properties.TRN.WebFormDefaults);

                // output.AddLine(webFD.ToString() + "--" + obj.GetPropertyValueString(Properties.TRN.WebFormDefaults) + "-" + Properties.TRN.WebFormDefaults_DisplayName);
                if (ObjectsHelper.isGenerated(obj))
                {

                    if  (webDF == modelWebDF && obj.IsPropertyDefault(Properties.TRN.WebFormDefaults))
                    {
                        obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebFormDefaults_Values.ResponsiveWebDesign);
                        obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebFormDefaults_Values.PreviousVersionsCompatible);

                        isChanged = true;
                    }
                    else
                    {
                        output.AddText("NO CAMBIA WebFormDefault ");
                    }

                    WebPanelReference objmasterRef = (WebPanelReference)obj.GetPropertyValue(Properties.TRN.MasterPage);

                    if (objmasterRef.GetFullQualifyName(kbModel) == masterRef.GetFullQualifyName(kbModel) && obj.IsPropertyDefault(Properties.TRN.WebFormDefaults))
                    {
                        obj.SetPropertyValue(Properties.TRN.MasterPage, WebPanelReference.NoneRef);
                        obj.SetPropertyValue(Properties.TRN.MasterPage, objmasterRef );

                        isChanged = true;
                    }
                    else
                    {
                        output.AddText("NO CAMBIA MasterPage ");
                    }

                    ThemeWebReference objThemeRef = (ThemeWebReference) obj.GetPropertyValue(Properties.TRN.Theme);

                    if (objThemeRef.GetFullQualifiedName(kbModel) == themeReference.GetFullQualifiedName(kbModel) && obj.IsPropertyDefault(Properties.TRN.Theme))
                    {
                       // obj.SetPropertyValue(Properties.TRN.Theme, ThemeWebReference..NoneRef);
                        obj.SetPropertyValue(Properties.TRN.Theme, themeReference);

                        isChanged = true;
                    }
                    else
                    {
                        output.AddText("NO CAMBIA MasterPage ");
                    }

                   // obj.SetPropertyValue(Properties.TRN.Theme, themeReference);
                   // obj.SetPropertyValue(Properties.TRN.MasterPage, masterRef);
                }
                return isChanged;
            }

           

        }

        private void button2_Click(object sender, EventArgs e)
        {
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;

            //WebPanelReference masterRef = (WebPanelReference)kbModel.GetPropertyValue("idMASTER_PAGE");
            //ThemeWebReference themeReference = (ThemeWebReference)kbModel.GetPropertyValue(Properties.MODEL.DefaultTheme);
            string modelWebUX = kbModel.GetPropertyValueString(Properties.MODEL.WebUserExperience);

            //    output.AddWarningLine(masterRef.GetName(kbModel));

            foreach (KBObject obj in Transaction.GetAll(kbModel))
            {
                CambioSmooth(obj);
            }

            foreach (KBObject obj in WebPanel.GetAll(kbModel))
            {
                CambioSmooth(obj);
            }

            void CambioSmooth(KBObject obj)
            {

                if (CambioWEBUX(obj))
                    obj.Save();
                output.AddLine(obj.Name);
            }


     



            bool CambioWEBUX(KBObject obj)


            {
                string webUX = obj.GetPropertyValueString(Properties.TRN.WebUserExperience);

                // output.AddLine(webFD.ToString() + "--" + obj.GetPropertyValueString(Properties.TRN.WebFormDefaults) + "-" + Properties.TRN.WebFormDefaults_DisplayName);
                if (ObjectsHelper.isGenerated(obj) && webUX == modelWebUX && obj.IsPropertyDefault(Properties.TRN.WebUserExperience))
                {
                    obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebUserExperience_Values.Smooth);
                    obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebUserExperience_Values.PreviousVersionsCompatible);

                    return true;
                }
                else
                {
                    output.AddText("NO CAMBIA WEBUX ");
                    return false;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;

            foreach (Transaction obj in Transaction.GetAll(kbModel)) 
            {
                if (ObjectsHelper.isGenerated(obj) && ObjectsHelper.isGeneratedbyPattern(obj))
                {
                    string eventList = ListOfEvents(obj);
                   // output.AddLine(obj.Name + "," + obj.GetPropertyValueString(Properties.TRN.WebUserExperience) + "," + obj.IsPropertyDefault(Properties.TRN.WebUserExperience) +  eventList);
                    output.AddLine(obj.Name + "," + obj.GetPropertyValueString(Properties.TRN.WebUserExperience) + "," + eventList);

                    if (obj.GetPropertyValueString(Properties.TRN.WebUserExperience) == "Previous versions compatible")
                    {
                        obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebUserExperience_Values.Smooth);
                        obj.Save();
                    }
                }
            }

            foreach (WebPanel obj in WebPanel.GetAll(kbModel))
            {
                if (ObjectsHelper.isGenerated(obj) && ObjectsHelper.isGeneratedbyPattern(obj))
                {
                    string eventList = ListOfEvents(obj);
                 //   output.AddLine(obj.Name + "," + obj.GetPropertyValueString(Properties.TRN.WebUserExperience) + "," + obj.IsPropertyDefault(Properties.TRN.WebUserExperience) + eventList);
                          
                  //  output.AddLine(obj.Name + "," + obj.GetPropertyValueString(Properties.TRN.WebUserExperience) + "," + obj.IsPropertyDefault(Properties.TRN.WebUserExperience) +  eventList);
                    output.AddLine(obj.Name + "," + obj.GetPropertyValueString(Properties.TRN.WebUserExperience) + "," + eventList);

                    string aux = obj.GetPropertyValueString(Properties.TRN.WebUserExperience);

                    if (obj.GetPropertyValueString(Properties.TRN.WebUserExperience) == "Previous versions compatible")
                    {
                        obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebUserExperience_Values.Smooth);
                        obj.Save();
                    }
                }
            }

        }

        public static string ListOfEvents(KBObject obj)
        {
            string eventsList = "";
            string source = obj.Parts.Get<EventsPart>().Source;
            string sourceWOcomments = Utility.ExtractComments(source);
            bool containLOAD = false, containREFRESH = false , containSTART = false;

            using (StringReader reader = new StringReader(sourceWOcomments))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {

                    if (line.ToUpper().StartsWith("EVENT "))
                    {
                        eventsList += "," + line.ToUpper().Replace("EVENT ", "")  ;
                        if (line.ToUpper().Contains("LOAD"))
                            containLOAD = true;
                        if (line.ToUpper().Contains("REFRESH"))
                            containREFRESH = true;
                        if (line.ToUpper().Contains("START"))
                            containSTART = true;
                      
                    }
                }

            }
            eventsList = ","+containSTART.ToString() + "," + containREFRESH.ToString() + "," + containLOAD.ToString() + eventsList;
            return eventsList;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;

            //WebPanelReference masterRef = (WebPanelReference)kbModel.GetPropertyValue("idMASTER_PAGE");
            //ThemeWebReference themeReference = (ThemeWebReference)kbModel.GetPropertyValue(Properties.MODEL.DefaultTheme);
            string modelWebUX = kbModel.GetPropertyValueString(Properties.MODEL.WebUserExperience);

            //    output.AddWarningLine(masterRef.GetName(kbModel));
            int objTotal = 0;
            int objSmooth = 0;
            int objMaster = 0;
            int objResponsive = 0;

            foreach (KBObject obj in Transaction.GetAll(kbModel))
            {
                CuentoSmoothResponsive(obj);
            }

            foreach (KBObject obj in WebPanel.GetAll(kbModel))
            {
                CuentoSmoothResponsive(obj);
            }


            output.AddLine("#Objects,#ObjSmooth,#ObjMaster,#ObjResponsive");
            output.AddLine(objTotal.ToString() + "," + objSmooth.ToString() + "," + objMaster.ToString() + "," + objResponsive.ToString() );

            void CuentoSmoothResponsive(KBObject obj)
            {
                if (ObjectsHelper.isGenerated(obj))
                {


                    objTotal += 1;

                    string webUX = obj.GetPropertyValueString(Properties.TRN.WebUserExperience).ToUpper();
                    WebPanelReference objmasterRef = (WebPanelReference)obj.GetPropertyValue(Properties.TRN.MasterPage);

                    if (ObjectsHelper.isGenerated(obj) && webUX == "SMOOTH")
                    {
                        output.AddLine(obj.Name + " " + objmasterRef.GetName(kbModel));
                        objSmooth += 1;
                    }


                    if (objmasterRef.GetName(kbModel)== "LuciaMasterPage_v4" )
                    {
                        objMaster += 1;
                    }

                    string webDF = obj.GetPropertyValueString(Properties.TRN.WebFormDefaults);
                    if (webDF != "Previous versions compatible")
                    {
                        objResponsive += 1;
                    }

                }
            }
        }



        private void button5_Click(object sender, EventArgs e)
        {
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;

            WebPanelReference masterRef = (WebPanelReference)kbModel.GetPropertyValue("idMASTER_PAGE");

            output.AddWarningLine(masterRef.GetName(kbModel));

            foreach (KBObject obj in Transaction.GetAll(kbModel))
            {
                CambioMasterPage(obj);
            }

            foreach (KBObject obj in WebPanel.GetAll(kbModel))
            {
                CambioMasterPage(obj);
            }


            bool CambioMasterPage(KBObject obj)
            {
                bool isChanged = false;

                // output.AddLine(webFD.ToString() + "--" + obj.GetPropertyValueString(Properties.TRN.WebFormDefaults) + "-" + Properties.TRN.WebFormDefaults_DisplayName);
                if (ObjectsHelper.isGenerated(obj))
                {
                    WebPanelReference objmasterRef = (WebPanelReference)obj.GetPropertyValue(Properties.TRN.MasterPage);

                    if (objmasterRef.GetFullQualifyName(kbModel) == masterRef.GetFullQualifyName(kbModel) && obj.IsPropertyDefault(Properties.TRN.WebFormDefaults))
                    {
                        obj.SetPropertyValue(Properties.TRN.MasterPage, WebPanelReference.NoneRef);
                        obj.SetPropertyValue(Properties.TRN.MasterPage, objmasterRef);

                        isChanged = true;
                        obj.Save();

                        string webUX = obj.GetPropertyValueString(Properties.TRN.WebUserExperience).ToUpper();
                        if (webUX == "SMOOTH")
                               output.AddLine("ERROR SMOOTH WITH MASTERPAGE DEFAULT:" + obj.Name +  "--" + objmasterRef.GetFullQualifyName(kbModel) );
                    }
                    else
                    {
                        output.AddText("NO CAMBIA MasterPage ");
                    }

                }
                return isChanged;
            }

        }


    }
}


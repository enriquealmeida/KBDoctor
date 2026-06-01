using System;
using System.IO;
using System.Xml;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    static class ResponsiveSmoothActions
    {
        public static void Run(string action)
        {
            string title = "KBDoctor - Responsive/Smooth actions";
            KBDoctorOutput.StartSection(title);
            switch (action)
            {
                case "responsiveDefaults":
                    ConvertDefaultResponsiveProperties();
                    break;
                case "smoothDefaults":
                    ConvertDefaultWebUXToSmooth();
                    break;
                case "generatedPreviousToSmooth":
                    ConvertGeneratedPreviousCompatibleToSmooth();
                    break;
                case "auditSmoothResponsive":
                    AuditSmoothResponsive();
                    break;
                case "resetDefaultMasterPage":
                    ResetDefaultMasterPageReferences();
                    break;
                default:
                    KBDoctorOutput.Error("Unknown action: " + action);
                    KBDoctorOutput.EndSection(title, false);
                    return;
            }

            KBDoctorOutput.EndSection(title, true);
        }

        private static void ConvertDefaultResponsiveProperties()
        {
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            WebPanelReference masterRef = (WebPanelReference)kbModel.GetPropertyValue("idMASTER_PAGE");
            ThemeWebReference themeReference = (ThemeWebReference)kbModel.GetPropertyValue(Properties.MODEL.DefaultTheme);
            string modelWebDF = kbModel.GetPropertyValueString(Properties.MODEL.WebFormDefaults_DisplayName);

            ForEachTransactionAndWebPanel(kbModel, obj =>
            {
                bool changed = false;
                string webDF = obj.GetPropertyValueString(Properties.TRN.WebFormDefaults);
                if (ObjectsHelper.isGenerated(obj))
                {
                    if (webDF == modelWebDF && obj.IsPropertyDefault(Properties.TRN.WebFormDefaults))
                    {
                        obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebFormDefaults_Values.ResponsiveWebDesign);
                        obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebFormDefaults_Values.PreviousVersionsCompatible);
                        changed = true;
                    }

                    WebPanelReference objmasterRef = (WebPanelReference)obj.GetPropertyValue(Properties.TRN.MasterPage);
                    if (objmasterRef.GetFullQualifyName(kbModel) == masterRef.GetFullQualifyName(kbModel) && obj.IsPropertyDefault(Properties.TRN.WebFormDefaults))
                    {
                        obj.SetPropertyValue(Properties.TRN.MasterPage, WebPanelReference.NoneRef);
                        obj.SetPropertyValue(Properties.TRN.MasterPage, objmasterRef);
                        changed = true;
                    }

                    ThemeWebReference objThemeRef = (ThemeWebReference)obj.GetPropertyValue(Properties.TRN.Theme);
                    if (objThemeRef.GetFullQualifiedName(kbModel) == themeReference.GetFullQualifiedName(kbModel) && obj.IsPropertyDefault(Properties.TRN.Theme))
                    {
                        obj.SetPropertyValue(Properties.TRN.Theme, themeReference);
                        changed = true;
                    }
                }

                if (changed)
                {
                    obj.Save();
                }
                output.AddLine(obj.Name);
            });
        }

        private static void ConvertDefaultWebUXToSmooth()
        {
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            string modelWebUX = kbModel.GetPropertyValueString(Properties.MODEL.WebUserExperience);

            ForEachTransactionAndWebPanel(kbModel, obj =>
            {
                string webUX = obj.GetPropertyValueString(Properties.TRN.WebUserExperience);
                if (ObjectsHelper.isGenerated(obj) && webUX == modelWebUX && obj.IsPropertyDefault(Properties.TRN.WebUserExperience))
                {
                    obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebUserExperience_Values.Smooth);
                    obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebUserExperience_Values.PreviousVersionsCompatible);
                    obj.Save();
                }
                else
                {
                    output.AddText("NO CAMBIA WEBUX ");
                }

                output.AddLine(obj.Name);
            });
        }

        private static void ConvertGeneratedPreviousCompatibleToSmooth()
        {
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            ForEachTransactionAndWebPanel(kbModel, obj =>
            {
                if (ObjectsHelper.isGenerated(obj) && ObjectsHelper.isGeneratedbyPattern(obj))
                {
                    string eventList = ListOfEvents(obj);
                    output.AddLine(obj.Name + "," + obj.GetPropertyValueString(Properties.TRN.WebUserExperience) + "," + eventList);
                    if (obj.GetPropertyValueString(Properties.TRN.WebUserExperience) == "Previous versions compatible")
                    {
                        obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebUserExperience_Values.Smooth);
                        obj.Save();
                    }
                }
            });
        }

        private static void AuditSmoothResponsive()
        {
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            QualifiedName qn = new QualifiedName("DECLARACIONES.Dua.HCIMDPol");
            WebPanel webp = WebPanel.Get(kbModel, qn);
            WebPanelReference luciamasterRef = webp == null ? null : (WebPanelReference)webp.GetPropertyValue(Properties.TRN.MasterPage);

            output.AddLine("ObjectName,UX,MasterPageName,Responsive,Theme,AbstracEditor,PatternOrManual,NeedFix");
            int objTotal = 0;
            int objToFix = 0;

            ForEachTransactionAndWebPanel(kbModel, obj =>
            {
                if (ObjectsHelper.isGenerated(obj))
                {
                    objTotal += 1;
                    string webUX = obj.GetPropertyValueString(Properties.TRN.WebUserExperience).ToUpper();
                    WebPanelReference objmasterRef = (WebPanelReference)obj.GetPropertyValue(Properties.TRN.MasterPage);
                    string webDF = obj.GetPropertyValueString(Properties.TRN.WebFormDefaults);
                    string themeObj = obj.GetPropertyValueString(Properties.TRN.Theme);
                    WebFormPart webFormPart = obj is WebPanel ? ((WebPanel)obj).WebForm : ((Transaction)obj).WebForm;
                    bool abstracEditor = WebPanelFormRootIsAbstract(webFormPart);
                    string genPatt = ObjectsHelper.isGeneratedbyPattern(obj) ? "Pattern" : "Manual";
                    bool needFix = webUX != "SMOOTH" || objmasterRef.GetName(kbModel) != "GIAMasterPage_v1" || webDF == "Previous versions compatible" || themeObj != "GIATheme_v1" || !abstracEditor;
                    if (needFix)
                    {
                        objToFix += 1;
                    }

                    output.AddLine(obj.Name + "," + webUX + "," + objmasterRef.GetName(kbModel) + "," + webDF + "," + themeObj + "," + abstracEditor.ToString() + "," + genPatt + "," + needFix.ToString());
                    if (luciamasterRef != null && objmasterRef.GetName(kbModel) == "GIAMasterPage_v1" && !abstracEditor && genPatt == "Manual")
                    {
                        output.AddLine(">>> Cambio " + obj.Name);
                        obj.SetPropertyValue(Properties.TRN.MasterPage, luciamasterRef);
                        obj.Save();
                    }
                }
            });

            output.AddLine("   ");
            output.AddLine("#Objects:" + objTotal.ToString() + "  #Objects to Fix: " + objToFix.ToString());
        }

        private static void ResetDefaultMasterPageReferences()
        {
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            WebPanelReference masterRef = (WebPanelReference)kbModel.GetPropertyValue("idMASTER_PAGE");
            output.AddWarningLine(masterRef.GetName(kbModel));

            ForEachTransactionAndWebPanel(kbModel, obj =>
            {
                if (ObjectsHelper.isGenerated(obj))
                {
                    WebPanelReference objmasterRef = (WebPanelReference)obj.GetPropertyValue(Properties.TRN.MasterPage);
                    if (objmasterRef.GetFullQualifyName(kbModel) == masterRef.GetFullQualifyName(kbModel) && obj.IsPropertyDefault(Properties.TRN.WebFormDefaults))
                    {
                        obj.SetPropertyValue(Properties.TRN.MasterPage, WebPanelReference.NoneRef);
                        obj.SetPropertyValue(Properties.TRN.MasterPage, objmasterRef);
                        obj.Save();

                        string webUX = obj.GetPropertyValueString(Properties.TRN.WebUserExperience).ToUpper();
                        if (webUX == "SMOOTH")
                        {
                            output.AddLine("ERROR SMOOTH WITH MASTERPAGE DEFAULT:" + obj.Name + "--" + objmasterRef.GetFullQualifyName(kbModel));
                        }
                    }
                }
            });
        }

        private static void ForEachTransactionAndWebPanel(KBModel kbModel, Action<KBObject> action)
        {
            foreach (KBObject obj in Utility.EditableObjects(Transaction.GetAll(kbModel)))
            {
                action(obj);
            }

            foreach (KBObject obj in Utility.EditableObjects(WebPanel.GetAll(kbModel)))
            {
                action(obj);
            }
        }

        private static string ListOfEvents(KBObject obj)
        {
            string eventsList = "";
            string source = obj.Parts.Get<EventsPart>().Source;
            string sourceWOcomments = Utility.ExtractComments(source);
            bool containLOAD = false, containREFRESH = false, containSTART = false;

            using (StringReader reader = new StringReader(sourceWOcomments))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.ToUpper().StartsWith("EVENT "))
                    {
                        eventsList += "," + line.ToUpper().Replace("EVENT ", "");
                        if (line.ToUpper().Contains("LOAD"))
                            containLOAD = true;
                        if (line.ToUpper().Contains("REFRESH"))
                            containREFRESH = true;
                        if (line.ToUpper().Contains("START"))
                            containSTART = true;
                    }
                }
            }

            return "," + containSTART.ToString() + "," + containREFRESH.ToString() + "," + containLOAD.ToString() + eventsList;
        }

        private static bool WebPanelFormRootIsAbstract(WebFormPart part)
        {
            if (part.EditableContent != null)
            {
                XmlDocument xmlDocumentWebForm = new XmlDocument();
                xmlDocumentWebForm.LoadXml(part.EditableContent);
                return xmlDocumentWebForm.SelectSingleNode("//Form[@id='" + xmlDocumentWebForm.DocumentElement?.Attributes["rootId"]?.Value + "']")?.Attributes["type"]?.Value == "layout";
            }

            return part.Model.GetPropertyValue<Properties.MODEL.DefaultWebFormEditor_Enum>(Properties.MODEL.DefaultWebFormEditor) == Properties.MODEL.DefaultWebFormEditor_Enum.AbstractLayout;
        }
    }
}

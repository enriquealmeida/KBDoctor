using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Helpers.Templates;
using Artech.Common.Properties;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common.ModelParts;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.SDT;
using Artech.Genexus.Common.Parts.WebForm;
using Concepto.Packages.KBDoctorCore.Sources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Concepto.Packages.KBDoctor
{
    class BadSmells
    {
        public static void ListDynamicCombo()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - List objects with dynamic combobox in grid columns";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                IOutputService output = CommonServices.Output;
                output.StartSection("KBDoctor", title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Description", "Control Name", "Col Visible", "Conditions" });


                //All useful objects are added to a collection
                foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())

                {
                    string objName = obj.Name;

                    if (obj is WebPanel || obj is Transaction)
                    {
                        //KBDoctorOutput.Message( "Procesing.." + obj.Name);
                        WebFormPart webForm = obj.Parts.Get<WebFormPart>();
                        foreach (IWebTag current in WebFormHelper.EnumerateWebTag(webForm))
                        {

                            try
                            {
                                //  if (current.Node.ParentNode.Name == "gxGrid" && current.Type == WebTagType.Column)
                                // {
                                string controlName = current.Properties.GetPropertyValueString("ControlName");
                                string controltype = current.Properties.GetPropertyValueString("ControlType");

                                string hidden = "";

                                //  string att = current.Properties.GetPropertyValueString("Attribute");
                                if (controltype != null && controltype == "Dynamic Combo Box")
                                {
                                    KBDoctorOutput.Message(">>>>Procesing.." + obj.Name + "-" + current.Type);
                                    string conditions = current.Properties.GetPropertyValueString("ControlWhere");
                                   // string conditions = "";
                                    writer.AddTableData(new string[] { Functions.linkObject(obj), obj.Description, controlName, hidden.ToString(), conditions });
                                }
                                //  string suggest = current.Properties.GetPropertyValueString("EditAutocomplete");
                                //  object suggest_obj = current.Properties.GetPropertyValue("EditAutocomplete");
                                //   string read_only = current.Properties.GetPropertyValueString("ColReadOnly");
                                //                              if (controltype == "Edit" && suggest != "No" && read_only == "True")
                                //                              {
                                // current.Properties.SilentSetPropertyValue("EditAutocomplete", 0);
                                // webForm.Save();
                                //                                 writer.AddTableData(new string[] { Functions.linkObject(obj), obj.Description, controlName, hidden.ToString(), suggest });

                                //                             }
                                // }
                            } 
                            catch (Exception e)
                            {
                               // KBDoctorOutput.Message(e.Message);
                            }
                        }
                    }
                }
                writer.AddFooter();
                writer.Close();

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                bool success = true;
                output.EndSection("KBDoctor", title, success);


            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public static void ListProperties()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - List Properties";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                IOutputService output = CommonServices.Output;
                output.StartSection("KBDoctor", title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Level", "Property Name", "Value", "Is Default", "MSBUILD Command" });



                foreach (Property kbp in kbserv.CurrentKB.Properties.Properties)
                {
                    string kbpvalue = "";
                    if (kbp.Value != null)
                        kbpvalue = kbp.Value.ToString();

                    writer.AddTableData(new string[] { "KB", kbp.Name, kbpvalue, kbp.IsDefault.ToString(), String.Format("SetKnowledgeBaseProperty Name={0} Value={1} ", kbp.Name, kbpvalue) });
                }

                foreach (Property kbp in kbserv.CurrentKB.DesignModel.Properties)
                {
                    string kbpvalue = "";
                    if (kbp.Value != null)
                        kbpvalue = kbp.Value.ToString();

                    writer.AddTableData(new string[] { "VER", kbp.Name, kbpvalue, kbp.IsDefault.ToString(), String.Format("SetVersionProperty Name={0} Value={1} ", kbp.Name, kbpvalue) });
                }

                foreach (Property kbp in kbserv.CurrentKB.DesignModel.Environment.TargetModel.Properties)
                {
                    string kbpvalue = "";
                    if (kbp.Value != null)
                        kbpvalue = kbp.Value.ToString();

                    writer.AddTableData(new string[] { "ENV", kbp.Name, kbpvalue, kbp.IsDefault.ToString(), String.Format("SetEnvironmentProperty Name={0} Value={1} ", kbp.Name, kbpvalue) });
                }

                KBModel design = UIServices.KB.CurrentModel;
                KBModel target = design.Environment.TargetModel;

                /*  sacar para GX17

                foreach (GxGenerator gen in target.Parts.Get<GeneratorsPart>().Generators)
                {
                    foreach (Property genp in gen.Properties.Properties)
                    {
                        string kbpvalue = "";
                        if (genp.Value != null)
                            kbpvalue = genp.Value.ToString();
                        string msbuildstr = string.Format(" SetGeneratorProperty Generator={0} Name = {1} Value={2} ", gen.Description, genp.Name, kbpvalue);
                        writer.AddTableData(new string[] { "GEN", genp.Name, kbpvalue, genp.IsDefault.ToString(), msbuildstr });
                    }
                }
                */

                foreach (GxDataStore ds in target.Parts.Get<DataStoresPart>().DataStores)
                {
                    foreach (Property dsp in ds.Properties.Properties)
                    {
                        string kbpvalue = "";
                        if (dsp.Value != null)
                            kbpvalue = dsp.Value.ToString();
                    //    string msbuildstr = string.Format(" SetDataStoreProperty Generator={0} Name = {1} Value={2} ", ds.ds.Description, dsp.Name, kbpvalue);
             //           writer.AddTableData(new string[] { "DS", dsp.Name, kbpvalue, dsp.IsDefault.ToString(), //msbuildstr });
                    }
                }



                /*            KBEnvironment kBEnvironment = UIServices.KB.WorkingEnvironment;

                            foreach (KBModelPart kBObjectPart in  kBEnvironment.TargetModel.Parts)
                            {
                                KBDoctorOutput.Message("PART:" + kBObjectPart.Name); 
                                foreach (Property kbp in kbserv.CurrentKB.DesignModel.Environment.TargetModel.Properties)
                                {
                                    string kbpvalue = "";
                                    if (kbp.Value != null)
                                        kbpvalue = kbp.Value.ToString();

                                    writer.AddTableData(new string[] { "PART", kbp.Name, kbpvalue, kbp.IsDefault.ToString(), "MSBUILD " });
                                }
                            }

                            foreach (GeneratorCategory gen in GeneratorCategory.GetAll(kbserv.CurrentModel))
                            {
                                KBDoctorOutput.Message("PART:" );
                            } */


                writer.AddFooter();
                writer.Close();

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                bool success = true;
                output.EndSection("KBDoctor", title, success);


            }
            catch (Exception e)
            {
                bool success = false;
                KBDoctorOutput.Message(e.Message + e.StackTrace);
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }
        
        public static void ListNamespace()
            {
            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - List Namespace";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                IOutputService output = CommonServices.Output;
                output.StartSection("KBDoctor", title);

                KBDoctorOutput.Message("");
                KBDoctorOutput.Message("===== SOAP NAMESPACE IN PROCEDURE ========");

                foreach (Procedure p in Procedure.GetAll(kbserv.CurrentModel))
                {
                    string propval = p.GetPropertyValueString("SOAP_NAMESPACE");
                    Artech.Common.Properties.Property prop = p.GetProperty("SOAP_NAMESPACE");

                    if (propval != "" && !prop.IsDefault)
                    {
                        KBDoctorOutput.Message(p.Name + " Namespace:" + propval);
                        p.SetPropertyValue(prop.Name, null);
                    }

                }

                KBDoctorOutput.Message("");
                KBDoctorOutput.Message("===== xml NAMESPACE IN SDT ========");
                foreach (SDT sdt in SDT.GetAll(kbserv.CurrentModel))
                {
                    ListSdtNamespace(sdt.SDTStructure.Root, sdt.Name);

                }

            }

            catch (Exception e)
            {
                bool success = false;
                KBDoctorOutput.Message(e.Message + " " + e.InnerException);
                KBDoctorOutput.EndSection(title, success);
            }

         
        }

        private static void ListSdtNamespace(SDTLevel level, string sdtName)
        {
            foreach (var childItem in level.GetItems<SDTItem>())
            {
                string sp = childItem.GetPropertyValue<string>("idXmlNamespace");
                if (sp != "")
                    KBDoctorOutput.Message("SDT: " + sdtName + " Element: " + childItem.Name + " Namespace: " + childItem.GetPropertyValue<string>("idXmlNamespace"));
            }
            foreach (var childLevel in level.GetItems<SDTLevel>())
                ListSdtNamespace(childLevel, sdtName);

        }

    }
}

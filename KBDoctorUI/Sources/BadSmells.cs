using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.WebForm;
using Concepto.Packages.KBDoctorCore.Sources;
using System;
using System.Collections.Generic;
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
                writer.AddTableHeader(new string[] { "Object", "Description", "Control Name", "Col Visible", "Suggest" });


                //All useful objects are added to a collection
                foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())

                {
                    string objName = obj.Name;

                    if (obj is WebPanel)
                    {
                        //KBDoctorOutput.Message( "Procesing.." + obj.Name);
                        WebFormPart webForm = obj.Parts.Get<WebFormPart>();
                        foreach (IWebTag current in WebFormHelper.EnumerateWebTag(webForm))
                        {
                            if (current.Node.ParentNode.Name == "gxGrid" && current.Type == WebTagType.Column)
                            {
                                string controlName = current.Properties.GetPropertyValueString("ControlName");
                                string controltype = current.Properties.GetPropertyValueString("ControlType");

                                bool hidden = current.Properties.GetPropertyValue<bool>("ColVisible");
                                //  string att = current.Properties.GetPropertyValueString("Attribute");
                                if (controltype == "Dynamic Combo Box")
                                {
                                    KBDoctorOutput.Message( ">>>>Procesing.." + obj.Name + "-" + current.Type);
                                    writer.AddTableData(new string[] { Functions.linkObject(obj), obj.Description, controlName, hidden.ToString(), "N/A" });
                                }
                                string suggest = current.Properties.GetPropertyValueString("EditAutocomplete");
                                object suggest_obj = current.Properties.GetPropertyValue("EditAutocomplete");
                                string read_only = current.Properties.GetPropertyValueString("ColReadOnly");
                                if (controltype == "Edit" && suggest != "No" && read_only == "True")
                                {
                                    current.Properties.SilentSetPropertyValue("EditAutocomplete", 0);
                                    webForm.Save();
                                    writer.AddTableData(new string[] { Functions.linkObject(obj), obj.Description, controlName, hidden.ToString(), suggest });

                                }
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

    }
}

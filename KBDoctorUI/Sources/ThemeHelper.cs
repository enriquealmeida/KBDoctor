using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Artech.Architecture.Common.Collections;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Diagnostics;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Udm.Framework.References;
using Artech.Genexus.Common.Parts.WebForm;
using Artech.Common.Collections;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    class ThemeHelper
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
                writer.AddTableHeader(new string[] { "Object", "Description", "Control Name", "Col Visible" });


                //All useful objects are added to a collection
                foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())

                {
                        string objName = obj.Name;
                       
                        if (obj is WebPanel)
                        {
                            //output.AddLine("KBDoctor", "Procesing.." + obj.Name);
                            WebFormPart webForm = obj.Parts.Get<WebFormPart>();
                            foreach (IWebTag current in WebFormHelper.EnumerateWebTag(webForm))
                            {
                                if (current.Node.ParentNode.Name == "gxGrid" && current.Type == WebTagType.Column)
                                {
                                    string controlName = current.Properties.GetPropertyValueString("ControlName");
                                    string controltype = current.Properties.GetPropertyValueString("ControlType");
                                    bool  hidden = current.Properties.GetPropertyValue<bool>("ColVisible");
                                  //  string att = current.Properties.GetPropertyValueString("Attribute");
                                    if (controltype == "Dynamic Combo Box")
                                    {
                                        output.AddLine("KBDoctor", ">>>>Procesing.." + obj.Name + "-" + current.Type);
                                        writer.AddTableData(new string[] { Functions.linkObject(obj) , obj.Description, controlName,hidden.ToString()   });
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

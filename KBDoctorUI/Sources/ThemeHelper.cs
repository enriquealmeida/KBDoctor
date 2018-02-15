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
        public static void ObjectsWithVarNotBasedOnAtt()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Object with variables not based on attribute/domain";
            string outputFile = Functions.CreateOutputFile(kbserv, title);


            IOutputService output = CommonServices.Output;
            output.StartSection(title);
            
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Type", "Name", "Variable", "Attribute", "Domain" });


            //All useful objects are added to a collection
            foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
            {

                output.AddLine("Procesing.." + obj.Name);
                if ((obj is Transaction) || (obj is WebPanel))
                {
                    WebFormPart webForm = obj.Parts.Get<WebFormPart>();
                    foreach (IWebTag current in WebFormHelper.EnumerateWebTag(webForm))
                    {
                        output.AddLine("Procesing.." + obj.Name + "-" );
                    }
                    
                 }

            }
            writer.AddFooter();
            writer.Close();
            writer.Dispose();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

            /*   
            type = obj.TypeDescriptor.Description;
                name = obj.Name;
                primerVarObj = true;
                if (isGenerated(obj))
                {
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    if (vp != null)
                    {
                        foreach (Variable v in vp.Variables)
                        {
                            if ((!v.IsStandard) && (v.AttributeBasedOn == null) && (v.DomainBasedOn) == null)
                            {
                                varName = v.Name;
                                if (primerVarObj)
                                {
                                    primerVarObj = false;
                                    writer.AddTableData(new string[] { type, name, "", "", "" });
                                }
                                string suggestedDomains = "";
                                foreach (Domain d in Domain.GetAll(kbserv.CurrentModel))
                                {
                                    if ((v.Type == d.Type) && (v.Length == d.Length) && (v.Decimals == d.Decimals) && (v.Signed == d.Signed))
                                    {
                                        if (suggestedDomains != "")
                                        {
                                            suggestedDomains += ", ";
                                        }
                                        suggestedDomains += "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;AssignDomainToVariable&objName=" + obj.Name + "&objtype=" + type + "&varId=" + v.Id.ToString() + "&domainName=" + d.Name + "\">" + d.Name + "</a>";
                                    }
                                }
                                string suggestedAttribute = "";
                                foreach (Artech.Genexus.Common.Objects.Attribute a in Artech.Genexus.Common.Objects.Attribute.GetAll(kbserv.CurrentModel))
                                {
                                    if ((v.Type == a.Type) && (v.Length == a.Length) && (v.Decimals == a.Decimals) && (v.Signed == a.Signed))
                                    {
                                        if (suggestedAttribute != "")
                                        {
                                            suggestedAttribute += ", ";
                                        }
                                        suggestedAttribute += "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;AssignAttributeToVariable&objName=" + obj.Name + "&objtype=" + type + "&varId=" + v.Id.ToString() + "&attId=" + a.Id.ToString() + "\">" + a.Name + "</a>";
                                    }
                                }
                                writer.AddTableData(new string[] { "", "", varName, suggestedAttribute, suggestedDomains });
                            }
                        }
                    }
                }
            }

            
             * */
        }

    }
}

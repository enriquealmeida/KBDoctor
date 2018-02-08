using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.Language;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Architecture.Common.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Artech.Udm.Framework.References;
using Artech.Common.Framework.Selection;
using Artech.Common.Framework.Commands;
using Artech.Genexus.Common.Types;
using Artech.Architecture.Common.Descriptors;
using Artech.Genexus.Common.Services;
using Artech.Architecture.BL.Framework.Services;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Parts.WebForm;
using Artech.Architecture.UI.Framework.Helper;
using Artech.Common.Diagnostics;
using Artech.Genexus.Common.Parts.Layout;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using Artech.Udm.Framework;

namespace Concepto.Packages.KBDoctorCore.Sources
{
    static class CleanKB
    {
        internal static void CleanKBObjectVariables(KBObject kbObj, IOutputService output)
        {
            try
            {
                VariablesPart variablesPart = null;
                if (!kbObj.IsCurrentVersion || kbObj.Dirty)
                {
                    kbObj = KBObject.Get(kbObj.Model, kbObj.Key);
                }
                List<Variable> list = new List<Variable>();
                List<IEnumerable<VariableReference>> list2 = new List<IEnumerable<VariableReference>>();
                List<VariableReference> list3 = new List<VariableReference>();
                string text = null;
                foreach (KBObjectPart current in kbObj.Parts)
                {
                    if (current is VariablesPart)
                    {
                        variablesPart = (VariablesPart)current;
                    }
                    else
                    {
                        if (current is IHasVariableReferences)
                        {
                            list2.Add(((IHasVariableReferences)current).GetReferencedVariables());
                        }
                    }
                    if (current is LayoutPart && ((LayoutPart)current).Layout != null)
                    {
                        using (IEnumerator<IReportBand> enumerator2 = ((LayoutPart)current).Layout.ReportBands.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                IReportBand current2 = enumerator2.Current;
                                foreach (IReportComponent current3 in current2.Controls)
                                {
                                    if (current3 is ReportAttribute)
                                    {
                                        VariableReference item = new VariableReference(current3.Name);
                                        list3.Add(item);
                                    }
                                }
                            }
                            continue;
                        }
                    }
                    if (current is WebFormPart && ((WebFormPart)current).Document != null)
                    {
                        text = ((WebFormPart)current).Document.OuterXml;
                    }
                }
                if (list3.Count > 0)
                {
                    list2.Add(list3);
                }
                if (variablesPart != null && !variablesPart.GetPropertyValue<bool>("IsDefault"))
                {
                    foreach (Variable current4 in variablesPart.Variables)
                    {
                        if (!current4.IsAutoDefined && !current4.IsStandard && (Artech.Genexus.Common.Properties.ATT.Dimensions_Enum)Enum.Parse(typeof(Artech.Genexus.Common.Properties.ATT.Dimensions_Enum), current4.GetPropertyValue<string>("AttNumDim")) == Artech.Genexus.Common.Properties.ATT.Dimensions_Enum.Scalar)
                        {
                            bool flag = false;
                            foreach (IEnumerable<VariableReference> current5 in list2)
                            {
                                foreach (VariableReference current6 in current5)
                                {
                                    if (current6.Name.Replace("&", "").Equals(current4.Name.Replace("&", ""), StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (flag)
                                {
                                    break;
                                }
                            }
                            if (!flag && (text == null || !text.Contains("\"var:" + current4.Id + "\"")))
                            {
                                list.Add(current4);
                            }
                        }
                    }
                    if (list.Count > 0)
                    {
                        string text2 = "";
                        foreach (Variable current7 in list)
                        {
                            text2 = text2 + ", " + current7.Name;
                            variablesPart.Remove(current7);
                        }
                        OutputMessages outputMessages = new OutputMessages();
                        if (kbObj.Validate(outputMessages))
                        {
                            kbObj.Save();
                            output.AddLine(kbObj.Name + "Object cleaned successfully. Variables deleted: " + text2.Substring(2));

                        }
                        using (IEnumerator<BaseMessage> enumerator8 = outputMessages.GetEnumerator())
                        {
                            while (enumerator8.MoveNext())
                            {
                                BaseMessage current8 = enumerator8.Current;
                                if (current8.Level == MessageLevel.Error)
                                {
                                    output.AddErrorLine(current8.Text);
                                }
                            }

                        }
                    }

                }

            }
            catch (Exception ex)
            {
                output.AddWarningLine("Object '" + kbObj.Name + "' was not cleaned because an error ocurred: " + ex.Message);
            }

        }
    }
}

using System;
using System.Xml;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using Artech.Udm.Framework;
using Artech.Udm.Framework.References;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Types;
using Artech.Genexus.Common.Services;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.WebForm;
using Artech.Genexus.Common.Parts.Layout;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.CustomTypes;
using Artech.Common.Framework.Selection;
using Artech.Common.Framework.Commands;
using Artech.Common.Diagnostics;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.UI.Framework.Helper;
using Artech.Architecture.Language;
using Artech.Architecture.Common.Services;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Collections;
using Artech.Architecture.BL.Framework.Services;
using Concepto.Packages.KBDoctor;

namespace Concepto.Packages.KBDoctorCore.Sources
{
    static class CleanKB
    {
        private static void CleanAllWebForm(KBObject obj)
        {
            List<KBObjectPart> parts = new List<KBObjectPart>() { obj.Parts[typeof(WebFormPart).GUID] };
            parts.ForEach(part =>
            {
                if (part.Default.CanCalculateDefault())
                    part.Default.SilentSetIsDefault(true);
            }
                          );
        }

        private static void CleanAllWInForm(KBObject obj)
        {
            List<KBObjectPart> parts = new List<KBObjectPart>() { obj.Parts[typeof(WinFormPart).GUID] };
            parts.ForEach(part =>
            {
                if (part.Default.CanCalculateDefault())
                    part.Default.SilentSetIsDefault(true);
            }
                        );

        }
        private static void CleanAllEvents(KBObject obj)
        {
            EventsPart evPart = obj.Parts.Get<EventsPart>();
            evPart.Source = "";
        }

        private static void CleanAllProcedurePart(KBObject obj)
        {
            ProcedurePart procPart = obj.Parts.Get<ProcedurePart>();
            procPart.Source = "";
        }

        private static void CleanAllConditions(KBObject obj)
        {
            ConditionsPart cndPart = obj.Parts.Get<ConditionsPart>();
            cndPart.Source = "";
        }

        private static void CleanAllRules(KBObject obj)
        {
            RulesPart rulesPart = obj.Parts.Get<RulesPart>();
            rulesPart.Source = "";
        }
        public static void CleanAllVars(KBObject obj)
        {
            ArrayList idVasrBorrar = new ArrayList();

            VariablesPart vp = obj.Parts.Get<VariablesPart>();
            ArrayList variables = new ArrayList();
            foreach (Variable v in vp.Variables)
            {
                if (!v.IsStandard)
                    variables.Add(v);
            }
            foreach (Variable v in variables)
            {
                vp.Remove(v);
            }

        }

        /// <summary>
        /// Clean and destroy objects. Initizlize objects 
        /// </summary>
        public static void CleanObjects(KnowledgeBase kb, IEnumerable<KBObject> kbojs, IOutputService output)
        {

            output.StartSection("Cleaning objects");
            foreach (KBObject obj in kbojs)
            {
                CleanObject(obj, output);
            }
            output.EndSection("Cleaning objects", true);

        }

        public static void CleanObject(KBObject obj, IOutputService output)
        {

            KBDoctorOutput.Message("Cleaning object " + obj.Name);
            if (obj is Transaction)
            {
                KBDoctorCore.Sources.API.CleanKBObjectVariables(obj, output);
                CleanAllRules(obj);
                CleanAllWebForm(obj);
                CleanAllWInForm(obj);
                CleanAllEvents(obj);
                CleanAllVars(obj);
                obj.SetPropertyValue(Artech.Genexus.Common.Properties.TRN.MasterPage, WebPanelReference.NoneRef);
            }

            if (obj is Procedure)
            {
                CleanAllRules(obj);
                CleanAllProcedurePart(obj);
                CleanAllConditions(obj);
                CleanAllVars(obj);
            }
            if (obj is WebPanel)
            {
                CleanAllRules(obj);
                CleanAllWebForm(obj);
                CleanAllEvents(obj);
                CleanAllConditions(obj);
                CleanAllVars(obj);
            }
            if (obj is WorkPanel)
            {
                CleanAllRules(obj);
                CleanAllWInForm(obj);
                CleanAllEvents(obj);
                CleanAllConditions(obj);
                CleanAllVars(obj);
            }

            try
            {

                obj.Save();
            }
            catch (Exception e)
            {
                KBDoctorOutput.Message("Can't clean " + obj.Name + " Message: " + e.Message + "--" + e.StackTrace);
            }
        }

        internal static void RemoveObjectsNotCalled(KBModel kbmodel, IOutputService output, out List<string[]> lineswriter)
        {
            int callers;
            string remove = "";
            bool continuar = true;
            lineswriter = new List<string[]>();
            do
            {
                continuar = false;
                foreach (KBObject obj in kbmodel.Objects.GetAll())
                {
                    ICallableObject callableObject = obj as ICallableObject;
                    if ((callableObject != null) | (obj is Artech.Genexus.Common.Objects.Attribute)
                        | obj is Artech.Genexus.Common.Objects.Table | obj is Domain | obj is ExternalObject | obj is Image | obj is SDT)
                    {
                        callers = 0;
                        foreach (EntityReference reference in obj.GetReferencesTo(LinkType.UsedObject))
                        {
                            callers = callers + 1;
                        }

                        if (callers == 0)
                        {
                            if ((obj is Transaction) | obj is Table | obj is Artech.Genexus.Common.Objects.Attribute | obj is Domain | obj is Image)
                            {
                                remove = "";
                            }
                            else
                            {
                                remove = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;RemoveObject&guid=" + obj.Guid.ToString() + "\">Remove</a>";
                            }
                            string objNameLink = Utility.linkObject(obj);
                            string isMainstr = (Utility.isMain(obj) ? "Main" : string.Empty);
                            string isGeneratedstr = (Utility.isGenerated(obj) ? "Yes" : string.Empty);
                            if (!Utility.isMain(obj))
                            {
                                if (remove != "")
                                {
                                    try
                                    {
                                        obj.Delete();
                                        KBDoctorOutput.Message("REMOVING..." + obj.Name);
                                        remove = "REMOVED!";
                                        objNameLink = obj.Name;
                                        continuar = true;
                                    }
                                    catch  { };

                                }
                                lineswriter.Add(new string[] { obj.TypeDescriptor.Name, objNameLink, remove, isGeneratedstr, isMainstr });
                            }
                            if ((obj is Transaction) && (obj.GetPropertyValue<bool>(Artech.Genexus.Common.Properties.TRN.GenerateObject)))
                            {
                                try
                                {
                                    obj.SetPropertyValue(Artech.Genexus.Common.Properties.TRN.GenerateObject, false);
                                    CleanObject(obj, output);
                                }
                                catch { };

                            }
                        }
                    }
                }
            } while (continuar);
        }


        internal static void RemoveAttributesWithoutTable(KBModel kbmodel, IOutputService output, out List<string[]> lineswriter)
        {
            lineswriter = new List<string[]>();
            // grabo todos los atributos en una colección
            List<Artech.Genexus.Common.Objects.Attribute> attTodos = new List<Artech.Genexus.Common.Objects.Attribute>();
            foreach (Artech.Genexus.Common.Objects.Attribute a in Artech.Genexus.Common.Objects.Attribute.GetAll(kbmodel))
            {
                attTodos.Add(a);
            }

            // voy borrando todos los atributos que estan en alguna tabla
            foreach (Table t in Table.GetAll(kbmodel))
            {
                foreach (EntityReference reference in t.GetReferences(LinkType.UsedObject))
                {
                    KBObject objRef = KBObject.Get(kbmodel, reference.To);
                    if (objRef is Artech.Genexus.Common.Objects.Attribute)
                    {
                        Artech.Genexus.Common.Objects.Attribute a = (Artech.Genexus.Common.Objects.Attribute)objRef;
                        attTodos.Remove(a);
                    }
                }
            }

            // TODO: Atributos en dataviews

            foreach (Artech.Genexus.Common.Objects.Attribute a in attTodos)
            {
                if (!Utility.AttIsSubtype(a))
                {
                    Utility.KillAttribute(a);
                    string strRemoved = "";
                    try
                    {
                        a.Delete();
                        KBDoctorOutput.Message("Atribute deleted: " + a.Name);
                    }
                    catch (Exception e)
                    {
                        output.AddErrorLine("Can't delete " + a.Name + " Msg: " + e.Message);

                    }
                    string attNameLink = Utility.linkObject(a); //"<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;OpenObject&name=" + a.Guid.ToString() + "\">" + a.Name + "</a>";
                    strRemoved = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;RemoveObject&guid=" + a.Guid.ToString() + "\">Remove</a>";
                    string Picture = Utility.FormattedTypeAttribute(a);
                    lineswriter.Add(new string[] { strRemoved, attNameLink, a.Description, Picture });
                }
            }
        }

        internal static void CleanKBObjectVariables(KBObject kbObj, IOutputService output, ref string recomendations)
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
                            string recommend = "Object '" + kbObj.Name + "' cleaned successfully. Variables deleted: " + text2.Substring(2);
                            KBDoctorOutput.Message(recommend);
                            recomendations += recommend + "<br>";

                        }
                        using (IEnumerator<BaseMessage> enumerator8 = outputMessages.GetEnumerator())
                        {
                            while (enumerator8.MoveNext())
                            {
                                BaseMessage current8 = enumerator8.Current;
                                if (current8.Level == MessageLevel.Error)
                                {
                                    output.AddErrorLine("KBDoctor", current8.Text);
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                output.AddWarningLine("KBDoctor", "Object '" + kbObj.Name + "' was not cleaned because an error ocurred: " + ex.Message);
            }
        }
    }
}

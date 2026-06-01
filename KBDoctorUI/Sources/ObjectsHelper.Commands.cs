using System;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using Artech.Udm.Framework.References;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Services;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.SDT;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Helpers;
using Artech.Genexus.Common.CustomTypes;

using Artech.Common.Diagnostics;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.UI.Framework.Objects;
using Artech.Architecture.Language.Services;
using Artech.Architecture.Language.Parser;
using Artech.Architecture.Common;
using Artech.Architecture.Common.Services;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Collections;
using Artech.Architecture.BL.Framework.Services;
using GeneXus.Server.Contracts;
using Artech.Architecture.Language.ComponentModel;

using Artech.Udm.Framework;
using Concepto.Packages.KBDoctorCore.Sources;
using System.Threading;
using Artech.Genexus.Common.Parts.WebForm;
using API = Concepto.Packages.KBDoctorCore.Sources.API;
using Concepto.Packages.KBDoctor.Sources;

using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.Parts.Form;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Packages.Patterns.Objects;
using Artech.Patterns.WorkWithDevices.Helpers;
using Artech.Patterns.WorkWithDevices;
using Artech.Common.Helpers.Reflection;


namespace Concepto.Packages.KBDoctor
{
    partial class ObjectsHelper
    {
        public static void ListProcedureCallWebPanelTransaction()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - List Procedure that call Webpanel or Transaction";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                KBDoctorOutput.StartSection(title);


                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Description", "Reference", "Description" });

                foreach (KBObject obj in UIServices.KB.CurrentModel.Objects.GetByPropertyValue(Properties.PRC.MainProgram,true))
                {
                    KBObjectCollection reachablesObjects = new KBObjectCollection(); ;
                    if (obj is Procedure)
                    {
                        KBDoctorOutput.Message("MAIN:" + obj.Name);
                        MarkReachables(output, obj, reachablesObjects);
                    }

                }

                writer.AddFooter();
                writer.Close();

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                bool success = true;
                KBDoctorOutput.EndSection(title, success);
            }
            catch (Exception e)
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public static void RemoveObject(object[] parameters)
        {

            string objtype = "";
            string objName = "";
            string mensaje = "";
            Guid guid = Guid.Empty;

            foreach (object o in parameters)
            {
                Dictionary<string, string> dic = (Dictionary<string, string>)o;

                foreach (string s in dic.Values)
                {
                    try
                    {
                        guid = new Guid(s);
                    }
                    catch (FormatException)
                    {
                        guid = Guid.Empty;
                    }
                }
            }
            KBObject obj = UIServices.KB.CurrentModel.Objects.Get(guid);
            if (!Utility.IsUserEditableObject(obj))
            {
                MessageBox.Show("The selected object is read-only and cannot be removed.", "Could not remove object", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            objtype = obj.TypeDescriptor.Name;
            objName = obj.Name;
            mensaje = string.Format("Are you sure you want to delete " + objtype.Trim() + " {0}?", objName);
            DialogResult dr = MessageBox.Show(mensaje, "Remove object", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {


                try
                {
                    obj.Delete();
                    MessageBox.Show(objtype + " " + objName + " was successfully removed.");
                }
                catch (GxException gxe)
                {
                    MessageBox.Show(gxe.Message, "Could not remove " + objtype, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        public static void RemoveUnreferencedObjectsInUserModules()
        {
            List<KBObject> objectsToDelete = KbStatistics.GetUnreferencedObjectsInUserModules(UIServices.KB.CurrentModel)
                .Where(obj => obj.CanDelete)
                .Select(obj => obj.Object)
                .Where(obj => Utility.IsUserEditableObject(obj) && !(obj is Transaction))
                .ToList();

            if (objectsToDelete.Count == 0)
            {
                MessageBox.Show("There are no deletable unreferenced objects in user modules.", "Remove unreferenced objects", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string message = string.Format("Are you sure you want to delete {0} unreferenced object(s) in user modules?", objectsToDelete.Count);
            DialogResult dr = MessageBox.Show(message, "Remove unreferenced objects", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr != DialogResult.Yes)
            {
                return;
            }

            string title = "KBDoctor - Remove unreferenced objects in user modules";
            int removed = 0;
            int failed = 0;

            KBDoctorOutput.StartSection(title);
            foreach (KBObject obj in objectsToDelete)
            {
                try
                {
                    string objectName = obj.TypeDescriptor.Name + " " + obj.QualifiedName;
                    obj.Delete();
                    removed++;
                    KBDoctorOutput.Message("Removed: " + objectName);
                }
                catch (GxException gxe)
                {
                    failed++;
                    KBDoctorOutput.Error("Could not remove " + obj.Name + ": " + gxe.Message);
                }
                catch (Exception ex)
                {
                    failed++;
                    KBDoctorOutput.Error("Could not remove " + obj.Name + ": " + ex.Message);
                }
            }

            KBDoctorOutput.Message("Removed: " + removed + ". Failed: " + failed + ".");
            KBDoctorOutput.EndSection(title, failed == 0);
            MessageBox.Show("Removed: " + removed + ". Failed: " + failed + ".", "Remove unreferenced objects", MessageBoxButtons.OK, failed == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            KbStats.ListUnreferencedObjectsInUserModules();
        }

        public static void OpenObject(object[] parameters)
        {
            foreach (object o in parameters)
            {
                Dictionary<string, string> dic = (Dictionary<string, string>)o;
                int cant = 0;
                foreach (string s in dic.Values)
                {
                    if (cant == 1)
                    {

                        Guid guid = new Guid(s);
                        KBObject obj = UIServices.KB.CurrentModel.Objects.Get(guid);
                        UIServices.DocumentManager.OpenDocument(obj, OpenDocumentOptions.CurrentVersion);
                    }
                    cant++;
                }
            }
        }

        public static void SetObjectPropertyText(object[] parameters)
        {
            string guidValue = KBDoctorWebForms.GetParameter(parameters, "guid");
            string property = KBDoctorWebForms.GetParameter(parameters, "property");
            Guid guid;
            if (!Guid.TryParse(guidValue, out guid) || string.IsNullOrEmpty(property))
            {
                KBDoctorOutput.Error("Missing object or property.");
                return;
            }

            KBObject obj = UIServices.KB.CurrentModel.Objects.Get(guid);
            if (!Utility.IsUserEditableObject(obj))
            {
                KBDoctorOutput.Error("Object not found or read-only.");
                return;
            }

            KBDoctorWebForms.ShowTextInput(
                "KBDoctor - Set object property",
                "Set " + property + " for " + obj.Name,
                "ApplyObjectPropertyText",
                new Dictionary<string, string> { { "guid", guidValue }, { "property", property } },
                "value",
                obj.GetPropertyValueString(property));
        }

        public static void ApplyObjectPropertyText(object[] parameters)
        {
            string guidValue = KBDoctorWebForms.GetParameter(parameters, "guid");
            string property = KBDoctorWebForms.GetParameter(parameters, "property");
            string value = KBDoctorWebForms.GetParameter(parameters, "value");
            Guid guid;
            if (!Guid.TryParse(guidValue, out guid) || string.IsNullOrEmpty(property))
            {
                KBDoctorOutput.Error("Missing object or property.");
                return;
            }

            KBObject obj = UIServices.KB.CurrentModel.Objects.Get(guid);
            if (!Utility.IsUserEditableObject(obj))
            {
                KBDoctorOutput.Error("Object not found or read-only.");
                return;
            }

            obj.SetPropertyValue(property, value);
            obj.Save();
            KBDoctorOutput.Message("Updated " + property + " for " + obj.Name);
        }

        // TODO: Sin referencias textuales encontradas; revisar si se usa por comando/reflection antes de eliminar.
        public static void OpenObjectRules(object[] parameters)
        {
            foreach (object o in parameters)
            {
                Dictionary<string, string> dic = (Dictionary<string, string>)o;
                int cant = 0;
                foreach (string s in dic.Values)
                {
                    if (cant == 1)
                    {

                        Guid guid = new Guid(s);
                        KBObject obj = UIServices.KB.CurrentModel.Objects.Get(guid);

                        OpenDocumentOptions options = OpenDocumentOptions.CurrentVersion;

                    }
                    cant++;
                }
            }
        }

        public static void ObjectsWithVarNotBasedOnAtt()
        {
            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - Object with variables not based on attribute/domain";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                IOutputService output = CommonServices.Output;
                KBDoctorOutput.StartSection(title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Type", "Name", "Variable", "Picture", "Attribute", "Domain" });


                //All useful objects are added to a collection
                foreach (KBObject obj in Utility.EditableObjects(kbserv.CurrentModel.Objects.GetAll()))
                {


                    Boolean SaveObj = false;
                    if (isGenerated(obj) && (obj is Procedure))
                    {
                        KBDoctorOutput.Message( "Procesing.... " + obj.Name + " - " + obj.TypeDescriptor.Name);
                        string pic2 = (string)obj.GetPropertyValue("ATT_PICTURE");


                        VariablesPart vp = obj.Parts.Get<VariablesPart>();
                        if (vp != null)
                        {
                            foreach (Variable v in vp.Variables)
                            {
                                if ((!v.IsStandard))
                                {
                                    string attname = (v.AttributeBasedOn == null) ? "" : v.AttributeBasedOn.Name;
                                    string domname = (v.DomainBasedOn == null) ? "" : v.DomainBasedOn.Name;

                                    string picture = (string)v.GetPropertyValue("ATT_PICTURE");

                                    if (attname == "" && domname == "")
                                    {

                                        if (v.Name.ToLower() == "archivo" && v.Type == eDBType.CHARACTER && v.Length == 50)
                                            v.DomainBasedOn = Functions.DomainByName("Archivo");

                                        if (v.Name.ToLower() == "in" && v.Type == eDBType.VARCHAR && v.Length >= 9999)
                                            v.DomainBasedOn = Functions.DomainByName("XMLContenido");

                                        if (v.Name.ToLower() == "out" && v.Type == eDBType.VARCHAR && v.Length >= 9999)
                                            v.DomainBasedOn = Functions.DomainByName("XMLContenido");

                                        if (v.DomainBasedOn != null)
                                        {
                                            string vname = v.Name.ToLower();
                                            writer.AddTableData(new string[] { obj.TypeDescriptor.Name, Functions.linkObject(obj), v.Name, picture, attname, domname });
                                            SaveObj = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (SaveObj)
                            obj.Save();

                    }

                }
                writer.AddFooter();
                writer.Close();

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                bool success = true;
                KBDoctorOutput.EndSection(title, success);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        // TODO: Sin referencias textuales encontradas; revisar si se usa manualmente antes de eliminar.
        public static string VariablesNotBasedAttributesOrDomain(KBObject obj)
        {
            IKBService kbserv = UIServices.KB;
            KBModel kbmod = kbserv.CurrentModel;

            string variables = "";
            VariablesPart vp = obj.Parts.Get<VariablesPart>();
            if (vp != null)
            {
                foreach (Variable v in vp.Variables)
                {
                    if ((!v.IsStandard) && (v.AttributeBasedOn == null) && (v.DomainBasedOn == null) && (v.Type != eDBType.GX_USRDEFTYP)
                        && (v.Type != eDBType.GX_SDT) && (v.Type != eDBType.GX_EXTERNAL_OBJECT) && (v.Type != eDBType.Boolean))
                    {
                        variables += v.Name + " " + Utility.FormattedTypeVariable(v).ToLower() + "<br>" + Environment.NewLine;
                        string objaux = obj.Name + "," + v.Name + "," + Utility.FormattedTypeVariable(v);
                        Functions.AddLineSummary("ObjectsVariableSinDom.Txt", objaux);

                    }
                }
            }
            return variables;
        }

        public static void AssignDomainToVariable(object[] parameters)
        {
            IKBService kbserv = UIServices.KB;
            string domainName = "";
            string objName = "";
            string type = "";
            int varid = 0;
            Artech.Architecture.UI.Framework.Objects.IGxDocument document;
            foreach (object o in parameters)
            {
                Dictionary<string, string> dic = (Dictionary<string, string>)o;
                int cant = 0;

                foreach (string s in dic.Values)
                {
                    switch (cant)
                    {
                        case 1:
                            objName = s;
                            break;
                        case 2:
                            type = s;
                            break;
                        case 3:
                            varid = int.Parse(s);
                            break;
                        case 4:
                            domainName = s;
                            break;
                        default:
                            break;
                    }
                    cant++;
                }
            }
            if ((objName != "") && (domainName != "") && (type != "") && (varid != 0))
            {
                Domain d = Functions.DomainByName(domainName);
                foreach (KBObject obj in UIServices.KB.CurrentModel.Objects.GetByName("Objects", null, objName))
                {
                    Variable v = obj.Parts.Get<VariablesPart>().GetVariable(varid);
                    v.DomainBasedOn = d;
                    if (UIServices.DocumentManager.IsOpenDocument(obj, out document))
                    {
                        ObjectsHelper.SetDocumentDirty(document);
                        UIServices.TrackSelection.OnSelectChange(document.Object, null);
                        MessageBox.Show("Object open, save to complete the operation.");
                    }
                    else
                    {
                        obj.Save();
                        MessageBox.Show("Variable assigned.");
                    }
                }
            }


        }

        public static void AssignAttributeToVariable(object[] parameters)
        {
            IKBService kbserv = UIServices.KB;
            string objName = "";
            string type = "";
            int varid = 0;
            int attid = 0;
            Artech.Architecture.UI.Framework.Objects.IGxDocument document;
            foreach (object o in parameters)
            {
                Dictionary<string, string> dic = (Dictionary<string, string>)o;
                int cant = 0;

                foreach (string s in dic.Values)
                {
                    switch (cant)
                    {
                        case 1:
                            objName = s;
                            break;
                        case 2:
                            type = s;
                            break;
                        case 3:
                            varid = int.Parse(s);
                            break;
                        case 4:
                            attid = int.Parse(s);
                            break;
                        default:
                            break;
                    }
                    cant++;
                }
            }


            if (objName != "" && attid != 0 && type != "" && varid != 0)
            {
                Artech.Genexus.Common.Objects.Attribute a = Artech.Genexus.Common.Objects.Attribute.Get(UIServices.KB.CurrentModel, attid);
                foreach (KBObject obj in UIServices.KB.CurrentModel.Objects.GetByName("Objects", null, objName))
                {
                    Variable v = obj.Parts.Get<VariablesPart>().GetVariable(varid);
                    v.AttributeBasedOn = a;

                    if (UIServices.DocumentManager.IsOpenDocument(obj, out document))
                    {
                        ObjectsHelper.SetDocumentDirty(document);
                        UIServices.TrackSelection.OnSelectChange(document.Object, null);
                        MessageBox.Show("Object open, save to complete the operation.");
                    }
                    else
                    {
                        obj.Save();
                        MessageBox.Show("Variable assigned.");
                    }
                }
            }
        }

        public static void AssignAttributeOrDomainToVariable(object[] parameters)
        {
            string guidValue = KBDoctorWebForms.GetParameter(parameters, "guid");
            string varName = KBDoctorWebForms.GetParameter(parameters, "varName");
            Guid guid;
            if (!Guid.TryParse(guidValue, out guid) || string.IsNullOrEmpty(varName))
            {
                KBDoctorOutput.Error("Missing object or variable to assign.");
                return;
            }

            KBObject obj = UIServices.KB.CurrentModel.Objects.Get(guid);
            if (!Utility.IsUserEditableObject(obj))
            {
                KBDoctorOutput.Error("Object not found or read-only.");
                return;
            }

            VariablesPart vp = obj.Parts.Get<VariablesPart>();
            Variable variable = vp == null ? null : vp.Variables.FirstOrDefault(v => string.Equals(v.Name, varName, StringComparison.OrdinalIgnoreCase));
            if (variable == null)
            {
                KBDoctorOutput.Error("Variable not found: " + varName);
                return;
            }

            AttributeVariableDialogInfo info = new AttributeVariableDialogInfo();
            info.Filter = TypedObjectKind.Attribute | TypedObjectKind.Domain;
            info.DialogTitle = "Select domain/attribute for " + obj.Name + "." + varName;
            info.MultiSelection = false;
            IList<object> selectedObjects = GenexusUIServices.SelectAttributeVariable.SelectAttributeVariable(info);
            if (selectedObjects.Count == 0)
            {
                return;
            }

            Domain domain = selectedObjects[0] as Domain;
            if (domain != null)
            {
                variable.DomainBasedOn = domain;
            }
            else
            {
                Artech.Genexus.Common.Objects.Attribute attribute = selectedObjects[0] as Artech.Genexus.Common.Objects.Attribute;
                if (attribute == null)
                {
                    KBDoctorOutput.Error("Selected object is not an attribute or domain.");
                    return;
                }

                variable.AttributeBasedOn = attribute;
            }

            obj.Save();
            KBDoctorOutput.Message("Variable updated: " + obj.Name + "." + varName);
        }

        public static void IndexWithNotRefAtt()
        {
            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - Index with not referenced attributes";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                IOutputService output = CommonServices.Output;
                KBDoctorOutput.StartSection(title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Table", "Name", "Attribute", "Composition", "Remove Index", "Remove attribute from the index", "Remove attribute and next attributes from the index" });
                string remove = "";
                string remAtt = "";
                string remAttRigth = "";

                KBObjectCollection attUnreach = Unreachables("Attribute");
                KBObjectCollection indexes = new KBObjectCollection();
                KBObjectCollection indexatt = null;
                foreach (KBObject obj in Artech.Genexus.Common.Objects.Index.GetAll(kbserv.CurrentModel))
                {
                    if ((((Index)obj).Source == IndexSource.User) && (((Index)obj).Table != null))
                        indexes.Add(obj);
                }

                bool indexWithAtt = false;
                string composition = "";
                foreach (Artech.Genexus.Common.Objects.Attribute obj in attUnreach)
                {
                    indexatt = new KBObjectCollection();
                    foreach (Index ind in indexes)
                    {
                        indexWithAtt = false;
                        composition = "";

                        foreach (IndexMember im in ind.IndexStructure.Members)
                        {
                            if (composition != "")
                            {
                                composition += ", ";
                            }
                            composition += im.Attribute.Name;
                            if (im.Attribute.Id == obj.Id)
                            {
                                indexWithAtt = true;
                            }
                        }

                        if (indexWithAtt)
                        {
                            indexatt.Add(ind);
                            remove = Functions.CommandLink("RemoveIndexAttribute", "Remove Index", "mode", "1", "indId", ind.Id.ToString(), "attId", obj.Id.ToString());
                            remAtt = Functions.CommandLink("RemoveIndexAttribute", "Remove attribute from the index", "mode", "2", "indId", ind.Id.ToString(), "attId", obj.Id.ToString());
                            remAttRigth = Functions.CommandLink("RemoveIndexAttribute", "Remove attribute and next attributes from the index", "mode", "3", "indId", ind.Id.ToString(), "attId", obj.Id.ToString());
                            writer.AddTableData(new string[] { ind.Table.Name, ind.Name, obj.Name, composition, remove, remAtt, remAttRigth });
                        }
                    }
                    indexes.RemoveAll(indexatt);
                }
                writer.AddFooter();
                writer.Close();

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                bool success = true;
                KBDoctorOutput.EndSection(title, success);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public static KBObjectCollection Unreachables(string type)
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;

            KBObjectCollection reachablesObjects = new KBObjectCollection();
            KBObjectCollection unreachablesObjects = new KBObjectCollection();
            KBCategory mainCategory = KBCategory.Get(kbserv.CurrentModel, "Main Programs");
            foreach (KBObject obj in mainCategory.AllMembers)
            {
                MarkReachables(output, obj, reachablesObjects);
            }

            if (type == "Attribute")
            {
                foreach (KBObject obj in Artech.Genexus.Common.Objects.Attribute.GetAll(kbserv.CurrentModel))
                {
                    unreachablesObjects.Add(obj);
                }
            }
            else
            {
                foreach (KBObject obj in Utility.EditableObjects(kbserv.CurrentModel.Objects.GetAll()))
                {
                    ICallableObject callableObject = obj as ICallableObject;
                    if ((callableObject != null) || (obj is Artech.Genexus.Common.Objects.Attribute))
                    {
                        unreachablesObjects.Add(obj);
                    }
                }
            }
            unreachablesObjects.RemoveAll(reachablesObjects);
            return unreachablesObjects;
        }

        public static void RemoveIndexAttribute(object[] parameters)
        {
            IKBService kbserv = UIServices.KB;
            int type = 0;
            int indexId = 0;
            int attid = 0;
            string mensaje = "";
            string name = "";
            foreach (object o in parameters)
            {
                Dictionary<string, string> dic = (Dictionary<string, string>)o;
                int cant = 0;

                foreach (string s in dic.Values)
                {
                    switch (cant)
                    {
                        case 1:
                            type = int.Parse(s);
                            break;
                        case 2:
                            indexId = int.Parse(s);
                            break;
                        case 3:
                            attid = int.Parse(s);
                            break;
                        default:
                            break;
                    }
                    cant++;
                }
            }

            if ((type != 0) && (indexId != 0) && (attid != 0))
            {
                Index i = Artech.Genexus.Common.Objects.Index.Get(kbserv.CurrentModel, indexId);
                Table t = i.Table;
                name = i.Name;
                if (type == 1)
                {
                    mensaje = string.Format("Are you sure you want to remove the index {0}?", i.Name);
                    DialogResult dr = MessageBox.Show(mensaje, "Remove attribute", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        foreach (TableIndex index in t.TableIndexes.Indexes)
                        {
                            if (index.Index.Id == indexId)
                            {
                                t.TableIndexes.Indexes.Remove(index);
                                break;
                            }
                        }
                        i.Delete();
                        t.Save();
                        MessageBox.Show("Index " + name + " was successfully removed.");
                    }
                }
                else
                {
                    if (type == 2)
                    {
                        foreach (IndexMember im in i.IndexStructure.Members)
                        {
                            if (im.Attribute.Id == attid)
                            {
                                mensaje = string.Format("Are you sure you want to remove the attribute " + im.Attribute.Name + " from the index {0}?", i.Name);
                                DialogResult dr = MessageBox.Show(mensaje, "Remove attribute", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (dr == DialogResult.Yes)
                                {
                                    i.IndexStructure.Members.Remove(im);
                                    i.Save();
                                    MessageBox.Show("Index " + name + " was successfully updated.");
                                    break;
                                }
                            }
                        }

                    }
                    else
                    {
                        if (type == 3)
                        {
                            ArrayList indexToRemove = new ArrayList();
                            bool nextAtt = false;
                            foreach (IndexMember im in i.IndexStructure.Members)
                            {
                                if (im.Attribute.Id == attid)
                                {
                                    mensaje = string.Format("Are you sure you want to remove the attribute " + im.Attribute.Name + " and the next attributes from the index {0}?", i.Name);
                                    DialogResult dr = MessageBox.Show(mensaje, "Remove attribute", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                    if (dr == DialogResult.Yes)
                                    {
                                        indexToRemove.Add(im);
                                        nextAtt = true;
                                    }
                                }
                                else
                                    if (nextAtt)
                                    indexToRemove.Add(im);

                            }
                            if (nextAtt)
                            {
                                foreach (IndexMember im2 in indexToRemove)
                                    i.IndexStructure.Members.Remove(im2);
                                i.Save();
                                MessageBox.Show("Index " + name + " was successfully updated.");
                            }
                        }
                    }
                }
            }
        }

        public static void ObjectsWithVarsNotUsed()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool varused = true;
            string title = "KBDoctor - Object with variables not used";
            string varsNotUsed = "";
            string varname = "";
            string remove = "";

            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Type", "Name", "Clean" });

                foreach (KBObject obj in Utility.EditableObjects(kbserv.CurrentModel.Objects.GetAll()))
                {
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();

                    if ((vp != null) && isGenerated(obj))
                    {
                        KBDoctorOutput.Message( "Procesing.." + obj.Name);
                        varsNotUsed = "";
                        foreach (Variable v in vp.Variables)
                        {
                            if (!v.IsStandard)
                            {
                                varused = false;
                                ProcedurePart pp = obj.Parts.Get<ProcedurePart>();
                                if (pp != null)
                                {
                                    varused = VarUsedInText(pp.Source, varname);
                                }
                                if (!varused)
                                {
                                    RulesPart rp = obj.Parts.Get<RulesPart>();
                                    if (rp != null)
                                    {
                                        varused = VarUsedInText(rp.Source, varname);
                                    }
                                }
                                if (!varused)
                                {
                                    ConditionsPart cp = obj.Parts.Get<ConditionsPart>();
                                    if (cp != null)
                                    {
                                        varused = VarUsedInText(cp.Source, varname);
                                    }
                                }
                                if (!varused)
                                {
                                    EventsPart ep = obj.Parts.Get<EventsPart>();
                                    if (ep != null)
                                    {
                                        varused = VarUsedInText(ep.Source, varname);
                                    }
                                }
                                if (!varused)
                                {
                                    WebFormPart fp = obj.Parts.Get<WebFormPart>();
                                    if (fp != null)
                                    {
                                        varused = VarUsedInWebForm(fp, v.Id); ;
                                    }
                                }
                                if (!varused)
                                {
                                    if (varsNotUsed != "")
                                    {
                                        varsNotUsed += "&varid" + v.Id + "=";
                                    }
                                    varsNotUsed += v.Id;
                                }

                            }
                        }
                        if (varsNotUsed != "")
                        {
                            remove = Functions.CommandLink("CleanVarsNotUsed", "Clean vars not used", "ObjName", obj.Name, "varid", varsNotUsed);
                            writer.AddTableData(new string[] { obj.TypeDescriptor.Name, obj.Name, remove });
                        }
                    }
                }

                writer.AddFooter();
                writer.Close();

                KBDoctorHelper.ShowKBDoctorResults(outputFile);

                bool success = true;
                KBDoctorOutput.EndSection(title, success);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }

        }

        public static bool VarUsedInText(string reglas, string varName)
        {
            bool usedvar = false;
            if (reglas != null)
            {
                Regex myReg = new Regex("//.*", RegexOptions.None);
                Regex myReg2 = new Regex(@"/\*.*\*/", RegexOptions.Singleline);
                Regex paramReg = new Regex(varName + @"\W+", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                reglas = myReg.Replace(reglas, "");
                reglas = myReg2.Replace(reglas, "");
                System.Text.RegularExpressions.Match match = paramReg.Match(reglas);
                if (match.Success)
                {
                    usedvar = true;
                }
                return usedvar;
            }
            else
            {
                return false;
            }
        }

        public static bool VarUsedInWebForm(WebFormPart wF, int varId)
        {
            return (wF.GetVariable(varId) != null);
        }

        public static void CleanVarsNotUsed()
        {
            string title = "KBDoctor - Clean variables not used by DVelop Software.";
            IOutputService output = CommonServices.Output;
            KBDoctorOutput.StartSection(title);

            KBDoctorCore.Sources.API.CleanAllKBObjectVariables(UIServices.KB.CurrentKB, output);

            KBDoctorOutput.EndSection(title, true);

        }

        private static void SetDocumentDirty(IGxDocument doc)
        {
            if (UIServices.Environment.InvokeRequired) // devuelve true cuando el thread que est� ejecutando no es el thread de UI
                UIServices.Environment.BeginInvoke(() => SetDocumentDirty(doc)); // dispara un invoke asincr�nico a SetDocumentDirty
            else
                doc.Dirty = true;
        }

        public static bool IsCallalable(KBObject obj)
        {
            return ((obj is Transaction) || (obj is Procedure) || (obj is WebPanel) || (obj is WorkPanel) || (obj is DataProvider) || (obj is Artech.Genexus.Common.Objects.Menubar) || (obj is DataSelector));
        }

        public static bool isGeneratedbyPattern(KBObject obj)
        {
            if (!(obj == null))
            { return obj.GetPropertyValue<bool>(KBObjectProperties.IsGeneratedObject); }
            else
            { return true; }

        }

        public static void ResetWINForm()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;

            string title = "KBDoctor - Reset WIN Forms";


            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Type", "Name" });

                foreach (Transaction obj in Transaction.GetAll(kbserv.CurrentModel)) //kbserv.CurrentModel.Objects.GetAll<Transaction>)
                {
                    if (isGenerated(obj))
                    {
                        KBDoctorOutput.Message( "Procesing.." + obj.Name);

                        List<KBObjectPart> parts = new List<KBObjectPart>() { obj.Parts[typeof(WinFormPart).GUID] };
                        parts.ForEach(part =>
                        {
                            if (part.Default.CanCalculateDefault())
                            {
                                part.Default.SilentSetIsDefault(true);
                                try
                                {
                                    obj.Save();
                                }
                                catch (Exception e) { output.AddErrorLine(e.Message); }

                                writer.AddTableData(new string[] { obj.TypeDescriptor.Name, Functions.linkObject(obj) });
                            }
                        }
                                    );
                    }
                }
                writer.AddFooter();
                writer.Close();

                KBDoctorHelper.ShowKBDoctorResults(outputFile);

                bool success = true;
                KBDoctorOutput.EndSection(title, success);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public static void BuildObjectAndReferences()
        {
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;

            bool success = true;
            string title = "KBDoctor - Build Objects with references";
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);
                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Description", "Visibility", "Is Referenced by" });


                KBObjectCollection objToBuild = new KBObjectCollection();

                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                string lista = "";

                foreach (KBObject obj in Utility.EditableObjects(UIServices.SelectObjectDialog.SelectObjects(selectObjectOption)))
                {

                    if (KBObjectHelper.IsSpecifiable(obj))
                    {

                        if (!objToBuild.Contains(obj))
                        {
                            objToBuild.Add(obj);
                            writer.AddTableData(new string[] { obj.QualifiedName.ToString(), obj.Description, obj.IsPublic ? "Public" : "", "" });
                        }
                    }
                    ModulesHelper.AddObjectsReferenceTo(obj, objToBuild, writer);

                }
                foreach (KBObject obj2 in objToBuild)
                { lista += obj2.Name + ";"; };


                writer.AddTableData(new string[] { lista });
                writer.AddFooter();
                writer.Close();
                KBDoctorHelper.ShowKBDoctorResults(outputFile);

                GenexusUIServices.Build.BuildWithTheseOnly(objToBuild.Keys);

                do
                {
                    Application.DoEvents();
                } while (GenexusUIServices.Build.IsBuilding);

                KBDoctorOutput.Message( lista);
                KBDoctorOutput.EndSection(title, true);
            }
            catch
            {
                success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }

        }


        public static void BuildObjectWithProperty()
        {
            KBDoctorWebForms.ShowActionMenu(
                "KBDoctor - Responsive/Smooth actions",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("responsiveDefaults", "Convert default responsive properties"),
                    new KeyValuePair<string, string>("smoothDefaults", "Convert default Web UX to Smooth"),
                    new KeyValuePair<string, string>("generatedPreviousToSmooth", "Move generated objects from previous compatible to Smooth"),
                    new KeyValuePair<string, string>("auditSmoothResponsive", "Audit Smooth/Responsive objects"),
                    new KeyValuePair<string, string>("resetDefaultMasterPage", "Reset default MasterPage references")
                });
        }

        public static void RunResponsiveSmoothAction(object[] parameters)
        {
            ResponsiveSmoothActions.Run(KBDoctorWebForms.GetParameter(parameters, "action"));
        }

        public static void ListWebObjectsProperties()
        {


            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;

            bool success = true;
            string title = "KBDoctor - Objects with property";
            KBDoctorOutput.StartSection(title);

            string propertyString = Properties.WBP.WebUserExperience ;
            string propertyValue = Properties.WBP.WebUserExperience_Values.Smooth;
            try
            {


                KBDoctorOutput.Message("Type, Name, Web User Exp, MasterPage, Theme, Web Form Defaults, AutoRefreh, IsGenerated ");

                foreach (KBObject obj in Utility.EditableObjects(kbModel.Objects.GetAll()))
                    if (obj is WebPanel)

                    {
                        KBDoctorOutput.Message(obj.Name + "," + obj.GetPropertyValueString("WebUX") + "," + obj.GetPropertyValueString("MasterPage") + "," + obj.GetPropertyValueString("Theme")
                      + "," + obj.GetPropertyValueString("WebFormDefaults") + "," + obj.GetPropertyValueString("AUTO_REFRESH") + "," + isGenerated(obj).ToString() );
                    }
            }
            catch { }
            finally { };

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

public static void ListAPIObjects()
        {
            IKBService kbserv = UIServices.KB;

            Dictionary<string, KBObjectCollection> dic = new Dictionary<string, KBObjectCollection>();

            string title = "KBDoctor - List API Objects ";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                IOutputService output = CommonServices.Output;
                KBDoctorOutput.StartSection(title);

                string  sw2 = "";
                SortedDictionary<string, string> sw3 = new SortedDictionary<string, string>();
                int numObj = 0;


                foreach (KBObject obj in Utility.EditableObjects(kbserv.CurrentModel.Objects.GetAll()))
                {
                    if (obj != null && ObjectsHelper.isGenerated(obj))


                    {
                        bool tieneInterfaz = false;
                        if (obj is DataProvider && obj.GetPropertyValue<bool>("idISWEBSERVICE"))
                            tieneInterfaz = true;
                        if (obj is Transaction || obj is WebPanel)
                            tieneInterfaz = true;
                        if (obj.TypeDescriptor.Name == "MasterPage")
                            tieneInterfaz = false;

                        string qualifiedName = obj.QualifiedName.ToString();
                        bool isMain = obj.GetPropertyValue<bool>("IsMain");
                        if (obj is Procedure && isMain)
                        {
                            qualifiedName = obj.QualifiedName.ModuleName + (obj.QualifiedName.ModuleName == "" ? "a" : ".a") + obj.QualifiedName.ObjectName;
                            tieneInterfaz = true;
                        }
                        if (obj is WorkPanel && isMain)
                            tieneInterfaz = true;

                        if (obj is ExternalObject || obj is Artech.Genexus.Common.Objects.SDT)
                            tieneInterfaz = true;


                        string callprotocol = obj.GetPropertyValueString("CALL_PROTOCOL");
                        if (callprotocol == "")
                            callprotocol = "Internal";

                        if (tieneInterfaz)
                        {

                            sw3[callprotocol + "\t" + obj.Name] = qualifiedName;
                        }
                        numObj += 1;
                        if ((numObj % 100) == 0)
                            KBDoctorOutput.Message( obj.TypeDescriptor.Name + "," + obj.Name + "," + obj.Description); //+ "," + obj.Timestamp.ToString());
                    }
                }

                bool success = true;


                string directoryArg = KBDoctorHelper.SpcDirectory(kbserv);
                string fechahora = String.Format("{0:yyyy-MM-dd-HHmm}", DateTime.Now);




                foreach (KeyValuePair<string, string> entry in sw3)
                {
                    sw2 += entry.Value + "\r\n";
                }


                string fileName2 = directoryArg + @"\API3-" + fechahora + ".txt";
                System.IO.File.WriteAllText(fileName2, sw2);
                KBDoctorOutput.Message( "URL/URI file generated in " + fileName2);
                KBDoctorOutput.EndSection(title, success);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public static void ObjectsUpdatingAttributes()
        {
            SelectObjectsUpdatingAttributes();
        }

        public static void SelectObjectsUpdatingAttributes()
        {
            KBDoctorWebForms.ShowTableAttributeSelection("KBDoctor - Objects updating attribute", "ApplyObjectsUpdateAttribute", GetTableAttributes(UIServices.KB.CurrentModel));
        }

        public static void ApplyObjectsUpdatingAttributes(object[] parameters)
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = false;
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = kbserv.CurrentModel;

            string title = "KBDoctor - Objects updating attribute";
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);

                writer.AddTableHeader(new string[] { "Table", "Transactions", "Objects updating table", "Objects updating attribute" });

                string tblName = KBDoctorWebForms.GetParameter(parameters, "tblName");
                string attName = KBDoctorWebForms.GetParameter(parameters, "attName");
                if (!string.IsNullOrEmpty(tblName) && !string.IsNullOrEmpty(attName))
                {
                    string trnstring = "";
                    string updatetablestring = "";
                    string updateattstring = "";
                    if (tblName != "" && attName != "")
                    {
                        Table t = Table.Get(kbModel, tblName);
                        Artech.Genexus.Common.Objects.Attribute att = Artech.Genexus.Common.Objects.Attribute.Get(kbModel, attName);
                        if (att != null && t != null)
                        {
                            List<KBObject> updaters = API.ObjectsUpdatingTable(t, output);

                            List<KBObject> updatersAtt = API.ObjectsUpdateAttribute(updaters, att, output);

                            foreach (KBObject obj in updaters)
                            {
                                if (obj is Transaction)
                                {

                                    trnstring += Functions.linkObject(obj) + " ";
                                }
                                else
                                {
                                    updatetablestring += Functions.linkObject(obj) + " ";
                                }
                                KBDoctorOutput.Message( obj.Name);
                            }

                            foreach (KBObject obj in updatersAtt)
                            {
                                updateattstring += Functions.linkObject(obj) + " ";
                            }

                            writer.AddTableData(new string[] { Functions.linkObject(t), trnstring, updatetablestring, updateattstring });
                            writer.AddFooter();
                            writer.Close();

                            KBDoctorHelper.ShowKBDoctorResults(outputFile);


                        }

                    }
                    else
                    {
                        bool success = false;
                        KBDoctorOutput.EndSection(title, success);
                        writer.AddFooter();
                        writer.Close();
                    }

                }
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        private static Dictionary<string, IList<string>> GetTableAttributes(KBModel model)
        {
            Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
            foreach (Table table in Table.GetAll(model))
            {
                List<string> attributes = new List<string>();
                foreach (TableAttribute attribute in table.TableStructure.Attributes)
                {
                    attributes.Add(attribute.Name);
                }

                result[table.Name] = attributes;
            }

            return result;
        }
        // TODO: Sin referencias textuales encontradas; revisar si debe exponerse como comando antes de eliminar.
        public static void ObjectsWithTheSameSignature()
        {
            // Object with parm() rule without in: out: or inout:
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;

            string title = "KBDoctor - Objects with the same signature";
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);

                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());
                IEnumerable<KBObject> objects = Utility.EditableObjects(kbserv.CurrentModel.Objects.GetAll());

                HashSet<int> classes;
                Hashtable[] Classes_types;

                API.GetClassesTypesWithTheSameSignature(objects, out classes, out Classes_types);

                writer.AddTableHeader(new string[] { "Class", "Object", "Datatype params" });

                //Despliego en pantalla los objetos para los cuales existe otro objeto con la misma firma, de forma ordenada.
                foreach (int i in classes)
                {
                    Hashtable table_type = Classes_types[i - 1];
                    foreach (string parameters in table_type.Keys)
                    {
                        List<KBObject> objs = table_type[parameters] as List<KBObject>;
                        if (objs.Count > 1)
                        {
                            foreach (KBObject obj in objs)
                            {
                                writer.AddTableData(new string[] { parameters, obj.Name, Functions.ExtractRuleParm(obj) });
                            }
                        }
                    }
                }

                writer.AddFooter();
                writer.Close();
                bool success = true;
                KBDoctorOutput.EndSection(title, success);

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        // TODO: Sin referencias textuales encontradas; revisar si debe exponerse como comando antes de eliminar.
        public static void ObjectsWithTheSameSignatureAssociated()
        {
            // Object with parm() rule without in: out: or inout:
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;

            string title = "KBDoctor - Objects with the same signature associated to a transaction";
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);

                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());
                List<KBObject> objects = new List<KBObject>();
                HashSet<EntityKey> guids = new HashSet<EntityKey>();
                foreach (Transaction transaction in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
                {
                    if (!Utility.IsUserEditableObject(transaction))
                    {
                        continue;
                    }

                    foreach (EntityReference refer in transaction.GetReferences())
                    {
                        KBObject refto = KBObject.Get(kbserv.CurrentModel, refer.To);
                        ICallableObject callableObject = refto as ICallableObject;
                        if (!guids.Contains(refer.To))
                        {
                            if (callableObject != null)
                            {
                                objects.Add(refto);
                                guids.Add(refer.To);
                            }
                        }
                    }
                }

                HashSet<int> classes;
                Hashtable[] Classes_types;

                API.GetClassesTypesWithTheSameSignature(objects, out classes, out Classes_types);

                writer.AddTableHeader(new string[] { "Class", "Object", "Datatype params" });

                //Despliego en pantalla los objetos para los cuales existe otro objeto con la misma firma, de forma ordenada.
                foreach (int i in classes)
                {
                    Hashtable table_type = Classes_types[i - 1];
                    foreach (string parameters in table_type.Keys)
                    {
                        List<KBObject> objs = table_type[parameters] as List<KBObject>;
                        if (objs.Count > 1)
                        {
                            foreach (KBObject obj in objs)
                            {
                                writer.AddTableData(new string[] { parameters, obj.Name, Functions.ExtractRuleParm(obj) });
                            }
                        }
                    }
                }

                writer.AddFooter();
                writer.Close();
                bool success = true;
                KBDoctorOutput.EndSection(title, success);

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public static bool ThemeClassesNotUsed()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<ThemeClass>());
            foreach (ThemeClass themeclass in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                if (!Utility.IsUserEditableObject(themeclass))
                {
                    continue;
                }

                KBDoctorCore.Sources.API.ThemeClassesNotUsed(kbserv.CurrentKB, output, themeclass);
            }
            output.AddErrorLine("KBDoctor", "No theme was selected");


            return true;
        }

        public static void AssignTypesComparer()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());
            List<KBObject> objs = Utility.EditableObjects(UIServices.SelectObjectDialog.SelectObjects(selectObjectOption)).ToList();
            KBDoctorOutput.StartSection("KBDoctor - Assign Types Comparer");
            string recommendations = "";
            int cant;
            Thread thread = new Thread(() => API.AssignTypesComprarer(kbserv.CurrentKB, objs, ref recommendations, out cant));
            thread.Start();
        }

        // TODO: Sin referencias textuales encontradas; parece herramienta de prueba/manual. Revisar antes de eliminar.
        public static void TestParser()
        {

            Hashtable token_meaning = new Hashtable();

            token_meaning.Add(TokensIds.FNONE, "FNONE");
            token_meaning.Add(TokensIds.FOB, " '(' Open Bracket");
            token_meaning.Add(TokensIds.FFN, " 'Function(' Fuction call");
            token_meaning.Add(TokensIds.FNA, " Name Attribute");
            token_meaning.Add(TokensIds.FNC, " Name Cconstant");
            token_meaning.Add(TokensIds.FCB, " ')' Close Bracket");
            token_meaning.Add(TokensIds.FPL, " '+''-' Plus Minus oper.");
            token_meaning.Add(TokensIds.FPR, " '*''/' Product Divis. oper.");
            token_meaning.Add(TokensIds.FCM, " ',' Comma separate parms.");
            token_meaning.Add(TokensIds.FNT, " 'NOT' NOT");
            token_meaning.Add(TokensIds.FAN, " 'AND' 'OR' AND OR");
            token_meaning.Add(TokensIds.FRE, " '<' '>' '=' Relational oper.");
            token_meaning.Add(TokensIds.EXP, " Expression");
            token_meaning.Add(TokensIds.SUM, " Sum");
            token_meaning.Add(TokensIds.COU, " Count");
            token_meaning.Add(TokensIds.AVE, " Average");
            token_meaning.Add(TokensIds.MAX, " Maximum");
            token_meaning.Add(TokensIds.MIN, " Minimum");
            token_meaning.Add(TokensIds.FIF, " IF ...");
            token_meaning.Add(TokensIds.FSC, " Semicolon ';'");
            token_meaning.Add(TokensIds.FOT, " Otherwise");
            token_meaning.Add(TokensIds.ERR_TOKEN, "ERR_TOKEN");
            token_meaning.Add(TokensIds.FEN, " EOExpression");
            token_meaning.Add(TokensIds.FCO, " Comment (') for rules / commands");
            token_meaning.Add(TokensIds.FUV, " User Variable (&) for rules/commands");
            token_meaning.Add(TokensIds.FUA, " User Variable Array '&xx('");
            token_meaning.Add(TokensIds.FCN, " Continuation Line / White spaces");
            token_meaning.Add(TokensIds.FAM, " String to replace '&' with '&&'");
            token_meaning.Add(TokensIds.FCL, " CLass id (used for calls)");
            token_meaning.Add(TokensIds.FOI, " Object Id (used for calls)");
            token_meaning.Add(TokensIds.FCT, " ConTrol ID/Name (for properties)");
            token_meaning.Add(TokensIds.FCI, " Control type Id (combo/edit/etc.)");
            token_meaning.Add(TokensIds.FMT, " control id/name (for MeThods) (Used only in specifier)");
            token_meaning.Add(TokensIds.FBI, " BInary info in value (used to save bin data in obj_info)");
            token_meaning.Add(TokensIds.FDC, " Date constante (used only in dYNQ by now)");
            token_meaning.Add(TokensIds.FCV, " Control Variable (the var associated with the control (Used only in specifier)");
            token_meaning.Add(TokensIds.FWH, " WHEN (GXW) / WHERE (DKL) ...");
            token_meaning.Add(TokensIds.FNS, " Name space ...");
            token_meaning.Add(TokensIds.FON, " ON ...");
            token_meaning.Add(TokensIds.FBC, " Comentario de bloque");
            token_meaning.Add(TokensIds.FOR, " ORDER ...");
            token_meaning.Add(TokensIds.TKN_TRUE, " TRUE");
            token_meaning.Add(TokensIds.TKN_FALSE, " FALSE");
            token_meaning.Add(TokensIds.TKN_NONE, " NONE, para expresi�n FOR EACH ... ORDER NONE ... ENDFOR");
            token_meaning.Add(TokensIds.PRM, " Par�metro, utilizado en DYNQ");
            token_meaning.Add(TokensIds.FND, " Name Domain");
            token_meaning.Add(TokensIds.FLV, " LEVEL token");
            token_meaning.Add(TokensIds.TKN_NEW, " NEW token");
            token_meaning.Add(TokensIds.FSDTCLS, " Structure Class");
            token_meaning.Add(TokensIds.TKN_NULL, " NULL");
            token_meaning.Add(TokensIds.TKN_IN, " IN");
            token_meaning.Add(TokensIds.SSL, " SUBSELECT : used by generators; reserved it for Gx.");
            token_meaning.Add(TokensIds.FEX, " Exception name");
            token_meaning.Add(TokensIds.TMSGID, " Message id");
            token_meaning.Add(TokensIds.TNCNT, " Token Name Constant NonTranslatable");
            token_meaning.Add(TokensIds.TFOR, " For token, defined to be used with Lookup Deklarit's rule");
            token_meaning.Add(TokensIds.TDEPENDENCIES, " Dependencies token, new condition for rules.");
            token_meaning.Add(TokensIds.TRULE, " Rule token");
            token_meaning.Add(TokensIds.TBY, " 'By' token");
            token_meaning.Add(TokensIds.TGIVEN, " 'Given' token");
            token_meaning.Add(TokensIds.TWHERE, " 'Where' token -GeneXus, Deklarit uses FWH");
            token_meaning.Add(TokensIds.TDEFINEDBY, " 'Defined by' token");
            token_meaning.Add(TokensIds.TSECTION, " [Web], [Win], [Web], [Text]");
            token_meaning.Add(TokensIds.TINDP, " Used for token 'in <dataselector>'");
            token_meaning.Add(TokensIds.OPENSQUAREBRACKET, "OPENSQUAREBRACKET");
            token_meaning.Add(TokensIds.CLOSESQUAREBRACKET, "CLOSESQUAREBRACKET");
            token_meaning.Add(TokensIds.OUTPUTNAME, "OUTPUTNAME");
            token_meaning.Add(TokensIds.OUTPUTDYNAMICSYM, "OUTPUTDYNAMICSYM");
            token_meaning.Add(TokensIds.INPUT, "INPUT");
            token_meaning.Add(TokensIds.OUTPUTPROPERTY, "OUTPUTPROPERTY");
            token_meaning.Add(TokensIds.OBJREFERENCE, "OBJREFERENCE");
            token_meaning.Add(TokensIds.TUSING, "TUSING");
            token_meaning.Add(TokensIds.TSIGN, " Now that rules supports comments, define the TSIGN token to specified the sign of an expression (e.g. '-1')");
            token_meaning.Add(TokensIds.TEXO, "TEXO");
            token_meaning.Add(TokensIds.DTEJE, " 'Eject'");
            token_meaning.Add(TokensIds.DTNSK, " 'NoSkip'");
            token_meaning.Add(TokensIds.DTLNN, " 'Lineno'");
            token_meaning.Add(TokensIds.DTPRC, "DTPRC");
            token_meaning.Add(TokensIds.DTCLL, " 'Call'");
            token_meaning.Add(TokensIds.DTDBA, "DTDBA");
            token_meaning.Add(TokensIds.DTCOB, "DTCOB");
            token_meaning.Add(TokensIds.DTASG, " Assignment");
            token_meaning.Add(TokensIds.DTPRI, "DTPRI");
            token_meaning.Add(TokensIds.DTIF, "IF");
            token_meaning.Add(TokensIds.DTELS, " 'Else'");
            token_meaning.Add(TokensIds.DTEIF, " 'Endif'");
            token_meaning.Add(TokensIds.DTNPR, "Defined by");
            token_meaning.Add(TokensIds.DTDEL, " 'Delete'");
            token_meaning.Add(TokensIds.DTDO, " 'Do'");
            token_meaning.Add(TokensIds.DTEDO, " 'Enddo'");
            token_meaning.Add(TokensIds.DTWHE, "Where");
            token_meaning.Add(TokensIds.DTNEW, "New");
            token_meaning.Add(TokensIds.DTRET, "Return");
            token_meaning.Add(TokensIds.DTHEA, "DTHEA");
            token_meaning.Add(TokensIds.DTBEG, "DTBEG");
            token_meaning.Add(TokensIds.DTFOR, " 'ForEach'");
            token_meaning.Add(TokensIds.DTEND, "DTEND");
            token_meaning.Add(TokensIds.DTPL, "DTPL");
            token_meaning.Add(TokensIds.DTMT, "DTMT");
            token_meaning.Add(TokensIds.DTMB, "DTMB");
            token_meaning.Add(TokensIds.DTSRC, "DTSRC");
            token_meaning.Add(TokensIds.DTENW, "End New");
            token_meaning.Add(TokensIds.DTEFO, " 'EndFor'");
            token_meaning.Add(TokensIds.DTWDU, " 'When Duplicate'");
            token_meaning.Add(TokensIds.DTWNO, " 'When None'");
            token_meaning.Add(TokensIds.DTCP, "DTCP");
            token_meaning.Add(TokensIds.DTCMM, "Commit");
            token_meaning.Add(TokensIds.DTXFE, "DTXFE");
            token_meaning.Add(TokensIds.DTXFF, "DTXFF");
            token_meaning.Add(TokensIds.DTXNW, "DTXNW");
            token_meaning.Add(TokensIds.DTXEF, "DTXEF");
            token_meaning.Add(TokensIds.DTXEN, "DTXEN");
            token_meaning.Add(TokensIds.DTDBY, "DTDBY");
            token_meaning.Add(TokensIds.DTEXF, " 'Exit' from a 'Do While'");
            token_meaning.Add(TokensIds.DTEXD, "DTEXD");
            token_meaning.Add(TokensIds.DTMSG, "Msg - Message");
            token_meaning.Add(TokensIds.DTFOO, "DTFOO");
            token_meaning.Add(TokensIds.DTPRO, " 'Sub' 'subroutine'");
            token_meaning.Add(TokensIds.DTEPR, " 'EndSub'");
            token_meaning.Add(TokensIds.DTDOP, " Do 'subroutine'");
            token_meaning.Add(TokensIds.DTEVT, "DTEVT");
            token_meaning.Add(TokensIds.DTEEV, "DTEEV");
            token_meaning.Add(TokensIds.DTREF, "DTREF");
            token_meaning.Add(TokensIds.DTFLN, "DTFLN");
            token_meaning.Add(TokensIds.DTEFL, "DTEFL");
            token_meaning.Add(TokensIds.DTCNF, "DTCNF");
            token_meaning.Add(TokensIds.DTDOC, "Do case");
            token_meaning.Add(TokensIds.DTCAS, "Case 'Condition''");
            token_meaning.Add(TokensIds.DTECA, "EndCase");
            token_meaning.Add(TokensIds.DTLOA, "DTLOA");
            token_meaning.Add(TokensIds.DTLVL, "DTLVL");
            token_meaning.Add(TokensIds.DTRBK, " Comando ROLLBACK");
            token_meaning.Add(TokensIds.DTSBM, " Comando SUBMIT");
            token_meaning.Add(TokensIds.DTGRA, "DTGRA");
            token_meaning.Add(TokensIds.DTERH, " Commando Error_Handler");
            token_meaning.Add(TokensIds.DTVB, " Comando VB");
            token_meaning.Add(TokensIds.DTFSL, "DTFSL");
            token_meaning.Add(TokensIds.DTDMY, "Reserved for spec RPC");
            token_meaning.Add(TokensIds.DTOTH, "Otherwise");
            token_meaning.Add(TokensIds.DTEFS, " Reserved for End for each selected line");
            token_meaning.Add(TokensIds.DTJAV, " Comando JAVA");
            token_meaning.Add(TokensIds.DTSQL, " Comando SQL");
            token_meaning.Add(TokensIds.DTFLS, "DTFLS");
            token_meaning.Add(TokensIds.DTFSS, "DTFSS");
            token_meaning.Add(TokensIds.DTEFF, "DTEFF");
            token_meaning.Add(TokensIds.DTLNK, " Comando LINK");
            token_meaning.Add(TokensIds.DTAPL, " Asignaci�n del tipo +=");
            token_meaning.Add(TokensIds.DTAMI, " Asignaci�n del tipo -=");
            token_meaning.Add(TokensIds.DTAMU, " Asignaci�n del tipo *=");
            token_meaning.Add(TokensIds.DTADI, " Asignaci�n del tipo /=");
            token_meaning.Add(TokensIds.DTFIN, " FOR <var> IN <array>");
            token_meaning.Add(TokensIds.DTEFI, " END' del token anterior");
            token_meaning.Add(TokensIds.DTFFT, " FOR <var>=<exp> TO <exp> STEP <exp>");
            token_meaning.Add(TokensIds.DTEFT, " END' del token anterior");
            token_meaning.Add(TokensIds.DTIN, " Comando IN de FOR var IN array");
            token_meaning.Add(TokensIds.DTTO, " Comando TO de FOR EACH var=exp TO exp");
            token_meaning.Add(TokensIds.DTSTP, " Comando STEP de FOR var=exp TO exp STEP exp");
            token_meaning.Add(TokensIds.DTCSH, " Comando CSHARP");
            token_meaning.Add(TokensIds.DTON, " Comando ON");
            token_meaning.Add(TokensIds.DTWHN, " Comando WHEN");
            token_meaning.Add(TokensIds.DTOPD, " Comando OPTION DISTINCT");
            token_meaning.Add(TokensIds.DTUSG, " Comando USING de FOR EACH ... ENDFOR");
            token_meaning.Add(TokensIds.DTPOPUP, " Comando POPUP()");
            token_meaning.Add(TokensIds.BLOCKING, " Comando BLOCKING");
            token_meaning.Add(TokensIds.OUTPUTELEMENT, "OUTPUTELEMENT");
            token_meaning.Add(TokensIds.OPENCURLYBRACKET, "OPENCURLYBRACKET");
            token_meaning.Add(TokensIds.CLOSECURLYBRACKET, "CLOSECURLYBRACKET");
            token_meaning.Add(TokensIds.PRINT, "PRINT");
            token_meaning.Add(TokensIds.INSERT, "INSERT");
            token_meaning.Add(TokensIds.SUBGROUP, "SUBGROUP");
            token_meaning.Add(TokensIds.ENDSUBGROUP, "ENDSUBGROUP");
            token_meaning.Add(TokensIds.DTStub, " 'public sub'");
            token_meaning.Add(TokensIds.DTJavaScript, " 'javascript' command - not implemented yet! - reserved number");
            token_meaning.Add(TokensIds.DTEndStub, "DTEndStub");
            token_meaning.Add(TokensIds.DTCallStub, "DTCallStub");
            token_meaning.Add(TokensIds.DTRuby, " Comando 'RUBY <LINE>'");
            token_meaning.Add(TokensIds.DTREDUNDANCY, " Used to give redundancy info to the specifier");

            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;

            string title = "KBDoctor - TESTEO DE PARSER ";
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);

                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());

                var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
                ParserInfo parserInfo;

                writer.AddTableHeader(new string[] { "OBJECT", "COMMAND", "TOKEN", "Id", "Row" });


                foreach (KBObject obj in Utility.EditableObjects(UIServices.SelectObjectDialog.SelectObjects(selectObjectOption)))
                {
                    Artech.Genexus.Common.Parts.ProcedurePart source = obj.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
                    Artech.Genexus.Common.Parts.VariablesPart vp = obj.Parts.Get<VariablesPart>();

                    if (source != null)
                    {
                        parserInfo = new ParserInfo(source);


                        var info = new Artech.Architecture.Language.Parser.ParserInfo(source);
                        if (parser.Validate(info, source.Source))
                        {
                            Artech.Genexus.Common.AST.AbstractNode paramRootNode = Artech.Genexus.Common.AST.ASTNodeFactory.Create(parser.Structure, source, vp, info);
                        }

                    }
                }
                writer.AddFooter();
                writer.Close();
                bool success = true;
                KBDoctorOutput.EndSection(title, success);
                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }
        // TODO: Sin referencias textuales encontradas; revisar si se usa manualmente antes de eliminar.
        public static void CopyWinFormToWebForm(KBObject obj)
        {
            IOutputService output = CommonServices.Output;
            WinFormPart winForm = obj.Parts.Get<WinFormPart>();
            WebFormPart webForm = obj.Parts.Get<WebFormPart>();

            if (winForm == null || webForm == null)
            {
                output.AddErrorLine("WinForm or WebForm part not found in the object.");
                return;
            }

            foreach (var element in winForm.MyDocument.Definitions.SelectMany(def => def.Value.Canvas.Elements))
            {
                AddElementToWebForm(element, webForm);
            }

            try
            {
                obj.Save();
                output.AddLine("Elements copied from WinForm to WebForm successfully.");
            }
            catch (Exception e)
            {
                output.AddErrorLine("Error saving object: " + e.Message);
            }
        }

        private static void AddElementToWebForm(FormElement element, WebFormPart webForm)
        {

        }
        // TODO: Sin referencias textuales encontradas; revisar si se usa manualmente antes de eliminar.
        public static void AddObjectToSDPanel(SDPanel sDPanel, KBObject objToAdd)
        {
            IOutputService output = CommonServices.Output;
            try
            {
                sDPanel.Save();
                output.AddLine($"Object {objToAdd.Name} added to SDPanel {sDPanel.Name} successfully.");
            }
            catch (Exception e)
            {
                output.AddErrorLine($"Error adding object to SDPanel: {e.Message}");
            }
        }
        public static bool ObjThemeClassesNotUsed()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
            foreach (WebPanel webPanel in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                if (!Utility.IsUserEditableObject(webPanel))
                {
                    continue;
                }

                KBDoctorCore.Sources.API.ObjThemeClassesNotUsed(kbserv.CurrentKB, output, webPanel);
            }
            output.AddErrorLine("KBDoctor", "No theme was selected");


            return true;
        }


    }
}

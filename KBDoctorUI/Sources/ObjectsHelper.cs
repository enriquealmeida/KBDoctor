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

        public static void Unreachables()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;
            string title = "KBDoctor - Unreachable Objects";
            output.StartSection("KBDoctor", title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Type", "Description", "Remove" });
                KBObjectCollection reachablesObjects = new KBObjectCollection();
                KBObjectCollection unreachablesObjects = new KBObjectCollection();
                KBCategory mainCategory = KBCategory.Get(kbserv.CurrentModel, "Main Programs");
                foreach (KBObject obj in mainCategory.AllMembers)
                {
                    MarkReachables(output, obj, reachablesObjects);
                }


                foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
                {
                    ICallableObject callableObject = obj as ICallableObject;
                    if ((callableObject != null) | (obj is Artech.Genexus.Common.Objects.Attribute)
                        | obj is Artech.Genexus.Common.Objects.Table | obj is Domain | obj is ExternalObject | obj is Artech.Genexus.Common.Objects.SDT) //Saco Image
                    {
                        unreachablesObjects.Add(obj);
                    }
                }
                int cantObj = unreachablesObjects.Count;
                //saco los objetos alcanzables. 
                unreachablesObjects.RemoveAll(reachablesObjects);
                int cantUnObj = unreachablesObjects.Count;
                KBDoctorOutput.Message( "(Re)creating KBDoctor.Unreachable category");
                KBCategory catR = KBCategory.Get(kbserv.CurrentModel, "KBDoctor.UnReachable");
                if (catR == null)
                {
                    catR = new KBCategory(kbserv.CurrentModel);
                    catR.Name = "KBDoctor.UnReachable";
                    catR.Description = "Category for unreachable objects";
                    catR.ShowInModelTree = true;
                    BLServices.TeamDevClient.IgnoreForCommit(catR.Model, catR.Key);
                    catR.Save();
                }

                foreach (KBObject obj in catR.AllMembers)
                {
                    KBDoctorOutput.Message( "Removing " + obj.Name + " from  KBDoctor.Unreachable category");
                    obj.RemoveCategory(catR);
                    if (!obj.GetPropertyValue<bool>(Properties.TRN.GenerateObject) && (obj is Procedure | obj is WebPanel | obj is WorkPanel | obj is Transaction | obj is DataProvider ))
                    {
                        obj.SetPropertyValue(Properties.TRN.GenerateObject, true);
                    }
                    obj.Save();

                }


                Boolean SaveObj = false;
                foreach (KBObject obj in unreachablesObjects)
                {
                    Artech.Architecture.UI.Framework.Objects.IGxDocument document;

                    SaveObj = false;


                    if (UIServices.DocumentManager.IsOpenDocument(obj, out document))
                    {
                        document.Object.AddCategory(catR);
                        ObjectsHelper.SetDocumentDirty(document);
                        UIServices.TrackSelection.OnSelectChange(document.Object, null);
                    }
                    else
                    {
                        if (!catR.ContainsMember(obj))
                        {
                            obj.AddCategory(catR);
                            SaveObj = true;
                        }


                        if (obj.GetPropertyValue<bool>(Properties.TRN.GenerateObject) && (obj is Procedure | obj is WebPanel | obj is WorkPanel | obj is Transaction | obj is DataProvider))
                        {
                            obj.SetPropertyValue(Properties.TRN.GenerateObject, false);
                            SaveObj = true;
                        }

                        string objNameLink = Functions.linkObject(obj);
                        string remove = Functions.CommandLink("RemoveObject", "Remove", "guid", obj.Guid.ToString());
                        writer.AddTableData(new string[] { objNameLink, obj.TypeDescriptor.Name, obj.Description, remove });

                        if (SaveObj)
                        {
                            try
                            {
                                KBDoctorOutput.Message( obj.TypeDescriptor.Name + "-" + obj.Name + " is unreachable (SAVING) ");
                                obj.Save();
                            }
                            catch
                            {
                                output.AddWarningLine("Error saving " + obj.TypeDescriptor.Name + "-" + obj.Name);
                                success = false;
                            }
                        }
                        else
                        {
                            KBDoctorOutput.Message( obj.TypeDescriptor.Name + "-" + obj.Name + " is unreachable ");
                        }
                    }


                }

                KBDoctorOutput.Message( "");
                KBDoctorOutput.Message( "Total Objects:" + cantObj.ToString() + ". Unreachable Objects: " + cantUnObj.ToString());
                output.EndSection("KBDoctor", title, success);
                writer.AddFooter();
                writer.Close();
                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            catch
            {
                success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        private static void MarkReachables(IOutputService output, KBObject obj, KBObjectCollection reachablesObjects)
        {
            IKBService kbserv = UIServices.KB;
            reachablesObjects.Add(obj);

            foreach (EntityReference reference in obj.GetReferences()) // LinkType.UsedObject))
            {
                KBObject objRef = KBObject.Get(obj.Model, reference.To);
              

                if ((objRef != null) && !reachablesObjects.Contains(objRef))
                {
                        KBDoctorOutput.Message("Referencia:" + obj.Name + " LinkType: " + reference.LinkType.ToString() + " LinkTypeInfo: " + reference.LinkTypeInfo.ToString() + " ReferenceType: " + reference.ReferenceType.ToString() + " Objref: " + objRef.Name);


                    MarkReachables(output, objRef, reachablesObjects);
                }
            }
        }



        public static void ObjectsMainsCalled()
        {

            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Object main called by others";

            output.StartSection("KBDoctor", title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Type", "Object", "Description", "Parm", "#Callers", "Call Protocol", "Generated by Pattern", " Encrypt Parameters " });

                KBCategory mainCategory = KBCategory.Get(kbserv.CurrentModel, "Main Programs");
                foreach (KBObject obj in mainCategory.AllMembers)
                {
                    int callers = 0;
                    foreach (EntityReference reference in obj.GetReferencesTo(LinkType.UsedObject))
                    {
                        callers = callers + 1;
                    }
                    string ruleParm = Functions.ExtractRuleParm(obj);
                    string enc = obj.GetPropertyValueString("USE_ENCRYPTION");
                    writer.AddTableData(new string[] { obj.TypeDescriptor.Name, Functions.linkObject(obj), obj.Description, ruleParm, callers.ToString(), obj.GetPropertyValueString("CALL_PROTOCOL"), isGeneratedbyPattern(obj).ToString(), enc });
                }


                writer.AddFooter();
                writer.Close();

                bool success = true;
                output.EndSection("KBDoctor", title, success);

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public static void ParmWOInOut()
        {

            // Object with parm() rule without in: out: or inout
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            List<KBObject> objectsWithProblems = API.ObjectsWithoutINOUT(UIServices.KB.CurrentKB, output);
            
            string title = "KBDoctor - Object with parameters without IN:/OUT:/INOUT:";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Folder", "Object", "Description", "Param rule", "Timestamp", "Mains" });

                foreach (KBObject obj in objectsWithProblems)
                {
                    string ruleParm = Functions.ExtractRuleParm(obj);
                    string objNameLink = Functions.linkObject(obj);

                    KBObjectCollection objColl = new KBObjectCollection();

                    string mainss = "";

                    writer.AddTableData(new string[] { obj.Parent.Name, objNameLink, obj.Description, ruleParm, obj.Timestamp.ToString(), mainss });
                }

                writer.AddTableData(new string[] { "#Objects with problems ", objectsWithProblems.Count.ToString(), "", "" });

                writer.AddFooter();
                writer.Close();
                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }



        public static bool isGenerated(KBObject obj)
        {
            if (obj is DataSelector)  //Los Dataselector no tienen la propiedad de generarlos o no , por lo que siempre devuelven falso y sin son referenciados se generan. 
                return true;
            if (obj is Artech.Architecture.Common.Objects.Module)
                return false;
            object aux = obj.GetPropertyValue(Properties.TRN.GenerateObject);
            return ((aux != null) && (aux.ToString() == "True"));

        }

        public static void ObjectsWithParmAndCommitOnExit()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool hasParameters;
            bool commitOnExit;
            string title = "KBDoctor - Object with parameters and Commit on Exit = Yes";
            string objNameLink;

            output.StartSection("KBDoctor", title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Type", "Name", "Description", "isGenerated?" });

                foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
                {
                    ICallableObject callableObject = obj as ICallableObject;
                    if (callableObject != null)
                    {
                        KBDoctorOutput.Message( "Processing " + obj.TypeDescriptor.Name + " " + obj.Name);

                        hasParameters = false;
                        foreach (Signature signature in callableObject.GetSignatures())
                        {
                            foreach (Parameter parm in signature.Parameters)
                            {
                                hasParameters = true;
                            }
                        }

                        if (hasParameters)
                        {
                            object aux = obj.GetPropertyValue("CommitOnExit");
                            commitOnExit = ((aux != null) && (aux.ToString() == "Yes"));

                            if (commitOnExit)
                            {
                                string isGeneratedstr = (isGenerated(obj) ? "Yes" : string.Empty);
                                objNameLink = Functions.linkObject(obj);
                                writer.AddTableData(new string[] { obj.TypeDescriptor.Name, objNameLink, obj.Description, isGeneratedstr });
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

        public static void ObjetNotCalled()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Not referenced objects";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                output.StartSection("KBDoctor", title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Type", "Object", "Remove", "is generated?", "isMain?" });
                int callers;
                string remove = "";
                bool continuar = true;

                do
                {
                    continuar = false;
                    foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
                    {
                        ICallableObject callableObject = obj as ICallableObject;
                        if ((callableObject != null) | (obj is Artech.Genexus.Common.Objects.Attribute)
                            | obj is Table | obj is Domain | obj is ExternalObject | obj is Image | obj is Artech.Genexus.Common.Objects.SDT)
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
                                    remove = Functions.CommandLink("RemoveObject", "Remove", "guid", obj.Guid.ToString());
                                }
                                string objNameLink = Functions.linkObject(obj);
                                string isMainstr = (Utility.IsMain(obj) ? "Main" : string.Empty);
                                string isGeneratedstr = (isGenerated(obj) ? "Yes" : string.Empty);
                                if (!Utility.IsMain(obj))
                                {

                                    if (remove != "")
                                    {
                                        try
                                        {
                                            obj.Delete();
                                            KBDoctorOutput.Message( "REMOVING..." + obj.Name);
                                            remove = "REMOVED!";
                                            objNameLink = obj.Name;
                                            continuar = true;
                                        }
                                        catch (Exception e) { Console.WriteLine(e.Message); };

                                    }
                                    writer.AddTableData(new string[] { obj.TypeDescriptor.Name, objNameLink, remove, isGeneratedstr, isMainstr });
                                }
                                if ((obj is Transaction) && (obj.GetPropertyValue<bool>(Properties.TRN.GenerateObject)))
                                {
                                    try
                                    {
                                        obj.SetPropertyValue(Properties.TRN.GenerateObject, false);
                                        KBDoctorCore.Sources.API.CleanKBObject(obj, output);
                                    }
                                    catch (Exception e) { Console.WriteLine(e.Message); };

                                }
                            }
                        }
                    }
                } while (continuar);

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

        public static void RemovableTransactions()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;

            string title = "KBDoctor - Removable Transactions";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                output.StartSection("KBDoctor", title);


                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Table", "Object", "Remove", "is generated?", "isMain?" });

                string remove = "";

                foreach (Transaction trn in Transaction.GetAll(kbserv.CurrentModel))
                {

                    if (!isGenerated(trn))
                    {
                        KBDoctorOutput.Message( "Procesing... " + trn.Name);
                        bool isRemovable, isRemovableWithWarning;
                        string lstTrns;
                        KBObjectCollection attExclusive;

                        CheckIfRemovable(output, writer, trn, out isRemovable, out isRemovableWithWarning, out lstTrns, out attExclusive);

                        if (isRemovable)
                        {
                            KBDoctorOutput.Message( "Procesing... " + trn.Name + " REMOVABLE ");
                            remove = Functions.CommandLink("RemoveObject", "Remove", "guid", trn.Guid.ToString());
                            writer.AddTableData(new string[] { "", Functions.linkObject(trn), remove, trn.Description, "" });
                        }
                        else
                            if (isRemovableWithWarning)

                        {
                            KBDoctorOutput.Message( "Procesing... " + trn.Name + " REMOVABLE with warning ");
                            remove = Functions.CommandLink("RemoveObject", "Remove", "guid", trn.Guid.ToString());
                            writer.AddTableData(new string[] { "", Functions.linkObject(trn), remove, trn.Description, lstTrns + " WHIT WARNING" });
                        }
                        else
                        {
                            if (attExclusive.Count > 0)
                            {
                                string lstAtt = "";
                                foreach (Artech.Genexus.Common.Objects.Attribute a in attExclusive)
                                {
                                    lstAtt = Functions.linkObject(a) + " ";
                                }

                                writer.AddTableData(new string[] { "", Functions.linkObject(trn), "Exclusive Attributes:" + lstAtt, trn.Description, "NOT REMOVABLE" });
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

        private static void CheckIfRemovable(IOutputService output, KBDoctorXMLWriter writer, Transaction trn, out bool isRemovable, out bool isRemovableWithWarning, out string lstTrns, out KBObjectCollection attExclusive)
        {
            isRemovable = true;
            isRemovableWithWarning = true;
            lstTrns = "";
            attExclusive = new KBObjectCollection();

            foreach (Artech.Genexus.Common.Parts.TransactionLevel LVL in trn.Structure.GetLevels())
            {
                bool isLevelRemovable = true;

                Table TBL = LVL.AssociatedTable;
                string tblName = TBL.Name;

                KBObjectCollection attLvl = new KBObjectCollection();
                attLvl = AttributesFromGeneratedTransactions(TBL);

                KBObjectCollection attLvlAll = new KBObjectCollection();

                attLvlAll = AttributesFromAllTransactionsExceptOne(TBL, trn, out lstTrns);

                foreach (Artech.Genexus.Common.Objects.Attribute a in LVL.Structure.GetAttributes())
                {


                    if (!attLvl.Contains(a))
                    {
                        KBDoctorOutput.Message( "Transaction " + trn.Name + " Table " + tblName + " LVL " + LVL.Name + " Attribute "
                            + a.Name + " not in any generated transaction");
                        isRemovable = false;
                        isLevelRemovable = false;
                    }

                    if (!attLvlAll.Contains(a))
                    {
                        KBDoctorOutput.Message( "Transaction " + trn.Name + " Table " + tblName + " LVL " + LVL.Name + " Attribute "
                            + a.Name + " not in any other transaction");
                        isRemovableWithWarning = false;
                        isLevelRemovable = false;
                        attExclusive.Add(a);
                    }




                }
                if (isLevelRemovable)
                    writer.AddTableData(new string[] { tblName, Functions.linkObject(trn), LVL.Name, trn.Description, "Level Removable" });

            }
        }

        internal static void ChangeCommitOnExit()
        {
            // Object with parm() rule without in: out: or inout:
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Change Commit on Exit ";

            output.StartSection("KBDoctor", title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);


                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Type", "Description", "Commit on Exit", "Update DB?", "Commit in Source", "Timestamp", "Last Update" });

                string commitOnExit = "";
                string commitInSource = "";
                string UpdateInsertDelete = "";
                foreach (KBObject objRef in kbserv.CurrentModel.Objects.GetAll())
                {
                    if (objRef is Procedure)
                    {
                        object aux = objRef.GetPropertyValue("CommitOnExit");
                        if (aux != null)
                        {
                            commitOnExit = aux.ToString() == "Yes" ? "YES" : " ";

                        }
                        UpdateInsertDelete = CleanKBHelper.ObjectUpdateDB(objRef) ? "YES" : "";

                        Procedure prc = (Procedure)objRef;
                        try
                        {
                            if (Functions.ExtractComments(prc.ProcedurePart.Source.ToString().ToUpper()).Contains("COMMIT"))
                                commitInSource = "YES ";
                            else
                                commitInSource = "";
                        }
                        catch (Exception e)
                        { output.AddErrorLine(e.Message); }
                        finally
                        { commitInSource = ""; };



                        if (UpdateInsertDelete == "" & commitOnExit == "YES")
                        {
                            objRef.SetPropertyValue("CommitOnExit", "No");
                            objRef.Save();
                            writer.AddTableData(new string[] { Functions.linkObject(objRef), objRef.TypeDescriptor.Name, objRef.Description, commitOnExit, UpdateInsertDelete, commitInSource, objRef.Timestamp.ToString(), objRef.LastUpdate.ToString() });
                        }
                    }

                }


                writer.AddFooter();
                writer.Close();

                bool success = true;
                output.EndSection("KBDoctor", title, success);

                KBDoctorHelper.ShowKBDoctorResults(outputFile);

            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }

        }

        private static KBObjectCollection AttributesFromGeneratedTransactions(Table tBL)
        {
            KBObjectCollection attlist = new KBObjectCollection();
            foreach (Transaction trn in tBL.AssociatedTransactions)
            {
                if (isGenerated(trn))
                {
                    foreach (Artech.Genexus.Common.Parts.TransactionLevel LVL in trn.Structure.GetLevels())
                    {
                        if (LVL.AssociatedTable == tBL)
                        {
                            foreach (Artech.Genexus.Common.Objects.Attribute a in LVL.Structure.GetAttributes())
                            {
                                attlist.Add(a);
                            }
                        }
                    }
                }

            }
            return attlist;
        }

        private static KBObjectCollection AttributesFromAllTransactionsExceptOne(Table tBL, Transaction trnToExclude, out string lstTrn)
        {
            KBObjectCollection attlist = new KBObjectCollection();
            lstTrn = "";
            foreach (Transaction trn in tBL.AssociatedTransactions)
            {
                if (trn != trnToExclude)
                {
                    lstTrn += trn.QualifiedName + " ";
                    foreach (Artech.Genexus.Common.Parts.TransactionLevel LVL in trn.Structure.GetLevels())
                    {
                        if (LVL.AssociatedTable == tBL)
                        {
                            foreach (Artech.Genexus.Common.Objects.Attribute a in LVL.Structure.GetAttributes())
                            {
                                attlist.Add(a);
                            }
                        }
                    }
                }

            }
            return attlist;
        }

        public static void MainTableUsed()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;

            string title = "KBDoctor - Tables used by mains";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                output.StartSection("KBDoctor", title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Main", "Table", "Operation" });


                KBCategory mainCategory = KBCategory.Get(kbserv.CurrentModel, "Main Programs");
                foreach (KBObject obj in mainCategory.AllMembers)
                {
                    StringCollection tableOperation = new StringCollection();
                    KBObjectCollection objMarked = new KBObjectCollection();
                    KBDoctorOutput.Message( " ");
                    KBDoctorOutput.Message( " =============================> " + obj.Name);

                    string mainstr = obj.Name + " " + obj.GetPropertyValueString("AppLocation");
                    TablesUsed(output, obj, tableOperation, objMarked, mainstr, writer);


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

        private static void TablesUsed(IOutputService output, KBObject obj, StringCollection tableOperation, KBObjectCollection objMarked, string mainstr, KBDoctorXMLWriter writer)
        {
            IKBService kbserv = UIServices.KB;

            objMarked.Add(obj);

            foreach (EntityReference reference in obj.GetReferences())
            {
                KBObject objRef = KBObject.Get(obj.Model, reference.To);

                if ((objRef != null) && !objMarked.Contains(objRef))
                {
                    if (Utility.IsMain(objRef))
                        return;
                    if ((reference.ReferenceType == ReferenceType.WeakExternal) && (objRef is Table))
                    {
                        bool read, insert, update, delete, isBase;
                        if (ReferenceTypeInfo.ReadTableInfo(reference.LinkTypeInfo, out read, out insert, out update, out delete, out isBase))
                        {
                            string linea = "";
                            if (read)
                            {
                                linea = mainstr + " , " + objRef.Name + " , " + "SELECT";

                                if (!tableOperation.Contains(linea))
                                {
                                    KBDoctorOutput.Message( linea);
                                    tableOperation.Add(linea);
                                    writer.AddTableData(new string[] { mainstr, objRef.Name, "SELECT" });
                                };
                            }
                            if (insert)
                            {
                                linea = mainstr + " , " + objRef.Name + " , " + "INSERT";
                                if (!tableOperation.Contains(linea))
                                {
                                    KBDoctorOutput.Message( linea);
                                    tableOperation.Add(linea);
                                    writer.AddTableData(new string[] { mainstr, objRef.Name, "INSERT" });
                                };
                            }

                            if (update)
                            {
                                linea = mainstr + " , " + objRef.Name + " , " + "UPDATE";
                                if (!tableOperation.Contains(linea))
                                {
                                    KBDoctorOutput.Message( linea);
                                    tableOperation.Add(linea);
                                    writer.AddTableData(new string[] { mainstr, objRef.Name, "UPDATE" });
                                };
                            }

                            if (delete)
                            {
                                linea = mainstr + " , " + objRef.Name + " , " + "DELETE";
                                if (!tableOperation.Contains(linea))
                                {
                                    KBDoctorOutput.Message( linea);
                                    tableOperation.Add(linea);
                                    writer.AddTableData(new string[] { mainstr, objRef.Name, "DELETE" });
                                };
                            }

                        }

                    }
                    TablesUsed(output, objRef, tableOperation, objMarked, mainstr, writer);

                }


            }
        }

        public static void CreateDeployUnits()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            SpecificationListHelper helper = new SpecificationListHelper(kbserv.CurrentModel.Environment.TargetModel);
            string title = "KBDoctor - CreateDeployUnits";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                output.StartSection("KBDoctor", title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "AppLocation", "Main", "Object", "WIN/WEB" });

                StringCollection objReferenced = new StringCollection();

                KBCategory mainCategory = KBCategory.Get(kbserv.CurrentModel, "Main Programs");
                foreach (KBObject obj in mainCategory.AllMembers)
                {

                    string objLocation = (string)obj.GetProperty("AppLocation").Value;

                    string objAppGenerator = obj.GetPropertyValueString("AppGenerator");
                    string Dircopia, exts;
                    if (objAppGenerator.ToUpper().Contains("WEB"))
                    {
                        Dircopia = @".\Web\";
                        exts = ".dll";
                    }
                    else
                    {
                        Dircopia = @".\";
                        exts = ".exe";
                    }

                    string letra = "";


                    if (obj is Procedure)
                        letra = "a";
                    if (obj is WorkPanel)
                        letra = "u";
                    if (obj is Transaction)
                        letra = "";


                    KBObjectCollection objMarked = new KBObjectCollection();
                    KBDoctorOutput.Message( " ");
                    KBDoctorOutput.Message( "ECHO  **** " + obj.Name + " ***** ");

                    if (obj.GetPropertyValueString("AppLocation") == "")
                    {

                        string mensaje = "Insert Location for (" + obj.TypeDescriptor.Name + ") " + obj.Name + "-" + obj.Description;

                        PromptDescription pd = new PromptDescription(mensaje);
                        DialogResult dr = pd.ShowDialog();

                        if (dr == DialogResult.OK)
                        {
                            obj.SetPropertyValue("AppLocation", pd.Description);
                            obj.Save();
                        }
                    }
                    string mainstr = obj.GetPropertyValueString("AppLocation");

                    string linea = "XCOPY /y/d " + Dircopia + @"bin\" + letra + obj.Name + exts + " %" + mainstr + @"%\bin\";
                    KBDoctorOutput.Message( linea);
                    objReferenced.Add(linea);

                    if (!(obj is Procedure) && !(obj is WorkPanel) && Dircopia == @".\Web\")
                    {
                        linea = "XCOPY /y/d " + Dircopia + letra + obj.Name + ".js %" + mainstr + "%";
                        KBDoctorOutput.Message( linea);
                        objReferenced.Add(linea);
                    }
                    WriteCopyObject(output, obj, objReferenced, objMarked, mainstr, Dircopia);

                }

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

        internal static void SplitMainObject()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            output.StartSection("Split Main Object");

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            KBModel kbModel = UIServices.KB.CurrentModel;




            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());

            foreach (Procedure viejo in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                //Cambio el objeto  seleccionado
                string objName = viejo.Name;
                object callProtocol = viejo.GetPropertyValue("CALL_PROTOCOL");
                viejo.SetPropertyValue("CALL_PROTOCOL", "Internal");
                viejo.SetPropertyValue("isMain", false);
                viejo.SetPropertyValue("ObjectVisibility", ObjectVisibility.Public);

                viejo.Name = objName + "_core";
                viejo.Save();

                Artech.Genexus.Common.Objects.Procedure nuevo = new Artech.Genexus.Common.Objects.Procedure(kbModel);
                foreach (Artech.Common.Properties.Property prop in viejo.Properties)
                {
                    if (!prop.IsDefault)
                        nuevo.SetPropertyValue(prop.Name, prop.Value);
                }
                nuevo.SetPropertyValue("Name", objName);
                nuevo.SetPropertyValue("isMain", true);
                nuevo.SetPropertyValue("CALL_PROTOCOL", callProtocol);
                viejo.SetPropertyValue("ObjectVisibility", ObjectVisibility.Private);

                string parm = Functions.ExtractRuleParm(viejo);
                string parm2 = "";

                if (parm != "")
                {
                    nuevo.Rules.Source = parm + ";";

                    parm2 = parm.ToLower();
                    parm2 = parm2.Replace("parm", "");
                    parm2 = parm2.Replace("in:", "");
                    parm2 = parm2.Replace("out:", "");
                    parm2 = parm2.Replace("inout:", "");
                    parm2 = parm2.Replace("(", "");
                    parm2 = parm2.Replace(")", "");
                };

                nuevo.ProcedurePart.Source = objName + "_core.call(" + parm2 + ") ";

                foreach (Variable v in viejo.Variables.Variables)
                {
                    if (!v.IsStandard)
                        nuevo.Variables.Add(v);
                }
                try
                {
                    output.AddWarningLine("Create new: " + nuevo.Name + " Protocol: " + callProtocol);
                    nuevo.Save();
                }
                catch (Exception e)
                {
                    output.AddErrorLine(e.Message + " - " + e.InnerException);
                    output.AddErrorLine("Can't save object" + objName + ". Try to save commented");
                    KBDoctorOutput.Message( "Parm = " + parm + " Parm2= " + parm2);

                    nuevo.ProcedurePart.Source = "//" + objName + "_core.call(" + parm2 + ") ";
                    nuevo.Rules.Source = "//" + parm + ";";
                    nuevo.Save();


                };
            }
            output.EndSection("Split Main Object", true);
        }

    }

}

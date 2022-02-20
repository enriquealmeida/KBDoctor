using System;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
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
using Artech.Common.Properties;
using Artech.Common.Language.Parser;

using Artech.Common.Diagnostics;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.UI.Framework.Objects;
using Artech.Architecture.Language.Services;
using Artech.Architecture.Language.Parser;
using Artech.Architecture.Language.Parser.Objects;
using Artech.Architecture.Language.Parser.Data;
using Artech.Architecture.Common;
using Artech.Architecture.Common.Services;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Helpers;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Collections;
using Artech.Architecture.BL.Framework;
using Artech.Architecture.BL.Framework.Services;
using GeneXus.Server.Contracts;
using Artech.Architecture.Language.ComponentModel;

using Artech.Udm.Framework;
using Concepto.Packages.KBDoctorCore.Sources;
using Artech.Genexus.Common.AST;
using Artech.Architecture.Common.Location;
using Artech.Genexus.Common.Types;
using System.Threading;
using Artech.Genexus.Common.Parts.WebForm;
using Artech.Common.Helpers.Structure;
using Artech.Genexus.Common.Parts.ExternalObject;
using API = Concepto.Packages.KBDoctorCore.Sources.API;
using Concepto.Packages.KBDoctor.Sources;

namespace Concepto.Packages.KBDoctor
{
    class ObjectsHelper
    {

        public static void Unreachables()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;
            string title = "KBDoctor - Unreachable Objects";
            output.StartSection("KBDoctor", title);
            // UIServices.ToolWindows.ShowToolWindow(new Guid("59CE53BC-F419-402b-AC09-AC275ED21AB9"));
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
                        | obj is Artech.Genexus.Common.Objects.Table | obj is Domain | obj is ExternalObject | obj is SDT) //Saco Image
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
                    if (!obj.GetPropertyValue<bool>(Properties.TRN.GenerateObject) && (obj is Procedure | obj is WebPanel | obj is WorkPanel | obj is Transaction | obj is DataProvider | obj is Menubar))
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
                        string remove = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;RemoveObject&guid=" + obj.Guid.ToString() + "\">Remove</a>";
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
                   // if (objRef.Name.StartsWith("Transaction"))
                   // {
                        KBDoctorOutput.Message("Referencia:" + obj.Name + " LinkType: " + reference.LinkType.ToString() + " LinkTypeInfo: " + reference.LinkTypeInfo.ToString() + " ReferenceType: " + reference.ReferenceType.ToString() + " Objref: " + objRef.Name);

                  //  }

                    MarkReachables(output, objRef, reachablesObjects);
                    //if (obj is Procedure && (objRef is Transaction || objRef is WebPanel))
                    //{
                     //   KBDoctorOutput.Warning(obj.Name + "===>" + objRef.Name);
                    //}
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
                        //break;
                    }
                    //if (callers > 0)
                    // {
                    string ruleParm = Functions.ExtractRuleParm(obj);
                    string enc = obj.GetPropertyValueString("USE_ENCRYPTION");
                    writer.AddTableData(new string[] { obj.TypeDescriptor.Name, Functions.linkObject(obj), obj.Description, ruleParm, callers.ToString(), obj.GetPropertyValueString("CALL_PROTOCOL"), isGeneratedbyPattern(obj).ToString(), enc });
                    /*
                    if (obj is Procedure)
                    {
                        try
                        {
                            KBDoctorOutput.Message(obj.Name);
                            obj.SetPropertyValue("isMain", false);
                            obj.Save();
                        }
                        catch (Exception e) { KBDoctorOutput.Message(e.Message.ToString()); };
                    }
                    */
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

                    // string  mainss = KbStats.MainsOf(obj, objColl, callTree);
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
                            remove = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;RemoveObject&guid=" + trn.Guid.ToString() + "\">Remove</a>";
                            writer.AddTableData(new string[] { "", Functions.linkObject(trn), remove, trn.Description, "" });
                        }
                        else
                            if (isRemovableWithWarning)

                        {
                            KBDoctorOutput.Message( "Procesing... " + trn.Name + " REMOVABLE with warning ");
                            remove = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;RemoveObject&guid=" + trn.Guid.ToString() + "\">Remove</a>";
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

            foreach (TransactionLevel LVL in trn.Structure.GetLevels())
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

        internal static void TreeCommit()
        {
            // Object with parm() rule without in: out: or inout:
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Tree Commit  ";

            output.StartSection("KBDoctor", title);
          //  try
           // {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);

                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Commit", "Description", "in New UTL", "Upd/Ins/Del", "TimeStamp", "Modified Tables" });


                string Anidacion = "";
                KBObjectCollection yaIncluidos = new KBObjectCollection();

                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                KBModel kbModel = UIServices.KB.CurrentModel;

                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());

                foreach (Procedure obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))

                {
                    GraboLlamado(obj, Anidacion, yaIncluidos, writer);
                }


                writer.AddFooter();
                writer.Close();

                bool success = true;
                output.EndSection("KBDoctor", title, success);

                KBDoctorHelper.ShowKBDoctorResults(outputFile);

        }

        private static void GraboLlamado(Procedure obj, string Anidacion, KBObjectCollection yaIncluido, KBDoctorXMLWriter writer)
        {

            if (yaIncluido.Contains(obj) || !(obj is Procedure))
            {
                writer.AddTableData(new string[] { Anidacion + Functions.linkObject(obj), "", "----already included ", "", "", "", "" });
            }
            else
            {

                string commitOnExit = "";
                string commitInSource = "";
                string UpdateInsertDelete = "";
                string doCommit = "";
                object aux = obj.GetPropertyValue("CommitOnExit");
                if (aux != null)
                {
                    commitOnExit = aux.ToString() == "Yes" ? "YES" : " ";
                }
                UpdateInsertDelete = CleanKBHelper.ObjectUpdateDB(obj) ? "YES" : "";

                Procedure prc = (Procedure)obj;
                string source = "";
                try
                {
                    source = Functions.ExtractComments(prc.ProcedurePart.Source.ToString().ToUpper());
                }
                catch (Exception e)
                    { KBDoctorOutput.Error("ERROR Grabollamado:" + e.Message); };

                if (source.Contains("COMMIT"))
                    commitInSource = "YES";
                else
                    commitInSource = "";

                if ((commitOnExit == "YES" && UpdateInsertDelete == "YES") || (commitInSource == "YES"))
                {
                    doCommit = "YES";
                };

                string ExecuteInNewLuw = "";
                object aux2 = obj.GetPropertyValue(Properties.PRC.ExecuteInNewLuw);
                if (aux2 != null)
                {
                    ExecuteInNewLuw = aux2.ToString() == "True" ? "YES" : "";
                }

                KBModel model = obj.Model;
                IList<KBObject> tableUpdInsDel = (from r in model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                                                  where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                                  where (ReferenceTypeInfo.HasUpdateAccess(r.LinkTypeInfo) | ReferenceTypeInfo.HasDeleteAccess(r.LinkTypeInfo) | ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo))
                                                  select model.Objects.Get(r.To)).ToList();

                string tblList = "";
                string coma = "";
                foreach (KBObject objref in tableUpdInsDel)
                {
                    tblList += coma + Functions.linkObject(objref);
                    coma = ",";
                }
                writer.AddTableData(new string[] { Anidacion + Functions.linkObject(obj), doCommit, obj.Description, ExecuteInNewLuw, UpdateInsertDelete, obj.Timestamp.ToShortDateString(), tblList });


                // Anidacion += "____";
                yaIncluido.Add(obj);

                Parse(obj.Model, obj, Anidacion, yaIncluido, writer);

            }
        }

        static public void Parse(KBModel modelo, KBObject obj, string Anidacion, KBObjectCollection yaIncluido, KBDoctorXMLWriter writer)
        {
            ILanguageService parserSrv = Artech.Architecture.Common.Services.Services.GetService(new Guid("C26F529E-9A69-4df5-B825-9194BA3983A3")) as ILanguageService;
            IParserEngine parser = parserSrv.CreateEngine();
            ParserInfo parserInfo;

            // Para parsear los eventos de un Procedure
            Artech.Genexus.Common.Parts.ProcedurePart source = obj.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
            if (source != null)
            {
                parserInfo = new ParserInfo(source);
                
                int FCLobjClass = 0;
                string FCIobjName = "";
                foreach (TokenData token in parser.GetTokens(true, parserInfo, source.Source))
                {

                    if (token.Token == (int)TokensIds.FCL)
                        FCLobjClass = token.Id;
                    if (token.Token == (int)TokensIds.FOI)
                    {
                        FCIobjName = token.Word;
                        KBDoctorOutput.Message( Anidacion + FCLobjClass.ToString() + "-" + FCIobjName);

                        EntityKey objKey = new EntityKey(ObjClass.Procedure, token.Id);
                        KBObject objRef = KBObject.Get(obj.Model, objKey);
                        if (objRef != null)
                             GraboLlamado((Procedure)objRef, Anidacion + "____", yaIncluido, writer);
                        //Parse(obj.Model, objRef, Anidacion + "____");
                    }
                    if (token.Token == (int)TokensIds.DTCMM)
                        writer.AddTableData(new string[] { Anidacion + " COMMIT ", "", "Commit Explicito", "", "", "", "", "" });


                }
            }
        }

        public enum TokensIds
        {
            FNONE = -1,

            ///////////////////////////////////////////////////////////
            // TOKENS
            ///////////////////////////////////////////////////////////

            // 0
            FOB = 0, // '(' Open Bracket
            FFN,  // 'Function(' Fuction call
            FNA,  // Name Attribute
            FNC,  // Name Cconstant
            FCB,  // ')' Close Bracket
            FPL,  // '+''-' PLus MiNus oper.
            FPR,  // '*''/' PRoduct DiVis. oper.
            FCM,  // ',' CoMma separate parms.
            FNT,  // 'NOT' NoT
            FAN,  // 'AND' 'OR' ANd OR
                  // 10
            FRE,  // '<' '>' '=' RElational oper.
            EXP,  // Expression
            SUM,  // Sum
            COU,  // Count
            AVE,  // Average
            MAX,  // Maximum
            MIN,  // Minimum
            FIF,  // IF ...
            FSC,  // Semicolon ';'
            FOT,  // Otherwise
                  // 20
            ERR_TOKEN,
            FEN,  // EOExpression
            FCO,  // COmment (//) for rules/commands
            FUV,  // User Variable (&) for rules/commands
            FUA,  // User Variable Array '&xx('
            FCN,  // CoNtinuation Line / White spaces
            FAM,  // String to replace '&' with '&&'
            FCL,  // CLass id (used for calls)
            FOI,  // Object Id (used for calls)
            FCT,  // ConTrol ID/Name (for properties)
                  // 30
            FCI,  // Control type Id (combo/edit/etc.)
            FMT,  // control id/name (for MeThods) (Used only in specifier)
            FBI,  // BInary info in value (used to save bin data in obj_info)
            FDC,  // Date constante (used only in dYNQ by now)
            FCV,  // Control Variable (the var associated with the control (Used only in specifier)
            FWH,  // WHEN (GXW) / WHERE (DKL) ...
            FNS,  // Name space ...
            FON,  // ON ...
            FBC,  // Comentario de bloque
            FOR,  // ORDER ...
                  // 40
            TKN_TRUE, // TRUE
            TKN_FALSE, // FALSE
            TKN_NONE, // NONE, para expresión FOR EACH ... ORDER NONE ... ENDFOR 
            PRM,  // Parámetro, utilizado en DYNQ
            FND,  // Name Domain
            FLV,  // LEVEL token
            TKN_NEW, // NEW token
            FSDTCLS, // Structure Class
            TKN_NULL, // NULL
            TKN_IN,  // IN
                     // 50
            SSL,  // SUBSELECT : used by generators; reserved it for Gx.
            FEX,  // Exception name
            TMSGID,  // Message id
            TNCNT,  // Token Name Constant NonTranslatable
            TFOR,  // For token, defined to be used with Lookup Deklarit's rule
            TDEPENDENCIES, // Dependencies token, new condition for rules.
            TRULE,  // Rule token
            TBY,  // 'By' token
            TGIVEN,  // 'Given' token
            TWHERE,  // 'Where' token -GeneXus, Deklarit uses FWH
                     // 60
            TDEFINEDBY, // 'Defined by' token
            TSECTION, // [Web], [Win], [Web], [Text]
            TINDP,  // Used for token 'in <dataselector>'
            OPENSQUAREBRACKET,
            CLOSESQUAREBRACKET,
            OUTPUTNAME,
            OUTPUTDYNAMICSYM,
            INPUT,
            OUTPUTPROPERTY,
            OBJREFERENCE,
            // 70
            TUSING,
            TSIGN, // Now that rules supports comments, define the TSIGN token to specified the sign of an expression (e.g. "-1")
            TEXO,

            // ¡¡¡ UNTIL 99 !!!
            //  Commands codes starts at 100, See dedotcmd.h

            ///////////////////////////////////////////////////////////
            // COMMANDS
            ///////////////////////////////////////////////////////////

            // 100
            DTEJE = 100, // 'Eject'
            DTNSK,  // 'NoSkip'
            DTLNN,  // 'Lineno'
            DTPRC,
            DTCLL,  // 'Call'
            DTDBA,
            DTCOB,
            DTASG,  // Assignment
            DTPRI,
            DTIF,
            // 110
            DTELS,  // 'Else'
            DTEIF,  // 'Endif'
            DTNPR,
            DTDEL,  // 'Delete'
            DTDO,  // 'Do'
            DTEDO,  // 'Enddo'
            DTWHE,
            DTNEW,
            DTRET,
            DTHEA,
            // 120
            DTBEG,
            DTFOR,  // 'ForEach'
            DTEND,
            DTPL,
            DTMT,
            DTMB,
            DTSRC,
            DTENW,
            DTEFO,  // 'EndFor'
            DTWDU,  // 'When Duplicate'
                    // 130
            DTWNO,  // 'When None'
            DTCP,
            DTCMM,
            DTXFE,
            DTXFF,
            DTXNW,
            DTXEF,
            DTXEN,
            DTDBY,
            DTEXF,  // 'Exit' from a 'Do While'
                    // 140
            DTEXD,
            DTMSG,
            DTFOO,
            DTPRO, // 'Sub' <subroutine>
            DTEPR, // 'EndSub'
            DTDOP, // Do <subroutine>
            DTEVT,
            DTEEV,
            DTREF,
            DTFLN,
            // 150
            DTEFL,
            DTCNF,
            DTDOC,
            DTCAS,
            DTECA,
            DTLOA,
            DTLVL,
            DTRBK, // Comando ROLLBACK
            DTSBM, // Comando SUBMIT
            DTGRA,
            // 160
            DTERH, // Commando Error_Handler
            DTVB,  // Comando VB
            DTFSL,
            DTDMY, //Reserved for spec RPC
            DTOTH,
            DTEFS, // Reserved for End for each selected line
            DTJAV, // Comando JAVA
            DTSQL, // Comando SQL
            DTFLS,
            DTFSS,
            // 170
            DTEFF,
            DTLNK, // Comando LINK
            DTAPL, // Asignación del tipo +=
            DTAMI, // Asignación del tipo -=
            DTAMU, // Asignación del tipo *=
            DTADI, // Asignación del tipo /=
            DTFIN, // FOR <var> IN <array>
            DTEFI, // END // del token anterior
            DTFFT, // FOR <var>=<exp> TO <exp> STEP <exp>
            DTEFT, // END // del token anterior
                   // 180
            DTIN,  // Comando IN de FOR <var> IN <array>
            DTTO,  // Comando TO de FOR EACH <var>=<exp> TO <exp>
            DTSTP, // Comando STEP de FOR <var>=<exp> TO <exp> STEP <exp>
            DTCSH, // Comando CSHARP
            DTON,  // Comando ON
            DTWHN, // Comando WHEN
            DTOPD, // Comando OPTION DISTINCT
            DTUSG, // Comando USING de FOR EACH ... ENDFOR
            DTPOPUP, // Comando POPUP()
            BLOCKING, // Comando BLOCKING
                      // 190
            OUTPUTELEMENT,
            OPENCURLYBRACKET,
            CLOSECURLYBRACKET,
            PRINT,
            INSERT,
            SUBGROUP,
            ENDSUBGROUP,
            DTStub, // 'public sub'
            DTJavaScript, // 'javascript' command - not implemented yet! - reserved number
            DTEndStub,
            //200
            DTCallStub,
            DTRuby,  // Comando "RUBY <LINE>"

            DTREDUNDANCY = 397, // Used to give redundancy info to the specifier
        };
        /*
private static void IsLevelRemovable(Transaction trn, Table tbl, out bool isRemovable, out bool isRemovableWithWarning)
{
isRemovable = true;
isRemovableWithWarning = true;
lstTrns = "";
attExclusive = new KBObjectCollection();

foreach (TransactionLevel LVL in trn.Structure.GetLevels())
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
           KBDoctorOutput.Message("Transaction " + trn.Name + " Table " + tblName + " LVL " + LVL.Name + " Attribute "
               + a.Name + " not in any generated transaction");
           isRemovable = false;
           isLevelRemovable = false;
       }

       if (!attLvlAll.Contains(a))
       {
           KBDoctorOutput.Message("Transaction " + trn.Name + " Table " + tblName + " LVL " + LVL.Name + " Attribute "
               + a.Name + " not in any other transaction");
           isRemovableWithWarning = false;
           isLevelRemovable = false;
           attExclusive.Add(a);
       }




   }
   if (isLevelRemovable)
       writer.AddTableData(new string[] { tblName, linkObject(trn), LVL.Name, trn.Description, "Level Removable" });

}
}
*/
        private static KBObjectCollection AttributesFromGeneratedTransactions(Table tBL)
        {
            KBObjectCollection attlist = new KBObjectCollection();
            foreach (Transaction trn in tBL.AssociatedTransactions)
            {
                if (isGenerated(trn))
                {
                    foreach (TransactionLevel LVL in trn.Structure.GetLevels())
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
                    foreach (TransactionLevel LVL in trn.Structure.GetLevels())
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
                        /* } */


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

            //QualifiedName qn = 
            //qn.ModuleName = "Root";
            //qn.ObjectName = "Root";
            //Folder root = Folder.Get(kbModel,"Root KBCategory mainCategory = KBCategory.Get(kbserv.CurrentModel, "Main Programs");



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
                    //parm = parm.PadLeft().PadRight();
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
                    // CleanKBHelper.CleanKBObjectVariables(nuevo);
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

        public static void CalculateCheckSum()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            SpecificationListHelper helper = new SpecificationListHelper(kbserv.CurrentModel.Environment.TargetModel);

            string title = "KBDoctor - Generate objects in text format";
            output.StartSection("KBDoctor", title);

            string fechahora = String.Format("{0:yyyy-MM-dd-HHmm}", DateTime.Now);
            string newDir = KBDoctorHelper.ObjComparerDirectory(kbserv) + @"\OBJ-" + fechahora + @"\";
            Directory.CreateDirectory(newDir);

            StringCollection objReferenced = new StringCollection();

            //KBCategory mainCategory = KBCategory.Get(kbserv.CurrentModel, "Main Programs");

            int iObj = 0;
            foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
            {
                if (!(obj is Domain | obj is Theme | obj is DataView | obj is Index | obj is KBCategory | obj is DataProvider | obj is Menubar | obj is DataView | obj is Diagram | obj is Folder | obj is Image |
                    obj is ExternalObject | obj is ThemeClass | obj is ThemeColor | obj is DataViewIndex))
                {
                    iObj += 1;
                    if ((iObj % 200) == 0)
                    {
                        KBDoctorOutput.Message( obj.GetFullName());
                    }
                    WriteObjectToTextFile(obj, newDir);
                }
            }

            bool success = true;
            output.EndSection("KBDoctor", title, success);
        }

        public static void GenerateLocationXML()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            SpecificationListHelper helper = new SpecificationListHelper(kbserv.CurrentModel.Environment.TargetModel);
            string title = "KBDoctor - Genenrate Location.xml";

            string outputFile = kbserv.CurrentKB.UserDirectory + @"\Location.xml";
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            output.StartSection("KBDoctor", title);
            KBDoctorOutput.Message( "Generate Location.xml template in " + outputFile);

            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.WriteStartElement("GXLocations");

            KBCategory mainCategory = KBCategory.Get(kbserv.CurrentModel, "Main Programs");
            foreach (ExternalObject eobj in kbserv.CurrentModel.GetObjects<ExternalObject>())
            {
                if (eobj.GetPropertyValueString(Properties.EXO.Type) == "WSDL")
                {

                    writer.WriteStartElement("GXLocation");
                    string locationName = eobj.QualifiedName.ToString().Replace(".", "_");
                    writer.WriteAttributeString("name", locationName);
                    writer.WriteStartElement("Common");

                    writer.WriteElementString("Host", "www.server.com");
                    writer.WriteElementString("Port", "80");
                    writer.WriteElementString("BaseUrl", "/baseUrl/");
                    writer.WriteElementString("Secure", "");
                    writer.WriteElementString("Timeout", "60");
                    writer.WriteElementString("CancelOnError", "1");

                    writer.WriteElementString("Proxyserverhost", "www.proxy.com");
                    writer.WriteElementString("Proxyserverport", "80");
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteStartElement("HTTP");
                    writer.WriteStartElement("Authentication");
                    writer.WriteElementString("Authenticationmethod", "0=Basic;1:Digest,2:NTML,3:Kerberos");
                    writer.WriteElementString("Authenticationrealm", "domain");
                    writer.WriteElementString("Authenticationuser", "user");
                    writer.WriteElementString("Authenticationpassword", "pass");
                    writer.WriteEndElement();
                    writer.WriteStartElement("Proxyauthentication");
                    writer.WriteElementString("Proxyauthenticationmethod", "0=Basic;1:Digest,2:NTML,3:Kerberos");
                    writer.WriteElementString("Proxyauthenticationrealm", "proxydomain");
                    writer.WriteElementString("Proxyauthenticationuser", "proxyuser");
                    writer.WriteElementString("Proxyauthenticationpassword", "proxypass");
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

            }
            writer.WriteEndElement();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection("KBDoctor", title, success);

        }

        private static void WriteObjectToTextFile(KBObject obj, string newDir)
        {

            string name = obj.GetFullName();
            name = name.Replace("'", "");
            name = name.Replace(":", "_");
            name = name.Replace("/", "_");
            name = name.Replace("\\", "_");
            name = name.Replace(" ", "_");

            string FileName = newDir + name + ".txt";

            System.IO.StreamWriter file = new System.IO.StreamWriter(FileName);

            /* 
            RulesPart rp = obj.Parts.Get<RulesPart>();
            if (rp != null)
            {
                file.WriteLine("=== RULES ===");
                file.WriteLine(rp.Source);
            }
            */

            switch (obj.TypeDescriptor.Name)
            {

                case "Attribute":

                    Artech.Genexus.Common.Objects.Attribute att = (Artech.Genexus.Common.Objects.Attribute)obj;

                    file.WriteLine(Utility.FormattedTypeAttribute(att));
                    if (att.Formula == null)
                        file.WriteLine("");
                    else
                        file.WriteLine(att.Formula.ToString());
                    break;

                case "Procedure":
                    ProcedurePart pp = obj.Parts.Get<ProcedurePart>();
                    if (pp != null)
                    {
                        file.WriteLine("=== PROCEDURE SOURCE ===");
                        file.WriteLine(pp.Source);
                    }
                    break;
                case "Transaction":
                    StructurePart sp = obj.Parts.Get<StructurePart>();
                    if (sp != null)
                    {
                        file.WriteLine("=== STRUCTURE ===");
                        file.WriteLine(sp.ToString());
                    }

                    EventsPart ep = obj.Parts.Get<EventsPart>();
                    if (ep != null)
                    {
                        file.WriteLine("=== EVENTS SOURCE ===");
                        file.WriteLine(ep.Source);
                    }

                    break;

                case "WorkPanel":
                    WorkPanel wkp = (WorkPanel)obj;

                    ep = obj.Parts.Get<EventsPart>();
                    if (ep != null)
                    {
                        file.WriteLine("=== EVENTS SOURCE ===");
                        file.WriteLine(ep.Source);
                    }
                    break;

                case "WebPanel":

                    WebPanel wbp = (WebPanel)obj;
                    ep = obj.Parts.Get<EventsPart>();
                    if (ep != null)
                    {
                        file.WriteLine("=== EVENTS SOURCE ===");
                        file.WriteLine(ep.Source);
                    }
                    break;


                case "WebComponent":

                    wbp = (WebPanel)obj;
                    ep = obj.Parts.Get<EventsPart>();
                    if (ep != null)
                    {
                        file.WriteLine("=== EVENTS SOURCE ===");
                        file.WriteLine(ep.Source);
                    }
                    break;

                case "Table":
                    Table tbl = (Table)obj;

                    foreach (TableAttribute attr in tbl.TableStructure.Attributes)
                    {
                        String line = "";
                        if (attr.IsKey)
                        {
                            line = "*";
                        }
                        else
                        {
                            line = " ";
                        }

                        line += attr.Name + "  " + attr.GetPropertiesObject().GetPropertyValueString("DataTypeString") + "-" + attr.GetPropertiesObject().GetPropertyValueString("Formula");

                        if (attr.IsExternalRedundant)
                            line += " External_Redundant";

                        line += " Null=" + attr.IsNullable;
                        if (attr.IsRedundant)
                            line += " Redundant";

                        file.WriteLine(line);
                    }
                    break;


                case "SDT":
                    SDT sdtToList = (SDT)obj;
                    if (sdtToList != null)
                    {
                        file.WriteLine("=== STRUCTURE ===");
                        ListStructure(sdtToList.SDTStructure.Root, 0, file);
                    }
                    break;

                default:
                    break;

            }
            file.Close();
        }

        internal static void ListCommitOnExit()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            
            bool commitOnExit;
            string title = "KBDoctor - Commit on Exit = Yes";
            string objNameLink;

            output.StartSection("KBDoctor", title);
            try
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter("CommitOnExit.txt");
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Type", "Name", "Description", "UpdateDB?" });

                foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
                {
                    ICallableObject callableObject = obj as ICallableObject;
                    if (callableObject != null && isGenerated(obj))
                    {
                        KBDoctorOutput.Message("Processing " + obj.TypeDescriptor.Name + " " + obj.Name);

                            object aux = obj.GetPropertyValue("CommitOnExit");
                            commitOnExit = ((aux != null) && (aux.ToString() == "Yes"));
                        string objName = obj.Name;
                        string updateDB = "";
                        bool updateDBbool = CleanKBHelper.ObjectUpdateDB(obj);

                        if (commitOnExit && obj.IsPropertyDefault("CommitOnExit") && isGenerated(obj))
                            {
                           
                                if (updateDBbool)
                                {
                                /*
                                obj.SetPropertyValue("CommitOnExit", "Yes");
                                obj.Description = obj.Description + '.';
                                obj.Save();*/
                                updateDB = "YES";
                                }
                                    
                                objNameLink = Functions.linkObject(obj);
                                writer.AddTableData(new string[] { obj.TypeDescriptor.Name, objNameLink, obj.Description, updateDB });
                            }
                        if (obj is Procedure || obj is Transaction)
                        {
                            file.WriteLine(obj.TypeDescriptor.Name + ":" + obj.Name + "  CommitOnExit: " + commitOnExit.ToString() + "  UpdateDB: " + updateDBbool.ToString());
                        }
                    }
                }

                writer.AddFooter();
                writer.Close();
                file.Close();

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
    

        private static void ListStructure(SDTLevel level, int tabs, System.IO.StreamWriter file)
        {
            WriteTabs(tabs, file);
            file.Write(level.Name);
            if (level.IsCollection)
                file.Write(", collection: {0}", level.CollectionItemName);
            file.WriteLine();

            foreach (var childItem in level.GetItems<SDTItem>())
                ListItem(childItem, tabs + 1, file);
            foreach (var childLevel in level.GetItems<SDTLevel>())
                ListStructure(childLevel, tabs + 1, file);
        }

        private static void ListItem(SDTItem item, int tabs, System.IO.StreamWriter file)
        {
            WriteTabs(tabs, file);
            string dataType = item.Type.ToString().Substring(0, 1) + "(" + item.Length.ToString() + (item.Decimals > 0 ? "." + item.Decimals.ToString() : "") + ")" + (item.Signed ? "-" : "");
            file.WriteLine("{0}, {1}, {2} {3}", item.Name, dataType, item.Description, (item.IsCollection ? ", collection " + item.CollectionItemName : ""));
            //if (item.IsCollection)
            //    file.Write(", collection: {0}", item.CollectionItemName);
        }

        private static void WriteTabs(int tabs, System.IO.StreamWriter file)
        {
            while (tabs-- > 0)
                file.Write('\t');
        }

        private static void WriteCopyObject(IOutputService output, KBObject obj, StringCollection tableOperation, KBObjectCollection objMarked, string mainstr, string Dircopia)
        {
            IKBService kbserv = UIServices.KB;
            string linea = String.Format(@"XCOPY /y/d {0}bin\{1}.dll %{2}%\bin\", Dircopia, obj.Name, mainstr);

            if (!(obj is Procedure) && !(obj is WorkPanel) && !(tableOperation.Contains(linea)))
            {
                KBDoctorOutput.Message( linea);
                KBDoctorOutput.Message( String.Format(@"XCOPY /y/d {0}{1}.js  %{2}%\ ", Dircopia, obj.Name, mainstr));
                tableOperation.Add(linea);
            }

            objMarked.Add(obj);

            foreach (EntityReference reference in obj.GetReferences())
            {
                KBObject objRef = KBObject.Get(obj.Model, reference.To);

                if ((objRef != null) && !objMarked.Contains(objRef))
                {
                    if (!Utility.IsMain(objRef))
                    {
                        if (objRef is Transaction || objRef is WorkPanel || objRef is WebPanel || objRef is Menubar || objRef is Procedure || objRef is DataProvider || objRef is DataSelector)
                            WriteCopyObject(output, objRef, tableOperation, objMarked, mainstr, Dircopia);
                    }
                    else
                        return;

                }


            }
        }

        public static void ObjectsComplex()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Complex Objects";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                output.StartSection("KBDoctor", title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Description", "Type", "Size" });

                string[] fileEntries = Directory.GetFiles(KBDoctorHelper.SpcDirectory(kbserv), "*.SP0", System.IO.SearchOption.AllDirectories);

                foreach (string fileName in fileEntries)
                {

                    var length = new System.IO.FileInfo(fileName).Length;
                    string objName = Path.GetFileNameWithoutExtension(fileName);

                    string[] ns = new[] { "Objects" }; //System.Collections.SortedList()  ;
                                                       //


                    foreach (KBObject obj in UIServices.KB.CurrentModel.Objects.GetByPartialName(ns, objName))
                    {

                        if ((obj.Name == objName) && (length > 200000))
                        {
                            writer.AddTableData(new string[] { Functions.linkObject(obj), obj.Description, obj.TypeDescriptor.Name, length.ToString("N0") });
                            KBDoctorOutput.Message( fileName);
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

        public static void ObjectsLegacyCode()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string[] legacyCode = new string[] { "Object", "Description", "Type", " call", " udp", " create", ".false", ".true", "new", "defined", "delete", ".and.", ".or.", ".not.", ".like." };
            string titulo = "KBDoctor - Objects - Legacy Code";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, titulo);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);

                output.StartSection(titulo);
                writer.AddHeader(titulo);
                writer.AddTableHeader(legacyCode);
                int objWithLegacyCode = 0;

                foreach (KBObject obj in UIServices.KB.CurrentModel.Objects.GetAll())
                {

                    if (obj is Transaction || obj is WebPanel || obj is Procedure || obj is WorkPanel)
                    {

                        if (isGenerated(obj))
                        {
                            KBDoctorOutput.Message( obj.Name);

                            string source = ObjectSource(obj);
                            source = Functions.RemoveEmptyLines(source);
                            string sourceWOComments = Functions.ExtractComments(source);
                            bool hasLegacyCode = false;
                            string[] data = new string[legacyCode.Length];
                            data[0] = Functions.linkObject(obj);
                            data[1] = obj.Description;
                            data[2] = obj.TypeDescriptor.Name;

                            for (int i = 3; i < legacyCode.Length; i++) //empieza en la columan 3 para saltear nombre de objeto, descripcion y tipo
                            {
                                if (sourceWOComments.Contains(legacyCode[i].ToUpper()))
                                {
                                    data[i] = "   **  ";
                                    hasLegacyCode = true;
                                }
                                else
                                {
                                    data[i] = "";
                                }
                            }
                            if (hasLegacyCode)
                            {
                                writer.AddTableData(data);
                                objWithLegacyCode += 1;
                            }
                        }
                    }
                }

                writer.AddFooter();
                writer.Close();

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                bool success = true;
                output.EndSection(titulo, success);
                Functions.AddLineSummary(titulo + ".txt", objWithLegacyCode.ToString());
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(titulo, success);
            }

        }

        public static void EditLegacyCodeToReplace()
        {
            string filename = CreateFileWithTextToTeplace();
            Process.Start("notepad.exe", filename);

        }

        public static void EditReviewObjects()
        {
            API.InitializeIniFile(UIServices.KB.CurrentKB);
            string filename = UIServices.KB.CurrentKB.UserDirectory + "\\KBDoctor.ini";
            Process.Start("notepad.exe", filename);

        }
        private static string CreateFileWithTextToTeplace()
        {
            IKBService kbserv = UIServices.KB;

            string filename = kbserv.CurrentKB.UserDirectory + @"\Replace.txt";
            if (!File.Exists(filename))
            {
                File.WriteAllText(filename, Comparer.Replace);
            }

            return filename;
        }

        public static void ChangeLegacyCode()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;

            Regex[] re4 = new Regex[9]
                {
                new Regex(@"([\b]*)([^.])([c][a][l][l][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]", RegexOptions.Compiled | RegexOptions.IgnoreCase ),
                new Regex(@"([\b]*)([^.])([u][d][p][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]" , RegexOptions.Compiled | RegexOptions.IgnoreCase ),
                new Regex(@"([\b]*)([^.])([c][r][e][a][t][e][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]" , RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"([\b]*)([^.])([l][i][n][k][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                new Regex(@"([\b]*)([^.])([c][a][l][l][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]", RegexOptions.Compiled | RegexOptions.IgnoreCase ),
                new Regex(@"([\b]*)([^.])([u][d][p][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]" , RegexOptions.Compiled | RegexOptions.IgnoreCase ),
                new Regex(@"([\b]*)([^.])([c][r][e][a][t][e][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]" , RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"([\b]*)([^.])([l][i][n][k][(])([\s]*[a-z][0-9a-z_]+)[\s]*[,]", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                new Regex(@"^([c][a][l][l][(])([\s]*[a-z][0-9a-z_]+)(,|\))", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline)
                };
            string[] re4Replace = new string[9]
                {
                @"$1$2$4.$3",@"$1$2$4.$3",@"$1$2$4.$3",@"$1$2$4.$3",
                @"$1$2$4.$3)",  @"$1$2$4.$3)",  @"$1$2$4.$3)",  @"$1$2$4.$3)",
                @"$2.$1"
                };

            string[] lines = System.IO.File.ReadAllLines(CreateFileWithTextToTeplace());
            string[,] leg = new string[100, 2];
            int j = 0;
            foreach (string line in lines)
            {
                if (!line.StartsWith("#") && line.Contains("|"))
                {
                    string[] cam = line.Split('|');

                    leg[j, 0] = cam[0];
                    leg[j, 1] = cam[1];
                    j += 1;
                }
            }

            string titulo = "KBDoctor - Change code to improve readability";
            output.StartSection(titulo);

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            KBModel kbModel = UIServices.KB.CurrentModel;

            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WorkPanel>());

            foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                if (obj is Transaction || obj is WebPanel || obj is Procedure || obj is WorkPanel)
                {
                    if (isGenerated(obj) && !isGeneratedbyPattern(obj))
                    {
                        string source = ObjectSource(obj);
                        string newSource = source;

                        KBDoctorOutput.Message( "Object " + obj.Name);

                        string callers = ChangeUDPCallWhenNecesary(obj);
                        // CleanKBHelper.CleanKBObjectVariables(obj);
                        //Cambio expresiones regulares
                        for (int i = 0; i < 9; i++)
                        {
                            newSource = re4[i].Replace(newSource, re4Replace[i]);
                        }

                        //Solo cambio si NO tiene codigo nativo. Cambiar codigo nativo puede traer lios. 
                        if (!(newSource.ToLower().Contains("java") || newSource.ToLower().Contains("csharp")))
                        {
                            for (int i = 0; i < (leg.Length) / 2; i++)
                            {
                                newSource = newSource.Replace(leg[i, 0], leg[i, 1], StringComparison.InvariantCultureIgnoreCase);
                            }

                            if (source != newSource)
                            {
                                try
                                {
                                    KBDoctorOutput.Message( "..Saving.." + obj.Name);
                                    SaveNewSource(obj, newSource);
                                }
                                catch (Exception e)
                                {
                                    output.AddErrorLine(e.Message);
                                    output.AddErrorLine("========= newsource ===============");
                                    KBDoctorOutput.Message( newSource);
                                    success = false;
                                };

                            }

                        }
                    }
                }
            }

            output.EndSection(titulo, success);

        }

        private static string ReplaceLegacyCode(string newSource, string original, string changeto)
        {
            newSource = newSource.Replace(original, changeto, StringComparison.CurrentCultureIgnoreCase);
            return newSource;
        }

        private static void SaveNewSource(KBObject obj, string newSource)
        {

            if (obj is Procedure)
            {
                Procedure p = (Procedure)obj;
                p.ProcedurePart.Source = newSource;
                p.Save();

            }
            if (obj is WebPanel)
            {
                WebPanel p = (WebPanel)obj;
                p.Events.Source = newSource;
                p.Save();
            }
            if (obj is Transaction)
            {
                Transaction p = (Transaction)obj;
                p.Events.Source = newSource;
                p.Save();
            }
            if (obj is WorkPanel)
            {
                WorkPanel p = (WorkPanel)obj;
                p.Events.Source = newSource;
                p.Save();
            }
        }

        private static string ReplaceOneLegacy(string myString, string v)
        {
            //IOutputService output = CommonServices.Output;
            int from, to;
            string newString = "";

            from = myString.ToLower().IndexOf(v);
            to = from + v.Length;
            string aux = myString.Substring(to, myString.Length - to).Trim();
            aux = aux.Replace("(", "");
            // aux = aux.Replace(" ", "");
            string objeto = "";
            foreach (var token in aux.Split(new char[] { ',', ')', ' ' }))
            {
                objeto = token;
                //  KBDoctorOutput.Message("Token>" + token + "<FinToken");
                break;
            }
            if (objeto != "" && objeto.Substring(0, 1) != "&") //Call Dinamico si esta llamando a una variable
            {
                aux = aux.Substring(objeto.Length, aux.Length - objeto.Length);
                if (aux != "" && aux.Substring(0, 1) == ",")
                {
                    aux = aux.Substring(1, aux.Length - 1);
                }

                newString = myString.Substring(0, from) + " " + objeto + "." + v.Trim() + "(" + aux;
            }

            return newString;

        }

        public static void ObjectsWithConstants()
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;

            var sorted = from obj in model.Objects.GetAll()
                         where (!obj.Name.StartsWith("KBD_"))
                         select obj;
            KBDoctorOutput.Message(sorted.ToList().Count.ToString());
            foreach (KBObject obj in sorted )
                {
                string objTypeName = obj.TypeDescriptor.Name;
               // KBDoctorOutput.Message(obj.Name);

                if (objTypeName.Contains("Pattern"))
                    continue;
                if (obj is ThemeClass)
                    continue;
                if (obj.Name.Contains("Context"))
                    continue;
                if (obj.Name.Contains("WorkWith"))
                    continue;
                if (isGeneratedbyPattern(obj))
                    continue;
                if (obj is Domain)
                    continue;
                if (obj is SDT)
                    continue;
                if (obj is GeneratorCategory)
                    continue;
                if (obj is DataStoreCategory)
                    continue;

               // KBDoctorOutput.Message(obj.Name);
                string objName = "KBD_" + obj.Guid.ToString().Replace("-", "");
                obj.Name = objName;
                obj.Description = objName;

                try
                {
                    obj.Save();
                }
                catch 
                    {
                       // KBDoctorOutput.Error(obj.Name + " " + obj.TypeDescriptor.Name + " " + e.Message + " " + e.InnerException);
                    }          

            }
            
        }

        private static bool TokenIsConstant(TokenData token)
        {
            string tokenword = token.Word;


            if (token.Token == 3 && token.Word.Length > 3 && (tokenword.Contains('"') || tokenword.Contains("'")))
            {
                return true;
            }
            else
            {
                decimal myDec;
                var Result = decimal.TryParse(tokenword, out myDec);
                if (Result)
                {
                    if (myDec > 9)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }

        }

        public static void CountTableAccess()
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Count Table Access per Object";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                output.StartSection("KBDoctor", title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Type", "Description", "Module", "Inserts", "Updates", "Delete", "Read", "Total", "Out Module","Out Module Tables" });



                foreach (KBObject obj in UIServices.KB.CurrentModel.Objects.GetAll())
                {
                    if (Functions.isRunable(obj))

                    {
                        KBDoctorOutput.Message( obj.Name);
                        int updaters = (from r in model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                                        where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                        where ReferenceTypeInfo.HasUpdateAccess(r.LinkTypeInfo)
                                        select model.Objects.Get(r.To)).ToList().Count;
                        int inserters = (from r in model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                                         where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                         where ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo)
                                         select model.Objects.Get(r.To)).ToList().Count;
                        int deleters = (from r in model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                                        where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                        where ReferenceTypeInfo.HasDeleteAccess(r.LinkTypeInfo)
                                        select model.Objects.Get(r.To)).ToList().Count;
                        int readers = (from r in model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                                       where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                       where ReferenceTypeInfo.HasReadAccess(r.LinkTypeInfo)
                                       select model.Objects.Get(r.To)).ToList().Count;
                        // ESTO DEJO DE SER NECESARIO EN GX15
                        // readers = readers - updaters - inserters - deleters;
                        int total = updaters + inserters + deleters + readers;

                        List<KBObject> outmodule = (from r in model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                                                    where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                                    where ReferenceTypeInfo.HasUpdateAccess(r.LinkTypeInfo) || ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo) 
                                                       || ReferenceTypeInfo.HasDeleteAccess(r.LinkTypeInfo) || ReferenceTypeInfo.HasReadAccess(r.LinkTypeInfo)
                                                   // where obj.Module != TablesHelper.TableModule(model, (Table)model.Objects.Get(r.To))
                                                    select model.Objects.Get(r.To)).ToList();
                        //Busco las tablas fuera del modulo. 
                        int outmoduleint = 0;
                        string tablas = "";
                        foreach (KBObject o2 in outmodule)
                        {
                            Table tbl = (Table)o2;

                            if (TablesHelper.TableModule(model, tbl) != obj.Module)
                            {
                                outmoduleint += 1;
                                tablas += " " + tbl.Name;
                            }
                        }
                        writer.AddTableData(new string[] { Functions.linkObject(obj), obj.TypeDescriptor.Name, obj.Description, obj.Module.Name, updaters.ToString(), inserters.ToString(), deleters.ToString(), readers.ToString(), total.ToString(),outmoduleint.ToString(),tablas });

                    }

                }
                writer.AddTableFooterOnly();
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

        private static int FindTrunkVersion(IEnumerable<KBVersionData> versions)
        {
            foreach (KBVersionData data in versions)
            {
                if (data.IsTrunk)
                {
                    return data.Id;
                }
            }
            throw new Exception("Could not find Trunk KBversion");
        }

        private static int FindKBVersion(IEnumerable<KBVersionData> versions, string serverKbVersion)
        {
            if (string.IsNullOrEmpty(serverKbVersion))
                return FindTrunkVersion(versions);

            foreach (KBVersionData data in versions)
            {
                if (string.Compare(data.Name, serverKbVersion, true) == 0)
                {
                    return data.Id;
                }
            }
            throw new Exception(string.Format("Could not find KBversion '{0}'", serverKbVersion));
        }

        private static IEnumerable<KBRevisionData> FilterRevisions(IEnumerable<KBRevisionData> revisions, DateTime FromDate, DateTime ToDate)
        {
            foreach (KBRevisionData data in revisions)
            {
                if (data.Timestamp < FromDate || data.Timestamp > ToDate)
                    continue;

                yield return data;
            }
        }

        public static void GenerateCSV_ObjectsRefactoring()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Refactoring candidates";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title) + ".csv";

                output.StartSection("KBDoctor", title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                // writer.AddHeader(title);
                writer.AddCSVLine(new string[] { "Object", "Description", "Type", "Module",
                                "#ParmIN", "#ParamOUT", "#ParamINOUT", "#Parameters",
                                "#Comments", "#Lines", "Max Nest", "Longest Code Block", "Cyclomatic Complexity",
                                "#Rules", "#FormControls", "#Var", "#ReadTables", "#UpdateTables", "#InsertTables", "#DeleteTables", "#FromRef", "#ToRef", "TimeStamp", "#Commits" });

                DateTime FromDate = new DateTime(1970, 01, 01);
                DateTime ToDate = DateTime.Today;
                string querystring = KBDoctorCore.Sources.Utility.GetQueryStringFromToDate(FromDate, ToDate);
                List<IKBVersionRevision> revisions_list = new List<IKBVersionRevision>();
                List<IKBVersionRevision> revisions_list_iter = new List<IKBVersionRevision>();
                bool fin = false;
                int retrys = 0;
                int i = 1;
                while (!fin)
                {
                    try
                    {
                        revisions_list_iter = (List<IKBVersionRevision>)UIServices.TeamDevClient.GetRevisions(kbserv.CurrentModel.KBVersion, querystring, i);
                        if (revisions_list_iter.Count > 0)
                        {
                            revisions_list.AddRange(revisions_list_iter);
                        }
                        retrys = 0;
                        i++;
                    }
                    catch(Exception e)
                    {
                        retrys++;
                        if (retrys == 5)
                        {
                            fin=true;
                        }
                        KBDoctorOutput.Message("Error: " + e.Message);
                        KBDoctorOutput.Message("Retry nro: " + retrys.ToString());
                    }
                    
                    if (revisions_list_iter.Count < 50)
                    {
                        fin = true;
                    }
                    KBDoctorOutput.Message("Iteración: " + i.ToString());
                }

                KBDoctorOutput.Message("Processing commits");
                Dictionary<string,int> dict_commits = GenerateCommitsDictionary(revisions_list, kbserv.CurrentModel);
                IEnumerable<KBObject> kbobjs = UIServices.KB.CurrentModel.Objects.GetAll();
                int total = kbobjs.Count();
                int percent_cant = Convert.ToInt32(Math.Round((double)(total / 100)));
                int nro_iter = 0;
                foreach (KBObject obj in kbobjs)
                {
                    nro_iter++;
                    if(nro_iter % percent_cant == 0)
                    {
                        double percent = ((double)nro_iter / (double)total)*100;
                        KBDoctorOutput.Message(percent.ToString() + "%");
                    }
                    if (obj is Transaction || obj is WebPanel || obj is Procedure || obj is WorkPanel)
                    {
                        if (isGenerated(obj) && !isGeneratedbyPattern(obj))
                        {
                           // KBDoctorOutput.Message("Processing :" + obj.Name);
                            

                            string source = Functions.ObjectSourceUpper(obj);
                            source = Functions.RemoveEmptyLines(source);

                            string sourceWOComments = Functions.ExtractComments(source);
                            sourceWOComments = Functions.RemoveEmptyLines(sourceWOComments);

                            int linesSource, linesComment;
                            float PercentComment;

                            CountCommentsLines(source, sourceWOComments, out linesSource, out linesComment, out PercentComment);

                            int MaxCodeBlock = Functions.MaxCodeBlock(sourceWOComments);
                            int MaxNestLevel = Functions.MaxNestLevel(sourceWOComments);
                            int ComplexityLevel = Functions.ComplexityLevel(sourceWOComments);

                            int parametersCount = ParametersCountObject(obj);

                            int ParamIn = ParametersTypeCountObject(obj, "PARM_IN");
                            int ParamOUT = ParametersTypeCountObject(obj, "PARM_OUT"); 
                            int ParamINOUT = ParametersTypeCountObject(obj, "PARM_INOUT"); 
                            int VarNumber = VariablesCountObject(obj);
                            int FromRef = ReferencesFromCountObject(obj);
                            int ToRef = ReferencesToCountObject(obj);
                            TablesAccessCountObject(obj, out int UpdateTables, out int InsertTables, out int DeleteTables, out int ReadTables);
                            DateTime TimeStamp = DateTime.Now;

                            int RulesNumber = CountRules(obj);
                            int FormControlsNumber = CountWebFormTags(obj);
                            int CommitTotal;
                            if (dict_commits.ContainsKey(obj.QualifiedName.ObjectName))
                            {
                                CommitTotal = dict_commits[obj.QualifiedName.ObjectName];
                            }
                            else
                            {
                                CommitTotal = 0;
                            }

                            writer.AddCSVLine(new string[] { obj.Name, obj.Description, obj.TypeDescriptor.Name, obj.Module.Name,
                                ParamIn.ToString(), ParamOUT.ToString(), ParamINOUT.ToString(), parametersCount.ToString(),
                                linesComment.ToString(), linesSource.ToString(), MaxNestLevel.ToString(), MaxCodeBlock.ToString(), ComplexityLevel.ToString(),
                                RulesNumber.ToString(), FormControlsNumber.ToString(), VarNumber.ToString(),
                                ReadTables.ToString(), UpdateTables.ToString(), InsertTables.ToString(), DeleteTables.ToString() , FromRef.ToString(), ToRef.ToString(), TimeStamp.ToString(), CommitTotal.ToString()
                            });

                        }
                    }
                }

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
        
        private static Dictionary<string, int> GenerateCommitsDictionary(IEnumerable<KBRevisionData> revisions_list, KBModel model)
        {
            Dictionary<string, int> dict_commits = new Dictionary<string, int>();
            foreach (KBRevisionData data in revisions_list)
            {
                foreach (KBRevisionActionData action in data.Actions)
                {
                    if (dict_commits.ContainsKey(action.ObjectName))
                    {
                        dict_commits[action.ObjectName] = dict_commits[action.ObjectName] + 1;
                    }
                    else
                    {
                        dict_commits.Add(action.ObjectName, 1);
                    }
                }
            }
            return dict_commits;
        }
            
        private static Dictionary<string, int> GenerateCommitsDictionary(List<IKBVersionRevision> revisions_list, KBModel model)
        {
            Dictionary<string, int> dict_commits = new Dictionary<string, int>();
            foreach (IKBVersionRevision revision in revisions_list)
            {
                foreach (IRevisionAction action in revision.Actions)
                {
                    QualifiedName qn;
                    KBObject obj_act = model.Objects.Get(action.Guid);
                    if (obj_act != null)
                    {
                        qn = obj_act.QualifiedName;
                        if (dict_commits.ContainsKey(qn.ObjectName))
                        {
                            dict_commits[qn.ObjectName] = dict_commits[qn.ObjectName] + 1;
                        }
                        else
                        {
                            dict_commits.Add(qn.ObjectName, 1);
                        }
                    }
                }
            }
            return dict_commits;
        }
        /*
        private static int CountCommits(KBObject obj, List<IKBVersionRevision> revisions_list)
        {
            int commits = 0;
            Dictionary<string, List<string[]>> review_by_user;
            List<string> objs_reviewed = new List<string>();
            foreach (IKBVersionRevision revision in revisions_list)
            {
                foreach (IRevisionAction action in revision.Actions)
                {
                    QualifiedName qn;
                    string name = "";
                    if (obj.Model.Objects.GetName(action.Key) != null)
                    {
                        KBObject obj_act = obj.KB.DesignModel.Objects.Get(action.Guid);
                        qn = obj_act.QualifiedName;
                        name = qn.ObjectName;
                    }
                    if (name != "")
                    {
                        if(name == obj.QualifiedName.ObjectName)
                        {
                            commits++;
                        }
                    }
                }
            }
            return commits;
        }*/

        private static int CountRules(KBObject obj)
        {
            string rules = Functions.ObjectRulesUpper(obj);
            rules = Functions.RemoveEmptyLines(rules);

            string rulesWOComments = Functions.ExtractComments(rules);
            rulesWOComments = Functions.RemoveEmptyLines(rulesWOComments);

            int linesRules, linesComment;
            float PercentComment;

            CountCommentsLines(rules, rulesWOComments, out linesRules, out linesComment, out PercentComment);

            return linesRules;
        }

        private static int CountWebFormTags(KBObject obj)
        {
            int tagcant = 0;
            if (((obj is Transaction) || (obj is WebPanel) || obj is ThemeClass) && (obj.GetPropertyValue<bool>(Properties.TRN.GenerateObject)))
            {
                WebFormPart webForm = obj.Parts.Get<WebFormPart>();

                foreach (IWebTag tag in WebFormHelper.EnumerateWebTag(webForm))
                {
                    tagcant++;
                }
            }
            return tagcant;
        }
        private static void TablesAccessCountObject(KBObject obj, out int updaters, out int inserters, out int deleters, out int readers)
        {
            updaters = 0;
            inserters = 0;
            deleters = 0;
            readers = 0;

            updaters = (from r in obj.Model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                            where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                            where ReferenceTypeInfo.HasUpdateAccess(r.LinkTypeInfo)
                            select obj.Model.Objects.Get(r.To)).ToList().Count;
            inserters = (from r in obj.Model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                             where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                             where ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo)
                             select obj.Model.Objects.Get(r.To)).ToList().Count;
            deleters = (from r in obj.Model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                            where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                            where ReferenceTypeInfo.HasDeleteAccess(r.LinkTypeInfo)
                            select obj.Model.Objects.Get(r.To)).ToList().Count;
            readers = (from r in obj.Model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                           where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                           where ReferenceTypeInfo.HasReadAccess(r.LinkTypeInfo)
                           select obj.Model.Objects.Get(r.To)).ToList().Count;

        }

        private static int ReferencesToCountObject(KBObject obj)
        {
            int callers = 0;
            foreach (EntityReference auxer in obj.Model.GetReferencesFrom(obj.Key))
            {
                KBObject auxobj = KBObject.Get(obj.Model, auxer.To);
                if(auxobj != null && auxer.ReferenceType != ReferenceType.Weak && auxer.LinkType != LinkType.Parent)
                {
                    string name = auxobj.Name;
                    callers++;
                }
            }

          //  callers = list_ref.Count();
            return callers;
        }

        private static int ReferencesFromCountObject(KBObject obj)
        {
            int callers = 0;
            callers = obj.GetReferencesTo(LinkType.UsedObject).Count();
            return callers;
        }

        private static int VariablesCountObject(KBObject obj)
        {
            int cantVar = 0;
            VariablesPart vp = obj.Parts.Get<VariablesPart>();
            if (vp != null)
            {
                foreach(Variable var in vp.Variables)
                {
                    if (!var.IsStandard)
                    {
                        cantVar++;
                    }
                }
            }
            return cantVar;
        }
        public static void ObjectsRefactoringCandidates()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Refactoring candidates";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                output.StartSection("KBDoctor", title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Description", "Type", "Module", "Folder", "Parm/IN:OUT:", "#Parameters",  "#Comments", "#Lines", "Max Nest", "Longest Code Block", "Cyclomatic Complexity", "#Rules", "#FormControls","#Var","#ReadTables", "#UpdateTables", "#InsertTables", "#DeleteTables","#FromRef", "#ToRef", "TimeStamp","Candidate", "Complexity Index" });

                int ComplexityIndexTotal = 0;
                int ObjectsTotal = 0;

                foreach (KBObject obj in UIServices.KB.CurrentModel.Objects.GetAll())
                {

                    if (obj is Transaction || obj is WebPanel || obj is Procedure || obj is WorkPanel)
                    {
                        if (isGenerated(obj) && !isGeneratedbyPattern(obj))
                        {
                            KBDoctorOutput.Message( obj.Name);

                            string source = Functions.ObjectSourceUpper(obj);
                            source = Functions.RemoveEmptyLines(source);

                            string sourceWOComments = Functions.ExtractComments(source);
                            sourceWOComments = Functions.RemoveEmptyLines(sourceWOComments);

                            int linesSource, linesComment;
                            float PercentComment;

                            CountCommentsLines(source, sourceWOComments, out linesSource, out linesComment, out PercentComment);

                            int MaxCodeBlock = Functions.MaxCodeBlock(sourceWOComments);
                            int MaxNestLevel = Functions.MaxNestLevel(sourceWOComments);
                            int ComplexityLevel = Functions.ComplexityLevel(sourceWOComments);

                            string ParmINOUT = Functions.ValidateINOUTinParm(obj) ? "Error" : "";
                            int parametersCount = ParametersCountObject(obj);

                            string Candidate = "";
                            if ((ParmINOUT == "Error") || (MaxNestLevel > 6) || (ComplexityLevel > 30) || (MaxCodeBlock > 500) || (parametersCount > 6))
                            {
                                Candidate = "*";
                            }

                            int ComplexityIndex = CalculateComplexityIndex(MaxCodeBlock, MaxNestLevel, ComplexityLevel, ParmINOUT);

                            string folder = obj.Parent.Name;

                            string codeCommented = Functions.CodeCommented(source);
                            codeCommented = codeCommented.Replace("'", "");
                            codeCommented = codeCommented.Replace(">", "");
                            codeCommented = codeCommented.Replace("<", "");

                            writer.AddTableData(new string[] { Functions.linkObject(obj), obj.Description, obj.TypeDescriptor.Name, obj.Module.Name, folder, ParmINOUT, parametersCount.ToString(), codeCommented, PercentComment.ToString("0"), linesComment.ToString(), linesSource.ToString(), MaxNestLevel.ToString(), MaxCodeBlock.ToString(), ComplexityLevel.ToString(), Candidate, ComplexityIndex.ToString() });
                            ObjectsTotal += 1;
                            ComplexityIndexTotal += ComplexityIndex;
                        }
                    }
                }
                writer.AddTableFooterOnly();
                writer.AddTableFooterOnly();

                int Average = ComplexityIndexTotal / ObjectsTotal;
                writer.AddTableHeader(new string[] { "Totals Objects= ", ObjectsTotal.ToString(), " Complexity Index Sum= ", ComplexityIndexTotal.ToString(), " Complexity Index Average= " + Average.ToString() });

                writer.AddTableFooterOnly();
                writer.AddTableFooterOnly();
                writer.AddTableFooterOnly();
                writer.Close();

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                bool success = true;
                output.EndSection("KBDoctor", title, success);
                Functions.AddLineSummary(title + ".txt", "Totals Objects= " + ObjectsTotal.ToString() + " Complexity Index Sum= " + ComplexityIndexTotal.ToString() + " Complexity Index Average= " + Average.ToString());
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        private static int ParametersCountObject(KBObject obj)
        {
            int countparm = 0;
            ICallableObject callableObject = obj as ICallableObject;
            if (callableObject != null)
            {
                foreach (Signature signature in callableObject.GetSignatures())
                {
                    countparm = signature.ParametersCount;
                }
            }
            return countparm;
        }

        
        private static int ParametersTypeCountObject(KBObject obj, string type)
        {
            int countparm = 0;
            ICallableObject callableObject = obj as ICallableObject;
            if (callableObject != null)
            {
                foreach (Signature signature in callableObject.GetSignatures())
                {
                    foreach(Parameter parm in signature.Parameters)
                    {
                        string accessor = parm.Accessor.ToString();
                        if (accessor == type)
                        {
                            countparm++;
                        }
                    }
                }
            }
            return countparm;
        }

        public static void FixVariablesNotBasedInAttributesOrDomain()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Variables not based in domain or attribute";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                output.StartSection("KBDoctor", title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Description", "Type", "# Variables not based in attributes or domain" });

                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                KBModel kbModel = UIServices.KB.CurrentModel;
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WorkPanel>());
                int cantVar = 0;
                IList<KBObject> listObj = UIServices.SelectObjectDialog.SelectObjects(selectObjectOption);

                foreach (KBObject obj in listObj)
                {
                    if (HasVariablesNotBasedOnAttributeOrDomain(obj, out cantVar))
                    {
                        AssignDomToVar f = new AssignDomToVar(obj);
                        f.ShowDialog();
                        f.Close();
                    }

                }

                foreach (KBObject obj in listObj)
                {
                    if (HasVariablesNotBasedOnAttributeOrDomain(obj, out cantVar))
                    {
                        writer.AddTableData(new string[] { Functions.linkObject(obj), obj.Description, obj.TypeDescriptor.Name, cantVar.ToString() });
                    }
                }
                writer.AddTableFooterOnly();
                writer.AddTableFooterOnly();

                writer.AddTableFooterOnly();
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

        private static bool HasVariablesNotBasedOnAttributeOrDomain(KBObject obj, out int cantVar)
        {
            bool tieneVarSinDomain = false;
            cantVar = 0;

            if (isGenerated(obj) && !isGeneratedbyPattern(obj))
            {
                VariablesPart vp = obj.Parts.Get<VariablesPart>();
                if (vp != null)
                {
                    foreach (Variable v in vp.Variables)
                    {
                        if ((!v.IsStandard) && (v.AttributeBasedOn == null) && (v.DomainBasedOn == null) && (v.Type != eDBType.GX_USRDEFTYP)
                            && (v.Type != eDBType.GX_SDT) && (v.Type != eDBType.GX_EXTERNAL_OBJECT) && (v.Type != eDBType.Boolean) && v.Type != eDBType.GX_BUSCOMP && v.Type != eDBType.GX_BUSCOMP_LEVEL && v.Type != eDBType.BITMAP)
                        {
                            tieneVarSinDomain = true;
                            cantVar += 1;                
                        }
                    }
                }
            }
            return tieneVarSinDomain;
        }



        public static void ObjectsUDPCallables()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - UDP CALLABLE";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                string callers = "";

                output.StartSection("KBDoctor", title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Description", "Type", "Folder", "SaveDate", "Param", "Referenced with Call" });


                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                KBModel kbModel = UIServices.KB.CurrentModel;
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WorkPanel>());
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());

                foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
                {
                    KBDoctorOutput.Message( obj.Name);
                    callers = ChangeUDPCallWhenNecesary(obj);

                    if (callers != "")
                    {
                        string Parms = Functions.ExtractRuleParm(obj);
                        writer.AddTableData(new string[] { Functions.linkObject(obj), obj.Description, obj.TypeDescriptor.Name, " " + obj.Parent.Name, " " + obj.VersionDate.ToShortDateString(), Parms, callers });
                    }

                }
                writer.AddTableFooterOnly();
                writer.AddTableFooterOnly();

                writer.AddTableFooterOnly();
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

        static string ChangeUDPCallWhenNecesary(KBObject obj)
        {
            string callers = "";
            if (isGenerated(obj) && !isGeneratedbyPattern(obj))
            {
                foreach (EntityReference reference in obj.GetReferences(LinkType.UsedObject))
                {
                    KBObject objRef = KBObject.Get(obj.Model, reference.To);
                    if (OnlyLastParameterIsOut(objRef))
                    {
                        if (ReplaceCallForUdpInObject(obj, objRef.Name))
                        {
                            callers += " " + Functions.linkObject(objRef);
                        }
                    }
                }
            }

            return callers;
        }

        private static string ReplaceCallForUDP(KBObject obj)
        {
            IOutputService output = CommonServices.Output;

            string callers = "";

            foreach (EntityReference reference in obj.GetReferencesTo(LinkType.UsedObject))
            {

                KBObject objRef = KBObject.Get(obj.Model, reference.From);

                // KBDoctorOutput.Message(objRef.Name + "->" + obj.Name);

                string source = Functions.ObjectSourceUpper(objRef);
                source = Functions.RemoveEmptyLines(source);

                string sourceWOComments = Functions.ExtractComments(source);
                sourceWOComments = sourceWOComments.Replace("\t", "");
                sourceWOComments = sourceWOComments.Replace(" ", "");
                // sourceWOComments = sourceWOComments.Replace(".CALL", "..");

                string callAux = "CALL(" + obj.Name.ToUpper();

                if (sourceWOComments.Contains(callAux))
                {

                    KBDoctorOutput.Message( objRef.Name + " ---> " + obj.Name);
                    callers += " " + Functions.linkObject(objRef);

                    ReplaceCallForUdpInObject(objRef, obj.Name);


                }
            }
            return callers;
        }

        private static bool ReplaceCallForUdpInObject(KBObject obj, string name)
        {
            IOutputService output = CommonServices.Output;
            string source = ObjectsHelper.ObjectSource(obj);

            string pattern = String.Format(@"(\s+)?(call\([\s\b]*({0})([\s\b]*)(,(.*)(,[\s\b]*)(&([a-z0-9\-]+))|,(.*)(&([a-z0-9\-]+)))[\s\b]*\)|(({0})(.call) ?\(((.*)(,[\s\b]*)(&([a-z0-9\-]+))|(.*)(&([a-z0-9\-] +)))[\s\b]*\)))", name);
            Regex regExpCall = new Regex(pattern, RegexOptions.IgnoreCase);

            string sourcemodified = regExpCall.Replace(source, "$1$8$11$19$22 = $3$14.udp($6$17)");
            bool modified = false;
            if (source != sourcemodified)
            {
                SaveNewSource(obj, sourcemodified);
                KBDoctorOutput.Message( "=====================================");
                KBDoctorOutput.Message( "Modified .. " + obj.Name);
                //KBDoctorOutput.Message(sourcemodified);
                modified = true;
            }
            return modified;
        }

        private static bool OnlyLastParameterIsOut(KBObject obj)
        {
            bool udpCallable = false;

            ICallableObject callableObject = obj as ICallableObject;
            if (callableObject != null)
            {
                udpCallable = true;
                foreach (Signature signature in callableObject.GetSignatures())
                {

                    int countparm = signature.ParametersCount;

                    Parameter[] p = signature.Parameters.ToArray<Parameter>();

                    string ss = p[countparm - 1].Accessor.ToString();
                    if (ss != "PARM_OUT")  //ULTIMO PARAMETRO no ES OUT
                    {
                        udpCallable = false;
                    }
                    else
                    {
                        for (int i = 0; i < countparm - 1; i++)
                        {
                            string ss2 = p[i].Accessor.ToString();
                            if (ss2 != "PARM_IN")
                            {
                                udpCallable = false;
                            }

                        }
                    }
                }
            }
            else
            {
                udpCallable = false;
            }
            return udpCallable;
        }

        private static string CompareCallParameters(KBObject obj)
        {
            IOutputService output = CommonServices.Output;
            string source = ObjectSource(obj);
            source = Functions.RemoveEmptyLines(source);
            string sourceWOComments = Functions.ExtractComments(source);

            string lista = "";
            //     KBDoctorOutput.Message("");
            //     KBDoctorOutput.Message("Processing : " + obj.Name);

            foreach (EntityReference reference in obj.GetReferences())
            {
                KBObject objRef = KBObject.Get(obj.Model, reference.To);


                //    List<string> Spec = new List<string>();
                if (IsCallalable(objRef))
                {
                    //           KBDoctorOutput.Message("     Calling : " + objRef.Name);
                    StringCollection interfazCalledObject = InspectCall(objRef);

                    using (StringReader reader = new StringReader(sourceWOComments))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {

                            if (line.Contains(objRef.Name))
                            {
                                line = line.Replace("\t", " ");
                                line = line.Replace(" ", "");
                                line.Replace(".call(", "(", StringComparison.CurrentCultureIgnoreCase);
                                line.Replace(".udp(", "(", StringComparison.CurrentCultureIgnoreCase);
                                line.Replace("call(", "(", StringComparison.CurrentCultureIgnoreCase);
                                line.Replace("udp(", "(", StringComparison.CurrentCultureIgnoreCase);
                                line.Replace(objRef.Name, "Ñ", StringComparison.CurrentCultureIgnoreCase);
                                //                     KBDoctorOutput.Message("............ Line . " + line);

                                StringCollection interfazCallerObject = ProcessingObjectCall(obj, line);
                                for (int i = 0; i < interfazCallerObject.Count; i++)
                                {
                                    if (interfazCallerObject[i] != interfazCalledObject[i])
                                        KBDoctorOutput.Message( "Diferencia: " + line + " Parametro: " + i.ToString() + " - " + interfazCallerObject[i] + "-" + interfazCalledObject[i]);
                                }
                            }
                        }
                    }

                }
            }


            return lista;
        }

        private static StringCollection ProcessingObjectCall(KBObject obj, string line)
        {
            IOutputService output = CommonServices.Output;
            StringCollection lista = new StringCollection();

            if (line.Contains("="))
            {

                String[] partes = line.Split('=');
                line = partes[1] + partes[0];

            }
            Char[] sep = { '=', ',', ')', '(' };
            String[] substrings = line.Split(sep);
            foreach (var substring in substrings)
            {

                if (substring != "")
                {
                    // KBDoctorOutput.Message("................ Substring ." + substring);
                    if (substring.StartsWith("&"))
                    {
                        lista.Add(TypeOfVariable(substring.Replace("&", ""), obj));
                        //KBDoctorOutput.Message("_________________________" + TypeOfVariable(substring.Replace("&", ""), obj));
                    }
                    else
                    {
                        IKBService kbserv = UIServices.KB;
                        //    +TypeDescriptor  { Attribute(adbb33c9 - 0906 - 4971 - 833c - 998de27e0676)}
                        //    Artech.Architecture.Common.Descriptors.KBObjectDescriptor

                        foreach (KBObject att in kbserv.CurrentModel.Objects.GetByName("Attributes", new Guid("adbb33c9-0906-4971-833c-998de27e0676"), substring))
                        {
                            if ((att != null) && (att is Artech.Genexus.Common.Objects.Attribute))
                            {
                                string type = (TypeOfAttribute((Artech.Genexus.Common.Objects.Attribute)att));
                                lista.Add(type);
                                //KBDoctorOutput.Message("__________x______________" + TypeOfAttribute((Artech.Genexus.Common.Objects.Attribute)att));
                                if (type == "")
                                {
                                    KBDoctorOutput.Message( "__________no se pudo______________" + substring);
                                }
                            }
                        }

                    }

                }
            }
            // }
            return lista;


        }

        private static StringCollection InspectCall(KBObject objRef)
        {
            IOutputService output = CommonServices.Output;
            StringCollection lista = new StringCollection();
            ICallableObject callableObject = objRef as ICallableObject;
            if (callableObject != null)
            {
                foreach (Signature signature in callableObject.GetSignatures())
                {
                    foreach (Parameter parm in signature.Parameters)
                    {
                        string p = TypeOfParm(parm, objRef);

                        lista.Add(p);
                        // KBDoctorOutput.Message("        parameter : " + parm.Name + " - " + p);
                    }
                }
            }
            return lista;
        }

        private static string TypeOfParm(Parameter parm, KBObject objRef)
        {
            string typeOfParm = "";
            if (parm.IsAttribute)
            {
                Artech.Genexus.Common.Objects.Attribute a = (Artech.Genexus.Common.Objects.Attribute)parm.Object;
                typeOfParm = TypeOfAttribute(a);
            }
            else
            {
                typeOfParm = TypeOfVariable(parm.Name, objRef);
            }
            return typeOfParm;
        }

        private static string TypeOfAttribute(Artech.Genexus.Common.Objects.Attribute a)
        {
            string typeOfParm;
            typeOfParm = "/" + Utility.FormattedTypeAttribute(a);
            if (a.DomainBasedOn != null)
                typeOfParm += "/" + a.DomainBasedOn.Name + "//";
            return typeOfParm;
        }

        private static string TypeOfVariable(string varname, KBObject objRef)
        {
            string typeOfParm = "";
            VariablesPart vp = objRef.Parts.Get<VariablesPart>();
            if (vp != null)
            {
                foreach (Variable v in vp.Variables)
                {
                    if (v.Name == varname)
                    {
                        typeOfParm = "/" + Utility.FormattedTypeVariable(v);
                        if (v.DomainBasedOn != null)
                            typeOfParm += "/" + v.DomainBasedOn.Name + "//";
                    }
                }

            }

            return typeOfParm;
        }

        private static int CalculateComplexityIndex(int MaxCodeBlock, int MaxNestLevel, int ComplexityLevel, string ParmINOUT)
        {
            int ComplexityIndex = 0;
            if (ParmINOUT == "Error") ComplexityIndex += 100;
            ComplexityIndex += MaxNestLevel * MaxNestLevel;
            ComplexityIndex += ComplexityLevel * 10;
            ComplexityIndex += MaxCodeBlock * 2;
            return ComplexityIndex;
        }

        private static void CountCommentsLines(string source, string sourceWOComments, out int linesSource, out int linesComment, out float PercentComment)
        {
            linesSource = Functions.LineCount(source);
            int linesWOComment = Functions.LineCount(sourceWOComments);

            linesComment = linesSource - linesWOComment;
            PercentComment = (linesSource == 0) ? 0 : (linesComment * 100) / linesSource;
        }

        public static string ObjectSource(KBObject obj)
        {
            string source = "";

            if (obj is Procedure) source = obj.Parts.Get<ProcedurePart>().Source;

            if (obj is Transaction) source = obj.Parts.Get<EventsPart>().Source;

            if (obj is WorkPanel) source = obj.Parts.Get<EventsPart>().Source;

            if (obj is WebPanel) source = obj.Parts.Get<EventsPart>().Source;

            return source;
        }

        private static void ParseSource(string source, out int MaxCodeBlock, out int MaxNestLevel, out int ComplexityLevel)
        {
            string input = source.ToUpper();
            int NestLevel = 0;

            ComplexityLevel = 0;

            using (StringReader reader = new StringReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {

                    line = line.TrimStart().ToUpper();
                    if (line.StartsWith("DO WHILE") || line.StartsWith("IF") || line.StartsWith("DO CASE") || line.StartsWith("FOR"))
                    {
                        ComplexityLevel += 1;
                    }
                }
            }

            MaxNestLevel = 0;
            NestLevel = 0;
            using (StringReader reader = new StringReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {

                    line = line.TrimStart().ToUpper();
                    if (line.StartsWith("DO '"))
                    {
                        //Si es un llamado a una subrutina, no hago nada y lo salteo. 
                    }
                    else
                    {

                        if (line.StartsWith("FOR ") || line.StartsWith("IF ") || line.StartsWith("DO ") || line.StartsWith("NEW") || line.StartsWith("SUB"))
                        {
                            NestLevel += 1;
                            MaxNestLevel = (NestLevel > MaxNestLevel) ? NestLevel : MaxNestLevel;
                        }
                        else
                            if (line.StartsWith("ENDFOR") || line.StartsWith("ENDIF") || line.StartsWith("ENDDO") || line.StartsWith("ENDCASE") || line.StartsWith("ENDNEW") || line.StartsWith("ENDSUB"))
                        {
                            NestLevel -= 1;
                        }
                    }
                }
            }
            if (NestLevel != 0)
            {
                Console.WriteLine(NestLevel.ToString());
            }

            MaxCodeBlock = 0;
            int countLine = 0;
            using (StringReader reader = new StringReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //  line = line.TrimStart().ToUpper();
                    countLine += 1;

                    if (line.StartsWith("SUB ") || line.StartsWith("EVENT "))
                    {
                        MaxCodeBlock = (MaxCodeBlock <= countLine) ? countLine : MaxCodeBlock;
                        countLine = 1;
                    }

                }
                MaxCodeBlock = (MaxCodeBlock <= countLine) ? countLine : MaxCodeBlock;
            }
        }

        public static void ListProcedureCallWebPanelTransaction()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - List Procedure that call Webpanel or Transaction";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                output.StartSection("KBDoctor", title);


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
                                  
/*
                    foreach (KBObject objRef in reachablesObjects)
                    {
                      
                        if (objRef is Transaction || objRef is WebPanel)

                            KBDoctorOutput.Message( obj.TypeName + " " + obj.Name + " => " + objRef.Name + " " + objRef.TypeName );
                            
                        
                    } */
                }

                writer.AddFooter();
                writer.Close();

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                bool success = true;
                output.EndSection("KBDoctor", title, success);
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
                output.StartSection("KBDoctor", title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Type", "Name", "Variable", "Picture", "Attribute", "Domain" });


                //All useful objects are added to a collection
                foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
                {


                    Boolean SaveObj = false;
                    if (isGenerated(obj) && (/*obj is Transaction || obj is WebPanel || obj is WorkPanel ||*/ obj is Procedure))
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
                output.EndSection("KBDoctor", title, success);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

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
                        variables += v.Name + " " + v.Type.ToString().ToLower() + "(" + v.Length.ToString() + ")<br>" + Environment.NewLine;
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

        public static void IndexWithNotRefAtt()
        {
            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - Index with not referenced attributes";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                IOutputService output = CommonServices.Output;
                output.StartSection("KBDoctor", title);

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
                            remove = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;RemoveIndexAttribute&mode=1&indId=" + ind.Id + "&attId=" + obj.Id + "\">Remove Index</a>";
                            remAtt = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;RemoveIndexAttribute&mode=2&indId=" + ind.Id + "&attId=" + obj.Id + "\">Remove attribute from the index</a>";
                            remAttRigth = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;RemoveIndexAttribute&mode=3&indId=" + ind.Id + "&attId=" + obj.Id + "\">Remove attribute and next attributes from the index</a>";
                            writer.AddTableData(new string[] { ind.Table.Name, ind.Name, obj.Name, composition, remove, remAtt, remAttRigth });
                        }
                    }
                    indexes.RemoveAll(indexatt);
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
                foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
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
                        //   i.Save();
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

            output.StartSection("KBDoctor", title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Type", "Name", "Clean" });

                foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
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
                            remove = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;CleanVarsNotUsed&ObjName=" + obj.Name + "&varid=" + varsNotUsed + "\">Clean vars not used</a>";
                            writer.AddTableData(new string[] { obj.TypeDescriptor.Name, obj.Name, remove });
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
            output.StartSection("KBDoctor", title);

            KBDoctorCore.Sources.API.CleanAllKBObjectVariables(UIServices.KB.CurrentKB, output);

            output.EndSection("KBDoctor", title, true);

        }

        private static void SetDocumentDirty(IGxDocument doc)
        {
            if (UIServices.Environment.InvokeRequired) // devuelve true cuando el thread que está ejecutando no es el thread de UI
                UIServices.Environment.BeginInvoke(() => SetDocumentDirty(doc)); // dispara un invoke asincrónico a SetDocumentDirty
            else
                doc.Dirty = true;
        }

        public static bool IsCallalable(KBObject obj)
        {
            return ((obj is Transaction) || (obj is Procedure) || (obj is WebPanel) || (obj is WorkPanel) || (obj is DataProvider) || (obj is Menubar) || (obj is DataSelector));
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


            output.StartSection("KBDoctor", title);
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
                output.EndSection("KBDoctor", title, success);
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
            output.StartSection("KBDoctor", title);
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

                // selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Module>());
                foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
                {

                    if (KBObjectHelper.IsSpecifiable(obj))
                    {
                        //        lista += obj.Name + ";";
                        //output.Add(obj.QualifiedName.ObjectName.ToString() + ";");

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
                output.EndSection("KBDoctor", title, true);
            }
            catch
            {
                success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }

        }

        public static void BuildObjectWithProperty()
        {

            using (Form form = new ResponsiveSmooth())
            {
                form.ShowDialog();
                return;
            }
            /*
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;

            bool success = true;
            string title = "KBDoctor - Objects with property";
            output.StartSection("KBDoctor", title);

            //string propertyString = Properties.WBP.WebUserExperience ;
            //string propertyValue = Properties.WBP.WebUserExperience_Values.Smooth;
            try
            {
               
          
                KBDoctorOutput.Message( "Type, Name, Web User Exp, MasterPage, Theme, Web Form Defaults, FormisDefault ");
                //foreach (KBObject obj in kbModel.Objects.GetByPropertyValue(propertyString, propertyValue) )
                /*  foreach (KBObject obj in Transaction.GetAll(kbModel))

                   {
                       ListaObjectProperties(obj);
                   }
                */

            /*
                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
             //   List<KBObject> objects = new List<KBObject>();
             //   HashSet<EntityKey> guids = new HashSet<EntityKey>();
                foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
                {
                    ListaObjectProperties(obj);
                }
             


                /*
                KBDoctorOutput.Message("===== External Objects = WSDL ========");
                foreach (KBObject obj in ExternalObject.GetAll(kbModel)) //.GetByPropertyValue("USE_ENCRYPTION", "SITE"))
                {
                    if (obj.GetPropertyValueString("ExoType") == "WSDL")
                    {
                        KBDoctorOutput.Message(obj.Name);

                        EXOStructurePart EOPart = obj.Parts.Get<EXOStructurePart>();
                        foreach (ExternalObjectMethod eom in EOPart.ExternalMethods)
                        {
                            String eomAddress = eom.GetPropertyValueString("ExoMethodAddress");

                            Uri uri = new Uri(eomAddress);
                            // string requested = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;
                            string LocationSecure = (uri.Scheme == "http") ? "0" : "1";
                            string LocationProgram = uri.Segments[uri.Segments.Length - 1];
                            string LocationBaseURL = string.Empty;
                            string LocationID = obj.QualifiedName.ToString().Replace(".", "_");
                            for (int i = 0; i < uri.Segments.Length - 1; i++)
                            {
                                LocationBaseURL += uri.Segments[i];
                            }
                            string LocationNameID = string.Empty;
                            if (obj.Description == "")
                                LocationNameID = LocationID;
                            else
                                LocationNameID = obj.Description;

                            KBDoctorOutput.Message(" ");

                            KBDoctorOutput.Message("Msg('Inserted:  " + LocationID  + "',status)");
                            KBDoctorOutput.Message("new ");
                            KBDoctorOutput.Message("  LocationID=  '" + LocationID +"'");
                            KBDoctorOutput.Message("  LocationNameID='" + LocationNameID + "'");
                            KBDoctorOutput.Message("  LocationHost=  '" + uri.Host + "'") ;
                            KBDoctorOutput.Message("  LocationPort=  " + uri.Port);
                            KBDoctorOutput.Message("  LocationSecure=" + LocationSecure);
                            KBDoctorOutput.Message("  LocationBaseURL='" + LocationBaseURL + "'");
                            KBDoctorOutput.Message("  LocationProgram='" + LocationProgram + "'");
                            KBDoctorOutput.Message("  LocationTimeout = 3600");
                            
                            KBDoctorOutput.Message("      when duplicate Msg('ERROR DUPLICATE: ' + LocationID,status)");
                            KBDoctorOutput.Message("endnew " + Environment.NewLine );
                            break;
                        }

                    }
                }
                
                KBDoctorOutput.Message( "");
                KBDoctorOutput.Message( "===== SOAP ========");
                bool hassdt;
                foreach (KBObject obj in kbModel.Objects.GetByPropertyValue("CALL_PROTOCOL", "SOAP"))
                {
                    hassdt = false;
                    ICallableObject callableObject = obj as ICallableObject;
                    if (callableObject != null)
                    {
                        foreach (Signature signature in callableObject.GetSignatures())
                        {
                            foreach (Parameter parm in signature.Parameters)
                            {

                                if (parm.IsAttribute)
                                {
                                    Artech.Genexus.Common.Objects.Attribute att = (Artech.Genexus.Common.Objects.Attribute)parm.Object;
                                    if (att != null)
                                        if (Utility.FormattedTypeAttribute(att).Contains("GX_SDT")){
                                            hassdt = true;
                                        }
                                }
                                else
                                {
                                    Variable var = (Variable)parm.Object;
                                    if (var != null)
                                        if (Utility.FormattedTypeVariable(var).Contains("GX_SDT"))
                                        {
                                            hassdt = true;
                                        }
                                }
                            }
                        }
                    }
                    if (hassdt)
                    {
                        KBDoctorOutput.Message("SOAP:" + obj.Name + " (SDT in Parms)");
                    }
                    else
                    {
                        KBDoctorOutput.Message("SOAP:" + obj.Name);
                    }
                    
                }
                
                KBDoctorOutput.Message( "");
                KBDoctorOutput.Message( "===== HTTP ========");
                foreach (KBObject obj in kbModel.Objects.GetByPropertyValue("CALL_PROTOCOL", "HTTP"))
                {
                    KBDoctorOutput.Message( obj.Name);
                }
                /*
                KBDoctorOutput.Message( "");
                KBDoctorOutput.Message( "===== SOAP NAMESPACE IN PROCEDURE ========");
                foreach (Procedure p in Procedure.GetAll(kbModel))
                {
                    string propval = p.GetPropertyValueString("SOAP_NAMESPACE");
                    Artech.Common.Properties.Property prop =  p.GetProperty("SOAP_NAMESPACE");
                    
                    if (propval != "" && !prop.IsDefault)
                    {
                        KBDoctorOutput.Message( p.Name + " Namespace:" + propval );
                    }

                }

                KBDoctorOutput.Message( "");
                KBDoctorOutput.Message( "===== xml NAMESPACE IN SDT ========");
                foreach (SDT sdt in SDT.GetAll(kbModel))
                {
                    ListSdtNamespace(sdt.SDTStructure.Root, sdt.Name);

                }*/
            /*
            }
            catch (Exception e)
            {
                success = false;
                output.AddErrorLine(e.Message + " " +e.InnerException);
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }

            void ListaObjectProperties( KBObject obj)
            {
                if (isGenerated(obj) )
                {


                    WebFormPart webForm = obj.Parts.Get<WebFormPart>();
                    KBDoctorOutput.Message(obj.Name + "," + obj.GetPropertyValueString("WebUX") + "," + obj.GetPropertyValueString("MasterPage") + "," + obj.GetPropertyValueString("Theme")
                        + "," + obj.GetPropertyValueString("WebFormDefaults") + ","+ isGeneratedbyPattern(obj) + "," + webForm.IsDefault);



                    Theme theme = Theme.Get(kbModel, "LuciaTheme_v4");
                    obj.SetPropertyValue(Properties.WBP.Theme, new ThemeWebReference(theme));

                    QualifiedName qname = new QualifiedName("", "LuciaMasterPage_v4");
                    WebPanel masterPage = WebPanel.Get(kbModel, qname);

                    obj.SetPropertyValue(Properties.WBP.MasterPage, new WebPanelReference(masterPage) );


                    obj.SetPropertyValue(Properties.TRN.WebUserExperience, Properties.TRN.WebUserExperience_Values.Smooth);
                    obj.SetPropertyValue(Properties.TRN.WebFormDefaults, Properties.TRN.WebFormDefaults_Enum.ResponsiveWebDesign);
                    obj.Save();


                    
                   

                }

            }*/
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
                output.StartSection("KBDoctor", title);

                string  sw2 = "";
                SortedDictionary<string, string> sw3 = new SortedDictionary<string, string>();
                //     KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                //     writer.AddHeader(title);
                int numObj = 0;

                //writer.AddTableHeader(new string[] { "Type", "Object", "Module", "Parm" });

                foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
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

                        if (obj is ExternalObject || obj is SDT)
                            tieneInterfaz = true;


                        //  string ruleparm = Functions.ExtractRuleParm(obj);
                        //  ruleparm = Regex.Replace(ruleparm, @"\t|\n|\r", "");
                        // ruleparm = ruleparm.Replace(" ", "");
                        string callprotocol = obj.GetPropertyValueString("CALL_PROTOCOL");
                        if (callprotocol == "")
                            callprotocol = "Internal";

                        if (tieneInterfaz)
                        {
                            // if (obj is Procedure && isMain)

                            //   writer.AddTableData(new string[] { obj.TypeDescriptor.Name + " ", Functions.linkObject(obj), obj.Module.Name, ruleparm });
                            //sw += obj.TypeDescriptor.Name + "\t" + obj.QualifiedName + "\t" + callprotocol  + "\t"  + ruleparm + "\r\n";
                            //      sw2 += callprotocol+ "\t" + obj.Name + "\t" + qualifiedName + "\r\n";
                            sw3[callprotocol + "\t" + obj.Name] = qualifiedName;
                        }
                        numObj += 1;
                        if ((numObj % 100) == 0)
                            KBDoctorOutput.Message( obj.TypeDescriptor.Name + "," + obj.Name + "," + obj.Description); //+ "," + obj.Timestamp.ToString());
                    }
                }
                //     writer.AddFooter();
                //     writer.Close();

                //KBDoctorHelper.ShowKBDoctorResults(outputFile);
                bool success = true;


                string directoryArg = KBDoctorHelper.SpcDirectory(kbserv);
                string fechahora = String.Format("{0:yyyy-MM-dd-HHmm}", DateTime.Now);

                // string fileName = directoryArg + @"\API-" + fechahora + ".txt";
                // System.IO.File.WriteAllText(fileName, sw);



                foreach (KeyValuePair<string, string> entry in sw3)
                {
                    sw2 += entry.Value + "\r\n";
                }


                string fileName2 = directoryArg + @"\API3-" + fechahora + ".txt";
                System.IO.File.WriteAllText(fileName2, sw2);
                KBDoctorOutput.Message( "URL/URI file generated in " + fileName2);
                output.EndSection("KBDoctor", title, success);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public static void ObjectsUpdatingAttributes()
        {
            IOutputService output = CommonServices.Output;
            output.SelectOutput("KBDoctor");

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = false;
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = kbserv.CurrentModel;

            string title = "KBDoctor - Objects updating attribute";
            output.StartSection("KBDoctor", title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);

                writer.AddTableHeader(new string[] { "Table", "Transactions", "Objects updating table", "Objects updating attribute" });

                AskAttributeandTable at = new AskAttributeandTable();
                DialogResult dr = new DialogResult();
                dr = at.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    string tblName = at.tblName;
                    string attName = at.attName;

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
                                // KBDoctorOutput.Message(obj.Name);
                            }

                            writer.AddTableData(new string[] { Functions.linkObject(t), trnstring, updatetablestring, updateattstring });
                            writer.AddFooter();
                            writer.Close();

                            KBDoctorHelper.ShowKBDoctorResults(outputFile);


                            //output.EndSection("KBDoctor", title, success);
                        }

                    }
                    else
                    {
                        bool success = false;
                        output.EndSection("KBDoctor", title, success);
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
        public static void ObjectsWithTheSameSignature()
        {
            // Object with parm() rule without in: out: or inout:
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;

            string title = "KBDoctor - Objects with the same signature";
            output.StartSection("KBDoctor", title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);

                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());
                IEnumerable<KBObject> objects = kbserv.CurrentModel.Objects.GetAll();

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
                output.EndSection("KBDoctor", title, success);

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public static void ObjectsWithTheSameSignatureAssociated()
        {
            // Object with parm() rule without in: out: or inout:
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;

            string title = "KBDoctor - Objects with the same signature associated to a transaction";
            output.StartSection("KBDoctor", title);
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
                output.EndSection("KBDoctor", title, success);

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
            List<KBObject> objs = (List<KBObject>) UIServices.SelectObjectDialog.SelectObjects(selectObjectOption);
            output.SelectOutput("KBDoctor");
            output.StartSection("KBDoctor - Assign Types Comparer");
            string recommendations = "";
            int cant;
            Thread thread = new Thread(() => API.AssignTypesComprarer(kbserv.CurrentKB, objs, ref recommendations, out cant));
            thread.Start();
        }

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
            token_meaning.Add(TokensIds.TKN_NONE, " NONE, para expresión FOR EACH ... ORDER NONE ... ENDFOR");
            token_meaning.Add(TokensIds.PRM, " Parámetro, utilizado en DYNQ");
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
            token_meaning.Add(TokensIds.DTAPL, " Asignación del tipo +=");
            token_meaning.Add(TokensIds.DTAMI, " Asignación del tipo -=");
            token_meaning.Add(TokensIds.DTAMU, " Asignación del tipo *=");
            token_meaning.Add(TokensIds.DTADI, " Asignación del tipo /=");
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
            output.StartSection("KBDoctor", title);
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


                foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
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

                        /*
                        foreach (TokenData token in parser.GetTokens(true, parserInfo, source.Source))
                        {
                                string meaning = token_meaning[(TokensIds)token.Token] as string;
                            if (token.Token >= 100)
                            {
                                writer.AddTableData(new string[] { "", meaning + "(" + token.Word + ")", "", token.Token.ToString() });
                            }
                            else
                            {
                                writer.AddTableData(new string[] { "", "", meaning + "(" + token.Word + ")", token.Token.ToString() });
                            }
                        }*/
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


        
    }
}

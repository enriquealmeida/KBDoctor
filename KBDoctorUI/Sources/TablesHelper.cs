using Artech.Architecture.Common;
using Artech.Architecture.Common.Collections;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Services;
using Artech.Udm.Framework.References;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    static class TablesHelper

    {
        public static void ListTables()
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = UIServices.KB.CurrentModel;
            string title = "KBDoctor - Tables";
            string outputFile = Functions.CreateOutputFile(kbserv, title);


            IOutputService output = CommonServices.Output;
            output.StartSection(title);


           KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] {
                "Name", "Description","Module","is Public", "#Key", "Key Width", "Width Variable", "Width Fixed", "Width Total" , "Cache Level"
            });

            string description;
            foreach (Table t in Table.GetAll(kbserv.CurrentModel))
            {
                description = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;AssignDescriptionToTable&tblName=" + t.Description + "\">" + t.Description + "</a>";
                string objNameLink = Functions.linkObject(t);

                output.AddLine("Processing... " + t.Name);

                int countAttr = 0;
                int countKeyAttr = 0;
                int widthKey = 0;
                int width = 0;
                int widthVariable = 0;
                int widthFixed = 0;
                foreach (TableAttribute attr in t.TableStructure.Attributes)
                {
                    countAttr += 1;
                    if (attr.IsKey)
                    {
                        countKeyAttr += 1;
                        widthKey += attr.Attribute.Length;
                    }
                    width += attr.Attribute.Length;
                    if ((attr.Attribute.Type == Artech.Genexus.Common.eDBType.LONGVARCHAR) || (attr.Attribute.Type == Artech.Genexus.Common.eDBType.VARCHAR))
                    {
                        widthVariable += attr.Attribute.Length;
                    }
                    else
                    {
                        widthFixed += attr.Attribute.Length;
                    }
                }

                string CacheLevel = t.GetPropertyValueString("CACHE_LEVEL");
                string isPublic = t.IsPublic ? "Yes" : "";
                writer.AddTableData(new string[] {
                    objNameLink, t.Description, TableModule(model,t).Name, isPublic, countKeyAttr.ToString(), widthKey.ToString(), widthVariable.ToString(), widthFixed.ToString(), width.ToString() , CacheLevel
                });

            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }

        internal static Table TableOfAttribute(Artech.Genexus.Common.Objects.Attribute a)
        {
           foreach (EntityReference refer in a.GetReferencesTo(a.Model.Id))
            {
                KBObject t = KBObject.Get(a.Model, refer.From);
                if (t != null)
                {
                    if (t is Table)
                        return (Table)t;
                }
            }
            return null;
        }

        public static void ListWithoutDescription()
        {
            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - Tables with incomplete description";
            string outputFile = Functions.CreateOutputFile(kbserv, title);


            IOutputService output = CommonServices.Output;
            output.StartSection(title);


           KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] {
                "Name", "Description"
            });

            string description;
            foreach (Table t in Table.GetAll(kbserv.CurrentModel))
            {
                if (t.Name == t.Description.Replace(" ", ""))
                {
                    description = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;AssignDescriptionToTable&tblName=" + t.Name + "\">" + t.Description + "</a>";
                    writer.AddTableData(new string[] {
                        t.Name, description
                    });
                }
            }

            writer.AddFooter();
            writer.Close();

            //UIServices.StartPage.GetToolWindow().OpenPage(outputFile, "KBDoctor");
            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }

        public static void ListGroupWithoutDescription()
        {
            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - Subtypes Group with incomplete description";
            string outputFile = Functions.CreateOutputFile(kbserv, title);


            IOutputService output = CommonServices.Output;
            output.StartSection(title);


           KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] {
                "Name", "Description"
            });


            foreach (Group g in Group.GetAll(kbserv.CurrentModel))
            {
                if (g.Name == g.Description.Replace(" ", "") || (g.Name == ""))
                {
                    string grpLink = Functions.linkObject(g);

                    writer.AddTableData(new string[] {
                        grpLink, g.Description
                    });
                }
            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }

        public static void AssignDescriptionToTable(object[] parameters)
        {
            foreach (object o in parameters)
            {
                Dictionary<string, string> dic = (Dictionary<string, string>)o;
                int cant = 0;

                string tblName = "";
                string mensaje;
                PromptDescription pd;
                DialogResult dr;

                foreach (string s in dic.Values)
                {
                    if (cant == 1)
                    {
                        tblName = s;
                        mensaje = "Insert description for table " + tblName;
                        pd = new PromptDescription(mensaje);
                        dr = pd.ShowDialog();

                        if (dr == DialogResult.OK)
                        {
                            Table t = Table.Get(UIServices.KB.CurrentModel, tblName);
                            t.Description = pd.Description;
                            t.Save();
                        }
                    }

                    cant++;
                }
            }
        }

        public static void ListTablesWidth()
        {
            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - Tables Width";
            string outputFile = Functions.CreateOutputFile(kbserv, title);


            IOutputService output = CommonServices.Output;
            output.StartSection(title);


           KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] {
                "Name", "Description", "#Key", "Key Width", "Width Variable", "Width Fixed", "Width Total" , "Cache Level"
            });

            string description;
            foreach (Table t in Table.GetAll(kbserv.CurrentModel))
            {
                description = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;AssignDescriptionToTable&tblName=" + t.Description + "\">" + t.Description + "</a>";
                string objNameLink = Functions.linkObject(t);

                output.AddLine("Processing... " + t.Name);

                int countAttr = 0;
                int countKeyAttr = 0;
                int widthKey = 0;
                int width = 0;
                int widthVariable = 0;
                int widthFixed = 0;
                foreach (TableAttribute attr in t.TableStructure.Attributes)
                {
                    countAttr += 1;
                    if (attr.IsKey)
                    {
                        countKeyAttr += 1;
                        widthKey += attr.Attribute.Length;
                    }
                    width += attr.Attribute.Length;
                    if ((attr.Attribute.Type == Artech.Genexus.Common.eDBType.LONGVARCHAR) || (attr.Attribute.Type == Artech.Genexus.Common.eDBType.VARCHAR))
                    {
                        widthVariable += attr.Attribute.Length;
                    }
                    else {
                        widthFixed += attr.Attribute.Length;
                    }
                }

                string CacheLevel = t.GetPropertyValueString("CACHE_LEVEL");
                writer.AddTableData(new string[] {
                    objNameLink, t.Description, countKeyAttr.ToString(), widthKey.ToString(), widthVariable.ToString(), widthFixed.ToString(), width.ToString() , CacheLevel
                });

            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }



        public static void ListTableTransaction()
        {
            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - Tables Transaction Relation";
            string outputFile = Functions.CreateOutputFile(kbserv, title);


            StringCollection strCol = new StringCollection();
            IOutputService output = CommonServices.Output;
            output.StartSection(title);

           KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] {
                "Table", "Transactions with GenerateObject=False", " Transactions with GENERATEObject=True","Check"
            });


            foreach (Table tbl in Table.GetAll(kbserv.CurrentModel))
            {
                string tblNamelink = Functions.linkObject((KBObject)tbl);

                string trnGen = "";
                string trnNoGen = "";
                foreach (Transaction trn in tbl.AssociatedTransactions)
                {
                    if (trn.GetPropertyValue<bool>(Properties.TRN.GenerateObject)) trnGen += Functions.linkObject(trn) + " ";
                    else trnNoGen += Functions.linkObject(trn) + " ";
                }

                writer.AddTableData(new string[] {

                    tblNamelink, trnNoGen, trnGen,(trnGen!="" && trnNoGen!="")?"*":""
                });
            }

            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }

        public static void GenterateSimpleTransactionFromNotGeneratedTransaction()
        {
            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - Tables Transaction Relation2";

            StringCollection strCol = new StringCollection();
            IOutputService output = CommonServices.Output;
            output.StartSection(title);

            foreach (Transaction trn in Transaction.GetAll(kbserv.CurrentModel))
            {
                if (!KBDoctorCore.Sources.Utility.isGenerated(trn) && !trn.Name.StartsWith("KBDoctor_"))
                {
                    output.AddLine("");
                    output.AddLine("Processing transaction " + trn.Name);

                    foreach (TransactionLevel lvl in trn.Structure.GetLevels())
                    {
                        Table tbl = lvl.AssociatedTable;
                        GenerateTrnFromTable(kbserv, tbl, trn.Module);
                    }
                    try
                    {
                        trn.Delete();
                        output.AddLine(" Deleted transaction " + trn.Name);
                    }
                    catch (Exception e) { output.AddErrorLine("ERROR: Can't delete transaction " + trn.Name + e.Message); };
                }

            }


            bool success = true;
            output.EndSection(title, success);

        }


        public static void ListTableUpdate()
        {



            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - Table - Update/Delete/Insert/Read";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            StringCollection strCol = new StringCollection();
            IOutputService output = CommonServices.Output;
            output.StartSection(title);


           KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] {
                "Table", "Update", "Delete", "Insert", "Read", "*"
            });

            KBModel model = UIServices.KB.CurrentModel;



            foreach (Table tbl in Table.GetAll(model))
            {

                string tblNamelink = Functions.linkObject((KBObject)tbl);

                string objUpdaters, objDeleters, objInserters, objReaders, str;
                ObjectsReferenceTable(model, tbl, out objUpdaters, out objDeleters, out objInserters, out objReaders, out str);


                writer.AddTableData(new string[] {
                    tblNamelink, objUpdaters, objDeleters, objInserters, objReaders, str
                });

            }


            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }

        public static void ObjectsReferenceTable(KBModel model, Table tbl, out string objUpdaterspar, out string objDeleterspar, out string objInserterspar, out string objReaderspar, out string str)
        {
            string objUpdaters = "";
            string objDeleters = "";
            string objInserters = "";
            string objReaders = "";
            str = "";
            IList<KBObject> updaters = (from r in model.GetReferencesTo(tbl.Key, LinkType.UsedObject)
                                        where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                        where ReferenceTypeInfo.HasUpdateAccess(r.LinkTypeInfo)
                                        select model.Objects.Get(r.From)).ToList();
            updaters.ToList().ForEach(v => objUpdaters += " " + Functions.linkObject(v));

            IList<KBObject> inserters = (from r in model.GetReferencesTo(tbl.Key, LinkType.UsedObject)
                                         where r.ReferenceType == ReferenceType.WeakExternal
                                         where ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo)
                                         select model.Objects.Get(r.From)).ToList();
            inserters.ToList().ForEach(v => objInserters += " " + Functions.linkObject(v));

            IList<KBObject> deleters = (from r in model.GetReferencesTo(tbl.Key, LinkType.UsedObject)
                                        where r.ReferenceType == ReferenceType.WeakExternal
                                        where ReferenceTypeInfo.HasDeleteAccess(r.LinkTypeInfo)
                                        select model.Objects.Get(r.From)).ToList();
            deleters.ToList().ForEach(v => objDeleters += " " + Functions.linkObject(v));

            IList<KBObject> readers = (from r in model.GetReferencesTo(tbl.Key, LinkType.UsedObject)
                                       where r.ReferenceType == ReferenceType.WeakExternal
                                       where ReferenceTypeInfo.HasReadAccess(r.LinkTypeInfo)
                                       select model.Objects.Get(r.From)).ToList();
            readers.ToList().ForEach(v => objReaders += " " + Functions.linkObject(v));

            if (objUpdaters != "" || objDeleters != "" || objInserters != "") str = "";
            else str = " * ";
            objUpdaterspar = objUpdaters;
            objInserterspar = objInserters;
            objReaderspar = objReaders;
            objDeleterspar = objDeleters;
        }
    


        public static IList<KBObject> ObjectsUpdateTableOutsideModule(KBModel model, Table tbl)
        {
            Module mdlTable = TableModule(model, tbl);

            IList<KBObject> updaters = (from r in model.GetReferencesTo(tbl.Key, LinkType.UsedObject)
                                        where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                        where ReferenceTypeInfo.HasUpdateAccess(r.LinkTypeInfo) || ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo) || ReferenceTypeInfo.HasDeleteAccess(r.LinkTypeInfo)
                                        where model.Objects.Get(r.From).Module != mdlTable
                                        select model.Objects.Get(r.From)).ToList();
            return updaters;
            
        }

        public static IList<KBObject> ObjectsReadTableOutsideModule(Table tbl)
        {
            KBModel model = tbl.Model;
            Module mdlTable = TableModule(model, tbl);

            IList<KBObject> readers = (from r in model.GetReferencesTo(tbl.Key, LinkType.UsedObject)
                                        where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                        //where ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo) || ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo) || ReferenceTypeInfo.HasDeleteAccess(r.LinkTypeInfo)
                                        where model.Objects.Get(r.From).Module != mdlTable
                                        select model.Objects.Get(r.From)).ToList();
            return readers;

        }
        public static void ListTableInsertNew()
        {
            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - Table - New - Not instanciated attributed";
            string outputFile = Functions.CreateOutputFile(kbserv, title);



            StringCollection strCol = new StringCollection();
            IOutputService output = CommonServices.Output;
            output.StartSection(title);

           KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] {
                "Table", "Insert", "Att not referenced"
            });

            KBModel model = UIServices.KB.CurrentModel;



            foreach (Table tbl in Table.GetAll(model))
            {

                string objInserters = "";
                string str = "";
                string tblNamelink = Functions.linkObject((KBObject)tbl);

                KBObjectCollection attTable = new KBObjectCollection();

                foreach (Artech.Genexus.Common.Objects.Attribute att in tbl.GetAttributes())
                {
                    Formula formula = att.Formula;
                    if (formula == null)
                    {
                        if (!Functions.AttIsSubtype(att)) attTable.Add(att); //solo agrego si no es formula o subtipo. 
                    }
                }

                IList<KBObject> inserters = (from r in model.GetReferencesTo(tbl.Key, LinkType.UsedObject)
                                             where r.ReferenceType == ReferenceType.WeakExternal
                                             where ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo)
                                             select model.Objects.Get(r.From)).ToList();

                foreach (KBObject prc in inserters)
                {
                    if (prc is Procedure)
                    {
                        objInserters += " " + Functions.linkObject(prc);
                        foreach (EntityReference reference in prc.GetReferences())
                        {
                            KBObject objRef = KBObject.Get(prc.Model, reference.To);
                            if ((objRef != null) && (objRef is Artech.Genexus.Common.Objects.Attribute))
                            {
                                attTable.Remove(objRef);
                            }
                        }

                    }
                }

                if (objInserters != "")
                {
                    attTable.ToList().ForEach(v => str += " " + Functions.linkObject(v));
                    writer.AddTableData(new string[] {
                        tblNamelink, objInserters, str
                    });
                };

            }


            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);

        }

        internal static void GenerateTrnFromTables()
        {

            IKBService kbserv = UIServices.KB;


            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            IOutputService output = CommonServices.Output;
            output.StartSection("Creating Transaction from tables");

            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Table>());
            foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                Module m = TableModule(kbserv.CurrentModel, (Table)obj);
                GenerateTrnFromTable(kbserv, (Table)obj, m);
            }
            output.EndSection("Creating Transaction from tables", true);
        }

        internal static void GenerateTrnFromTables2()
        {

            IKBService kbserv = UIServices.KB;


            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            IOutputService output = CommonServices.Output;
            output.StartSection("Creating Procedure from tables");

            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Table>());
            foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                Module m = TableModule(kbserv.CurrentModel, (Table)obj);
                GenerateInitializdeProcedureFromTable(kbserv, (Table)obj, m);
            }
            output.EndSection("Creating Procedure from tables", true);
        }

        public static Module TableModule(KBModel m, Table t)
        {
            return Artech.Genexus.Common.Services.GenexusBLServices.Tables.GetBestAssociatedTransaction(m, t.Key).Module;
        }

        public static void GenerateTrnFromTable(IKBService kbserv, Table t, Module m)
        {
            IOutputService output = CommonServices.Output;
            Transaction trn = Transaction.Create(kbserv.CurrentModel);
            trn.Name = "KBDoctor_table_" + t.Name;
            trn.Description = t.Description;
            trn.SetPropertyValue(Properties.TRN.GenerateObject, false);
            trn.SetPropertyValue(Properties.TRN.MasterPage, WebPanelReference.NoneRef);
            trn.Module = m;

            foreach (TableAttribute attr in t.TableStructure.Attributes)
            {
                TransactionAttribute trnAtt = trn.Structure.Root.AddAttribute(attr);
                trnAtt.IsKey = attr.IsKey;
            };
            output.AddLine("Create transaction " + trn.Name + " from table " + t.Name);
            try {
                trn.Save();
            } catch (Exception e) { output.AddErrorLine("ERROR: Can't Save. Transaction " + trn.Name + " already exists." + e.Message); }
        }

        public static void GenerateInitializdeProcedureFromTable(IKBService kbserv, Table t, Module m)
        {
            IOutputService output = CommonServices.Output;
            Procedure p = Procedure.Create(kbserv.CurrentModel);
            string source = "";
            p.Name = "KBDoctor_Initialize_" + t.Name;
            p.Description = t.Description;
            p.SetPropertyValue(Properties.PRC.GenerateObject, true);
            p.Module = m;

            VariablesPart vp = p.Parts.Get<VariablesPart>();
            Variable v = new Variable(vp);
            v.Name = "comilla";
            v.Type = eDBType.CHARACTER;
            v.Length = 1;

            p.Variables.Add(v);

            Variable ret = new Variable(vp);
            ret.Name = "ret";
            ret.Type = eDBType.NUMERIC;
            ret.Length = 2;
            p.Variables.Add(ret);
            
            string outputfilename = kbserv.CurrentKB.UserDirectory + @"\" + t.Name + ".kbd";

            source = "&ret = DFWOpen('" + outputfilename + "','','')" + Environment.NewLine;
            source += String.Format("&ret= DFWPTxt('//Initialize {0} ')", t.Name)  + Environment.NewLine;
            source += String.Format("&ret= DFWNext()") + Environment.NewLine;
            source += "&comilla='\"'" + Environment.NewLine;
            source += "for each " + Environment.NewLine;
            source += "&ret = DFWPTxt('    new ')" + Environment.NewLine;
            source += String.Format("   &ret= DFWNext()") + Environment.NewLine;
            foreach (TableAttribute attr in t.TableStructure.Attributes)
            {
                string comillaini = "";
                string comillafin = "";
                if (attr.Attribute.Type == eDBType.CHARACTER || attr.Attribute.Type == eDBType.VARCHAR || attr.Attribute.Type == eDBType.LONGVARCHAR)
                {
                    comillaini = " + &comilla + ";
                    comillafin = " + &comilla ";
                }
                else
                {
                    comillaini = " + ";
                    comillafin = "";
                }
                source += String.Format("   &ret = DFWPTxt('        {0} =' {1}{0}.ToFormattedString(){2})  ",attr.Name,comillaini,comillafin) + Environment.NewLine;
                source += String.Format("   &ret = DFWNext()") + Environment.NewLine;
            }
            source += "&Ret = DFWPTxt('    endnew ')" + Environment.NewLine + Environment.NewLine;
            source += String.Format("&ret= DFWNext()") + Environment.NewLine;

            source += "endfor " + Environment.NewLine;
            source += "&ret = DFWClose()" + Environment.NewLine;
            source += "Msg('Generated in : "+outputfilename + "',status)";

            p.SetPropertyValue("IsMain", true);
            p.SetPropertyValue("CALL_PROTOCOL", "CLINE");

            p.ProcedurePart.Source = source;


            try
            {
                p.Save();
            }
            catch (Exception e) { output.AddErrorLine("ERROR: Can't Save. " + p.Name + Environment.NewLine + e.Message); }
            
        }
    
  

        public static void GenerateSQLScripts()
        {
            //Genera scripts que ayudan al manejo de nulos en la KB. 
            //  UpdateNullValues.sql - Update en la base para sacar los valores nulos
            //  CheckPKGXNullValues.sql - Lista las primary key con el null GX  (Error)
            //  CheckFKGXNullValues.sql - Lista todas los atributos que participan en una FK que tengan el nulo de GeneXus.
            //  CheckReferentialIntegrity.sql  - Lista todos los atributos que violan la integridad referencial

            IKBService kbserv = UIServices.KB;
            string title = "KBDoctor - Scripts to check/correct data";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            KBModel model = kbserv.CurrentKB.DesignModel;
            int ATTNAME_LEN = model.GetPropertyValue<int>("ATTNAME_LEN");
            int TBLNAME_LEN = model.GetPropertyValue<int>("TBLNAME_LEN");
            int OBJNAME_LEN = model.GetPropertyValue<int>("OBJNAME_LEN");


            IOutputService output = CommonServices.Output;
            output.StartSection(title);

           KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Check", "File" });

            string Check = "Check DB Structure";
            string Name = Functions.CleanFileName(Check);
            string FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".sql";
            
            GenerateSciptCheckDBStructure(Name, FileName, ATTNAME_LEN,TBLNAME_LEN);
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });
            
            Check = "Update Null Values";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".sql";
            GenerateSciptUpdateNullValues(Name, FileName, ATTNAME_LEN, TBLNAME_LEN); ;
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });

            Check = "Check PK Empty Values";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".sql";
            GenerateSciptCheckPKEmptyValues(Name, FileName, ATTNAME_LEN, TBLNAME_LEN);
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });

            Check = "Check FK Empty Values";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".sql";
            GenerateSciptCheckFKEmptyValues(Name, FileName, ATTNAME_LEN, TBLNAME_LEN);
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });

            Check = "Count Referential Integrity Problems";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".sql";
            GenerateCountReferentialIntegrityProblems(Name, FileName, ATTNAME_LEN, TBLNAME_LEN);
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });

            Check = "Check Referential Integrity";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".sql";
            GenerateCheckReferentialIntegrity(Name, FileName, ATTNAME_LEN, TBLNAME_LEN);
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });

            Check = "Delete Invalid Integrity Values";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".sql";
            GenerateDeleteInvalidIntegrityValues(Name, FileName, ATTNAME_LEN, TBLNAME_LEN);
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });

            Check = "Copy test data from databases";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".sql";
            GenerateCopyTestData(Name, FileName, ATTNAME_LEN, TBLNAME_LEN);
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });
            
            /*
            Check = "Check Referential Integrity Numeric3";
            Name = Functions.CleanFileName(Check);
            FileName = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Name + ".sql";
            GenerateCheckReferentialIntegrityN3(Name, FileName, ATTNAME_LEN, TBLNAME_LEN);
            writer.AddTableData(new string[] { Check, Functions.linkFile(FileName) });
            */


            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);
        }

        private static void GenerateKBTableGraph(string name, string fileName)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(fileName);
            IOutputService output = CommonServices.Output;
            StringCollection aristas = new StringCollection();
            output.AddLine("Generating " + name);

            scriptFile.WriteLine("<?xml version = '1.0' encoding = 'UTF-8'?>");

            scriptFile.WriteLine("<gexf xmlns='http://www.gexf.net/1.2draft'  xmlns:viz='http://www.gexf.net/1.2draft/viz' ");
            scriptFile.WriteLine("     xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='http://www.gexf.net/1.2draft http://www.gexf.net/1.2draft/gexf.xsd' version = '1.2' > ");

            scriptFile.WriteLine("  <graph mode = 'static' defaultedgetype = 'directed' > ");
            scriptFile.WriteLine("<attributes class='node'> <attribute id='0' title = 'module' type = 'string' />  </attributes >");

            scriptFile.WriteLine("      <nodes>");
            foreach (Table t in Table.GetAll(model))
                {
                    scriptFile.WriteLine("          <node id='" + t.Name + "' label='" + t.Name + "' >");
                    scriptFile.WriteLine("              <attvalues>  <attvalue for='0' value = '" + TablesHelper.TableModule(model,t).Name + "' /> </attvalues>");
                    scriptFile.WriteLine("          </node>");
                foreach (TableRelation relation in t.SuperordinatedTables)
                {
                    String edge = "          <edge id='XXXX' source='" + relation.BaseTable.Name + "' target='" + relation.RelatedTable.Name + "' />  ";
                    if (!aristas.Contains(edge))
                               aristas.Add(edge);
                }
            };
            scriptFile.WriteLine("      </nodes>");

            //Grabo las aristas
            scriptFile.WriteLine("      <edges>");

            int i = 0;
            foreach (String s in aristas)
            {
                string s2 = s.Replace("XXXX", i.ToString());
                scriptFile.WriteLine("          " + s2);
                i += 1;
            };
            scriptFile.WriteLine("      </edges>");
            scriptFile.WriteLine("  </graph>");
            scriptFile.WriteLine("</gexf>");
            scriptFile.Close();
        }

        private static void GenerateKBObjectGraph(string name, string fileName)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(fileName);
            IOutputService output = CommonServices.Output;
            StringCollection aristas = new StringCollection();
            output.AddLine("Generating " + name);

            scriptFile.WriteLine("<?xml version = '1.0' encoding = 'UTF-8'?>");

            scriptFile.WriteLine("<gexf xmlns='http://www.gexf.net/1.2draft'  xmlns:viz='http://www.gexf.net/1.2draft/viz' ");
            scriptFile.WriteLine("     xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='http://www.gexf.net/1.2draft http://www.gexf.net/1.2draft/gexf.xsd' version = '1.2' > ");

            scriptFile.WriteLine("  <graph mode = 'static' defaultedgetype = 'directed' > ");
            scriptFile.WriteLine("<attributes class='node'> <attribute id='0' title = 'module' type = 'string' />  </attributes >");

            scriptFile.WriteLine("      <nodes>");
            foreach (KBObject obj in model.Objects.GetAll())
            {
                if (Functions.isRunable(obj) || obj is Table)
                {
                    string modulename = ModulesHelper.ObjectModuleName(obj);

                    scriptFile.WriteLine("          <node id='" + obj.Name + "' label='" + obj.Name + "' >");
                    scriptFile.WriteLine("              <attvalues>  <attvalue for='0' value = '" + modulename + "' /> </attvalues>");
                    scriptFile.WriteLine("          </node>");

                    foreach (EntityReference r in obj.GetReferences())
                    {
                        KBObject objRef = KBObject.Get(obj.Model, r.To);
                        if ((objRef != null) && (Functions.isRunable(objRef) || objRef is Table))

                        {
                            String edge = "          <edge id='XXXX' source='" + obj.Name + "' target='" + objRef.Name + "' />  ";
                            if (!aristas.Contains(edge))
                                aristas.Add(edge);
                        }
                    }
                }
            };
            scriptFile.WriteLine("      </nodes>");

            //Grabo las aristas
            scriptFile.WriteLine("      <edges>");

            int i = 0;
            foreach (String s in aristas)
            {
                string s2 = s.Replace("XXXX", i.ToString());
                scriptFile.WriteLine("          " + s2);
                i += 1;
            };
            scriptFile.WriteLine("      </edges>");
            scriptFile.WriteLine("  </graph>");
            scriptFile.WriteLine("</gexf>");
            scriptFile.Close();
        }



        private static void GenerateCopyTestData(string name, string fileName, int ATTNAME_LEN, int TBLNAME_LEN)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(fileName);
            IOutputService output = CommonServices.Output;
            output.AddLine("Generating " + name);

            scriptFile.WriteLine("/* KBDoctor - CopyTestData " + DateTime.Now.ToString("yyyy/MM/dd"));
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Script to copy data from difeerents databases.  ");
            scriptFile.WriteLine("    Replace <SourceDB>.<SourceSchema>.  and <TargetDB>.<TargetSchema>. ");
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Works only in default datastore and tables whitout Data View ");
            scriptFile.WriteLine("   ");
            scriptFile.WriteLine("      */");
            //scriptFile.WriteLine("declare @max int;");

            foreach (Table t in Table.GetAll(model))
            {

                string tblAtt = "";
                string coma = "";
                string tblName = ShortName(TBLNAME_LEN, t.Name);
                string PreAutonumber="", PostAutonumber="", SeedAutonumber="";
               
                foreach (TableAttribute a in t.TableStructure.Attributes)
                {

                    string attName = ShortName(ATTNAME_LEN, a.Name);
                    bool isAutonumber = IsAutonumberAndPK(t, a); 

                    if (isAutonumber && a.IsKey)
                    {
                        PreAutonumber = Environment.NewLine + "SET IDENTITY_INSERT <TargetDB>.<TargetSchema>." + tblName + " ON; ";
                        PostAutonumber = "SET IDENTITY_INSERT <TargetDB>.<TargetSchema>." + tblName + " OFF; ";

                        //SeedAutonumber = "select @max = max(" + attName + ") from <TargetDB>.<TargetSchema>." + tblName +";" ;
                        //SeedAutonumber += Environment.NewLine + "if @max IS NUll  SET @max = 0";
                        //SeedAutonumber += Environment.NewLine + "DBCC CHECKIDENT ( <TargetDB>.<TargetSchema>." + tblName + ", RESEED, @max);";
                    }

                    if (!(a.IsFormula) || (a.IsFormula && a.IsRedundant)) 
                        if (!(a.IsInferred))
                        {
                            tblAtt += coma + attName;
                            coma = ",";
                        }
                    
                }
                scriptFile.WriteLine(PreAutonumber);
                scriptFile.WriteLine("insert into <TargetDB>.<TargetSchema>." + tblName + " (" + tblAtt + " ) select " + tblAtt + " from  <SourceDB>.<SourceSchema>." + tblName + ";");
                scriptFile.WriteLine(PostAutonumber);
                //scriptFile.WriteLine(SeedAutonumber);
            }
            scriptFile.Close();
        }

        private static bool IsAutonumberAndPK(Table t, TableAttribute a)
        {
            
            if ((t.TableStructure.PrimaryKey.Count==1) && a.IsKey && a.Attribute.GetPropertyValue<bool>("AUTONUMBER"))
                return true;
            else
                return false;
        }

        public static void GenerateSciptCheckDBStructure(string Name, string FileName, int ATTNAME_LEN, int TBLNAME_LEN)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(FileName);
            IOutputService output = CommonServices.Output;
            output.AddLine("Generating " + Name);

            scriptFile.WriteLine("/* KBDoctor - CheckDBStructure " + DateTime.Now.ToString("yyyy/MM/dd"));
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Select attributes (except formulas or inferred) from KB tables ");
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Works only in default datastore and tables whitout Data View ");
            scriptFile.WriteLine("   ");
            scriptFile.WriteLine("      */");

            foreach (Table t in Table.GetAll(model))
            {
               
                string tblAtt = "";
                string coma = "";
                string tblName = ShortName(TBLNAME_LEN,t.Name);

                foreach (TableAttribute a in t.TableStructure.Attributes)
                {
                    if (!(a.IsFormula) && !(a.IsInferred))
                    {
                        string attName = ShortName(ATTNAME_LEN, a.Name);
                        tblAtt += coma + attName;
                        coma = ",";
                    }
                }
                scriptFile.WriteLine("select " + tblAtt + " from " + tblName + " where 1=0;");
            }
            scriptFile.Close();
        }



        public static void GenerateSciptCheckPKEmptyValues(string Name, string FileName, int ATTNAME_LEN, int TBLNAME_LEN)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(FileName);
            IOutputService output = CommonServices.Output;
            output.AddLine("Generating " + Name);

            scriptFile.WriteLine("/* KBDoctor - " + Name + " " + DateTime.Now.ToString("yyyy/MM/dd"));
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Rows with empty value in Foreing Keys ");
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Works only in default datastore and tables whitout Data View  ");
            scriptFile.WriteLine("   ");
            scriptFile.WriteLine("      */");

            string description;
            string comilla = "\'";
            foreach (Table t in Table.GetAll(model))
            {
                string gxnullcondition = NullConditionKey(t,ATTNAME_LEN);
                string tblKey = KeyList(t, ATTNAME_LEN);
                string tblName = ShortName(TBLNAME_LEN, t.Name);

                scriptFile.WriteLine("");
                scriptFile.WriteLine("select '== Table: " + tblName + " Primary Key: " + tblKey + " == ';");
                scriptFile.WriteLine("select " + tblKey + " from " + t.Name + " where " + gxnullcondition + ";");
            }
            scriptFile.Close();
        }

        public static void GenerateSciptCheckFKEmptyValues(string Name, string FileName, int ATTNAME_LEN, int TBLNAME_LEN)
        {
            string description;
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            string comilla = "\'";

            IOutputService output = CommonServices.Output;
            output.AddLine("Generating " + Name);

            StreamWriter scriptFile = new StreamWriter(FileName);
            scriptFile.WriteLine("/* KBDoctor - " + Name + " " + DateTime.Now.ToString("yyyy/MM/dd"));
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Rows with empty value in Foreing Keys ");
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Works only in default datastore and tables whitout Data View  ");
            scriptFile.WriteLine("   ");
            scriptFile.WriteLine("      */");

            foreach (Table t in Table.GetAll(model))
            {
                foreach (TableAttribute a in t.TableStructure.ForeignKeyAttributes)
                {
                    string tblKey = KeyList(t, ATTNAME_LEN);

                    scriptFile.WriteLine("");
                    scriptFile.WriteLine("select '== Table: " + t.Name + " Primary Key: " + tblKey + " == Foreing Key: " + a.Name + " ==  ';");
                    scriptFile.WriteLine("select " + tblKey + ",'->', " + a.Name + " from " + t.Name + " where " + a.Name + "=" + GxNull(a) + ";");
                }
            }
            scriptFile.Close();
        }

        public static void GenerateSciptUpdateNullValues(string Name, string FileName, int ATTNAME_LEN, int TBLNAME_LEN)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(FileName);
            scriptFile.WriteLine("/* KBDoctor - " + Name + " " + DateTime.Now.ToString("yyyy/MM/dd"));
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Update Nulls values in data base.  ");
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Works only in default datastore and tables whitout Data View  ");
            scriptFile.WriteLine("   ");
            scriptFile.WriteLine("      */");

            foreach (Table t in Table.GetAll(model))
            {
                string tblKey = KeyList(t, ATTNAME_LEN);
                foreach (TableAttribute a in t.TableStructure.Attributes)
                {
                    if ((a.Attribute.Type.ToString().Contains("CHAR") || (a.Attribute.Type == eDBType.NUMERIC) || a.Attribute.Type == eDBType.DATE || a.Attribute.Type == eDBType.DATETIME)
                        && !(a.IsFormula) && !(a.IsInferred) && !(a.IsKey))
                    {
                        scriptFile.WriteLine("update " + t.Name + " set " + a.Name + "= " + GxNull(a) + " where " + a.Name + " is null;");
                    }
                }
            }
            scriptFile.Close();
        }

        private static void GenerateCheckReferentialIntegrity(string Name, string FileName, int ATTNAME_LEN, int TBLNAME_LEN)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(FileName);
            IOutputService output = CommonServices.Output;
            output.AddLine("Generating " + Name);

            scriptFile.WriteLine("/* KBDoctor - " + Name + " " + DateTime.Now.ToString("yyyy/MM/dd"));
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   List records with integrity referentials problems ");
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Works only in default datastore and tables whitout Data View  ");
            scriptFile.WriteLine("   ");
            scriptFile.WriteLine("      */");

            string comilla = "'";

            foreach (Table t in Table.GetAll(model))
            {

                string tblKey = KeyList(t,ATTNAME_LEN);

                foreach (TableRelation relation in t.SuperordinatedTables)
                {
                    string baseAttributes = ExtractListBaseAttributes(relation, ATTNAME_LEN);
                    string relatedAttributes = ExtractListRelatedAttributes(relation, ATTNAME_LEN);
                    string whereCondition = ExtractWhereCondition(t, relation, ATTNAME_LEN,TBLNAME_LEN);
                    string whereJoinCondition = ExtractWhereJoinCondition(t, relation, ATTNAME_LEN, TBLNAME_LEN);

                    scriptFile.WriteLine(" select " + comilla + ShortName(TBLNAME_LEN,t.Name) + comilla + "," + tblKey + ",'->'," + comilla + ShortName(TBLNAME_LEN,relation.RelatedTable.Name) 
                        + comilla + "," + baseAttributes + " from " + ShortName(TBLNAME_LEN,relation.BaseTable.Name) 
                        + " where not exists  " + "(select * from " + ShortName(TBLNAME_LEN,relation.RelatedTable.Name) + " where " 
                        + whereJoinCondition + ") " + whereCondition + ";");
                    output.AddLine(relation.BaseTable.Name + "," + relation.RelatedTable.Name);
                }

            }
            scriptFile.Close();
        }

        private static void GenerateCheckReferentialIntegrityN3(string Name, string FileName, int ATTNAME_LEN, int TBLNAME_LEN)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(FileName);
            IOutputService output = CommonServices.Output;
            output.AddLine("Generating " + Name);

            scriptFile.WriteLine("/* KBDoctor - " + Name + " " + DateTime.Now.ToString("yyyy/MM/dd"));
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   List records with integrity referentials problems ");
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Works only in default datastore and tables whitout Data View  ");
            scriptFile.WriteLine("   ");
            scriptFile.WriteLine("      */");

            string comilla = "'";

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Artech.Genexus.Common.Objects.Attribute>());
            foreach (Artech.Genexus.Common.Objects.Attribute att  in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                foreach (EntityReference rto in att.GetReferencesTo() )
                {
                    KBObject tbl = KBObject.Get(att.Model, rto.From);
                    if (tbl != null && tbl is Table)
                    {
                        string line = "Select " + comilla + tbl.Name + comilla + " , COUNT(*) from genexus." + tbl.Name + " where " + att.Name + " not in (select TO_NUMERIC(CODI_MONED) FROM GENEXUS.RGMONED);";
                        output.AddLine(line);
                        scriptFile.WriteLine(line);
                    }
                       
                }
            }

              
            
            scriptFile.Close();
        }

        private static void GenerateCountReferentialIntegrityProblems(string Name, string FileName, int ATTNAME_LEN, int TBLNAME_LEN)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(FileName);
            IOutputService output = CommonServices.Output;
            output.AddLine("Generating " + Name);

            scriptFile.WriteLine("/* KBDoctor - " + Name + " " + DateTime.Now.ToString("yyyy/MM/dd"));
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Count records with integrity referentials problems ");
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Works only in default datastore and tables whitout Data View  ");
            scriptFile.WriteLine("   ");
            scriptFile.WriteLine("      */");

            string comilla = "'";

            foreach (Table t in Table.GetAll(model))
            {

                string tblKey = KeyList(t,ATTNAME_LEN);

                foreach (TableRelation relation in t.SuperordinatedTables)
                {
                    string baseAttributes = ExtractListBaseAttributes(relation,ATTNAME_LEN);
                   // string relatedAttributes = ExtractListRelatedAttributes(relation,ATTNAME_LEN);
                    string whereCondition = ExtractWhereCondition(t, relation,ATTNAME_LEN,TBLNAME_LEN);
                    string whereJoinCondition = ExtractWhereJoinCondition(t, relation,ATTNAME_LEN,TBLNAME_LEN);
                    scriptFile.WriteLine(" select " + comilla + ShortName(TBLNAME_LEN,relation.RelatedTable.Name) + comilla + ",'->',"
                                                    + comilla + ShortName(TBLNAME_LEN, relation.BaseTable.Name) + comilla + "," 
                                                    + baseAttributes + ",count(*) from " + ShortName(TBLNAME_LEN,relation.BaseTable.Name) 
                                                    + " where not exists (select * from " + ShortName(TBLNAME_LEN,relation.RelatedTable.Name) 
                                                    + " where " + whereJoinCondition + ") " 
                                                    + whereCondition + " group by  " + baseAttributes + ";");

                }

            }
            scriptFile.Close();
        }

        private static void GenerateDeleteInvalidIntegrityValues(string name, string fileName, int ATTNAME_LEN, int TBLNAME_LEN)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            StreamWriter scriptFile = new StreamWriter(fileName);
            IOutputService output = CommonServices.Output;
            output.AddLine("Generating " + name);

            scriptFile.WriteLine("/* KBDoctor - " + name + " " + DateTime.Now.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture));
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   Delete records with referential integrity problemas ");
            scriptFile.WriteLine("");
            scriptFile.WriteLine("   WARNING!!: USE THIS SCRIPT AT YOUR OWN RISK   ");
            scriptFile.WriteLine("   ");
            scriptFile.WriteLine("      */");


            foreach (Table t in Table.GetAll(model))
            {

             //   string tblKey = KeyList(t,ATTNAME_LEN);

                foreach (TableRelation relation in t.SuperordinatedTables)
                {

                    string whereJoinCondition = ExtractWhereJoinCondition(t, relation,ATTNAME_LEN,TBLNAME_LEN);

                    scriptFile.WriteLine("delete from " + ShortName(TBLNAME_LEN,t.Name) + " where not exists (select * from " 
                        + ShortName(TBLNAME_LEN,relation.RelatedTable.Name) + " where " + whereJoinCondition + ");");
                }

            }
            scriptFile.Close();
            scriptFile.Dispose();
   }

        public static string ShortName(int length, string name)
        {
            if (name.Length > length)
                name = name.Substring(0, length);
            return name;
        }

        private static string KeyList(Table t, int ATTNAME_LEN)
        {
            string tblKey = "";
            string coma = "";
            foreach (TableAttribute a in t.TableStructure.PrimaryKey)
            {
                string attName = ShortName(ATTNAME_LEN, a.Name);
                tblKey += coma + attName;
                coma = ",";
            }

            return tblKey;
        }

        private static string NullConditionKey(Table t, int ATTNAME_LEN)
        {
            string gxnullcondition = "";
            string orstr = " ";
            foreach (TableAttribute a in t.TableStructure.PrimaryKey)
            {
                string attName = ShortName(ATTNAME_LEN, a.Name);
                gxnullcondition += orstr + attName + "=" + GxNull(a);
                orstr = " or ";
            }

            return gxnullcondition;
        }


        private static string ExtractWhereCondition(Table t, TableRelation relation, int  ATTNAME_LEN, int TBLNAME_LEN)
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            string whereCondition2 = "";
            foreach (Artech.Genexus.Common.Objects.Attribute arel2 in relation.BaseAttributes)
            {
                Artech.Genexus.Common.Objects.Attribute attaux = Artech.Genexus.Common.Objects.Attribute.Get(model, arel2.Id);
                TableAttribute tatt = t.TableStructure.GetAttribute(attaux);
                if (tatt.IsNullable == TableAttribute.IsNullableValue.True)
                {
                    whereCondition2 += " and " + ShortName(TBLNAME_LEN,t.Name) + "." + ShortName(ATTNAME_LEN,arel2.Name) + "<>" + GxNull(attaux) + " ";
                }
            }

            return whereCondition2;
        }

        private static string ExtractListBaseAttributes(TableRelation relation, int ATTNAME_LEN)
        {
            string baseAttributes = "";
            string coma = "";
            foreach (Artech.Genexus.Common.Objects.Attribute arel2 in relation.BaseAttributes)
            {
                baseAttributes += coma + ShortName(ATTNAME_LEN,arel2.Name);
                coma = " , ";
            }

            return baseAttributes;
        }

        private static string ExtractListRelatedAttributes(TableRelation relation, int ATTNAME_LEN)
        {
            string relatedAttributes = "";
            foreach (Artech.Genexus.Common.Objects.Attribute arel in relation.RelatedAttributes)
            {
                relatedAttributes += " , " + ShortName(ATTNAME_LEN, arel.Name);
            }

            return relatedAttributes;
        }

        private static string ExtractWhereJoinCondition(Table t, TableRelation relation,int ATTNAME_LEN,int TBLNAME_LEN)
        {
            //Armo condicion de filtrado. 
            string whereCondition = "";
            string andstr = "";
            for (int i = 0; i < relation.BaseAttributes.Count; i++)
            {
                whereCondition += andstr + ShortName(TBLNAME_LEN,t.Name) + "." + ShortName(ATTNAME_LEN,relation.BaseAttributes[i].Name) + "=" + ShortName(TBLNAME_LEN, relation.RelatedTable.Name) + "." + ShortName(ATTNAME_LEN, relation.RelatedAttributes[i].Name);
                andstr = " and ";
            }
            return whereCondition;
        }

        private static string GxNull(Artech.Genexus.Common.Objects.Attribute a)
        {
            string gxnull = "xx";
            if (a.Type == eDBType.Boolean)
                gxnull = "false";
            if (a.Type == eDBType.CHARACTER)
                gxnull = "' '";
            if (a.Type == eDBType.VARCHAR)
                gxnull = "' '";
            if (a.Type == eDBType.LONGVARCHAR)
                gxnull = "' '";
            if (a.Type == eDBType.NUMERIC)
                gxnull = "0";
            if (a.Type == eDBType.DATE)
                gxnull = "'1753-01-01'";
            if (a.Type == eDBType.DATETIME)
                gxnull = "'1753-01-01'";
            return gxnull;
            }
        
    }
}

using Artech.Architecture.BL.Framework.Services;
using Artech.Architecture.Common;
using Artech.Architecture.Common.Collections;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Services;
using Artech.Genexus.Common.Wiki;
using Artech.Udm.Framework.References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Artech.Genexus.Common.Helpers;
using Artech.Architecture.UI.Framework.Controls;
using Artech.Common.Framework.Commands;
using Artech.Udm.Framework;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;

namespace Concepto.Packages.KBDoctor
{

    static class ModulesHelper
    {

        public static string ObjectModule(KBObject obj)
        {
            string modulename = "";
            if (obj is Table)
                modulename = TablesHelper.TableModule(obj.Model, (Table)obj).Name;
            else
                modulename = obj.Module.Name;
            return modulename;
        }

        public static void MarkPublicObjects()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;
            string title = "KBDoctor - Mark Public Object";
            output.StartSection(title);
            string outputFile = Functions.CreateOutputFile(kbserv, title);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Object", "Type", "Description", "Visibility" });

            MakeAllObjectPublic(kbserv, output);

            foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
            {
                output.AddLine("Object " + obj.Name);
                ICallableObject callableObject = obj as ICallableObject;
                if (((callableObject != null) || obj is ExternalObject || obj is SDT || obj is DataSelector) && (!(obj is Transaction)))
                {
                    ObjectVisibility objVisibility = obj.GetPropertyValue<ObjectVisibility>("ObjectVisibility");

                    ObjectVisibility newObjVisibility = RecoverVisibility(obj);

                    if (objVisibility != newObjVisibility)
                    {
                        obj.SetPropertyValue("ObjectVisibility", newObjVisibility);
                        Functions.SaveObject(output, obj);
                        string objNameLink = Functions.linkObject(obj);
                        writer.AddTableData(new string[] { objNameLink, obj.TypeDescriptor.Name, obj.Description, newObjVisibility.ToString() });
                        output.AddLine("....Change Object " + obj.Name);
                    }  
                }
            }
            output.AddLine("");
            output.EndSection(title, success);
            writer.AddFooter();
            writer.Close();
            KBDoctorHelper.ShowKBDoctorResults(outputFile);
        }

        private static void MakeAllObjectPublic(IKBService kbserv, IOutputService output)
        {
            bool ToContinue = true;
            int cant = 0;

            do
            {
                ToContinue = false;
                cant = 1;
                foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
                {
                    ObjectVisibility objVisibility = obj.GetPropertyValue<ObjectVisibility>("ObjectVisibility");
                    if (objVisibility != ObjectVisibility.Public && Functions.isRunable(obj) && !(obj is Transaction))
                    {
                        obj.SetPropertyValue("ObjectVisibility", ObjectVisibility.Public);
                        Functions.SaveObject(output, obj);
                        ToContinue = true;
                        cant += 1;
                    }
                }
                output.AddLine("Cambio " + cant.ToString());
            } while (ToContinue);

        }


        private static ObjectVisibility RecoverObjectVisibility(KBObject obj)
        {
            if (obj is Table)
                return TableVisibility((Table)obj);
            else
                return obj.GetPropertyValue<ObjectVisibility>("ObjectVisibility");
        }

        private static ObjectVisibility RecoverVisibility(KBObject obj)
        {
            string objModule = obj.Module.Name;
            string objName = obj.Name;

            ObjectVisibility objVisibility = ObjectVisibility.Private;

            foreach (EntityReference reference in obj.GetReferencesTo())
            {


                KBObject objRef = KBObject.Get(obj.Model, reference.From);

                if (objRef != null && reference.ReferenceType == ReferenceType.Hard && Functions.isRunable(obj))
                {

                    if (objRef is Artech.Genexus.Common.Objects.Attribute && obj is Procedure)
                        objVisibility = ObjectVisibility.Public;
                    else
                    {
                        if (Functions.isRunable(objRef))
                        {
                            string objRefModule = objRef.Module.Name;
                            if (objRefModule != objModule)
                            {
                                objVisibility = ObjectVisibility.Public;
                            }
                        }
                    }
                }
            }
            return objVisibility;
        }

        private static ObjectVisibility TableVisibility(Table tbl)
        {
            
            ObjectVisibility objVisibility = ObjectVisibility.Private;

            foreach (Transaction trn in tbl.AssociatedTransactions)
            {
                if (trn.GetPropertyValue<ObjectVisibility>("ObjectVisibility") == ObjectVisibility.Public)
                    objVisibility = ObjectVisibility.Public;
            }
            return objVisibility;
            
        }

        /*

Dependencias Entrantes
Dependencias Salientes
IE = Salientes / (Entrantes + Salientes)
#Componentes Conexos dentro del módulo.
Tiene dependencia cíclicla ?
Largo máximo de dependencias en la que participa.
El módulo tiene objetos públicos no referenciados por externos?
*/

        public static void ListModulesStatistics()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;
            int objInRoot = 0;
            int objSinRoot = 0;
            string title = "KBDoctor - List Modules Statistics";
            output.StartSection(title);
            string outputFile = Functions.CreateOutputFile(kbserv, title);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Module", "Description", "Tables", "Public Tables", "Objects", "Public Obj", "Obj/Publ %", "In References" , "Out References" });

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Module>());
            foreach (Module mdl in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {

                    output.AddLine(mdl.Name + "....");
                    string[] mdlStat = ModuleStats2(mdl);

                if (mdl.Name == "Root Module")
                {
                    objInRoot = Int32.Parse(mdlStat[4]);
                }
                else
                {
                    if (Int32.Parse(mdlStat[2]) != 0)
                         objSinRoot = objSinRoot + Int32.Parse(mdlStat[4]);
                }
                    writer.AddTableData(mdlStat);
                

            }
            output.AddLine("");
            output.EndSection(title, success);
            int ratio = (objInRoot == 0) ? 0 : (objSinRoot * 100) / objInRoot;
            string Resumen = "Obj in Modules, Obj Root, Ratio  " + objSinRoot.ToString() + "," + objInRoot.ToString() + "," + ratio.ToString();

            writer.AddTableData(new string[] { Resumen });
            writer.AddFooter();
            writer.Close();
            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            Functions.AddLineSummary("moduleStats.txt", Resumen);

        }

        private static string[] ModuleStats2(Module mdl)
        {
            IOutputService output = CommonServices.Output;

            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;

            /*

Dependencias Entrantes
Dependencias Salientes
IE = Salientes / (Entrantes + Salientes)
#Componentes Conexos dentro del módulo.
Tiene dependencia cíclicla ?
Largo máximo de dependencias en la que participa.
El módulo tiene objetos públicos no referenciados por externos?
*/

            int cantobj = 0;
            int cantobjmain = 0;
            int cantobjPub = 0;
            int cantInRef = 0;
            int cantOutRef = 0;
            foreach (KBObject obj in mdl.GetAllMembers())
            {
                cantobj += 1;
                string aux = "";
                bool isReferencedFromOutside = IsReferencedFromOutside(mdl, obj, out aux);
                
                ObjectVisibility objVisibility = RecoverObjectVisibility(obj);

                if (Functions.isRunable(obj))
                    { 
                        if (objVisibility == ObjectVisibility.Public)
                            {
                                cantobjPub += 1;
                            }

                        if (!isReferencedFromOutside)
                            {
                                cantInRef += 1; ;
                            }
                    }
                

            }


            //Hago lo mismo para las tablas del modulo. 

            IList<KBObject> tblList = (from r in Table.GetAll(kbModel)
                                       where TablesHelper.TableModule(kbModel, r) == mdl
                                       select kbModel.Objects.Get(r.Key)).ToList();

            int cntTables = 0;
            int cntPublicTables = 0;
            foreach (Table tbl in tblList)
            {
                cntTables += 1;

                string tableName = tbl.Name;
                ObjectVisibility visibility = TableVisibility(tbl);

                if (visibility == ObjectVisibility.Public)
                    cntPublicTables += 1;

                string objOutsideModuleAccessPrivateTable = "";
                bool TableReferencedFromOutside = IsReferencedFromOutside(mdl, (KBObject)tbl, out objOutsideModuleAccessPrivateTable);


                if (TableReferencedFromOutside )
                {
                    cantInRef += 1; ;
                }
                
            }


            int rel = (cantobj == 0 ? 0 : (cantobjPub * 100) / cantobj);
            string[] mdlStats = new string[] { Functions.linkObject(mdl), mdl.Description, cntTables.ToString(), cntPublicTables.ToString(), cantobj.ToString(),  cantobjPub.ToString(), rel.ToString() + " %" , cantInRef.ToString(), cantOutRef.ToString() };
                                              
            return mdlStats;
        }

        public static void MoveTransactions()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;
            int objInRoot = 0;
            int objSinRoot = 0;
            string title = "KBDoctor - Move Transaction";
            output.StartSection(title);
            string outputFile = Functions.CreateOutputFile(kbserv, title);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Transaction", "Description", "Table", "Most referenced in Folder" });

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());
            foreach (Transaction trn in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {


                    foreach (TransactionLevel lvl in trn.Structure.GetLevels())
                    {
                    Table tbl = lvl.AssociatedTable;
                    List<string> fldlist = MostReferencedInFolder(tbl);
                    string listatxt = "";
                    foreach (string s in fldlist)
                        listatxt += s + "<br>";

                    writer.AddTableData(new string[] { Functions.linkObject(trn) , trn.Description, Functions.linkObject(tbl),  listatxt });
 

                }

            }


            writer.AddFooter();
            writer.Close();
            KBDoctorHelper.ShowKBDoctorResults(outputFile);


        }

        private static List<string> MostReferencedInFolder(Table tbl)
        {
            IOutputService output = CommonServices.Output;
            List<string> list = new List<string>();

            foreach (EntityReference refe in tbl.GetReferencesTo())
            {
                KBObject objRef = KBObject.Get(tbl.Model, refe.From);
                if (objRef != null)
                {
                    bool read, insert, update, delete, isBase;

                    ReferenceTypeInfo.ReadTableInfo(refe.LinkTypeInfo, out read, out insert, out update, out delete, out isBase);

                    string updated = (update || delete || insert) ? "UPDATED" : "";

                    if (objRef.Parent is Folder)
                        list.Add("FOLDER:" + objRef.Parent.Name + " |  " + updated);
                    if (objRef.Parent is Module)
                        list.Add("MODULE:" + objRef.Parent.Name + " |  " + updated);
                }
                
            }
            output.AddLine(" ");
            output.AddLine("============> " + tbl.Name);
            
            list.Sort();
            foreach (string s in list)
                output.AddLine(s);
            return list;
        }

        public static void ListModulesErrors()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;

            string title = "KBDoctor - List Modules Errors";
            output.StartSection(title);
            string outputFile = Functions.CreateOutputFile(kbserv, title);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] {
                "Module", "Warning Public Objects not referenced", "ERROR Private Objects Referenced", "ERROR  Reference private Tables", "ERROR Update outside Module", "Object to Move" });

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Module>());
            foreach (Module mdl in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                if (mdl is Module)
                {
                    output.AddLine(mdl.Name + "....");
                    string[] mdlStat = ModuleStats((Module)mdl);

                    writer.AddTableData(mdlStat);
                }

            }
            output.AddLine("");
            output.EndSection(title, success);

            writer.AddFooter();
            writer.Close();
            KBDoctorHelper.ShowKBDoctorResults(outputFile);


        }

        private static string[] ModuleStats(Module mdl)
        {
            IOutputService output = CommonServices.Output;

            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            string listPubObjNotReferenced = "";
            string listPirvateObjReferenced = "";
            string ListobjOutsideModuleAccessPrivateTable = "";

            foreach (KBObject obj in mdl.GetAllMembers())
            {

                string aux = "";
                bool isReferencedFromOutside = IsReferencedFromOutside(mdl, obj, out aux);

                ObjectVisibility objVisibility = RecoverObjectVisibility(obj);

                if (objVisibility == ObjectVisibility.Public && Functions.isRunable(obj) && !(obj is Transaction))
                {

                    if (!isReferencedFromOutside)
                    {
                        listPubObjNotReferenced += Functions.linkObject(obj) + " ";
                    }
                }


                // Por un error de GX, no se listan los SDT pues todos quedan como publicos aunque esten marcados como privados. 
                if (objVisibility == ObjectVisibility.Private && isReferencedFromOutside && !(obj is SDT) && !(obj is Folder))
                {
                    listPirvateObjReferenced += Functions.linkObject(obj) + " ";
                }
            }

            string objOutsideModuleAccessPrivateTable = "";
            string objUpdateOutsideModuleList = "";
            string objToMove = "";
            //Hago lo mismo para las tablas del modulo. 

            IList<KBObject> tblList = (from r in Table.GetAll(kbModel)
                                       where TablesHelper.TableModule(kbModel, r) == mdl
                                       select kbModel.Objects.Get(r.Key)).ToList();
            foreach (Table tbl in tblList)
            {
                string tableName = tbl.Name;
                ObjectVisibility visibility = TableVisibility(tbl);

                bool TableReferencedFromOutside = IsReferencedFromOutside(mdl, (KBObject)tbl, out objOutsideModuleAccessPrivateTable);

                if (TableReferencedFromOutside && (visibility == ObjectVisibility.Private))
                {
                    ListobjOutsideModuleAccessPrivateTable += objOutsideModuleAccessPrivateTable + "<BR>" + Environment.NewLine;
                }
                else
                    if (!TableReferencedFromOutside && (visibility == ObjectVisibility.Public))
                      {
                         listPubObjNotReferenced += Functions.linkObject((KBObject)tbl) + " ";
                      }

                string objUpdateOutsideModuleListTable = "";
                foreach (KBObject objUpdateOutsideModule in TablesHelper.ObjectsUpdateTableOutsideModule(kbModel, tbl))
                {

                    objUpdateOutsideModuleListTable += Functions.linkObject(objUpdateOutsideModule) + " ";
                }
                if (objUpdateOutsideModuleListTable != "")
                    objUpdateOutsideModuleList += tbl.Name + "  (" + objUpdateOutsideModuleListTable + ")<BR>";

                //Veo los objetos que referencian alguna tabla desde fuera del modulo, si tienen alguna tabla de otro modulo
                //Si son todas de mi modulo, lo pongo en la lista de los objetos a mover a mi modulo. 
               
                foreach (KBObject objreadOutsideModule in TablesHelper.ObjectsReadTableOutsideModule(tbl))
                {
                    
                    if (ListModulesOfReferencedTables(objreadOutsideModule).Count == 1)
                        objToMove += Functions.linkObject(objreadOutsideModule) +"; " ;
                }
            }

            string[] mdlStats = new string[] { Functions.linkObject(mdl), listPubObjNotReferenced, listPirvateObjReferenced, ListobjOutsideModuleAccessPrivateTable, objUpdateOutsideModuleList,objToMove };
          
            return mdlStats;
        }



        private static bool IsReferencedFromOutside(Module mdl, KBObject obj, out string objList)
        {
            if (obj is Table)
                objList = obj.Name + " ";
            else
                objList = "";

            bool isReferencedFromOutside = false;
            string objName = obj.Name;
            foreach (EntityReference refer in obj.GetReferencesTo())
            {
                KBObject objRef = KBObject.Get(obj.Model, refer.From);
                if (objRef != null && Functions.isRunable(objRef)) 
                {
                    Module modref = objRef.Module;
                    if (modref != mdl)
                    {
                        isReferencedFromOutside = true;
                        if (obj is Table)
                            objList += " " + Functions.linkObject(objRef);
                    }
                }
            }

            return isReferencedFromOutside;
        }
        private static KBObjectCollection ObjectsReferencesFromOutside(KBObject obj)
        {
            KBObjectCollection objCol = new KBObjectCollection();

            string mdlName = ObjectModule(obj);
           
            foreach (EntityReference refer in obj.GetReferencesTo())
            {
                KBObject objRef = KBObject.Get(obj.Model, refer.From);
                if (objRef != null && Functions.isRunable(objRef)) 
                {
                    string mdlNameRef = ObjectModule(objRef);
                    if (mdlNameRef != mdlName)
                    {
                        if (obj is Table)
                            objCol.Add(obj); 
                    }
                }
            }
            return objCol;
        }


        public static void BuildModule()
        {
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;

            bool success = true;
            string title = "KBDoctor - Build Module";
            output.StartSection(title);
            string outputFile = Functions.CreateOutputFile(kbserv, title);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Object", "Description", "Visibility" });

            KBObjectCollection objToBuild = new KBObjectCollection();

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;

            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Module>());
            foreach (Module mdl in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                foreach (KBObject o in CreateListObjectsModuleAndReferences(kbModel, mdl, writer))
                       objToBuild.Add(o);
            }

            writer.AddFooter();
            writer.Close();
            KBDoctorHelper.ShowKBDoctorResults(outputFile);

            GenexusUIServices.Build.BuildWithTheseOnly(objToBuild.Keys);

            do
            {
                Application.DoEvents();
            } while (GenexusUIServices.Build.IsBuilding);

            output.EndSection("KBDoctor", true);


        }

        private static KBObjectCollection CreateListObjectsModuleAndReferences(KBModel kbModel, Module mdl, KBDoctorXMLWriter writer)
        {
            KBObjectCollection objToBuild = new KBObjectCollection();
            foreach (KBObject obj in mdl.GetAllMembers())
            {
                if (KBObjectHelper.IsSpecifiable(obj) && ObjectsHelper.isGenerated(obj))
                {
                    if (!objToBuild.Contains(obj))
                    {
                        objToBuild.Add(obj);
                        writer.AddTableData(new string[] { obj.QualifiedName.ToString(), obj.Description, obj.QualifiedName.ToString() + " (" + obj.TypeDescriptor.Name + ")" });
                    }
                }
                AddObjectsReferenceTo(obj, objToBuild, writer);
            }

            foreach (Table tbl in Table.GetAll(kbModel))
            {
                if (TablesHelper.TableModule(kbModel, tbl) == mdl)
                    AddObjectsReferenceTo(tbl, objToBuild, writer);
            }
            return objToBuild;
        }

        public static void AddObjectsReferenceTo(KBObject obj, KBObjectCollection objToBuild, KBDoctorXMLWriter writer)
        {
            foreach (EntityReference refe in obj.GetReferencesTo())
            {
                KBObject objRef = KBObject.Get(obj.Model, refe.From);
                if (objRef != null && KBObjectHelper.IsSpecifiable(objRef) && !objToBuild.Contains(objRef) && ObjectsHelper.isGenerated(objRef) )
                {
                    objToBuild.Add(objRef);
                    writer.AddTableData(new string[] { objRef.QualifiedName.ToString(), objRef.Description,  obj.QualifiedName.ToString() + " (" + obj.TypeDescriptor.Name + ")" });
                }
            }
        }


        public static List<EntityKey> GetAllObjectsKeys(IEnumerable<KBObject> objects)
        {
            List<KBObject> parents = new List<KBObject>(objects);
            if (parents.Count == 0)
                return null;

            List<EntityKey> keys = new List<EntityKey>();
            parents.ForEach(o => keys.AddRange(UIServices.KB.CurrentModel.Objects.GetAllChildren(o).Select(ch => ch.Key)));

            if (keys.Count == 0)
                return null;
            keys.RemoveAll(k => k.Type == typeof(Folder).GUID);

            return keys;
        }

        internal static void ListTableInModules()
        {

            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel.GetDesignModel();

            string title = "KBDoctor - List tables in modules";
            IOutputService output = CommonServices.Output;
            output.StartSection(title);

            string outputFile = Functions.CreateOutputFile(kbserv, title);

            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] {
                "Name", "Description", "Visibility", "Best", "Best Module", "Modules","Transaction", "Trn(NoGenerate)", "Referenced Modules" , "Referenced"  });

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = true;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Module>());
            foreach (KBObject module in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                foreach (Table t in Table.GetAll(module.Model))

                {
                    if (TablesHelper.TableModule(module.Model, t) == module)
                    {
                        string objNameLink = Functions.linkObject(t);

                        output.AddLine("Processing... " + t.Name);

                        int countAttr = 0;
                        int countKeyAttr = 0;
                        int widthKey = 0;
                        int width = 0;
                        int widthVariable = 0;
                        int widthFixed = 0;

                        ObjectVisibility objVisibility = TableVisibility(t);
                        KBObject trnBest = GenexusBLServices.Tables.GetBestAssociatedTransaction(model, t.Key);
                        Module mdlTrnBest = trnBest.Module;

                        string trnGen = "";
                        string trnNoGen = "";
                        List<string> mdlList = new List<string>();
                        foreach (Transaction trn in t.AssociatedTransactions)

                        {
                            string trnstr = Functions.linkObject(trn) + "(" + ((trn.IsPublic) ? "Public" : "Private") + ") <br/> " ;
                            if (trn.GetPropertyValue<bool>(Properties.TRN.GenerateObject)) trnGen += trnstr ;
                            else trnNoGen += trnstr;

                            if (!mdlList.Contains(trn.Module.Name))
                            {
                                mdlList.Add(trn.Module.Name);
                            }
                        }


                        IList<KBObject> objList = (from r in model.GetReferencesTo(t.Key, LinkType.UsedObject)
                                                   where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                                                                                       //where ReferenceTypeInfo.HasUpdateAccess(r.LinkTypeInfo)
                                                   select model.Objects.Get(r.From)).ToList();

                        IList<KBObject> objList2 = objList.Where(r => r.Module != mdlTrnBest).ToList();

                        string objListQNames = null;
                        objList2.ToList().ForEach(v => objListQNames += " " + Functions.linkObject(v));

                        List<string> mdlReferencedList = new List<string>();
                        foreach (KBObject o in objList)
                        {
                            if (!mdlReferencedList.Contains(o.Module.Name))
                            {
                                mdlReferencedList.Add(o.Module.Name);
                            }
                        }

                        string mdlListstr = String.Join(" ", mdlList.ToArray());
                        string mdlReferencedListstr = String.Join(" ", mdlReferencedList.ToArray());

                        writer.AddTableData(new string[] {
                            objNameLink, t.Description,objVisibility.ToString(),trnBest.QualifiedName.ToString(),mdlTrnBest.Name,
                                 mdlListstr, trnGen, trnNoGen ,mdlReferencedListstr, objListQNames});

                    }
                }

            }


            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);
        }

        internal static void ModuleDependencies()

        {
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            bool success = true;
            int objInRoot = 0;
            int objSinRoot = 0;
            string title = "KBDoctor - Module references";
            output.StartSection(title);
            string outputFile = Functions.CreateOutputFile(kbserv, title);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Object", "Module", "Type" , "Is referenced by"});

            KBObjectCollection objRefCollection = new KBObjectCollection();

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = false;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Module>());
            Module module2 = new Module(kbModel);
            foreach (Module module in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                module2 = module;
                
                    output.AddLine("Procesing " + module.Name + "....");

                    foreach (KBObject obj in ModuleObjects(module))
                    {
                        foreach (EntityReference reference in obj.GetReferences())
                        {
                            KBObject objref = KBObject.Get(obj.Model, reference.To);
                            if (objref != null && objref.TypeDescriptor.Name != "Attribute"  && objref.TypeDescriptor.Name != "MasterPage")
                            {
                                Module objrefModule = ((objref is Table) ? TablesHelper.TableModule(objref.Model, (Table)objref) : objref.Module);

                                if (objrefModule != module)
                                    if (!(objref is Domain) && !(objref is Image) && !(objref is Theme) && !(objref is ThemeClass)
                                        && !(objref is GeneratorCategory) && !(objref is KBCategory) && !(objref is SDT))
                                    {
                                        bool contain = objRefCollection.Any(p => p.Guid == objref.Guid);
                                        if (!contain)
                                        {
                                            objRefCollection.Add(objref);
                                        }
                                    }
                            }
                          
                        }

                    

                }

            }
            string listObj = "";
            //Listo todos los objetos externos referenciados desde el modulo
            foreach (KBObject o in objRefCollection)
            {
                listObj = "";
                //Armo lista de objetos de mi modulo que referencian al objeto externo
                foreach (EntityReference refe in o.GetReferencesTo())
                {
                    
                    KBObject objref = KBObject.Get(o.Model, refe.From);
                    if (objref !=null)
                    {
                        Module objrefModule = ((objref is Table) ? TablesHelper.TableModule(objref.Model, (Table)objref) : objref.Module);
                        if (objrefModule == module2)
                        {
                            listObj += " " + Functions.linkObject(objref);
                        }

                    }

                }
                Module oModule = ((o is Table) ? TablesHelper.TableModule(o.Model, (Table)o) : o.Module);
                writer.AddTableData(new string[] { Functions.linkObject(o), oModule.Name, o.TypeDescriptor.Name, listObj  });
            }
            output.AddLine("");
            output.EndSection(title, success);

            writer.AddFooter();
            writer.Close();
            KBDoctorHelper.ShowKBDoctorResults(outputFile);


        }

        internal static void ObjectsToDivide()

        {
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            bool success = true;
            int objInRoot = 0;
            int objSinRoot = 0;
            string title = "KBDoctor - Objects to divide";
            output.StartSection(title);
            string outputFile = Functions.CreateOutputFile(kbserv, title);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Object", "Module", "Type", "Is referenced by" });

            KBObjectCollection objRefCollection = new KBObjectCollection();

            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = false;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Module>());
            Module module2 = new Module(kbModel);
            foreach (Module module in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                module2 = module;

                output.AddLine("Procesing " + module.Name + "....");

                foreach (KBObject obj in ModuleObjects(module))
                {
                    foreach (EntityReference reference in obj.GetReferences())
                    {
                        KBObject objref = KBObject.Get(obj.Model, reference.To);
                        if (objref != null && objref.TypeDescriptor.Name != "Attribute" && objref.TypeDescriptor.Name != "MasterPage")
                        {
                            Module objrefModule = ((objref is Table) ? TablesHelper.TableModule(objref.Model, (Table)objref) : objref.Module);

                            if (objrefModule != module)
                                if (!(objref is Domain) && !(objref is Image) && !(objref is Theme) && !(objref is ThemeClass)
                                    && !(objref is GeneratorCategory) && !(objref is KBCategory) && !(objref is SDT))
                                {
                                    bool contain = objRefCollection.Any(p => p.Guid == objref.Guid);
                                    if (!contain)
                                    {
                                        objRefCollection.Add(objref);
                                    }
                                }
                        }

                    }



                }

            }
            string listObj = "";
            //Listo todos los objetos externos referenciados desde el modulo
            foreach (KBObject o in objRefCollection)
            {
                listObj = "";
                //Armo lista de objetos de mi modulo que referencian al objeto externo
                foreach (EntityReference refe in o.GetReferencesTo())
                {

                    KBObject objref = KBObject.Get(o.Model, refe.From);
                    if (objref != null)
                    {
                        Module objrefModule = ((objref is Table) ? TablesHelper.TableModule(objref.Model, (Table)objref) : objref.Module);
                        if (objrefModule == module2)
                        {
                            listObj += " " + Functions.linkObject(objref);
                        }

                    }

                }
                Module oModule = ((o is Table) ? TablesHelper.TableModule(o.Model, (Table)o) : o.Module);
                writer.AddTableData(new string[] { Functions.linkObject(o), oModule.Name, o.TypeDescriptor.Name, listObj });
            }
            output.AddLine("");
            output.EndSection(title, success);

            writer.AddFooter();
            writer.Close();
            KBDoctorHelper.ShowKBDoctorResults(outputFile);


        }



        internal static KBObjectCollection ModuleObjects(Module module)
        {
            {
                KBObjectCollection objectsModule = new KBObjectCollection();
                foreach (KBObject obj in module.GetAllMembers())
                {
                    if (KBObjectHelper.IsSpecifiable(obj))
                    {
                        if (!objectsModule.Contains(obj))
                                    objectsModule.Add(obj);
                        
                    }

                }


                foreach (Table tbl in Table.GetAll(module.Model))

                {
                    if (TablesHelper.TableModule(module.Model, tbl) == module)
                        objectsModule.Add(tbl);
                }
                return objectsModule;
            }
        }

        public static void RecomendedModule()
        {
            IKBService kbserv = UIServices.KB;

            Dictionary<string, KBObjectCollection> dic = new Dictionary<string, KBObjectCollection>();

            string title = "KBDoctor - Recomended module";
            string outputFile = Functions.CreateOutputFile(kbserv, title);

            IOutputService output = CommonServices.Output;
            output.StartSection(title);

            string sw = "";
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            int numObj = 0;

            writer.AddTableHeader(new string[] { "Type", "Object", "Module", "List of modules" });


            SelectObjectOptions selectObjectOption = new SelectObjectOptions();
            selectObjectOption.MultipleSelection = false;
            selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Module>());
            // Module module2 = new Module(kbModel);
            foreach (Module module in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
            {
                foreach (KBObject obj in module.GetAllMembers()) 
                {
                    if (obj != null && ObjectsHelper.isGenerated(obj) && Functions.isRunable(obj))
                    {

                        string moduleListString = "";
                        foreach (Module mod in ListModulesOfReferencedTables(obj))
                            moduleListString += mod.Name + " ";

                        if (obj.Module.Name != moduleListString.Trim() && moduleListString.Trim() != "")
                            writer.AddTableData(new string[] { obj.TypeDescriptor.Name + " ", Functions.linkObject(obj), obj.Module.Name, moduleListString });
                    }
                }
            }
            writer.AddFooter();
            writer.Close();

            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            bool success = true;
            output.EndSection(title, success);


        }

        private static List<Module> ListModulesOfReferencedTables(KBObject obj)
        {
            List<Module> moduleList = new List<Module>();
            foreach (EntityReference refe in obj.GetReferences())
            {
                KBObject objref = KBObject.Get(obj.Model, refe.To);
                if (objref != null && objref is Table)
                {
                    Module objrefModule = TablesHelper.TableModule(objref.Model, (Table)objref);
                    if (!moduleList.Contains(objrefModule))
                        moduleList.Add(objrefModule);
                }
            }

            return moduleList;
        }
    }







}

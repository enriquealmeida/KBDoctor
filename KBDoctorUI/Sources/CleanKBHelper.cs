using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
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
using Artech.Packages.Patterns;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Artech.Udm.Framework;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    static class CleanKBHelper
    {

        public static void CleanKBAsMuchAsPossible()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string mensaje = "Do you have a backup?. Are you sure you want to clean the KB? Some objects will be modified and deleted.";
            DialogResult dr = MessageBox.Show(mensaje, "Clean KB", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                ObjectsHelper.CleanVarsNotUsed();
                CleanKBHelper.RemoveAttributeWithoutTable();
                ObjectsHelper.Unreachables();

                KBCategory UnreachableCategory = KBCategory.Get(kbserv.CurrentModel, "KBDoctor.UnReachable");
                foreach (KBObject obj in UnreachableCategory.AllMembers)
                {
                    KBDoctorCore.Sources.API.CleanKBObject(obj, output);
                }

                ProcessObjectsInCategory(output, UnreachableCategory);
                KBDoctorOutput.Message("Finish");

            }
        }

        private static void ProcessObjectsInCategory(IOutputService output, KBCategory Category)
        {
            bool stay = true;
            int pasada = 1;
            int borrados = 0;
            do
            {
                KBDoctorOutput.Message("Pass number " + pasada.ToString() + ". Cleaned objects: " + borrados.ToString());
                stay = false;

                foreach (KBObject obj in Category.AllMembers)
                {
                    if ((obj is Transaction) || (obj is Table) || (obj is Image) || (obj is Artech.Genexus.Common.Objects.Group) || (obj is DataView))
                    {
                        KBDoctorOutput.Message("Skipping " + obj.Name);

                    }
                    else
                    {
                        try
                        {
                            obj.Delete();
                            KBDoctorOutput.Message("Removing : " + obj.Name);
                            stay = true;
                            borrados += 1;
                        }
                        catch
                        {
                            KBDoctorOutput.Message("ERROR: Can't remove :" + obj.Name);

                        }

                    }

                }



                pasada += 1;
            } while (stay);
        }





        public static void AddINParmRule()
        {

            // Object with parm() rule without in: out: or inout:
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - ADD IN: to Parm() rule";

            output.StartSection("KBDoctor",title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Description", "Tipo" });
                int cantObjChanged = 0;

                SelectObjectOptions selectObjectOption = new SelectObjectOptions
                {
                    MultipleSelection = true
                };
             //   selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
               // selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());

                foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
                {
                    if (obj.Description.StartsWith("Work With"))
                    {
                        writer.AddTableData(new string[] { obj.Name, obj.Description, obj.TypeDescriptor.Name });
                        obj.Description=obj.Description.Replace("Work With ", "Trabajar con ");
                        obj.Save();
                    }

                }

                writer.AddFooter();
                writer.Close();

                bool success = true;
                KBDoctorOutput.EndSection( title, success);
                KBDoctorOutput.Message( "Object changed " + cantObjChanged.ToString());

                KBDoctorHelper.ShowKBDoctorResults(outputFile);
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        private static void PrintNewRuleParm(KBDoctorXMLWriter writer, KBObject obj, string oldParm, string newParm)
        {
            ICallableObject callableObject = obj as ICallableObject;

            if (callableObject != null)
            {
                foreach (Signature signature in callableObject.GetSignatures())
                {
                    foreach (Parameter parm in signature.Parameters)
                    {
                        string nameParm = parm.IsAttribute ? parm.Name : "&" + parm.Name;
                        ListParmReferences(obj, nameParm, writer);
                    }
                }
            }
            writer.AddTableData(new string[] { Functions.linkObject(obj), oldParm, "====" });
            writer.AddTableData(new string[] { "", newParm, "=====" });
            writer.AddTableData(new string[] { "======", "======", "=======" });
        }

        public static void ListTableAttributesUsingDomain()
        {

            // Object with parm() rule without in: out: or inout:
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Attributes using domain";

            output.StartSection("KBDoctor",title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Domain", "Table", "Description", "Attribute", "Descripcion", "Module" });

                SelectObjectOptions selectObjectOption = new SelectObjectOptions
                {
                    MultipleSelection = true
                };
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Domain>());

                //Pido dominios
                foreach (KBObject dom in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
                {
                    //Atributos con ese dominio
                    foreach (EntityReference reference in dom.GetReferencesTo())
                    {
                        KBObject att = KBObject.Get(dom.Model, reference.From);

                        if ((att != null) && (att is Artech.Genexus.Common.Objects.Attribute))
                        {

                            foreach (EntityReference reference2 in att.GetReferencesTo())
                            {
                                KBObject tbl = KBObject.Get(att.Model, reference2.From);

                                if ((tbl != null) && (tbl is Table))
                                {
                                    Artech.Genexus.Common.Objects.Attribute a = (Artech.Genexus.Common.Objects.Attribute)att;
                                    Formula formula = a.Formula;
                                    if (formula == null)
                                    {
                                        writer.AddTableData(new string[] { Functions.linkObject(dom), Functions.linkObject(tbl), tbl.Description, Functions.linkObject(att), att.Description, ModulesHelper.ObjectModuleName(tbl) });

                                        KBDoctorOutput.Message("select '" + tbl.Name + " " + att.Name + "' from dual;");

                                        string tblKey = TablesHelper.KeyList((Table)tbl, 31);
                                        KBDoctorOutput.Message("select " + tblKey + "," + att.Name + " from " + tbl.Name + " where " + att.Name + " = '&value' ;");

                                    }
                                }
                            }
                        }
                    }
                }

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

        private static void ListParmReferences(KBObject obj, string name, KBDoctorXMLWriter writer)
        {
            string source = ObjectsHelper.ObjectSource(obj);
            source = Functions.ExtractComments(source);
            string linesWithParmName = "";
            using (StringReader reader = new StringReader(source))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {

                    if (line.ToUpper().Contains(name.ToUpper()))
                    {
                        linesWithParmName += line + Environment.NewLine + "<br>";

                    }

                }
                
            }
            writer.AddTableData(new string[] { Functions.linkObject(obj),  name, linesWithParmName });
        }

        public static string ChangeRuleParmWithIN(KBObject obj)
        {
            string newParm="";

            RulesPart rulesPart = obj.Parts.Get<RulesPart>();

            ICallableObject callableObject = obj as ICallableObject;

            if (callableObject != null)
            {
                foreach (Signature signature in callableObject.GetSignatures())
                {
                    Boolean someInOut = false;
                    foreach (Parameter parm in signature.Parameters)
                    {
                        if (parm.Accessor.ToString() == "PARM_INOUT")
                        {
                            someInOut = true;
                            break;
                        }
                    }
                    if (someInOut && (rulesPart != null))
                    {
                        Regex myReg = new Regex("//.*", RegexOptions.None);
                        Regex myReg2 = new Regex(@"/\*.*\*/", RegexOptions.Singleline);
                        Regex paramReg = new Regex(@"parm\(.*\)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
                        string reglas = rulesPart.Source;
                        reglas = myReg.Replace(reglas, "");
                        reglas = myReg2.Replace(reglas, "");

                        string reglas2 = reglas.Replace(Environment.NewLine, " ");

                        Match match = paramReg.Match(reglas2);


                        if (match != null)
                        {
                            int countparms = match.ToString().Split(new char[] { ',' }).Length;
                            int countsemicolon = match.ToString().Split(new char[] { ':' }).Length - 1;
                            if (countparms != countsemicolon)
                            {
                                string objNameLink = Functions.linkObject(obj);

                                Regex coma = new Regex(",", RegexOptions.None);
                                newParm = coma.Replace(match.ToString(), ", IN:");

                                Regex inreg = new Regex("IN:", RegexOptions.IgnoreCase);
                                newParm = inreg.Replace(newParm, "IN:");

                                Regex outreg = new Regex("OUT:", RegexOptions.IgnoreCase);
                                newParm = outreg.Replace(newParm, "OUT:");

                                Regex IOreg = new Regex("INOUT:", RegexOptions.IgnoreCase);
                                newParm = IOreg.Replace(newParm, "INOUT:");


                                newParm = newParm.Replace(" ", "");
                                newParm = newParm.Replace("out:", "OUT:");
                                newParm = newParm.Replace("Out:", "OUT:");
                                newParm = newParm.Replace("InOut:", "INOUT:");
                                newParm = newParm.Replace("inout:", "INOUT:");
                                newParm = newParm.Replace("in:", "IN:");
                                newParm = newParm.Replace("In:", "IN:");
                                //---- CAMBIO REPETIDOS.
                                newParm = newParm.Replace("()", "##"); //Por los vectores

                                newParm = newParm.Replace("(", "(IN:");

                                newParm = newParm.Replace("IN:IN:", "IN:");
                                newParm = newParm.Replace("IN:OUT:", "OUT:");
                                newParm = newParm.Replace("IN:INOUT:", "INOUT:");
                                newParm = newParm.Replace("##", "()"); //Vuelvo el vector al original
                                newParm = newParm + ";";
                            }
                        }
                    }
                }
            }
           
            return newParm;
        }

        public static void SaveObjectNewParm(IOutputService output, KBObject obj, string oldParm, string newParm)
        {

            RulesPart rulPart = obj.Parts.Get<RulesPart>();

            string newRules = "";
            try {
                newParm = newParm.Replace(";", " "); 
                newRules = rulPart.Source.Replace(oldParm, newParm);
                rulPart.Source = newRules;

            }
                catch (Exception e) { KBDoctorOutput.Message(e.Message); };
            

            try
            {
                obj.Save();
            }
            catch (Exception e)
            {
                KBDoctorOutput.Message("Error SAVING " + obj.Name + " New parm rule " + newParm + " " + e.Message);
            }
        }

        internal static void RenameVariables()
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
                writer.AddTableHeader(new string[] { "Type", "Name", "Variable", "Attribute", "Domain" });

                Domain dom = Functions.DomainByName("Fecha");

                //All useful objects are added to a collection
                foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
                {
                    KBDoctorOutput.Message( "Procesing.... " + obj.Name + " - " + obj.Type.ToString());

                    if (Utility.isGenerated(obj) && !ObjectsHelper.isGeneratedbyPattern(obj) && (obj is Transaction || obj is WebPanel || obj is WorkPanel))
                    {
                        Functions.AddLine("RenameVariables.txt", "##" + obj.Name);
                        List<Variable> lstVariables = VariablesToRename(obj);
                    }

                }

                string inputFile = kbserv.CurrentKB.UserDirectory + @"\RenameVariables.txt";

                // Input
                List<String> data = File.ReadAllLines(inputFile).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();

                // Processing
                data.Sort();

                // Output   
                string outputFile2 = kbserv.CurrentKB.UserDirectory + @"\RenameVariables2.txt";
                File.WriteAllLines(outputFile2, data.ToArray());


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


        public static List<Variable>  VariablesToRename(KBObject obj)
        {
            IKBService kbserv = UIServices.KB;
            KBModel kbmod = kbserv.CurrentModel;

            List<Variable> variables = new List<Variable>();
            VariablesPart vp = obj.Parts.Get<VariablesPart>();
            if (vp != null)
            {
                foreach (Variable v in vp.Variables)
                {
                    if ((!v.IsStandard) && (v.Type != eDBType.GX_USRDEFTYP) && (v.Type != eDBType.GX_SDT) && (v.Type != eDBType.GX_EXTERNAL_OBJECT) )
                    {
                        if (v.AttributeBasedOn != null && !v.Name.ToLower().Contains(v.AttributeBasedOn.Name.ToLower()))
                        {
                            AddVariableToFile(v);
                            variables.Add(v);
                        }
                        else
                        {
                            if (v.DomainBasedOn != null && !v.Name.ToLower().Contains(v.DomainBasedOn.Name.ToLower()))
                            {
                                AddVariableToFile(v);
                                variables.Add(v);
                            }
                            else
                            {
                                if ((v.AttributeBasedOn == null) && (v.DomainBasedOn == null))
                                {
                                    AddVariableToFile(v);
                                    variables.Add(v);
                                }
                            }
                        }



                    }
                }
            }
            return variables;
        }

        private static void AddVariableToFile(Variable v)
        {
            string attBAux = v.AttributeBasedOn == null ? "" : v.AttributeBasedOn.Name;
            string domBAux = v.DomainBasedOn == null ? "" : v.DomainBasedOn.Name;
            string varaux = v.Name + ":" + v.Type.ToString() + ":" + v.Length.ToString() + ":" + attBAux + ":" + domBAux;
            varaux = varaux + "->" + varaux; 
                   
            Functions.AddLine("RenameVariables.txt", varaux);
        }

        private static void MarkReachables(IOutputService output, KBObject obj, KBObjectCollection reachablesObjects)
        {
            reachablesObjects.Add(obj);

            if (Utility.IsMain(obj))
                output.AddLine(obj.Name);
            

            foreach (EntityReference reference in obj.GetReferences(LinkType.UsedObject))
            {
                KBObject objRef = KBObject.Get(obj.Model, reference.To);

                if ((objRef != null) && !reachablesObjects.Contains(objRef))
                {
                    if (obj is Procedure && (objRef is Transaction || objRef is WebPanel))
                                 output.AddWarningLine(obj.Name + " " + objRef.Name);
                    MarkReachables(output, objRef, reachablesObjects);
                }

            }
        }

        /// <summary>
        /// Crea un procedure con todos los SDT seleccionados por el usuario. 
        /// </summary>
        public static void CreateProcedureSDT()
        {
            IKBService kB = UIServices.KB;
            if (kB != null && kB.CurrentModel != null)
            {
                SelectObjectOptions selectObjectOption = new SelectObjectOptions
                {
                    MultipleSelection = true
                };
                KBModel kbModel = UIServices.KB.CurrentModel;

                Artech.Genexus.Common.Objects.Procedure proc = new Artech.Genexus.Common.Objects.Procedure(kbModel);
                string procName = "SDTForceGenerate";

                proc.Name = procName;
                proc.ProcedurePart.Source = "// Generated by KBDoctor, to generate SDT source";
                proc.SetPropertyValue("IsMain", true);
                proc.Save();

                //MUESTRO LOS SDT A REGENERAR
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<SDT>());
                foreach (KBObject kBObject in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
                {
                    SDT sdtObj = (SDT)kBObject;
                    CodeGeneration.AddSDTVariable(kbModel, proc, sdtObj);
                    Application.DoEvents();
                }

                proc.Save();


                //Para cada uno de los generadores del environment, genero el proc con los SDT.  

                GxModel gm = UIServices.KB.WorkingEnvironment.TargetModel.GetAs<GxModel>();

#if GX16
                foreach (var gen in gm.Environments)
#else
                foreach (var gen in gm.Generators)
#endif
                {
                    int generator = gen.Generator;

                    KBObject copy = BLServices.KnowledgeManager.Clone(proc);
                    copy.Name = procName + "_" + generator.ToString();
#if GX16
                    copy.SetPropertyValue(Artech.Genexus.Common.Properties.TRN.Generator, new EnvironmentCategoryReference { Definition = gen });
#else
                    copy.SetPropertyValue(Artech.Genexus.Common.Properties.TRN.Generator, new GeneratorCategoryReference { Definition = gen });
#endif
                    UIServices.Objects.Save(copy);

                    GenexusUIServices.Build.Rebuild(copy.Key);

                    do
                    {
                        Application.DoEvents();

                    } while (GenexusUIServices.Build.IsBuilding);
                    copy.Delete();
                }
                proc.Delete();

            }
        }

     

        /// <summary>
        /// Search and replace text in objects
        /// </summary>
        public static void SearchAndReplace() //SearchAndReplace()
        {
            IKBService kB = UIServices.KB;
            IOutputService output = CommonServices.Output;

            string mensaje = "";
            string title = "Search and replace";
            output.StartSection("KBDoctor",title);
            if (kB != null && kB.CurrentModel != null)
            {

                PromptDescription pd;
                DialogResult dr;
                mensaje = "Find";

                pd = new PromptDescription(mensaje);
                dr = pd.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    string txtfind = pd.Description;
                    mensaje = "Replace with";
                    pd = new PromptDescription(mensaje);
                    dr = pd.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        string txtreplace = pd.Description;
                        SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                        selectObjectOption.MultipleSelection = true;
                        KBModel kbModel = UIServices.KB.CurrentModel;

                        int objcambiados = 0;
                        int objtotales = 0;
                        //SELECCIONO OBJETOS A BUSCAR
                        selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());
                        selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());
                        selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WebPanel>());
                        selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<WorkPanel>());

                        foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
                        {
                            objtotales += 1;
                            Application.DoEvents();
                            if ((objtotales % 100) == 0)
                            {

                                KBDoctorOutput.Message("Searching in " + objtotales + " objects ");
                            }

                            StringBuilder buffer = new StringBuilder();
                            using (TextWriter writer = new StringWriter(buffer))
                                obj.Serialize(writer);

                            string objxml = buffer.ToString();
                            string newobjxml = objxml.Replace(txtfind, txtreplace, StringComparison.InvariantCultureIgnoreCase);

                            using (StringReader strReader = new StringReader(newobjxml))
                            using (XmlTextReader reader = new XmlTextReader(strReader))
                                BLServices.KnowledgeManager.ImportInObject(reader, obj);
                            if (objxml != newobjxml)
                            {
                                try
                                {
                                    obj.Save();
                                    KBDoctorOutput.Message("Changed >> '" + txtfind + "' to '" + txtreplace + "' in object " + obj.Name);
                                    objcambiados += 1;
                                }
                                catch (Exception e)
                                {
                                    if (e.InnerException == null)
                                        output.AddErrorLine(e.Message);
                                    else
                                        output.AddErrorLine(e.Message + " - " + e.InnerException);
                                };
                            }

                        }
                        title = "Changed objects " + objcambiados.ToString();
                        output.EndSection("KBDoctor", title, true);
                    }
                }
            }

        }

        
        public static string Replace(this string str, string old, string @new, StringComparison comparison)
        {
            @new = @new ?? "";
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(old) )
                return str;
            string @aux = "";
            for (int i = 0; i< @new.Length ;i++ )
                @aux += "°";

            int foundAt;
            while ((foundAt = str.IndexOf(old, 0, comparison )) != -1)
                str = str.Remove(foundAt, old.Length).Insert(foundAt, @aux);
            return str.Replace(@aux, @new);
        }
        

        public static void ObjectsReferenced()
        {

            // Object with parm() rule without in: out: or inout:
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Object referenced by object ";

            output.StartSection("KBDoctor",title);
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);


                SelectObjectOptions selectObjectOption = new SelectObjectOptions
                {
                    MultipleSelection = false
                };
                KBModel kbModel = UIServices.KB.CurrentModel;
                KBObjectCollection objRefCollection = new KBObjectCollection();

                foreach (KBObject obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))

                {
                    if (obj != null)
                    {
                        title += obj.Name + "-" + obj.Description;
                        writer.AddHeader(title);
                        writer.AddTableHeader(new string[] { "Type", "Object", "Description", "Commit on Exit", "Update DB?", "Commit in Source", "Do Commit", "Timestamp", "Last Update" });

                        MarkReachables(output, obj, objRefCollection);
                    }
                }


                string commitOnExit = "";
                string commitInSource = "";
                string UpdateInsertDelete = "";
                string doCommit = "";
                foreach (KBObject objRef in objRefCollection)
                {
                    if (objRef is Procedure)
                    {
                        object aux = objRef.GetPropertyValue("CommitOnExit");
                        if (aux != null)
                        {
                            commitOnExit = aux.ToString() == "Yes" ? "YES" : " ";

                        }
                        UpdateInsertDelete = ObjectUpdateDB(objRef) ? "YES" : "";

                        Procedure prc = (Procedure)objRef;
                        if (Functions.ExtractComments(prc.ProcedurePart.Source.ToString().ToUpper()).Contains("COMMIT"))
                            commitInSource = "YES";
                        else
                            commitInSource = "";

                        if (((commitOnExit == "YES") && (UpdateInsertDelete == "YES")) || (commitInSource == "YES"))
                            doCommit = "YES";
                        else
                            doCommit = "";

                        writer.AddTableData(new string[] { "Procedure", Functions.linkObject(objRef), objRef.Description, commitOnExit, UpdateInsertDelete, commitInSource, doCommit, objRef.Timestamp.ToString(), objRef.LastUpdate.ToString() });

                    }
                    else
                    {
                        string objType = objRef.TypeName.ToString();

                        if (!(new[] { "Attribute", "Domain", "ExternalObject", "ThemeClass", "Image", "Table" , "SDT", "WorkWithPlus", "WorkWith" }.Contains(objType)))
                        {
                            output.AddErrorLine(objRef.Name + " " + objRef.TypeName.ToString());
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

        public static Boolean ObjectUpdateDB(KBObject obj)
        {
            KBModel model = obj.Model;
            IList<KBObject> tableUpdInsDel = (from r in model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                                              where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                              where (ReferenceTypeInfo.HasUpdateAccess(r.LinkTypeInfo) || ReferenceTypeInfo.HasDeleteAccess(r.LinkTypeInfo) || ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo))
                                              select model.Objects.Get(r.From)).ToList();
            return (tableUpdInsDel.Count > 0) ;
        }

        /// <summary>
        /// Cambia los objetos que tienen source o eventos. 
        /// </summary>
        /// <param name="obj">Objeto a cambiar</param>
        /// <param name="txtfind">texto a buscar</param>
        /// <param name="txtreplace">texto a remplazar</param>
        public static bool ReplaceEvents(KBObject obj, string txtfind, string txtreplace)
        {
            bool cambio = false;
            if ((obj is Transaction) || (obj is WebPanel) || (obj is WorkPanel))
            {
                EventsPart evPart = obj.Parts.Get<EventsPart>();
                if (evPart != null)
                {
                    string evsource = evPart.Source;

                    string source2 = Regex.Replace(evsource, txtfind, txtreplace, RegexOptions.IgnoreCase);
                    source2 = source2.Replace(txtfind.ToLower(), txtreplace);
                    source2 = source2.Replace(txtfind.ToUpper(), txtreplace);
                    if (evsource != source2)
                    {
                        evPart.Source = source2;
                        cambio = true;
                    }
                }
            }
            return cambio;
        }

        /// <summary>
        /// Cambia los objetos que tienen source o eventos. 
        /// </summary>
        /// <param name="obj">Objeto a cambiar</param>
        /// <param name="txtfind">texto a buscar</param>
        /// <param name="txtreplace">texto a remplazar</param>
        public static bool ReplaceSource(KBObject obj, string txtfind, string txtreplace)
        {
            bool cambio = false;
            if (obj is Procedure)
            {
                ProcedurePart Part = obj.Parts.Get<ProcedurePart>();
                if (Part != null)
                {
                    string source = Part.Source;
                    string source2 = source.Replace(txtfind, txtreplace);
                    source2 = source2.Replace(txtfind.ToLower(), txtreplace);
                    source2 = source2.Replace(txtfind.ToUpper(), txtreplace);

                    if (source != source2)
                    {
                        Part.Source = source2;
                        cambio = true;
                    }
                }
            }
            return cambio;
        }

        /// <summary>
        /// Cambia los objetos transaction y webpanels y los WebForms. 
        /// </summary>
        /// <param name="obj">Objeto a cambiar</param>
        /// <param name="txtfind">texto a buscar</param>
        /// <param name="txtreplace">texto a remplazar</param>
        public static bool ReplaceWebForm(KBObject obj, string txtfind, string txtreplace)
        {
            bool cambio = false;

            if ((obj is Transaction) || (obj is WebPanel))
            {
                WebFormPart wfPart = obj.Parts.Get<WebFormPart>();
                if (wfPart != null)
                {
                    XmlDocument xmldoc = wfPart.Document;

                    if (xmldoc != null)
                    {

                        string source = xmldoc.OuterXml;

                        string source2 = source.Replace(txtfind, txtreplace);
                        source2 = source2.Replace(txtfind.ToLower(), txtreplace);
                        source2 = source2.Replace(txtfind.ToUpper(), txtreplace);

                        if (source != source2)
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(source2);
                            wfPart.Document = doc;
                            cambio = true;
                        }
                    }
                }
            }
            return cambio;
        }

        internal static void HistoryGXServer()
        {
          /*
           No Implementado
             */
        }





        public static void RenameAttributesAndTables()
        {

            string message = "This option rename Objects (attributes, tables and objects) to significant name length. " + 
                Environment.NewLine + Environment.NewLine + "Do you have KB BACKUP? ";
            const string caption = "ATTENTION!!";
            var result = MessageBox.Show(message, caption,
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Exclamation);

            // If the no button was pressed ...
            if (result == DialogResult.Yes)
            {
                IKBService kbserv = UIServices.KB;
                KBModel model = kbserv.CurrentKB.DesignModel;

                int ATTNAME_LEN = model.GetPropertyValue<int>("ATTNAME_LEN");
                int TBLNAME_LEN = model.GetPropertyValue<int>("TBLNAME_LEN");
                int OBJNAME_LEN = model.GetPropertyValue<int>("OBJNAME_LEN");

                string title = "KBDoctor - Rename Objects to significant name length";
                try
                {
                    string outputFile = Functions.CreateOutputFile(kbserv, title);
                    KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                    writer.AddHeader(title);
                    writer.AddTableHeader(new string[] { "Type", "Object", "Description" });

                    IOutputService output = CommonServices.Output;
                    output.StartSection("KBDoctor", title);

                    foreach (KBObject obj in kbserv.CurrentModel.Objects.GetAll())
                    {
                        Boolean SaveObj = false;
                        if ((obj is Artech.Genexus.Common.Objects.Attribute) && (obj.Name.Length > ATTNAME_LEN))
                        {
                            KBDoctorOutput.Message( "RENAMING ATTRIBUTE " + obj.Name + " to " + obj.Name.Substring(0, ATTNAME_LEN));
                            obj.Name = obj.Name.Substring(0, ATTNAME_LEN);
                            SaveObj = true;
                        }
                        else
                        {
                            if (((obj is Table) || (obj is Index)) && (obj.Name.Length > TBLNAME_LEN))
                            {
                                KBDoctorOutput.Message( "RENAMING TABLE/INDEX " + obj.Name + " to " + obj.Name.Substring(0, TBLNAME_LEN));
                                obj.Name = obj.Name.Substring(0, TBLNAME_LEN);
                                SaveObj = true;
                            }
                            else
                            {
                                if ((obj.Name.Length > OBJNAME_LEN) && ObjectsHelper.isGeneratedbyPattern(obj))
                                {
                                    KBDoctorOutput.Message( "RENAMING OBJECT " + obj.Name + " to " + obj.Name.Substring(0, OBJNAME_LEN));
                                    obj.Name = obj.Name.Substring(0, OBJNAME_LEN);
                                    SaveObj = true;
                                }
                            }
                        }
                        if (SaveObj)
                        {
                            string attNameLink = Functions.linkObject(obj);
                            writer.AddTableData(new string[] { attNameLink, obj.Description, obj.TypeDescriptor.Name });
                            try
                            {
                                obj.Save();
                            }
                            catch (Exception e)
                            {
                                KBDoctorOutput.Message( "ERROR saving  .. " + obj.Name + " - " + e.Message);
                            }
                        }
                    }
                    writer.AddFooter();
                    writer.Close();

                    KBDoctorHelper.ShowKBDoctorResults(outputFile);
                    bool success = true;
                    KBDoctorOutput.EndSection( title, success);
                }
                catch
                {
                    bool success = false;
                    KBDoctor.KBDoctorOutput.EndSection(title, success);
                }
            }
        }

        public static void RemoveAttributeWithoutTable()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Remove attributes without table";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                IOutputService output = CommonServices.Output;
                output.StartSection("KBDoctor", title);

                List<string[]> lineswriter;
                KBDoctorCore.Sources.API.RemoveAttributesWithoutTable(kbserv.CurrentModel, output, out lineswriter);
                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);

                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "", "Attribute", "Description", "Data type" });
                foreach (string[] line in lineswriter)
                {
                    writer.AddTableData(line);
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
















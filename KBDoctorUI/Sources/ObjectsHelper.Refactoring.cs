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
                if (obj is Artech.Genexus.Common.Objects.SDT)
                    continue;
                if (obj is GeneratorCategory)
                    continue;
                if (obj is DataStoreCategory)
                    continue;

                string objName = "KBD_" + obj.Guid.ToString().Replace("-", "");
                obj.Name = objName;
                obj.Description = objName;

                try
                {
                    obj.Save();
                }
                catch
                    {
                    }

            }

        }

        // TODO: Sin referencias textuales encontradas; revisar antes de eliminar.
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


                KBDoctorOutput.StartSection(title);

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
                        int total = updaters + inserters + deleters + readers;

                        List<KBObject> outmodule = (from r in model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                                                    where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                                    where ReferenceTypeInfo.HasUpdateAccess(r.LinkTypeInfo) || ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo)
                                                       || ReferenceTypeInfo.HasDeleteAccess(r.LinkTypeInfo) || ReferenceTypeInfo.HasReadAccess(r.LinkTypeInfo)
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
                KBDoctorOutput.EndSection(title, success);
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

        // TODO: Sin referencias textuales encontradas; revisar antes de eliminar.
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

        // TODO: Sin referencias textuales encontradas; revisar antes de eliminar.
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

                KBDoctorOutput.StartSection(title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
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
                    KBDoctorOutput.Message("Iteraci�n: " + i.ToString());
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
                KBDoctorOutput.EndSection(title, success);
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

                KBDoctorOutput.StartSection(title);

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
                KBDoctorOutput.EndSection(title, success);
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

                KBDoctorOutput.StartSection(title);

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
                KBDoctorOutput.EndSection(title, success);
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

                KBDoctorOutput.StartSection(title);

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
                KBDoctorOutput.EndSection(title, success);
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

        // TODO: Sin referencias textuales encontradas; revisar antes de eliminar.
        private static string ReplaceCallForUDP(KBObject obj)
        {
            IOutputService output = CommonServices.Output;

            string callers = "";

            foreach (EntityReference reference in obj.GetReferencesTo(LinkType.UsedObject))
            {

                KBObject objRef = KBObject.Get(obj.Model, reference.From);


                string source = Functions.ObjectSourceUpper(objRef);
                source = Functions.RemoveEmptyLines(source);

                string sourceWOComments = Functions.ExtractComments(source);
                sourceWOComments = sourceWOComments.Replace("\t", "");
                sourceWOComments = sourceWOComments.Replace(" ", "");

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

        // TODO: Sin referencias textuales encontradas; revisar antes de eliminar.
        private static string CompareCallParameters(KBObject obj)
        {
            IOutputService output = CommonServices.Output;
            string source = ObjectSource(obj);
            source = Functions.RemoveEmptyLines(source);
            string sourceWOComments = Functions.ExtractComments(source);

            string lista = "";

            foreach (EntityReference reference in obj.GetReferences())
            {
                KBObject objRef = KBObject.Get(obj.Model, reference.To);


                if (IsCallalable(objRef))
                {
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
                                line.Replace(objRef.Name, "�", StringComparison.CurrentCultureIgnoreCase);

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
                    if (substring.StartsWith("&"))
                    {
                        lista.Add(TypeOfVariable(substring.Replace("&", ""), obj));
                    }
                    else
                    {
                        IKBService kbserv = UIServices.KB;
                        foreach (KBObject att in kbserv.CurrentModel.Objects.GetByName("Attributes", new Guid("adbb33c9-0906-4971-833c-998de27e0676"), substring))
                        {
                            if ((att != null) && (att is Artech.Genexus.Common.Objects.Attribute))
                            {
                                string type = (TypeOfAttribute((Artech.Genexus.Common.Objects.Attribute)att));
                                lista.Add(type);
                                if (type == "")
                                {
                                    KBDoctorOutput.Message( "__________no se pudo______________" + substring);
                                }
                            }
                        }

                    }

                }
            }
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

    }
}

using Artech.Architecture.Common;
using Artech.Architecture.Common.Collections;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Helpers;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Services;
using Artech.Udm.Framework.References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Artech.Genexus.Common.Helpers;
using Artech.Udm.Framework;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{

    static class ModulesHelper
    {
        private const string CallableFromWorkflowProperty = "PWFCallable";
        private const string ObjectVisibilityProperty = "ObjectVisibility";

        private sealed class ModuleMoveRequest
        {
            public int LineNumber { get; set; }
            public string ModuleQualifiedName { get; set; }
            public string ObjectQualifiedName { get; set; }
            public Guid ModuleGuid { get; set; }
            public Guid ObjectGuid { get; set; }
        }

        public static string ObjectModuleName(KBObject obj)
        {
            string modulename = "";
            if (obj is null)
                return "";
            if (obj is Table)
                modulename = TablesHelper.TableModule(obj.Model, (Table)obj).Name;
            else
                modulename = obj.Module.Name;
            return modulename;
        }

        public static bool MoveUserObjectToModule(KBObject obj, string moduleName)
        {
            string message;
            return MoveUserObjectToModule(obj, moduleName, out message);
        }

        public class ModuleSuggestion
        {
            public Module Module { get; private set; }
            public int IntraModuleWeight { get; private set; }
            public int InterModuleWeight { get; private set; }
            public int TotalWeight { get; private set; }
            public int Score { get; private set; }
            public int DeltaScore { get; private set; }
            public bool IsCurrentModule { get; private set; }

            public ModuleSuggestion(Module module, int intraModuleWeight, int interModuleWeight, int currentScore, bool isCurrentModule)
            {
                Module = module;
                IntraModuleWeight = intraModuleWeight;
                InterModuleWeight = interModuleWeight;
                TotalWeight = intraModuleWeight + interModuleWeight;
                Score = intraModuleWeight - interModuleWeight;
                DeltaScore = Score - currentScore;
                IsCurrentModule = isCurrentModule;
            }
        }

        public static ModuleSuggestion SuggestModuleForObject(KBObject obj)
        {
            List<ModuleSuggestion> suggestions = SuggestModulesForObject(obj);
            if (suggestions.Count == 0)
            {
                return null;
            }

            return suggestions[0];
        }

        public static List<ModuleSuggestion> SuggestModulesForObject(KBObject obj)
        {
            List<ModuleSuggestion> suggestions = new List<ModuleSuggestion>();

            if (!CanSuggestModuleForObject(obj))
            {
                return suggestions;
            }

            List<WeightedModuleDependency> dependencies = GetWeightedModuleDependencies(obj);
            int currentScore = ScoreModuleCandidate(obj.Module, dependencies);

            foreach (Module module in Module.GetAll(obj.Model))
            {
                int intraModuleWeight = 0;
                int interModuleWeight = 0;

                foreach (WeightedModuleDependency dependency in dependencies)
                {
                    if (dependency.Module == module)
                    {
                        intraModuleWeight += dependency.Weight;
                    }
                    else
                    {
                        interModuleWeight += dependency.Weight;
                    }
                }

                suggestions.Add(new ModuleSuggestion(module, intraModuleWeight, interModuleWeight, currentScore, obj.Module == module));
            }

            return suggestions
                .OrderByDescending(s => s.Score)
                .ThenByDescending(s => s.IntraModuleWeight)
                .ThenBy(s => s.InterModuleWeight)
                .ThenByDescending(s => s.IsCurrentModule)
                .ThenBy(s => s.Module.QualifiedName.ToString())
                .ToList();
        }

        private static bool CanSuggestModuleForObject(KBObject obj)
        {
            return obj != null
                && Utility.IsUserEditableObject(obj)
                && !(obj is Table)
                && Utility.HasModule(obj)
                && !ObjectsHelper.isGeneratedbyPattern(obj);
        }

        private static int ScoreModuleCandidate(Module module, List<WeightedModuleDependency> dependencies)
        {
            int intraModuleWeight = 0;
            int interModuleWeight = 0;

            foreach (WeightedModuleDependency dependency in dependencies)
            {
                if (dependency.Module == module)
                {
                    intraModuleWeight += dependency.Weight;
                }
                else
                {
                    interModuleWeight += dependency.Weight;
                }
            }

            return intraModuleWeight - interModuleWeight;
        }

        private static List<WeightedModuleDependency> GetWeightedModuleDependencies(KBObject obj)
        {
            List<WeightedModuleDependency> dependencies = new List<WeightedModuleDependency>();

            foreach (EntityReference reference in obj.GetReferences())
            {
                KBObject objRef = KBObject.Get(obj.Model, reference.To);
                if (objRef != null && objRef != obj && GraphHelper.IncludedInGraph(objRef))
                {
                    AddWeightedModuleDependency(dependencies, ModuleOfObject(objRef), GraphHelper.ReferenceWeight(obj, objRef));
                }
            }

            foreach (EntityReference reference in obj.GetReferencesTo())
            {
                KBObject objRef = KBObject.Get(obj.Model, reference.From);
                if (objRef != null && objRef != obj && GraphHelper.IncludedInGraph(objRef))
                {
                    AddWeightedModuleDependency(dependencies, ModuleOfObject(objRef), GraphHelper.ReferenceWeight(objRef, obj));
                }
            }

            return dependencies;
        }

        private static void AddWeightedModuleDependency(List<WeightedModuleDependency> dependencies, Module module, int weight)
        {
            if (module == null || weight <= 0)
            {
                return;
            }

            dependencies.Add(new WeightedModuleDependency(module, weight));
        }

        private static Module ModuleOfObject(KBObject obj)
        {
            if (obj is Table)
            {
                return TablesHelper.TableModule(obj.Model, (Table)obj);
            }

            if (!Utility.HasModule(obj))
            {
                return null;
            }

            return obj.Module;
        }

        private class WeightedModuleDependency
        {
            public Module Module { get; private set; }
            public int Weight { get; private set; }

            public WeightedModuleDependency(Module module, int weight)
            {
                Module = module;
                Weight = weight;
            }
        }

        public static bool MoveUserObjectToModule(KBObject obj, string moduleName, out string message)
        {
            message = "";

            if (obj == null)
            {
                message = "Object not found.";
                return false;
            }

            if (moduleName == null || moduleName.Trim() == "")
            {
                message = "Module name is empty.";
                return false;
            }

            if (!Utility.IsUserEditableObject(obj))
            {
                message = "Object is read-only.";
                return false;
            }

            if (obj is Table)
            {
                message = "Tables are not moved by this function.";
                return false;
            }

            if (!Utility.HasModule(obj))
            {
                message = "Object does not support module assignment.";
                return false;
            }

            if (ObjectsHelper.isGeneratedbyPattern(obj))
            {
                message = "Objects generated by pattern are not moved by this function.";
                return false;
            }

            Module newModule = FindModuleByName(obj.Model, moduleName);
            if (newModule == null)
            {
                message = "Module '" + moduleName + "' was not found.";
                return false;
            }

            if (obj.Module == newModule || SameModuleName(obj.Module, moduleName))
            {
                message = "Object is already in module '" + moduleName + "'.";
                return false;
            }

            Module previousModule = obj.Module;
            try
            {
                obj.Module = newModule;
                obj.Save();
                message = "Object moved to module '" + newModule.QualifiedName.ToString() + "'.";
                return true;
            }
            catch (Exception e)
            {
                obj.Module = previousModule;
                message = "Could not move object to module '" + newModule.QualifiedName.ToString() + "': " + e.Message;
                return false;
            }
        }

        private static Module FindModuleByName(KBModel model, string moduleName)
        {
            string normalizedName = moduleName.Trim();
            Module moduleBySimpleName = null;

            foreach (Module module in Module.GetAll(model))
            {
                if (String.Equals(module.QualifiedName.ToString(), normalizedName, StringComparison.OrdinalIgnoreCase))
                {
                    return module;
                }

                if (moduleBySimpleName == null && String.Equals(module.Name, normalizedName, StringComparison.OrdinalIgnoreCase))
                {
                    moduleBySimpleName = module;
                }
            }

            return moduleBySimpleName;
        }

        private static bool SameModuleName(Module module, string moduleName)
        {
            if (module == null)
            {
                return false;
            }

            string normalizedName = moduleName.Trim();
            return String.Equals(module.QualifiedName.ToString(), normalizedName, StringComparison.OrdinalIgnoreCase)
                || String.Equals(module.Name, normalizedName, StringComparison.OrdinalIgnoreCase);
        }

        public static void MarkPublicObjects()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;
            string title = "KBDoctor - Mark Public Object";
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);
                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Type", "Description", "Visibility" });

                MakeAllObjectPublic(kbserv, output);

                foreach (KBObject obj in Utility.EditableObjects(kbserv.CurrentModel.Objects.GetAll()))
                {
                    KBDoctorOutput.Message( "Object " + obj.Name);
                    ICallableObject callableObject = obj as ICallableObject;
                    if (((callableObject != null) || obj is ExternalObject || obj is SDT || obj is DataSelector) && (!(obj is Transaction)))
                    {
                        ObjectVisibility objVisibility = obj.GetPropertyValue<ObjectVisibility>("ObjectVisibility");

                        ObjectVisibility newObjVisibility = RecoverVisibility(obj);

                        if (objVisibility != newObjVisibility)
                        {
                            obj.SetPropertyValue("ObjectVisibility", newObjVisibility);
                            Utility.SaveObject(output, obj);
                            string objNameLink = Utility.linkObject(obj);
                            writer.AddTableData(new string[] { objNameLink, obj.TypeDescriptor.Name, obj.Description, newObjVisibility.ToString() });
                            KBDoctorOutput.Message( "....Change Object " + obj.Name);
                        }
                    }
                }
                KBDoctorOutput.Message( "");
                KBDoctorOutput.EndSection(title, success);
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

        public static void MakeWorkflowObjectsPublic()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Hacer publicos objetos de workflow";
            string outputFile = Functions.CreateOutputFile(kbserv, title);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new[] { "Object", "Type", "Module", "Callable from Workflow", "Previous Visibility", "New Visibility", "Result" });

            KBDoctorOutput.StartSection(title);

            List<KBObject> objects = new List<KBObject>();
            objects.AddRange(Utility.EditableObjects(Procedure.GetAll(kbserv.CurrentModel)));
            objects.AddRange(Utility.EditableObjects(WebPanel.GetAll(kbserv.CurrentModel)));
            List<KBObject> workflowObjects = objects.Where(IsCallableFromWorkflow).ToList();

            int changed = 0;
            int alreadyPublic = 0;
            int failed = 0;

            for (int index = 0; index < workflowObjects.Count; index++)
            {
                KBObject obj = workflowObjects[index];
                string progress = "(" + (index + 1).ToString() + "/" + workflowObjects.Count.ToString() + ") ";
                string previousVisibility = "";
                string newVisibility = "";
                string result;

                try
                {
                    ObjectVisibility visibility = obj.GetPropertyValue<ObjectVisibility>(ObjectVisibilityProperty);
                    previousVisibility = visibility.ToString();
                    if (visibility == ObjectVisibility.Public)
                    {
                        alreadyPublic += 1;
                        newVisibility = visibility.ToString();
                        result = "Already public";
                    }
                    else
                    {
                        obj.SetPropertyValue(ObjectVisibilityProperty, ObjectVisibility.Public);
                        obj.Save();
                        changed += 1;
                        newVisibility = ObjectVisibility.Public.ToString();
                        result = "Changed";
                    }

                    output.AddLine(progress + obj.QualifiedName.ToString() + ": " + result);
                }
                catch (Exception e)
                {
                    failed += 1;
                    result = "Error: " + e.Message;
                    output.AddErrorLine(progress + obj.QualifiedName.ToString() + ": " + e.ToString());
                }

                writer.AddTableData(new[]
                {
                    Functions.linkObject(obj),
                    obj.TypeDescriptor.Name,
                    obj.Module == null ? "" : obj.Module.Name,
                    obj.GetPropertyValueString(CallableFromWorkflowProperty),
                    previousVisibility,
                    newVisibility,
                    result
                });
            }

            writer.AddFooter();
            writer.Close();

            output.AddLine("");
            output.AddLine("Workflow objects: " + workflowObjects.Count.ToString()
                + ". Changed: " + changed.ToString()
                + ". Already public: " + alreadyPublic.ToString()
                + ". Failed: " + failed.ToString() + ".");
            output.AddLine("Report: " + outputFile);
            KBDoctorHelper.ShowKBDoctorResults(outputFile);
            KBDoctorOutput.EndSection(title, failed == 0);
        }

        private static bool IsCallableFromWorkflow(KBObject obj)
        {
            if (!obj.ContainsPropertyDefinition(CallableFromWorkflowProperty))
            {
                return false;
            }

            object value = obj.GetPropertyValue(CallableFromWorkflowProperty);
            if (value is bool booleanValue)
            {
                return booleanValue;
            }

            string text = obj.GetPropertyValueString(CallableFromWorkflowProperty);
            return string.Equals(text, "True", StringComparison.OrdinalIgnoreCase)
                || string.Equals(text, "Yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(text, "1", StringComparison.OrdinalIgnoreCase);
        }

        public static void GeneratedObjectsNotReachableFromDeploymentUnits()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Generated objects not reachable from deployment units";
            string outputFile = Functions.CreateOutputFile(kbserv, title);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new[] { "Type", "Object", "Module", "Generated by Pattern", "Is Main", "Referenced By", "Reason" });

            KBDoctorOutput.StartSection(title);

            try
            {
                Dictionary<Guid, string> reachableByDeploymentUnit = new Dictionary<Guid, string>();
                List<DeploymentUnitCategory> deploymentUnits = DeploymentUnitCategory.GetAll(kbserv.CurrentModel).ToList();

                output.AddLine("Deployment units: " + deploymentUnits.Count.ToString());
                foreach (DeploymentUnitCategory deploymentUnit in deploymentUnits)
                {
                    output.AddLine("Processing deployment unit: " + deploymentUnit.QualifiedName.ToString());
                    MarkDeploymentUnitReachables(deploymentUnit, reachableByDeploymentUnit);
                }

                int candidates = 0;
                int notReachable = 0;
                foreach (KBObject obj in Utility.EditableObjects(kbserv.CurrentModel.Objects.GetAll()))
                {
                    if (!IsGeneratedCandidateForDeploymentReachability(obj))
                    {
                        continue;
                    }

                    candidates += 1;
                    if (reachableByDeploymentUnit.ContainsKey(obj.Guid))
                    {
                        continue;
                    }

                    notReachable += 1;
                    writer.AddTableData(new[]
                    {
                        obj.TypeDescriptor.Name,
                        Functions.linkObject(obj),
                        obj.Module == null ? "" : obj.Module.Name,
                        ObjectsHelper.isGeneratedbyPattern(obj).ToString(),
                        Utility.IsMain(obj).ToString(),
                        obj.GetReferencesTo(LinkType.UsedObject).Count().ToString(),
                        "Generated object not reachable from any Deployment Unit"
                    });
                }

                writer.AddFooter();
                writer.Close();
                output.AddLine("");
                output.AddLine("Generated candidates: " + candidates.ToString()
                    + ". Reachable from deployment units: " + reachableByDeploymentUnit.Count.ToString()
                    + ". Not reachable: " + notReachable.ToString() + ".");
                output.AddLine("Report: " + outputFile);
                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                KBDoctorOutput.EndSection(title, true);
            }
            catch (Exception e)
            {
                writer.AddFooter();
                writer.Close();
                output.AddErrorLine(e.ToString());
                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                KBDoctorOutput.EndSection(title, false);
            }
        }

        private static void MarkDeploymentUnitReachables(DeploymentUnitCategory deploymentUnit, Dictionary<Guid, string> reachableByDeploymentUnit)
        {
            if (deploymentUnit.DeploymentUnitDefinition == null)
            {
                return;
            }

            foreach (DefinitionMember member in deploymentUnit.DeploymentUnitDefinition.Members)
            {
                MarkReachableFromDeploymentUnit(member.Object, deploymentUnit.Name, reachableByDeploymentUnit);
            }

            foreach (EntityReference reference in deploymentUnit.DeploymentUnitDefinition.GetPartReferences())
            {
                KBObject referencedObject = KBObject.Get(deploymentUnit.Model, reference.To);
                MarkReachableFromDeploymentUnit(referencedObject, deploymentUnit.Name, reachableByDeploymentUnit);
            }
        }

        private static void MarkReachableFromDeploymentUnit(KBObject obj, string deploymentUnitName, Dictionary<Guid, string> reachableByDeploymentUnit)
        {
            if (obj == null)
            {
                return;
            }

            if (reachableByDeploymentUnit.ContainsKey(obj.Guid))
            {
                return;
            }

            reachableByDeploymentUnit[obj.Guid] = deploymentUnitName;

            foreach (EntityReference reference in obj.GetReferences())
            {
                KBObject referencedObject = KBObject.Get(obj.Model, reference.To);
                MarkReachableFromDeploymentUnit(referencedObject, deploymentUnitName, reachableByDeploymentUnit);
            }
        }

        private static bool IsGeneratedCandidateForDeploymentReachability(KBObject obj)
        {
            if (obj == null || obj is Artech.Architecture.Common.Objects.Module || obj is Folder)
            {
                return false;
            }

            if (obj is Table || obj is Domain || obj is Artech.Genexus.Common.Objects.Attribute || obj is Image)
            {
                return false;
            }

            return ObjectsHelper.isGenerated(obj);
        }

        private static void MakeAllObjectPublic(IKBService kbserv, IOutputService output)
        {
            bool ToContinue = true;
            int cant = 0;

            do
            {
                ToContinue = false;
                cant = 1;
                foreach (KBObject obj in Utility.EditableObjects(kbserv.CurrentModel.Objects.GetAll()))
                {
                    ObjectVisibility objVisibility = obj.GetPropertyValue<ObjectVisibility>("ObjectVisibility");
                    if (objVisibility != ObjectVisibility.Public && Utility.isRunable(obj) && !(obj is Transaction))
                    {
                        obj.SetPropertyValue("ObjectVisibility", ObjectVisibility.Public);
                        Utility.SaveObject(output, obj);
                        ToContinue = true;
                        cant += 1;
                    }
                }
                KBDoctorOutput.Message("Cambio " + cant.ToString());
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

                if (objRef != null && reference.ReferenceType == ReferenceType.Hard && Utility.isRunable(obj))
                {

                    if (objRef is Artech.Genexus.Common.Objects.Attribute && obj is Procedure)
                        objVisibility = ObjectVisibility.Public;
                    else
                    {
                        if (Utility.isRunable(objRef))
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

        private const string ModularizationInfoStart = "KBDoctor Modularization Information --Start--";
        private const string ModularizationInfoEnd = "KBDoctor Modularization Information --End--";

        public static void AddModularizationInformationToObjects()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Add Modularization Information";
            bool success = true;
            int updatedObjects = 0;

            KBDoctorOutput.StartSection(title);

            try
            {
                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;

                foreach (KBObject obj in Utility.EditableObjects(UIServices.SelectObjectDialog.SelectObjects(selectObjectOption)))
                {
                    if (obj == null)
                    {
                        continue;
                    }

                    KBDoctorOutput.Message("Processing " + obj.Name);
                    DocumentationPart documentation = obj.Parts.Get<DocumentationPart>();

                    if (documentation == null)
                    {
                        output.AddWarningLine("Object " + obj.Name + " does not have documentation part.");
                        continue;
                    }

                    string currentDocumentation = documentation.Page == null ? "" : documentation.Page.Content;
                    Artech.Genexus.Common.Wiki.WikiPage page = documentation.Page ?? new Artech.Genexus.Common.Wiki.WikiPage(obj.Module);
                    page.Content = ReplaceModularizationInformation(currentDocumentation, BuildModularizationInformation(obj));
                    documentation.Page = page;
                    Functions.SaveObject(output, obj);
                    updatedObjects += 1;
                }

                KBDoctorOutput.Message("Objects updated: " + updatedObjects.ToString());
            }
            catch (Exception e)
            {
                success = false;
                output.AddErrorLine(e.Message);
            }
            finally
            {
                KBDoctorOutput.EndSection(title, success);
            }
        }

        private static string ReplaceModularizationInformation(string documentation, string modularizationInformation)
        {
            if (documentation == null)
            {
                documentation = "";
            }

            int startIndex = documentation.IndexOf(ModularizationInfoStart);
            int endIndex = documentation.IndexOf(ModularizationInfoEnd);

            if (startIndex >= 0 && endIndex >= startIndex)
            {
                endIndex += ModularizationInfoEnd.Length;
                string before = documentation.Substring(0, startIndex).TrimEnd();
                string after = documentation.Substring(endIndex).TrimStart();

                return JoinDocumentationParts(before, modularizationInformation, after);
            }

            return JoinDocumentationParts(documentation.TrimEnd(), modularizationInformation, "");
        }

        private static string JoinDocumentationParts(string before, string modularizationInformation, string after)
        {
            StringBuilder builder = new StringBuilder();

            if (!String.IsNullOrEmpty(before))
            {
                builder.AppendLine(before);
                builder.AppendLine();
            }

            builder.AppendLine(modularizationInformation);

            if (!String.IsNullOrEmpty(after))
            {
                builder.AppendLine();
                builder.Append(after);
            }

            return builder.ToString();
        }

        private static string BuildModularizationInformation(KBObject obj)
        {
            StringBuilder builder = new StringBuilder();
            string source = Functions.ObjectSourceUpper(obj);
            source = Functions.RemoveEmptyLines(source);
            string sourceWOComments = Functions.ExtractComments(source);
            sourceWOComments = Functions.RemoveEmptyLines(sourceWOComments);

            builder.AppendLine(ModularizationInfoStart);
            builder.AppendLine();
            builder.AppendLine("## KBDoctor Modularization Information");
            builder.AppendLine();
            builder.AppendLine("- **Objeto:** " + MarkdownCell(obj.QualifiedName.ToString()));
            builder.AppendLine("- **Tipo:** " + MarkdownCell(obj.TypeDescriptor.Name));
            builder.AppendLine("- **Modulo:** " + MarkdownCell(ObjectModuleName(obj)));
            builder.AppendLine();
            builder.AppendLine("### Tablas accedidas por el objeto");
            builder.AppendLine();
            builder.AppendLine("| Acceso | Tabla | Modulo |");
            builder.AppendLine("| --- | --- | --- |");
            AppendTableAccessRows(builder, "Actualizada", GetTablesByAccess(obj, TableAccessType.Update));
            AppendTableAccessRows(builder, "Borrada", GetTablesByAccess(obj, TableAccessType.Delete));
            AppendTableAccessRows(builder, "Insertada", GetTablesByAccess(obj, TableAccessType.Insert));
            AppendTableAccessRows(builder, "Leida", GetTablesByAccess(obj, TableAccessType.Read));
            builder.AppendLine();
            builder.AppendLine("### Objetos referenciados");
            builder.AppendLine();
            AppendReferenceTable(builder, GetReferencedObjects(obj), "Objeto referenciado");
            builder.AppendLine();
            builder.AppendLine("### Objetos que lo referencian");
            builder.AppendLine();
            AppendReferenceTable(builder, GetReferencingObjects(obj), "Objeto que lo referencia");
            builder.AppendLine();
            builder.AppendLine("### Metricas");
            builder.AppendLine();
            builder.AppendLine("| Metrica | Valor |");
            builder.AppendLine("| --- | ---: |");
            AppendMetricRow(builder, "Bloque de Codigo mas largo", Functions.MaxCodeBlock(sourceWOComments));
            AppendMetricRow(builder, "Numero ciclomatico del codigo", Functions.ComplexityLevel(sourceWOComments));
            AppendMetricRow(builder, "Cantidad de Controles en pantalla", CountWebFormTags(obj));
            AppendMetricRow(builder, "Cantidad de Reglas", CountRules(obj));
            AppendMetricRow(builder, "Cantidad de Conditions", CountConditions(obj));
            AppendMetricRow(builder, "Cantidad de lineas de codigo", Functions.LineCount(source));
            builder.AppendLine();
            builder.AppendLine(ModularizationInfoEnd);

            return builder.ToString().TrimEnd();
        }

        private static void AppendTableAccessRows(StringBuilder builder, string access, IList<TableAccessInfo> values)
        {
            if (values.Count == 0)
            {
                builder.AppendLine("| " + MarkdownCell(access) + " | None |  |");
                return;
            }

            foreach (TableAccessInfo value in values)
            {
                builder.AppendLine("| " + MarkdownCell(access) + " | " + MarkdownCell(value.TableName) + " | " + MarkdownCell(value.ModuleName) + " |");
            }
        }

        private static void AppendReferenceTable(StringBuilder builder, IList<ReferenceInfo> values, string objectHeader)
        {
            builder.AppendLine("| Tipo de referencia | " + MarkdownCell(objectHeader) + " | Tipo de objeto | Modulo |");
            builder.AppendLine("| --- | --- | --- | --- |");

            if (values.Count == 0)
            {
                builder.AppendLine("| None | None |  |  |");
            }
            else
            {
                foreach (ReferenceInfo value in values)
                {
                    builder.AppendLine("| " + MarkdownCell(value.ReferenceType) + " | " + MarkdownCell(value.ObjectName) + " | " + MarkdownCell(value.ObjectType) + " | " + MarkdownCell(value.ModuleName) + " |");
                }
            }
        }

        private static void AppendMetricRow(StringBuilder builder, string metric, int value)
        {
            builder.AppendLine("| " + MarkdownCell(metric) + " | " + value.ToString() + " |");
        }

        private static string MarkdownCell(string value)
        {
            if (value == null)
            {
                return "";
            }

            return value.Replace("\\", "\\\\").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private static IList<TableAccessInfo> GetTablesByAccess(KBObject obj, TableAccessType accessType)
        {
            List<TableAccessInfo> tables = new List<TableAccessInfo>();

            foreach (EntityReference reference in obj.Model.GetReferencesFrom(obj.Key, LinkType.UsedObject))
            {
                if (reference.ReferenceType != ReferenceType.WeakExternal || !ReferenceHasAccess(reference, accessType))
                {
                    continue;
                }

                KBObject referencedObject = obj.Model.Objects.Get(reference.To);
                Table table = referencedObject as Table;

                if (table != null)
                {
                    AddDistinct(tables, new TableAccessInfo(table.Name, ObjectModuleName(table)));
                }
            }

            return tables;
        }

        private static bool ReferenceHasAccess(EntityReference reference, TableAccessType accessType)
        {
            switch (accessType)
            {
                case TableAccessType.Update:
                    return ReferenceTypeInfo.HasUpdateAccess(reference.LinkTypeInfo);
                case TableAccessType.Delete:
                    return ReferenceTypeInfo.HasDeleteAccess(reference.LinkTypeInfo);
                case TableAccessType.Insert:
                    return ReferenceTypeInfo.HasInsertAccess(reference.LinkTypeInfo);
                case TableAccessType.Read:
                    return ReferenceTypeInfo.HasReadAccess(reference.LinkTypeInfo);
                default:
                    return false;
            }
        }

        private static IList<ReferenceInfo> GetReferencedObjects(KBObject obj)
        {
            List<ReferenceInfo> objects = new List<ReferenceInfo>();

            foreach (EntityReference reference in obj.GetReferences())
            {
                KBObject referencedObject = KBObject.Get(obj.Model, reference.To);

                if (IsRelevantModularizationReference(referencedObject))
                {
                    AddDistinct(objects, BuildReferenceInfo(reference, referencedObject));
                }
            }

            return objects;
        }

        private static IList<ReferenceInfo> GetReferencingObjects(KBObject obj)
        {
            List<ReferenceInfo> objects = new List<ReferenceInfo>();

            foreach (EntityReference reference in obj.GetReferencesTo())
            {
                KBObject referencingObject = KBObject.Get(obj.Model, reference.From);

                if (IsRelevantModularizationReference(referencingObject))
                {
                    AddDistinct(objects, BuildReferenceInfo(reference, referencingObject));
                }
            }

            return objects;
        }

        private static bool IsRelevantModularizationReference(KBObject obj)
        {
            return obj != null
                && !(obj is Artech.Genexus.Common.Objects.Attribute)
                && !(obj is Domain)
                && !(obj is Image)
                && !(obj is Theme)
                && !(obj is ThemeClass)
                && !(obj is GeneratorCategory)
                && !(obj is KBCategory);
        }

        private static ReferenceInfo BuildReferenceInfo(EntityReference reference, KBObject obj)
        {
            return new ReferenceInfo(FormatReferenceType(reference), obj.QualifiedName.ToString(), obj.TypeDescriptor.Name, ObjectModuleName(obj));
        }

        private static string FormatReferenceType(EntityReference reference)
        {
            return reference.ReferenceType.ToString() + " / " + reference.LinkType.ToString();
        }

        private static void AddDistinct(List<TableAccessInfo> values, TableAccessInfo value)
        {
            if (!values.Any(v => v.TableName == value.TableName && v.ModuleName == value.ModuleName))
            {
                values.Add(value);
            }
        }

        private static void AddDistinct(List<ReferenceInfo> values, ReferenceInfo value)
        {
            if (!values.Any(v => v.ReferenceType == value.ReferenceType && v.ObjectName == value.ObjectName && v.ObjectType == value.ObjectType && v.ModuleName == value.ModuleName))
            {
                values.Add(value);
            }
        }

        private static int CountRules(KBObject obj)
        {
            string rules = Functions.ObjectRulesUpper(obj);
            rules = Functions.RemoveEmptyLines(rules);

            string rulesWOComments = Functions.ExtractComments(rules);
            rulesWOComments = Functions.RemoveEmptyLines(rulesWOComments);

            return Functions.LineCount(rulesWOComments);
        }

        private static int CountConditions(KBObject obj)
        {
            ConditionsPart conditionsPart = obj.Parts.Get<ConditionsPart>();

            if (conditionsPart == null || String.IsNullOrEmpty(conditionsPart.Source))
            {
                return 0;
            }

            string conditions = Functions.RemoveEmptyLines(Functions.ExtractComments(conditionsPart.Source.ToUpper()));
            return Functions.LineCount(conditions);
        }

        private static int CountWebFormTags(KBObject obj)
        {
            int tagcant = 0;

            if (((obj is Transaction) || (obj is WebPanel) || obj is ThemeClass) && ObjectsHelper.isGenerated(obj))
            {
                WebFormPart webForm = obj.Parts.Get<WebFormPart>();

                if (webForm != null && webForm.Document != null)
                {
                    tagcant = webForm.Document.SelectNodes("//*").Count;
                }
            }

            return tagcant;
        }

        private enum TableAccessType
        {
            Update,
            Delete,
            Insert,
            Read
        }

        private class TableAccessInfo
        {
            public TableAccessInfo(string tableName, string moduleName)
            {
                TableName = tableName;
                ModuleName = moduleName;
            }

            public string TableName { get; private set; }
            public string ModuleName { get; private set; }
        }

        private class ReferenceInfo
        {
            public ReferenceInfo(string referenceType, string objectName, string objectType, string moduleName)
            {
                ReferenceType = referenceType;
                ObjectName = objectName;
                ObjectType = objectType;
                ModuleName = moduleName;
            }

            public string ReferenceType { get; private set; }
            public string ObjectName { get; private set; }
            public string ObjectType { get; private set; }
            public string ModuleName { get; private set; }
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

        public static void ListModulesStatisticsTotal()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            KBModel kbmodel = kbserv.CurrentModel;
            Module Root = kbserv.CurrentModel.GetDesignModel().RootModule;
            bool success = true;
            int objInRoot = 0;
            int objInModule = 0;
            int tblInRoot = 0;
            int tblInModule = 0;
            int objTot = 0;
            int modules = 0;
            string title = "KBDoctor - List Modules Statistics Total";
            KBDoctorOutput.StartSection(title);


            foreach (KBObject obj in Utility.EditableObjects(kbserv.CurrentModel.Objects.GetAll()))
            {
                if (Utility.HasModule(obj))
                {
                    objTot += 1;
                    if (obj is Table)
                    {
                        if (TablesHelper.TableModule(kbmodel, (Table)obj) == Root)
                            tblInRoot += 1;
                        else
                            tblInModule += 1;
                    }
                    else
                    {
                        if (obj.Module == Root)
                            objInRoot += 1;
                        else
                            objInModule += 1;

                        if (obj is Module)
                            modules += 1;

                    }
                }
            }
            int ratio = (objInRoot == 0) ? 0 : (objInModule * 100) / objInRoot;
            KBDoctorOutput.Message("# Objects: " + objTot + " in Module: " + objInModule.ToString() + " in Root: " + objInRoot.ToString() );
            KBDoctorOutput.Message("% Modularization:  " + ratio.ToString());
            KBDoctorOutput.Message("# Tables in Module: " + tblInModule.ToString() + " in Root: " + tblInRoot.ToString());


            KBDoctorOutput.EndSection(title, success);

          //  Utility.AddLineSummary("moduleStats.txt", Resumen);

        }

        public static void ListModularizationQuality()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;

            string title = "KBDoctor - List Modularization Quality (More is better)";
            KBDoctorOutput.StartSection(title);

            Dictionary<string,int> interModule = new Dictionary<string, int>();
            Dictionary<string, int> intraModule = new Dictionary<string, int>();

            KBModel  model = kbserv.CurrentModel;
            foreach (Module mdl in Module.GetAll(model))
            {
                interModule[mdl.Name] = 0;
                intraModule[mdl.Name] = 0;
            }

            foreach (KBObject objTo in Utility.EditableObjects(kbserv.CurrentModel.Objects.GetAll()))
            {
                //     KBDoctorOutput.Message(objTo.Name + ":" + objTo.TypeDescriptor.Name );
                int intraAcum = 0;
                int interAcum = 0;
                foreach (EntityReference refer in objTo.GetReferencesTo())
                {
                    KBObject objFrom = KBObject.Get(objTo.Model, refer.From);
                    if (objFrom != null )
                    {
                        if (GraphHelper.IncludedInGraph(objTo) && GraphHelper.IncludedInGraph(objFrom))
                        {
                            string mdlTo = ObjectModuleName(objTo);
                            string mdlFrom = ObjectModuleName(objFrom);
                            int weight = GraphHelper.ReferenceWeight(objFrom, objTo);

                            // KBDoctorOutput.Message(objFrom.Name + ":" + objFrom.TypeDescriptor.Name + "," + mdlFrom + "," + objTo.Name + ":" + objTo.TypeDescriptor.Name + "," + mdlTo + "," + weight.ToString());


                            if (mdlTo == mdlFrom)
                            {
                                intraModule[mdlTo] += (2 * weight);
                                intraAcum += (2 * weight);
                            }
                            else
                            {
                                interModule[mdlTo] += weight;
                                interModule[mdlFrom] += weight;
                                interAcum += (2 * weight);
                                // KBDoctorOutput.Message(objFrom.Name +":" +objFrom.TypeDescriptor.Name  +"," + mdlFrom +","+ objTo.Name + ":" + objFrom.TypeDescriptor.Name + "," + mdlTo + "," + weight.ToString());
                            }

                        }
                    }

                }
                if (GraphHelper.IncludedInGraph(objTo))
                    KBDoctorOutput.Message(objTo.Name + ":" + objTo.TypeDescriptor.Name + "," + intraAcum + "," + interAcum);

            }

            //  try
            //  {
            string outputFile = Utility.CreateOutputFile(kbserv, title);
            KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
            writer.AddHeader(title);
            writer.AddTableHeader(new string[] { "Module", "Intra", "Inter * 2", "CF", "Order" });

            double TurboMQ = 0;
            foreach (string mdl in intraModule.Keys)
            {
                if (mdl == "SD" || mdl == "Notifications" || mdl == "Media" || mdl == "Social" || mdl == "Synchronization" || mdl == "Store")
                    break;

                    //KBDoctorOutput.Message( "Calculating " + mdl + " CF");
                double cf = 0.00;
                if (interModule[mdl] > 0 || intraModule[mdl] > 0)
                {
                    cf = ((2.0 * intraModule[mdl]) / ((2.0 * intraModule[mdl]) + interModule[mdl]));
                    TurboMQ += cf;
                    int ord = (int) (cf * 1000);
                    writer.AddTableData(new string[] { mdl, intraModule[mdl].ToString(), interModule[mdl].ToString(), cf.ToString(),ord.ToString() });
                }
            }


            writer.AddTableData(new string[] { "TurboMQ = "  , "", "", TurboMQ.ToString("N" + 6) });
                KBDoctorOutput.Message( "");
                KBDoctorOutput.EndSection(title, success);


                writer.AddFooter();
                writer.Close();
                KBDoctorHelper.ShowKBDoctorResults(outputFile);
               // Utility.AddLineSummary("moduleQuality.txt", Resumen);
           // }
            /*catch
            {
                success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
            */
        }



        private static double CF(Module mdl)
        {
            foreach (KBObject obj in Utility.EditableObjects(mdl.GetAllMembers()))
            {


            }
            return 1.0;
        }

        public static void ListModulesStatistics()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;
            int objInRoot = 0;
            int objSinRoot = 0;
            string title = "KBDoctor - List Modules Statistics";
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);
                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Module", "Description", "Tables", "Public Tables", "Objects", "Public Obj", "Obj/Publ %", "In References", "Out References" });

                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Module>());
                foreach (Module mdl in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
                {

                    KBDoctorOutput.Message( mdl.Name + "....");
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
                KBDoctorOutput.Message( "");
                KBDoctorOutput.EndSection(title, success);
                int ratio = (objInRoot == 0) ? 0 : (objSinRoot * 100) / objInRoot;
                string Resumen = "Obj in Modules, Obj Root, Ratio  " + objSinRoot.ToString() + "," + objInRoot.ToString() + "," + ratio.ToString();

                writer.AddTableData(new string[] { Resumen });
                writer.AddFooter();
                writer.Close();
                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                Utility.AddLineSummary("moduleStats.txt", Resumen);
            }
            catch
            {
                success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
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
            int cantobjPub = 0;
            int cantInRef = 0;
            int cantOutRef = 0;
            foreach (KBObject obj in Utility.EditableObjects(mdl.GetAllMembers()))
            {
                cantobj += 1;
                string aux = "";
                bool isReferencedFromOutside = IsReferencedFromOutside(mdl, obj, out aux);

                ObjectVisibility objVisibility = RecoverObjectVisibility(obj);

                if (Utility.isRunable(obj))
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
            string[] mdlStats = new string[] { Utility.linkObject(mdl), mdl.Description, cntTables.ToString(), cntPublicTables.ToString(), cantobj.ToString(),  cantobjPub.ToString(), rel.ToString() + " %" , cantInRef.ToString(), cantOutRef.ToString() };

            return mdlStats;
        }

        public static void MoveTransactions()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;

            string title = "KBDoctor - Move Transaction";
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);
                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Transaction", "Description", "Table", "Most referenced in Folder" });

                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());
                foreach (Transaction trn in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
                {
                    if (!Utility.IsUserEditableObject(trn))
                    {
                        continue;
                    }


                    foreach (TransactionLevel lvl in trn.Structure.GetLevels())
                    {
                        Table tbl = lvl.AssociatedTable;
                        List<string> fldlist = MostReferencedInFolder(tbl);
                        string listatxt = "";
                        foreach (string s in fldlist)
                            listatxt += s + "<br>";

                        writer.AddTableData(new string[] { Utility.linkObject(trn), trn.Description, Utility.linkObject(tbl), listatxt });


                    }

                }


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

        internal static void DetectMavericks()
        {
            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel;
            Dictionary<string, int> objectWeight = new Dictionary<string, int>();
            KBDoctorOutput.StartSection("Detect Mavericks");

            foreach (KBObject obj in Utility.EditableObjects(model.Objects.GetAll()))
            {
               // objRefName = GraphHelper.NombreNodo(objRef);
                if (GraphHelper.IncludedInGraph(obj))
                {
                    int cantReferences = 0;
                    int totWeight = 0;
                    int cantReferencesTables = 0;
                    foreach (EntityReference r in obj.GetReferences())
                    {
                        KBObject objRef = KBObject.Get(model, r.To);
                        if ((obj != null) && (Utility.isRunable(obj)) && (obj != objRef))
                        {
                            if (GraphHelper.IncludedInGraph(objRef))
                            {
                                int weight = GraphHelper.ReferenceWeight(obj, objRef);
                                cantReferences += 1;
                                totWeight += weight;
                                if (objRef is Table)
                                {
                                    cantReferencesTables += 1;
                                }
                            }
                        }
                    }
                    objectWeight[obj.TypeDescriptor.Name + "," + obj.Name + ","
                           + totWeight.ToString() + "," + cantReferences.ToString() + "," + cantReferencesTables.ToString()] = totWeight;

                    //KBDoctorOutput.Message(obj.TypeDescriptor.Name + "," + obj.Name + "," + totWeight.ToString() + "," + cantReferences.ToString()+"," + cantReferencesTables.ToString()  );

                }
            }

            KBDoctorOutput.Message("Type,Object,Weight,#References,#Tables ");

            foreach (KeyValuePair<string,int> objweight in objectWeight.OrderByDescending(p => p.Value).Take(20))
            {
                KBDoctorOutput.Message(objweight.Key );
            }
            KBDoctorOutput.EndSection("Detect Mavericks",true);

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
            KBDoctorOutput.Message(" ");
            KBDoctorOutput.Message("============> " + tbl.Name);

            list.Sort();
            foreach (string s in list)
                KBDoctorOutput.Message(s);
            return list;
        }

        public static void ListModulesErrors()
        {
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            bool success = true;

            string title = "KBDoctor - List Modules Errors";
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);
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
                        KBDoctorOutput.Message( mdl.Name + "....");
                        string[] mdlStat = ModuleStats((Module)mdl);

                        writer.AddTableData(mdlStat);
                    }

                }
                KBDoctorOutput.Message( "");
                KBDoctorOutput.EndSection(title, success);

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

        private static string[] ModuleStats(Module mdl)
        {
            IOutputService output = CommonServices.Output;

            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            string listPubObjNotReferenced = "";
            string listPirvateObjReferenced = "";
            string ListobjOutsideModuleAccessPrivateTable = "";

            foreach (KBObject obj in Utility.EditableObjects(mdl.GetAllMembers()))
            {

                string aux = "";
                bool isReferencedFromOutside = IsReferencedFromOutside(mdl, obj, out aux);

                ObjectVisibility objVisibility = RecoverObjectVisibility(obj);

                if (objVisibility == ObjectVisibility.Public && Utility.isRunable(obj) && !(obj is Transaction))
                {

                    if (!isReferencedFromOutside)
                    {
                        listPubObjNotReferenced += Utility.linkObject(obj) + " ";
                    }
                }


                // Por un error de GX, no se listan los SDT pues todos quedan como publicos aunque esten marcados como privados.
                if (objVisibility == ObjectVisibility.Private && isReferencedFromOutside && !(obj is SDT) && !(obj is Folder))
                {
                    listPirvateObjReferenced += Utility.linkObject(obj) + " ";
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
                         listPubObjNotReferenced += Utility.linkObject((KBObject)tbl) + " ";
                      }

                string objUpdateOutsideModuleListTable = "";
                foreach (KBObject objUpdateOutsideModule in TablesHelper.ObjectsUpdateTableOutsideModule(kbModel, tbl))
                {

                    objUpdateOutsideModuleListTable += Utility.linkObject(objUpdateOutsideModule) + " ";
                }
                if (objUpdateOutsideModuleListTable != "")
                    objUpdateOutsideModuleList += tbl.Name + "  (" + objUpdateOutsideModuleListTable + ")<BR>";

                //Veo los objetos que referencian alguna tabla desde fuera del modulo, si tienen alguna tabla de otro modulo
                //Si son todas de mi modulo, lo pongo en la lista de los objetos a mover a mi modulo.

                foreach (KBObject objreadOutsideModule in TablesHelper.ObjectsReadTableOutsideModule(tbl))
                {

                    if (ListModulesOfReferencedTables(objreadOutsideModule).Count == 1)
                        objToMove += Utility.linkObject(objreadOutsideModule) +"; " ;
                }
            }

            string[] mdlStats = new string[] { Utility.linkObject(mdl), listPubObjNotReferenced, listPirvateObjReferenced, ListobjOutsideModuleAccessPrivateTable, objUpdateOutsideModuleList,objToMove };

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
                if (objRef != null && Utility.isRunable(objRef))
                {
                    Module modref = objRef.Module;
                    if (modref != mdl)
                    {
                        isReferencedFromOutside = true;
                        if (obj is Table)
                            objList += " " + Utility.linkObject(objRef);
                    }
                }
            }

            return isReferencedFromOutside;
        }


        private static KBObjectCollection ObjectsReferencesFromOutside(KBObject obj)
        {
            KBObjectCollection objCol = new KBObjectCollection();

            string mdlName = ObjectModuleName(obj);

            foreach (EntityReference refer in obj.GetReferencesTo())
            {
                KBObject objRef = KBObject.Get(obj.Model, refer.From);
                if (objRef != null && Utility.isRunable(objRef))
                {
                    string mdlNameRef = ObjectModuleName(objRef);
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
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);
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

                KBDoctorOutput.EndSection(title, true);
            }
            catch
            {
                success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }

        }

        private static KBObjectCollection CreateListObjectsModuleAndReferences(KBModel kbModel, Module mdl, KBDoctorXMLWriter writer)
        {
            KBObjectCollection objToBuild = new KBObjectCollection();
            foreach (KBObject obj in Utility.EditableObjects(mdl.GetAllMembers()))
            {
                if (KBObjectHelper.IsSpecifiable(obj) && KBDoctorCore.Sources.Utility.isGenerated(obj))
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
                if (objRef != null && KBObjectHelper.IsSpecifiable(objRef) && !objToBuild.Contains(objRef) && KBDoctorCore.Sources.Utility.isGenerated(objRef) )
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
            KBDoctorOutput.StartSection(title);

            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] {
                "Name", "Description", "Visibility", "Best", "Best Module", "Modules","Transaction", "Trn(NoGenerate)", "Referenced Modules" , "Not in Module", "Count"  });

                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Module>());
                foreach (KBObject module in Utility.EditableObjects(UIServices.SelectObjectDialog.SelectObjects(selectObjectOption)))
                {
                    foreach (Table t in Table.GetAll(module.Model))

                    {
                        if (TablesHelper.TableModule(module.Model, t) == module)
                        {
                            string objNameLink = Utility.linkObject(t);

                            KBDoctorOutput.Message( "Processing... " + t.Name);


                            ObjectVisibility objVisibility = TableVisibility(t);
                            KBObject trnBest = GenexusBLServices.Tables.GetBestAssociatedTransaction(model, t.Key);
                            Module mdlTrnBest = trnBest.Module;

                            string trnGen = "";
                            string trnNoGen = "";
                            List<string> mdlList = new List<string>();
                            foreach (Transaction trn in t.AssociatedTransactions)

                            {
                                string trnstr = Utility.linkObject(trn) + "(" + ((trn.IsPublic) ? "Public" : "Private") + ") <br/> ";
                                if (trn.GetPropertyValue<bool>(Properties.TRN.GenerateObject)) trnGen += trnstr;
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

                            IList<KBObject> objList2 = objList.Where(r => r.Module != mdlTrnBest & !(r is Transaction)).ToList();

                            string objListQNames = null;
                            objList2.ToList().ForEach(v => objListQNames += " " + Utility.linkObject(v));

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
                                 mdlListstr, trnGen, trnNoGen ,mdlReferencedListstr, objListQNames, objList2.Count.ToString()});

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

        internal static void ListObjectsWithTableInOtherModule()
        {

            IKBService kbserv = UIServices.KB;
            KBModel model = kbserv.CurrentModel.GetDesignModel();

            string title = "KBDoctor - List Objects with table in other module";
            IOutputService output = CommonServices.Output;
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);
                writer.AddTableHeader(new string[] {
                "Name", "Description",  "Object Module", "Table ","Table Module"  });

                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;

                foreach (KBObject obj in Utility.EditableObjects(UIServices.SelectObjectDialog.SelectObjects(selectObjectOption)))
                {
                    string modulename = ModulesHelper.ObjectModuleName(obj);

                    foreach (EntityReference reference in obj.GetReferences())

                    {

                        KBObject objref = KBObject.Get(obj.Model, reference.To);

                        if (objref != null && objref is Table && !ObjectsHelper.isGeneratedbyPattern(obj))
                        {

                            Table t = (Table)objref;
                            string tablemodulename = TablesHelper.TableModule(t.Model, t).Name;

                            if (tablemodulename != modulename)
                            {
                                string objNameLink = Utility.linkObject(obj);

                                KBDoctorOutput.Message( "Processing... " + obj.Name + " reference table " + t.Name + " Object module:" + modulename + " Table module:" + tablemodulename);

                                writer.AddTableData(new string[] { objNameLink, obj.Description, modulename, t.Name, tablemodulename });

                            }
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

        internal static void ModuleDependencies()

        {
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            bool success = true;

            string title = "KBDoctor - Module references";
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);
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

                    KBDoctorOutput.Message( "Procesing " + module.Name + "....");

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
                                listObj += " " + Utility.linkObject(objref);
                            }

                        }

                    }
                    Module oModule = ((o is Table) ? TablesHelper.TableModule(o.Model, (Table)o) : o.Module);
                    writer.AddTableData(new string[] { Utility.linkObject(o), oModule.Name, o.TypeDescriptor.Name, listObj });
                }
                KBDoctorOutput.Message( "");
                KBDoctorOutput.EndSection(title, success);

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

        internal static void ObjectsToDivide()

        {
            IKBService kbserv = UIServices.KB;
            KBModel kbModel = UIServices.KB.CurrentModel;
            IOutputService output = CommonServices.Output;
            bool success = true;

            string title = "KBDoctor - Objects to divide";
            KBDoctorOutput.StartSection(title);
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);
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

                    KBDoctorOutput.Message( "Procesing " + module.Name + "....");

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
                                listObj += " " + Utility.linkObject(objref);
                            }

                        }

                    }
                    Module oModule = ((o is Table) ? TablesHelper.TableModule(o.Model, (Table)o) : o.Module);
                    writer.AddTableData(new string[] { Utility.linkObject(o), oModule.Name, o.TypeDescriptor.Name, listObj });
                }
                KBDoctorOutput.Message( "");
                KBDoctorOutput.EndSection(title, success);

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



        internal static KBObjectCollection ModuleObjects(Module module)
        {
            {
                KBObjectCollection objectsModule = new KBObjectCollection();
                foreach (KBObject obj in Utility.EditableObjects(module.GetAllMembers()))
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

            string title = "KBDoctor - Recomended module";
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);

                IOutputService output = CommonServices.Output;
                KBDoctorOutput.StartSection(title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title);

                writer.AddTableHeader(new string[] { "Type", "Object", "Current Module", "Suggested Module", "Intra Weight", "Inter Weight", "Score", "Delta", "Top candidates" });


                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = false;
                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Module>());
                // Module module2 = new Module(kbModel);
                foreach (Module module in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
                {
                    foreach (KBObject obj in Utility.EditableObjects(module.GetAllMembers()))
                    {
                        if (CanSuggestModuleForObject(obj))
                        {
                            List<ModuleSuggestion> suggestions = SuggestModulesForObject(obj);
                            if (suggestions.Count == 0)
                            {
                                continue;
                            }

                            ModuleSuggestion suggestion = suggestions[0];
                            if (suggestion.Module != obj.Module && suggestion.TotalWeight > 0)
                            {
                                writer.AddTableData(new string[] {
                                    obj.TypeDescriptor.Name,
                                    Utility.linkObject(obj),
                                    obj.Module.QualifiedName.ToString(),
                                    suggestion.Module.QualifiedName.ToString(),
                                    suggestion.IntraModuleWeight.ToString(),
                                    suggestion.InterModuleWeight.ToString(),
                                    suggestion.Score.ToString(),
                                    suggestion.DeltaScore.ToString(),
                                    FormatTopModuleCandidates(suggestions)
                                });
                            }
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

        private static string FormatTopModuleCandidates(List<ModuleSuggestion> suggestions)
        {
            StringBuilder builder = new StringBuilder();

            foreach (ModuleSuggestion suggestion in suggestions.Take(3))
            {
                if (builder.Length > 0)
                {
                    builder.Append("<br>");
                }

                builder.Append(suggestion.Module.QualifiedName.ToString());
                builder.Append(" (");
                builder.Append(suggestion.Score.ToString());
                if (suggestion.IsCurrentModule)
                {
                    builder.Append(", current");
                }
                builder.Append(")");
            }

            return builder.ToString();
        }

        public static void ApplyExternalModularization()
        {
            KBDoctorOutput.StartSection("Modularization");
            KBModel model = UIServices.KB.CurrentModel;
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Modularization|*.bunch";
            openFileDialog1.Filter = "Todos|*.*";
            openFileDialog1.Title = "Select a Bunch modularization File";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                string line = "";
                using (sr)
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] words = line.Split('=');
                        string mdl = words[0].Trim();
                        Module newmodule = ModuloAAsignar(mdl);

                        string[] objects = words[1].Split(',');


                        foreach (string objAndType in objects)
                        {
                            string[] parts = objAndType.Split(':');
                            string objname = parts[0].Trim();

                            string objtype = parts[1].Trim();

                            if (objtype != "Table")
                            {
                                KBObjectDescriptor kbod = KBObjectDescriptor.Get(objtype);

                                string[] ns = new[] { "Objects" };

                                foreach (KBObject obj in UIServices.KB.CurrentModel.Objects.GetByPartialName(ns, objname))
                                {
                                    if (obj != null && obj.Name == objname && obj.Type == kbod.Id)
                                    {
                                        KBDoctorOutput.Message(obj.Name + " : " + obj.TypeDescriptor.Name);
                                        obj.Module = newmodule;
                                        try
                                        {
                                            obj.Save();
                                        }
                                        catch
                                        {
                                            KBDoctorOutput.Message("Can't save " + obj.Name);
                                        }
                                    }
                                }
                            }

                        }
                    }


                }
                sr.Close();
            }

            //Borro modulos vacios
            foreach (Module mdl in Module.GetAll(model))
            {
                try
                {
                    mdl.Delete();
                }
                catch {  }
            }
            KBDoctorOutput.EndSection("Modularization");
        }

        public static void MoveObjectsToModulesFromFile()
        {
            const string title = "KBDoctor - Move objects to modules from file";
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Select the module movements file",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            KBDoctorOutput.StartSection(title);
            IOutputService output = CommonServices.Output;
            KBModel model = UIServices.KB.CurrentModel;
            List<string> errors = new List<string>();
            List<ModuleMoveRequest> requests;

            try
            {
                requests = ParseAndValidateModuleMoves(
                    model,
                    System.IO.File.ReadAllLines(dialog.FileName),
                    errors);
            }
            catch (Exception exception)
            {
                errors.Add("Could not read the file: " + exception.Message);
                requests = new List<ModuleMoveRequest>();
            }

            if (errors.Count > 0)
            {
                ShowModuleMoveErrors(errors, output);
                KBDoctorOutput.EndSection(title, false);
                return;
            }

            List<ModuleMoveRequest> pendingRequests = requests
                .Where(request =>
                {
                    KBObject obj = FindObjectsByQualifiedName(model, request.ObjectQualifiedName).Single();
                    return obj.Module == null
                        || !string.Equals(
                            obj.Module.QualifiedName.ToString(),
                            request.ModuleQualifiedName,
                            StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            if (pendingRequests.Count == 0)
            {
                output.AddLine("The file is valid. All objects are already in their destination modules.");
                KBDoctorOutput.EndSection(title, true);
                return;
            }

            KnowledgeBase kb = model.KB;
            KBVersion workingVersion = KBVersion.GetActive(kb);
            KBVersion frozenVersion;
            string frozenVersionName = CreateModuleMoveSnapshotName(kb);

            try
            {
                frozenVersion = KBVersionHelper.FreezeModel(
                    frozenVersionName,
                    "Created by KBDoctor before moving objects between modules.",
                    workingVersion);
                output.AddLine("Frozen version created: " + frozenVersion.Name);
            }
            catch (Exception exception)
            {
                output.AddErrorLine("Could not create the frozen version. No objects were moved.");
                output.AddErrorLine(exception.ToString());
                MessageBox.Show(
                    "The frozen version could not be created. No objects were moved.\r\n\r\n" + exception.Message,
                    title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                KBDoctorOutput.EndSection(title, false);
                return;
            }

            int moved = 0;
            Exception movementError = null;
            ModuleMoveRequest failedRequest = null;

            foreach (ModuleMoveRequest request in pendingRequests)
            {
                try
                {
                    KBModel currentModel = UIServices.KB.CurrentModel;
                    Module destination = FindModuleByQualifiedName(currentModel, request.ModuleQualifiedName);
                    List<KBObject> matches = FindObjectsByQualifiedName(currentModel, request.ObjectQualifiedName);
                    KBObject obj = matches.Count == 1 ? matches[0] : null;

                    if (destination == null || obj == null)
                    {
                        throw new InvalidOperationException(
                            "The object or destination module is no longer available in the current version.");
                    }

                    obj.Module = destination;
                    obj.Save();
                    moved += 1;
                    output.AddLine(obj.QualifiedName + " -> " + destination.QualifiedName);
                }
                catch (Exception exception)
                {
                    movementError = exception;
                    failedRequest = request;
                    break;
                }
            }

            if (movementError == null)
            {
                output.AddLine("Completed. Objects moved: " + moved + ". Duplicates were ignored.");
                output.AddLine("Recovery version: " + frozenVersion.Name);
                KBDoctorOutput.EndSection(title, true);
                return;
            }

            output.AddErrorLine(
                "Movement failed at line " + failedRequest.LineNumber
                + " for object '" + failedRequest.ObjectQualifiedName
                + "': " + movementError.Message);
            output.AddErrorLine("Objects moved before the error: " + moved + ".");
            output.AddErrorLine("Frozen recovery version: " + frozenVersion.Name);

            DialogResult rollback = MessageBox.Show(
                "An error occurred while moving '" + failedRequest.ObjectQualifiedName + "'.\r\n\r\n"
                + movementError.Message + "\r\n\r\n"
                + moved + " object(s) had already been moved.\r\n"
                + "Do you want to restore the working version from '" + frozenVersion.Name + "'?",
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);

            if (rollback == DialogResult.Yes)
            {
                try
                {
                    KBVersionHelper.Revert(frozenVersion, workingVersion);
                    KBVersionHelper.SetAsActive(workingVersion, true);
                    output.AddLine("The working version was restored from " + frozenVersion.Name + ".");
                }
                catch (Exception rollbackException)
                {
                    output.AddErrorLine("Automatic restore failed: " + rollbackException);
                    MessageBox.Show(
                        "Automatic restore failed.\r\n\r\n"
                        + rollbackException.Message + "\r\n\r\n"
                        + "The frozen version '" + frozenVersion.Name + "' is still available for manual recovery.",
                        title,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                output.AddLine("Restore was not requested. Frozen version available: " + frozenVersion.Name);
            }

            KBDoctorOutput.EndSection(title, false);
        }

        private static List<ModuleMoveRequest> ParseAndValidateModuleMoves(
            KBModel model,
            IEnumerable<string> lines,
            List<string> errors)
        {
            List<ModuleMoveRequest> requests = new List<ModuleMoveRequest>();
            Dictionary<Guid, ModuleMoveRequest> requestsByObject = new Dictionary<Guid, ModuleMoveRequest>();
            int lineNumber = 0;

            foreach (string sourceLine in lines)
            {
                lineNumber += 1;
                string line = sourceLine == null ? "" : sourceLine.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                int separator = line.IndexOf(':');
                if (separator <= 0 || separator != line.LastIndexOf(':'))
                {
                    errors.Add("Line " + lineNumber + ": expected DestinationModule:Object1;Object2.");
                    continue;
                }

                string moduleName = line.Substring(0, separator).Trim();
                string objectsText = line.Substring(separator + 1).Trim();
                Module destination = FindModuleByQualifiedName(model, moduleName);
                if (destination == null)
                {
                    errors.Add("Line " + lineNumber + ": destination module '" + moduleName + "' does not exist.");
                }

                string[] objectNames = objectsText.Split(';');
                if (objectNames.Length == 0 || objectNames.Any(name => string.IsNullOrWhiteSpace(name)))
                {
                    errors.Add("Line " + lineNumber + ": every object must have a qualified name.");
                    continue;
                }

                foreach (string rawObjectName in objectNames)
                {
                    string objectName = rawObjectName.Trim();
                    List<KBObject> matches = FindObjectsByQualifiedName(model, objectName);
                    if (matches.Count == 0)
                    {
                        errors.Add("Line " + lineNumber + ": object '" + objectName + "' does not exist.");
                        continue;
                    }

                    if (matches.Count > 1)
                    {
                        errors.Add("Line " + lineNumber + ": object '" + objectName + "' is ambiguous.");
                        continue;
                    }

                    KBObject obj = matches[0];
                    if (obj is Module || obj is Table || obj.IsReadOnly)
                    {
                        errors.Add(
                            "Line " + lineNumber + ": object '" + objectName
                            + "' cannot be moved because it is a module, table, or read-only object.");
                        continue;
                    }

                    if (destination == null)
                    {
                        continue;
                    }

                    ModuleMoveRequest existing;
                    if (requestsByObject.TryGetValue(obj.Guid, out existing))
                    {
                        if (existing.ModuleGuid != destination.Guid)
                        {
                            errors.Add(
                                "Line " + lineNumber + ": object '" + objectName
                                + "' is assigned to both '" + existing.ModuleQualifiedName
                                + "' and '" + destination.QualifiedName + "'.");
                        }
                        continue;
                    }

                    ModuleMoveRequest request = new ModuleMoveRequest
                    {
                        LineNumber = lineNumber,
                        ModuleQualifiedName = destination.QualifiedName.ToString(),
                        ObjectQualifiedName = obj.QualifiedName.ToString(),
                        ModuleGuid = destination.Guid,
                        ObjectGuid = obj.Guid
                    };
                    requests.Add(request);
                    requestsByObject.Add(obj.Guid, request);
                }
            }

            if (requests.Count == 0 && errors.Count == 0)
            {
                errors.Add("The file does not contain any object movements.");
            }

            return requests;
        }

        private static Module FindModuleByQualifiedName(KBModel model, string qualifiedName)
        {
            return Module.GetAll(model).FirstOrDefault(
                module => string.Equals(
                    module.QualifiedName.ToString(),
                    qualifiedName,
                    StringComparison.OrdinalIgnoreCase));
        }

        private static List<KBObject> FindObjectsByQualifiedName(KBModel model, string qualifiedName)
        {
            int separator = qualifiedName.LastIndexOf('.');
            string objectName = separator < 0
                ? qualifiedName
                : qualifiedName.Substring(separator + 1);

            return model.Objects
                .GetByPartialName(new[] { "Objects" }, objectName)
                .Where(obj => string.Equals(
                    obj.QualifiedName.ToString(),
                    qualifiedName,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private static string CreateModuleMoveSnapshotName(KnowledgeBase kb)
        {
            string baseName = "KBDoctor before module moves " + DateTime.Now.ToString("yyyyMMdd HHmmss");
            string name = baseName;
            int suffix = 2;
            while (KBVersion.Get(kb, name) != null)
            {
                name = baseName + " " + suffix;
                suffix += 1;
            }
            return name;
        }

        private static void ShowModuleMoveErrors(IEnumerable<string> errors, IOutputService output)
        {
            List<string> errorList = errors.ToList();
            output.AddErrorLine("The file has errors. No version was created and no objects were moved.");
            foreach (string error in errorList)
            {
                output.AddErrorLine(error);
            }

            string details = string.Join("\r\n", errorList.Take(20));
            if (errorList.Count > 20)
            {
                details += "\r\n... and " + (errorList.Count - 20) + " more error(s).";
            }

            MessageBox.Show(
                "The file has errors. No changes were made.\r\n\r\n" + details,
                "KBDoctor - Move objects to modules from file",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private static Module ModuloAAsignar(string mdl)
        {
            string mdl2 = FixObjectName(mdl);

            Module modu = new Module(UIServices.KB.CurrentModel);
            Guid moduletypeid = new Guid("c88fffcd-b6f8-0000-8fec-00b5497e2117");
            foreach (KBObject obj in Module.GetAll(UIServices.KB.CurrentModel))
            {
                if (obj.Name==mdl2)
                        modu = (Module)obj;
            }

            if (modu.Name == null )

            {
                Random rnd = new Random();
                int length = 5;
                var str = "";
                for (var i = 0; i < length; i++)
                {
                    str += ((char)(rnd.Next(1, 26) + 64)).ToString();
                }

                modu.Name = mdl2 + "_" + str;
                modu.Module = Module.GetRoot(UIServices.KB.CurrentModel);
                modu.Save();
            }
            return modu;
        }

        private static string FixObjectName(string mdl)
        {
            mdl = mdl.Replace("SS(", "");
            mdl = mdl.Replace(".ss)", "");
            mdl = mdl.Replace(".", "");
            mdl = mdl.Replace(":", "");
            mdl = mdl.Replace(")", "");
            mdl = mdl.Replace("(", "");
            mdl = mdl.Replace(" ", "");
            return mdl;
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

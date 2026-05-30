using System;
using System.Collections.Generic;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Helpers;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Packages.Patterns.Objects;
using Artech.Udm.Framework.References;

namespace Concepto.Packages.KBDoctorCore.Sources
{
    public static class KbStatistics
    {
        public sealed class GeneratedByPatternSummary
        {
            public IList<BasicObjectInfo> ObjectsNotGeneratedByPattern { get; set; }
            public int TotalObjects { get; set; }
            public int ObjectsNotGeneratedWithPatterns { get; set; }
            public int ObjectsNotGeneratedWithPatternsPercent { get; set; }
            public string SummaryText { get; set; }
        }

        public sealed class BasicObjectInfo
        {
            public KBObject Object { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string LastUpdate { get; set; }
            public string Timestamp { get; set; }
        }

        public sealed class GeneratedPatternPartInfo
        {
            public KBObject Object { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string PartName { get; set; }
            public string Description { get; set; }
        }

        public sealed class GeneratedObjectInfo
        {
            public KBObject Object { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            public string ModuleName { get; set; }
            public string IsPublic { get; set; }
            public string LastUpdate { get; set; }
            public string IsMain { get; set; }
            public string Timestamp { get; set; }
            public string IsGenerated { get; set; }
            public string Protocol { get; set; }
            public string QualifiedName { get; set; }
        }

        public sealed class MainObjectInfo
        {
            public KBObject Object { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            public string ModuleName { get; set; }
            public string IsPublic { get; set; }
            public string AppGenerator { get; set; }
            public string Protocol { get; set; }
            public string LastUpdate { get; set; }
        }

        public sealed class ProcedureInfo
        {
            public KBObject Object { get; set; }
            public string Type { get; set; }
            public string CommitOnExit { get; set; }
            public string ModuleName { get; set; }
            public string IsPublic { get; set; }
            public string LastUpdate { get; set; }
        }

        public sealed class UnreferencedUserModuleObjectInfo
        {
            public KBObject Object { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            public string ModuleName { get; set; }
            public string IsPublic { get; set; }
            public string LastUpdate { get; set; }
            public bool CanDelete { get; set; }
        }

        public static GeneratedByPatternSummary CountGeneratedByPattern(KBModel model)
        {
            List<BasicObjectInfo> objects = new List<BasicObjectInfo>();
            int totalObjects = 0;
            int objectsNotGeneratedWithPatterns = 0;

            foreach (KBObject obj in model.Objects.GetAll())
            {
                totalObjects++;
                if (Utility.isGenerated(obj) && IsPatternCandidate(obj) && !Utility.IsGeneratedByPattern(obj))
                {
                    objectsNotGeneratedWithPatterns++;
                    objects.Add(CreateBasicObjectInfo(obj));
                }
            }

            int percent = totalObjects == 0 ? 0 : objectsNotGeneratedWithPatterns * 100 / totalObjects;
            string summaryText = "Objects no generated with patterns / Total objects = " + percent + " %,No generated with Patterns=" + objectsNotGeneratedWithPatterns + ", Total Objects=" + totalObjects;

            return new GeneratedByPatternSummary
            {
                ObjectsNotGeneratedByPattern = objects,
                TotalObjects = totalObjects,
                ObjectsNotGeneratedWithPatterns = objectsNotGeneratedWithPatterns,
                ObjectsNotGeneratedWithPatternsPercent = percent,
                SummaryText = summaryText
            };
        }

        public static IEnumerable<GeneratedPatternPartInfo> GetGeneratedPatternPartsWithoutDynamism(KBModel model)
        {
            foreach (KBObject obj in model.Objects.GetAll())
            {
                if (IsGeneratedObjectByPattern(obj))
                {
                    foreach (KBObjectPart part in obj.Parts)
                    {
                        if (part != null && !part.IsDefault && !part.IsVirtualPart() && !part.Name.Contains("Help") && !part.Name.Contains("Documentation") && !part.Name.Contains("Conditions"))
                        {
                            yield return new GeneratedPatternPartInfo
                            {
                                Object = obj,
                                Type = obj.TypeDescriptor.Name,
                                Name = obj.Name,
                                PartName = part.Name,
                                Description = obj.Description
                            };
                        }
                    }
                }
            }
        }

        public static IEnumerable<BasicObjectInfo> GetAllObjects(KBModel model)
        {
            foreach (KBObject obj in model.Objects.GetAll())
            {
                yield return CreateBasicObjectInfo(obj);
            }
        }

        public static IEnumerable<GeneratedObjectInfo> GetGeneratedRunnableObjects(KBModel model)
        {
            foreach (KBObject obj in model.Objects.GetAll())
            {
                if (obj != null && Utility.isRunable(obj) && Utility.isGenerated(obj))
                {
                    string protocol = obj.GetPropertyValueString("CALL_PROTOCOL");
                    if (protocol == "Internal")
                    {
                        protocol = string.Empty;
                    }

                    yield return new GeneratedObjectInfo
                    {
                        Object = obj,
                        Type = obj.TypeDescriptor.Name + " ",
                        Description = CleanDescription(obj.Description),
                        ModuleName = obj.Module.Name,
                        IsPublic = obj.IsPublic.ToString(),
                        LastUpdate = obj.LastUpdate.ToString(),
                        IsMain = obj.GetPropertyValue<bool>("IsMain") ? "True" : string.Empty,
                        Timestamp = obj.Timestamp.ToString(),
                        IsGenerated = Utility.isGenerated(obj) ? "Yes" : string.Empty,
                        Protocol = protocol,
                        QualifiedName = obj.QualifiedNameString
                    };
                }
            }
        }

        public static IEnumerable<MainObjectInfo> GetMainObjects(KBModel model)
        {
            KBCategory mainCategory = Utility.MainCategory(model);
            foreach (KBObject obj in mainCategory.AllMembers)
            {
                if (obj != null)
                {
                    yield return new MainObjectInfo
                    {
                        Object = obj,
                        Type = obj.TypeDescriptor.Name + " ",
                        Description = CleanDescription(obj.Description),
                        ModuleName = obj.Module.Name,
                        IsPublic = obj.IsPublic.ToString(),
                        AppGenerator = obj.GetPropertyValueString("AppGenerator"),
                        Protocol = obj.GetPropertyValueString("CALL_PROTOCOL"),
                        LastUpdate = obj.LastUpdate.ToShortDateString()
                    };
                }
            }
        }

        public static IEnumerable<ProcedureInfo> GetProcedures(KBModel model, DateTime now)
        {
            foreach (KBObject obj in model.Objects.GetAll())
            {
                if (obj is Procedure)
                {
                    string commitOnExit = string.Empty;
                    if (obj.VersionDate >= now.AddDays(-45))
                    {
                        object value = obj.GetPropertyValue("CommitOnExit");
                        if (value != null)
                        {
                            commitOnExit = value.ToString();
                        }
                    }

                    yield return new ProcedureInfo
                    {
                        Object = obj,
                        Type = obj.TypeDescriptor.Name + " ",
                        CommitOnExit = commitOnExit,
                        ModuleName = obj.Module.Name,
                        IsPublic = obj.IsPublic.ToString(),
                        LastUpdate = obj.LastUpdate.ToShortDateString()
                    };
                }
            }
        }

        public static IEnumerable<UnreferencedUserModuleObjectInfo> GetUnreferencedObjectsInUserModules(KBModel model)
        {
            foreach (KBObject obj in model.Objects.GetAll())
            {
                if (obj != null && IsUserEditableObject(model, obj) && !HasReferencesTo(obj))
                {
                    yield return new UnreferencedUserModuleObjectInfo
                    {
                        Object = obj,
                        Type = obj.TypeDescriptor.Name,
                        Description = obj.Description,
                        ModuleName = obj.Module.QualifiedName.ToString(),
                        IsPublic = obj.IsPublic.ToString(),
                        LastUpdate = obj.LastUpdate.ToString(),
                        CanDelete = !obj.IsReadOnly && !(obj is Transaction)
                    };
                }
            }
        }

        private static BasicObjectInfo CreateBasicObjectInfo(KBObject obj)
        {
            return new BasicObjectInfo
            {
                Object = obj,
                Type = obj.TypeDescriptor.Name,
                Name = obj.Name,
                Description = obj.Description,
                LastUpdate = obj.LastUpdate.ToString(),
                Timestamp = obj.Timestamp.ToString()
            };
        }

        private static bool IsPatternCandidate(KBObject obj)
        {
            return obj is WebPanel || obj is Transaction || obj is WorkPanel || obj is DataSelector;
        }

        private static bool IsGeneratedObjectByPattern(KBObject obj)
        {
            return obj == null || obj.GetPropertyValue<bool>(KBObjectProperties.IsGeneratedObject);
        }

        private static bool IsObjectInUserModule(KBModel model, KBObject obj)
        {
            if (obj is Module || obj is Folder || obj.Module == null)
            {
                return false;
            }

            Module root = Module.GetRoot(model);
            Module module = obj.Module;
            if (module == root || module.Guid == Guid.Empty)
            {
                return false;
            }

            while (module != null && module.Guid != Guid.Empty)
            {
                if (module == root || module.Module == root)
                {
                    return true;
                }

                module = module.Module;
            }

            return false;
        }

        private static bool IsUserEditableObject(KBModel model, KBObject obj)
        {
            return !obj.IsReadOnly && IsObjectInUserModule(model, obj);
        }

        private static bool HasReferencesTo(KBObject obj)
        {
            foreach (EntityReference reference in obj.GetReferencesTo(LinkType.UsedObject))
            {
                if (reference.From != obj.Key)
                {
                    return true;
                }
            }

            return false;
        }

        private static string CleanDescription(string description)
        {
            if (description == null)
            {
                return string.Empty;
            }

            return description.Replace(",", " ").Replace(">", string.Empty).Replace("<", string.Empty);
        }
    }
}

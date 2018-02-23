using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.Common.Services;
using System.Collections;


namespace Concepto.Packages.KBDoctorCore.Sources
{
    public static class API
    {
        public static void Main()
        {
            return;
        }
        //
        public static void PrepareCompareNavigations(KnowledgeBase KB, IOutputService output)
        {
            Navigation.PrepareComparerNavigation(KB, output);
        }
        //
        public static bool CompareNavigations(KnowledgeBase KB, IOutputService output)
        {
           Navigation.ReplaceModulesInNVGFiles(KB, output);
           return Navigation.CompareLastNVGDirectories(KB, output);
        }
        //
        public static void GetClassesTypesWithTheSameSignature(IEnumerable<KBObject> objects, out HashSet<int> classes, out Hashtable[] Classes_types)
        {
            Objects.GetClassesTypesWithTheSameSignature(objects, out classes, out Classes_types);
        }
        //
        public static void CleanKBObjectVariables(KBObject obj, IOutputService output)
        {
            CleanKB.CleanKBObjectVariables(obj, output);
        }
        //
        public static void CleanAllKBObjectVariables(KnowledgeBase KB, IOutputService output)
        {
            foreach (KBObject kbo in KB.DesignModel.Objects.GetAll())
                CleanKB.CleanKBObjectVariables(kbo, output);
        }
        //
        public static void RemoveAttributesWithoutTable(KBModel kbmodel, IOutputService output, out List<string[]> lineswriter)
        {
            CleanKB.RemoveAttributesWithoutTable(kbmodel, output, out lineswriter);
        }
        //
        public static void CleanKBObject(KBObject obj, IOutputService output)
        {
            CleanKB.CleanObject(obj, output);
        }
        //
        public static void CleanKBObjects(KBModel kbmodel, IOutputService output)
        {
            CleanKB.CleanObjects(kbmodel, output);
        }
        //
        public static void RemoveObjectsNotCalled(KBModel kbmodel, IOutputService output, out List<string[]> lineswriter)
        {
            CleanKB.RemoveObjectsNotCalled(kbmodel, output, out lineswriter);
        }
    }
}

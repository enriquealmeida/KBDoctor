using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using System.Collections;
using System.Threading.Tasks;

namespace Concepto.Packages.KBDoctorCore.Sources
{
    public static class API
    {
        public static void Main()
        {
            return;
        }
        public static void PrepareCompareNavigations(KnowledgeBase KB, IOutputService output)
        {
            Navigation.PrepareComparerNavigation(KB, output);
        }
        public static bool CompareNavigations(KnowledgeBase KB, IOutputService output)
        {
            return Navigation.CompareLastNVGDirectories(KB, output);
        }
        public static void GetClassesTypesWithTheSameSignature(IEnumerable<KBObject> objects, out HashSet<int> classes, out Hashtable[] Classes_types)
        {
            Objects.GetClassesTypesWithTheSameSignature(objects, out classes, out Classes_types);
        }
        public static void CleanKBObjectVariables(KBObject obj, IOutputService output)
        {
            CleanKB.CleanKBObjectVariables(obj, output);
        }

        public static void CleanAllKBObjectVariables(KnowledgeBase KB, IOutputService output)
        {
            foreach (KBObject kbo in KB.DesignModel.Objects.GetAll())
                CleanKB.CleanKBObjectVariables(kbo, output);
        }
    }
}

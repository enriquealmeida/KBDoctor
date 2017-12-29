using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
//using System.Threading.Tasks;

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
            Navigation.PrepareComparerNavigation(KB,output);
        }
        public static bool CompareNavigations(KnowledgeBase KB, IOutputService output)
        {
            return Navigation.CompareLastNVGDirectories(KB, output);
        }
    }
}

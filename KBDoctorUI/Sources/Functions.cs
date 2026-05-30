using System;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework;
using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    static class Functions
    {
        public static int MaxCodeBlock(string source)
        {
            return Utility.MaxCodeBlock(source);
        }

        public static int ComplexityLevel(string source)
        {
            return Utility.ComplexityLevel(source);
        }

        public static int MaxNestLevel(string source)
        {
            return Utility.MaxNestLevel(source);
        }

        public static bool ValidateINOUTinParm(KBObject obj)
        {
            return Utility.ValidateINOUTinParm(obj);
        }

        internal static void AddLineSummary(string fileName, string texto)
        {
            Utility.AddLineSummary(UIServices.KB.CurrentKB, fileName, texto);
        }

        internal static void AddLine(string fileName, string texto)
        {
            Utility.AddLine(UIServices.KB.CurrentKB, fileName, texto);
        }

        public static string ObjectSourceUpper(KBObject obj)
        {
            return Utility.ObjectSourceUpper(obj);
        }

        public static string ObjectRulesUpper(KBObject obj)
        {
            return Utility.ObjectRulesUpper(obj);
        }

        public static bool isRunable(KBObject obj)
        {
            return Utility.isRunable(obj);
        }

        public static bool CanBeBuilt(KBObject obj)
        {
            return Utility.CanBeBuilt(obj);
        }

        public static string ExtractComments(string source)
        {
            return Utility.ExtractComments(source);
        }

        public static string CodeCommented(string source)
        {
            return Utility.CodeCommented(source);
        }

        public static bool HasCodeCommented(string source)
        {
            return Utility.HasCodeCommented(source);
        }

        public static Domain DomainByName(string domainName)
        {
            return Utility.DomainByName(UIServices.KB.CurrentModel, domainName);
        }

        public static string RemoveEmptyLines(string lines)
        {
            return Utility.RemoveEmptyLines(lines);
        }

        public static int LineCount(string s)
        {
            return Utility.LineCount(s);
        }

        public static string linkObject(KBObject obj)
        {
            if (obj != null)
                return CommandLink("OpenObject", obj.Name, "name", obj.Guid.ToString());

            return "";
        }

        public static string CommandLink(string command, string text, params string[] parameters)
        {
            string href = "gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;" + command;

            for (int i = 0; i + 1 < parameters.Length; i += 2)
            {
                href += "&" + parameters[i] + "=" + Uri.EscapeDataString(parameters[i + 1] ?? "");
            }

            return "<a href=\"" + href + "\">" + text + "</a>";
        }

        public static string linkFile(string file)
        {
            return Utility.linkFile(file);
        }

        public static string ExtractRuleParm(KBObject obj)
        {
            return Utility.ExtractRuleParm(obj);
        }

        public static string CleanFileName(string filename)
        {
            return Utility.CleanFileName(filename);
        }

        public static string CreateOutputFile(IKBService kbserv, string title)
        {
            return Utility.CreateOutputFile(kbserv.CurrentKB, title);
        }

        public static bool AttIsSubtype(Artech.Genexus.Common.Objects.Attribute a)
        {
            return Utility.AttIsSubtype(a);
        }

        public static void KillAttribute(Artech.Genexus.Common.Objects.Attribute a)
        {
            Utility.KillAttribute(a);
        }

        internal static bool hasModule(KBObject obj)
        {
            return Utility.HasModule(obj);
        }

        internal static KBCategory MainCategory(KBModel model)
        {
            return Utility.MainCategory(model);
        }

        public static void SaveObject(IOutputService output, KBObject obj)
        {
            Utility.SaveObject(output, obj);
        }
    }
}

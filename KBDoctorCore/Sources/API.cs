using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.Common.Services;
using System.Collections;
using Artech.Common.Diagnostics;
using Artech.Architecture.Common.Location;
using Artech.Genexus.Common.Objects;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using Artech.Architecture.BL.Framework.Services;
using Microsoft.VisualBasic.ApplicationServices;
using System.Diagnostics;
using System.IO;
using Microsoft.Practices.CompositeUI.EventBroker;
using Artech.Architecture.Common.Events;

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
            Utility.CreateModuleNamesFile(KB);
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
            string rec = "";
            CleanKB.CleanKBObjectVariables(obj, output, ref rec);
        }
        //
        public static void CleanAllKBObjectVariables(KnowledgeBase KB, IOutputService output)
        {
            string rec = "";
            foreach (KBObject kbo in KB.DesignModel.Objects.GetAll())
                CleanKB.CleanKBObjectVariables(kbo, output, ref rec);
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
        public static void CleanKBObjects(KnowledgeBase kb, IEnumerable<KBObject> kbojs, IOutputService output)
        {
            CleanKB.CleanObjects(kb, kbojs, output);
        }
        //
        public static void RemoveObjectsNotCalled(KBModel kbmodel, IOutputService output, out List<string[]> lineswriter)
        {
            CleanKB.RemoveObjectsNotCalled(kbmodel, output, out lineswriter);
        }
        //
        public static void SaveObjectsWSDLSource(KnowledgeBase KB, IOutputService output)
        {
            Navigation.SaveObjectsWSDL(KB, output, true);
        }
        //
        public static void SaveObjectsWSDL(KnowledgeBase KB, IOutputService output)
        {
            Navigation.SaveObjectsWSDL(KB, output, false);
        }
        //
        public static void CompareWSDL(KnowledgeBase KB, IOutputService output)
        {
            Navigation.CompareWSDLDirectories(KB, output);
        }
        //
        public static List<KBObject> ObjectsWithoutINOUT(KnowledgeBase KB, IOutputService output)
        {
            return Objects.ParmWOInOut(KB, output);
        }
        //
        public static void PreProcessPendingObjects(KnowledgeBase KB, IOutputService output, List<KBObject> objs, out List<string[]> lineswriter)
        {

            const string KBDOCTOR_OUTPUTID = "KBDoctor";
            output.SelectOutput(KBDOCTOR_OUTPUTID);

            FileIniDataParser fileIniData = new FileIniDataParser();
            InitializeIniFile(KB);

            string filename = KB.UserDirectory + "\\KBDoctor.ini";
            IniData parsedData = fileIniData.ReadFile(filename);
            string SectionName = "ReviewObject";

            List<Tuple<KBObject, string>> recommended_list = new List<Tuple<KBObject, string>>();

            List<KBObject> atts = new List<KBObject>();
            foreach (KBObject obj in objs)
            {
                string recommendations = "";
                List<KBObject> objlist = new List<KBObject>();
                objlist.Add(obj);
                if (Utility.isRunable(obj) && !Utility.IsGeneratedByPattern(obj))
                {

                    //Check objects with parameteres without inout
                    if (CheckKeyInINI(parsedData, SectionName, "ParamINOUT", "true", "Check if all parameters have IN: OUT: INOUT: keywords", filename))
                        Objects.ParmWOInOut(objlist, output, ref recommendations);

                    //Clean variables not used
                    if (CheckKeyInINI(parsedData, SectionName, "CleanUnusedVariables", "true", "Remove unused variables from objects", filename))
                        CleanKB.CleanKBObjectVariables(obj, output, ref recommendations);

                    //Check commit on exit
                    if (CheckKeyInINI(parsedData, SectionName, "CheckCommitOnExit", "true", "Check if property Commit on exit = YES", filename))
                        Objects.CommitOnExit(objlist, output, ref recommendations);

                    //Is in module
                    if (CheckKeyInINI(parsedData, SectionName, "CheckModule", "true", "Use of modules is required", filename))
                        Objects.isInModule(objlist, output, ref recommendations);

                    //Fix variables not based in domains or attributes
                    bool fixvar = true;
                    if (CheckKeyInINI(parsedData, SectionName, "FixVariables", "false", "Fix variables definition, assinging Attribute or Domain", filename))
                        fixvar = false;

                    //With variables not based on attributes
                    if (CheckKeyInINI(parsedData, SectionName, "VariablesBasedAttOrDomain", "true", "Variables must be based on Attributes or Domains", filename))
                        Objects.ObjectsWithVarNotBasedOnAtt(objlist, output, fixvar, ref recommendations);

                    //Code commented
                    if (CheckKeyInINI(parsedData, SectionName, "CodeCommented", "true", "Code commented is marked as error", filename))
                        Objects.CodeCommented(objlist, output, ref recommendations);

                    //Assign types comparer
                    if (CheckKeyInINI(parsedData, SectionName, "AssignTypes", "true", "Check if assignments have the correct Type or Domain", filename))
                        AssignTypesComprarer(KB, objlist, ref recommendations);
                    //Parameter types comparer
                    if (CheckKeyInINI(parsedData, SectionName, "ParameterTypes", "true", "Check if call parameters have the correct Type or Domain", filename))
                        ParametersTypeComparer(KB, objlist, ref recommendations);
                    //Empty conditional blocks
                    if (CheckKeyInINI(parsedData, SectionName, "EmptyConditionalBlocks", "true", "Checks if exists any IF/Else block without code in it", filename))
                        EmptyConditionalBlocks(KB, objlist);
                    //Constants in code
                    if (CheckKeyInINI(parsedData, SectionName, "ConstantsInCode", "true", "Check if there are hardcoded constants", filename))
                        ConstantsInCode(KB, objlist);
                    //For eachs without when none
                    if (CheckKeyInINI(parsedData, SectionName, "ForEachsWithoutWhenNone", "true", "Check if there is any 'ForEach' block without a 'When None' clause", filename))
                        ForEachsWithoutWhenNone(KB, objlist);
                    //News without when duplicate
                    if (CheckKeyInINI(parsedData, SectionName, "NewsWithoutWhenDuplicate", "true", "Check if there is any 'New' block without 'When Duplicate' clause", filename))
                        NewsWithoutWhenDuplicate(KB, objlist);
                    if (CheckKeyInINI(parsedData, SectionName, "ProceduresCalledAsFuction", "true", "Check if the procedures are called as functions", filename))
                        ProceduresCalledAsFunction(KB, objlist, ref recommendations);


                    //Check complexity metrics
                    //maxNestLevel  6 - ComplexityLevel  30 - MaxCodeBlock  500 - parametersCount  6

                    bool aux;
                    aux = CheckKeyInINI(parsedData, SectionName, "MaxNestLevel", "7", "Maximun nesting level allowed in source", filename);
                    aux = CheckKeyInINI(parsedData, SectionName, "MaxComplexity", "30", "Maximun Complexity level allowed in sources", filename);
                    aux = CheckKeyInINI(parsedData, SectionName, "MaxBlockSize", "500", "Maximun block of code", filename);
                    aux = CheckKeyInINI(parsedData, SectionName, "MaxParameterCount", "6", "Maximun Number of parameters allowed in parm rule", filename);

                    int maxNestLevel = 7;
                    Int32.TryParse(parsedData[SectionName]["MaxNestLevel"], out maxNestLevel);

                    int complexityLevel = 30;
                    Int32.TryParse(parsedData[SectionName]["MaxComplexity"], out complexityLevel);

                    int maxCodeBlock = 500;
                    Int32.TryParse(parsedData[SectionName]["MaxBlockSize"], out maxCodeBlock);

                    int maxParametersCount = 6;
                    Int32.TryParse(parsedData[SectionName]["MaxParameterCount"], out maxParametersCount);

                    Objects.CheckComplexityMetrics(objlist, output, maxNestLevel, complexityLevel, maxCodeBlock, maxParametersCount, ref recommendations);

                }
                if (obj is Artech.Genexus.Common.Objects.Attribute && CheckKeyInINI(parsedData, SectionName, "AttributeBasedOnDomain", "true", "Attributes must be based on domains", filename))
                {
                    atts.Add(obj);
                    //Attribute Has Domain
                    Objects.AttributeHasDomain(objlist, output, ref recommendations);
                }
                if (obj is SDT && CheckKeyInINI(parsedData, SectionName, "SDTBasedAttOrDomain", "true", "SDT items must be based on attributes or domains", filename))
                {
                    //SDTItems Has Domain
                    Objects.SDTBasedOnAttDomain(objlist, output, ref recommendations);
                }
                if (obj is Transaction && CheckKeyInINI(parsedData, SectionName, "AttributeBasedOnDomain", "true", "Attributes must be based on domains", filename))
                {
                    Objects.AttributeHasDomain(Objects.GetAttributesFromTrn((Transaction)obj), output, ref recommendations);
                }

                if (recommendations != "")
                {
                    Tuple<KBObject, string> recommend_tuple = new Tuple<KBObject, string>(obj, recommendations);
                    recommended_list.Add(recommend_tuple);
                }

            }
            if (atts.Count > 0 && CheckKeyInINI(parsedData, SectionName, "AttributeWithoutTable", "true", "All attributes must be in table", filename))
            {
                // Attributes without table
                Objects.AttributeWithoutTable(atts, output);
            }

            output.AddLine("KBDoctor", "KBDoctor Review Object finished");
            output.UnselectOutput(KBDOCTOR_OUTPUTID);
            output.SelectOutput("General");
            lineswriter = new List<string[]>();

            foreach (Tuple<KBObject, string> item in recommended_list)
            {
                string[] line = new string[] { Utility.linkObject(item.Item1), item.Item2 };
                lineswriter.Add(line);
            }

            /*
* Tiene todas las referencias?
* Tiene calls que pueden ser UDP
* Mas de un parametro de salida
* Constantes en el código
* Nombre "poco claro" / Descripcion "poco clara"
* Si es modulo, revisar que no tenga objetos publicos no llamados
* Si es modulo, revisar que no tenga objetos privados llamados desde fuera
* Si es modulo, Valor de la propiedad ObjectVisibility <> Private
* Atributo Varchar que debe ser char
* Atributo Char que debe ser varchar
* Column Title muy ancho para el ancho del atributo
* Nombre del Control en pantalla por Default
* Todos los eventos son invocados
*
*/

        }

        private static bool CheckKeyInINI(IniData parsedData, string SectionName, string KeyName, string KeyValue, string Comment, string INIfilename)
        {
            string key = SectionName + parsedData.SectionKeySeparator + KeyName;
            string value;
            parsedData.TryGetKey(key, out value);

            if (value == "") {

                AddKeyToIni(parsedData, SectionName, KeyName, KeyValue, Comment);
                var parser = new FileIniDataParser();
                //Save the file
                parser.WriteFile(INIfilename, parsedData);
                value = KeyValue; //
            }
            return value.ToLower() == KeyValue;

        }

        public static void InitializeIniFile(KnowledgeBase KB)
        {
            string filename = KB.UserDirectory + @"\KBDoctor.ini";

            if (!File.Exists(filename))
            {
                var parser = new FileIniDataParser();

                IniData data = new IniData();

                string SectionName = "ReviewObject";

                data.Sections.AddSection(SectionName);

                AddKeyToIni(data, SectionName, "CleanUnusedVariables", "true", "Remove unused variables from objects");
                AddKeyToIni(data, SectionName, "FixVariables", "false", "Fix variables definition, assinging Attribute or Domain");
                AddKeyToIni(data, SectionName, "ParamINOUT", "true", "Check if all parameters have IN: OUT: INOUT: keywords");
                AddKeyToIni(data, SectionName, "CheckCommitOnExit", "true", "Check if property Commit on exit = YES");
                AddKeyToIni(data, SectionName, "CheckModule", "true", "Use of modules is required");
                AddKeyToIni(data, SectionName, "CodeCommented", "true", "Code commented is marked as error");

                AddKeyToIni(data, SectionName, "VariablesBasedAttOrDomain", "true", "Variables must be based on Attributes or Domains");
                AddKeyToIni(data, SectionName, "AttributeBasedOnDomain", "true", "Attributes must be based on domains");
                AddKeyToIni(data, SectionName, "SDTBasedAttOrDomain", "true", "SDT items must be based on attributes or domains");
                AddKeyToIni(data, SectionName, "AttributeWithoutTable", "true", "All attributes must be in table");
                AddKeyToIni(data, SectionName, "AssignTypes", "true", "Check if assignments have the correct Type or Domain");
                AddKeyToIni(data, SectionName, "ParameterTypes", "true", "Check if assignments have the correct Type or Domain");
                AddKeyToIni(data, SectionName, "EmptyConditionalBlocks", "true", "Check if there is any IF/Else block without code in it");
                AddKeyToIni(data, SectionName, "ConstantsInCode", "true", "Check if there are hardcoded constants");
                AddKeyToIni(data, SectionName, "ForEachsWithoutWhenNone", "true", "Check if there is any 'ForEach' block without a 'When None' clause");
                AddKeyToIni(data, SectionName, "NewsWithoutWhenDuplicate", "true", "Check if there is any 'New' block without 'When Duplicate' clause");
                AddKeyToIni(data, SectionName, "ProceduresCalledAsFuction", "true", "Check if the procedures are called as functions");

                AddKeyToIni(data, SectionName, "MaxNestLevel", "7", "Maximun nesting level allowed in source");
                AddKeyToIni(data, SectionName, "MaxComplexity", "30", "Maximun Complexity level allowed in sources");
                AddKeyToIni(data, SectionName, "MaxBlockSize", "300", "Maximun block of code");
                AddKeyToIni(data, SectionName, "MaxParameterCount", "6", "Maximun Number of parameters allowed in parm rule");

                //Save the file
                parser.WriteFile(filename, data);
            }
        }

        private static void AddKeyToIni(IniData data, string SectionName, string TestKey, string TestValue, string Comment)
        {
            KeyData key = new KeyData(TestKey);
            key.Value = TestValue;
            key.Comments.Add(Comment);
            data[SectionName].AddKey(key);
        }

        public static List<KBObject> ObjectsUpdateAttribute(List<KBObject> updaters, Artech.Genexus.Common.Objects.Attribute att, IOutputService output)
        {
            return Objects.ObjectsUpdateAttribute(updaters, att, output);
        }

        public static List<KBObject> ObjectsUpdatingTable(Table Table, IOutputService output)
        {
            return Objects.ObjectsUpdatingTable(Table);
        }

        public static bool ThemeClassesNotUsed(KnowledgeBase KB, IOutputService output, ThemeClass themeclass)
        {
            return Objects.ThemeClassesNotUsed(KB, output, themeclass);
        }

        public static bool AssignTypesComprarer(KnowledgeBase KB, List<KBObject> objs, ref string recommendations)
        {
            foreach (KBObject obj in objs)
            {
                Objects.AssignTypeComparer(KB.DesignModel, obj, ref recommendations);
            }
            return true;
        }

        public static void ParametersTypeComparer(KnowledgeBase KB, List<KBObject> objs, ref string recommendations)
        {
            foreach (KBObject obj in objs)
            {
                Objects.ParameterTypeComparer(KB.DesignModel, obj, ref recommendations);
            }
        }

        public static void ConstantsInCode(KnowledgeBase KB, List<KBObject> objs)
        {  
            foreach (KBObject obj in objs)
            {
                Objects.ConstantsInCode(KB.DesignModel, obj);
            }
        }

        public static void ReivewCommits(KnowledgeBase KB)
        {
            //DateTime ayer = new DateTime(2018,08,01);
            DateTime ayer = DateTime.Today.AddDays(-1);
            Objects.TeamDevTest(KB, "http://concepto.genexusserver.com/xev3", "local\\*******", "*******", "LuciaX", "LuciaX", ayer, DateTime.Today);
        }
        public static void EmptyConditionalBlocks(KnowledgeBase KB, List<KBObject> objs)
        {
            foreach (KBObject obj in objs)
            {
                Objects.EmptyConditionalBlocks(KB.DesignModel, obj);
            }
        }

        public static void ForEachsWithoutWhenNone(KnowledgeBase KB, List<KBObject> objs)
        {
            foreach (KBObject obj in objs)
            {
                Objects.ForEachsWithoutWhenNone(KB.DesignModel, obj);
            }
        }

        public static void NewsWithoutWhenDuplicate(KnowledgeBase KB, List<KBObject> objs)
        {
            foreach (KBObject obj in objs)
            {
                Objects.NewsWithoutWhenDuplicate(KB.DesignModel, obj);
            }
        }

        public static void ProceduresCalledAsFunction(KnowledgeBase KB, List<KBObject> objs, ref string recommendations)
        {
            foreach (KBObject obj in objs)
            {
                Objects.ProceduresCalledAsFunction(KB.DesignModel, obj, ref recommendations);
            }
        }



#if EVO3
        public class Tuple<T1, T2>
        {
            public T1 Item1 { get; private set; }
            public T2 Item2 { get; private set; }
            public Tuple(T1 item1, T2 item2)
            {
                Item1 = item1;
                Item2 = item2;
            }
        }

        public static class Tuple
        {
            public static Tuple<T1, T2> New<T1, T2>(T1 item1, T2 item2)
            {
                var tuple = new Tuple<T1, T2>(item1, item2);
                return tuple;
            }
        }
#endif

    }
}

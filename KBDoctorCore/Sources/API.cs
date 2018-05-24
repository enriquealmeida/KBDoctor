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
        public static void CleanKBObjects(KnowledgeBase kb, IEnumerable<KBObject> kbojs, IOutputService output)
        {
            CleanKB.CleanObjects(kb,kbojs,output);
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
        public static void PreProcessPendingObjects(KnowledgeBase KB, IOutputService output, List<KBObject> objs)
        {
            FileIniDataParser fileIniData = new FileIniDataParser();

            output.Clear();
            output.SelectOutput("KBDoctor");
            output.StartSection("KBDoctor", "Review_Objects", "Review Objects");


            InitializeIniFile(KB);

            IniData parsedData = fileIniData.ReadFile(KB.UserDirectory + "\\KBDoctor.ini");
            string SectionName = "ReviewObject";

            List<KBObject> atts = new List<KBObject>();
            foreach (KBObject obj in objs)
            {
                List<KBObject> objlist = new List<KBObject>();
                objlist.Add(obj);
                if (Utility.isRunable(obj) && !Utility.IsGeneratedByPattern(obj))
                {
                    //Check objects with parameteres without inout
                    if (parsedData[SectionName]["ParamINOUT"].ToLower() == "true")
                           Objects.ParmWOInOut(objlist, output);

                    //Clean variables not used
                    if (parsedData[SectionName]["CleanUnusedVariables"].ToLower() == "true") 
                        CleanKB.CleanKBObjectVariables(obj, output);

                    //Check commit on exit
                    if (parsedData[SectionName]["CheckCommitOnExit"].ToLower() == "true")
                        Objects.CommitOnExit(objlist, output);

                    //Is in module
                    if (parsedData[SectionName]["CheckModule"].ToLower() == "true")
                        Objects.isInModule(objlist, output);

                    //With variables not based on attributes
                    if (parsedData[SectionName]["VariablesBasedAttOrDomain"].ToLower() == "true")
                        Objects.ObjectsWithVarNotBasedOnAtt(objlist, output);

                    //Code commented
                    if (parsedData[SectionName]["CodeCommented"].ToLower() == "true")
                        Objects.CodeCommented(objlist, output);

                    //Check complexity metrics
                    //maxNestLevel  6 - ComplexityLevel  30 - MaxCodeBlock  500 - parametersCount  6
                    int maxNestLevel = 7;
                    Int32.TryParse( parsedData[SectionName]["MaxNestLevel"], out maxNestLevel);

                    int complexityLevel = 30;
                    complexityLevel = Int32.Parse(parsedData[SectionName]["MaxComplexity"]);

                    int maxCodeBlock = 500;
                    maxCodeBlock = Int32.Parse(parsedData[SectionName]["MaxBlockSize"]);

                    int maxParametersCount = 6;
                    maxParametersCount = Int32.Parse(parsedData[SectionName]["MaxParameterCount"]);

                    Objects.CheckComplexityMetrics(objlist, output, maxNestLevel, complexityLevel, maxCodeBlock, maxParametersCount);



                    /*
                    * Tiene todas las referencias?
                    * Tiene calls que pueden ser UDP
                    * Mas de un parametro de salida
                    * Constantes en el código
                    * Nombre "poco claro" / Descripcion "poco clara"
                    * Si es modulo, revisar que no tenga objetos publicos no llamados
                    * Si es modulo, revisar que no tenga objetos privados llamados desde fuera
                    * Si es modulo, Valor de la propiedad ObjectVisibility <> Private
                    */
                }
                if (obj is Artech.Genexus.Common.Objects.Attribute && parsedData[SectionName]["AttributeBasedOnDomain"].ToLower() == "true")
                {
                    atts.Add(obj);
                    //Attribute Has Domain
                    Objects.AttributeHasDomain(objlist, output);
                }
                if (obj is SDT && parsedData[SectionName]["SDTBasedAttOrDomain"].ToLower() == "true")
                {
                    //SDTItems Has Domain
                     Objects.SDTBasedOnAttDomain(objlist, output);
                }
            }
            if (atts.Count > 0 && parsedData[SectionName]["AttributeWithoutTable"].ToLower() == "true")
            {
                // Attributes without table
                Objects.AttributeWithoutTable(atts, output);
            }

            output.EndSection("KBDoctor", "Review_Objects", true); 
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
                AddKeyToIni(data, SectionName, "ParamINOUT", "true", "Check if all parameters have IN: OUT: INOUT: keywords");
                AddKeyToIni(data, SectionName, "CheckCommitOnExit", "true", "Check if property Commit on exit = YES");
                AddKeyToIni(data, SectionName, "CheckModule", "true", "Use of modules is required");
                AddKeyToIni(data, SectionName, "CodeCommented", "true", "Code commented is marked as error");
                AddKeyToIni(data, SectionName, "VariablesBasedAttOrDomain", "true", "Variables must be based on Attributes or Domains");
                AddKeyToIni(data, SectionName, "AttributeBasedOnDomain", "true", "Attributes must be based on domains");
                AddKeyToIni(data, SectionName, "SDTBasedAttOrDomain", "true", "SDT items must be based on attributes or domains");
                AddKeyToIni(data, SectionName, "AttributeWithoutTable", "true", "All attributes must be in table");

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
    }
}

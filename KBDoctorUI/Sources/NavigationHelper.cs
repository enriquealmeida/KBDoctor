using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Xsl;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

using Artech.Architecture.Common.Collections;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Helpers;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Udm.Framework.References;
using Artech.Architecture.UI.Framework.Helper;
using Artech.Common.Framework.Commands;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Collections;

using Concepto.Packages.KBDoctorCore.Sources;

namespace Concepto.Packages.KBDoctor
{
    static class NavigationHelper
    {
        public static void ListObjSimilarNavigation()
        {

            IKBService kbserv = UIServices.KB;
            List<string> objWarnErr = new List<string>();

            string title = "KBDoctor - Similar Navigations";
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);


                IOutputService output = CommonServices.Output;
                KBDoctorOutput.StartSection(title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);

                string path2 = kbserv.CurrentKB.UserDirectory + @"\Navigation.txt";
                try
                {
                    File.Delete(path2);
                }
                catch (Exception e) { Console.WriteLine(e.Message); };

                Stream stream = File.Open(path2, FileMode.OpenOrCreate, FileAccess.Write);
                TextWriter writer2 = new StreamWriter(stream);

                writer.AddHeader(title);
                GxModel gxModel = kbserv.CurrentKB.DesignModel.Environment.TargetModel.GetAs<GxModel>();

                var fileWildcardsArg = new[] { "*.xml" };
                var directoryArg = KBDoctorHelper.SpcDirectory(kbserv);

                writer.AddTableHeader(new string[] { "Type", "Tables", "Attributes", "Object/Line/Event" });

                foreach (string d in Directory.GetDirectories(directoryArg, "NVG", System.IO.SearchOption.AllDirectories))
                {
                    ProcesoDirNavigations(d, output, writer2);
                }

                writer2.Close();

                //    string inFile = @"Navigation.txt";
                string outFile = kbserv.CurrentKB.UserDirectory + @"\NavigationOrdered.csv";
                var contents = File.ReadAllLines(path2);
                //string[]  q = contents.Distinct().ToArray();
                Array.Sort(contents);
                File.WriteAllLines(outFile, contents);

                string clave = "";
                string objetos = "";
                string objeto = "";
                string objetosAnterior = "";
                int numObj = 1;

                string claveanterior = "";
                string tableanterior = "";
                string attanterior = "";
                string LevelTypeanterior = "";

                TextFieldParser parser = new TextFieldParser(outFile);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Process row
                    string[] fields = parser.ReadFields();
                    string LevelType = fields[0].Replace(" ", "_");
                    string Tables = fields[1];
                    string Atts = fields[2];
                    string ObjLink = fields[3];
                    string ObjEvent = fields[4];
                    string ObjLine = fields[5];

                    clave = LevelType + Tables + Atts;
                    objeto = ObjLink + " / " + ObjLine + " / " + ObjEvent + "<BR>";
                    if (clave == claveanterior)
                    {
                        if (!objetos.Contains(objeto))
                        {
                            objetos += objeto;
                            numObj += 1;
                        }
                    }
                    else
                    {
                        if (numObj > 1)
                        {
                            writer.AddTableData(new string[] { LevelTypeanterior, tableanterior, attanterior, objetosAnterior });
                        }
                        objetos = objeto;
                        numObj = 1;
                    }
                    claveanterior = clave;
                    tableanterior = Tables;
                    attanterior = Atts;
                    LevelTypeanterior = LevelType;
                    objetosAnterior = objetos;
                }
                parser.Close();

                writer.AddFooter();
                writer.Close();


                KBDoctorHelper.ShowKBDoctorResults(outputFile);
                bool success = true;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
            catch (Exception ex)
            {
                bool success = false;
                KBDoctorOutput.Error(ex.Message);
                KBDoctor.KBDoctorOutput.EndSection(title, success);
            }
        }

        public static void ProcesoDirNavigations(string directoryArg, IOutputService output, TextWriter writer2)
        {

            IKBService kbserv = UIServices.KB;

            string fileWildcard = @"*.xml";
            var searchSubDirsArg = System.IO.SearchOption.AllDirectories;
            string[] xFiles = System.IO.Directory.GetFiles(directoryArg, fileWildcard, searchSubDirsArg);
            int numFiles = 0;

            foreach (string x in xFiles)
            {

                if (!Path.GetFileNameWithoutExtension(x).StartsWith("Gx0"))
                {

                    //if ((numFiles % 200) == 0 )
                            KBDoctorOutput.Message(x);
                    numFiles += 1;

                    string xmlstring = AddXMLHeader(x);

                    KBObject obj = ExtractObject(xmlstring);
                   // if (!ObjectsHelper.isGeneratedbyPattern(obj))
                   // {
                        ProcesoNavigation(xmlstring, output, writer2, obj);
                   /// }
                }
            }
        }

        private static KBObject ExtractObject(string xmlstring)
        {
            IKBService kbserv = UIServices.KB;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlstring);
                string objName = FirstDescendantValue(doc, "ObjName");
                string objType = FirstDescendantValue(doc, "ObjClsName");
                KBObject obj = GetObjectFromNavigationName(kbserv.CurrentModel, objType, objName);
                if (obj != null)
                {
                    return obj;
                }

                if (objName.EndsWith("_BC", StringComparison.OrdinalIgnoreCase))
                {
                    return GetObjectFromNavigationName(kbserv.CurrentModel, objType, objName.Substring(0, objName.Length - 3));
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); };
            return null;

        }

        private static KBObject GetObjectFromNavigationName(KBModel model, string objType, string qualifiedObjectName)
        {
            if (string.IsNullOrEmpty(qualifiedObjectName))
            {
                return null;
            }

            string moduleName = "";
            string objectName = qualifiedObjectName;
            int lastDot = qualifiedObjectName.LastIndexOf('.');
            if (lastDot >= 0)
            {
                moduleName = qualifiedObjectName.Substring(0, lastDot);
                objectName = qualifiedObjectName.Substring(lastDot + 1);
            }

            QualifiedName qualifiedName = new QualifiedName(moduleName, objectName);
            if (!string.IsNullOrEmpty(objType))
            {
                KBObjectDescriptor descriptor = KBObjectDescriptor.Get(objType);
                if (descriptor != null)
                {
                    KBObject typedObject = model.Objects.Get(descriptor.Id, qualifiedName);
                    if (typedObject != null)
                    {
                        return typedObject;
                    }
                }
            }

            foreach (KBObject obj in model.Objects.GetByPartialName(new[] { "Objects" }, objectName))
            {
                if (string.Equals(obj.QualifiedName.ToString(), qualifiedObjectName, StringComparison.OrdinalIgnoreCase)
                    || (string.Equals(obj.Name, objectName, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(obj.QualifiedName.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase)))
                {
                    return obj;
                }
            }

            return null;
        }

        private static void ProcesoNavigation(string xmlstring, IOutputService output, TextWriter writer2, KBObject obj)
        {
            //Create the XmlDocument.
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlstring);

            string ObjName = "";
            string EventName = "";
            string LevelType = "";
            string LevelRow = "";
            string TableNames = "";
            string AttNames = "";

            using (XmlReader reader = new XmlTextReader(new System.IO.StringReader(xmlstring)))
            {
                while (reader.Read())
                {

                    //  string inner = reader.ReadInnerXml();

                    switch (reader.Name)
                    {
                        case "ObjName":
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                reader.Read();
                                if (ObjName == "")
                                    ObjName = reader.Value;
                            }
                            break;

                        case "EventName":  //<EventName>Sbfhoras.Load</EventName>
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                reader.Read();
                                EventName = reader.Value;
                            }
                            //reader.Read();
                            //EventName = reader.Value;
                            break;

                        case "LevelType":  //<LevelType>For First</LevelType>
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                //IMPRIMO EL LEVEL ANTERIOR
                                //      if (LevelType!="")
                                //             KBDoctorOutput.Message(String.Format("OBJECT= {0} EVENTNAME= {1} ROW= {2} LEVELTYPE= {3} TABLES= {4} ATTRIBUTES= {5} ", ObjName, EventName, LevelRow, LevelType, TableNames, AttNames));
                                reader.Read();
                                LevelType = reader.Value;
                                //VACIO TABLAS Y ATRIBUTOS PUES CAMBIO DE LEVEL
                                TableNames = "";
                                AttNames = "";

                            }
                            break;

                        case "LevelBeginRow":  //<LevelBeginRow>31</LevelBeginRow>
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                reader.Read();
                                LevelRow = reader.Value;
                            }
                            break;

                        case "NavigationTree":

                            while (reader.Read() && !(reader.Name == "NavigationTree" && reader.NodeType == XmlNodeType.EndElement))
                            {
                                switch (reader.Name)
                                {
                                    case "TableName":
                                        if (reader.NodeType == XmlNodeType.Element)
                                        {
                                            reader.Read();
                                            TableNames += reader.Value + " ";
                                        }
                                        break;

                                    case "AttriName":
                                        if (reader.NodeType == XmlNodeType.Element)
                                        {
                                            reader.Read();
                                            AttNames += reader.Value + " ";
                                        }
                                        break;
                                }
                            }
                            if (LevelType != "")
                            {
                                string aux = LevelType + "," + TableNames + " ,  " + AttNames;
                                string hash;
                                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                                {
                                    hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(aux))
                                                    ).Replace("-", String.Empty);
                                }
                                //Cuento Cantidad de tablas.
                                hash = TableNames.Count(Char.IsWhiteSpace).ToString("D2") + hash;

                               KBDoctorOutput.Message(String.Format("{0} ,  {1} ,  {2} ,  {3} , {4}, {5}  ", ObjName, EventName, LevelRow, LevelType, TableNames, AttNames));
                               // writer.AddTableData(new string[] { hash, Utility.linkObject(obj), EventName, LevelRow, LevelType, TableNames, AttNames });
                                writer2.WriteLine( LevelType +  "," + TableNames + ","  + AttNames + "," + ObjName + "," + EventName + "," + LevelRow.PadLeft(10,' '));
                                LevelType = "";
                                LevelRow = "";
                                TableNames = "";
                                AttNames = "";
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }


        public static void PrepareComparerNavigations(KnowledgeBase KB, IOutputService output)
        {
            string title = "KBDoctor - Prepare Comparer Navigation Files";
            KBDoctorOutput.StartSection(title);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            IKBService kbserv = UIServices.KB;
            string directoryArg = KBDoctorHelper.NvgComparerDirectory(kbserv);
            string fechahora = String.Format("{0:yyyy-MM-dd-HHmm}", DateTime.Now);
            string newDir = directoryArg + @"\NVG-" + fechahora + @"\";
            Directory.CreateDirectory(newDir);
            WriteXSLTtoDir();
            foreach (string d in Directory.GetDirectories(KBDoctorHelper.SpcDirectory(kbserv), "NVG", System.IO.SearchOption.AllDirectories))
            {
                string generator = d.Replace(KBDoctorHelper.SpcDirectory(kbserv), "");
                generator = generator.Replace("NVG_", "");
                generator = @"\" + generator.Replace(@"\", "_") + "_";
                generator = generator.Replace("NVG_", "");

                ProcesoDir(d, newDir, generator, output);
            }


            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            KBDoctorOutput.Message(title + " elepsed time: " + elapsedTime);
            KBDoctorOutput.EndSection(title, true);

        }

        private static void WriteXSLTtoDir()
        {
            IKBService kbserv = UIServices.KB;
            string outputFile = kbserv.CurrentKB.UserDirectory + @"\KBdoctorEv2.xslt";
            File.WriteAllText(outputFile, StringResources.specXEv3_V15);

        }


        public static void ProcesoDir(string directoryArg, string newDir, string generator, IOutputService output)
        {

            IKBService kbserv = UIServices.KB;
            string outputFile = kbserv.CurrentKB.UserDirectory + @"\KBdoctorEv2.xslt";
            XslCompiledTransform xslTransform = new XslCompiledTransform();

            XsltSettings settings = new XsltSettings(true, true);

            xslTransform.Load(outputFile,settings, new XmlUrlResolver() );

            string fileWildcard = @"*.xml";
            var searchSubDirsArg = System.IO.SearchOption.AllDirectories;
            string[] xFiles = System.IO.Directory.GetFiles(directoryArg, fileWildcard, searchSubDirsArg);

            foreach (string x in xFiles)
            {

                if (!Path.GetFileNameWithoutExtension(x).StartsWith("Gx0"))
                {
                    KBDoctorOutput.Message(x);
                    string xTxt = newDir + generator + Path.GetFileNameWithoutExtension(x) + ".nvg";


                    string xmlstring = AddXMLHeader(x);

                    string newXmlFile = x.Replace(".xml", ".xxx");
                    File.WriteAllText(newXmlFile, xmlstring);

                    try
                    {
                        xslTransform.Transform(newXmlFile, xTxt);
                    }
                    catch(Exception e)
                    {
                        KBDoctorOutput.Error(x + ' ' + e.Message);
                    }

                    //  xslt.Transform(newXmlFile, xTxt);

                    File.Delete(newXmlFile);
                }


            }
        }
        public static void ListObjWarningsErrors()
        {

            IKBService kbserv = UIServices.KB;
            List<string> objWarnErr = new List<string>();

            string title = "KBDoctor - Warnings and Errors";
            try
            {
                string outputFile = Utility.CreateOutputFile(kbserv, title);

                IOutputService output = CommonServices.Output;
                KBDoctorOutput.StartSection(title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);

                writer.AddHeader(title);
                GxModel gxModel = kbserv.CurrentKB.DesignModel.Environment.TargetModel.GetAs<GxModel>();

                var fileWildcardsArg = new[] { "*.xml" };
                var directoryArg = KBDoctorHelper.SpcDirectory(kbserv);
                var searchSubDirsArg = System.IO.SearchOption.AllDirectories;
                var ignoreCaseArg = true;

                writer.AddTableHeader(new string[] { "Error Type", "ObjClass", "Object", "Description", "UserName", "Observation" });

                SearchNVGFiles(output, writer, fileWildcardsArg, directoryArg, searchSubDirsArg, "<Error>", ignoreCaseArg, objWarnErr);
                SearchNVGFiles(output, writer, fileWildcardsArg, directoryArg, searchSubDirsArg, "deprecated", ignoreCaseArg, objWarnErr);
                SearchNVGFiles(output, writer, fileWildcardsArg, directoryArg, searchSubDirsArg, "<Warning>", ignoreCaseArg, objWarnErr);
                SearchNVGFiles(output, writer, fileWildcardsArg, directoryArg, searchSubDirsArg, "<Icon>client<", ignoreCaseArg, objWarnErr);
                SearchNVGFiles(output, writer, fileWildcardsArg, directoryArg, searchSubDirsArg, "<JoinLocation>0</JoinLocation>", ignoreCaseArg, objWarnErr);


                writer.AddFooter();
                //agrego lista de objetos para que sea facil hacerle un BUILD WITH THIS ONLY
                string lstObjWarn = "";
                string puntoycoma = "";
                foreach (string objstr in objWarnErr)
                {
                    lstObjWarn += puntoycoma + Path.GetFileNameWithoutExtension(objstr);
                    puntoycoma = ";";
                }
                writer.AddTableData(new string[] { lstObjWarn });
                writer.AddTableData(new string[] { "  " + objWarnErr.Count.ToString() });

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

        private static void SearchNVGFiles(IOutputService output, KBDoctorXMLWriter writer, string[] fileWildcardsArg, string directoryArg, System.IO.SearchOption searchSubDirsArg, string containsTextArg, bool ignoreCaseArg, List<string> objWarnErr)
        {
            KBDoctorOutput.Message(">>Searching for " + containsTextArg);
            foreach (string file in FindInFiles(directoryArg, containsTextArg, ignoreCaseArg, searchSubDirsArg, (string[])fileWildcardsArg))
            {
                if (!objWarnErr.Contains(file))
                {
                    AddFileWithWarningsErrors(output, writer, containsTextArg, file);
                    objWarnErr.Add(file);
                }

            }
        }

        private static void AddFileWithWarningsErrors(IOutputService output, KBDoctorXMLWriter writer, string containsTextArg, string file)
        {
            KBDoctorOutput.Message(file);
            string objName = Path.GetFileNameWithoutExtension(file);
            containsTextArg = containsTextArg.Replace("<", "");
            containsTextArg = containsTextArg.Replace(">", "");
            containsTextArg = containsTextArg.Replace("Icon", "");


            string[] ns = new[] { "Objects" };

            bool objExists = false;
            foreach (KBObject obj in UIServices.KB.CurrentModel.Objects.GetByPartialName(ns, objName))
            {
                objExists = true;
                if (obj.Name == objName)
                {
                    string spcTxt = ExtractSpcInfo(file, containsTextArg);
                    writer.AddTableData(new string[] { containsTextArg, obj.TypeDescriptor.Name, Utility.linkObject(obj), obj.Description, obj.UserName, spcTxt });

                }

            }
            if (!objExists)
                writer.AddTableData(new string[] { containsTextArg, "", objName, "Don't exist", "", file });
        }
        /// <summary>
        /// Get a list of files based on filename-with-wildcard search criteria and file-content search criteria.
        /// Regular expressions are not supported (yet).
        /// Calls System.IO.Directory.GetFiles to get files.
        /// Calls System.IO.File.ReadAllText().Contains to search contents.
        /// Uses ToLower() to perform case-insensitive search.
        /// </summary>
        /// <param name="directoryArg">Directory to start search, such as @"C:\" or Environment.GetEnvironmentVariable("SystemRoot")</param>
        /// <param name="containsTextArg">Test to search for. "" will be found in any file.</param>
        /// <param name="ignoreCaseArg"></param>
        /// <param name="searchSubDirsArg"></param>
        /// <param name="fileWildcardsArg">Can be an array of files or a single file such as "*.ini"</param>
        /// <returns>a list of files (complete paths) found.</returns>

        static IEnumerable<string> FindInFiles(
            string directoryArg,
            string containsTextArg,
            bool ignoreCaseArg,
            System.IO.SearchOption searchSubDirsArg,
            params string[] fileWildcardsArg)
        {
            List<String> files = new List<string>(); // This List accumulates files found.

            foreach (string fileWildcard in fileWildcardsArg)
            {
                string[] xFiles = System.IO.Directory.GetFiles(directoryArg, fileWildcard, searchSubDirsArg);

                foreach (string x in xFiles)
                {
                    if (!files.Contains(x)) // If file not already found...
                    {
                        // See if the file contains the search text.
                        // Assume a null search string matches any file.
                        // Use ToLower to perform a case-insensitive search.
                        bool containsText =
                            containsTextArg.Length == 0 ||
                            ignoreCaseArg ?
                            System.IO.File.ReadAllText(x).ToLower().Contains(containsTextArg.ToLower()) :
                            System.IO.File.ReadAllText(x).Contains(containsTextArg);

                        if (containsText)
                        {
                            files.Add(x); // This file is a keeper. Add it to the list.
                        } // if
                    } // if
                } // foreach file
            } //foreach wildcard
            return files;


        }

        static string ExtractSpcInfo(string file, string containsTextArg)
        {
            string sourcestring = AddXMLHeader(file);

            sourcestring = sourcestring.Replace(System.Environment.NewLine, "");

            Regex re = new Regex("");
            RegexOptions myRegexOptions = RegexOptions.Multiline;
            string mTxt = "";

            switch (containsTextArg)
            {
                case "Error":
                    re = new Regex(@"<Errors>(.)*<\/Errors>", myRegexOptions);
                    break;
                case "Warning":
                    re = new Regex(@"<Warnings>(.)*<\/Warnings>", myRegexOptions);
                    break;
                case "deprecated":
                    re = new Regex(@"<Warnings>(.)*<\/Warnings>", myRegexOptions);
                    break;
                case "client":

                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(sourcestring);

                    XmlNode node = xmlDocument.DocumentElement.SelectSingleNode(@"//Condition[Icon='client']");

                    mTxt = node.InnerXml.ToString();
                    Regex r = new Regex(@"<AttriId>(.)*<\/AttriId>", RegexOptions.Singleline);
                    mTxt = r.Replace(mTxt, string.Empty);

                    Regex r2 = new Regex(@"<Description>(.)*<\/Description>", RegexOptions.Singleline);
                    mTxt = r2.Replace(mTxt, string.Empty);

                    mTxt = mTxt.Replace("client", System.Environment.NewLine);

                    return StripHTML(mTxt);


                default:


                    return Utility.linkFile(file);



            }
            MatchCollection mc = re.Matches(sourcestring);

            foreach (Match m in mc)
            {
                for (int gIdx = 0; gIdx < m.Groups.Count; gIdx++)
                    if (gIdx <= 4)
                        mTxt += m.Groups[gIdx].Value;
            }

            mTxt = StripHTML(mTxt);
            mTxt = mTxt.Replace(">", "");
            return mTxt;
        }


        public static string StripHTML(string HTMLText)
        {
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            return reg.Replace(HTMLText, " ");
        }

        public static void AttUpdated()
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Where update this attribute";
            KBDoctorWebForms.ShowTableAttributeSelectionWithLabels(title, "ApplyAttUpdated", GetTableAttributeOptions(kbserv.CurrentModel));
        }

        public static void ApplyAttUpdated(object[] parameters)
        {
            IKBService kbserv = UIServices.KB;

            string title = "KBDoctor - Where update this attribute";
            string outputFile = string.Empty;
            KBDoctorXMLWriter writer = null;
            bool reportStarted = false;
            bool success = false;
            try
            {
                outputFile = Utility.CreateOutputFile(kbserv, title);

                IOutputService output = CommonServices.Output;
                KBDoctorOutput.StartSection(title);
                reportStarted = true;
                KBDoctorOutput.Message("AttUpdated debug: output file '" + outputFile + "'.");

                string tblName = KBDoctorWebForms.GetParameter(parameters, "tblName");
                string attName = KBDoctorWebForms.GetParameter(parameters, "attName");
                KBDoctorOutput.Message("AttUpdated debug: selected table='" + tblName + "', attribute='" + attName + "'.");
                if (string.IsNullOrEmpty(tblName) || string.IsNullOrEmpty(attName))
                {
                    KBDoctorOutput.Error("Missing table or attribute selection.");
                    return;
                }

                writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                writer.AddHeader(title + attName + " in table " + tblName);
                writer.AddTableHeader(new string[] { "Object", "Description", "Type", "Navigation File" });

                int IndFiles = 0;
                int foundRows = 0;

                //   IKBService kbserv = UIServices.KB;
                string fileWildcard = @"*.xml";
                var searchSubDirsArg = System.IO.SearchOption.AllDirectories;
                int totalFiles = 0;
                int matchingTableFiles = 0;
                int matchingAttributeFiles = 0;

                foreach (string directoryArg in NavigationXmlDirectories(kbserv))
                {
                    KBDoctorOutput.Message("AttUpdated debug: scanning navigation directory '" + directoryArg + "'.");
                    string[] xFiles = System.IO.Directory.GetFiles(directoryArg, fileWildcard, searchSubDirsArg);
                    KBDoctorOutput.Message("AttUpdated debug: directory file count=" + xFiles.Length.ToString() + ".");

                    foreach (string x in xFiles)
                    {


                        if (!Path.GetFileNameWithoutExtension(x).StartsWith("Gx0"))

                        {
                            // KBDoctorOutput.Message(x);
                            totalFiles++;


                            string xmlstring = AddXMLHeader(x);

                            if (ObjectChangesTable(xmlstring, tblName))
                            {
                                matchingTableFiles++;
                                if (!ObjectUpdateTable(xmlstring, tblName, attName))
                                {
                                    continue;
                                }

                                matchingAttributeFiles++;
                                IndFiles += 1;
                                if (IndFiles % 100 == 0)
                                    KBDoctorOutput.Message(" Procesing " + IndFiles.ToString() + " navigation files.");

                                KBObject obj = ExtractObject(xmlstring);
                                if (obj == null)
                                {
                                    KBDoctorOutput.Warning("AttUpdated debug: matching file but object could not be resolved. File='" + x + "'.");
                                    writer.AddTableData(new string[] { "Can't find object", "", "", x });
                                    foundRows++;
                                }
                                else if (Utility.isGenerated(obj) || obj.GetPropertyValue<bool>("idISBUSINESSCOMPONENT"))
                                {
                                    writer.AddTableData(new string[] { Utility.linkObject(obj), obj.Description, obj.TypeDescriptor.Name, x });
                                    foundRows++;
                                }
                            }
                        }
                    }
                }

                if (foundRows == 0)
                {
                    writer.AddTableData(new string[] { "No objects found", "", "", "" });
                }

                KBDoctorOutput.Message("AttUpdated debug: total XML files checked=" + totalFiles.ToString() + ".");
                KBDoctorOutput.Message("AttUpdated debug: files changing table=" + matchingTableFiles.ToString() + ".");
                KBDoctorOutput.Message("AttUpdated debug: files updating attribute=" + matchingAttributeFiles.ToString() + ".");
                KBDoctorOutput.Message("AttUpdated debug: report rows=" + foundRows.ToString() + ".");
                success = true;
            }
            catch (Exception ex)
            {
                KBDoctorOutput.Error("Error generating AttUpdated report: " + ex.Message);
                KBDoctorOutput.Error("AttUpdated debug stack: " + ex.StackTrace);
            }
            finally
            {
                if (writer != null)
                {
                    KBDoctorOutput.Message("AttUpdated debug: closing report writer.");
                    writer.AddFooter();
                    writer.Close();
                }
                else
                {
                    KBDoctorOutput.Warning("AttUpdated debug: writer was not created.");
                }

                if (reportStarted)
                {
                    KBDoctor.KBDoctorOutput.EndSection(title, success);
                }

                KBDoctorOutput.Message("AttUpdated debug: reportStarted=" + reportStarted.ToString() + ", success=" + success.ToString() + ", output exists=" + File.Exists(outputFile).ToString() + ".");
                if (!string.IsNullOrEmpty(outputFile) && File.Exists(outputFile))
                {
                    KBDoctorHelper.ShowKBDoctorResults(outputFile);
                }
                else
                {
                    KBDoctorOutput.Error("AttUpdated debug: report file was not found and cannot be shown. File='" + outputFile + "'.");
                }
            }
        }

        private static Dictionary<string, IList<string>> GetTableAttributes(KBModel model)
        {
            Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
            foreach (Table table in Table.GetAll(model))
            {
                List<string> attributes = new List<string>();
                foreach (TableAttribute attribute in table.TableStructure.Attributes)
                {
                    attributes.Add(attribute.Name);
                }

                result[table.Name] = attributes;
            }

            return result;
        }

        private static Dictionary<string, IList<KBDoctorWebForms.SelectOption>> GetTableAttributeOptions(KBModel model)
        {
            Dictionary<string, IList<KBDoctorWebForms.SelectOption>> result = new Dictionary<string, IList<KBDoctorWebForms.SelectOption>>();
            foreach (Table table in Table.GetAll(model))
            {
                List<KBDoctorWebForms.SelectOption> attributes = new List<KBDoctorWebForms.SelectOption>();
                foreach (TableAttribute attribute in table.TableStructure.Attributes)
                {
                    string description = attribute.Attribute == null || string.IsNullOrEmpty(attribute.Attribute.Description) ? attribute.Name : attribute.Attribute.Description;
                    attributes.Add(new KBDoctorWebForms.SelectOption(attribute.Name, attribute.Name + " - " + description));
                }

                result[table.Name] = attributes;
            }

            return result;
        }

        private static IEnumerable<string> NavigationXmlDirectories(IKBService kbserv)
        {
            HashSet<string> directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string generatorDirectory in Directory.GetDirectories(KBDoctorHelper.SpcDirectory(kbserv), "GEN*", System.IO.SearchOption.TopDirectoryOnly))
            {
                AddDirectoryIfExists(directories, Path.Combine(generatorDirectory, "NVG"));
            }

            string kbLocation = kbserv.CurrentKB.Location;
            if (Directory.Exists(kbLocation))
            {
                foreach (string spcDirectory in Directory.GetDirectories(kbLocation, "GXSPC*", System.IO.SearchOption.TopDirectoryOnly))
                {
                    foreach (string generatorDirectory in Directory.GetDirectories(spcDirectory, "GEN*", System.IO.SearchOption.TopDirectoryOnly))
                    {
                        AddDirectoryIfExists(directories, Path.Combine(generatorDirectory, "NVG"));
                    }
                }
            }

            if (directories.Count == 0)
            {
                AddDirectoryIfExists(directories, KBDoctorHelper.SpcDirectory(kbserv));
            }

            return directories;
        }

        private static void AddDirectoryIfExists(HashSet<string> directories, string directory)
        {
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                directories.Add(directory);
            }
        }

        private static string AddXMLHeader(string fileName)
        {
            string xmlstring = File.ReadAllText(fileName);
            xmlstring = "<?xml version='1.0' encoding='iso-8859-1'?>" + xmlstring;
            return xmlstring;
        }

        private static Boolean ObjectUpdateTable( string xmlstring, string tblName, string attName)
        {

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(xmlstring);

                foreach (XmlNode tableToUpdate in NodesByLocalName(doc, "TableToUpdate"))
                {
                    string tableName = FirstDescendantValue(tableToUpdate, "TableName");
                    if (!string.Equals(tableName, tblName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    foreach (XmlNode attrisToUpdate in ChildNodesByLocalName(tableToUpdate, "AttrisToUpdate"))
                    {
                        foreach (XmlNode attribute in ChildNodesByLocalName(attrisToUpdate, "Attribute"))
                        {
                            string attributeName = FirstDescendantValue(attribute, "AttriName");
                            if (string.Equals(attributeName, attName, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); };

            return false;

        }

        private static Boolean ObjectChangesTable(string xmlstring, string tblName)
        {

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(xmlstring);

                foreach (XmlNode tableToUpdate in NodesByLocalName(doc, "TableToUpdate"))
                {
                    string tableName = FirstDescendantValue(tableToUpdate, "TableName");
                    if (!string.Equals(tableName, tblName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string tableAction = FirstDescendantValue(tableToUpdate, "TableAction");
                    if (IsTableChangeAction(tableAction))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); };

            return false;

        }

        private static bool IsTableChangeAction(string tableAction)
        {
            return string.Equals(tableAction, "insert", StringComparison.OrdinalIgnoreCase)
                || string.Equals(tableAction, "update", StringComparison.OrdinalIgnoreCase)
                || string.Equals(tableAction, "delete", StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<XmlNode> NodesByLocalName(XmlDocument doc, string localName)
        {
            foreach (XmlNode node in doc.GetElementsByTagName("*"))
            {
                if (string.Equals(node.LocalName, localName, StringComparison.OrdinalIgnoreCase))
                {
                    yield return node;
                }
            }
        }

        private static IEnumerable<XmlNode> ChildNodesByLocalName(XmlNode parent, string localName)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                if (string.Equals(node.LocalName, localName, StringComparison.OrdinalIgnoreCase))
                {
                    yield return node;
                }
            }
        }

        private static string FirstDescendantValue(XmlNode parent, string localName)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                if (string.Equals(node.LocalName, localName, StringComparison.OrdinalIgnoreCase))
                {
                    return node.InnerText;
                }

                string value = FirstDescendantValue(node, localName);
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

    }
}

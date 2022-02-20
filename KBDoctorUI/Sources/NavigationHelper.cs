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
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Helpers;
using Artech.Genexus.Common.Objects;
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
                string outputFile = Functions.CreateOutputFile(kbserv, title);


                IOutputService output = CommonServices.Output;
                output.StartSection("KBDoctor", title);

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
            String ObjName = "";
            try
            {
                doc.LoadXml(xmlstring);
                ObjName = doc.SelectSingleNode("ObjectSpec/Object/ObjName").InnerText;
                if (ObjName.EndsWith("_BC"))
                    ObjName = ObjName.Replace("_BC", "");
            }
            catch (Exception e) { Console.WriteLine(e.Message); };
            return KbStats.ObjectPartialName(ObjName);

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
                               // writer.AddTableData(new string[] { hash, Functions.linkObject(obj), EventName, LevelRow, LevelType, TableNames, AttNames });
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

        
        public static void PrepareComparerNavigation()
        {


            string title = "KBDoctor - Prepare Comparer Navigation Files";
            IOutputService output = CommonServices.Output;
            output.StartSection("KBDoctor",title);

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
            output.EndSection("KBDoctor", title, true);
        }

        private static void WriteXSLTtoDir()
        {
            IKBService kbserv = UIServices.KB;
            string outputFile = kbserv.CurrentKB.UserDirectory + @"\KBdoctorEv2.xslt";
            File.WriteAllText(outputFile, StringResources.specXEv2);

        }

        
        public static void ProcesoDir(string directoryArg, string newDir, string generator, IOutputService output)
        {

            IKBService kbserv = UIServices.KB;
            string outputFile = kbserv.CurrentKB.UserDirectory + @"\KBdoctorEv2.xslt";
            XslCompiledTransform xslTransform = new XslCompiledTransform();

            xslTransform.Load(outputFile);

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

                    xslTransform.Transform(newXmlFile, xTxt);
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
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                IOutputService output = CommonServices.Output;
                output.StartSection("KBDoctor", title);

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
                output.EndSection("KBDoctor", title, success);
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
                    writer.AddTableData(new string[] { containsTextArg, obj.TypeDescriptor.Name, Functions.linkObject(obj), obj.Description, obj.UserName, spcTxt });

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


                    return Functions.linkFile(file);



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

            string title = "KBDoctor - Where update this attribute? :";
            try
            {
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                IOutputService output = CommonServices.Output;
                output.StartSection("KBDoctor", title);


                AskAttributeandTable at = new AskAttributeandTable();
                DialogResult dr = new DialogResult();
                dr = at.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    string tblName = at.tblName;
                    string attName = at.attName;

                    List<string> Objlist = new List<string>();


                    KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);
                    writer.AddHeader(title + attName + " in table " + tblName);
                    writer.AddTableHeader(new string[] { "Object", "Description", "Type", "Navigation File" });

                    int IndFiles = 0;

                    //   IKBService kbserv = UIServices.KB;
                    string directoryArg = KBDoctorHelper.SpcDirectory(kbserv);
                    string fileWildcard = @"*.xml";
                    var searchSubDirsArg = System.IO.SearchOption.AllDirectories;
                    string[] xFiles = System.IO.Directory.GetFiles(directoryArg, fileWildcard, searchSubDirsArg);

                    foreach (string x in xFiles)
                    {
                        // KBDoctorOutput.Message(x);
                        IndFiles += 1;
                        if (IndFiles % 100 == 0)
                            KBDoctorOutput.Message( " Procesing " + IndFiles.ToString() + " navigation files.");

                        string filename = Path.GetFileNameWithoutExtension(x);

                        if (!Objlist.Contains(filename))
                        {
                            Objlist.Add(filename);
                            if (!Path.GetFileNameWithoutExtension(x).StartsWith("Gx0"))
                            {
                                string xmlstring = AddXMLHeader(x);

                                if (ObjectUpdateTable(xmlstring, tblName, attName))
                                {
                                    KBObject obj = ExtractObject(xmlstring);
                                    if (obj == null)
                                        writer.AddTableData(new string[] { "Can't find object", "", "", x });
                                    else
                                        if (Utility.isGenerated(obj) || obj.GetPropertyValue<bool>("idISBUSINESSCOMPONENT"))
                                        writer.AddTableData(new string[] { Functions.linkObject(obj), obj.Description, obj.TypeDescriptor.Name, x });
                                }
                            }
                        }
                    }

                    writer.AddFooter();
                    writer.Close();

                    KBDoctorHelper.ShowKBDoctorResults(outputFile);
                    bool success = true;
                    output.EndSection("KBDoctor", title, success);
                }
            }
            catch
            {
                bool success = false;
                KBDoctor.KBDoctorOutput.EndSection(title, success);
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

              //  string xpathstr = "//TableToUpdate/Table/TableName[text()='" + tblName + "']/../../AttrisToUpdate/Attribute/AttriName[text()='" + attName + "']";
                string xpathstr = "//TableToUpdate[Table/TableName='" + tblName + "' and ./TableAction='update' and  ./AttrisToUpdate/Attribute/AttriName='" + attName + "']";
                XmlNode node = doc.SelectSingleNode(xpathstr);
                if (node == null)
                    return false;
                else
                {
                    
                    return true;
                }
                 }
            catch (Exception e) { Console.WriteLine(e.Message); };

               
           
            return false;
                     
        }
             
    }
}

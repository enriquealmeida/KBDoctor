using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Xsl;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

using Artech.Genexus.Common;
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
using Artech.Genexus.Common.Collections;
using Artech.Architecture.Common.Descriptors;

namespace Concepto.Packages.KBDoctorCore.Sources
{
    static class Navigation
    {
        internal static void PrepareComparerNavigation(KnowledgeBase KB, IOutputService output)
        {
            string title = "KBDoctor - Prepare Comparer Navigation Files";
            output.StartSection(title);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            string directoryArg = Utility.NvgComparerDirectory(KB);
            string fechahora = String.Format("{0:yyyy-MM-dd-HHmm}", DateTime.Now);
            string newDir = directoryArg + @"\NVG-" + fechahora + @"\";

            try
            {
                Directory.CreateDirectory(newDir);
                Utility.WriteXSLTtoDir(KB);
                foreach (string d in Directory.GetDirectories(Utility.SpcDirectory(KB), "NVG", System.IO.SearchOption.AllDirectories))
                {
                    output.AddLine("Procesando directorio: " + d);
                    string generator = d.Replace(Utility.SpcDirectory(KB), "");
                    generator = generator.Replace("NVG_", "");
                    generator = @"\" + generator.Replace(@"\", "_") + "_";
                    generator = generator.Replace("NVG_", "");

                    ProcesoDir(KB, d, newDir, generator, output);
                }

                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value. 
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);

                output.AddLine(title + " elepsed time: " + elapsedTime);
                output.EndSection(title, true);
            }
            catch (Exception e){
                output.AddErrorLine(e.Message);
            }
        }

        private static void ProcesoDir(KnowledgeBase KB, string directoryArg, string newDir, string generator, IOutputService output)
        {
            string outputFile = KB.UserDirectory + @"\KBdoctorEv2.xslt";
            XslTransform xslTransform = new XslTransform();

            output.AddLine("Cargando archivo xslt: " + outputFile);
            xslTransform.Load(outputFile);
            output.AddLine("Archivo xslt cargado correctamente.");
            string fileWildcard = @"*.xml";
            var searchSubDirsArg = System.IO.SearchOption.AllDirectories;
            string[] xFiles = System.IO.Directory.GetFiles(directoryArg, fileWildcard, searchSubDirsArg);

            Hashtable colisiones = new Hashtable();
            HashSet<string> colisionesStr = new HashSet<string>();

            //Busco colisiones en los nombres de los archivos
            foreach (string x in xFiles)
            {
                string filename = Path.GetFileNameWithoutExtension(x);
                if (!colisiones.Contains(filename))
                {
                    List<string> paths = new List<string>();
                    paths.Add(x);
                    colisiones[filename] = paths;
                }
                else
                {
                    List<string> paths = (List<string>)colisiones[filename];
                    paths.Add(x);
                    colisiones[filename] = paths;
                    if (!colisionesStr.Contains(filename))
                        colisionesStr.Add(filename);
                }
            }

            //Me quedo sólo con el archivo más nuevo y cambio el nombre de todos los demás. 
            foreach (string name in colisionesStr)
            {
                FileInfo newestFile = null;
                DateTime newestDate = new DateTime(1891,09,28);
                List<string> paths = (List<string>)colisiones[name];
                List<FileInfo> oldfiles = new List<FileInfo>();
                output.AddLine("Colisión en archivos: ");
                foreach (string path in paths)
                {
                    output.AddLine("-- -- -- -- -- -- -- -" + path);
                    FileInfo file = new FileInfo(path);
                    if(file.LastWriteTime >= newestDate)
                    {
                        if (newestFile != null)
                        {
                            oldfiles.Add(newestFile);
                        }                    
                        newestDate = file.LastWriteTime;
                        newestFile = file;
                    }
                    else
                    {
                        oldfiles.Add(file);
                    }
                }
                int i = 1;
                foreach(FileInfo fileToRename in oldfiles)
                {
                    fileToRename.MoveTo(fileToRename.DirectoryName + '\\' + Path.GetFileNameWithoutExtension(fileToRename.Name) + i.ToString() + ".oldxml");
                    i++;
                }
            }

            xFiles = System.IO.Directory.GetFiles(directoryArg, fileWildcard, searchSubDirsArg);

            foreach (string x in xFiles)
            {
                if (!Path.GetFileNameWithoutExtension(x).StartsWith("Gx0"))
                {
                    output.AddLine("Procesando archivo: " + x);
                    string xTxt = newDir + generator + Path.GetFileNameWithoutExtension(x) + ".nvg";

                    string xmlstring = Utility.AddXMLHeader(x);

                    string newXmlFile = x.Replace(".xml", ".xxx");
                    File.WriteAllText(newXmlFile, xmlstring);

                    xslTransform.Transform(newXmlFile, xTxt);
                    File.Delete(newXmlFile);
                }
            }
        }

        

        internal static void ReplaceModulesInNVGFiles(KnowledgeBase KB, IOutputService output)
        {
            string pathNvg = Path.Combine(Utility.SpcDirectory(KB), "NvgComparer");
            string fileWildcard = @"*.*";
            string[] Files = Directory.GetDirectories(pathNvg, fileWildcard);
            string[] Last2directories = GetLast2Directorys(Files, output);
            List<string> replaces = new List<string>();
            foreach(string line in File.ReadAllLines(Utility.GetModuleNamesFilePath(KB)))
            {
                if(!line.TrimStart().StartsWith("#"))
                {
                    replaces.Add(line + ".");
                }
            }
            RemoveTextInFiles(Last2directories[0], fileWildcard, replaces, output);
            RemoveTextInFiles(Last2directories[1], fileWildcard, replaces, output);
        }

        internal static void RemoveTextInFiles(string path, string fileWildcard, List<string> replaces, IOutputService output)
        {
            var searchSubDirsArg = System.IO.SearchOption.AllDirectories;
            string[] FilesDirectory = System.IO.Directory.GetFiles(path, fileWildcard, searchSubDirsArg);
            bool contains;
            foreach (string filePath in FilesDirectory)
            {
                string dataline;
                string nameline;
                contains = false;
                StreamReader sr = new StreamReader(filePath);
                dataline = sr.ReadLine();
                nameline = sr.ReadLine();
                Encoding enc = sr.CurrentEncoding;
                sr.Close();

                DeleteFirstLines(2, filePath);

                string text = File.ReadAllText(filePath);

                foreach (string replace in replaces)
                {
                    if (text.Contains(replace))
                    {
                        string escreplace = replace.Replace(".", @"\.");
                        contains = true;
                        string pattern = @"([^a-zA-Z0-9])" + '(' + escreplace + ')';
                        string replacement = "$1";
                        Regex rgx = new Regex(pattern);
                        text = rgx.Replace(text, replacement);
                    }
                }
                File.WriteAllText(filePath, dataline + "\r\n" + nameline + "\r\n" + text, enc);
                if (contains)
                    output.AddLine("Module names replaced in: " + filePath);
            }
        }

        internal static void DeleteFirstLines(int number, string filePath)
        {
            for(int i = 1;i<= number; i++)
            {
                var lines = File.ReadAllLines(filePath);
                File.WriteAllLines(filePath, lines.Skip(1).ToArray());
            }
        }

        internal static bool CompareLastNVGDirectories (KnowledgeBase KB, IOutputService output)
        {
            try
            {
                bool isSuccess = true;
                string pathNvg = Path.Combine(Utility.SpcDirectory(KB), "NvgComparer");
                string fileWildcard = @"*.*";
                string[] Files = Directory.GetDirectories(pathNvg, fileWildcard);
                string[] Last2directories = GetLast2Directorys(Files, output);
                int cant_error = 0;
                if (Last2directories == null || Last2directories.Length != 2) {
                    output.AddErrorLine("Ocurrió un error procesando los directorios de navegaciones.");
                    output.AddErrorLine("Asegúrece de que existen al menos dos directorios con nombres en el formato válido (NVG-AAAA-MM-DD-HHMM)");
                }
                else
                {
                    output.AddLine("Se utilizarán los siguientes directorios para comparar:");
                    output.AddLine("-- " + Last2directories[0].ToString());
                    output.AddLine("-- " + Last2directories[1].ToString());
                    List<string> Diffs = EqualNavigationDirectories(Last2directories[0], Last2directories[1], output);
                    output.AddLine("-- Los directorios se procesaron correctamente.");
                    if (Diffs.Count > 0)
                    {
                        output.AddLine("-- Se encontraron diferencias en las navegaciones de los siguientes objetos:");
                        foreach (string x in Diffs)
                        {
                            string[] objectnametype = Utility.ReadQnameTypeFromNVGFile(x, output);
                            string objtype = objectnametype[0];
                            string objmodule = objectnametype[1];
                            string objname = objectnametype[2];
                            KBObjectDescriptor kbod = KBObjectDescriptor.Get(objtype);
                            QualifiedName qname = new QualifiedName(objmodule, objname);
                            KBObject obj = KB.DesignModel.Objects.Get(kbod.Id,qname);
                            if (obj != null)
                            {
                                if (obj.Timestamp <= Utility.GetDateTimeNVGDirectory(Last2directories[1].ToString()))
                                {
                                    if (objmodule != "")
                                    {
                                        output.AddLine("-- ERROR " + objmodule + '.' + objname + " fue modificado en \t\t" + obj.Timestamp.ToString());
                                    }
                                    else
                                    {
                                        output.AddLine("-- ERROR " + objname + " fue modificado en \t\t" + obj.Timestamp.ToString());
                                    }
                                    
                                    isSuccess = false;
                                    cant_error++; 
                                }
                                else
                                {
                                    if (objmodule != "")
                                    {
                                        output.AddLine("-- -- OK " + objmodule + '.' + objname + " fue modificado en \t\t" + obj.Timestamp.ToString());
                                    }
                                    else {
                                        output.AddLine("-- -- OK " + objname + " fue modificado en \t\t" + obj.Timestamp.ToString());
                                    }
                                }
                            }
                            else
                            {
                                output.AddLine("-- NO SE ENCONTRO EL OBJETO: " + qname.ToString());
                            }
                        }
                    }
                    else
                    {
                        output.AddLine("No se encontraron diferencias en las navegaciones");
                    }
                }
                if (cant_error > 0){
                    output.AddErrorLine("Se encontraron " + cant_error + " errores en la comparación.");
                }
                return isSuccess;
            }
            catch (Exception e)
            {
                output.AddLine(e.Message);
                return false;
            }
        }

        private static string[] GetLast2Directorys(string[] Files, IOutputService output)
        {
            
            DateTime FechaMax = new DateTime(1830,1,1);
            DateTime FechaMaxSec = new DateTime(1830,1,1);
            string DirectoryMax = "";
            string DirectorySec = "";
            string[] last2directories = new string[2];
            if (Files.Length <= 1)
            {
                output.AddErrorLine("Error: No existen 2 directorios de navegaciones para comparar");
                return last2directories;
            }
            foreach (string x in Files)
            {
                DateTime FechaX = Utility.GetDateTimeNVGDirectory(x);
                if (!DateTime.Equals(FechaX, new DateTime()))
                {
                    if (DateTime.Compare(FechaMax, FechaX) < 0)
                    {
                        FechaMaxSec = FechaMax;
                        DirectorySec = DirectoryMax;
                        FechaMax = FechaX;
                        DirectoryMax = x;
                    }
                    else
                    {
                        if (DateTime.Compare(FechaMaxSec, FechaX) < 0)
                        {
                            FechaMaxSec = FechaX;
                            DirectorySec = x;
                        }
                    }
                }
                else
                {
                    output.AddWarningLine("Error al intentar obtener la fecha del directorio: " + x);
                }
            }
            last2directories[0] = DirectoryMax;
            last2directories[1] = DirectorySec;
            return last2directories;
        }

        
        private static List<string> EqualNavigationDirectories(string pathSource, string pathNew, IOutputService output)
        {
            string fileWildcard = @"*.*";
            var searchSubDirsArg = System.IO.SearchOption.AllDirectories;
            string[] FilesDirectorySource = System.IO.Directory.GetFiles(pathSource, fileWildcard, searchSubDirsArg);
            List<string> Diffs = new List<string>();
            foreach (string fileSourcePath in FilesDirectorySource)
            {
                string fileNewPath = Path.Combine(pathNew, Path.GetFileName(fileSourcePath));
                if (File.Exists(fileNewPath))
                {
                    FileInfo fileNew = new FileInfo(fileNewPath);                   
                    FileInfo fileSource = new FileInfo(fileSourcePath);
                    if (!Utility.FilesAreEqual(fileSource, fileNew))
                    {
                        StreamReader sr = new StreamReader(fileNewPath);
                        string datalineNew = sr.ReadLine();
                        string namelineNew = sr.ReadLine();
                        Encoding encnew = sr.CurrentEncoding;
                        sr.Close();
                        DeleteFirstLines(2, fileNewPath);
                        string textnew = File.ReadAllText(fileNewPath);

                        sr = new StreamReader(fileSourcePath);
                        string datalineSource = sr.ReadLine();
                        string namelineSource = sr.ReadLine();
                        Encoding encSource = sr.CurrentEncoding;
                        sr.Close();
                        DeleteFirstLines(2, fileSourcePath);
                        string textsource = File.ReadAllText(fileSourcePath);

                        FileInfo fileNewReplace = new FileInfo(fileNewPath);
                        FileInfo fileSourceReplace = new FileInfo(fileSourcePath);
                        if (!Utility.FilesAreEqual(fileSourceReplace, fileNewReplace))
                            Diffs.Add(fileSourcePath);                            

                        File.WriteAllText(fileNewPath, datalineNew + "\r\n" + namelineNew + "\r\n" + textnew, encnew);
                        File.WriteAllText(fileSourcePath, datalineSource + "\r\n" + namelineSource + "\r\n" + textsource, encSource);
                    }

                    
                }
                else
                {
                    output.AddLine("-- Nuevo: " + fileNewPath);
                }
            }
            return Diffs;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Xsl;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.Common.Services;
using Artech.Udm.Framework.References;
using Artech.Genexus.Common;
using Artech.Common.Helpers.Structure;
using Artech.Genexus.Common.Parts.SDT;
using Artech.Architecture.Common.Descriptors;
using Artech.Udm.Framework;
using Artech.Common.Properties;
using Artech.Genexus.Common.Entities;
using Artech.Packages.Patterns.Definition;
using Artech.Packages.Patterns.Engine;
using Artech.Packages.Patterns;
using Artech.Packages.Patterns.Objects;
using Artech.Genexus.Common.Helpers;
//using static Concepto.Packages.KBDoctorCore.Sources.Objects;
//using Concepto.Packages.KBDoctor;

namespace Concepto.Packages.KBDoctorCore.Sources
{
    public static class Utility
    {
#pragma warning disable IDE1006 // Estilos de nombres
        public static bool isMain(KBObject obj)
#pragma warning restore IDE1006 // Estilos de nombres
        {
            object aux = obj.GetPropertyValue("isMain");
            return ((aux != null) && (aux.ToString() == "True"));

        }

#pragma warning disable IDE1006 // Estilos de nombres
        public static bool isGenerated(KBObject obj)
#pragma warning restore IDE1006 // Estilos de nombres
        {
            object aux = obj.GetPropertyValue(Artech.Genexus.Common.Properties.TRN.GenerateObject);
            return ((aux != null) && (aux.ToString() == "True"));

        }

        internal static string AddXMLHeader(string fileName)
        {
            string xmlstring = File.ReadAllText(fileName);
            xmlstring = "<?xml version='1.0' encoding='iso-8859-1'?>" + xmlstring;
            return xmlstring;
        }

        internal static DateTime GetDateTimeNVGDirectory(string FileName)
        {
            string NVGDirectory = Path.GetFileName(FileName);
            int posanio = 4;
            string StringAnio = NVGDirectory.Substring(posanio, 4);
            int Anio = Int32.Parse(StringAnio);
            string StringMes = NVGDirectory.Substring(posanio + 5, 2);
            int Mes = Int32.Parse(StringMes);
            string StringDia = NVGDirectory.Substring(posanio + 8, 2);
            int Dia = Int32.Parse(StringDia);
            string StringHora = NVGDirectory.Substring(posanio + 11, 2);
            int Hora = Int32.Parse(StringHora);
            string StringMinutos = NVGDirectory.Substring(posanio + 13, 2);
            int Minutos = Int32.Parse(StringMinutos);
            DateTime DateTimeDir = new DateTime();
            if (ValidoAtributosDateTime(Anio, Mes, Dia, Hora, Minutos, 0)){
                DateTimeDir = new DateTime(Anio, Mes, Dia, Hora, Minutos, 0);
            }
            return DateTimeDir;
        }

        internal static DateTime GetDateTimeWSDLDirectory(string FileName)
        {
            string WSDLDirectory = Path.GetFileName(FileName);
            int posanio = 5;
            string StringAnio = WSDLDirectory.Substring(posanio, 4);
            int Anio = Int32.Parse(StringAnio);
            string StringMes = WSDLDirectory.Substring(posanio + 5, 2);
            int Mes = Int32.Parse(StringMes);
            string StringDia = WSDLDirectory.Substring(posanio + 8, 2);
            int Dia = Int32.Parse(StringDia);
            string StringHora = WSDLDirectory.Substring(posanio + 11, 2);
            int Hora = Int32.Parse(StringHora);
            string StringMinutos = WSDLDirectory.Substring(posanio + 13, 2);
            int Minutos = Int32.Parse(StringMinutos);
            DateTime DateTimeDir = new DateTime();
            if (ValidoAtributosDateTime(Anio, Mes, Dia, Hora, Minutos, 0))
            {
                DateTimeDir = new DateTime(Anio, Mes, Dia, Hora, Minutos, 0);
            }
            return DateTimeDir;
        }

        private static bool ValidoAtributosDateTime(int Anio, int Mes, int Dia, int Hora, int Minutos, int Segundos)
        {
            if (Anio >= 2000 && Mes > 0 && Mes <= 12 && Dia <= 31 && Dia > 0 && Hora >= 0 && Hora <= 23 && Minutos >= 0 && Minutos <= 60 && Segundos >= 0 && Segundos <= 60)
                return true;
            else 
                return false;
        }

        internal static string NvgComparerDirectory(KnowledgeBase KB)
        {
            
            GxModel gxModel = KB.DesignModel.Environment.TargetModel.GetAs<GxModel>();
            string dir = Path.Combine(SpcDirectory(KB), "NvgComparer");
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch (Exception e) { Console.WriteLine(e.Message); }

            return dir;
        }

        internal static string WsdlComparerDirectory(KnowledgeBase KB)
        {

            GxModel gxModel = KB.DesignModel.Environment.TargetModel.GetAs<GxModel>();
            string dir = Path.Combine(SpcDirectory(KB), "WsdlComparer");
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch (Exception e) { Console.WriteLine(e.Message);  }

            return dir;
        }

        internal static string WsdlDir(KnowledgeBase KB, bool isSource)
        {
            string dir;
            string comparedir = WsdlComparerDirectory(KB);
            if (isSource)
            {
                dir = Path.Combine(comparedir, "Source");
            }
            else
            {
                string fechahora = String.Format("{0:yyyy-MM-dd-HHmm}", DateTime.Now);
                dir =  comparedir + @"\WSDL-" + fechahora + @"\";
            }

            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir;
        }

        internal static void ShowKBDoctorResults(string outputFile)
        {
            //Usando la nueva tool window
            //StartWindow sw = new StartWindow()
            // Usando la start page
            /*
            UIServices.StartPage.OpenPage(outputFile, "KBDoctor", null);
            //   UIServices.StartPage.OpenPage(outputFile, pageTitle, null);
            UIServices.ToolWindows.FocusToolWindow(UIServices.StartPage.ToolWindow.Id);
            */
        }

        internal static string SpcDirectory(KnowledgeBase KB)
        {
            GxModel gxModel = KB.DesignModel.Environment.TargetModel.GetAs<GxModel>();
            return KB.Location + string.Format(@"\GXSPC{0:D3}\", gxModel.Model.Id);
        }

        internal static string ObjComparerDirectory(KnowledgeBase KB)
        {
            GxModel gxModel = KB.DesignModel.Environment.TargetModel.GetAs<GxModel>();
            string dir = Path.Combine(SpcDirectory(KB), "ObjComparer");
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch (Exception e) { Console.WriteLine(e.Message); }

            return dir;
        }

        internal static int MaxCodeBlock(string source)
        {
            int MaxCodeBlock = 0;
            int countLine = 0;
            using (StringReader reader = new StringReader(source))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    countLine += 1;

                    if (line.StartsWith("SUB ") || line.StartsWith("EVENT "))
                    {
                        MaxCodeBlock = (MaxCodeBlock <= countLine) ? countLine : MaxCodeBlock;
                        countLine = 1;
                    }

                }
                MaxCodeBlock = (MaxCodeBlock <= countLine) ? countLine : MaxCodeBlock;
            }

            return MaxCodeBlock;
        }

        internal static int ComplexityLevel(string source)
        {
            int ComplexityLevel = 0;

            using (StringReader reader = new StringReader(source))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {

                    line = line.TrimStart().ToUpper();
                    if (line.StartsWith("DO WHILE") || line.StartsWith("IF") || line.StartsWith("DO CASE") || line.StartsWith("FOR"))
                    {
                        ComplexityLevel += 1;
                    }
                }
            }
            return ComplexityLevel;
        }

        internal static int MaxNestLevel(string source)
        {
            int MaxNestLevel = 0;
            int NestLevel = 0;
            using (StringReader reader = new StringReader(source))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {

                    line = line.TrimStart().ToUpper();
                    if (line.StartsWith("DO '"))
                    {
                        //Si es un llamado a una subrutina, no hago nada y lo salteo. 
                    }
                    else
                    {

                        if (line.StartsWith("FOR ") || line.StartsWith("IF ") || line.StartsWith("DO ") || line.StartsWith("NEW") || line.StartsWith("SUB"))
                        {
                            NestLevel += 1;
                            MaxNestLevel = (NestLevel > MaxNestLevel) ? NestLevel : MaxNestLevel;
                        }
                        else
                            if (line.StartsWith("ENDFOR") || line.StartsWith("ENDIF") || line.StartsWith("ENDDO") || line.StartsWith("ENDCASE") || line.StartsWith("ENDNEW") || line.StartsWith("ENDSUB"))
                        {
                            NestLevel -= 1;
                        }
                    }
                }
                return MaxNestLevel;
            }
        }
      
        internal static bool ValidateINOUTinParm(KBObject obj)
        {
            bool err = false;

            if (obj is ICallableObject callableObject)
            {
                foreach (Signature signature in callableObject.GetSignatures())
                {
                    Boolean someInOut = false;
                    foreach (Parameter parm in signature.Parameters)
                    {
                        if (parm.Accessor.ToString() == "PARM_INOUT")
                        {
                            someInOut = true;
                            break;
                        }
                    }
                    if (someInOut)
                    {
                        RulesPart rulesPart = obj.Parts.Get<RulesPart>();

                        if (rulesPart != null)
                        {
                            Regex myReg = new Regex("//.*", RegexOptions.None);
                            Regex paramReg = new Regex(@"parm\(.*\)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
                            string reglas = rulesPart.Source;
                            reglas = myReg.Replace(reglas, "");
                            Match match = paramReg.Match(reglas);
                            if (match != null)
                            {
                                int countparms = match.ToString().Split(new char[] { ',' }).Length;
                                int countsemicolon = match.ToString().Split(new char[] { ':' }).Length - 1;
                                err = (countparms != countsemicolon);

                            }
                        }
                    }
                }
            }
            return (err);
        }

        internal static void AddLineSummary(KnowledgeBase KB, string fileName, string texto)
        {
            string outputFile = KB.UserDirectory + @"\" + fileName;

            using (FileStream fs = new FileStream(outputFile, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(DateTime.Now.ToString() + "," + texto);
                fs.Dispose();
            }
            
        }

        internal static void AddLine(KnowledgeBase KB, string fileName, string texto)
        {
            string outputFile = KB.UserDirectory + @"\" + fileName;

            using (FileStream fs = new FileStream(outputFile, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(texto);
                fs.Dispose();
            }
        }

        internal static string ObjectSourceUpper(KBObject obj)
        {
            string source = "";
            try
            {
                if (obj is Procedure) source = obj.Parts.Get<ProcedurePart>().Source;

                if (obj is Transaction) source = obj.Parts.Get<EventsPart>().Source;

                if (obj is WorkPanel) source = obj.Parts.Get<EventsPart>().Source;

                if (obj is WebPanel) source = obj.Parts.Get<EventsPart>().Source;
            }
            catch (Exception e) { Console.WriteLine(e.Message);  }

            return source.ToUpper();
        }

        public static KBObjectPart ObjectSourcePart(KBObject obj)
        {

            try
            {
                if (obj is Procedure) return obj.Parts.Get<ProcedurePart>();

                if (obj is Transaction) return obj.Parts.Get<EventsPart>();

                if (obj is WorkPanel) return obj.Parts.Get<EventsPart>();

                if (obj is WebPanel) return obj.Parts.Get<EventsPart>();
            }
            catch (Exception e) { Console.WriteLine(e.Message);  }
            return null;
            
        }

        internal static KBObjectPart ObjectRulesPart(KBObject obj)
        {

            try
            {
                if (obj is Procedure) return obj.Parts.Get<RulesPart>();

                if (obj is Transaction) return obj.Parts.Get<RulesPart>();

                if (obj is WorkPanel) return obj.Parts.Get<RulesPart>();

                if (obj is WebPanel) return obj.Parts.Get<RulesPart>();
            }
            catch (Exception e) { Console.WriteLine(e.Message);  }
            return null;

        }

#pragma warning disable IDE1006 // Estilos de nombres
        internal static bool isRunable(KBObject obj)
#pragma warning restore IDE1006 // Estilos de nombres
        {
            return (obj is Transaction || obj is WorkPanel || obj is WebPanel
                || obj is DataProvider || obj is DataSelector || obj is Procedure || obj is Menubar);
        }

        internal static bool CanBeBuilt(KBObject obj)
        {
            return (obj is Transaction || obj is WebPanel || obj is Procedure || obj is DataProvider || obj is Menubar);
        }

        public static string ExtractComments(string source)
        {

            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            string noComments = Regex.Replace(source, blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
             me =>
             {
                 if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                     return me.Value.StartsWith("//") ? Environment.NewLine : "";
                 // Keep the literal strings
                 return me.Value;
             },
                    RegexOptions.Singleline);

            noComments = noComments.Replace("(", " (");
            noComments = noComments.Replace(")", ") ");
            noComments = noComments.Replace("\"", "\'");
            noComments = noComments.Replace("\t", " ");

            //saco blancos
            string aux = noComments.Replace("  ", " ");
            do
            {
                noComments = aux;
                aux = noComments.Replace("  ", " ");
            } while (noComments != aux);

            //noComments = noComments.ToUpper();
            return noComments;
        }

        public static string ExtractCommentsAndBreakLines(string source)
        {

            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            string noComments = Regex.Replace(source, blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
             me =>
             {
                 if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                     return me.Value.StartsWith("//") ? Environment.NewLine : "";
                 // Keep the literal strings
                 return me.Value;
             },
                    RegexOptions.Singleline);
            noComments = noComments.Replace("\t", "");
            noComments = noComments.Replace("\n", "");

            //saco blancos
            string aux = noComments.Replace("  ", " ");
            do
            {
                noComments = aux;
                aux = noComments.Replace("  ", " ");
            } while (noComments != aux);

            //noComments = noComments.ToUpper();
            return noComments;
        }

        internal static string CodeCommented(string source)
        {
            string source2 = source.Replace("//\n", "####\n");
            var codeComments = @"[^\/](\/\*)([\b\s]*(msg|do|call|udp|where|if|else|endif|endfor|for|defined by|while|enddo|&[A-Za-z0-9_\-.\s]*=))(\*(?!\/)|[^*])*(\*\/)|(\/\/)[\b\s]*((msg|do|call|udp|where|if|else|endif|endfor|for|defined by|while|enddo|&[A-Za-z0-9_\-.\s]*=)([^\r\n]+)?)";

            return Regex.Match(source2, codeComments).Value;

        }

        internal static bool HasCodeCommented(string source)
        {

            var codeComments = @"[^\/](\/\*)([\b\s]*(msg|do|call|udp|where|if|else|endif|endfor|for|defined by|while|enddo|&[A-Za-z0-9_\-.\s]*=))(\*(?!\/)|[^*])*(\*\/)|(\/\/)[\b\s]*((msg|do|call|udp|where|if|else|endif|endfor|for|defined by|while|enddo|&[A-Za-z0-9_\-.\s]*=)([^\r\n]+)?)";

            return (Regex.Match(source, codeComments).Value != "");

        }

        internal static Domain DomainByName(KBModel model, string domainName)
        {
            foreach (Domain d in Domain.GetAll(model))
            {
                if (d.Name == domainName)
                {
                    return d;
                   
                }
            }
            return null;
        }

        internal static string RemoveEmptyLines(string lines)
        {
            return Regex.Replace(lines, @"^\s*$\n|\r", "", RegexOptions.Multiline);
        }

        internal static int LineCount(string s)
        {
            int n = 0;
            foreach (var c in s)
            {
                if (c == '\n') n++;
            }
            return n;
        }

#pragma warning disable IDE1006 // Estilos de nombres
        internal static string linkObject(KBObject obj)
#pragma warning restore IDE1006 // Estilos de nombres
        {
            return "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;OpenObject&name=" + obj.Guid.ToString() + "\">" + obj.Name + "</a>";
        }

#pragma warning disable IDE1006 // Estilos de nombres
        internal static string linkFile(string file)
#pragma warning restore IDE1006 // Estilos de nombres
        {
            return "<a href=\"file:///" + file + "\"" + ">" + file + "</a" + ">";
        }

        internal static string ExtractRuleParm(KBObject obj)
        {
            RulesPart rulesPart = obj.Parts.Get<RulesPart>();
            string aux = "";

            if (rulesPart != null)
            {
                Regex myReg = new Regex("//.*", RegexOptions.None);
                Regex paramReg = new Regex(@"parm\(.*\)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
                string reglas = rulesPart.Source;
                reglas = myReg.Replace(reglas, "");
                Match match = paramReg.Match(reglas);
                if (match != null)
                    aux = match.ToString();
                else
                    aux = "";
            }
            return aux;
        }

        internal static string CleanFileName(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        internal static string CreateOutputFile(KnowledgeBase KB, string title)
        {
            string outputFile = KB.UserDirectory + @"\kbdoctor." + Utility.CleanFileName(title) + ".html";
            if (File.Exists(outputFile))
            {
                
                try
                {
                    File.Delete(outputFile);
                }
                catch
                {
                    
                }
                KBDoctor.KBDoctorOutput.Warning("File " + outputFile + " is locked. The start page cannot be generated");
            }

            return outputFile;
        }

        internal static bool AttIsSubtype(Artech.Genexus.Common.Objects.Attribute a)
        {
            if (a.SuperTypeKey != null)
                return true;
            else
                return false;

        }

        internal static void KillAttribute(Artech.Genexus.Common.Objects.Attribute a)
        {
            IOutputService output = CommonServices.Output;

            foreach (EntityReference reference in a.GetReferencesTo())
            {
                KBObject objRef = KBObject.Get(a.Model, reference.From);

                if (objRef != null)
                {
                    CleanVariablesBasedInAttribute(a, output, objRef);
                    CleanSDT(a, output, objRef);
                    if (!(objRef is DataView))
                    {
                        try
                        {
                            objRef.Save();
                        }
                        catch (Exception e)
                        {
                            output.AddErrorLine("ERROR: Can't save object: " + objRef.Name + e.Message);
                        }
                    }
                }
            }
        }

        internal static void CleanVariablesBasedInAttribute(Artech.Genexus.Common.Objects.Attribute a, IOutputService output, KBObject objRef)
        {
            output.AddLine("KBDoctor","Cleaning variables references to " + a.Name + " in " + objRef.Name);

            VariablesPart vp = objRef.Parts.Get<VariablesPart>();
            if (vp != null)
            {
                foreach (Variable v in vp.Variables)
                {
                    if (!v.IsStandard)
                    {
                        if ((v.AttributeBasedOn != null) && (a.Name == v.AttributeBasedOn.Name))
                        {
                            output.AddLine("KBDoctor","&" + v.Name + " based on  " + a.Name);
                            eDBType type = v.Type;
                            int length = v.Length;
                            bool signed = v.Signed;
                            string desc = v.Description;
                            int dec = v.Decimals;

                            //Modifico la variable, para que no se base en el atributo. 
                            v.AttributeBasedOn = null;
                            v.Type = type;
                            v.Decimals = dec;
                            v.Description = desc;
                            v.Length = length;
                            v.Signed = signed;
                        }
                    }
                }
            }
        }

        internal static KBCategory MainCategory(KBModel model)
        {
            return KBCategory.Get(model, "Main Programs");
        }

        public static EntityKey KeyOfBasedOn_CompatibleConEvo3(SDTItem sdtItem)
        {
            // esto es para mantener compatibilidad con Evo3 y la 15
            EntityKey myKey = new EntityKey(Guid.Empty, 0);
#if EVO3
            myKey = sdtItem.BasedOn.ObjKey;
#else
            myKey = sdtItem.BasedOn.Key;
#endif
            //Termina compatibilidad Evo3 y 15. 
            return myKey;
        }

        internal static void CleanSDT(Artech.Genexus.Common.Objects.Attribute a, IOutputService output, KBObject objRef)
        {


            if (objRef is SDT)
            {
                output.AddLine("KBDoctor","Cleaning SDT references to " + a.Name + " in " + objRef.Name);
                SDTStructurePart sdtstruct = objRef.Parts.Get<SDTStructurePart>();

                foreach (IStructureItem structItem in sdtstruct.Root.Items)
                {
                    SDTItem sdtItem = (SDTItem)structItem;
                    EntityKey myKey = KeyOfBasedOn_CompatibleConEvo3(sdtItem);

                    if (sdtItem.BasedOn != null && myKey == a.Key)
                    {

                        output.AddLine("KBDoctor","..." + sdtItem.Name + " based on  " + a.Name);
                        eDBType type = sdtItem.Type;
                        int length = sdtItem.Length;
                        bool signed = sdtItem.Signed;
                        string desc = sdtItem.Description;
                        int dec = sdtItem.Decimals;

                        //Modifico la variable, para que no se base en el atributo. 
                        sdtItem.AttributeBasedOn = null;
                        sdtItem.Type = type;
                        sdtItem.Decimals = dec;
                        sdtItem.Description = desc;
                        sdtItem.Length = length;
                        sdtItem.Signed = signed;

                    }
                }
            }
        }

        public static KBObject GetObjectByNameModule(KBModel model, string name, string module)
        {
            foreach (KBObject obj in model.Objects.GetByPropertyValue("Name", name))
            {
                if (obj.Module.Name != null)
                {
                    if (obj.Module.Name.ToLower() == module.ToLower())
                        return obj;
                }
            }
            return null;
        }

        public static KBObject GetObjectByQName(KBModel model, QualifiedName qname, Dictionary<string, KBObject> hash_mains)
        {
            foreach (KBObject obj in model.Objects.GetByPropertyValue("Name", qname.ObjectName))
            {
                if(obj.Name.ToLower() == qname.ObjectName.ToLower())
                {
                    if (obj.Module.Name != null)
                    {
                        if (obj.QualifiedName.ModuleName.ToLower() == qname.ModuleName.ToLower())
                            return obj;
                    }
                }
            }

            if (hash_mains.ContainsKey(qname.ToString().ToLower()))
            {
                return hash_mains[qname.ToString().ToLower()];
            }

            return null;
        }

        internal static List<Tuple<Domain, string>> GetParametersDomains(KBObject obj)
        {

            if (obj is ICallableObject callableObject)
            {
                List<Signature> signatures = (List<Signature>)callableObject.GetSignatures();
                if (signatures.Count == 1)
                {
                    List<Tuple<Domain, string>> domain_accessors = new List<Tuple<Domain, string>>();
                    foreach (Signature signature in signatures)
                    {
                        foreach (Parameter parm in signature.Parameters)
                        {
                            Domain param_domain;
                            string param_access = parm.Accessor.ToString();
                            if (parm.IsAttribute)
                            {
                                Artech.Genexus.Common.Objects.Attribute att = (Artech.Genexus.Common.Objects.Attribute)parm.Object;
                                if (att != null)
                                    param_domain = att.DomainBasedOn;
                                else
                                    return null;
                            }
                            else
                            {
                                Variable var = (Variable)parm.Object;
                                if (var != null)
                                    param_domain = var.DomainBasedOn;
                                else
                                    param_domain = null;
                            }
                            Tuple<Domain, string> par = new Tuple<Domain, string>(param_domain, param_access);
                            domain_accessors.Add(par);
                        }
                    }
                    return domain_accessors;
                }

            }
            return null;
        }

        internal static List<Tuple<string,string>> GetParametersFormatedType(KBObject obj)
        {

            if (obj is ICallableObject callableObject)
            {
                List<Signature> signatures = (List<Signature>)callableObject.GetSignatures();
                if (signatures.Count == 1)
                {
                    List<Tuple<string, string>> types_accessors = new List<Tuple<string, string>>();
                    foreach (Signature signature in signatures)
                    {
                        foreach (Parameter parm in signature.Parameters)
                        {
                            string param_type = "";
                            string param_access = parm.Accessor.ToString();
                            if (parm.IsAttribute)
                            {
                                Artech.Genexus.Common.Objects.Attribute att = (Artech.Genexus.Common.Objects.Attribute)parm.Object;
                                if (att != null)
                                {
                                    param_type = Utility.FormattedTypeAttribute(att);
                                }
                                else
                                {
                                    param_type = "Unknown";
                                }
                            }
                            else
                            {
                                Variable var = (Variable)parm.Object;
                                if (var != null)
                                    param_type = Utility.FormattedTypeVariable(var);
                                else if (obj is DataProvider)
                                {
                                    param_type = "Unknown";
                                }
                            }
                            Tuple<string, string> par = new Tuple<string, string>(param_type, param_access);
                            types_accessors.Add(par);
                        }
                    }
                    return types_accessors;
                }

            }
            return null;
        }

        public static string GetOutputFormatedType(KBObject obj)
        {

            if (obj is ICallableObject callableObject)
            {
                List<Signature> signatures = (List<Signature>)callableObject.GetSignatures();
                if (signatures.Count == 1)
                {
                    foreach (Signature signature in signatures)
                    {
                        Parameter lastparameter = null;
                        foreach (Parameter parm in signature.Parameters)
                        {
                            lastparameter = parm;
                        }
                        if (lastparameter != null && lastparameter.Accessor.ToString() != "PARM_IN")
                        {
                            if (lastparameter.IsAttribute)
                            {
                                Artech.Genexus.Common.Objects.Attribute att = (Artech.Genexus.Common.Objects.Attribute)lastparameter.Object;
                                if (att != null)
                                    return Utility.FormattedTypeAttribute(att);
                                else
                                    return "";
                            }
                            else
                            {
                                Variable var = (Variable)lastparameter.Object;
                                if (var != null)
                                    return Utility.FormattedTypeVariable(var);
                                else
                                    return "";
                            }
                        }
                    }
                }
            }
            return "";
        }
        
        public static Domain GetOutputDomains(KBObject obj)
        {

            if (obj is ICallableObject callableObject)
            {
                List<Signature> signatures = (List<Signature>)callableObject.GetSignatures();
                if (signatures.Count == 1)
                {
                    foreach (Signature signature in signatures)
                    {
                        Parameter lastparameter = null;
                        foreach (Parameter parm in signature.Parameters)
                        {
                            lastparameter = parm;
                        }
                        if (lastparameter != null && lastparameter.Accessor.ToString() != "PARM_IN")
                        {
                            if (lastparameter.IsAttribute)
                            {
                                Artech.Genexus.Common.Objects.Attribute att = (Artech.Genexus.Common.Objects.Attribute)lastparameter.Object;
                                if (att != null)
                                    return att.DomainBasedOn;
                                else
                                    return null;
                            }
                            else
                            {
                                Variable var = (Variable)lastparameter.Object;
                                if (var != null)
                                {
                                    return var.DomainBasedOn;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        internal static void CreateModuleNamesFile(KnowledgeBase KB)
        {
            string pathNvg = Path.Combine(Utility.SpcDirectory(KB), "NvgComparer");
            string path = pathNvg + "\\ModuleNames.txt";
            KBObjectDescriptor kbod = KBObjectDescriptor.Get("Module");
            List<string> lines = new List<string>();
            foreach (KBObject obj in KB.DesignModel.Objects.GetAll(kbod.Id))
            {
                lines.Add(obj.QualifiedName.ToString());
            }
            File.WriteAllLines(path, SortModulesByLevel(lines).ToArray());
        }

        internal static List<string> SortModulesByLevel(List<string> moduleNames)
        {
            List<string> sortedlist = new List<string>();
            int level = 1;
            bool end = false;
            while (!end)
            {
                end = true;
                foreach (string module in moduleNames)
                {
                    if (LevelQualifiedName(module) == level)
                    {
                        end = false;
                        sortedlist.Add(module);
                    }
                }
                level++;
            }
            sortedlist.Reverse();
            return sortedlist;
        }

        internal static string GetModuleNamesFilePath(KnowledgeBase KB)
        {
            string pathNvg = Path.Combine(Utility.SpcDirectory(KB), "NvgComparer");
            return pathNvg + "\\ModuleNames.txt";
        }

        internal static int LevelQualifiedName(string name)
        {
            return name.Split('.').Length;
        }

        internal static string[] ReadQnameTypeFromNVGFile(string path, IOutputService output)
        {
            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path);
                int i = 0;
                int lines = 2; //Linea en la que se encuentra el tipo para el formato versión 10.8
                string line = "";
                while (i < lines)
                {
                    line = sr.ReadLine();
                    i++;
                }
                sr.Dispose();
                string type = ReadTypeFromLine(line);
                string[] qname = ReadQnameFromLine(line,output);
                string[] ret = new string[3];
                ret[0] = type;
                ret[1] = qname[0];
                ret[2] = qname[1];


                sr.Dispose();

                return ret;
            }
            else
            {
                throw new System.ArgumentException("El archivo no existe. " + path); 
            }
        }

        private static string ReadTypeFromLine(string Line)
        {
            string ret = "";
            string[] splits = Line.Split(' ');
            if (splits[5] == "Web")
            {
                ret = splits[5] + splits[6];
            }
            else
            {
                ret = splits[5];
            }
            return ret;
        }

        public static string GetQueryStringFromToDate(DateTime FromDate, DateTime ToDate)
        {
            string querystring = "operation:Commit";
            if (FromDate != null)
            {
                querystring += " after:" + FromDate.Year.ToString() + "/" + FromDate.Month.ToString().PadLeft(2, '0') + "/" + FromDate.Day.ToString().PadLeft(2, '0');
            }
            else
            {
                DateTime ayer = DateTime.Today.AddDays(-1);
                querystring += " after:" + ayer.Year.ToString() + "/" + ayer.Month.ToString().PadLeft(2, '0') + "/" + ayer.Day.ToString().PadLeft(2, '0');
            }
            if (ToDate != null)
            {
                querystring += " before:" + ToDate.Year.ToString() + "/" + ToDate.Month.ToString().PadLeft(2, '0') + "/" + ToDate.Day.ToString().PadLeft(2, '0');
            }

            return querystring;
        }

        private static string[] ReadQnameFromLine(string Line, IOutputService output)
        {
            string qname = "";
            string[] splits = Line.Split(' ');
            if (splits[5] == "Web")
            {
                qname = splits[7];
            }
            else
            {
                qname = splits[6];
            }
            string[] ret = new string[2];
            if (qname.Contains('.'))
            {
                string module = "";
                splits = qname.Split('.');
                int i = 0;
                while (i < splits.Length - 1){
                    //Si este es el último, no pongo un punto. 
                    if (i + 1 < splits.Length - 1)
                        module += splits[i] + ".";
                    else
                        module += splits[i];
                    i++;
                }
                ret[0] = module;
                ret[1] = splits[splits.Length - 1];
            }
            else
            {
                ret[0] = "";
                ret[1] = qname;
            }
            
            return ret;
        }

        internal static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            int BYTES_TO_READ = sizeof(Int64);
            if (first.Length != second.Length)
                return false;

            if (first.FullName == second.FullName)
                return true;

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);
                    fs2.Read(two, 0, BYTES_TO_READ);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }

            return true;
        }

        public static string FormattedTypeAttribute(Artech.Genexus.Common.Objects.Attribute a)
        {
            return ReturnFormattedType(a.Type, a.Length, a.Decimals, a.Signed);
        }

        public static string FormattedTypeVariable(Variable v)
        {
            return ReturnFormattedType(v.Type, v.Length, v.Decimals, v.Signed);
        }

        public static string FormattedTypeDomain(Domain d)
        {
            return ReturnFormattedType(d.Type, d.Length, d.Decimals, d.Signed);
        }
        
        public static string ReturnFormattedType(eDBType type, int length, int decimals, bool signed)
        {
            string formatType = "";
            if (type == eDBType.BINARY || type == eDBType.Boolean || type == eDBType.BITMAP)
                formatType = type.ToString();
            else if(type == eDBType.VARCHAR || type == eDBType.LONGVARCHAR || type == eDBType.CHARACTER)
            {
                formatType = type.ToString() + "(" + length.ToString() + ")" + (signed ? "-" : "");
            }
            else { 
                formatType = type.ToString() + "(" + length.ToString() + (decimals > 0 ? "." + decimals.ToString() : "") + ")" + (signed ? "-" : "");
            }

            return formatType;

        }
        internal static void SaveObject(IOutputService output, KBObject obj)
        {
            try
            {
                obj.Save();
            }
            catch (Exception e)
            {
                output.AddErrorLine(e.Message + " - " + e.InnerException);
            }
        }

        internal static void WriteXSLTtoDir(KnowledgeBase KB)
        {
            string outputFile = KB.UserDirectory + @"\KBdoctorEv2.xslt";
            File.WriteAllText(outputFile, StringResources.specXEv2);
        }

        internal static IEnumerable<KBObject> GetObjectsSOAP(KnowledgeBase KB)
        {
            IEnumerable<KBObject> objects = KB.DesignModel.Objects.GetByPropertyValue("CALL_PROTOCOL", "SOAP");
            return objects;
        }
        internal static string GetWebRootProperty(KnowledgeBase KB, string GeneratorName)
        {
            KBModel targetmodel = KB.DesignModel.Environment.TargetModel;
            GxModel arg = targetmodel.GetAs<GxModel>();
            PropertiesObject propertiesObject = null;
            GeneratorCategory environment;
            foreach (GeneratorCategory current in arg.GeneratorCategories)
            {
                if ( current.Name.ToLower().Trim() == GeneratorName.ToLower().Trim())
                {
                    environment = current;
                    propertiesObject = PropertiesObject.GetFrom(current);
                }
            }
            string salida;
            if (propertiesObject == null)
            {
               salida = "";
            }
            else
            {
                salida = propertiesObject.GetPropertyValueString("WebRoot");
            }
            return salida;
        }

        internal static bool VarHasToBeInDomain(Variable v)
        {
            return TypeHasToBeInDomain(v.Type);
        }

        internal static bool AttHasToBeInDomain(Artech.Genexus.Common.Objects.Attribute a)
        {
            return TypeHasToBeInDomain(a.Type);
        }

        internal static bool TypeHasToBeInDomain(eDBType type)
        {
            if (type != eDBType.Boolean && type != eDBType.BITMAP && type != eDBType.BINARY && type != eDBType.GX_SDT && 
                type != eDBType.GX_EXTERNAL_OBJECT && type != eDBType.GX_USRDEFTYP && type != eDBType.GX_BUSCOMP && type != eDBType.GX_BUSCOMP_LEVEL)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static string GetStringType(Variable v)
        {
            string type = v.Type.ToString();
            string name = v.Name;
            string txtDimensions = "";
            if ((!v.IsStandard) && (v.AttributeBasedOn == null) && (type == "GX_USRDEFTYP")) //|| (type==) || (type == "GX_BUSCOMP_LEVEL") )
            {
                txtDimensions = v.GetPropertyValue<string>(Artech.Genexus.Common.Properties.ATT.DataTypeString);
            }
            return txtDimensions;
        }

        internal static bool IsGeneratedByPattern(KBObject obj)
        {
            Boolean isGeneratedWithPattern = false;
            PatternDefinition pattern;
            if (InstanceManager.IsInstanceObject(obj, out pattern))
                isGeneratedWithPattern = true;
            bool isParentPattern = false;
            foreach (PatternDefinition p in PatternEngine.Patterns)
            {
                if (PatternInstance.Get(obj, p.Id) != null)
                    isParentPattern = true;
            }
            if (isGeneratedWithPattern || isParentPattern)
            {
                return true; 
            }
            else
            {
                return false;
            }
        }

        internal static bool IsTypeAllowedInWP(string type)
        {
            if (type == "Directory" || type == "ExcelDocument" || type == "HttpClient" || type == "HttpRequest" || type == "HttpResponse" ||
                type == "MailMessage" || type == "POP3Session" || type == "SMTPSession" || type == "WebWrapper" || type == "XMLReader" || type == "XMLWriter")
            {
                return false;
            }
            return true;
        }

        public static int CompareRevisionList(IKBVersionRevision x, IKBVersionRevision y)
        {
            return x.UserName.CompareTo(y.UserName);
        }

        public static List<KBObject> ModuleObjects(Module module)
        {
            {
                List<KBObject> objectsModule = new List<KBObject>();
                foreach (KBObject obj in module.GetAllMembers())
                {
                    if (obj is Folder)
                    {
                        List<KBObject> objectsAux = new List<KBObject>();
                        objectsAux = FolderObjects((Folder)obj);
                        foreach (KBObject objAux in objectsAux)
                        {
                            objectsModule.Add(objAux);
                        }
                    }
                    else
                    {
                        if(obj is Module)
                        {
                            List<KBObject> objectsAux = new List<KBObject>();
                            objectsAux = ModuleObjects((Module)obj);
                            foreach(KBObject objAux in objectsAux)
                            {
                                objectsModule.Add(objAux);
                            }
                        }
                        else
                        {
                            if (!objectsModule.Contains(obj))
                                objectsModule.Add(obj);
                        }
                    }
                }
                return objectsModule;
            }
        }

        public static bool IsMain(KBObject obj)
        {
            object aux = obj.GetPropertyValue("isMain");
            return ((aux != null) && (aux.ToString() == "True"));

        }

        public static List<KBObject> FolderObjects(Folder folder)
        {
            {
                List<KBObject> objectsFolder = new List<KBObject>();
                foreach (KBObject obj in folder.AllObjects)
                {
                    if (obj is Folder)
                    {
                        List<KBObject> objectsAux = new List<KBObject>();
                        objectsAux = FolderObjects((Folder)obj);
                        foreach(KBObject objAux in objectsAux)
                        {
                            objectsFolder.Add(objAux);
                        }
                    }
                    else
                    {
                        if (!objectsFolder.Contains(obj))
                            objectsFolder.Add(obj);
                    }   
                }
                return objectsFolder;
            }
        }
/*
#if EVO3
        public class Tuple<T1, T2>
        {
            public T1 Item1 { get; private set; }
            public T2 Item2 { get; private set; }
            internal Tuple(T1 first, T2 second)
            {
                Item1 = first;
                Item2 = second;
            }
        }

        public static class Tuple
        {
            public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second)
            {
                var tuple = new Tuple<T1, T2>(first, second);
                return tuple;
            }
        }
#endif
*/
    }
}

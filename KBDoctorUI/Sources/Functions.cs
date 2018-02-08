using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using System.Text.RegularExpressions;
using System.IO;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.Common.Services;
using Artech.Udm.Framework.References;
using Artech.Genexus.Common;
using Artech.Common.Helpers.Structure;
using Artech.Genexus.Common.Parts.SDT;

namespace Concepto.Packages.KBDoctor
{
    class Functions
    {
        public static int MaxCodeBlock(string source)
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

        public static int ComplexityLevel(string source)
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

        public static int MaxNestLevel(string source)
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




        public static bool ValidateINOUTinParm(KBObject obj)
        {
            bool err = false;
            ICallableObject callableObject = obj as ICallableObject;

            if (callableObject != null)
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

        internal static void AddLineSummary(string fileName, string texto)
        {
            IKBService kbserv = UIServices.KB;

            string outputFile = kbserv.CurrentKB.UserDirectory + @"\" + fileName;

            using (FileStream fs = new FileStream(outputFile, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(DateTime.Now.ToString() + "," + texto);
            }
        }

        internal static void AddLine(string fileName, string texto)
        {
            IKBService kbserv = UIServices.KB;

            string outputFile = kbserv.CurrentKB.UserDirectory + @"\" + fileName;

            using (FileStream fs = new FileStream(outputFile, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(texto);
            }
        }

        public static string ObjectSourceUpper(KBObject obj)
        {
            string source = "";
            try
            {
                if (obj is Procedure) source = obj.Parts.Get<ProcedurePart>().Source;

                if (obj is Transaction) source = obj.Parts.Get<EventsPart>().Source;

                if (obj is WorkPanel) source = obj.Parts.Get<EventsPart>().Source;

                if (obj is WebPanel) source = obj.Parts.Get<EventsPart>().Source;
            } 
            catch (Exception e) { }

            return source.ToUpper();
        }

        public static bool isRunable(KBObject obj)
        {
            return (obj is Transaction | obj is WorkPanel | obj is WebPanel 
                | obj is DataProvider | obj is DataSelector | obj is Procedure | obj is Menubar);
        }

        public static bool CanBeBuilt(KBObject obj)
        {
            return (obj is Transaction | obj is WebPanel | obj is Procedure | obj is DataProvider | obj is Menubar);
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

        public static string CodeCommented(string source)
        {

            var codeComments = @"[^\/](\/\*)([\b\s]*(msg|do|call|udp|where|if|else|endif|endfor|for|defined by|while|enddo|&[A-Za-z0-9_\-.\s]*=))(\*(?!\/)|[^*])*(\*\/)|(\/\/)[\b\s]*((msg|do|call|udp|where|if|else|endif|endfor|for|defined by|while|enddo|&[A-Za-z0-9_\-.\s]*=)([^\r\n]+)?)";

            return Regex.Match(source, codeComments).Value;

        }

        public static bool HasCodeCommented(string source)
        {

            var codeComments = @"[^\/](\/\*)([\b\s]*(msg|do|call|udp|where|if|else|endif|endfor|for|defined by|while|enddo|&[A-Za-z0-9_\-.\s]*=))(\*(?!\/)|[^*])*(\*\/)|(\/\/)[\b\s]*((msg|do|call|udp|where|if|else|endif|endfor|for|defined by|while|enddo|&[A-Za-z0-9_\-.\s]*=)([^\r\n]+)?)";

            return (Regex.Match(source, codeComments).Value != "");

        }

        public static Domain DomainByName(string domainName)
        {
            foreach (Domain d in Domain.GetAll(UIServices.KB.CurrentModel))
            {
                if (d.Name == domainName)
                    {
                    return d;
                    break;
                    }  
             }
            return null;
        }

        public static string RemoveEmptyLines(string lines)
        {
            return Regex.Replace(lines, @"^\s*$\n|\r", "", RegexOptions.Multiline);
        }

        public static int LineCount(string s)
        {
            int n = 0;
            foreach (var c in s)
            {
                if (c == '\n') n++;
            }
            return n;
        }

        public static string linkObject(KBObject obj)
        {
            return "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;OpenObject&name=" + obj.Guid.ToString() + "\">" + obj.Name + "</a>";
        }

        public static string linkFile(string file)
        {
            return "<a href=\"file:///" + file + "\"" + ">" + file + "</a" + ">";
        }

        public static string ExtractRuleParm(KBObject obj)
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
    
        public static string CleanFileName(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string CreateOutputFile(IKBService kbserv, string title)
        {
            string outputFile = kbserv.CurrentKB.UserDirectory + @"\kbdoctor." + Functions.CleanFileName(title) + ".html";
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            return outputFile;
        }

        public static bool AttIsSubtype(Artech.Genexus.Common.Objects.Attribute a)
        {
            if (a.SuperTypeKey != null)
                return true;
            else
                return false;

        }

        public static void KillAttribute(Artech.Genexus.Common.Objects.Attribute a)
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

        private static void CleanVariablesBasedInAttribute(Artech.Genexus.Common.Objects.Attribute a, IOutputService output, KBObject objRef)
        {
            output.AddLine("Cleaning variables references to " + a.Name + " in " + objRef.Name);

            VariablesPart vp = objRef.Parts.Get<VariablesPart>();

            if (vp != null)
            {
                foreach (Variable v in vp.Variables)
                {
                    if (!v.IsStandard)
                    {
                        if ((v.AttributeBasedOn != null) && (a.Name == v.AttributeBasedOn.Name))
                        {
                            output.AddLine("&" + v.Name + " based on  " + a.Name);
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

        private static void CleanSDT(Artech.Genexus.Common.Objects.Attribute a, IOutputService output, KBObject objRef)
        {


            if (objRef is SDT)
            {
                output.AddLine("Cleaning SDT references to " + a.Name + " in " + objRef.Name);
                SDTStructurePart  sdtstruct  = objRef.Parts.Get<SDTStructurePart>();

                foreach (IStructureItem structItem in sdtstruct.Root.Items)
                {
                    try
                    {
                        SDTItem sdtItem = (SDTItem)structItem;
                        if (sdtItem.BasedOn != null && sdtItem.BasedOn.Key == a.Key)
                        {

                            output.AddLine("..." + sdtItem.Name + " based on  " + a.Name);
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
                    catch (Exception e) { output.AddErrorLine(e.Message); };

                }
            }
        }
    


        public static string ReturnPicture(Artech.Genexus.Common.Objects.Attribute a)
        {
            string Picture = "";
            Picture = a.Type.ToString() + "(" + a.Length.ToString() + (a.Decimals > 0 ? "." + a.Decimals.ToString() : "") + ")" + (a.Signed ? "-" : "");
            return Picture;
        }

        public static string ReturnPictureVariable(Variable v)
        {
            string Picture = "";
            Picture = v.Type.ToString() + "(" + v.Length.ToString() + (v.Decimals > 0 ? "." + v.Decimals.ToString() : "") + ")" + (v.Signed ? "-" : "");
            return Picture;
        }

        public static string ReturnPictureDomain(Domain d)
        {

            string Picture = "";
            Picture = d.Type.ToString() + "(" + d.Length.ToString() + (d.Decimals > 0 ? "." + d.Decimals.ToString() : "") + ")" + (d.Signed ? "-" : "");
            return Picture;
        }

        public static void SaveObject(IOutputService output, KBObject obj)
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



    }
}


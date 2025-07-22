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
using Artech.Udm.Framework;

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
            Utility.AddLineSummary(UIServices.KB.CurrentKB, fileName, texto);
        }

        internal static void AddLine(string fileName, string texto)
        {
            Utility.AddLine(UIServices.KB.CurrentKB, fileName, texto);
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
            catch (Exception e) {
                source = "";
                Console.WriteLine(e.Message);
            }

            return source.ToUpper();
        }

        public static string ObjectRulesUpper(KBObject obj)
        {
            string rules = "";
            try
            {
                if (obj is Procedure) rules = obj.Parts.Get<RulesPart>().Source;

                if (obj is Transaction) rules = obj.Parts.Get<RulesPart>().Source;

                if (obj is WorkPanel) rules = obj.Parts.Get<RulesPart>().Source;

                if (obj is WebPanel) rules = obj.Parts.Get<RulesPart>().Source;
            }
            catch (Exception e)
            {
                rules = "";
                Console.WriteLine(e.Message);
            }

            return rules.ToUpper();
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

            return noComments;
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
             }
            return null;
        }

        public static string RemoveEmptyLines(string lines)
        {
            return Utility.RemoveEmptyLines(lines);
        }

        public static int LineCount(string s)
        {
            return Utility.LineCount(s);
        }
            return n;
        }

        public static string linkObject(KBObject obj)
        {
            return Utility.linkObject(obj);
        }

        public static string linkFile(string file)
        {
            return Utility.linkFile(file);
        }

        public static string ExtractRuleParm(KBObject obj)
        {
            return Utility.ExtractRuleParm(obj);
        }
            return aux;
        }
    
        public static string CleanFileName(string filename)
        {
            return Utility.CleanFileName(filename);
        }

        public static string CreateOutputFile(IKBService kbserv, string title)
        {
            return Utility.CreateOutputFile(kbserv.CurrentKB, title);
        }
                catch
                {
                    KBDoctor.KBDoctorOutput.Warning("File " + outputFile + " is locked. The start page cannot be generated");
                    throw new Exception("File " + outputFile + " is locked.The start page cannot be generated");
                }
            }

            return outputFile;
        }

        public static bool AttIsSubtype(Artech.Genexus.Common.Objects.Attribute a)
        {
            return Utility.AttIsSubtype(a);
        }

        public static void KillAttribute(Artech.Genexus.Common.Objects.Attribute a)
        {
            Utility.KillAttribute(a);
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
            output.AddLine("KBDoctor","Cleaning variables references to " + a.Name + " in " + objRef.Name);

            VariablesPart vp = objRef.Parts.Get<VariablesPart>();

            if (vp != null)
            {
                foreach (Variable v in vp.Variables)
                {
                    if (!v.IsStandard && ((v.AttributeBasedOn != null) && (a.Name == v.AttributeBasedOn.Name)))
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

        internal static bool hasModule(KBObject obj)
        {
            return Utility.HasModule(obj);
        }


        internal static KBCategory MainCategory(KBModel model)
        {
            return Utility.MainCategory(model);
        }

        private static void CleanSDT(Artech.Genexus.Common.Objects.Attribute a, IOutputService output, KBObject objRef)
        {


            if (objRef is SDT)
            {
                output.AddLine("KBDoctor","Cleaning SDT references to " + a.Name + " in " + objRef.Name);
                SDTStructurePart  sdtstruct  = objRef.Parts.Get<SDTStructurePart>();

                foreach (IStructureItem structItem in sdtstruct.Root.Items)
                {
                    try
                    {
                        SDTItem sdtItem = (SDTItem)structItem;

                        EntityKey myKey = KBDoctorCore.Sources.Utility.KeyOfBasedOn_CompatibleConEvo3(sdtItem);

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
                    catch (Exception e) { output.AddErrorLine(e.Message); };

                }
            }
        }






        public static void SaveObject(IOutputService output, KBObject obj)
        {
            Utility.SaveObject(output, obj);
        }

    }
}


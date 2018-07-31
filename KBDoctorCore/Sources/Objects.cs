using Artech.Architecture.Common.Collections;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.Language.ComponentModel;
using Artech.Architecture.Language.Parser;
using Artech.Architecture.Language.Services;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Windows.Media;
using Artech.Common.Diagnostics;
using Artech.Architecture.Common.Location;
using Artech.Genexus.Common.Parts;
using Artech.Common.Helpers.Structure;
using Artech.Genexus.Common.Parts.SDT;
using Artech.Udm.Framework;
using Artech.Udm.Framework.References;
using Concepto.Packages.KBDoctor;
using Artech.Genexus.Common.AST;
using Artech.Architecture.Common.Descriptors;
using Artech.Genexus.Common.Types;
//using Artech.Common.Language.Parser;
using Artech.Architecture.Language.Parser.Objects;

namespace Concepto.Packages.KBDoctorCore.Sources
{
    static class Objects
    {
        public enum TokensIds
        {
            FNONE = -1,

            ///////////////////////////////////////////////////////////
            // TOKENS
            ///////////////////////////////////////////////////////////

            // 0
            FOB = 0, // '(' Open Bracket
            FFN,  // 'Function(' Fuction call
            FNA,  // Name Attribute
            FNC,  // Name Cconstant
            FCB,  // ')' Close Bracket
            FPL,  // '+''-' PLus MiNus oper.
            FPR,  // '*''/' PRoduct DiVis. oper.
            FCM,  // ',' CoMma separate parms.
            FNT,  // 'NOT' NoT
            FAN,  // 'AND' 'OR' ANd OR
                  // 10
            FRE,  // '<' '>' '=' RElational oper.
            EXP,  // Expression
            SUM,  // Sum
            COU,  // Count
            AVE,  // Average
            MAX,  // Maximum
            MIN,  // Minimum
            FIF,  // IF ...
            FSC,  // Semicolon ';'
            FOT,  // Otherwise
                  // 20
            ERR_TOKEN,
            FEN,  // EOExpression
            FCO,  // COmment (//) for rules/commands
            FUV,  // User Variable (&) for rules/commands
            FUA,  // User Variable Array '&xx('
            FCN,  // CoNtinuation Line / White spaces
            FAM,  // String to replace '&' with '&&'
            FCL,  // CLass id (used for calls)
            FOI,  // Object Id (used for calls)
            FCT,  // ConTrol ID/Name (for properties)
                  // 30
            FCI,  // Control type Id (combo/edit/etc.)
            FMT,  // control id/name (for MeThods) (Used only in specifier)
            FBI,  // BInary info in value (used to save bin data in obj_info)
            FDC,  // Date constante (used only in dYNQ by now)
            FCV,  // Control Variable (the var associated with the control (Used only in specifier)
            FWH,  // WHEN (GXW) / WHERE (DKL) ...
            FNS,  // Name space ...
            FON,  // ON ...
            FBC,  // Comentario de bloque
            FOR,  // ORDER ...
                  // 40
            TKN_TRUE, // TRUE
            TKN_FALSE, // FALSE
            TKN_NONE, // NONE, para expresión FOR EACH ... ORDER NONE ... ENDFOR 
            PRM,  // Parámetro, utilizado en DYNQ
            FND,  // Name Domain
            FLV,  // LEVEL token
            TKN_NEW, // NEW token
            FSDTCLS, // Structure Class
            TKN_NULL, // NULL
            TKN_IN,  // IN
                     // 50
            SSL,  // SUBSELECT : used by generators; reserved it for Gx.
            FEX,  // Exception name
            TMSGID,  // Message id
            TNCNT,  // Token Name Constant NonTranslatable
            TFOR,  // For token, defined to be used with Lookup Deklarit's rule
            TDEPENDENCIES, // Dependencies token, new condition for rules.
            TRULE,  // Rule token
            TBY,  // 'By' token
            TGIVEN,  // 'Given' token
            TWHERE,  // 'Where' token -GeneXus, Deklarit uses FWH
                     // 60
            TDEFINEDBY, // 'Defined by' token
            TSECTION, // [Web], [Win], [Web], [Text]
            TINDP,  // Used for token 'in <dataselector>'
            OPENSQUAREBRACKET,
            CLOSESQUAREBRACKET,
            OUTPUTNAME,
            OUTPUTDYNAMICSYM,
            INPUT,
            OUTPUTPROPERTY,
            OBJREFERENCE,
            // 70
            TUSING,
            TSIGN, // Now that rules supports comments, define the TSIGN token to specified the sign of an expression (e.g. "-1")
            TEXO,

            // ¡¡¡ UNTIL 99 !!!
            //  Commands codes starts at 100, See dedotcmd.h

            ///////////////////////////////////////////////////////////
            // COMMANDS
            ///////////////////////////////////////////////////////////

            // 100
            DTEJE = 100, // 'Eject'
            DTNSK,  // 'NoSkip'
            DTLNN,  // 'Lineno'
            DTPRC,
            DTCLL,  // 'Call'
            DTDBA,
            DTCOB,
            DTASG,  // Assignment
            DTPRI,
            DTIF,
            // 110
            DTELS,  // 'Else'
            DTEIF,  // 'Endif'
            DTNPR,
            DTDEL,  // 'Delete'
            DTDO,  // 'Do'
            DTEDO,  // 'Enddo'
            DTWHE,
            DTNEW,
            DTRET,
            DTHEA,
            // 120
            DTBEG,
            DTFOR,  // 'ForEach'
            DTEND,
            DTPL,
            DTMT,
            DTMB,
            DTSRC,
            DTENW,
            DTEFO,  // 'EndFor'
            DTWDU,  // 'When Duplicate'
                    // 130
            DTWNO,  // 'When None'
            DTCP,
            DTCMM,
            DTXFE,
            DTXFF,
            DTXNW,
            DTXEF,
            DTXEN,
            DTDBY,
            DTEXF,  // 'Exit' from a 'Do While'
                    // 140
            DTEXD,
            DTMSG,
            DTFOO,
            DTPRO, // 'Sub' <subroutine>
            DTEPR, // 'EndSub'
            DTDOP, // Do <subroutine>
            DTEVT,
            DTEEV,
            DTREF,
            DTFLN,
            // 150
            DTEFL,
            DTCNF,
            DTDOC,
            DTCAS,
            DTECA,
            DTLOA,
            DTLVL,
            DTRBK, // Comando ROLLBACK
            DTSBM, // Comando SUBMIT
            DTGRA,
            // 160
            DTERH, // Commando Error_Handler
            DTVB,  // Comando VB
            DTFSL,
            DTDMY, //Reserved for spec RPC
            DTOTH,
            DTEFS, // Reserved for End for each selected line
            DTJAV, // Comando JAVA
            DTSQL, // Comando SQL
            DTFLS,
            DTFSS,
            // 170
            DTEFF,
            DTLNK, // Comando LINK
            DTAPL, // Asignación del tipo +=
            DTAMI, // Asignación del tipo -=
            DTAMU, // Asignación del tipo *=
            DTADI, // Asignación del tipo /=
            DTFIN, // FOR <var> IN <array>
            DTEFI, // END // del token anterior
            DTFFT, // FOR <var>=<exp> TO <exp> STEP <exp>
            DTEFT, // END // del token anterior
                   // 180
            DTIN,  // Comando IN de FOR <var> IN <array>
            DTTO,  // Comando TO de FOR EACH <var>=<exp> TO <exp>
            DTSTP, // Comando STEP de FOR <var>=<exp> TO <exp> STEP <exp>
            DTCSH, // Comando CSHARP
            DTON,  // Comando ON
            DTWHN, // Comando WHEN
            DTOPD, // Comando OPTION DISTINCT
            DTUSG, // Comando USING de FOR EACH ... ENDFOR
            DTPOPUP, // Comando POPUP()
            BLOCKING, // Comando BLOCKING
                      // 190
            OUTPUTELEMENT,
            OPENCURLYBRACKET,
            CLOSECURLYBRACKET,
            PRINT,
            INSERT,
            SUBGROUP,
            ENDSUBGROUP,
            DTStub, // 'public sub'
            DTJavaScript, // 'javascript' command - not implemented yet! - reserved number
            DTEndStub,
            //200
            DTCallStub,
            DTRuby,  // Comando "RUBY <LINE>"

            DTREDUNDANCY = 397, // Used to give redundancy info to the specifier
        };



        internal static KBDAST ParseSourceIntoAST(Artech.Genexus.Common.Parts.ProcedurePart source)
        {

            /* if (source != null)
             {
                 Stack stk;
                 ILanguageService parserSrv = Artech.Architecture.Common.Services.Services.GetService(new Guid("C26F529E-9A69-4df5-B825-9194BA3983A3")) as ILanguageService;
                 IParserEngine parser = parserSrv.CreateEngine();
                 ParserInfo parserInfo = new ParserInfo(source);
                 foreach (TokenData token in parser.GetTokens(true, parserInfo, source.Source))
                 {
                     if (token.Token >= 100)
                     {
                         //Command   
                         List<TokensIds>[] IndentTokens = GetIndentationTokens()
                     }
                     else
                     {
                         //Token

                     }
                 }
             }*/
            return null;


            if (source != null)
            {
                Stack stk;
                ILanguageService parserSrv = Artech.Architecture.Common.Services.Services.GetService(new Guid("C26F529E-9A69-4df5-B825-9194BA3983A3")) as ILanguageService;
                IParserEngine parser = parserSrv.CreateEngine();
                ParserInfo parserInfo = new ParserInfo(source);
                foreach (TokenData token in parser.GetTokens(true, parserInfo, source.Source))
                {
                    if (token.Token >= 100)
                    {
                        //Command   
                        List<TokensIds>[] IndentTokens = GetIndentationTokens();
                    }
                    else
                    {
                        //Token

                    }
                }

            }
            return null;

        }

        private static List<TokensIds>[] GetIndentationTokens()
        {
            List<TokensIds>[] tokens = new List<TokensIds>[300];

            tokens[114] = new List<TokensIds>();
            tokens[114].Add((TokensIds)115);

            tokens[152] = new List<TokensIds>();
            tokens[152].Add((TokensIds)154);

            tokens[153] = new List<TokensIds>();
            tokens[153].Add((TokensIds)153);
            tokens[153].Add((TokensIds)154);
            tokens[153].Add((TokensIds)164);

            tokens[109] = new List<TokensIds>();
            tokens[109].Add((TokensIds)110);
            tokens[109].Add((TokensIds)111);

            tokens[110] = new List<TokensIds>();
            tokens[110].Add((TokensIds)111);

            tokens[176] = new List<TokensIds>();
            tokens[176].Add((TokensIds)177);

            tokens[121] = new List<TokensIds>();
            tokens[121].Add((TokensIds)128);
            tokens[121].Add((TokensIds)130);

            tokens[143] = new List<TokensIds>();
            tokens[143].Add((TokensIds)144);

            tokens[117] = new List<TokensIds>();
            tokens[117].Add((TokensIds)127);
            tokens[117].Add((TokensIds)129);

            tokens[130] = new List<TokensIds>();
            tokens[130].Add((TokensIds)128);

            tokens[129] = new List<TokensIds>();
            tokens[129].Add((TokensIds)127);

            tokens[164] = new List<TokensIds>();
            tokens[164].Add((TokensIds)154);

            return tokens;

        }

        internal static void GetClassesTypesWithTheSameSignature(IEnumerable<KBObject> objects, out HashSet<int> classes, out Hashtable[] Classes_types)
        {
            int MAX_PARAMS = 512;
            int max_quantity = 0;
            classes = new HashSet<int>();
            List<KBObject>[] Classes_quantity = new List<KBObject>[MAX_PARAMS];

            //Divido el conjunto de objetos en clases determinadas por la cantidad de parametros (tomando en cuenta si son IN/OUT/INOUT). 
            foreach (KBObject obj in objects)
            {
                ICallableObject callableObject = obj as ICallableObject;

                if (callableObject != null)
                {
                    foreach (Signature signature in callableObject.GetSignatures())
                    {
                        if (Classes_quantity[signature.ParametersCount] == null)
                        {
                            if (signature.ParametersCount > 0)
                            {
                                Classes_quantity[signature.ParametersCount] = new List<KBObject>();
                                classes.Add(signature.ParametersCount);
                                if (signature.ParametersCount > max_quantity)
                                {
                                    max_quantity = signature.ParametersCount;
                                }
                            }
                        }
                        if (signature.ParametersCount > 0)
                        {
                            Classes_quantity[signature.ParametersCount].Add(obj);
                        }
                    }
                }
            }

            //Divido el conjunto de objetos nuevamente por tipo de datos. 
            Classes_types = new Hashtable[max_quantity];
            foreach (int i in classes)
            {
                Hashtable Class_type = new Hashtable();
                foreach (KBObject obj in Classes_quantity[i])
                {
                    string paramstring = GetParametersString(obj);
                    List<KBObject> objects_list;
                    if (Class_type.ContainsKey(paramstring))
                    {
                        objects_list = Class_type[paramstring] as List<KBObject>;
                        objects_list.Add(obj);
                        Class_type[paramstring] = new List<KBObject>(objects_list);
                    }
                    else
                    {
                        objects_list = new List<KBObject>();
                        objects_list.Add(obj);
                        Class_type.Add(paramstring, new List<KBObject>(objects_list));
                    }
                }
                Classes_types[i - 1] = new Hashtable(Class_type);
            }
        }

        private static string GetParametersString(KBObject obj)
        {

            Tuple<int, string> type_access;
            List<Tuple<int, string>> parameters = new List<Tuple<int, string>>();
            ICallableObject callableObject = obj as ICallableObject;
            foreach (Signature signature in callableObject.GetSignatures())
            {
                foreach (RuleDefinition.ParameterDefinition param in signature.Data.GetParameters())
                {
                    parameters.Add(new Tuple<int, string>(param.Type.DataType, param.AccessType.ToString()));
                }
            }
            string paramstring = "";
            parameters.Sort(Comparer<Tuple<int, string>>.Default);
            foreach (Tuple<int, string> parameter in parameters)
            {
                string accessor = "";
                if (parameter.Item2 == "PARM_IN")
                {
                    accessor = "I";
                }
                if (parameter.Item2 == "PARM_INOUT")
                {
                    accessor = "IO";
                }
                if (parameter.Item2 == "PARM_OUT")
                {
                    accessor = "O";
                }
                paramstring += accessor + ":" + parameter.Item1.ToString() + " ";
            }
            return paramstring.TrimEnd();
        }

        internal static List<KBObject> ParmWOInOut(KnowledgeBase KB, IOutputService output)
        {
            string title = "KBDoctor - Objects with parameters without IN:/OUT:/INOUT:";
            output.StartSection("KBDoctor", title);
            string rec = "";
            List<KBObject> objs = KB.DesignModel.Objects.GetAll().ToList();
            List<KBObject> objectsWithProblems = GetObjectsWithProblems(objs, output, ref rec);
            bool success = true;
            output.EndSection("KBDoctor", title, success);
            return objectsWithProblems;
        }

        internal static List<KBObject> ParmWOInOut(List<KBObject> objs, IOutputService output, ref string recommendations)
        {
            // Object with parm() rule without in: out: or inout:

            List<KBObject> objectsWithProblems = GetObjectsWithProblems(objs, output, ref recommendations);
            bool success = true;
            return objectsWithProblems;
        }

        private static List<KBObject> GetObjectsWithProblems(List<KBObject> objs, IOutputService output, ref string recommendations)
        {
            int numObj = 0;
            int objWithProblems = 0;
            List<KBObject> objectsWithProblems = new List<KBObject>();
            foreach (KBObject obj in objs)
            {
                ICallableObject callableObject = obj as ICallableObject;

                if (callableObject != null)
                {
                    numObj += 1;
                    if ((numObj % 100) == 0)
                        output.AddLine("KBDoctor", "Processing " + obj.Name);
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

                            string recommend = "Parameter INOUT ";
                            OutputError err = new OutputError(recommend, MessageLevel.Warning, new KBObjectPosition(obj.Parts.Get<RulesPart>()));
                            recommendations += recommend + "<br>";
                            output.Add("KBDoctor", err);

                            string ruleParm = Utility.ExtractRuleParm(obj);
                            if (ruleParm != "")
                            {
                                int countparms = ruleParm.Split(new char[] { ',' }).Length;
                                int countsemicolon = ruleParm.Split(new char[] { ':' }).Length - 1;
                                if (countparms != countsemicolon)
                                {
                                    objWithProblems += 1;
                                    objectsWithProblems.Add(obj);
                                     recommend = "Parameter without IN/OUT/INOUT ";
                                     err = new OutputError(recommend, MessageLevel.Error, new KBObjectPosition(obj.Parts.Get<RulesPart>()));
                                    recommendations += recommend + "<br>";
                                    output.Add("KBDoctor", err);
                                }
                            }
                        }
                    }
                }
            }

            return objectsWithProblems;
        }

        private static bool isGeneratedbyPattern(KBObject obj)
        {
            if (!(obj == null))
            { return obj.GetPropertyValue<bool>(KBObjectProperties.IsGeneratedObject); }
            else
            { return true; }

        }

        private static bool isGenerated(KBObject obj)
        {
            if (obj is DataSelector)  //Los Dataselector no tienen la propiedad de generarlos o no , por lo que siempre devuelven falso y sin son referenciados se generan. 
                return true;
            object aux = obj.GetPropertyValue(Artech.Genexus.Common.Properties.TRN.GenerateObject);
            return ((aux != null) && (aux.ToString() == "True"));

        }

        private static int CalculateComplexityIndex(int MaxCodeBlock, int MaxNestLevel, int ComplexityLevel, string ParmINOUT)
        {
            int ComplexityIndex = 0;
            if (ParmINOUT == "Error") ComplexityIndex += 100;
            ComplexityIndex += MaxNestLevel * MaxNestLevel;
            ComplexityIndex += ComplexityLevel * 10;
            ComplexityIndex += MaxCodeBlock * 2;
            return ComplexityIndex;
        }

        private static int ParametersCountObject(KBObject obj)
        {
            int countparm = 0;
            ICallableObject callableObject = obj as ICallableObject;
            if (callableObject != null)
            {
                foreach (Signature signature in callableObject.GetSignatures())
                {
                    countparm = signature.ParametersCount;
                }
            }
            return countparm;
        }

        internal static void CommitOnExit(List<KBObject> objs, IOutputService output, ref string recommendations)
        {
            bool commitOnExit;
            foreach (KBObject obj in objs)
            {
                if (obj is Procedure)
                {
                    object aux = obj.GetPropertyValue("CommitOnExit");
                    if (aux != null)
                    {
                        commitOnExit = aux.ToString() == "Yes";
                        if (commitOnExit)
                        {
                            string recommend = "Commit on EXIT = YES ";
                            OutputError wrn = new OutputError(recommend, MessageLevel.Warning, new KBObjectAnyPosition(obj));
                            recommendations += recommend + "<br>";
                            output.Add("KBDoctor", wrn);
                        }
                    }
                }
            }
        }

        internal static void CheckComplexityMetrics(List<KBObject> objs, IOutputService output, int maxNestLevel, int maxComplexityLevel, int maxCodeBlock, int maxParametersCount, ref string recommendations)
        {
            foreach (KBObject obj in objs)
            {
                if (obj is Transaction || obj is WebPanel || obj is Procedure || obj is WorkPanel)
                {
                    if (isGenerated(obj) && !isGeneratedbyPattern(obj))
                    {
                        string source = Utility.ObjectSourceUpper(obj);
                        source = Utility.RemoveEmptyLines(source);

                        string sourceWOComments = Utility.ExtractComments(source);
                        sourceWOComments = Utility.RemoveEmptyLines(sourceWOComments);

                        int CodeBlock = Utility.MaxCodeBlock(sourceWOComments);
                        int NestLevel = Utility.MaxNestLevel(sourceWOComments);
                        int ComplexityLevel = Utility.ComplexityLevel(sourceWOComments);

                        KBObjectPart part = Utility.ObjectSourcePart(obj);

                        if (NestLevel > maxNestLevel)
                        {
                            string recommend = "Nested level too high (" + NestLevel.ToString() + "). Recommended max: " + maxNestLevel.ToString();
                            OutputError err = new OutputError(recommend, MessageLevel.Error, new KBObjectPosition(part));
                            recommendations += recommend + "<br>";
                            output.Add("KBDoctor", err);

                        }
                        if (ComplexityLevel > maxComplexityLevel)
                        {
                            string recommend = "Complexity too high(" + ComplexityLevel.ToString() + ").Recommended max: " + maxComplexityLevel.ToString();
                            OutputError err = new OutputError(recommend, MessageLevel.Error, new KBObjectPosition(part));
                            recommendations += recommend + "<br>";
                            output.Add("KBDoctor", err);
                        }

                        if (CodeBlock > maxCodeBlock)
                        {
                            string recommend = "Code block too large(" + CodeBlock.ToString() + ").Recommended max: " + maxCodeBlock.ToString();
                            OutputError err = new OutputError(recommend, MessageLevel.Error, new KBObjectPosition(part));
                            recommendations += recommend + "<br>";
                            output.Add("KBDoctor", err);
                        }

                        int parametersCount = ParametersCountObject(obj);
                        if (parametersCount > maxParametersCount)
                        {
                            string recommend = "Too many parameters (" + parametersCount.ToString() + ").Recommended max: " + maxParametersCount.ToString();
                            KBObjectPart rpart = Utility.ObjectRulesPart(obj);
                            OutputError err = new OutputError(recommend, MessageLevel.Error, new KBObjectPosition(rpart));
                            recommendations += recommend + "<br>";
                            output.Add("KBDoctor", err);
                        }
                    }
                }
            }
        }

        internal static void isInModule(List<KBObject> objs, IOutputService output, ref string recommendations)
        {
            foreach (KBObject obj in objs)
            {
                if (obj.Module.Description == "Root Module" && !Utility.IsMain(obj))
                {
                    string recommend = "Object without module.";
                    OutputError err = new OutputError(recommend, MessageLevel.Warning, new KBObjectAnyPosition(obj));
                    recommendations += recommend + "<br>";
                    output.Add("KBDoctor", err);
                }
            }
        }

        internal static void ObjectsWithVarNotBasedOnAtt(List<KBObject> objs, IOutputService output, ref string recommendations)
        {
            foreach (KBObject obj in objs)
            {
                string vnames = "";
                Boolean hasErrors = false;
                Boolean SaveObj = false;
                if (isGenerated(obj) && (obj is Transaction || obj is WebPanel || obj is WorkPanel || obj is Procedure))
                {
                    string pic2 = (string)obj.GetPropertyValue("ATT_PICTURE");
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    if (vp != null)
                    {
                        foreach (Variable v in vp.Variables)
                        {
                            if ((!v.IsStandard) && Utility.VarHasToBeInDomain(v))
                            {

                                string attname = (v.AttributeBasedOn == null) ? "" : v.AttributeBasedOn.Name;
                                string domname = (v.DomainBasedOn == null) ? "" : v.DomainBasedOn.Name;
                                if (attname == "" && domname == "")
                                {
                                    string vname = v.Name.ToLower();
                                    vnames += vname + " ";
                                    hasErrors = true;
                                    if (FixObjectVariable(v))
                                        SaveObj = true;
                                }
                            }
                        }
                    }
                    if (hasErrors)
                    {
                        string recommend = "Variables not based in attributes or domain: " + vnames;
                        OutputError err = new OutputError(recommend, MessageLevel.Error, new KBObjectPosition(vp));
                        recommendations += recommend + "<br>";
                        output.Add("KBDoctor", err);
                    }
                    if (SaveObj)
                        obj.Save();
                }
            }
        }

        private static bool FixObjectVariable(Variable v)
        {
            KBObject obj = v.KBObject;
            KBModel kBModel = obj.Model;
            foreach (Artech.Genexus.Common.Objects.Attribute att in kBModel.Objects.GetByName("Attributes", new Guid("adbb33c9-0906-4971-833c-998de27e0676"), v.Name))
            {
                KBDoctorOutput.Message("Attribute " + att.Name + " assigned to variable " + v.Name);
                v.AttributeBasedOn = att;
                return true;
                break;
            }

            //Si no asigno atributo intento asignar dominio
            if (v.AttributeBasedOn == null)
            {
                foreach (Domain d in kBModel.Objects.GetByName("Domains", new Guid("00972a17-9975-449e-aab1-d26165d51393"), v.Name))
                {

                    KBDoctorOutput.Message("Domain " + d.Name + " assigned to variable " + v.Name);
                    v.DomainBasedOn = d;
                    return true;
                    break;
                }
            }
            return false;

        }

        internal static void CodeCommented(List<KBObject> objs, IOutputService output, ref string recommendations)
        {
            foreach (KBObject obj in objs)
            {
                string source = Utility.ObjectSourceUpper(obj);
                source = Utility.RemoveEmptyLines(source);
                string codeCommented = Utility.CodeCommented(source);
                codeCommented = codeCommented.Replace("'", "");
                codeCommented = codeCommented.Replace(">", "");
                codeCommented = codeCommented.Replace("<", "");
                if (codeCommented != "")
                {
                    string snippet = (codeCommented.Length > 30) ? codeCommented.Substring(1, 30) + "..." : codeCommented;
                    KBObjectPart part = Utility.ObjectSourcePart(obj);
                    string recommend = "Commented code [" + snippet + "]";
                    OutputError err = new OutputError(recommend, MessageLevel.Warning, new KBObjectPosition(part));
                    recommendations += recommend + "<br>";
                    output.Add("KBDoctor", err);
                }
            }
        }

        internal static void AttributeHasDomain(List<KBObject> objs, IOutputService output, ref string recommendations)
        {
            foreach (KBObject obj in objs)
            {
                if (obj is Artech.Genexus.Common.Objects.Attribute)
                {
                    Artech.Genexus.Common.Objects.Attribute a = (Artech.Genexus.Common.Objects.Attribute)obj;
                    string Picture = Utility.ReturnPicture(a);
                    bool isSubtype = Utility.AttIsSubtype(a);

                    if ((a.DomainBasedOn == null) && !isSubtype && Utility.AttHasToBeInDomain(a))
                    {
                        string recommend = "Attribute without domain: " + a.Name;
                        OutputError err = new OutputError(recommend, MessageLevel.Warning, new KBObjectAnyPosition(obj));
                        recommendations += recommend + "<br>";
                        output.Add("KBDoctor", err);
                    }
                }

            }
        }

        internal static void SDTBasedOnAttDomain(List<KBObject> objs, IOutputService output, ref string recommendations)
        {
            foreach (KBObject obj in objs)
            {
                if (obj is SDT)
                {
                    SDTStructurePart sdtstruct = obj.Parts.Get<SDTStructurePart>();
                    bool hasItemNotBasedOn = false;
                    string itemnames = "";
                    foreach (IStructureItem structItem in sdtstruct.Root.Items)
                    {
                        if (structItem is SDTItem)
                        {
                            SDTItem sdtItem = (SDTItem)structItem;
                            if (sdtItem.BasedOn == null && sdtItem.AttributeBasedOn == null && Utility.TypeHasToBeInDomain(sdtItem.Type))
                            {
                                hasItemNotBasedOn = true;
                                itemnames += sdtItem.Name + " ";
                            }
                        }
                    }
                    if (hasItemNotBasedOn)
                    {
                        string recommend = "SDT with items without domain: " + itemnames;
                        OutputError err = new OutputError(recommend, MessageLevel.Warning, new KBObjectAnyPosition(obj));
                        recommendations += recommend + "<br>";
                        output.Add("KBDoctor", err);
                    }
                }

            }
        }

        internal static void AttributeWithoutTable(List<KBObject> objs, IOutputService output)
        {
            List<Artech.Genexus.Common.Objects.Attribute> attTodos = new List<Artech.Genexus.Common.Objects.Attribute>();

            KBModel model = null;
            foreach (KBObject obj in objs)
            {
                model = obj.Model;
                if (obj is Artech.Genexus.Common.Objects.Attribute)
                {
                    attTodos.Add((Artech.Genexus.Common.Objects.Attribute)obj);
                }
            }
            if (model != null)
            {
                foreach (Table t in Table.GetAll(model))
                {
                    foreach (EntityReference reference in t.GetReferences(LinkType.UsedObject))
                    {
                        KBObject objRef = KBObject.Get(model, reference.To);
                        if (objRef is Artech.Genexus.Common.Objects.Attribute)
                        {
                            Artech.Genexus.Common.Objects.Attribute a = (Artech.Genexus.Common.Objects.Attribute)objRef;
                            attTodos.Remove(a);
                        }
                    }
                }
            }
            foreach (Artech.Genexus.Common.Objects.Attribute att in attTodos)
            {
                string recommend = "Attribute without table: " + att.Name;
                OutputError err = new OutputError(recommend, MessageLevel.Error, new KBObjectAnyPosition(att));
                output.Add("KBDoctor", err);
            }
        }

        public static List<KBObject> ObjectsUpdateAttribute(List<KBObject> updaters, Artech.Genexus.Common.Objects.Attribute att, IOutputService output)
        {
            List<KBObject> retobjs = new List<KBObject>();
            foreach (KBObject obj in updaters)
            {
                if (obj is Procedure)
                {
                    output.AddLine("KBDoctor", obj.Name);
                    string name = obj.Name;
                    if (ProcedureUpdateAttribute(obj, att))
                    {
                        retobjs.Add(obj);
                    }
                }
            }
            return retobjs;
        }

        public static List<KBObject> ObjectsUpdatingTable(Table t)
        {
            KBModel model = t.Model;
            List<KBObject> updaters = (from r in model.GetReferencesTo(t.Key, LinkType.UsedObject)
                                       where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                       where ReferenceTypeInfo.HasUpdateAccess(r.LinkTypeInfo) || ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo)
                                       select model.Objects.Get(r.From)).ToList();
            return updaters;
        }

        private static bool ProcedureUpdateAttribute(KBObject proc, KBObject att)
        {
            var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
            ParserInfo parserInfo;
            Artech.Genexus.Common.Parts.ProcedurePart source = proc.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
            Artech.Genexus.Common.Parts.VariablesPart vp = proc.Parts.Get<VariablesPart>();

            if (source != null)
            {
                parserInfo = new ParserInfo(source);

                var info = new Artech.Architecture.Language.Parser.ParserInfo(source);
                if (parser.Validate(info, source.Source))
                {
                    Artech.Genexus.Common.AST.AbstractNode paramRootNode = Artech.Genexus.Common.AST.ASTNodeFactory.Create(parser.Structure, source, vp, info);
                }
                bool InAssign = false;
                bool Equal = false;
                bool HasAttribute = false;
                foreach (TokenData token in parser.GetTokens(true, parserInfo, source.Source))
                {
                    if (InAssign && token.Token > 100) //Command
                    {
                        InAssign = false;
                        Equal = false;
                        HasAttribute = false;
                    }
                    if (token.Token == 107) //Assignment 
                    {
                        InAssign = true;
                    }
                    if (InAssign && token.Token == 10) //Relational Oper =
                    {
                        if (HasAttribute)
                            return true;
                        Equal = true;
                    }
                    if (InAssign && (!Equal) && token.Token == 2 && token.Word == att.Name) //Estoy dentro de un Assign, antes del operador = y el atributo es el que estoy buscando.
                    {
                        HasAttribute = true;
                    }

                }
            }
            return false;
        }

        internal static void AssignTypeComparer(KBModel model, KBObject obj, IOutputService output)
        {
            if (!isGeneratedbyPattern(obj))
            {
                if (obj is Procedure)
                {
                    Artech.Genexus.Common.Parts.ProcedurePart procpart = obj.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
                    Artech.Genexus.Common.Parts.VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    Artech.Genexus.Common.Parts.RulesPart rules = obj.Parts.Get<RulesPart>();
                    if (procpart != null)
                    {
                        ProccessAssignmentsInSource(model, procpart, vp, output, obj.Name);
                    }
                    if (rules != null)
                    {
                        ProccessAssignmentsInSource(model, rules, vp, output, obj.Name);
                    }
                }
                else
                {
                    if (obj is WebPanel || obj is Transaction)
                    {
                        Artech.Genexus.Common.Parts.EventsPart eventspart = obj.Parts.Get<Artech.Genexus.Common.Parts.EventsPart>();
                        Artech.Genexus.Common.Parts.VariablesPart vp = obj.Parts.Get<VariablesPart>();
                        Artech.Genexus.Common.Parts.RulesPart rules = obj.Parts.Get<RulesPart>();
                        if (eventspart != null)
                        {
                            ProccessAssignmentsInSource(model, eventspart, vp, output, obj.Name);
                        }
                        if (rules != null)
                        {
                            ProccessAssignmentsInSource(model, rules, vp, output, obj.Name);
                        }
                    }
                }
            }
        }

        private static void ProccessAssignmentsInSource(KBModel model, SourcePart procpart, VariablesPart vp, IOutputService output, string objname)
        {
            var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
            ParserInfo parserInfo;
            parserInfo = new ParserInfo(procpart);

            var info = new Artech.Architecture.Language.Parser.ParserInfo(procpart);
            if (parser.Validate(info, procpart.Source))
            {
                Artech.Genexus.Common.AST.AbstractNode paramRootNode = Artech.Genexus.Common.AST.ASTNodeFactory.Create(parser.Structure, procpart, vp, info);
                List<AbstractNode> assigns = getAssignmentsInSource(paramRootNode);
                foreach(AssignmentNode assign in assigns)
                {
                    if(assign.Left is VariableNameNode)
                    {
                        VariableNameNode vn = (VariableNameNode) assign.Left;
                        Variable varL = vp.GetVariable(vn.VarName);
                        if(varL != null)
                        {
                            string picture = Utility.ReturnPictureVariable(varL);
                            Domain domL = varL.DomainBasedOn;
                            CompareAssignTypes(model, vp, output, assign, picture, domL, objname, procpart);
                        }
                    }
                    if (assign.Left is AttributeNameNode)
                    {
                        AttributeNameNode an = (AttributeNameNode)assign.Left;
                        Artech.Genexus.Common.Objects.Attribute att = an.Attribute;
                        if (att != null)
                        {
                            string picture = Utility.ReturnPicture(att);
                            Domain domL = att.DomainBasedOn;
                            CompareAssignTypes(model, vp, output, assign, picture, domL, objname, procpart);
                        }
                        
                    }
                    if (assign.Left is ObjectPropertyNode)
                    {
                        ObjectPropertyNode op = (ObjectPropertyNode)assign.Left;

                        AttributeTree.Dependency.Types type;
                        StructureTypeReference parentRef = AttributeTree.GetStructureTypeReference(op.Children.First(), model, out type);

                        string ChildText = op.Children.Skip(1).First().Text;
                        foreach (Artech.Common.Helpers.Structure.IStructureItem item in AttributeTree.GetStructureSubStructures(parentRef, model))
                        {
                            if (item.Name == ChildText)
                                if (item is SDTLevel)
                                    new StructureTypeReference(((SDTLevel)item).ItemEntity.Type, ((SDTLevel)item).ItemEntity.Id);
                                else if (item is TransactionLevel)
                                {
                                    TransactionLevel TrnLvl = (TransactionLevel)item;
                                    new StructureTypeReference(TrnLvl.Transaction.Key.Type, TrnLvl.Transaction.Key.Id, StructureInfoProvider.GetLevelPrimaryKey(TrnLvl));
                                }
                                else if (item is SDTItem)
                                {
                                    SDTItem sdtitem = (SDTItem)item;
                                    Domain domL = sdtitem.DomainBasedOn;
                                    string pictureL;
                                    if (domL != null)
                                    {
                                        pictureL = Utility.ReturnPictureDomain(domL);
                                    }
                                    else
                                    {
                                        pictureL = Utility.ReturnFormattedType(sdtitem.Type, sdtitem.Length, sdtitem.Decimals, sdtitem.Signed);
                                    }

                                    CompareAssignTypes(model, vp, output, assign, pictureL, domL, objname, procpart);
                                }
                                else if (item is TransactionAttribute)
                                {

                                }
                                else if (item is Artech.Genexus.Common.Parts.ExternalObject.ExternalObjectProperty)
                                {

                                }
                                else
                                {

                                }
                        }
                    }
                }
            }
        }

        private static void CompareAssignTypes(KBModel model, VariablesPart vp, IOutputService output, AssignmentNode assign, string pictureL, Domain domL, string objname, KBObjectPart part)
        {
            if (assign.Right is VariableNameNode)
            {
                VariableNameNode vnr = (VariableNameNode)assign.Right;
                Variable varR = vp.GetVariable(vnr.VarName);
                string pictureR = Utility.ReturnPictureVariable(varR);
                Domain domR = varR.DomainBasedOn;
                CheckVarAndAttAssignTypes(output, assign, pictureL, domL, pictureR, domR, objname, part);

            }
            if (assign.Right is AttributeNameNode)
            {
                AttributeNameNode anR = (AttributeNameNode)assign.Right;
                if((assign.Right.Text.ToLower() != "true" && assign.Right.Text.ToLower() != "false") && pictureL.ToLower().Contains("boolean"))
                {
                    Artech.Genexus.Common.Objects.Attribute att = anR.Attribute;
                    if(att != null) { 
                        string pictureR = Utility.ReturnPicture(att);
                        Domain domR = att.DomainBasedOn;
                        CheckVarAndAttAssignTypes(output, assign, pictureL, domL, pictureR, domR, objname, part);
                    }
                }
                if(!pictureL.ToLower().Contains("boolean"))
                {
                    Artech.Genexus.Common.Objects.Attribute att = anR.Attribute;
                    if(att != null) { 
                        string pictureR = Utility.ReturnPicture(att);
                        Domain domR = att.DomainBasedOn;
                        CheckVarAndAttAssignTypes(output, assign, pictureL, domL, pictureR, domR, objname, part);
                    }
                }
            }
            if (assign.Right is FunctionNode)
            {
                FunctionNode fn = (FunctionNode)assign.Right;
                if (fn.Element != null)
                {
                    KBObject proc = (KBObject)(((FunctionNode)assign.Right).Element.Name.Tag);
                    if(proc != null)
                    {
                        string pictureR = Utility.GetOutputFormatedType(proc);
                        Domain domR = Utility.GetOutputDomains(proc);
                        if (pictureR != "")
                        {
                            CheckVarAndAttAssignTypes(output, assign, pictureL, domL, pictureR, domR, objname, part);
                        }
                    }
                    else
                    {
                        if(fn.FunctionName.ToLower() == "udp")
                        {
                            if (fn.Children.First() is ObjectPropertyNode)
                            {
                                ObjectPropertyNode opn = (ObjectPropertyNode)fn.Children.First();
                                if (opn.Node != null)
                                {
                                    string methodname = opn.PropertyName;
                                    string text = opn.Text;
                                    string[] splits = text.ToLower().Split('.');
                                    if (splits.Length > 1)
                                    {
                                        KBObject obj = Utility.GetObjectByNameModule(model, methodname, splits[splits.Length - 2]);
                                        string pictureR = Utility.GetOutputFormatedType(obj);
                                        Domain domR = Utility.GetOutputDomains(obj);
                                        if (pictureR != "")
                                        {
                                            CheckVarAndAttAssignTypes(output, assign, pictureL, domL, pictureR, domR, objname, part);
                                        }
                                    }
                                }
                            }
                            else if(fn.Children.First() is AttributeNameNode)
                            {
                                AttributeNameNode ann = (AttributeNameNode)fn.Children.First();
                                KBObject obj = (KBObject)ann.Element.Tag;
                                string pictureR = Utility.GetOutputFormatedType(obj);
                                Domain domR = Utility.GetOutputDomains(obj);
                                if(pictureR != "")
                                {
                                    CheckVarAndAttAssignTypes(output, assign, pictureL, domL, pictureR, domR, objname, part);
                                }
                            }
                        }
                    }
                }
            }
            if (assign.Right is ObjectMethodNode)
            {
                ObjectMethodNode omn = (ObjectMethodNode) assign.Right;
                if(omn.Node != null)
                {
                    string methodname = omn.MethodName;
                    if (methodname == "udp") {
                        if(omn.Children.First() is ObjectPropertyNode) { 
                            ObjectPropertyNode opn = (ObjectPropertyNode)omn.Children.First();
                            if (opn.Node != null)
                            {
                                string objectname = opn.PropertyName;
                                string text = opn.Text;
                                string[] splits = text.ToLower().Split('.');
                                if (splits.Length > 1)
                                {
                                    KBObject obj = Utility.GetObjectByNameModule(model, objectname, splits[splits.Length - 2]);
                                    string pictureR = Utility.GetOutputFormatedType(obj);
                                    Domain domR = Utility.GetOutputDomains(obj);
                                    if (pictureR != "")
                                    {
                                        CheckVarAndAttAssignTypes(output, assign, pictureL, domL, pictureR, domR, objname, part);
                                    }
                                }
                            }
                        }
                        else if(omn.Children.First() is AttributeNameNode)
                        {
                            AttributeNameNode ann = (AttributeNameNode)omn.Children.First();
                            KBObject obj = (KBObject)ann.Element.Tag;
                            string pictureR = Utility.GetOutputFormatedType(obj);
                            Domain domR = Utility.GetOutputDomains(obj);
                            if (pictureR != "")
                            {
                                CheckVarAndAttAssignTypes(output, assign, pictureL, domL, pictureR, domR, objname, part);
                            }
                        }
                    }
                    else
                    {
                        string text = omn.Text;
                        string[] splits = text.ToLower().Split('.');
                        if (splits.Length > 1)
                        {
                            KBObject obj = Utility.GetObjectByNameModule(model, methodname, splits[splits.Length - 2]);
                            string pictureR = Utility.GetOutputFormatedType(obj);
                            Domain domR = Utility.GetOutputDomains(obj);
                            if (pictureR != "")
                            {
                                CheckVarAndAttAssignTypes(output, assign, pictureL, domL, pictureR, domR, objname, part);
                            }
                        }
                    }
                }
            }
            if (assign.Right is StringConstantNode)
            {          
                StringConstantNode scn = (StringConstantNode)assign.Right;
                string text = scn.Text;
                int textlength = text.Length - 2;   //Chequeo logitud ignorando las 2 comillas
                if(textlength > int.Parse(getLengthFromPicture(pictureL)))
                {
                    OutputError err = new OutputError(Utility.ExtractCommentsAndBreakLines(assign.Text) + " Text assigned is too long (" + textlength.ToString() + ") for " + pictureL, MessageLevel.Warning, new KBObjectPosition(part));
                    output.Add("KBDoctor", err);
                }
            }
            if (assign.Right is NumberNode)
            {
                if (!pictureL.ToLower().Contains("boolean")) {
                    NumberNode nn = (NumberNode)assign.Right;
                    string text = nn.Text;
                    string[] def = SplitDecimals(text);
                    string lengthPic = getLengthFromPicture(pictureL);
                    string[] defPic = SplitDecimals(lengthPic);
                    bool hasLength = false;
                    if (defPic[1] != "0")                                                                   //Chequeo de longitud (wiki genexus): If it is defined as numeric you must consider that the
                                                                                                            //whole length includes the decimal places, the decimal point and the sign.
                    {
                        hasLength = (int.Parse(defPic[0]) - 1) >= (def[0].Length + def[1].Length);
                    }
                    else if (def[1]!="0")
                    {                                                                                   
                        hasLength = (int.Parse(defPic[0]) >= (def[0].Length + def[1].Length));
                    }
                    else
                    {
                        hasLength = (int.Parse(defPic[0]) >= (def[0].Length));
                    }

                    if (!hasLength)                                                                                                                                                                                   
                    {
                        OutputError err = new OutputError(Utility.ExtractCommentsAndBreakLines(assign.Text) + " Number assigned is too long (" + pictureL + ")", MessageLevel.Warning, new KBObjectPosition(part));
                        output.Add("KBDoctor", err);
                    }

                    if(hasLength && int.Parse(def[1]) != 0 && int.Parse(defPic[1]) < def[1].Length) //Chequeo de decimales
                    {
                        OutputError err = new OutputError(Utility.ExtractCommentsAndBreakLines(assign.Text) + " Number assigned decimals are too long (" + pictureL + ")", MessageLevel.Warning, new KBObjectPosition(part));
                        output.Add("KBDoctor", err);
                    }
                }
                else
                {
                    NumberNode nn = (NumberNode)assign.Right;
                    string text = nn.Text;
                    if(text != "0" && text != "1")
                    {
                        OutputError err = new OutputError(Utility.ExtractCommentsAndBreakLines(assign.Text) + " Number greater than 1 assigned to a boolean", MessageLevel.Warning, new KBObjectPosition(part));
                        output.Add("KBDoctor", err);
                    }
                }
            }
            if (assign.Right is DateConstantNode)
            {
            }
        }



        private static void CheckVarAndAttAssignTypes(IOutputService output, AssignmentNode assign, string pictureL, Domain domL, string pictureR, Domain domR, string objname, KBObjectPart part)
        {
            if (pictureL.ToLower().Contains("char") && pictureR.ToLower().Contains("char"))
            {
                string lengthPicL = getLengthFromPicture(pictureL);
                string lengthPicR = getLengthFromPicture(pictureR);
                if (int.Parse(lengthPicL) < int.Parse(lengthPicR))
                {
                    OutputError err = new OutputError(Utility.ExtractCommentsAndBreakLines(assign.Text) + " String assigned is too long " + pictureL + "<" + pictureR, MessageLevel.Warning, new KBObjectPosition(part));
                    output.Add("KBDoctor", err);
                }
                CheckDomains(output, assign, domL, domR, objname, part);
            }
            else if (pictureL.ToLower().Contains("numeric") && pictureR.ToLower().Contains("numeric"))
            {
                string lengthPicL = getLengthFromPicture(pictureL);
                string lengthPicR = getLengthFromPicture(pictureR);
                string[] splitsL = SplitDecimals(lengthPicL);
                string[] splitsR = SplitDecimals(lengthPicR);
                if ((int.Parse(splitsL[0]) - int.Parse(splitsL[1])) < (int.Parse(splitsR[0]) - int.Parse(splitsR[1])))
                {
                    OutputError err = new OutputError(Utility.ExtractCommentsAndBreakLines(assign.Text) + " Number assigned is too long " + pictureL + "<" + pictureR, MessageLevel.Warning, new KBObjectPosition(part));
                    output.Add("KBDoctor", err);
                }
                else if (int.Parse(splitsL[1]) < int.Parse(splitsR[1]))
                {
                    OutputError err = new OutputError(Utility.ExtractCommentsAndBreakLines(assign.Text) + " Number decimals assigned are too long " + pictureL + "<" + pictureR, MessageLevel.Warning, new KBObjectPosition(part));
                    output.Add("KBDoctor", err);
                }
                CheckDomains(output, assign, domL, domR, objname, part);
                
            }
            else
            {
                if (pictureR != pictureL)
                {
                    OutputError err = new OutputError(Utility.ExtractCommentsAndBreakLines(assign.Text) + " " + pictureL + "<>" + pictureR, MessageLevel.Warning, new KBObjectPosition(part));
                    output.Add("KBDoctor", err);
                }
            }
        }

        private static void CheckDomains(IOutputService output, AssignmentNode assign, Domain domL, Domain domR, string objname, KBObjectPart part)
        {
            if (domL != null)
            {
                if (domR != null)
                {
                    if (domL.Name != domR.Name)
                    {
                        OutputError err = new OutputError(Utility.ExtractCommentsAndBreakLines(assign.Text) + " Variables are based on different domains " + domL.Name + "<>" + domR.Name, MessageLevel.Warning, new KBObjectPosition(part));
                        output.Add("KBDoctor", err);
                    }
                }
                else
                {
                    OutputError err = new OutputError(Utility.ExtractCommentsAndBreakLines(assign.Text) + " (" + assign.Right.Text + ") Doesn't have domain but (" + assign.Left.Text + ") is BasedOn " + domL.Name, MessageLevel.Warning, new KBObjectPosition(part));
                    output.Add("KBDoctor", err);
                }
            }
            else
            {
                if (domR != null)
                {
                    OutputError err = new OutputError(Utility.ExtractCommentsAndBreakLines(assign.Text) + " (" + assign.Left.Text + ") Doesn't have domain but (" + assign.Right.Text + ") is BasedOn " + domR.Name, MessageLevel.Warning, new KBObjectPosition(part));
                    output.Add("KBDoctor", err);
                }
                else
                {
                }
            }
        }

        private static string[] SplitDecimals(string text)
        {
            string[] splits = text.Split('.');
            string length = splits[0];
            if (splits.Length == 2)
            {
                string decimals = splits[1];
                string[] definition = { length, decimals };
                return definition;
            }
            else
            {
                string[] definition = { length , "0"};
                return definition;
            }
        }

        private static string getLengthFromPicture(string picture)
        {
            string[] splits = picture.Split('(');
            splits = splits[1].Split(')');
            return splits[0];
        }

        private static List<AbstractNode> getAssignmentsInSource(Artech.Genexus.Common.AST.AbstractNode root)
        {
            if(root != null) { 
                List<AbstractNode> assignments = new List<AbstractNode>();
                foreach(AbstractNode node in root.Children)
                {
                    if (node.Node != null)
                    {
                        if (node.Node.Token == 107 || node.Node.Token == -1) 
                        {
                            if (node is AssignmentNode)
                                assignments.Add(node);
                        }
                        else
                        {
                            assignments.AddRange(getAssignmentsInSource(node));
                        }
                    }
                }
                return assignments;
            }
            return null; 
        }

        internal static bool ThemeClassesNotUsed(KnowledgeBase KB, IOutputService output, ThemeClass themeclass)
        {

            KBModel model = KB.DesignModel;

            foreach (var entityRef in themeclass.GetReferencesTo())
            {
                entityRef.From.Id.ToString();
                entityRef.To.Id.ToString();
            }
            IEnumerable<EntityReference> references = themeclass.GetReferencesTo();
            if (references.Count<EntityReference>() == 0)
            {
                output.AddLine(themeclass.Name);
            }
            return true;
        }

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
#else
        //Nothing
#endif
    }
}
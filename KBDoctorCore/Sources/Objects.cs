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
using GeneXus.Server.Contracts;

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
using System.Xml;
using Attribute = Artech.Genexus.Common.Objects.Attribute;
using System.IO;

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

        internal static List<KBObject> ParmWOInOut(List<KBObject> objs, IOutputService output, ref string recommendations, out int cant)
        {
            // Object with parm() rule without in: out: or inout:
            List<KBObject> objectsWithProblems = GetObjectsWithProblems(objs, output, ref recommendations);
            bool success = true;
            cant = objectsWithProblems.Count;
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
                        KBDoctorOutput.Message( "Processing " + obj.Name);
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

        internal static void CommitOnExit(List<KBObject> objs, IOutputService output, ref string recommendations, out int cant)
        {
            cant = 0;
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
                            cant++;
                            string recommend = "Commit on EXIT = YES ";
                            OutputError wrn = new OutputError(recommend, MessageLevel.Warning, new KBObjectAnyPosition(obj));
                            recommendations += recommend + "<br>";
                            output.Add("KBDoctor", wrn);
                        }
                    }
                }
            }
        }

        internal static void CheckComplexityMetrics(KBObject obj, IOutputService output, int maxNestLevel, int maxComplexityLevel, int maxCodeBlock, int maxParametersCount, ref string recommendations, out int diffNestLevel, out int diffcomplexityLevel, out int diffCodeBlock, out int diffParametersCount)
        {
            diffNestLevel = 0;
            diffCodeBlock = 0;
            diffcomplexityLevel = 0;
            diffParametersCount = 0;
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
                        diffNestLevel = NestLevel - maxNestLevel;
                        string recommend = "Nested level too high (" + NestLevel.ToString() + "). Recommended max: " + maxNestLevel.ToString();
                        OutputError err = new OutputError(recommend, MessageLevel.Error, new KBObjectPosition(part));
                        recommendations += recommend + "<br>";
                        output.Add("KBDoctor", err);

                    }
                    if (ComplexityLevel > maxComplexityLevel)
                    {
                        diffcomplexityLevel = ComplexityLevel - maxComplexityLevel;
                        string recommend = "Complexity too high(" + ComplexityLevel.ToString() + ").Recommended max: " + maxComplexityLevel.ToString();
                        OutputError err = new OutputError(recommend, MessageLevel.Error, new KBObjectPosition(part));
                        recommendations += recommend + "<br>";
                        output.Add("KBDoctor", err);
                    }

                    if (CodeBlock > maxCodeBlock)
                    {
                        diffCodeBlock = CodeBlock - maxCodeBlock;
                        string recommend = "Code block too large(" + CodeBlock.ToString() + ").Recommended max: " + maxCodeBlock.ToString();
                        OutputError err = new OutputError(recommend, MessageLevel.Error, new KBObjectPosition(part));
                        recommendations += recommend + "<br>";
                        output.Add("KBDoctor", err);
                    }

                    int parametersCount = ParametersCountObject(obj);
                    if (parametersCount > maxParametersCount)
                    {
                        diffParametersCount = parametersCount - maxParametersCount;
                        string recommend = "Too many parameters (" + parametersCount.ToString() + ").Recommended max: " + maxParametersCount.ToString();
                        KBObjectPart rpart = Utility.ObjectRulesPart(obj);
                        OutputError err = new OutputError(recommend, MessageLevel.Error, new KBObjectPosition(rpart));
                        recommendations += recommend + "<br>";
                        output.Add("KBDoctor", err);
                    }
                }
            }
        }

        internal static void isInModule(List<KBObject> objs, IOutputService output, ref string recommendations, out int cant)
        {
            cant = 0;
            foreach (KBObject obj in objs)
            {
                if (obj.Module.Description == "Root Module" && !Utility.IsMain(obj))
                {
                    cant++;
                    string recommend = "Object without module.";
                    OutputError err = new OutputError(recommend, MessageLevel.Warning, new KBObjectAnyPosition(obj));
                    recommendations += recommend + "<br>";
                    output.Add("KBDoctor", err);
                }
            }
        }

        internal static void ObjectsWithVarNotBasedOnAtt(List<KBObject> objs, IOutputService output, bool fixvar, ref string recommendations, out int cant)
        {
            cant = 0;
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
                                    if (fixvar) { 
                                        if (FixObjectVariable(v, ref recommendations, output))
                                            SaveObj = true;
                                    }
                                    else
                                    {
                                        cant++;
                                    }
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

        private static bool FixObjectVariable(Variable v, ref string recommendations, IOutputService output)
        {
            KBObject obj = v.KBObject;
            KBModel kBModel = obj.Model;
            foreach (Artech.Genexus.Common.Objects.Attribute att in kBModel.Objects.GetByName("Attributes", new Guid("adbb33c9-0906-4971-833c-998de27e0676"), v.Name))
            {
                if(att.Type == v.Type)
                {
                    string recommend = "Attribute " + att.Name + " assigned to variable " + v.Name;
                    recommendations += recommend + "<br>";
                    OutputError err = new OutputError(recommend, MessageLevel.Warning, new KBObjectAnyPosition(obj));
                    output.Add("KBDoctor", err);
                    v.AttributeBasedOn = att;
                    return true;
                    break;
                }
                else
                {
                    string recommend = "The variable " + v.Name + " has the same name as an attribute but is based on a different type.";
                    recommendations += recommend + "<br>";
                    OutputError err = new OutputError(recommend, MessageLevel.Warning, new KBObjectAnyPosition(obj));
                    output.Add("KBDoctor", err);
                }
            }

            //Si no asigno atributo intento asignar dominio
            if (v.AttributeBasedOn == null)
            {
                foreach (Domain d in kBModel.Objects.GetByName("Domains", new Guid("00972a17-9975-449e-aab1-d26165d51393"), v.Name))
                {
                    if (d.Type == v.Type)
                    {
                        string recommend = "Domain " + d.Name + " assigned to variable " + v.Name;
                        recommendations += recommend + "<br>";
                        OutputError err = new OutputError(recommend, MessageLevel.Warning, new KBObjectAnyPosition(obj));
                        output.Add("KBDoctor", err);
                        v.DomainBasedOn = d;
                        return true;
                        break;
                    }
                    else
                    {
                        string recommend = "The variable " + v.Name + " has the same name as a domain but is based on a different type.";
                        recommendations += recommend + "<br>";
                        OutputError err = new OutputError(recommend, MessageLevel.Warning, new KBObjectAnyPosition(obj));
                        output.Add("KBDoctor", err);
                    }
                }
            }
            return false;

        }

        internal static void CodeCommented(List<KBObject> objs, IOutputService output, ref string recommendations, out int cant)
        {
            cant = 0;
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
                    cant++;
                    output.Add("KBDoctor", err);
                }
            }
        }

        internal static void AttributeHasDomain(List<KBObject> objs, IOutputService output, ref string recommendations, out int cant)
        {
            cant = 0;
            foreach (KBObject obj in objs)
            {
                if (obj is Artech.Genexus.Common.Objects.Attribute)
                {
                    Artech.Genexus.Common.Objects.Attribute a = (Artech.Genexus.Common.Objects.Attribute)obj;
                    string FormatType = Utility.FormattedTypeAttribute(a);
                    bool isSubtype = Utility.AttIsSubtype(a);

                    if ((a.DomainBasedOn == null) && !isSubtype && Utility.AttHasToBeInDomain(a))
                    {
                        cant++;
                        string recommend = "Attribute without domain: " + a.Name;
                        OutputError err = new OutputError(recommend, MessageLevel.Error, new KBObjectAnyPosition(obj));
                        recommendations += recommend + "<br>";
                        output.Add("KBDoctor", err);
                    }
                }

            }
        }

        internal static void SDTBasedOnAttDomain(List<KBObject> objs, IOutputService output, ref string recommendations, out int cant)
        {
            cant = 0;
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
                        cant++;
                        string recommend = "SDT with items without domain: " + itemnames;
                        OutputError err = new OutputError(recommend, MessageLevel.Warning, new KBObjectAnyPosition(obj));
                        recommendations += recommend + "<br>";
                        output.Add("KBDoctor", err);
                    }
                }

            }
        }
        
        internal static List<KBObject> GetAttributesFromTrn(Transaction trn)
        {
            List<KBObject> atts = new List<KBObject>();
            foreach(KBObject att in trn.GetAttributes())
            {
                if(att is Artech.Genexus.Common.Objects.Attribute)
                {
                    atts.Add((Artech.Genexus.Common.Objects.Attribute)att);
                }
            }
            return atts;
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
                    KBDoctorOutput.Message( obj.Name);
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

        internal static void ObjectsWithRuleOld(KBModel model, KBObject obj, ref string recommendations, out int cant)
        {
            cant = 0;
            if (!isGeneratedbyPattern(obj))
            {
                if (obj is Transaction || obj is Procedure || obj is WebPanel)
                {
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    RulesPart rules = obj.Parts.Get<RulesPart>();
                    RuleOldInSource(model, rules, vp, ref recommendations, out cant);
                }
            }
        }

        internal static bool RuleOldInSource(KBModel model, SourcePart source, VariablesPart vp, ref string recommendations, out int cant)
        {
            cant = 0;

            var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
            ParserInfo parserInfo;
            parserInfo = new ParserInfo(source);
            var info = new Artech.Architecture.Language.Parser.ParserInfo(source);

            if (parser.Validate(info, source.Source))
            {
                AbstractNode paramRootNode = ASTNodeFactory.Create(parser.Structure, source, vp, info);
                List<AbstractNode> calls = getOldsInSource(paramRootNode);
                foreach(AbstractNode an in calls)
                {
                    AbstractNode test = an.Children.First();
                    if (test is AttributeNameNode)
                    {
                        if (test.Node.Tag is Artech.Genexus.Common.Objects.Attribute)
                        {
                            Artech.Genexus.Common.Objects.Attribute att = (Artech.Genexus.Common.Objects.Attribute)test.Node.Tag;
                            if(att.Type == eDBType.DATE || att.Type == eDBType.DATETIME)
                            {
                                OutputMsgAssignComparer(an, source, "Rule old with date attribute in transaction", ref recommendations);
                            }
                        }
                    }
                }
            }
                return true;
        }

        internal static void VariablesNotBasedOnAttributes(KBModel designModel, KBObject obj, out List<string[]> lineswriter, out int cant_aux)
        {
            lineswriter = new List<string[]>();
            cant_aux = 0;
            VariablesPart vp = obj.Parts.Get<VariablesPart>();
            foreach(Variable v in vp.Variables)
            {
                if (!v.IsStandard) { 
                    foreach (KBObject aux in designModel.Objects.GetByPropertyValue("Name", v.Name))
                    {
                        if(aux is Attribute)
                        {
                            if(v.AttributeBasedOn == null)
                            {
                                Attribute att = (Attribute)aux;
                                string att_domain = (att.DomainBasedOn == null) ? "(No Domain)" : att.DomainBasedOn.Name;
                                string var_domain = (v.DomainBasedOn == null) ? "(No Domain)" : v.DomainBasedOn.Name;
                                string var_formtype = Utility.FormattedTypeVariable(v);
                                string att_formtype = Utility.FormattedTypeAttribute(att);
                                if (v.DomainBasedOn != null && att.DomainBasedOn != null)
                                {
                                    if(v.DomainBasedOn.QualifiedName.ToString().ToLower() != att.DomainBasedOn.QualifiedName.ToString().ToLower())
                                    {
                                        if(var_formtype != att_formtype)
                                        {
                                            string[] line = new string[] { Utility.linkObject(obj), v.Name, var_formtype, att_formtype, var_domain, att_domain };
                                            lineswriter.Add(line);
                                            string msgOutput = "Variable Name: " + v.Name + " Domains(var/att): " + var_domain + "/" + att_domain + " Types(var / att): " + var_formtype + "/" + att_formtype;
                                            OutputError error = new OutputError(msgOutput, MessageLevel.Warning, new SourcePosition(vp, 1, 1));
                                            KBDoctorOutput.OutputError(error);
                                            cant_aux += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    if (var_formtype != att_formtype)
                                    {
                                        string[] line = new string[] { Utility.linkObject(obj), v.Name, var_formtype, att_formtype, var_domain, att_domain };
                                        lineswriter.Add(line);
                                        string msgOutput = "Variable Name: " + v.Name + " Domains(var/att): " + var_domain + "/" + att_domain + " Types(var / att): " + var_formtype + "/" + att_formtype;
                                        OutputError error = new OutputError(msgOutput, MessageLevel.Warning, new SourcePosition(vp, 1, 1));
                                        KBDoctorOutput.OutputError(error);
                                        cant_aux += 1;
                                    }
                                }
                            }                                
                        }
                    }
                }
            }
        }

        internal static void CheckVariableUsages(KBModel model, KBObject obj, ref string recommendations, out int cant)
        {
            cant = 0;
            int cant_aux;
            if (!isGeneratedbyPattern(obj))
            {
                HashSet<string> readonly_var, writeonly_var;
                VariablesPart vp = obj.Parts.Get<VariablesPart>();
                LoadVariablesInObject(vp, out readonly_var, out writeonly_var);
                if (obj is Procedure)
                {
                    ProcedurePart procpart = obj.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
                    RulesPart rules = obj.Parts.Get<RulesPart>();
                    if (procpart != null)
                    {
                        ProcessAssignsInSource(model, procpart, vp, ref recommendations, out cant_aux, ref readonly_var, ref writeonly_var);
                        cant += cant_aux;
                    }
                    if (rules != null)
                    {
                        ProcessAssignsInSource(model, rules, vp, ref recommendations, out cant_aux, ref readonly_var, ref writeonly_var);
                        cant += cant_aux;
                    }
                }
                else if (obj is WebPanel || obj is Transaction)
                {
                    EventsPart eventspart = obj.Parts.Get<EventsPart>();
                    RulesPart rules = obj.Parts.Get<RulesPart>();
                    if (eventspart != null)
                    {
                        ProcessAssignsInSource(model, eventspart, vp, ref recommendations, out cant_aux, ref readonly_var, ref writeonly_var);
                        cant += cant_aux;
                    }
                    if (rules != null)
                    {
                        ProcessAssignsInSource(model, rules, vp, ref recommendations, out cant_aux, ref readonly_var, ref writeonly_var);
                        cant += cant_aux;
                    }
                }
                ShowVariableUsageResults(readonly_var, writeonly_var);
            }
        }

        private static void ProcessAssignsInSource(KBModel model, SourcePart source, VariablesPart vp, ref string recommendations, out int cant, ref HashSet<string> readonly_var, ref HashSet<string> writeonly_var)
        {
            int cant_aux;
            cant = 0;
            var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
            ParserInfo parserInfo;
            parserInfo = new ParserInfo(source);
            var info = new Artech.Architecture.Language.Parser.ParserInfo(source);


            if (parser.Validate(info, source.Source))
            {
                AbstractNode paramRootNode = ASTNodeFactory.Create(parser.Structure, source, vp, info);
                List<AbstractNode> assigns = getVariablesUsages(paramRootNode);
                foreach (AbstractNode assign in assigns)
                {
                    if (assign is AssignmentNode)
                    {
                        CheckAssignVariableUsages(assign, ref readonly_var, ref writeonly_var);


                    }
                    if (assign is ObjectMethodNode)
                    {
                        ObjectMethodNode omn = (ObjectMethodNode)assign;

                        switch (omn.MethodName.ToLower())
                        {
                            case "tostring":
                                RemoveReadOnlyVar(ref writeonly_var, omn);
                                break;
                            case "fromstring":
                                RemoveWriteOnlyVar(ref readonly_var, omn);
                                break;
                            case "setempty":
                                RemoveWriteOnlyVar(ref readonly_var, omn);
                                break;
                            case "integer":
                                RemoveReadOnlyVar(ref writeonly_var, omn);
                                break;
                            case "isempty":
                                RemoveReadOnlyVar(ref writeonly_var, omn);
                                break;
                            case "round":
                                RemoveReadOnlyVar(ref writeonly_var, omn);
                                break;
                            case "roundtoeven":
                                RemoveReadOnlyVar(ref writeonly_var, omn);
                                break;
                            case "toformattedstring":
                                RemoveReadOnlyVar(ref writeonly_var, omn);
                                break;
                            case "truncate":
                                RemoveReadOnlyVar(ref writeonly_var, omn);
                                break;
                        }
                    }
                }

            }
        }

        private static void ShowVariableUsageResults(HashSet<string> readonly_var, HashSet<string> writeonly_var)
        {
            List<string> aux_list = new List<string>();
            aux_list.AddRange(readonly_var);

            foreach (string varname in aux_list)
            {
                if (writeonly_var.Contains(varname) && readonly_var.Contains(varname))
                {
                    writeonly_var.Remove(varname);
                    readonly_var.Remove(varname);
                    KBDoctorOutput.Message("NotUsed> " + varname);
                }
            }
            foreach (string varname in readonly_var)
            {
                KBDoctorOutput.Message("Readonly> " + varname);
            }
            foreach (string varname in writeonly_var)
            {
                KBDoctorOutput.Message("Write> " + varname);
            }
        }

        private static void LoadVariablesInObject(VariablesPart vp, out HashSet<string> readonly_var, out HashSet<string> writeonly_var)
        {
            readonly_var = new HashSet<string>();
            writeonly_var = new HashSet<string>();
            foreach (Variable v in vp.Variables)
            {
                if (!v.IsStandard)
                {
                    readonly_var.Add(v.Name.ToLower());
                    writeonly_var.Add(v.Name.ToLower());
                }
            }
        }

        private static void CheckAssignVariableUsages(AbstractNode assign, ref HashSet<string> readonly_var, ref HashSet<string> writeonly_var)
        {
            AssignmentNode an = (AssignmentNode)assign;
            if (an.Left is VariableNameNode)
            {
                VariableNameNode vnn = (VariableNameNode)an.Left;
                RemoveWriteOnlyVar(ref readonly_var, vnn);
            }
            if (((AssignmentNode)assign).Right is ObjectMethodNode)
            {
                ObjectMethodNode omn = (ObjectMethodNode) ((AssignmentNode)assign).Right;
                switch (omn.MethodName)
                {
                    case "tostring":
                        RemoveReadOnlyVar(ref writeonly_var, omn);
                        break;
                    case "fromstring":
                        RemoveWriteOnlyVar(ref readonly_var, omn);
                        break;
                    case "setempty":
                        RemoveWriteOnlyVar(ref readonly_var, omn);
                        break;
                    case "integer":
                        RemoveReadOnlyVar(ref writeonly_var, omn);
                        break;
                    case "isempty":
                        RemoveReadOnlyVar(ref writeonly_var, omn);
                        break;
                    case "round":
                        RemoveReadOnlyVar(ref writeonly_var, omn);
                        break;
                    case "roundtoeven":
                        RemoveReadOnlyVar(ref writeonly_var, omn);
                        break;
                    case "toformattedstring":
                        RemoveReadOnlyVar(ref writeonly_var, omn);
                        break;
                    case "truncate":
                        RemoveReadOnlyVar(ref writeonly_var, omn);
                        break;

                }
                if (omn.Children.First() is VariableNameNode)
                {
                    VariableNameNode vnn = (VariableNameNode)omn.Children.First();

                    RemoveReadOnlyVar(ref writeonly_var, vnn);


                    /*
                    if(readonly_var.Contains(varname))
                    {
                        readonly_var.Remove(varname);
                    }
                    if (writeonly_var.Contains(varname))
                    {
                        writeonly_var.Remove(varname);
                    }*/
                }
            }
        }

        private static void RemoveWriteOnlyVar(ref HashSet<string> writeonly_var, ObjectMethodNode omn)
        {
            if (omn.Children.First() is VariableNameNode)
            {
                VariableNameNode vnn = (VariableNameNode)omn.Children.First();
                if (writeonly_var.Contains(vnn.VarName))
                {
                    writeonly_var.Remove(vnn.VarName);
                }
            }
        }

        private static void RemoveWriteOnlyVar(ref HashSet<string> writeonly_var, VariableNameNode vnn)
        {
            if (writeonly_var.Contains(vnn.VarName.ToLower()))
                writeonly_var.Remove(vnn.VarName.ToLower());         
        }

        private static void RemoveReadOnlyVar(ref HashSet<string> readonly_var, ObjectMethodNode omn)
        {
            if (omn.Children.First() is VariableNameNode)
            {
                VariableNameNode vnn = (VariableNameNode)omn.Children.First();
                if (readonly_var.Contains(vnn.VarName.ToLower()))
                {
                    readonly_var.Remove(vnn.VarName.ToLower());
                }
            }
        }

        private static void RemoveReadOnlyVar(ref HashSet<string> readonly_var, VariableNameNode vnn)
        {
            if (readonly_var.Contains(vnn.VarName))
            {
                readonly_var.Remove(vnn.VarName);
            }
        }

        private static List<AbstractNode> getVariablesUsages(Artech.Genexus.Common.AST.AbstractNode root)
        {

            if (root != null)
            {
                List<AbstractNode> assigns = new List<AbstractNode>();
                foreach (AbstractNode node in root.Children)
                {
                    if (node.Node != null)
                    {
                        if (node.Node.Token == 107)
                        {
                            if (node is AssignmentNode)
                            { 
                                assigns.Add(node);
                                assigns.AddRange(getVariablesUsages(node));
                            }
                        }
                        else if (node.Node.Token == 104)
                        {
                            if (node is AssignmentNode)
                            { 
                                assigns.Add(node);
                                assigns.AddRange(getVariablesUsages(node));
                            }
                            if (node is FunctionNode)
                            { 
                                assigns.Add(node);
                                assigns.AddRange(getVariablesUsages(node));
                            }
                        }
                        else if (node.Node.Token == 158)
                        {
                            if (node is AssignmentNode)
                            { 
                                assigns.Add(node);
                                assigns.AddRange(getVariablesUsages(node));
                            }
                            if (node is FunctionNode)
                            { 
                                assigns.Add(node);
                                assigns.AddRange(getVariablesUsages(node));
                            }
                        }
                        else if (node is ObjectMethodNode)
                        {
                            ObjectMethodNode omn = (ObjectMethodNode)node;
                            switch (omn.MethodName.ToLower())
                            {
                                case "tostring":
                                    assigns.Add(node);
                                    break;
                                case "fromstring":
                                    assigns.Add(node);
                                    break;
                                case "setempty":
                                    assigns.Add(node);
                                    break;
                                case "integer":
                                    assigns.Add(node);
                                    break;
                                case "isempty":
                                    assigns.Add(node);
                                    break;
                                case "round":
                                    assigns.Add(node);
                                    break;
                                case "roundtoeven":
                                    assigns.Add(node);
                                    break;
                                case "toformattedstring":
                                    assigns.Add(node);
                                    break;
                                case "truncate":
                                    assigns.Add(node);
                                    break;
                            }
                        }
                        else
                        { 
                            assigns.AddRange(getVariablesUsages(node));
                        }
                    }
                }
                return assigns;
            }
            return null;
        }
        
        internal static void ParameterTypeComparer(KBModel model, KBObject obj, ref string recommendations, out int cant)
        {
            cant = 0;
            int cant_aux;
            if (!isGeneratedbyPattern(obj))
            {
                if (obj is Procedure)
                {
                    ProcedurePart procpart = obj.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    RulesPart rules = obj.Parts.Get<RulesPart>();
                    if (procpart != null)
                    {
                        ProcessCallsInSource(model,procpart, vp, ref recommendations, out cant_aux);
                        cant += cant_aux;
                    }
                    if (rules != null)
                    {
                        ProcessCallsInSource(model, rules, vp, ref recommendations, out cant_aux);
                        cant += cant_aux;
                    }
                }else if(obj is WebPanel || obj is Transaction)
                {
                    EventsPart eventspart = obj.Parts.Get<EventsPart>();
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    RulesPart rules = obj.Parts.Get<RulesPart>();
                    if (eventspart != null)
                    {
                        ProcessCallsInSource(model, eventspart, vp, ref recommendations, out cant_aux);
                        cant += cant_aux;
                    }
                    if (rules != null)
                    {
                        ProcessCallsInSource(model, rules, vp, ref recommendations, out cant_aux);
                        cant += cant_aux;
                    }
                } 
            }
        }

        private static void ProcessCallsInSource(KBModel model, SourcePart source, VariablesPart vp, ref string recommendations, out int cant)
        {
            int cant_aux;
            cant = 0;
            var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
            ParserInfo parserInfo;
            parserInfo = new ParserInfo(source);
            var info = new Artech.Architecture.Language.Parser.ParserInfo(source);

            if (parser.Validate(info, source.Source))
            {
                AbstractNode paramRootNode = ASTNodeFactory.Create(parser.Structure, source, vp, info);
                List<AbstractNode> calls = getCallsInSource(paramRootNode);
                foreach (AbstractNode call in calls)
                {
                    if(call is AssignmentNode)
                    {
                        if(((AssignmentNode)call).Right is FunctionNode)
                        {
                            FunctionNode fn = ((FunctionNode)((AssignmentNode)call).Right);
                            if(fn.FunctionName.ToLower() == "udp" || fn.FunctionName.ToLower() == "create")
                            {
                                List<AbstractNode> param = new List<AbstractNode>();
                                foreach (AbstractNode an in fn.Parameters.Skip(1))
                                {
                                    param.Add(an);
                                }
                                if(!(call.Children.First() is UnknownNode))
                                {
                                    param.Add(call.Children.First());
                                }
                                CheckParameterTypeFunctionNode(model, call, fn, param, source, vp, ref recommendations, out cant_aux);
                                cant += cant_aux;
                            }
                            else
                            {
                                List<AbstractNode> param = new List<AbstractNode>();
                                foreach (AbstractNode an in fn.Children)
                                {
                                    param.Add(an);
                                }
                                param.Add(((AssignmentNode)call).Left);
                                CheckParameterTypeExplicitCall(model, call, fn, param, source, vp, ref recommendations, out cant_aux);
                                cant += cant_aux;
                            }
                        }
                        else if(((AssignmentNode)call).Right is ObjectMethodNode)
                        {
                            ObjectMethodNode omn = (ObjectMethodNode)((AssignmentNode)call).Right;
                            if (omn.MethodName.ToLower() == "call" || omn.MethodName.ToLower() == "udp" || omn.MethodName.ToLower() == "submit" || omn.MethodName.ToLower() == "create")
                            {
                                List<AbstractNode> param = new List<AbstractNode>();
                                foreach (AbstractNode an in omn.Function.Children)
                                {
                                    param.Add(an);
                                }
                                if(!(call.Children.First() is UnknownNode))
                                {
                                    param.Add(call.Children.First());
                                }
                                CheckParameterTypeObjectMethod(model, call, omn, param, source, vp, ref recommendations, out cant_aux);
                                cant += cant_aux;
                            }
                        }       
                    }
                    else if(call is FunctionNode)
                    {
                        FunctionNode fn = (FunctionNode)call;
                        if (fn.FunctionName.ToLower() == "call" || fn.FunctionName.ToLower() == "submit")
                        {
                            List<AbstractNode> param = new List<AbstractNode>();
                            foreach (AbstractNode an in fn.Parameters.Skip(1))
                            {
                                param.Add(an);
                            }
                            CheckParameterTypeFunctionNode(model, call, fn, param, source, vp, ref recommendations, out cant_aux);
                            cant += cant_aux;
                        }
                        else
                        {
                            List<AbstractNode> param = new List<AbstractNode>();
                            foreach (AbstractNode an in fn.Children)
                            {
                                param.Add(an);
                            }
                            CheckParameterTypeExplicitCall(model, call, fn, param, source, vp, ref recommendations, out cant_aux);
                            cant += cant_aux;
                        }
                    }
                }
            }
        }

        private static void CheckParameterTypeExplicitCall(KBModel model, AbstractNode call, FunctionNode fn, List<AbstractNode> parms, KBObjectPart part, VariablesPart vp, ref string recommendations, out int cant)
        {
            cant = 0;
            if (fn.Element != null)
            {
                KBObject obj = (KBObject)fn.Element.Name.Tag;
                if (obj != null)
                {
                    CheckObjectCallsParameters(model, call, parms, obj, part, vp, ref recommendations, out cant);
                }
            }
        }

        private static void CheckParameterTypeFunctionNode(KBModel model, AbstractNode call, FunctionNode fn, List<AbstractNode> parms, KBObjectPart part, VariablesPart vp, ref string recommendations, out int cant)
        {
            cant = 0;
            if (fn.Children.First() is AttributeNameNode)
            {
                AttributeNameNode ann = (AttributeNameNode)fn.Children.First();
                if (ann != null)
                {
                    KBObject obj = (KBObject)ann.Element.Tag;
                    if (obj != null)
                    {
                        CheckObjectCallsParameters(model, call, parms, obj, part, vp, ref recommendations, out cant);
                    }
                }
            }
        }

        private static void CheckParameterTypeObjectMethod(KBModel model, AbstractNode call, ObjectMethodNode omn, List<AbstractNode> parms, KBObjectPart part, VariablesPart vp, ref string recommendations, out int cant)
        {
            cant = 0;
            if (omn.Target is AttributeNameNode)
            {
                AttributeNameNode ann = (AttributeNameNode)omn.Target;
                if (ann != null)
                {
                    if (ann.Element != null)
                    {
                        KBObject obj = (KBObject)ann.Element.Tag;
                        if (obj != null)
                        {
                            CheckObjectCallsParameters(model, call, parms, obj, part, vp, ref recommendations, out cant);
                        }
                    }
                }
            }
        }

        private static void CheckObjectCallsParameters(KBModel model, AbstractNode call, List<AbstractNode> parms, KBObject obj, KBObjectPart part, VariablesPart vp, ref string recommendations, out int cant)
        {
            int cant_aux;
            cant = 0;
            int cnt = parms.Count;
            List<Tuple<string, string>> types_accessors = Utility.GetParametersFormatedType(obj);
            List<Tuple<Domain, string>> domain_accessors = Utility.GetParametersDomains(obj);
            int i = 0;
            foreach (AbstractNode parm in parms)
            {
                if (types_accessors != null && i < types_accessors.Count)
                {
                    
                    Tuple<string, string> parm_types_acc = types_accessors[i];
                    Tuple<Domain, string> parm_domain_acc = domain_accessors[i];
                    if (parm_types_acc != null && parm_domain_acc != null )
                    {
                        Tuple<Domain, string, int> call_type_domain = GetParameterTypeDomain(model, parm, vp);


                        if (call_type_domain != null)
                        {
                            if(call_type_domain.Item3 != 0) // Is not constant
                            {
                                if(call_type_domain.Item3 == 1) //String 
                                {
                                    string extra_text = " -- Parameter: (" + parm.Text + ") ";
                                    CheckAssignTypesStringConstant(call, parm_types_acc.Item1, part, int.Parse(call_type_domain.Item2), extra_text, ref recommendations, out cant_aux);
                                    cant += cant_aux;
                                }
                                if(call_type_domain.Item3 == 2) //Numeric
                                {
                                    string extra_text = " -- Parameter: (" + parm.Text + ") ";
                                    string[] definitionR = SplitDecimals(call_type_domain.Item2);
                                    string lengthFormatTypeL = getLengthFromFormattedType(parm_types_acc.Item1);
                                    string[] definitionL = SplitDecimals(lengthFormatTypeL);
                                    CheckAssignTypesNumericConstant(call, parm_types_acc.Item1, part, definitionR, definitionL, extra_text, ref recommendations, out cant_aux);
                                    cant += cant_aux;
                                }
                            }
                            else
                            {
                                if (parm_domain_acc.Item2 == "PARM_IN")
                                {
                                    string extra_text = " -- Parameter (IN): (" + parm.Text + ") ";
                                    CompareTypes(call, parm_types_acc.Item1, parm_domain_acc.Item1, call_type_domain.Item2, call_type_domain.Item1, vp.KBObject.Name, part, extra_text, ref recommendations, out cant_aux);
                                    cant += cant_aux;
                                }
                                if (parm_domain_acc.Item2 == "PARM_OUT")
                                {
                                    string extra_text = " -- Parameter (OUT): (" + parm.Text + ") ";
                                    CompareTypes(call, call_type_domain.Item2, call_type_domain.Item1, parm_types_acc.Item1, parm_domain_acc.Item1, vp.KBObject.Name, part, extra_text, ref recommendations, out cant_aux);
                                    cant += cant_aux;
                                }
                                if (parm_domain_acc.Item2 == "PARM_INOUT")
                                {
                                    string extra_text = " -- Parameter (INOUT): (" + parm.Text + ") ";
                                    CompareTypes(call, call_type_domain.Item2, call_type_domain.Item1, parm_types_acc.Item1, parm_domain_acc.Item1, vp.KBObject.Name, part, extra_text, ref recommendations, out cant_aux);
                                    cant += cant_aux;
                                    CompareTypes(call, parm_types_acc.Item1, parm_domain_acc.Item1, call_type_domain.Item2, call_type_domain.Item1, vp.KBObject.Name, part, extra_text, ref recommendations, out cant_aux);
                                    cant += cant_aux;
                                }
                            }
                            
                        }
                     /*   else
                        {
                            if (parm_domain_acc.Item2 == "PARM_IN")
                            {
                                CompareAssignTypes(call, parm_types_acc.Item1, parm_domain_acc.Item1, "", null, vp.KBObject.Name, part);
                            }
                            if (parm_domain_acc.Item2 == "PARM_OUT")
                            {
                                CompareAssignTypes(call, "", null, parm_types_acc.Item1, parm_domain_acc.Item1, vp.KBObject.Name, part);
                            }
                        }*/
                    }
                    i++;
                }
            }
        }

        private static Tuple<Domain, string, int> GetParameterTypeDomain(KBModel model, AbstractNode an, VariablesPart vp)
        {
            if(an is VariableNameNode)
            {
                VariableNameNode vnn = (VariableNameNode)an;
                Variable var = vp.GetVariable(vnn.VarName);
                string formatType = Utility.FormattedTypeVariable(var);
                Domain dom = var.DomainBasedOn;
                return new Tuple<Domain, string, int>(dom, formatType, 0);
            }
            else if(an is StringConstantNode)
            {
                StringConstantNode scn = (StringConstantNode)an;
                string text = scn.Text;
                int textlength = text.Length - 2;   //Chequeo logitud ignorando las 2 comillas
                return new Tuple<Domain, string, int>(null, textlength.ToString().Trim(), 1);
            }
            else if (an is NumberNode)
            {
                NumberNode nn = (NumberNode)an;
                string text = nn.Text;
                string[] definitionR = SplitDecimals(text);
                return new Tuple<Domain, string, int>(null, text, 2);
            }
            else if (an is AttributeNameNode)
            {
                AttributeNameNode ann = (AttributeNameNode)an;
                Artech.Genexus.Common.Objects.Attribute att = ann.Attribute;
                if (att != null)
                {
                    string formatType = Utility.FormattedTypeAttribute(att);
                    Domain dom = att.DomainBasedOn;
                    return new Tuple<Domain, string, int>(dom, formatType, 0);
                }
            }
            else if(an is ObjectPropertyNode)
            {
                Tuple<Domain, string, int> ret = GetTypeDomainSDTNode(model, (ObjectPropertyNode)an);
                if(ret == null)
                {
                    if(((ObjectPropertyNode)an).Target is AttributeNameNode)
                    {
                        AttributeNameNode ann = (AttributeNameNode)((ObjectPropertyNode)an).Target;
                        if(ann.Node.Code.ToString().ToLower() ==  "domain")
                        {
                            string domain_name = ann.Text;
                            Domain d = Utility.DomainByName(model, domain_name);
                            string formatType = Utility.FormattedTypeDomain(d);
                            ret = new Tuple<Domain, string, int>(d, formatType, 0);
                        }
                    }
                }
                return ret;
            }
            else
            {
                if(an is FunctionNode)
                {
                    //((FunctionNode)an).Node.Data..Code //val(asdfas,.. )
                }
                if(an is ArithmeticOperationNode) //"asasdf" + "asdfa" 
                {

                }
                if (an is ObjectMethodNode) //.ToString(), Trim(), etc.
                {

                }
                return null;
            }
            return null;
        }

        private static Tuple<Domain, string, int> GetTypeDomainSDTNode(KBModel model, ObjectPropertyNode op)
        {
            AttributeTree.Dependency.Types type;
            string ChildText = op.Children.Skip(1).First().Text;
            StructureTypeReference parentRef = AttributeTree.GetStructureTypeReference(op.Children.First(), model, out type);
            foreach (Artech.Common.Helpers.Structure.IStructureItem item in AttributeTree.GetStructureSubStructures(parentRef, model))
            {
                if (item.Name == ChildText)
                {
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
                        string formatTypeL;
                        if (domL != null)
                            formatTypeL = Utility.FormattedTypeDomain(domL);
                        else
                            formatTypeL = Utility.ReturnFormattedType(sdtitem.Type, sdtitem.Length, sdtitem.Decimals, sdtitem.Signed);
                        return new Tuple<Domain, string, int>(domL, formatTypeL, 0);
                    }
                }
            }
            return null;
        }

        private static List<AbstractNode> getOldsInSource(Artech.Genexus.Common.AST.AbstractNode root)
        {
            if (root != null)
            {
                List<AbstractNode> olds = new List<AbstractNode>();
                if(root is RuleNode && ((RuleNode)root).Conditions.Count > 0)
                {
                    foreach(AbstractNode an in ((RuleNode)root).Conditions)
                    {
                        olds.AddRange(getOldsInSource(an)); 
                    }
                }
                foreach (AbstractNode node in root.Children)
                {
                    if (node.Node != null)
                    {
                        if (node is FunctionNode)
                        {
                            if (((FunctionNode)node).FunctionName.ToLower() == "old")
                            {
                                olds.Add(node);
                            }
                        }
                        else
                        {
                            olds.AddRange(getOldsInSource(node));
                        }
                    }
                }
                return olds;
            }
            return null;
        }

        private static List<AbstractNode> getCallsInSource(Artech.Genexus.Common.AST.AbstractNode root)
        {
            if (root != null)
            {
                List<AbstractNode> calls = new List<AbstractNode>();
                foreach (AbstractNode node in root.Children)
                {
                    if (node.Node != null)
                    { 
                        if (node.Node.Token == 107)
                        {
                            if(node is AssignmentNode && ((AssignmentNode)node).Right is FunctionNode)
                                calls.Add(node);
                            if (node is AssignmentNode && ((AssignmentNode)node).Right is ObjectMethodNode)
                                calls.Add(node);
                        }
                        else if(node.Node.Token == 104)
                        {
                            if (node is AssignmentNode && ((AssignmentNode)node).Right is ObjectMethodNode)
                                calls.Add(node);
                            if (node is FunctionNode)
                                calls.Add(node);
                        }
                        else if (node.Node.Token == 158)
                        {
                            if (node is AssignmentNode && ((AssignmentNode)node).Right is ObjectMethodNode)
                                calls.Add(node);
                            if (node is FunctionNode)
                                calls.Add(node);
                        }
                        else
                        {
                            calls.AddRange(getCallsInSource(node));
                        }
                    }
                }
                return calls;
            }
            return null;
        }

        internal static void ProcessIfElseInSource(KBModel model, SourcePart source, VariablesPart vp, ref string recommendations, out int cant)
        {
            cant = 0;
            var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
            ParserInfo parserInfo;
            parserInfo = new ParserInfo(source);
            var info = new Artech.Architecture.Language.Parser.ParserInfo(source);

            if (parser.Validate(info, source.Source))
            {
                Artech.Genexus.Common.AST.AbstractNode paramRootNode = Artech.Genexus.Common.AST.ASTNodeFactory.Create(parser.Structure, source, vp, info);
                List<AbstractNode> conditions = getIfElseInSource(paramRootNode);

                foreach (CommandBlockNode cond in conditions)
                {
                    if(cond.Children.Count() == 0)
                    {
                        string msgOutput = "This conditional block has no code. Line " + cond.Node.Row.ToString();
                        recommendations += msgOutput + "<br>";
                        OutputError error = new OutputError(msgOutput, MessageLevel.Warning, new SourcePosition(source, cond.Node.Row, 0));
                        cant++;
                        KBDoctorOutput.OutputError(error);
                    }
                    else if(cond.Children.Count() == 1)
                    {
                        if (cond.Children.First() is CommandBlockNode && cond.Children.First().Node.Token == 110) //else
                        {
                            string msgOutput = "This conditional block has no code. Line " + cond.Node.Row.ToString();
                            recommendations += msgOutput + "<br>";
                            OutputError error = new OutputError(msgOutput, MessageLevel.Warning, new SourcePosition(source, cond.Node.Row, 0));
                            cant++;
                            KBDoctorOutput.OutputError(error);
                        }
                    }
                }
            }
        }

        internal static void ForEachsWithoutWhenNone(KBModel model, KBObject obj)
        {
            if (!isGeneratedbyPattern(obj))
            {
                if (obj is Procedure)
                {
                    ProcedurePart procpart = obj.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    if (procpart != null)
                        ProcessForeachsInSource(model, procpart, vp);
                }
                else
                {
                    if (obj is WebPanel || obj is Transaction)
                    {
                        EventsPart eventspart = obj.Parts.Get<Artech.Genexus.Common.Parts.EventsPart>();
                        VariablesPart vp = obj.Parts.Get<VariablesPart>();
                        if (eventspart != null)
                            ProcessForeachsInSource(model, eventspart, vp);
                    }
                }
            }
        }

        internal static void ProcessForeachsInSource(KBModel model, SourcePart source, VariablesPart vp)
        {
            var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
            ParserInfo parserInfo;
            parserInfo = new ParserInfo(source);
            var info = new Artech.Architecture.Language.Parser.ParserInfo(source);

            if (parser.Validate(info, source.Source))
            {
                Artech.Genexus.Common.AST.AbstractNode paramRootNode = Artech.Genexus.Common.AST.ASTNodeFactory.Create(parser.Structure, source, vp, info);
                List<AbstractNode> foreachs = getForeachsInSource(paramRootNode);

                foreach (CommandBlockNode @foreach in foreachs)
                {
                    if (@foreach.Children.Count() == 0)
                    {
                        OutputError error = new OutputError("This 'For Each' block has no code.", MessageLevel.Warning, new SourcePosition(source, @foreach.Node.Row, 0));
                        KBDoctorOutput.OutputError(error);
                    }
                    else
                    {
                        bool hasWN = false;
                        CommandBlockNode wn = null;
                        foreach (AbstractNode an in @foreach.Children)
                        {
                            if (an is CommandBlockNode && an.Node.Token == 130) //when none
                            {
                                hasWN = true;
                                wn = (CommandBlockNode)an;
                            }
                        }
                        if (hasWN)
                        {
                            if (wn.Children.Count() == 0)
                            {
                                OutputError error = new OutputError("This 'When None' block has no code.", MessageLevel.Warning, new SourcePosition(source, wn.Node.Row, 0));
                                KBDoctorOutput.OutputError(error);
                            }
                        }
                        else
                        {
                            OutputError error = new OutputError("This 'For Each' block has no 'When None' associated.", MessageLevel.Warning, new SourcePosition(source, @foreach.Node.Row, 0));
                            KBDoctorOutput.OutputError(error);
                        }
                    }
                }
            }
        }

        private static List<AbstractNode> getForeachsInSource(Artech.Genexus.Common.AST.AbstractNode root)
        {
            if (root != null)
            {
                List<AbstractNode> foreachs = new List<AbstractNode>();
                foreach (AbstractNode node in root.Children)
                {
                    if (node.Node != null)
                    {
                        if (node.Node.Token == 121)   // New
                        {
                            if (node is CommandBlockNode)
                            {
                                foreachs.Add(node);
                                foreachs.AddRange(getForeachsInSource(node));
                            }
                        }
                        else
                        {
                            foreachs.AddRange(getForeachsInSource(node));
                        }
                    }
                }
                return foreachs;
            }
            return null;
        }

        internal static void ConstantsInCode(KBModel model, KBObject obj, out int cant)
        {
            int cant_aux;
            cant = 0;
            if (!isGeneratedbyPattern(obj))
            {
                if (obj is Procedure)
                {
                    ProcedurePart procpart = obj.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    if (procpart != null)
                    { 
                        ProcessConstantsInSource(model, procpart, vp, out cant_aux);
                        cant += cant_aux;
                    }
                }
                else
                {
                    if (obj is WebPanel || obj is Transaction)
                    {
                        EventsPart eventspart = obj.Parts.Get<Artech.Genexus.Common.Parts.EventsPart>();
                        VariablesPart vp = obj.Parts.Get<VariablesPart>();
                        if (eventspart != null)
                        { 
                            ProcessConstantsInSource(model, eventspart, vp, out cant_aux);
                            cant += cant_aux;
                        }
                    }
                }
            }
        }

        internal static void ProcessConstantsInSource(KBModel model, SourcePart source, VariablesPart vp, out int cant)
        {
            int cant_aux;
            cant = 0;
            var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
            ParserInfo parserInfo;
            parserInfo = new ParserInfo(source);
            var info = new Artech.Architecture.Language.Parser.ParserInfo(source);

            if (parser.Validate(info, source.Source))
            {
                Artech.Genexus.Common.AST.AbstractNode paramRootNode = Artech.Genexus.Common.AST.ASTNodeFactory.Create(parser.Structure, source, vp, info);
                List<AbstractNode> constants = getConstantsInSource(paramRootNode);

                foreach (AbstractNode constant in constants)
                {
                    if(constant is StringConstantNode)
                    {
                        if (constant.Parent is FunctionNode)
                        {
                            if(!(((FunctionNode)constant.Parent).FunctionName.ToLower() == "msg"))
                            {
                                StringConstantsIsCorrect((StringConstantNode)constant, source, out cant_aux);
                                cant += cant_aux;
                            }
                        }
                        if(constant.Parent is CommandLineNode)
                        {
                            if(!( ( (CommandLineNode)constant.Parent).Name == "do")){
                                StringConstantsIsCorrect((StringConstantNode)constant, source, out cant_aux);
                                cant += cant_aux;
                            }
                        }
                        else
                        {
                            StringConstantsIsCorrect((StringConstantNode)constant, source, out cant_aux);
                            cant += cant_aux;
                        }
                    }
                    if(constant is NumberNode)
                    {
                        //OutputError error = new OutputError("Number constant in code", MessageLevel.Warning, new SourcePosition(source, constant.Node.Row, 0));
                        //KBDoctorOutput.OutputError(error);
                    }
                }
            }
        }

        private static void StringConstantsIsCorrect(StringConstantNode scn, SourcePart source, out int cant)
        {
            cant = 0;
            bool isCorrect = scn.Text.Contains(' ') || (scn.Text.Length <= 4) || (scn.Text.Contains('<') || scn.Text.Contains('>'));
            if (!isCorrect)
            {
                OutputError error = new OutputError("String Constant in code", MessageLevel.Warning, new SourcePosition(source, scn.Node.Row, 0));
                KBDoctorOutput.OutputError(error);
                cant++;
            }
        }

        private static List<AbstractNode> getConstantsInSource(Artech.Genexus.Common.AST.AbstractNode root)
        {
            if (root != null)
            {
                List<AbstractNode> constants = new List<AbstractNode>();
                foreach (AbstractNode node in root.Children)
                {
                    if (node.Node != null)
                    {
                        if (node.Node.Token == 3)   // Constants
                        {
                            constants.Add(node);
                            constants.AddRange(getConstantsInSource(node));
                        }
                        else
                        {
                            constants.AddRange(getConstantsInSource(node));
                        }
                    }
                }
                return constants;
            }
            return null;
        }

        internal static void NewsWithoutWhenDuplicate(KBModel model, KBObject obj)
        {
            if (!isGeneratedbyPattern(obj))
            {
                if (obj is Procedure)
                {
                    ProcedurePart procpart = obj.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    if (procpart != null)
                        ProcessNewsInSource(model, procpart, vp);
                }
                else
                {
                    if (obj is WebPanel || obj is Transaction)
                    {
                        EventsPart eventspart = obj.Parts.Get<Artech.Genexus.Common.Parts.EventsPart>();
                        VariablesPart vp = obj.Parts.Get<VariablesPart>();
                        if (eventspart != null)
                            ProcessNewsInSource(model, eventspart, vp);
                    }
                }
            }
        }


        internal static void ProcessNewsInSource(KBModel model, SourcePart source, VariablesPart vp)
        {
            var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
            ParserInfo parserInfo;
            parserInfo = new ParserInfo(source);
            var info = new Artech.Architecture.Language.Parser.ParserInfo(source);

            if (parser.Validate(info, source.Source))
            {
                Artech.Genexus.Common.AST.AbstractNode paramRootNode = Artech.Genexus.Common.AST.ASTNodeFactory.Create(parser.Structure, source, vp, info);
                List<AbstractNode> news = getNewsInSource(paramRootNode);

                foreach (CommandBlockNode @new in news)
                {
                    if (@new.Children.Count() == 0)
                    {
                        OutputError error = new OutputError("This 'New' block has no code.", MessageLevel.Warning, new SourcePosition(source, @new.Node.Row, 0));
                        KBDoctorOutput.OutputError(error);
                    }
                    else 
                    {
                        bool hasWD = false;
                        CommandBlockNode wd = null;
                        foreach (AbstractNode an in @new.Children)
                        {
                            if (an is CommandBlockNode && an.Node.Token == 129) //when duplicate
                            {
                                hasWD = true;
                                wd =(CommandBlockNode)an; 
                            }
                        }
                        if(hasWD)
                        {
                            if(wd.Children.Count() == 0)
                            {
                                OutputError error = new OutputError("This 'When Duplicate' block has no code.", MessageLevel.Warning, new SourcePosition(source, wd.Node.Row, 0));
                                KBDoctorOutput.OutputError(error);
                            }
                        }
                        else
                        {
                            OutputError error = new OutputError("This 'New' block has no 'When Duplicate' associated.", MessageLevel.Warning, new SourcePosition(source, @new.Node.Row, 0));
                            KBDoctorOutput.OutputError(error);
                        }
                    }
                }
            }
        }

        private static List<AbstractNode> getNewsInSource(Artech.Genexus.Common.AST.AbstractNode root)
        {
            if (root != null)
            {
                List<AbstractNode> ifelse = new List<AbstractNode>();
                foreach (AbstractNode node in root.Children)
                {
                    if (node.Node != null)
                    {
                        if (node.Node.Token == 117)   // New
                        {
                            if (node is CommandBlockNode)
                            {
                                ifelse.Add(node);
                                ifelse.AddRange(getNewsInSource(node));
                            }
                        }
                        else
                        {
                            ifelse.AddRange(getNewsInSource(node));
                        }
                    }
                }
                return ifelse;
            }
            return null;
        }

        private static List<AbstractNode> getIfElseInSource(Artech.Genexus.Common.AST.AbstractNode root)
        {
           if (root != null)
            {
                List<AbstractNode> ifelse = new List<AbstractNode>();
                foreach (AbstractNode node in root.Children)
                {
                    if (node.Node != null)
                    {
                        if (node.Node.Token == 109)
                        {
                            if (node is CommandBlockNode)
                            {
                                ifelse.Add(node);
                                ifelse.AddRange(getIfElseInSource(node));
                            }
                        }
                        else if (node.Node.Token == 110)
                        {
                            if (node is CommandBlockNode)
                            {
                                ifelse.Add(node);
                                ifelse.AddRange(getIfElseInSource(node));
                            }
                        }
                        else
                        {
                            ifelse.AddRange(getIfElseInSource(node));
                        }
                    }
                }
                return ifelse;
            }
            return null;
        }

        internal static void AssignTypeComparer(KBModel model, KBObject obj, ref string recommendations, out int cant)
        {
            int cant_aux;
            cant = 0;
            if (!isGeneratedbyPattern(obj))
            {
                if (obj is Procedure)
                {
                    ProcedurePart procpart = obj.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    RulesPart rules = obj.Parts.Get<RulesPart>();
                    if (procpart != null) { 
                        ProccessAssignmentsInSource(model, procpart, vp, obj.Name, ref recommendations, out cant_aux);
                        cant += cant_aux;
                    }
                    if (rules != null) { 
                        ProccessAssignmentsInSource(model, rules, vp, obj.Name, ref recommendations, out cant_aux);
                        cant += cant_aux;
                    }
                }
                else
                {
                    if (obj is WebPanel || obj is Transaction)
                    {
                        EventsPart eventspart = obj.Parts.Get<Artech.Genexus.Common.Parts.EventsPart>();
                        VariablesPart vp = obj.Parts.Get<VariablesPart>();
                        RulesPart rules = obj.Parts.Get<RulesPart>();
                        if (eventspart != null) {
                            ProccessAssignmentsInSource(model, eventspart, vp, obj.Name, ref recommendations, out cant_aux);
                            cant += cant_aux;
                        }   
                    if (rules != null) { 
                            ProccessAssignmentsInSource(model, rules, vp, obj.Name, ref recommendations, out cant_aux);
                            cant += cant_aux;   
                        }
                    }
                }
            }
        }

        internal static void EmptyConditionalBlocks(KBModel model, KBObject obj, ref string recommendations, out int cant)
        {
            int cant_aux;
            cant = 0;
            if (!isGeneratedbyPattern(obj))
            {
                if (obj is Procedure)
                {
                    ProcedurePart procpart = obj.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    if (procpart != null)
                    { 
                        ProcessIfElseInSource(model, procpart, vp, ref recommendations, out cant_aux);
                        cant += cant_aux;
                    }

                }
                else
                {
                    if (obj is WebPanel || obj is Transaction)
                    {
                        EventsPart eventspart = obj.Parts.Get<Artech.Genexus.Common.Parts.EventsPart>();
                        VariablesPart vp = obj.Parts.Get<VariablesPart>();
                        if (eventspart != null)
                        { 
                            ProcessIfElseInSource(model, eventspart, vp, ref recommendations, out cant_aux);
                            cant += cant_aux;
                        }
                    }
                }
            }
        }

        private static void ProccessAssignmentsInSource(KBModel model, SourcePart source, VariablesPart vp, string objname, ref string recommendations, out int cant)
        {
            cant = 0;
            int cant_aux;
            var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
            ParserInfo parserInfo;
            parserInfo = new ParserInfo(source);
            var info = new Artech.Architecture.Language.Parser.ParserInfo(source);

            if (parser.Validate(info, source.Source))
            {
                Artech.Genexus.Common.AST.AbstractNode paramRootNode = Artech.Genexus.Common.AST.ASTNodeFactory.Create(parser.Structure, source, vp, info);
                List<AbstractNode> assigns = getAssignmentsInSource(paramRootNode);

                foreach(AssignmentNode assign in assigns)
                {
                    if(assign.Left is VariableNameNode)
                    {
                        VariableNameNode vn = (VariableNameNode) assign.Left;
                        Variable varL = vp.GetVariable(vn.VarName);

                        if(varL != null)
                        {
                            string formatType = Utility.FormattedTypeVariable(varL);
                            Domain domL = varL.DomainBasedOn;
                            CompareAssignTypes(model, vp, assign, formatType, domL, objname, source, ref recommendations, out cant_aux );
                            cant += cant_aux;
                        }
                    }
                    if (assign.Left is AttributeNameNode)
                    {
                        AttributeNameNode an = (AttributeNameNode)assign.Left;
                        Artech.Genexus.Common.Objects.Attribute att = an.Attribute;

                        if (att != null)
                        {
                            string formatType = Utility.FormattedTypeAttribute(att);
                            Domain domL = att.DomainBasedOn;
                            CompareAssignTypes(model, vp, assign, formatType, domL, objname, source, ref recommendations, out cant_aux);
                            cant += cant_aux;
                        }
                    }
                    if (assign.Left is ObjectPropertyNode)
                    {
                        ObjectPropertyNode op = (ObjectPropertyNode)assign.Left;
                        CompareAssignTypesSDT(model, source, vp, objname, assign, op, ref recommendations, out cant_aux);
                        cant += cant_aux;
                    }
                }
            }
        }

        private static void CompareAssignTypesSDT(KBModel model, SourcePart procpart, VariablesPart vp, string objname, AssignmentNode assign, ObjectPropertyNode op, ref string recommendations, out int cant)
        {
            cant = 0;
            AttributeTree.Dependency.Types type;
            string ChildText = op.Children.Skip(1).First().Text;
            StructureTypeReference parentRef = AttributeTree.GetStructureTypeReference(op.Children.First(), model, out type);
            foreach (Artech.Common.Helpers.Structure.IStructureItem item in AttributeTree.GetStructureSubStructures(parentRef, model))
            {
                if (item.Name == ChildText)
                {
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
                        string formatTypeL;
                        if (domL != null)
                            formatTypeL = Utility.FormattedTypeDomain(domL);
                        else
                            formatTypeL = Utility.ReturnFormattedType(sdtitem.Type, sdtitem.Length, sdtitem.Decimals, sdtitem.Signed);
                        CompareAssignTypes(model, vp, assign, formatTypeL, domL, objname, procpart, ref recommendations, out cant);
                    }
                }
            }
        }

        private static void CompareAssignTypes(KBModel model, VariablesPart vp, AssignmentNode assign, string formatTypeL, Domain domL, string objname, KBObjectPart part, ref string recommendations, out int cant)
        {
            cant = 0;
            int cant_aux; 
            if (assign.Right is VariableNameNode)
            {
                VariableNameNode vnr = (VariableNameNode)assign.Right;
                Variable varR = vp.GetVariable(vnr.VarName);
                string formatTypeR = Utility.FormattedTypeVariable(varR);
                Domain domR = varR.DomainBasedOn;
                CompareTypes(assign, formatTypeL, domL, formatTypeR, domR, objname, part, ref recommendations, out cant_aux);
                cant += cant_aux;
            }
            if (assign.Right is AttributeNameNode)
            {
                AttributeNameNode anR = (AttributeNameNode)assign.Right;
                if((assign.Right.Text.ToLower() != "true" && assign.Right.Text.ToLower() != "false") && formatTypeL.ToLower().Contains("boolean"))
                {
                    Artech.Genexus.Common.Objects.Attribute att = anR.Attribute;
                    CheckAssignTypesFromAttribute(assign, formatTypeL, domL, objname, part, att, ref recommendations, out cant_aux);
                    cant += cant_aux;
                }
                if(!formatTypeL.ToLower().Contains("boolean"))
                {
                    Artech.Genexus.Common.Objects.Attribute att = anR.Attribute;
                    CheckAssignTypesFromAttribute(assign, formatTypeL, domL, objname, part, att, ref recommendations, out cant_aux);
                    cant += cant_aux;
                }
            }
            if (assign.Right is FunctionNode)
            {
                FunctionNode fn = (FunctionNode)assign.Right;
                if (fn.Element != null)
                {
                    KBObject proc = (KBObject)(((FunctionNode)assign.Right).Element.Name.Tag);
                    if(proc != null) { 
                        CheckAssignTypesFromObject(assign, formatTypeL, domL, objname, part, proc, ref recommendations, out cant_aux);
                        cant += cant_aux;
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
                                        CheckAssignTypesFromObject(assign, formatTypeL, domL, objname, part, obj, ref recommendations, out cant_aux);
                                        cant += cant_aux;
                                    }
                                }
                            }
                            else if(fn.Children.First() is AttributeNameNode)
                            {
                                AttributeNameNode ann = (AttributeNameNode)fn.Children.First();
                                KBObject obj = (KBObject)ann.Element.Tag;
                                CheckAssignTypesFromObject(assign, formatTypeL, domL, objname, part, obj, ref recommendations, out cant_aux);
                                cant += cant_aux;
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
                                    CheckAssignTypesFromObject(assign, formatTypeL, domL, objname, part, obj, ref recommendations, out cant_aux);
                                    cant += cant_aux;
                                }
                            }
                        }
                        else if(omn.Children.First() is AttributeNameNode)
                        {
                            AttributeNameNode ann = (AttributeNameNode)omn.Children.First();
                            KBObject obj = (KBObject)ann.Element.Tag;
                            CheckAssignTypesFromObject(assign, formatTypeL, domL, objname, part, obj, ref recommendations, out cant_aux);
                            cant += cant_aux;
                        }
                    }
                    else
                    {
                        string text = omn.Text;
                        string[] splits = text.ToLower().Split('.');
                        if (splits.Length > 1)
                        {
                            KBObject obj = Utility.GetObjectByNameModule(model, methodname, splits[splits.Length - 2]);
                            CheckAssignTypesFromObject(assign, formatTypeL, domL, objname, part, obj, ref recommendations, out cant_aux);
                            cant += cant_aux;
                        }
                    }
                }
            }
            if (assign.Right is StringConstantNode)
            {
                StringConstantNode scn = (StringConstantNode)assign.Right;
                string text = scn.Text;
                int textlength = text.Length - 2;   //Chequeo logitud ignorando las 2 comillas
                CheckAssignTypesStringConstant(assign, formatTypeL, part, textlength, "", ref recommendations, out cant_aux);
                cant += cant_aux;
            }
            if (assign.Right is NumberNode)
            {
                if (!formatTypeL.ToLower().Contains("boolean"))
                {
                    NumberNode nn = (NumberNode)assign.Right;
                    string text = nn.Text;
                    string[] definitionR = SplitDecimals(text);
                    string lengthFormatTypeL = getLengthFromFormattedType(formatTypeL);
                    string[] definitionL = SplitDecimals(lengthFormatTypeL);
                    CheckAssignTypesNumericConstant(assign, formatTypeL, part, definitionR, definitionL, "", ref recommendations, out cant_aux);
                    cant += cant_aux;
                }
                else
                {
                    NumberNode nn = (NumberNode)assign.Right;
                    string text = nn.Text;
                    if(text.Trim() != "0" && text.Trim() != "1")
                    {
                        cant++;
                        string msgOutput = " Number greater than 1 assigned to a boolean";
                        OutputMsgAssignComparer(assign, part, msgOutput, ref recommendations);
                    }
                }
            }
        }

        private static void CheckAssignTypesFromAttribute(AssignmentNode assign, string formatTypeL, Domain domL, string objname, KBObjectPart part, Artech.Genexus.Common.Objects.Attribute att, ref string recommendations, out int cant)
        {
            cant = 0;
            if (att != null)
            {
                string formatTypeR = Utility.FormattedTypeAttribute(att);
                Domain domR = att.DomainBasedOn;
                CompareTypes(assign, formatTypeL, domL, formatTypeR, domR, objname, part, ref recommendations, out cant);
            }
        }

        private static void CheckAssignTypesFromObject(AssignmentNode assign, string formatTypeL, Domain domL, string objname, KBObjectPart part, KBObject obj, ref string recommendations, out int cant)
        {
            cant = 0;
            string formatTypeR = Utility.GetOutputFormatedType(obj);
            Domain domR = Utility.GetOutputDomains(obj);
            if (formatTypeR != "")
                CompareTypes(assign, formatTypeL, domL, formatTypeR, domR, objname, part, ref recommendations, out cant);
        }

        private static void CheckAssignTypesStringConstant(AbstractNode an, string formatTypeL, KBObjectPart part, int textlength, string extra_text, ref string recommendations, out int cant)
        {
            cant = 0;
            if (textlength > int.Parse(getLengthFromFormattedType(formatTypeL)))
            {
                cant++;
                string msgOutput = " Text assigned is too long (" + textlength.ToString() + ") for " + formatTypeL;
                msgOutput = extra_text + msgOutput;
                OutputMsgAssignComparer(an, part, msgOutput, ref recommendations);
            }
        }

        private static void CheckAssignTypesNumericConstant(AbstractNode assign, string formatTypeL, KBObjectPart part, string[] definitionR, string[] definitionL, string extra_text, ref string recommendations, out int cant)
        {
            cant = 0;
            bool hasLength = false;
            if (definitionL[1] != "0")                                                                   //Chequeo de longitud (wiki genexus): If it is defined as numeric you must consider that the whole length includes the decimal places, the decimal point and the sign.
                hasLength = (int.Parse(definitionL[0]) - 1) >= (definitionR[0].Length + definitionR[1].Length);
            else if (definitionR[1] != "0")
                hasLength = (int.Parse(definitionL[0]) >= (definitionR[0].Length + definitionR[1].Length));
            else
                hasLength = (int.Parse(definitionL[0]) >= (definitionR[0].Length));

            if (!hasLength)
            {
                cant++;
                string msgOutput = " Number assigned is too long (" + formatTypeL + ")";
                msgOutput = extra_text + msgOutput;
                OutputMsgAssignComparer(assign, part, msgOutput, ref recommendations);
            }

            if (hasLength && int.Parse(definitionR[1]) != 0 && int.Parse(definitionL[1]) < definitionR[1].Length) //Chequeo de decimales
            {
                cant++;
                string msgOutput = " Number assigned decimals are too long (" + formatTypeL + ")";
                OutputMsgAssignComparer(assign, part, msgOutput, ref recommendations);
            }
        }

        private static void CompareTypes(AssignmentNode assign, string formatTypeL, Domain domL, string formatTypeR, Domain domR, string objname, KBObjectPart part, ref string recommendations, out int cant)
        {
            int cant_aux;
            cant = 0;
            if (formatTypeL.ToLower().Contains("char") && formatTypeR.ToLower().Contains("char"))
            {
                string lengthPicL = getLengthFromFormattedType(formatTypeL);
                string lengthPicR = getLengthFromFormattedType(formatTypeR);

                CheckAssignTypesLengthString(assign, formatTypeL, formatTypeR, part, lengthPicL, lengthPicR, ref recommendations, out cant_aux);
                cant += cant_aux;
                CheckAssignTypesDomains(assign, domL, domR, objname, part, ref recommendations, out cant_aux);
                cant += cant_aux;
            }
            else if (formatTypeL.ToLower().Contains("numeric") && formatTypeR.ToLower().Contains("numeric"))
            {
                string lengthPicL = getLengthFromFormattedType(formatTypeL);
                string lengthPicR = getLengthFromFormattedType(formatTypeR);
                string[] splitsL = SplitDecimals(lengthPicL);
                string[] splitsR = SplitDecimals(lengthPicR);
                CheckAssignTypesLengthNumeric(assign, formatTypeL, formatTypeR, part, splitsL, splitsR, "", ref recommendations, out cant_aux);
                cant += cant_aux;
                CheckAssignTypesDomains(assign, domL, domR, objname, part, ref recommendations, out cant_aux);
                cant += cant_aux;
            }
            else
            {
                if(formatTypeL != "Unknown" && formatTypeR != "Unknown")
                { 
                    if (formatTypeR != formatTypeL)
                    {
                        string msgOutput = " " + formatTypeL + "<>" + formatTypeR;
                        OutputMsgAssignComparer(assign, part, msgOutput, ref recommendations);
                    }
                }
            }
        }

        private static void CompareTypes(AbstractNode an, string formatTypeL, Domain domL, string formatTypeR, Domain domR, string objname, KBObjectPart part, string extra_text, ref string recommendations, out int cant)
        {
            int cant_aux;
            cant = 0;
            if (formatTypeL.ToLower().Contains("char") && formatTypeR.ToLower().Contains("char"))
            {
                string lengthPicL = getLengthFromFormattedType(formatTypeL);
                string lengthPicR = getLengthFromFormattedType(formatTypeR);
                CheckAssignTypesLengthString(an, formatTypeL, formatTypeR, part, lengthPicL, lengthPicR, extra_text, ref recommendations, out cant_aux);
                cant += cant_aux;
                CheckParametersTypesDomains(an, domL, domR, objname, part, extra_text, ref recommendations, out cant_aux);
                cant += cant_aux;                
            }
            else if (formatTypeL.ToLower().Contains("numeric") && formatTypeR.ToLower().Contains("numeric"))
            {
                string lengthPicL = getLengthFromFormattedType(formatTypeL);
                string lengthPicR = getLengthFromFormattedType(formatTypeR);
                string[] splitsL = SplitDecimals(lengthPicL);
                string[] splitsR = SplitDecimals(lengthPicR);
                CheckAssignTypesLengthNumeric(an, formatTypeL, formatTypeR, part, splitsL, splitsR, extra_text, ref recommendations, out cant_aux);
                cant += cant_aux;
                CheckParametersTypesDomains(an, domL, domR, objname, part, extra_text, ref recommendations, out cant_aux);
                cant += cant_aux;
            }
            else
            {
                if (formatTypeL != "Unknown" && formatTypeR != "Unknown")
                {
                    if (formatTypeR != formatTypeL)
                    {
                        string msgOutput = " " + formatTypeL + "<>" + formatTypeR;
                        msgOutput = extra_text + msgOutput;
                        OutputMsgAssignComparer(an, part, msgOutput, ref recommendations);
                    }
                }
            }
        }

        private static void CheckAssignTypesLengthString(AssignmentNode assign, string formatTypeL, string formatTypeR, KBObjectPart part, string lengthPicL, string lengthPicR, ref string recommendations, out int cant)
        {
            cant = 0;
            if (int.Parse(lengthPicL) < int.Parse(lengthPicR))
            {
                cant++;
                string msgOutput = " String assigned is too long " + formatTypeL + "<" + formatTypeR;
                OutputMsgAssignComparer(assign, part, msgOutput, ref recommendations);
            }
        }

        private static void CheckAssignTypesLengthString(AbstractNode an, string formatTypeL, string formatTypeR, KBObjectPart part, string lengthPicL, string lengthPicR, string extra_text, ref string recommendations, out int cant)
        {
            cant = 0;
            if (int.Parse(lengthPicL) < int.Parse(lengthPicR))
            {
                cant++;
                string msgOutput = " String assigned is too long " + formatTypeL + "<" + formatTypeR;
                msgOutput = extra_text + msgOutput;
                OutputMsgAssignComparer(an, part, msgOutput, ref recommendations);
            }
        }

        private static void CheckAssignTypesLengthNumeric(AbstractNode an, string formatTypeL, string formatTypeR, KBObjectPart part, string[] splitsL, string[] splitsR, string extra_text, ref string recommendations, out int cant)
        {
            cant = 0;
            if ((int.Parse(splitsL[0]) - int.Parse(splitsL[1])) < (int.Parse(splitsR[0]) - int.Parse(splitsR[1])))
            {
                string msgOutput = " Number assigned is too long " + formatTypeL + "<" + formatTypeR;
                msgOutput = extra_text + msgOutput;
                cant++;
                OutputMsgAssignComparer(an, part, msgOutput, ref recommendations);
            }
            else if (int.Parse(splitsL[1]) < int.Parse(splitsR[1]))
            {
                string msgOutput = " Number decimals assigned are too long " + formatTypeL + "<" + formatTypeR;
                msgOutput = extra_text + msgOutput;
                cant++;
                OutputMsgAssignComparer(an, part, msgOutput, ref recommendations);
            }
        }

        private static void CheckAssignTypesDomains(AssignmentNode assign, Domain domL, Domain domR, string objname, KBObjectPart part, ref string recommendations, out int cant)
        {
            cant = 0;
            if (domL != null)
            {
                if (domR != null)
                {
                    if (domL.Name != domR.Name)
                    {
                        cant++;
                        string msgOutput = " Variables are based on different domains " + domL.Name + "<>" + domR.Name;
                        OutputMsgAssignComparer(assign, part, msgOutput, ref recommendations);
                    }
                }
                else
                {
                    string textRight = "";
                    if (assign.Right.Text == "udp" || assign.Right.Text == "call")
                    {
                        textRight = assign.Right.Children.First<AbstractNode>().Text;
                    }
                    else
                    {
                        textRight = assign.Right.Text;
                    }
                    cant++;
                    string msgOutput = " (" + textRight + ") Doesn't have domain but (" + assign.Left.Text + ") is BasedOn " + domL.Name;
                    OutputMsgAssignComparer(assign, part, msgOutput, ref recommendations);
                }
            }
            else
            {
                if (domR != null)
                {
                    string textRight = "";
                    if (assign.Right.Text == "udp" || assign.Right.Text == "call")
                    {
                        textRight = assign.Right.Children.First<AbstractNode>().Text;
                    }
                    else
                    {
                        textRight = assign.Right.Text;
                    }
                    cant++;
                    string msgOutput = " (" + assign.Left.Text + ") Doesn't have domain but (" + textRight + ") is BasedOn " + domR.Name;
                    OutputMsgAssignComparer(assign, part, msgOutput, ref recommendations);
                }
            }
        }

        private static void CheckParametersTypesDomains(AbstractNode an, Domain domL, Domain domR, string objname, KBObjectPart part, string extra_text, ref string recommendations, out int cant)
        {
            cant = 0;
            if (domL != null)
            {
                if (domR != null)
                {
                    if (domL.Name != domR.Name)
                    {
                        cant++;
                        string msgOutput = " Variables are based on different domains " + domL.Name + "<>" + domR.Name;
                        msgOutput = extra_text + msgOutput;
                        OutputMsgAssignComparer(an, part, msgOutput, ref recommendations);
                    }
                }
                else
                {
                    cant++;
                    string msgOutput = " Variables are based on different domains " + domL.Name + "<>(No Domain)";
                    msgOutput = extra_text + msgOutput;
                    OutputMsgAssignComparer(an, part, msgOutput, ref recommendations);
                }
            }
            else
            {
                if (domR != null)
                {
                    cant++;
                    string msgOutput = " Variables are based on different domains (No Domain)<>" + domR.Name;
                    msgOutput = extra_text + msgOutput;
                    OutputMsgAssignComparer(an, part, msgOutput, ref recommendations);
                }
            }
        }

        private static void OutputMsgAssignComparer(AbstractNode an, KBObjectPart part, string msgOutput, ref string recommendations)
        {
            string printText = Utility.ExtractCommentsAndBreakLines(an.Text.Replace(System.Environment.NewLine, " "));
            recommendations += printText + "<br>" + msgOutput + "<br>";
            OutputError err = new OutputError(printText + msgOutput, MessageLevel.Warning, new SourcePosition(part, an.Node.Row, 0));
            KBDoctorOutput.OutputError(err);
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

        private static string getLengthFromFormattedType(string formatType)
        {
            if(formatType.Contains('(') && formatType.Contains(')'))
            {
                string[] splits = formatType.Split('(');
                splits = splits[1].Split(')');
                return splits[0];
            }
            else
            {
                return "0";
            }
            
        }

        private static List<AbstractNode> getAssignmentsInSource(Artech.Genexus.Common.AST.AbstractNode root)
        {
            if(root != null) { 
                List<AbstractNode> assignments = new List<AbstractNode>();
                foreach(AbstractNode node in root.Children)
                {
                    if (node.Node != null)
                        if (node.Node.Token == 107 || node.Node.Token == -1) { 
                            if (node is AssignmentNode)
                                assignments.Add(node);
                        }
                        else { 
                            assignments.AddRange(getAssignmentsInSource(node));
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

        internal static void ProceduresCalledAsFunction(KBModel model, KBObject obj, ref string recommendations, out int cant)
        {
            int cant_aux;
            cant = 0;
            if (!isGeneratedbyPattern(obj))
            {
                if (obj is Procedure)
                {
                    ProcedurePart procpart = obj.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    RulesPart rules = obj.Parts.Get<RulesPart>();
                    if (procpart != null)
                    {
                        ProcessCallsAsFuctions(model, procpart, vp, ref recommendations, out cant_aux);
                        cant += cant_aux;
                    }
                    if (rules != null)
                    {
                        ProcessCallsAsFuctions(model, rules, vp, ref recommendations, out cant_aux);
                        cant += cant_aux;
                    }
                }
                else if (obj is WebPanel || obj is Transaction)
                {
                    EventsPart eventspart = obj.Parts.Get<EventsPart>();
                    VariablesPart vp = obj.Parts.Get<VariablesPart>();
                    RulesPart rules = obj.Parts.Get<RulesPart>();
                    if (eventspart != null)
                    {
                        ProcessCallsAsFuctions(model, eventspart, vp, ref recommendations, out cant_aux);
                        cant += cant_aux;
                    }
                    if (rules != null)
                    {
                        ProcessCallsAsFuctions(model, rules, vp, ref recommendations, out cant_aux);
                        cant += cant_aux;
                    }
                }
            }
        }

        internal static bool ReviewCommitsFromTo(KnowledgeBase KB, List<IKBVersionRevision> list, out Dictionary<string, List<string[]>> reviews_by_user)
        {
            bool success;
            reviews_by_user = new Dictionary<string, List<string[]>>();
            try
            {
                List<string> objs_reviewed = new List<string>();
                foreach (IKBVersionRevision revision in list)
                {
                    foreach (IRevisionAction action in revision.Actions)
                    {
                        string name = "";
                        string module = "";
                        if (action.Operation.ToString().ToLower() != "delete")
                        {
                            QualifiedName qn = null;

                            if (KB.DesignModel.Objects.GetName(action.Key) != null)
                            {
                                KBObject obj_act = KB.DesignModel.Objects.Get(action.Guid);
                                qn = obj_act.QualifiedName;
                                name = qn.ObjectName;
                            }
                            else
                            {
                                qn = null;
                            }
                            KBDoctorOutput.Message(string.Format("{8}{0},{1},{2},{3},{4},{5},{6},{7}", revision.UserName, revision.Comment.Replace(",", " ").Replace(Environment.NewLine, " "),
                                                                                            action.Operation, action.Type, name, action.Name, action.Description, revision.CommitDate.ToString(), Environment.NewLine));
                            if (name != "")
                            {
                                KBObject obj = KB.DesignModel.Objects.Get(action.Key);
                                if (!(objs_reviewed.Contains(qn.ToString())))
                                {
                                    objs_reviewed.Add(qn.ToString());
                                    IOutputService output = CommonServices.Output;
                                    List<KBObject> objs = new List<KBObject>();
                                    objs.Add(obj);
                                    List<string[]> lines = new List<string[]>();
                                    double technical_debt;
                                    API.PreProcessPendingObjects(KB, output, objs, out lines, out technical_debt);
                                    if (lines.Count > 0)
                                    {
                                        if (!reviews_by_user.ContainsKey(revision.UserName))
                                        {
                                            List<string[]> reviews = new List<string[]>();
                                            reviews_by_user.Add(revision.UserName, reviews);
                                        }
                                        foreach (string[] line in lines)
                                        {
                                            reviews_by_user[revision.UserName].Add(line);
                                        }
                                    }
                                    objs.Clear();
                                }
                                else
                                {
                                    KBDoctorOutput.Message("Object already reviewed.");
                                }
                            }
                        }
                    }
                }
                success = true;
            }
            catch(Exception e)
            {
                success = false;
                KBDoctorOutput.InternalError("Error processing commits.", e);
            }
            return success;
        }

        private static void ProcessCallsAsFuctions(KBModel model, SourcePart source, VariablesPart vp, ref string recommendations, out int cant)
        {
            int cant_aux;
            cant = 0;
            var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
            ParserInfo parserInfo;
            parserInfo = new ParserInfo(source);
            var info = new Artech.Architecture.Language.Parser.ParserInfo(source);

            if (parser.Validate(info, source.Source))
            {
                AbstractNode paramRootNode = ASTNodeFactory.Create(parser.Structure, source, vp, info);
                List<AbstractNode> calls = getCallsInSource(paramRootNode);
                foreach (AbstractNode call in calls)
                {
                    if (call is AssignmentNode)
                    {
                        if (((AssignmentNode)call).Right is FunctionNode)
                        {
                            FunctionNode fn = ((FunctionNode)((AssignmentNode)call).Right);
                            if (!(fn.FunctionName.ToLower() == "udp" || fn.FunctionName.ToLower() == "create"))
                            {
                                if(fn.Element != null)
                                    if(fn.Element.Name.Tag != null)
                                    {
                                        MsgOutputProcessCallAsAFunction(source, ref recommendations, fn);
                                        cant++;
                                    }
                            }
                        }
                    }
                    else if (call is FunctionNode)
                    {
                        FunctionNode fn = (FunctionNode)call;
                        if (!(fn.FunctionName.ToLower() == "call" || fn.FunctionName.ToLower() == "submit"))
                        {
                            if (fn.Element != null)
                                if (fn.Element.Name.Tag != null)
                                {
                                    MsgOutputProcessCallAsAFunction(source, ref recommendations, fn);
                                    cant++;
                                }
                        }
                    }
                }
            }
        }

        private static void MsgOutputProcessCallAsAFunction(SourcePart source, ref string recommendations, FunctionNode fn)
        {
            string msgOutput = "Procedure called as a function";
            string printText = Utility.ExtractCommentsAndBreakLines(fn.Text.Replace(System.Environment.NewLine, " "));
            recommendations += printText + "<br>" + msgOutput + "<br>";
            OutputError err = new OutputError(printText + msgOutput, MessageLevel.Warning, new SourcePosition(source, fn.Node.Row, 0));
            KBDoctorOutput.OutputError(err);
        }

        public static void DocumentsInWebPanels(KnowledgeBase KB, KBObject obj, ref string recommendations, out int cant)
        {
            cant = 0;
            if (obj is WebPanel && !Utility.IsGeneratedByPattern(obj))
            {
                VariablesPart vp = obj.Parts.Get<VariablesPart>();
                if (vp != null)
                {
                    foreach (Variable v in vp.Variables)
                    {
                        string txtType = Utility.GetStringType(v);
                        if (!Utility.IsTypeAllowedInWP(txtType))
                        {
                            cant++;
                            string msgOutput = "Variable of type " + txtType + " used in a WebPanel";
                            recommendations +=  msgOutput + "<br>";
                            OutputError err = new OutputError(msgOutput, MessageLevel.Warning, new KBObjectPosition(obj.Parts.Get<VariablesPart>()));
                            KBDoctorOutput.OutputError(err);
                        }
                    }
                }
            }
        }

        internal static bool HasOutputRule(KBObject obj)
        {
            if(obj is Procedure)
            {
                RulesPart rp = obj.Parts.Get<RulesPart>();
                VariablesPart vp = obj.Parts.Get<VariablesPart>(); 
                if(rp != null)
                {
                    return hasOutputInRules(rp, vp);
                }
            }
            return false;
        }

        private static List<AbstractNode> getOutputsInSource(Artech.Genexus.Common.AST.AbstractNode root)
        {
            List<AbstractNode> calls = new List<AbstractNode>();
            if (root != null)
            {
                if (root is RuleNode)
                {

                    if (root.Node is IParserObjectBase)
                    {
                        IParserObjectBase pob = root.Node;

                        if (pob.Data is Artech.Architecture.Language.Parser.Data.Rule)
                        {
                            Artech.Architecture.Language.Parser.Data.Rule ruledata = (Artech.Architecture.Language.Parser.Data.Rule)pob.Data;
                            if (((Artech.Architecture.Language.Parser.Data.RuleName)ruledata.Name).Text.ToLower() == "output_file")
                            {
                                calls.Add(root);
                            }
                        }
                    }
                }
                
                foreach (AbstractNode node in root.Children)
                {
                    if (node.Node != null)
                    {
                        if(node is RuleNode)
                        {

                            if (node.Node is IParserObjectBase)
                            {
                                IParserObjectBase pob = node.Node;
                                
                                if(pob.Data is Artech.Architecture.Language.Parser.Data.Rule)
                                {
                                    Artech.Architecture.Language.Parser.Data.Rule ruledata = (Artech.Architecture.Language.Parser.Data.Rule)pob.Data;
                                    if (((Artech.Architecture.Language.Parser.Data.RuleName)ruledata.Name).Text.ToLower() == "output_file")
                                    {
                                        calls.Add(node);
                                    }
                                }
                            }
                        }
                    }
                }
                return calls;
            }
            return null;
        }

        internal static bool hasOutputInRules(RulesPart rp, VariablesPart vp)
        {

            var parser = Artech.Genexus.Common.Services.GenexusBLServices.Language.CreateEngine() as Artech.Architecture.Language.Parser.IParserEngine2;
            ParserInfo parserInfo;
            parserInfo = new ParserInfo(rp);
            var info = new Artech.Architecture.Language.Parser.ParserInfo(rp);

            if (parser.Validate(info, rp.Source))
            {
                AbstractNode paramRootNode = ASTNodeFactory.Create(parser.Structure, rp, vp, info);
                List<AbstractNode> outputs = getOutputsInSource(paramRootNode);
                if (outputs.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        internal static void GenerateRESTCalls(KnowledgeBase KB, KBObject obj)
        {
            if (obj.GetPropertyValue("CALL_PROTOCOL").ToString() == "HTTP")
            {
                if (!HasOutputRule(obj))
                {
                    string qualifiedName = obj.QualifiedName.ModuleName + (obj.QualifiedName.ModuleName == "" ? "a" : ".a") + obj.QualifiedName.ObjectName;
                    string strsignature = "";
                    string strsignaturenames = "";
                    bool first = true;
                    ICallableObject callableObject = obj as ICallableObject;
                    if (callableObject != null)
                    {

                        foreach (Signature signature in callableObject.GetSignatures())
                        {
                            foreach (Parameter parm in signature.Parameters)
                            {

                                if (!first)
                                {
                                    strsignature += ", ";
                                    strsignaturenames += ", ";
                                }
                                else
                                {
                                    first = false;
                                }

                                string typeparm = "";
                                string varnames = "";
                                if (parm.IsAttribute)
                                {
                                    Artech.Genexus.Common.Objects.Attribute att = (Artech.Genexus.Common.Objects.Attribute)parm.Object;
                                    if (att != null)
                                    {
                                        typeparm = Utility.FormattedTypeAttribute(att);
                                        varnames = att.Name + "{" + Utility.FormattedTypeAttribute(att) + "}";
                                    }
                                }
                                else
                                {
                                    Variable var = (Variable)parm.Object;
                                    if (var != null)
                                    {
                                        typeparm = Utility.FormattedTypeVariable(var);
                                        varnames += var.Name + "{" + Utility.FormattedTypeVariable(var) + "}";
                                    }

                                }
                                strsignaturenames += varnames;
                                switch (parm.Accessor.ToString())
                                {
                                    case "PARM_OUT":
                                        strsignature += "out: " + typeparm;
                                        break;
                                    case "PARM_INOUT":
                                        strsignature += "inout: " + typeparm;
                                        break;
                                    case "PARM_IN":
                                        strsignature += "in: " + typeparm;
                                        break;
                                }
                                
                            }
                        }

                    }
                    KBDoctorOutput.Message("# Objeto:" + obj.QualifiedName.ObjectName + " Parameters: " + strsignaturenames);
                    KBDoctorOutput.Message("%protocol%%subdomain%%url%%virtualdir%" + qualifiedName + ".aspx?" + GetDataFromSignature(strsignature));
                    KBDoctorOutput.Message(Environment.NewLine);
                }
                
            }
        }
        
        internal static string GetDataFromSignature(string signature)
        {
            bool first = true;
            string datacall = "";
            string[] splits = signature.Split(',');
            foreach(string split in splits)
            {
                if(!first)
                {
                    datacall += ",";
                }
                else
                {
                    first = false;
                }
                string parameter = "";
                if (split.Contains("in: "))
                {
                    parameter = split.Replace("in: ","").Trim();
                }
                if (split.Contains("inout: "))
                {
                    parameter = split.Replace("inout: ", "").Trim();
                }
                datacall+= parameter;
            }
            return datacall;
        }

        public static void SDTWithDateInWS(KBObject obj)
        {
            
            if (obj.GetPropertyValue("CALL_PROTOCOL").ToString().ToUpper() == "SOAP")
            {
                string sdtout = "";
                bool founddate = false;
                ICallableObject callableObject = obj as ICallableObject;
                foreach (Signature signature in callableObject.GetSignatures())
                {
                    foreach (Parameter parm in signature.Parameters)
                    {

                        if (parm.IsAttribute)
                        {
                            Artech.Genexus.Common.Objects.Attribute att = (Artech.Genexus.Common.Objects.Attribute)parm.Object;
                            if (att != null)
                                if (Utility.FormattedTypeAttribute(att).Contains("GX_SDT"))
                                {
                                    att.ToString();
                                }
                        }
                        else
                        {
                            Variable var = (Variable)parm.Object;
                            if (var != null)
                                if (Utility.FormattedTypeVariable(var).Contains("GX_SDT"))
                                {
                                    AttCustomType attcustype = (AttCustomType)var.GetPropertyValue("ATTCUSTOMTYPE");

                                    KBObject sdt_obj = GetSDTFromAttCustomType(obj.Model, attcustype);

                                    if(sdt_obj is SDT)
                                    {
                                        SDT sdt = (SDT)sdt_obj;
                                        founddate = CheckSDTHasDate(sdt.SDTStructure.Root, sdt.Name, ref sdtout) || founddate;

                                    }
                                }
                        }
                    }
                    if(founddate)
                    {
                        KBDoctorOutput.Message("Object: " + obj.Name);
                        KBDoctorOutput.Warning(sdtout);
                    }
                }
            }
            /*if (obj is SDT)
            {
                SDT sdt = (SDT)obj;
                CheckSDTHasDate(sdt.SDTStructure.Root, sdt.Name);
;            }*/
        }

        private static KBObject GetSDTFromAttCustomType(KBModel model, AttCustomType attcustype)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(attcustype.Guid);

            XmlNodeList xmlnlType = xdoc.GetElementsByTagName("Type");
            XmlNode nodetype = xmlnlType.Item(0);

            XmlNodeList xmlnlID = xdoc.GetElementsByTagName("Id");
            XmlNode nodeid = xmlnlID.Item(0);
            Guid guid = new Guid(nodetype.InnerText);
            int id = 0;
            if (!Int32.TryParse(nodeid.InnerText, out id))
            {
                KBDoctorOutput.Error("Error recovering SDT type.");
            }

            EntityKey entityKey = new EntityKey(guid, id);
            KBObject sdt = model.Objects.Get(entityKey);
            return sdt;
        }

        private static bool CheckSDTHasDate(SDTLevel level, string levelname, ref string strout)
        {
            bool ret = false;
            foreach (var childItem in level.GetItems<SDTItem>())
            {
                if(IsSDTItemDate(childItem))
                {
                    strout += levelname + "." + childItem.Name + " is date" + Environment.NewLine ;
                    ret = true;
                }
            }
            foreach(var childLevel in level.GetItems<SDTLevel>())
            {
                ret = ret || CheckSDTHasDate(childLevel, levelname + "." + childLevel.Name, ref strout);
            }
            return ret;
        }

        private static bool IsSDTItemDate(SDTItem item)
        {
            if(item.Type.ToString().ToLower().Contains("date"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        

        public static void ListSDT(KBObject obj)
        {
            if(obj is SDT)
            {
                SDT sdt = (SDT)obj;
                KBDoctorOutput.Message("//Data generated for SDT:" + sdt.Name);
                ListStructure(obj.Model, sdt.SDTStructure.Root, "");
            }
        }

        public static void ChangeSDTSerialization(KBObject obj)
        {
            if (obj is SDT)
            {
                SDT sdt = (SDT)obj;
                KBDoctorOutput.Message("//OBJ: " + sdt.Name);
                ChangeSDTSerializationStructure(obj.Model, sdt.SDTStructure.Root, "");
                obj.Save();
            }
        }

        private static void ChangeSDTSerializationStructure(KBModel model, SDTLevel level, string prev_levelname)
        {
            if (level.IsCollection)
            {
                string XmlSerializationProperty = level.GetPropertyValue("idXmlInclude").ToString();
                if (XmlSerializationProperty.ToLower() != "idXmlIncludeNotNull")
                {
                    level.SetPropertyValue("idXmlInclude", "idXmlIncludeNotNull");
                    KBDoctorOutput.Message("Objeto: " + level.FullName);
                    KBDoctorOutput.Message("Tiene valor: " + XmlSerializationProperty);
                    KBDoctorOutput.Message("------------");
                }
            }
            foreach (var childItem in level.GetItems<SDTItem>())
            {   
                if(childItem.IsCollection)
                {
                    string XmlSerializationProperty = childItem.GetPropertyValue("idXmlInclude").ToString();
                    if (XmlSerializationProperty.ToLower() != "idXmlIncludeNotNull")
                    {
                        childItem.SetPropertyValue("idXmlInclude", "idXmlIncludeNotNull");
                        KBDoctorOutput.Message("Objeto: " + childItem.FullName);
                        KBDoctorOutput.Message("Tiene valor: " + XmlSerializationProperty);
                        KBDoctorOutput.Message("------------");
                    }
                }
            }
            foreach (var childLevel in level.GetItems<SDTLevel>())
            {
                ChangeSDTSerializationStructure(model, childLevel, "");
            }
        }


                private static void ListStructure(KBModel model, SDTLevel level, string prev_levelname)
        {

            //KBDoctorOutput.Message(", collection: " + );
            //KBDoctorOutput.Message(Environment.NewLine);
            bool cont = true;
            int i = 0;
            var random = new Random();
            int max = random.Next(1) + 2;
            string levelname = prev_levelname + level.Name;
            string namevariable;
            namevariable = GetSDTLevelName(level, prev_levelname);
            
            if (level.IsCollection)
            {
                KBDoctorOutput.Message("&" + levelname + " = new ()" + "//Type: " + level.FullName + "(Collection)");
                KBDoctorOutput.Message("&" + namevariable + " = new ()" + "//Type: " + level.FullName + "(Item)");
            }
            else
            {
                KBDoctorOutput.Message("&" + levelname + " = new ()" + "//Type: " + (prev_levelname != "" ? prev_levelname + "." : "") + level.Name);
            }
            while (cont)
            {
                foreach (var childItem in level.GetItems<SDTItem>())
                {
                    string str_datatype = GetItemDataType(childItem);
                    if (!str_datatype.Contains("BITMAP"))
                    {
                        if (str_datatype.Contains("GX_SDT"))
                        {
                            if (childItem.IsCollection)
                            {
                                KBDoctorOutput.Message("//TO-DO: Run this proc on the SDT associated with " + childItem.FullName + " and add that variable to the following line:");

                                KBDoctorOutput.Message("//&" + namevariable + "." + childItem.Name + ".add({Name_Variable:" + childItem.FullName + "})");
                            }
                            else
                            {
                                KBDoctorOutput.Message("//TO-DO: Run this proc on the SDT associated with " + childItem.FullName + " and assign that variable to the following line:");
                                KBDoctorOutput.Message("//&" + namevariable + "." + childItem.Name + " = " + GetExamplesFromType(str_datatype));
                            }
                        }
                        else
                        {
                            KBDoctorOutput.Message("&" + namevariable + "." + childItem.Name + " = " + GetExamplesFromType(str_datatype));
                        }
                    }
                }

                if (level.IsCollection)
                {
                    KBDoctorOutput.Message("&" + levelname + ".Add(" + "&" + namevariable + ".Clone())");
                }
                else
                {
                    cont = false;
                }
                i++;
                if (i >= max)
                {
                    cont = false;
                }
            }
            //ListItem(childItem);
            foreach (var childLevel in level.GetItems<SDTLevel>())
            {
                ListStructure(model, childLevel, levelname);
                KBDoctorOutput.Message("&" + levelname + "." + childLevel.Name + " = " + "&" + levelname + childLevel.Name);
            }

        }

        private static string GetSDTLevelName(SDTLevel level, string prev_levelname)
        {
            string namevariable;
            if (level.IsCollection)
            {
                if (level.CollectionItemName.Contains(level.Name))
                {
                    if (level.CollectionItemName == level.Name)
                    {
                        namevariable = prev_levelname + level.CollectionItemName + "Item";
                    }
                    else
                    {
                        namevariable = prev_levelname + level.Name + level.CollectionItemName;
                    }
                }
                else
                {
                    namevariable = prev_levelname + level.Name + level.CollectionItemName;
                }
            }
            else
            {
                namevariable = prev_levelname + level.Name;
            }

            return namevariable;
        }

        private static string GetItemDataType(SDTItem item)
        {
            string dataType = item.Type.ToString() + "(" + item.Length.ToString() + (item.Decimals > 0 ? "." + item.Decimals.ToString() : "") + ")";
            return dataType;
        }

        internal static bool GetRandomBoolean()
        {
            bool[] chars = new bool[] { true, false };
            var random = new Random();
            bool selected = chars[random.Next(chars.Length)];
            return selected;
        }

        internal static string GetExamplesFromType(string type)
        {
            string[] splits = type.Split('(');
            string typename = splits[0];
            if (splits.Length == 2)
            {
                string typelength = splits[1].Replace(")", "");
                if (typelength.Contains("."))
                {
                    splits = typelength.Split('.');
                    int int_len = Int32.Parse(splits[0]);
                    int decimal_len = Int32.Parse(splits[1]);
                    return GetDataFromStringType(typename, int_len, decimal_len);
                }
                else
                {
                    int int_len = Int32.Parse(typelength);
                    return GetDataFromStringType(typename, int_len);
                }
            }
            else
            {
                return GetDataFromStringType(typename);
            }

        }

        internal static string GetDataFromStringType(string typename)
        {
            switch (typename.ToLower())
            {
                case "boolean":
                    return GetRandomBoolean().ToString();
                default:
                    return "";
            }
        }
        internal static string GetDataFromStringType(string typename, int int_len)
        {
            switch (typename.ToLower())
            {
                case "character":
                    return GetRandomString(int_len);
                    break;
                case "varchar":
                    return GetRandomString(int_len);
                    break;
                case "numeric":
                    return GetRandomNumber(int_len).ToString();
                case "boolean":
                    return GetRandomBoolean().ToString();
                default:
                    return "";
            }
        }

        internal static string GetDataFromStringType(string typename, int int_len, int decimal_len)
        {
            switch (typename.ToLower())
            {
                case "boolean":
                    return GetRandomBoolean().ToString();
                    break;
                case "character":
                    return GetRandomString(int_len);
                    break;
                case "varchar":
                    return GetRandomString(int_len);
                    break;
                case "numeric":
                    if (decimal_len == 0)
                    {
                        return GetRandomNumber(int_len).ToString();
                    }
                    else
                    {
                        return GetRandomNumber(int_len, decimal_len);
                    }
                case "date":
                    return GetRandomDate();
                case "datetime":
                    return GetRandomDateTime();
                default:
                    return "";
            }
        }

        internal static string GetRandomDateTime()
        {
            //Genero un aleatorio de los años 20XX
            string year = GetRandomYear();

            //Genero un aleatorio de los meses
            string month = GetRandomMonth();

            //genero un aleatorio de días
            string day = GetRandomDay();

            string hour = GetRandomHour();

            string minute = GetRandomMinute();

            var chars = "ap";
            var random = new Random();
            int length = 1;
            var numberChars = new char[length];

            for (int i = 0; i < numberChars.Length; i++)
            {
                numberChars[i] = chars[random.Next(chars.Length)];
            }

            string ampm = new String(numberChars);


            return "#" + year + "-" + month + "-" + day + " " + hour + ":" + minute + ampm + "#";
        }

        internal static string GetRandomDate()
        {
            //Genero un aleatorio de los años 20XX
            string year = GetRandomYear();

            //Genero un aleatorio de los meses
            string month = GetRandomMonth();

            //genero un aleatorio de días
            string day = GetRandomDay();

            return "#" + year + "-" + month + "-" + day + "#";
        }

        private static string GetRandomHour()
        {
            var chars = "0123456789";
            var random = new Random();
            int length = 1;
            var numberChars = new char[length];

            for (int i = 0; i < numberChars.Length; i++)
            {
                numberChars[i] = chars[random.Next(chars.Length)];
            }

            string hour = new String(numberChars);

            if (hour == "0" || hour == "1" || hour == "2")
            {
                int dec_month = random.Next(1);
                hour = dec_month.ToString() + hour;
            }

            return hour;
        }

        private static string GetRandomMinute()
        {
            var chars = "0123456789";
            var random = new Random();
            int length = 1;
            var numberChars = new char[length];

            for (int i = 0; i < numberChars.Length; i++)
            {
                numberChars[i] = chars[random.Next(chars.Length)];
            }

            string minute = new String(numberChars);

            int dec_minute = random.Next(5);

            minute = dec_minute.ToString() + minute;

            return minute;
        }

        private static string GetRandomDay()
        {
            var chars = "123456789";
            var random = new Random();
            int length = 1;
            var numberChars = new char[length];

            for (int i = 0; i < numberChars.Length; i++)
            {
                numberChars[i] = chars[random.Next(chars.Length)];
            }

            string day = new String(numberChars);

            int dec_day = -1;

            if (day == "9")
            {
                dec_day = random.Next(1);
            }
            else
            {
                dec_day = random.Next(2);
            }

            day = dec_day.ToString() + day;

            return day;
        }

        private static string GetRandomMonth()
        {
            var chars = "123456789";
            var random = new Random();
            int length = 1;
            var numberChars = new char[length];

            for (int i = 0; i < numberChars.Length; i++)
            {
                numberChars[i] = chars[random.Next(chars.Length)];
            }

            string month = new String(numberChars);

            if (month == "0" || month == "1" || month == "2")
            {
                int dec_month = random.Next(1);
                month = dec_month.ToString() + month;
            }

            return month;
        }

        private static string GetRandomYear()
        {
            var chars = "0123456789";
            var random = new Random();
            int length = 2;
            var numberChars = new char[length];

            for (int i = 0; i < numberChars.Length; i++)
            {
                numberChars[i] = chars[random.Next(chars.Length)];
            }

            string year = new String(numberChars);

            year = "20" + year;

            return year;
        }

        internal static string GetRandomString(int max_length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            
            int length = random.Next(Math.Min(max_length, 512) - 1) + 1;
            var stringChars = new char[length];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            string finalString = new String(stringChars);
            return '"' + finalString + '"';
        }

        internal static int GetRandomNumber(int max_length)
        {
            var chars = "0123456789";
            var random = new Random();
            int length = random.Next(max_length - 1) + 1;
            var numberChars = new char[length];

            for (int i = 0; i < numberChars.Length; i++)
            {
                numberChars[i] = chars[random.Next(chars.Length)];
            }

            string aux = new String(numberChars);
            int finalInt;
            if (Int32.TryParse(aux, out finalInt))
            {
                return finalInt;
            }
            else
            {
                return 0;
            }

        }

        internal static string GetRandomNumber(int max_length, int decimal_max_length)
        {
            var chars = "0123456789";
            var random = new Random();
            int length = 0;
            if (decimal_max_length > 0)
            {
                if (max_length - decimal_max_length - 1 > 0)
                    length = random.Next(max_length - decimal_max_length - 1) + 1;
                else
                    length = 0;
            }
            else
            {
                length = random.Next(max_length - 1) + 1;
            }

            int dec_lenght = random.Next(decimal_max_length - 1) + 1;
            string aux = "";
            if (length != 0)
            {
                var numberChars = new char[length];
                for (int i = 0; i < numberChars.Length; i++)
                {
                    numberChars[i] = chars[random.Next(chars.Length)];
                }
                aux = new String(numberChars);
            }
            else
            {
                aux = "0";
            }
            
            var numberDecChars = new char[dec_lenght];
            for (int i = 0; i < numberDecChars.Length; i++)
            {
                numberDecChars[i] = chars[random.Next(chars.Length)];
            }            
            string aux2 = new String(numberDecChars);
            if (dec_lenght > 0)
            {
                aux += "." + aux2;
            }
            string finalInt = aux;
            return finalInt;
        }

        internal static void AttributeAsOutput(KnowledgeBase KB, KBObject obj, out List<string[]> output_list)
        {
            output_list = new List<string[]>();
            foreach (EntityReference reference in obj.GetReferencesTo())
            {
                KBObject objRef = KBObject.Get(KB.DesignModel, reference.From);
                if(objRef is Procedure)
                {
                    Procedure proc = (Procedure)objRef;
                    ICallableObject callableObject = proc as ICallableObject;
                    if (callableObject != null)
                    {
                        foreach (Signature signature in callableObject.GetSignatures())
                        {
                            foreach (Parameter parm in signature.Parameters)
                            {
                                string accessor = parm.Accessor.ToString();
                                if (accessor == "PARM_OUT" || accessor == "PARM_INOUT")
                                { 
                                    if(obj is Attribute)
                                    {
                                        if (parm.IsAttribute)
                                        {
                                            Attribute att_obj = (Attribute)parm.Object;
                                            if (att_obj.QualifiedName.ToString() == obj.QualifiedName.ToString())
                                            {
                                                ShowOutputAttributeMessages(obj, proc, ref output_list);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            Variable var_obj = (Variable)parm.Object;
                                            if(var_obj.AttributeBasedOn != null)
                                            { 
                                                if(var_obj.AttributeBasedOn.QualifiedName.ToString() == ((Attribute)obj).QualifiedName.ToString())
                                                {
                                                    ShowOutputAttributeMessages(obj, proc, ref output_list);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if(obj is Domain)
                                    { 
                                        if(parm.IsAttribute)
                                        {
                                            Attribute att_obj = (Attribute)parm.Object;
                                            if(att_obj.DomainBasedOn != null)
                                            {
                                                if (att_obj.DomainBasedOn.QualifiedName.ToString() == ((Domain)obj).QualifiedName.ToString())
                                                {
                                                    ShowOutputAttributeMessages(obj, proc, ref output_list);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Variable var_obj = (Variable)parm.Object;
                                            if (var_obj.DomainBasedOn != null) { 
                                                if (var_obj.DomainBasedOn.QualifiedName.ToString() == ((Domain)obj).QualifiedName.ToString())
                                                {
                                                    ShowOutputAttributeMessages(obj, proc, ref output_list);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ShowOutputAttributeMessages(KBObject obj, Procedure proc, ref List<string[]> output_list)
        {
            
            string parm_rule = Utility.ExtractRuleParm(proc);
            KBDoctorOutput.Message("PROCEDURE:     " + proc.Name.ToString());
            KBDoctorOutput.Message("DESCRIPTION:   " + proc.Description);
            OutputError oe = new OutputError(parm_rule, MessageLevel.Information, new SourcePosition(proc.Parts.Get<RulesPart>(), 1, 0));
            KBDoctorOutput.OutputError(oe);
            KBDoctorOutput.Message(" ----------------------------------------------- ");
            string[] line = new string[] { Utility.linkObject(proc), proc.Description, parm_rule };
            output_list.Add(line);
        }

        internal static void CheckBldObjects(KnowledgeBase KB, Dictionary<string, KBObject> hash_mains)
        {
            string KBLocation = KB.Location;
            string targetPath = KB.DesignModel.Environment.TargetModel.TargetPath;
            string blddirectorypath = KBLocation + @"\" + targetPath + @"\web";
            string[] bldspaths = Directory.GetFiles(blddirectorypath, "bld*.cs", SearchOption.TopDirectoryOnly);
            foreach(string bldpath in bldspaths)
            {
                BldObjectInKB(KB, bldpath, hash_mains);
            }
        }

        private static void BldObjectInKB(KnowledgeBase KB, string bldpath, Dictionary<string, KBObject> hash_mains)
        {
            string filename = Path.GetFileName(bldpath);
            string parsedname = filename.Substring(3);
            parsedname = parsedname.Substring(0, parsedname.Length - 3);

            string module = "";
            string name = "";
            string name_alt = "";

            if (parsedname.Contains("-"))
            {
                string[] splits = parsedname.Split('-');
               
                bool first = true;
                foreach(string split in splits)
                {
                    if(first)
                    {
                        name = split;
                        first = false;
                    }
                    else
                    {
                        module += name + ".";
                        name = split;
                    }
                }
            }
            else
            {
                 name = parsedname;
            }
            if (name.StartsWith("a"))
            {
                name_alt = name.Substring(1);
            }
            if(module.EndsWith("."))
            {
                module = module.Substring(0, module.Length - 1);
            }

            QualifiedName qname = new QualifiedName(module, name);
            KBObject obj = Utility.GetObjectByQName(KB.DesignModel, qname, hash_mains);

            qname = new QualifiedName(module, name_alt);
            KBObject obj_alt = Utility.GetObjectByQName(KB.DesignModel, qname, hash_mains);

            if (obj == null)
            {
                if (obj_alt == null)
                {
                    if(module != "")
                    {
                        KBDoctorOutput.Warning("Object " + module + "." + name + " doesn't exists");
                    }
                    else
                    {
                        KBDoctorOutput.Warning("Object " + name + " doesn't exists");
                    }
                    
                }
            }
        }

        internal static Dictionary<string, KBObject> GetHashMainObjectsQNames(KBModel model)
        {
            Dictionary<string, KBObject> hash_ret = new Dictionary<string, KBObject>();
            KBCategory mainCategory = Utility.MainCategory(model);
            foreach (KBObject obj in mainCategory.AllMembers)
            {
                hash_ret.Add(obj.QualifiedName.ToString().ToLower(), obj);             
            }
            return hash_ret;
        }

#if EVO3
    public class Tuple<T1, T2>
    {
        public T1 Item1 { get; internal set; }
        public T2 Item2 { get; internal set; }
        internal Tuple(T1 first, T2 second)
        {
            Item1 = first;
            Item2 = second;
        }
    }

    public class Tuple<T1, T2, T3>
        {
            public T1 Item1 { get; internal set; }
            public T2 Item2 { get; internal set; }
            public T3 Item3 { get; internal set; }
            internal Tuple(T1 first, T2 second, T3 third)
            {
                Item1 = first;
                Item2 = second;
                Item3 = third;
            }
        }

    public class Tuple
    {
        public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second)
        {
            var tuple = new Tuple<T1, T2>(first, second);
            return tuple;
        }
        public static Tuple<T1, T2,T3> New<T1, T2,T3>(T1 first, T2 second, T3 third)
            {
                var tuple = new Tuple<T1, T2, T3>(first, second, third);
                return tuple;
            }
        }
#endif
    }
}
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.ComponentModel;
using Artech.Architecture.Language.Parser;
using Artech.Architecture.Language.Services;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Windows.Media;

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
                        List<TokensIds>[] IndentTokens = GetIndentationTokens()
                    }
                    else
                    {
                        //Token

                    }
                }
            }

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
            //TODO - ARREGLAR TUPLE 
            string paramstring = "";
            /* 
             * Tuple<int, string> type_access;
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
            */
            return paramstring.TrimEnd();
        }
    }
}

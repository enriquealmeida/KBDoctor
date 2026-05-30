using System;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using Artech.Udm.Framework.References;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Services;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.SDT;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Helpers;
using Artech.Genexus.Common.CustomTypes;

using Artech.Common.Diagnostics;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.UI.Framework.Objects;
using Artech.Architecture.Language.Services;
using Artech.Architecture.Language.Parser;
using Artech.Architecture.Common;
using Artech.Architecture.Common.Services;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Collections;
using Artech.Architecture.BL.Framework.Services;
using GeneXus.Server.Contracts;
using Artech.Architecture.Language.ComponentModel;

using Artech.Udm.Framework;
using Concepto.Packages.KBDoctorCore.Sources;
using System.Threading;
using Artech.Genexus.Common.Parts.WebForm;
using API = Concepto.Packages.KBDoctorCore.Sources.API;
using Concepto.Packages.KBDoctor.Sources;

using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.Parts.Form;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Packages.Patterns.Objects;
using Artech.Patterns.WorkWithDevices.Helpers;
using Artech.Patterns.WorkWithDevices;
using Artech.Common.Helpers.Reflection;


namespace Concepto.Packages.KBDoctor
{
    partial class ObjectsHelper
    {
        internal static void TreeCommit()
        {
            // Object with parm() rule without in: out: or inout:
            IKBService kbserv = UIServices.KB;
            IOutputService output = CommonServices.Output;
            string title = "KBDoctor - Tree Commit  ";

            output.StartSection("KBDoctor", title);
                string outputFile = Functions.CreateOutputFile(kbserv, title);

                KBDoctorXMLWriter writer = new KBDoctorXMLWriter(outputFile, Encoding.UTF8);

                writer.AddHeader(title);
                writer.AddTableHeader(new string[] { "Object", "Commit", "Description", "in New UTL", "Upd/Ins/Del", "TimeStamp", "Modified Tables" });


                string Anidacion = "";
                KBObjectCollection yaIncluidos = new KBObjectCollection();

                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = true;
                KBModel kbModel = UIServices.KB.CurrentModel;

                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Procedure>());

                foreach (Procedure obj in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))

                {
                    GraboLlamado(obj, Anidacion, yaIncluidos, writer);
                }


                writer.AddFooter();
                writer.Close();

                bool success = true;
                output.EndSection("KBDoctor", title, success);

                KBDoctorHelper.ShowKBDoctorResults(outputFile);

        }

        private static void GraboLlamado(Procedure obj, string Anidacion, KBObjectCollection yaIncluido, KBDoctorXMLWriter writer)
        {

            if (yaIncluido.Contains(obj) || !(obj is Procedure))
            {
                writer.AddTableData(new string[] { Anidacion + Functions.linkObject(obj), "", "----already included ", "", "", "", "" });
            }
            else
            {

                string commitOnExit = "";
                string commitInSource = "";
                string UpdateInsertDelete = "";
                string doCommit = "";
                object aux = obj.GetPropertyValue("CommitOnExit");
                if (aux != null)
                {
                    commitOnExit = aux.ToString() == "Yes" ? "YES" : " ";
                }
                UpdateInsertDelete = CleanKBHelper.ObjectUpdateDB(obj) ? "YES" : "";

                Procedure prc = (Procedure)obj;
                string source = "";
                try
                {
                    source = Functions.ExtractComments(prc.ProcedurePart.Source.ToString().ToUpper());
                }
                catch (Exception e)
                    { KBDoctorOutput.Error("ERROR Grabollamado:" + e.Message); };

                if (source.Contains("COMMIT"))
                    commitInSource = "YES";
                else
                    commitInSource = "";

                if ((commitOnExit == "YES" && UpdateInsertDelete == "YES") || (commitInSource == "YES"))
                {
                    doCommit = "YES";
                };

                string ExecuteInNewLuw = "";
                object aux2 = obj.GetPropertyValue(Properties.PRC.ExecuteInNewLuw);
                if (aux2 != null)
                {
                    ExecuteInNewLuw = aux2.ToString() == "True" ? "YES" : "";
                }

                KBModel model = obj.Model;
                IList<KBObject> tableUpdInsDel = (from r in model.GetReferencesFrom(obj.Key, LinkType.UsedObject)
                                                  where r.ReferenceType == ReferenceType.WeakExternal // las referencias a tablas que agrega el especificador son de este tipo
                                                  where (ReferenceTypeInfo.HasUpdateAccess(r.LinkTypeInfo) | ReferenceTypeInfo.HasDeleteAccess(r.LinkTypeInfo) | ReferenceTypeInfo.HasInsertAccess(r.LinkTypeInfo))
                                                  select model.Objects.Get(r.To)).ToList();

                string tblList = "";
                string coma = "";
                foreach (KBObject objref in tableUpdInsDel)
                {
                    tblList += coma + Functions.linkObject(objref);
                    coma = ",";
                }
                writer.AddTableData(new string[] { Anidacion + Functions.linkObject(obj), doCommit, obj.Description, ExecuteInNewLuw, UpdateInsertDelete, obj.Timestamp.ToShortDateString(), tblList });


                yaIncluido.Add(obj);

                Parse(obj.Model, obj, Anidacion, yaIncluido, writer);

            }
        }

        static public void Parse(KBModel modelo, KBObject obj, string Anidacion, KBObjectCollection yaIncluido, KBDoctorXMLWriter writer)
        {
            ILanguageService parserSrv = Artech.Architecture.Common.Services.Services.GetService(new Guid("C26F529E-9A69-4df5-B825-9194BA3983A3")) as ILanguageService;
            IParserEngine parser = parserSrv.CreateEngine();
            ParserInfo parserInfo;

            // Para parsear los eventos de un Procedure
            Artech.Genexus.Common.Parts.ProcedurePart source = obj.Parts.Get<Artech.Genexus.Common.Parts.ProcedurePart>();
            if (source != null)
            {
                parserInfo = new ParserInfo(source);
                
                int FCLobjClass = 0;
                string FCIobjName = "";
                foreach (TokenData token in parser.GetTokens(true, parserInfo, source.Source))
                {

                    if (token.Token == (int)TokensIds.FCL)
                        FCLobjClass = token.Id;
                    if (token.Token == (int)TokensIds.FOI)
                    {
                        FCIobjName = token.Word;
                        KBDoctorOutput.Message( Anidacion + FCLobjClass.ToString() + "-" + FCIobjName);

                        EntityKey objKey = new EntityKey(ObjClass.Procedure, token.Id);
                        KBObject objRef = KBObject.Get(obj.Model, objKey);
                        if (objRef != null)
                             GraboLlamado((Procedure)objRef, Anidacion + "____", yaIncluido, writer);
                    }
                    if (token.Token == (int)TokensIds.DTCMM)
                        writer.AddTableData(new string[] { Anidacion + " COMMIT ", "", "Commit Explicito", "", "", "", "", "" });


                }
            }
        }

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
    }
}

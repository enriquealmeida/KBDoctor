﻿using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Architecture.Common.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Artech.Udm.Framework.References;
using Artech.Common.Framework.Selection;
using Artech.Common.Framework.Commands;
using Artech.Genexus.Common.Types;
using Artech.Architecture.Common.Descriptors;
using Artech.Genexus.Common.Services;
using Artech.Architecture.BL.Framework.Services;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Parts.WebForm;
using Artech.Architecture.UI.Framework.Helper;
using Artech.Common.Diagnostics;
using Artech.Genexus.Common.Parts.Layout;
using Artech.Packages.Patterns;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;
using Artech.Genexus.Common.Parts.SDT;

namespace Concepto.Packages.KBDoctor
{
    static class CodeGeneration
    {

        public static void CreateProcedureGetSet()
        {
            IKBService kB = UIServices.KB;
            if (kB != null && kB.CurrentModel != null)
            {
                SelectObjectOptions selectObjectOption = new SelectObjectOptions();
                selectObjectOption.MultipleSelection = false;
                KBModel kbModel = UIServices.KB.CurrentModel;

                selectObjectOption.ObjectTypes.Add(KBObjectDescriptor.Get<Table>());
                //List<Table> tablas = UIServices.SelectObjectDialog.SelectObjects(selectObjectOption) as List<Table>;
                foreach (Table tabla in UIServices.SelectObjectDialog.SelectObjects(selectObjectOption))
                {


                    string title = "KBDoctor - Generating Get/Set/Insert/Delete for table " + tabla.Name;
                    IOutputService output = CommonServices.Output;
                    output.StartSection("KBDoctor",title);

                    try
                    {

                        output.AddLine("KBDoctor"," Generating SDT");
                        SDT Sdt = GenerateSdt(tabla);

                        output.AddLine("KBDoctor"," Generating Procedure GET");
                        GenerateGetProcedure(kbModel, tabla, Sdt);

                        output.AddLine("KBDoctor"," Generating Procedure SET");
                        GenerateSetProcedure(kbModel, tabla, Sdt);

                        output.AddLine("KBDoctor"," Generating Procedure INSERT");
                        GenerateInsertProcedure(kbModel, tabla, Sdt);

                        output.AddLine("KBDoctor"," Generating DataProvider");
                        GenerateDataProvider(kbModel, tabla, Sdt);

                        output.AddLine("KBDoctor"," Generating Exist");
                        GenerateExistProcedure(kbModel, tabla, Sdt);
                    }
                    catch (Exception ex)
                    {
                        output.AddErrorLine("KBDoctor",ex);
                    }
                    Application.DoEvents();
                    output.EndSection("KBDoctor", title, true);

                }

            }


       }


        private static SDT GenerateSdt(Table tabla)
        {
            SDT Sdt = SDT.Create(UIServices.KB.CurrentModel);
            Sdt.Name = "SDT_" + tabla.Name ;

            Sdt.Description = tabla.Description;

            // Añadir los atributos de la tabla.

            foreach (TableAttribute atr in tabla.TableStructure.Attributes)
            {
                SDTItem item = new SDTItem(Sdt.SDTStructure);
                item.Name = atr.Name;
                item.Description = atr.Attribute.Description;
                item.AttributeBasedOn = atr;
                Sdt.SDTStructure.Root.AddItem(item);
            }
            Sdt.Save();
            return Sdt;
        }

        private static void GenerateGetProcedure(KBModel kbModel, Table tabla, SDT Sdt)
        {
            Artech.Genexus.Common.Objects.Procedure proc = new Artech.Genexus.Common.Objects.Procedure(kbModel);
            string procName = "GET_" + tabla.Name;

            proc.Name = procName;


            AddSDTVariable(kbModel, proc, Sdt);

            string Source = "// Generated by KBDoctor " + DateTime.Now.ToString() + Environment.NewLine;
            Source += GenerateForEachToSDT(tabla, Sdt);
            proc.ProcedurePart.Source = Source;

            proc.Rules.Source = GenerateParmRule(tabla, Sdt,"out:");
            AddVariables(kbModel, proc, tabla);
            proc.Description = "Get for Table " + tabla.Name + ".";
            proc.Save();
        }


    

        private static void GenerateSetProcedure(KBModel kbModel, Table tabla, SDT Sdt)
        {
            Artech.Genexus.Common.Objects.Procedure proc = new Artech.Genexus.Common.Objects.Procedure(kbModel);
            string procName = "SET_" + tabla.Name;

            proc.Name = procName;
            string Source = "// Generated by KBDoctor " + DateTime.Now.ToString() + Environment.NewLine;


            AddSDTVariable(kbModel, proc, Sdt);
            Source += GenerateForEachFromSDT(tabla, Sdt);
            proc.ProcedurePart.Source = Source;
            proc.Rules.Source = GenerateParmRule(tabla, Sdt,"in:");
            AddVariables(kbModel, proc, tabla);
            proc.Description = "Set for Table " + tabla.Name + ".";
            proc.Save();
        }

        private static void AddVariables(KBModel kbModel, Procedure proc, Table tabla)
        {

            foreach (TableAttribute attr in tabla.TableStructure.Attributes)
            {
                Variable oVariableNew = new Variable(proc.Variables);
                oVariableNew.Name = attr.Name;
                oVariableNew.AttributeBasedOn = attr;

                DataType.ParseInto(kbModel, attr.Name, oVariableNew);

                proc.Variables.Add(oVariableNew);
            }
        }

        private static string GenerateParmRule(Table tabla, SDT sdt, string INorOut)
        {
            string Rules = "parm(";
            foreach (TableAttribute pk in tabla.TableStructure.PrimaryKey)
            {
                Rules +=  "in:&" + pk.Name + ",";

            }
            Rules += INorOut + "&" + sdt.Name + ");";
            return Rules;
        }

        private static string GenerateParmRuleINSDT(string sdtName)
        {
            string Rules = "parm(IN:&" + sdtName + ");";
            return Rules;
        }

        private static string GenerateNewFromSDT(Table tabla, SDT sdt)
        {
            string Source = "New" + Environment.NewLine;
            string Comment = "/*" + Environment.NewLine;
            

            foreach (TableAttribute atr in tabla.TableStructure.Attributes)
            {    
                if (!atr.IsFormula)
                    Source += "          " + atr.Name + " = &" + sdt.Name + "." + atr.Name + Environment.NewLine;
                Comment += " &" + sdt.Name + "." + atr.Name + " = " + atr.Name; 
                    
            }
            Source += "when duplicate /*nothing*/ " +  Environment.NewLine;
            Source += "endnew" + Environment.NewLine;

            Source += Comment + Environment.NewLine + "*/";

            return Source;
        }

        private static string GenerateForEachFromSDT(Table tabla, SDT sdt)
        {
            string Source = "for each" + Environment.NewLine;
                Source += GenerateWhere(tabla);
                Source += GenerateAssignFromSDT(tabla, sdt);
            Source += "endfor" + Environment.NewLine;

            return Source;
        }


        private static string GenerateForEachToSDT(Table tabla, SDT sdt)
        {
            string Source = "for each" + Environment.NewLine;
               Source += GenerateWhere(tabla);
               Source += GenerateAssignToSDT(tabla, sdt);
            Source += "endfor" + Environment.NewLine;

            return Source;
        }

        private static string GenerateAssignToSDT(Table tabla, SDT sdt)
        {
            String Source = Environment.NewLine;
            foreach (TableAttribute atr in tabla.TableStructure.Attributes)
            {
                if (!atr.IsKey) 
                    Source += "          &" + sdt.Name + "." + atr.Name + " = " + atr.Name + Environment.NewLine;
            }
            Source += Environment.NewLine;
            return Source;
        }

        private static string GenerateAssignFromSDT(Table tabla, SDT sdt)
        {
            String Source = Environment.NewLine;
            foreach (TableAttribute atr in tabla.TableStructure.Attributes)
            {
                if (!atr.IsKey)
                      Source += "          "  + atr.Name + " = &"  + sdt.Name + "." + atr.Name + Environment.NewLine;
            }
            Source += Environment.NewLine;
            return Source;
        }

        private static string GenerateWhere(Table tabla)
        {
            string Source = "";
            foreach (TableAttribute pk in tabla.TableStructure.PrimaryKey)
            {
                Source += "      where " + pk.Name + " = &" + pk.Name + Environment.NewLine;
            }

            return Source;
        }

        private static string GenerateWhereSDT(Table tabla, string sdtname)
        {
            string Source = "";
            foreach (TableAttribute pk in tabla.TableStructure.PrimaryKey)

            {
                string attName = pk.Name;
                Source += "      where " + attName + " = &" + sdtname + "." + attName + " when not &" + sdtname + "." + attName + ".IsEmpty() " + Environment.NewLine;
            }

            return Source;
        }

        private static string GenerateListaAtt(Table tabla)
        {
            string Source = "";
            foreach (TableAttribute att in tabla.TableStructure.Attributes)
            {
                Source += "       " + att.Name + Environment.NewLine;
            }

            return Source;
        }



        public static void AddSDTVariable(KBModel kbModel, Artech.Genexus.Common.Objects.Procedure proc, SDT sdtobj)
        {
            Variable oVariableNew = new Variable(proc.Variables);
            oVariableNew.Name = sdtobj.Name;
            oVariableNew.Type = eDBType.GX_SDT;

            DataType.ParseInto(kbModel, sdtobj.Name, oVariableNew);

            proc.Variables.Add(oVariableNew);
        }

        public static void AddSDTVariableToDP(KBModel kbModel, DataProvider dp, SDT sdtobj, string sdtname)
        {
            Variable oVariableNew = new Variable(dp.Variables);
            oVariableNew.Name = sdtname;
            oVariableNew.Type = eDBType.GX_SDT;

            DataType.ParseInto(kbModel, sdtobj.Name, oVariableNew);

            dp.Variables.Add(oVariableNew);
        }
        
        private static void GenerateInsertProcedure(KBModel kbModel, Table tabla, SDT Sdt)
        {
            Artech.Genexus.Common.Objects.Procedure proc = new Artech.Genexus.Common.Objects.Procedure(kbModel);
            string procName = "NEW_" + tabla.Name;

            proc.Name = procName;
            string Source = "// Generated by KBDoctor " + DateTime.Now.ToString() + Environment.NewLine;


            AddSDTVariable(kbModel, proc, Sdt);
            Source += GenerateNewFromSDT(tabla, Sdt);
            proc.ProcedurePart.Source = Source;
            proc.Rules.Source = GenerateParmRuleINSDT(Sdt.Name);
            AddVariables(kbModel, proc, tabla);
            proc.Description = "Insert for Table " + tabla.Name + ".";
            proc.Save();
        }
        

        private static void GenerateDataProvider(KBModel kbModel, Table tabla, SDT Sdt)
        {
            Artech.Genexus.Common.Objects.DataProvider dp = new Artech.Genexus.Common.Objects.DataProvider(kbModel);
            string dpName = "DP_" + tabla.Name;
            string sdtparam = "Param_" + Sdt.Name;

            dp.Name = dpName;
            string Source;

            Source = Sdt.Name + Environment.NewLine;
           
            Source += GenerateWhereSDT(tabla, sdtparam);
            Source += "{" + Environment.NewLine;
            Source += GenerateListaAtt(tabla);
            Source += "}";

            dp.DataProviderSource.Source = Source;
            
            AddSDTVariableToDP(kbModel, dp , Sdt, sdtparam);
            AddSDTVariableToDP(kbModel, dp, Sdt, Sdt.Name);

            dp.Rules.Source = GenerateParmRuleINSDT(sdtparam);
            
            dp.Description = "Data Provider for table  " + tabla.Name + ".";

            dp.SetPropertyValue(Properties.DPRV.Output, new KBObjectReference(Sdt));
            dp.SetPropertyValue(Properties.DPRV.Collection, true);
            dp.Save();

           // KBObject obj = kbModel.Objects.GetByName("Objects",,"DP_DSASOC")
        }

        private static void GenerateExistProcedure(KBModel kbModel, Table tabla, SDT sdt)
        {
            Artech.Genexus.Common.Objects.Procedure proc = new Artech.Genexus.Common.Objects.Procedure(kbModel);
            string procName = "EXISTS_" + tabla.Name;

            AddSDTVariable(kbModel, proc, sdt);

            proc.Name = procName;
            string Source;
            
            Source = "for each" + Environment.NewLine;
            Source += GenerateWhere(tabla);
            Source += "         &Exists=true" + Environment.NewLine;
            Source += "     when none " + Environment.NewLine;
            Source += "         &Exists=false" + Environment.NewLine;
            Source += "endfor" + Environment.NewLine;

            proc.ProcedurePart.Source = Source;

            string Rules = "parm(";
            foreach (TableAttribute pk in tabla.TableStructure.PrimaryKey)
            {
                Rules += "in:&" + pk.Name + ",";

            }
            Rules += "out:&Exists);";
            proc.Rules.Source = Rules;
            AddVariables(kbModel, proc, tabla);

            Variable oVariableNew = new Variable(proc.Variables);
            oVariableNew.Name = "Exists";
            oVariableNew.Type = eDBType.Boolean;

            proc.Variables.Add(oVariableNew);
            proc.Description = "Exists record in Table " + tabla.Name + ".";
            proc.Save();
        }

    }
}

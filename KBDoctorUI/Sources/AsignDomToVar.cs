using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Services;
using Concepto.Packages.KBDoctorCore.Sources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
//using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace Concepto.Packages.KBDoctor
{
    public partial class AssignDomToVar : Form
    {
        public AssignDomToVar(KBObject obj)
        {
            InitializeComponent();

            objName.Text = obj.Name + " (" + obj.TypeDescriptor.Name + "): " + obj.Description;
            objSource = ObjectsHelper.ObjectSource(obj);
            objRules = obj.Parts.Get<RulesPart>().Source;

            model = obj.Model;


            VariablesPart vp = obj.Parts.Get<VariablesPart>();
            if (vp != null)
            {

                foreach (Variable v in vp.Variables)
                {
                    if ((!v.IsStandard) && (v.AttributeBasedOn == null) && (v.DomainBasedOn == null) && (v.Type != eDBType.GX_USRDEFTYP)
                        && (v.Type != eDBType.GX_SDT) && (v.Type != eDBType.GX_EXTERNAL_OBJECT) && (v.Type != eDBType.Boolean) && v.Type != eDBType.GX_BUSCOMP && v.Type != eDBType.GX_BUSCOMP_LEVEL)
                    {
                        comboVar.Items.Add(new Item(v.Name + "  " + Utility.FormattedTypeVariable(v), v));
                       
                    }
                    cantVariables.Text = comboVar.Items.Count.ToString() + " not based variables";
                }


            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) //, KBObject obj, string vName)
        {
            
            Item it = (Item)comboVar.SelectedItem;
            Variable v2 = it.Var;
            string toSearch = @"&" + v2.Name +@"\b";

            string[] textLines = objSource.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            string tt = "";
            foreach (string line in textLines)
            {
                if (Regex.IsMatch(line, toSearch, RegexOptions.IgnoreCase) )
                    tt += line.Trim() + Environment.NewLine;
               // Match m = Regex.Match(line, toSearch, RegexOptions.IgnoreCase);
            }

            string rr = "";
            textLines = objRules.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string line in textLines)
            {
                if (Regex.IsMatch(line, toSearch, RegexOptions.IgnoreCase ))
                    rr += line.Trim() + Environment.NewLine;
            }

            source.Text = "====== SOURCE ===== " + Environment.NewLine + tt  +Environment.NewLine +  "==== Rules =====" + Environment.NewLine + rr;
        }


        public string vName = "";
        public string objSource = "";
        public string objRules = "";
        public Variable v2;
        public KBModel model;

        private class Item
        {
            public string Name;
            public Variable Var;
            public Item(string name, Variable var)
            {
                Name = name; Var = var;
            }
            public override string ToString()
            {
                return Name;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Item it = (Item)comboVar.SelectedItem;
            if (it != null)
            {
                Variable v3 = it.Var;
                if (v3 != null)
                {

                    AttributeVariableDialogInfo info = new AttributeVariableDialogInfo();
                    info.Filter = TypedObjectKind.Attribute | TypedObjectKind.Domain;
                    info.DialogTitle = "Select domain/Attribute";
                    info.MultiSelection = false;
                    IList<object> selectedAtts = GenexusUIServices.SelectAttributeVariable.SelectAttributeVariable(info);
                    if (selectedAtts.Count > 0)
                    {
                        Domain dom = null;
                        try
                        {
                            dom = (Domain)selectedAtts[0];
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine("{0} Exception caught.", ee);
                            dom = null;
                        };

                        if (dom != null)
                        {
                            v3.DomainBasedOn = dom;
                            v3.KBObject.Save();
                            comboVar.Items.Remove(it);
                            cantVariables.Text = comboVar.Items.Count.ToString() + " not based variables";
                            source.Text = "";
                            if (comboVar.Items.Count == 0)
                            {
                                this.Close();
                                return;
                            }
                            else
                            {
                                comboVar.SelectedIndex = 0;
                            }
                            comboVar.SelectedIndex = 0;
                        }
                        else
                        {
                            Artech.Genexus.Common.Objects.Attribute att = (Artech.Genexus.Common.Objects.Attribute)selectedAtts[0];
                            if (att != null)
                            {
                                v3.AttributeBasedOn = att;
                                v3.KBObject.Save();
                                comboVar.Items.Remove(it);
                                cantVariables.Text = comboVar.Items.Count.ToString() + " not based variables";
                                source.Text = "";
                                if (comboVar.Items.Count == 0)
                                {
                                    this.Close();
                                    return;
                                }
                                else
                                {
                                    comboVar.SelectedIndex = 0;
                                }
                            }
                        }
                    }
     
                }
            }
            else
            {
                //Debe seleccionar una variable
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Domain obj = new Domain(model);
            CreateObjectOptions opt = new CreateObjectOptions();
            opt.Name = "NewDomain";
            opt.Type = obj.TypeDescriptor;
            INewObjectDialogService j = UIServices.NewObjectDialog;
            Domain NewDom = (Domain)j.CreateObject(opt);
        }
    }
}

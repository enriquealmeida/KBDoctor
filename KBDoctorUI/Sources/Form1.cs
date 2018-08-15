using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using Concepto.Packages.KBDoctorCore.Sources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace Concepto.Packages.KBDoctor
{
    public partial class Form1 : Form
    {
        public Form1(KBObject obj)
        {
            InitializeComponent();

            objName.Text = obj.Name + " : " + obj.Description;
            objSource = ObjectsHelper.ObjectSource(obj);
            objRules = obj.Parts.Get<RulesPart>().Source;

            //.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

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

                }
              //  comboVar.SelectedIndex = 1;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) //, KBObject obj, string vName)
        {
            string tt = "";
            Item it = (Item)comboVar.SelectedItem;
            Variable v2 = it.Var;
            string toSearch = "&" + v2.Name;
            string[] textLines = objSource.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string line in textLines)
            {
                if (line.ToUpper().Contains(toSearch.ToUpper()))
                    tt += line.Trim() + Environment.NewLine;
            }
            string rr = "";
            textLines = objRules.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string line in textLines)
            {
                if (line.ToUpper().Contains(toSearch.ToUpper()))
                    rr += line.Trim() + Environment.NewLine;
            }

            source.Text = "====== SOURCE ===== " + Environment.NewLine + tt  +Environment.NewLine +  "==== Rules =====" + Environment.NewLine + rr;
            
            //Falta cargar los combos con atributos y dominios de tipo compatible. 
            
        }


        public string vName = "";
        public string objSource = "";
        public string objRules = "";

        private void button1_Click(object sender, EventArgs e)
        {

        }

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
                // Generates the text shown in the combo box
                return Name;
            }
        }
    }
}

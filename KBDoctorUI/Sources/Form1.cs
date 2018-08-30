using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
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
            o2 = obj;

            KBModel model = obj.Model;

            foreach (Artech.Genexus.Common.Objects.Attribute a in Artech.Genexus.Common.Objects.Attribute.GetAll(model))
            {
                attList.Add(new ItemAtt(Utility.FormattedTypeAttribute(a) + "  " + a.Name  , a));

            }

            foreach (Domain d in Artech.Genexus.Common.Objects.Domain.GetAll(model))
            {
                domList.Add(new ItemDom(Utility.FormattedTypeDomain(d)  + "   " + d.Name , d));

            }

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

            comboAtt.Items.Clear();
            foreach (ItemAtt ita in attList)
            {
                if (ita.Att.Type == v2.Type )
                        comboAtt.Items.Add(ita);

            }

            comboDom.Items.Clear();
            foreach (ItemDom dom in domList)
            {
                if (dom.Dom.Type == v2.Type)
                    comboDom.Items.Add(dom);

            }


            //Falta cargar los combos con atributos y dominios de tipo compatible. 

        }


        public string vName = "";
        public string objSource = "";
        public string objRules = "";
        private List<ItemAtt> attList = new List<ItemAtt>();
        private List<ItemDom> domList = new List<ItemDom>();
        public KBObject o2;
        public Variable v2;

      

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

        private class ItemDom
        {
            public string Name;
            public Domain Dom;
            public ItemDom(string name, Domain dom)
            {
                Name = name; Dom = dom;
            }
            public override string ToString()
            {
                // Generates the text shown in the combo box
                return Name;
            }
        }

        private class ItemAtt
        {
            public string Name;
            public Artech.Genexus.Common.Objects.Attribute Att;
            public ItemAtt(string name, Artech.Genexus.Common.Objects.Attribute att)
            {
                Name = name; Att = att;
            }
            public override string ToString()
            {
                // Generates the text shown in the combo box
                return Name;
            }
        }

        private void AssignDom_Click(object sender, EventArgs e)
        {
            Item it = (Item)comboVar.SelectedItem;
            Variable v3 = it.Var;
            if (v3 != null)
            {
                ItemDom idom = (ItemDom)comboDom.SelectedItem;
                Domain dom = idom.Dom;
                if (dom != null)
                {
                    v3.DomainBasedOn = dom;
                    v3.KBObject.Save();
                   
                    source.Text = "";
                    comboDom.Text = "";
                    comboVar.Items.Remove(it);
                    if (comboVar.Items.Count == 0) return;

                }
            }
        }

        private void AssignAtt_Click(object sender, EventArgs e)
        {
            Item it = (Item)comboVar.SelectedItem;
            Variable v3 = it.Var;
            if (v3 != null)
            {
                ItemAtt iat = (ItemAtt)comboAtt.SelectedItem;
                Artech.Genexus.Common.Objects.Attribute at = iat.Att;
                if (at != null)
                {
                    v3.AttributeBasedOn = at;
                    v3.KBObject.Save();
                    source.Text = "";
                    comboAtt.Text = "";

                    comboVar.Items.Remove(it);
                    if (comboVar.Items.Count == 0) return;
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}

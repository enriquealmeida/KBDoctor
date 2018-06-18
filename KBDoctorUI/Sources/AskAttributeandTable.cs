using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;

namespace Concepto.Packages.KBDoctor
{
    public partial class AskAttributeandTable : Form
    {
        public AskAttributeandTable()
        {
            InitializeComponent();
        }

        private void AskAttributeandTable_Load(object sender, EventArgs e)
        {
            List<string> names = new List<string>();
            foreach (Table t in Artech.Genexus.Common.Objects.Table.GetAll(UIServices.KB.CurrentModel))
            {
                names.Add(t.Name);
            }
            names.Sort();
            comboBoxTable.Items.AddRange(names.ToArray<string>());
        }

        private void comboBoxAtt_SelectedIndexChanged(object sender, EventArgs e)
        {
            // No implementado
        }

        private void comboBoxTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxAtt.Items.Clear();
            Table t = Artech.Genexus.Common.Objects.Table.Get(UIServices.KB.CurrentModel, comboBoxTable.SelectedItem.ToString());
            List<string> names = new List<string>();
            foreach (TableAttribute attr in t.TableStructure.Attributes) 
            {
                names.Add(attr.Name);
            }
            names.Sort();
            comboBoxAtt.Items.AddRange(names.ToArray<string>());

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBoxTable.SelectedItem != null || comboBoxAtt.SelectedItem != null)
            {
                lblError.Text = "Select Table and Attribute";
            }
            else
            {
                lblError.Text = "";
                return;
            }
        }

        public string tblName { get {
                if (comboBoxTable.SelectedItem != null)
                    return comboBoxTable.SelectedItem.ToString();
                else
                    return "";
            } }
        public string attName
        {
            get
            {
                if (comboBoxAtt.SelectedItem != null)
                    return comboBoxAtt.SelectedItem.ToString();
                else
                    return "";
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}

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
                foreach (Table t in Artech.Genexus.Common.Objects.Table.GetAll(UIServices.KB.CurrentModel))
                {
                  //  description = "<a href=\"gx://?Command=fa2c542d-cd46-4df2-9317-bd5899a536eb;AssignDescriptionToTable&tblName=" + t.Description + "\">" + t.Description + "</a>";
                  //  string objNameLink = Functions.linkObject(t);

                    comboBoxTable.Items.Add(t.Name);
                }
        }

        private void comboBoxAtt_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxAtt.Items.Clear();
            Table t = Artech.Genexus.Common.Objects.Table.Get(UIServices.KB.CurrentModel, comboBoxTable.SelectedItem.ToString());
            foreach (TableAttribute attr in t.TableStructure.Attributes) 
            {
                comboBoxAtt.Items.Add(attr.Name);

            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string tableName = comboBoxTable.SelectedItem.ToString();
            string attName = comboBoxAtt.SelectedItem.ToString();
            if (tableName == "" | attName == "")
            {
                lblError.Text = "Select Table and Attribute";
            }
            else
            {
                lblError.Text = "";
                return;
            }
        }

        public string tblName { get { return comboBoxTable.SelectedItem.ToString(); } }
        public string attName { get { return comboBoxAtt.SelectedItem.ToString(); } }


    }
}

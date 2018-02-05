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
using Artech.Udm.Framework.References;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Genexus.Common;

namespace Concepto.Packages.KBDoctor
{
    public partial class ReplaceDomain : Form
    {
        public ReplaceDomain()
        {
           
            InitializeComponent();
            labelError.Text = "";
        }

        private void ReplaceDomain_Load(object sender, EventArgs e)
        {
            foreach (Domain d in Domain.GetAll(UIServices.KB.CurrentModel))
            {
                comboBoxOriginalDomain.Items.Add(d.Name);
                comboBoxUnifiy.Items.Add(d.Name);
            }

            comboBoxOriginalDomain.Sorted = true;
            comboBoxUnifiy.Sorted = true;
            
        }



        private void button1_Click(object sender, EventArgs e)
        {

            string originalDomainName = comboBoxOriginalDomain.SelectedItem.ToString();
            string UnifyDomainName = comboBoxUnifiy.SelectedItem.ToString();
            if (originalDomainName == "" || UnifyDomainName == "")
            {
                labelError.Text = "Select Original and Replace Domain";
            }
            else
            {
                labelError.Text = "";
                this.Close();
                return;
            }

          
        }



        private void comboBoxOriginalName_SelectedIndexChanged(object sender, EventArgs e)
        {
            Domain od = Functions.DomainByName(comboBoxOriginalDomain.SelectedItem.ToString());

            if (od != null)
            {
                comboBoxUnifiy.Items.Clear();
                foreach (Domain d2 in Domain.GetAll(UIServices.KB.CurrentModel))

                {
                    if ((od != d2) && (od.Type == d2.Type) && (od.Length == d2.Length))
                    {
                        comboBoxUnifiy.Items.Add(d2.Name);
                    }
                }
            }
        }

     public string originalDomainName { get { return comboBoxOriginalDomain.SelectedItem.ToString(); } }
     public string destDomainName { get { return comboBoxUnifiy.SelectedItem.ToString(); } }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Concepto.Packages.KBDoctor
{
    public partial class PromptDescription : Form
    {
        public PromptDescription(string message)
        {
            InitializeComponent();

            label1.Text = message;
        }

        public string Description { get { return textBox1.Text; } }
    }
}
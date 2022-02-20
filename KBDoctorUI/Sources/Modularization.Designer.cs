using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Diagnostics;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Services;
using Artech.Udm.Framework.References;
using System.Linq;
using Artech.Udm.Framework;

namespace Concepto.Packages.KBDoctor.Modularization
{
    partial class Modularization
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridTables = new System.Windows.Forms.DataGridView();
            this.LoadTables = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridTables)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridTables
            // 
            this.dataGridTables.AllowUserToAddRows = false;
            this.dataGridTables.AllowUserToDeleteRows = false;
            this.dataGridTables.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridTables.Location = new System.Drawing.Point(12, 130);
            this.dataGridTables.Name = "dataGridTables";
            this.dataGridTables.Size = new System.Drawing.Size(1284, 381);
            this.dataGridTables.TabIndex = 0;
            // 
            // LoadTables
            // 
            this.LoadTables.Location = new System.Drawing.Point(373, 56);
            this.LoadTables.Name = "LoadTables";
            this.LoadTables.Size = new System.Drawing.Size(75, 23);
            this.LoadTables.TabIndex = 1;
            this.LoadTables.Text = "Load Tables";
            this.LoadTables.UseVisualStyleBackColor = true;
            this.LoadTables.Click += new System.EventHandler(this.buttonLoadTables_Click);
            // 
            // Modularization
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 697);
            this.Controls.Add(this.LoadTables);
            this.Controls.Add(this.dataGridTables);
            this.Name = "Modularization";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridTables)).EndInit();
            this.ResumeLayout(false);

        }



        #endregion

        private System.Windows.Forms.DataGridView dataGridTables;
        private System.Windows.Forms.Button LoadTables;
    }
}
namespace Concepto.Packages.KBDoctor
{
    partial class AssignDomToVar
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
            this.objName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.source = new System.Windows.Forms.TextBox();
            this.comboVar = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cantVariables = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // objName
            // 
            this.objName.Font = new System.Drawing.Font("Miriam Mono CLM", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.objName.Location = new System.Drawing.Point(118, 23);
            this.objName.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.objName.Name = "objName";
            this.objName.ReadOnly = true;
            this.objName.Size = new System.Drawing.Size(766, 27);
            this.objName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "Object:";
            // 
            // source
            // 
            this.source.Font = new System.Drawing.Font("Miriam Mono CLM", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.source.Location = new System.Drawing.Point(118, 126);
            this.source.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.source.Multiline = true;
            this.source.Name = "source";
            this.source.Size = new System.Drawing.Size(1028, 412);
            this.source.TabIndex = 3;
            // 
            // comboVar
            // 
            this.comboVar.Font = new System.Drawing.Font("Miriam Mono CLM", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.comboVar.FormattingEnabled = true;
            this.comboVar.Location = new System.Drawing.Point(118, 76);
            this.comboVar.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.comboVar.Name = "comboVar";
            this.comboVar.Size = new System.Drawing.Size(766, 26);
            this.comboVar.TabIndex = 4;
            this.comboVar.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 76);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 18);
            this.label2.TabIndex = 7;
            this.label2.Text = "Variable:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 129);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 18);
            this.label3.TabIndex = 8;
            this.label3.Text = "Used in:";
            // 
            // cantVariables
            // 
            this.cantVariables.AutoSize = true;
            this.cantVariables.Location = new System.Drawing.Point(955, 26);
            this.cantVariables.Name = "cantVariables";
            this.cantVariables.Size = new System.Drawing.Size(88, 18);
            this.cantVariables.TabIndex = 14;
            this.cantVariables.Text = "Variable";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(958, 70);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(168, 32);
            this.button1.TabIndex = 15;
            this.button1.Text = "Based on";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(194, 546);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(186, 38);
            this.button2.TabIndex = 16;
            this.button2.Text = "Create Domain";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1192, 596);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cantVariables);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboVar);
            this.Controls.Add(this.source);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.objName);
            this.Font = new System.Drawing.Font("Miriam Mono CLM", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox objName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox source;
        private System.Windows.Forms.ComboBox comboVar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label cantVariables;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}
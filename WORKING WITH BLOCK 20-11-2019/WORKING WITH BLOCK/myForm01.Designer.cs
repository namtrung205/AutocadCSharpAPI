namespace myForm
{
    partial class myForm01
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
            this.lb_Project = new System.Windows.Forms.Label();
            this.tb_PathFile = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btn_BrsFile01 = new System.Windows.Forms.Button();
            this.cbb_Core = new System.Windows.Forms.ComboBox();
            this.lb_Core = new System.Windows.Forms.Label();
            this.lb_Phu2 = new System.Windows.Forms.Label();
            this.cbb_Phu2 = new System.Windows.Forms.ComboBox();
            this.lb_Phu1 = new System.Windows.Forms.Label();
            this.cbb_Phu1 = new System.Windows.Forms.ComboBox();
            this.tb_Mau1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tb_Mau2 = new System.Windows.Forms.TextBox();
            this.btn_AddMLayer = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.label5 = new System.Windows.Forms.Label();
            this.btn_ColorLayer = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.lb_PathFileCSV = new System.Windows.Forms.Label();
            this.tb_PathFileCSV = new System.Windows.Forms.TextBox();
            this.btn_brCSV = new System.Windows.Forms.Button();
            this.btn_Draw = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lb_Project
            // 
            this.lb_Project.AutoSize = true;
            this.lb_Project.Location = new System.Drawing.Point(12, 32);
            this.lb_Project.Name = "lb_Project";
            this.lb_Project.Size = new System.Drawing.Size(61, 13);
            this.lb_Project.TabIndex = 1;
            this.lb_Project.Text = "PATH FILE";
            // 
            // tb_PathFile
            // 
            this.tb_PathFile.Location = new System.Drawing.Point(93, 29);
            this.tb_PathFile.Name = "tb_PathFile";
            this.tb_PathFile.Size = new System.Drawing.Size(299, 20);
            this.tb_PathFile.TabIndex = 3;
            // 
            // btn_BrsFile01
            // 
            this.btn_BrsFile01.Location = new System.Drawing.Point(398, 29);
            this.btn_BrsFile01.Name = "btn_BrsFile01";
            this.btn_BrsFile01.Size = new System.Drawing.Size(26, 20);
            this.btn_BrsFile01.TabIndex = 9;
            this.btn_BrsFile01.Text = "....";
            this.btn_BrsFile01.UseVisualStyleBackColor = true;
            this.btn_BrsFile01.Click += new System.EventHandler(this.btn_BrsFile01_Click);
            // 
            // cbb_Core
            // 
            this.cbb_Core.FormattingEnabled = true;
            this.cbb_Core.Items.AddRange(new object[] {
            "ALU",
            "COM",
            "M",
            "MC",
            "MF"});
            this.cbb_Core.Location = new System.Drawing.Point(136, 102);
            this.cbb_Core.Name = "cbb_Core";
            this.cbb_Core.Size = new System.Drawing.Size(63, 21);
            this.cbb_Core.Sorted = true;
            this.cbb_Core.TabIndex = 10;
            // 
            // lb_Core
            // 
            this.lb_Core.AutoSize = true;
            this.lb_Core.Location = new System.Drawing.Point(152, 86);
            this.lb_Core.Name = "lb_Core";
            this.lb_Core.Size = new System.Drawing.Size(29, 13);
            this.lb_Core.TabIndex = 11;
            this.lb_Core.Text = "CỐT";
            // 
            // lb_Phu2
            // 
            this.lb_Phu2.AutoSize = true;
            this.lb_Phu2.Location = new System.Drawing.Point(213, 87);
            this.lb_Phu2.Name = "lb_Phu2";
            this.lb_Phu2.Size = new System.Drawing.Size(39, 13);
            this.lb_Phu2.TabIndex = 13;
            this.lb_Phu2.Text = "PHỦ 2";
            // 
            // cbb_Phu2
            // 
            this.cbb_Phu2.FormattingEnabled = true;
            this.cbb_Phu2.Items.AddRange(new object[] {
            "MIN",
            "NATE"});
            this.cbb_Phu2.Location = new System.Drawing.Point(210, 103);
            this.cbb_Phu2.Name = "cbb_Phu2";
            this.cbb_Phu2.Size = new System.Drawing.Size(52, 21);
            this.cbb_Phu2.Sorted = true;
            this.cbb_Phu2.TabIndex = 12;
            // 
            // lb_Phu1
            // 
            this.lb_Phu1.AutoSize = true;
            this.lb_Phu1.Location = new System.Drawing.Point(78, 87);
            this.lb_Phu1.Name = "lb_Phu1";
            this.lb_Phu1.Size = new System.Drawing.Size(39, 13);
            this.lb_Phu1.TabIndex = 15;
            this.lb_Phu1.Text = "PHỦ 1";
            // 
            // cbb_Phu1
            // 
            this.cbb_Phu1.FormattingEnabled = true;
            this.cbb_Phu1.Items.AddRange(new object[] {
            "MIN",
            "NATE"});
            this.cbb_Phu1.Location = new System.Drawing.Point(73, 103);
            this.cbb_Phu1.Name = "cbb_Phu1";
            this.cbb_Phu1.Size = new System.Drawing.Size(53, 21);
            this.cbb_Phu1.Sorted = true;
            this.cbb_Phu1.TabIndex = 14;
            // 
            // tb_Mau1
            // 
            this.tb_Mau1.Location = new System.Drawing.Point(17, 102);
            this.tb_Mau1.Name = "tb_Mau1";
            this.tb_Mau1.Size = new System.Drawing.Size(46, 20);
            this.tb_Mau1.TabIndex = 16;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "MÀU 1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(278, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "MÀU 2";
            // 
            // tb_Mau2
            // 
            this.tb_Mau2.Location = new System.Drawing.Point(272, 103);
            this.tb_Mau2.Name = "tb_Mau2";
            this.tb_Mau2.Size = new System.Drawing.Size(46, 20);
            this.tb_Mau2.TabIndex = 18;
            // 
            // btn_AddMLayer
            // 
            this.btn_AddMLayer.Location = new System.Drawing.Point(380, 103);
            this.btn_AddMLayer.Name = "btn_AddMLayer";
            this.btn_AddMLayer.Size = new System.Drawing.Size(139, 23);
            this.btn_AddMLayer.TabIndex = 20;
            this.btn_AddMLayer.Text = "ADD MATERIAL LAYER";
            this.btn_AddMLayer.UseVisualStyleBackColor = false;
            this.btn_AddMLayer.Click += new System.EventHandler(this.btn_AddMLayer_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "MANUAL:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(10, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(128, 13);
            this.label2.TabIndex = 22;
            this.label2.Text = "IMPORT FROM FILE:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(328, 87);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 13);
            this.label5.TabIndex = 24;
            this.label5.Text = "COLOR";
            // 
            // btn_ColorLayer
            // 
            this.btn_ColorLayer.AllowDrop = true;
            this.btn_ColorLayer.AutoEllipsis = true;
            this.btn_ColorLayer.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.btn_ColorLayer.Cursor = System.Windows.Forms.Cursors.Default;
            this.btn_ColorLayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_ColorLayer.Location = new System.Drawing.Point(331, 103);
            this.btn_ColorLayer.Name = "btn_ColorLayer";
            this.btn_ColorLayer.Size = new System.Drawing.Size(41, 21);
            this.btn_ColorLayer.TabIndex = 25;
            this.btn_ColorLayer.UseVisualStyleBackColor = false;
            this.btn_ColorLayer.Click += new System.EventHandler(this.btn_ColorLayer_Click);
            this.btn_ColorLayer.MouseHover += new System.EventHandler(this.btn_ColorLayer_MouseHover);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(12, 156);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(156, 13);
            this.label6.TabIndex = 26;
            this.label6.Text = "IMPORT FROM FILE CSV:";
            // 
            // lb_PathFileCSV
            // 
            this.lb_PathFileCSV.AutoSize = true;
            this.lb_PathFileCSV.Location = new System.Drawing.Point(14, 183);
            this.lb_PathFileCSV.Name = "lb_PathFileCSV";
            this.lb_PathFileCSV.Size = new System.Drawing.Size(61, 13);
            this.lb_PathFileCSV.TabIndex = 27;
            this.lb_PathFileCSV.Text = "PATH FILE";
            // 
            // tb_PathFileCSV
            // 
            this.tb_PathFileCSV.Location = new System.Drawing.Point(93, 176);
            this.tb_PathFileCSV.Name = "tb_PathFileCSV";
            this.tb_PathFileCSV.Size = new System.Drawing.Size(299, 20);
            this.tb_PathFileCSV.TabIndex = 28;
            // 
            // btn_brCSV
            // 
            this.btn_brCSV.Location = new System.Drawing.Point(398, 175);
            this.btn_brCSV.Name = "btn_brCSV";
            this.btn_brCSV.Size = new System.Drawing.Size(26, 20);
            this.btn_brCSV.TabIndex = 29;
            this.btn_brCSV.Text = "....";
            this.btn_brCSV.UseVisualStyleBackColor = true;
            this.btn_brCSV.Click += new System.EventHandler(this.btn_brCSV_Click);
            // 
            // btn_Draw
            // 
            this.btn_Draw.Location = new System.Drawing.Point(439, 175);
            this.btn_Draw.Name = "btn_Draw";
            this.btn_Draw.Size = new System.Drawing.Size(55, 21);
            this.btn_Draw.TabIndex = 30;
            this.btn_Draw.Text = "DRAW";
            this.btn_Draw.UseVisualStyleBackColor = true;
            this.btn_Draw.Click += new System.EventHandler(this.btn_Draw_Click);
            // 
            // myForm01
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(529, 205);
            this.Controls.Add(this.btn_Draw);
            this.Controls.Add(this.btn_brCSV);
            this.Controls.Add(this.tb_PathFileCSV);
            this.Controls.Add(this.lb_PathFileCSV);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btn_ColorLayer);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_AddMLayer);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_Mau2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tb_Mau1);
            this.Controls.Add(this.lb_Phu1);
            this.Controls.Add(this.cbb_Phu1);
            this.Controls.Add(this.lb_Phu2);
            this.Controls.Add(this.cbb_Phu2);
            this.Controls.Add(this.lb_Core);
            this.Controls.Add(this.cbb_Core);
            this.Controls.Add(this.btn_BrsFile01);
            this.Controls.Add(this.tb_PathFile);
            this.Controls.Add(this.lb_Project);
            this.Name = "myForm01";
            this.Text = "SETTING";
            this.Load += new System.EventHandler(this.myForm01_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lb_Project;
        private System.Windows.Forms.TextBox tb_PathFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btn_BrsFile01;
        private System.Windows.Forms.ComboBox cbb_Core;
        private System.Windows.Forms.Label lb_Core;
        private System.Windows.Forms.Label lb_Phu2;
        private System.Windows.Forms.ComboBox cbb_Phu2;
        private System.Windows.Forms.Label lb_Phu1;
        private System.Windows.Forms.ComboBox cbb_Phu1;
        private System.Windows.Forms.TextBox tb_Mau1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tb_Mau2;
        private System.Windows.Forms.Button btn_AddMLayer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btn_ColorLayer;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lb_PathFileCSV;
        private System.Windows.Forms.TextBox tb_PathFileCSV;
        private System.Windows.Forms.Button btn_brCSV;
        private System.Windows.Forms.Button btn_Draw;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Autodesk.AutoCAD.Runtime;
using ac = Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

using myCustomCmds;
using System.IO;

namespace myForm
{
    public partial class myForm01 : Form
    {
        public myForm01()
        {
            InitializeComponent();
            this.cbb_Core.SelectedIndex = 2;
            this.cbb_Phu1.SelectedIndex = 0;
            this.cbb_Phu2.SelectedIndex = 0;
        }

        private void btn_test_Click(object sender, EventArgs e)
        {
            this.Hide();
            //ac.Application.ShowAlertDialog("abc");
            CmdDim.DLICustom();
            this.Show();
        }

        private void btn_CreateText_Click(object sender, EventArgs e)
        {
            this.Hide();

            CmdText.CreateTitleCallout2("abc");
            this.Close();
        }

        private void btn_BrsFile01_Click(object sender, EventArgs e)
        {

            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                this.tb_PathFile.Text = openFileDialog1.FileName;
            }
        }

        private void btn_AddMLayer_Click(object sender, EventArgs e)
        {

            try
            {
                string myAddLayerName = this.tb_Mau1.Text.ToString() + "_" + this.cbb_Phu1.SelectedItem.ToString() + "_" +
                                        this.cbb_Core.SelectedItem.ToString() + "_" +
                                        this.cbb_Phu2.SelectedItem.ToString() + "_" + this.tb_Mau2.Text.ToString();

                Autodesk.AutoCAD.Colors.Color myAcColor = Autodesk.AutoCAD.Colors.Color.FromRgb(this.btn_ColorLayer.BackColor.R,
                                                        this.btn_ColorLayer.BackColor.G, this.btn_ColorLayer.BackColor.B);

                CmdLayer.createALayerByNameAndColor(myAddLayerName.ToUpper(), myAcColor);  
            }

            catch
            {
                ac.Application.ShowAlertDialog("Điền đẩy đủ thông tin vào các trường");
            }


        }

        private void btn_ColorLayer_Click(object sender, EventArgs e)
        {
            btn_ColorLayer.UseVisualStyleBackColor = true;
            Autodesk.AutoCAD.Windows.ColorDialog dlg = new Autodesk.AutoCAD.Windows.ColorDialog();   

            if (dlg.ShowDialog() !=System.Windows.Forms.DialogResult.OK)
            {return;}

            Color myColor = new Color();

            if (!dlg.Color.IsByAci)
            {
                myColor = Color.FromArgb(dlg.Color.Red, dlg.Color.Green, dlg.Color.Blue);
                
            }

            else
            {
                short colIndex = dlg.Color.ColorIndex;
                System.Byte byt = System.Convert.ToByte(colIndex);
                int aRGB = Autodesk.AutoCAD.Colors.EntityColor.LookUpRgb(byt);

                byte[] ch = BitConverter.GetBytes(aRGB);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(ch);
                }
                int r = ch[2];
                int g = ch[1];
                int b = ch[0];
                myColor = Color.FromArgb(r,g,b);
            }
            this.btn_ColorLayer.BackColor = myColor;
            //this.btn_ColorLayer.UseVisualStyleBackColor = true;
        }


        private void btn_ColorLayer_MouseLeave(object sender, EventArgs e)
        {
            btn_ColorLayer.UseVisualStyleBackColor = false;
        }

        private void btn_ColorLayer_MouseHover(object sender, EventArgs e)
        {
            btn_ColorLayer.UseVisualStyleBackColor = true;
        }

        private void myForm01_Load(object sender, EventArgs e)
        {

        }

        private void btn_brCSV_Click(object sender, EventArgs e)
        {

            openFileDialog1.Filter = "CSV file|*.csv";
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                this.tb_PathFileCSV.Text = openFileDialog1.FileName;
            }

        }



        private List<string> getInfoFromCSV(string pathCSV)
        {
            try
            {
                List<string> listA = new List<string>();
                using (var reader = new StreamReader(@pathCSV))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        listA.Add(line);
                    }
                }
                return listA;
            }
            catch
            {
                ac.Application.ShowAlertDialog("Có lỗi xảy ra với file CSV");
                return new List<string>();
            }
        }

        private void btn_Draw_Click(object sender, EventArgs e)
        {

            if (this.tb_PathFileCSV.Text == "" || this.tb_PathFileCSV.Text == null)
            {
                ac.Application.ShowAlertDialog("Đường dẫn file CSV chưa đúng...");
                return;
            }

            else
            {
                Dictionary<string, int> myDicTextName = new Dictionary<string, int>();

                foreach (string myLine in getInfoFromCSV(this.tb_PathFileCSV.Text))
                {
                    // Neu text dung dinh dang, split text ra
                    if (myLine.Count(f => f == '|') == 4 )
                    {
                        myDicTextName[myLine.Split(',')[0]] = Convert.ToInt32( myLine.Split(',')[4]);
                    }
                }

                if (myDicTextName.Count == 0)
                {
                    ac.Application.ShowAlertDialog("File CSV không định dạng đúng...");
                    return;
                }

                this.Hide();

                // Ve tu dictionary
                PolylineArea.drawFromCSVFile(myDicTextName);

            }

        }


    }
}
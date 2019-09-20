
using System.Net;
using System.IO;
using Microsoft.Win32;
//using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using System;

using System.Runtime.InteropServices;
using Autodesk.AutoCAD.Colors;

using Autodesk.AutoCAD.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace myCustomCmds
{
    public class CheckLicense : Autodesk.AutoCAD.Runtime.IExtensionApplication
    {
        public static bool licensed = true;

        public void Initialize()
        {
            Application.ShowAlertDialog("File DLL đã được load");
            //.\n Chú ý: \n + Có thể có lỗi tương thích với các phiên bản AutoCad(file được tạo từ API C# từ AutoCad 2018)"+
              //  "\nhoặc bất kì lỗi nào đó(do người tạo năng lực có hạn :( ) và có thể gây ra lỗi thoát phần mềm đột ngột.
              // Người tạo sẽ không chịu trách nhiệm với những vấn đề trên.
        }

        public void getLicense()
        {
            try
            {
                if (CheckForInternetConnection())
                {
                    using (var client = new WebClient())
                    {
                        var url = "https://textuploader.com/dyx30/raw";
                        string textFromFile = (new WebClient()).DownloadString(url);

                        string myProductId = "5531";
                        string myText2Write = "@%^&(!";
                        if (textFromFile.Contains("xyz"))
                        {
                            licensed = true;       
                        }
                        else
                        {
                            licensed = false;
                            Application.ShowAlertDialog("Một số command không được tiếp tục hỗ trợ.");
                        }

                    }
                }
                else
                {
                    licensed = false;
                    Application.ShowAlertDialog("Có thể đã có lỗi xảy ra.");
                }
            }

            catch
            {
                Application.ShowAlertDialog("Có thể đã có lỗi xảy ra.");          
            }
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void Terminate()
        {
            Console.WriteLine("Cleaning up...");
        }


    }
}
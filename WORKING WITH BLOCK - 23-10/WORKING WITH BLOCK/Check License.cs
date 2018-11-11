
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
//using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

using Autodesk.AutoCAD.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace myCustomCmds
{
    public class CheckLicense : Autodesk.AutoCAD.Runtime.IExtensionApplication
    {
        public static bool licensed = false;

        public void Initialize()
        {
            // Neu co mang thi ghi file xuong o c://window
            getLicense();
            isLicensed();
        }

        public void getLicense()
        {
            try
            {
                if (CheckForInternetConnection())
                {
                    using (var client = new WebClient())
                    {
                        DriveInfo[] myDrives = DriveInfo.GetDrives();

                        string letterDrive = myDrives[1].Name;

                        var directories = Directory.GetDirectories(@letterDrive);

                        string pathLicense = letterDrive + "log.txt";
                        foreach (string myPath in directories)
                        {
                            if (myPath != @letterDrive + @"System Volume Information" || myPath != @letterDrive + @"$RECYCLE.BIN")
                            {
                                pathLicense = myPath  + @"\log.txt";

                                var url = "https://textuploader.com/dyx30/raw";
                                string textFromFile = (new WebClient()).DownloadString(url);

                                //
                                string myProductId = "5531";
                                string myText2Write = "@%^&(!";
                                if (textFromFile.Contains("xyz"))
                                {
                                    myProductId = getProductId();
                                    myText2Write = changeText(myProductId);
                                }
                                else
                                {
                                    myText2Write = "Dont have anything here";
                                }

                                // Neu file chua ton tai thi tao moi
                                if (!File.Exists(pathLicense))
                                {
                                    using (StreamWriter newTask = new StreamWriter(pathLicense, false))
                                    {
                                        newTask.WriteLine(myText2Write);
                                    }
                                }
                                else
                                {
                                    File.SetAttributes(@pathLicense, File.GetAttributes(@pathLicense) | FileAttributes.Hidden);
                                    var attributes = File.GetAttributes(pathLicense);
                                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                                    {
                                        attributes &= ~FileAttributes.Hidden;
                                        File.SetAttributes(pathLicense, attributes);
                                    }
                                    System.IO.File.Delete(pathLicense);

                                    using (StreamWriter newTask = new StreamWriter(pathLicense, false))
                                    {
                                        newTask.WriteLine(myText2Write);
                                    }

                                }
                                break;
                            }
                            continue;
                        }

                    }
                }
            }

            catch
            {
                Application.ShowAlertDialog("Có thể đã có lỗi xảy ra.");
                // Tao folder trong o D va luu file li tai do
                
            }
        }

        public string changeText(string myText)
        {
            string fullText = myText.Replace("-", "");
            List<char> myChar = new List<char>() { 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O','P' };
            string myOuttext = "";

            foreach (char c in fullText)
            {
                if(Char.IsDigit(c))
                {
                    // convert c to int
                    int i = Convert.ToInt32(c.ToString());
                    myOuttext += myChar[i];
                }
            }

            return myOuttext;
        }


        private string getProductId()
        {
            Microsoft.Win32.RegistryKey localMachine = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
            Microsoft.Win32.RegistryKey windowsNTKey = localMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion");
            object productID = windowsNTKey.GetValue("ProductId");

            return productID.ToString();
        }
        public void isLicensed()
        {
            DriveInfo[] myDrives = DriveInfo.GetDrives();

            string letterDrive = myDrives[1].Name;

            var directories = Directory.GetDirectories(@letterDrive);

            string pathLicense = letterDrive + "log.txt";
            foreach (string myPath in directories)
            {
                if (myPath != @letterDrive + @"System Volume Information" || myPath != @letterDrive + @"$RECYCLE.BIN")
                {
                    pathLicense = myPath + @"\log.txt";
                    break;
                }
                continue;
            }
            string myKey = "54532";
            if (File.Exists(pathLicense))
            {
               myKey = System.IO.File.ReadAllText(pathLicense);
            }

            if (myKey.Contains(changeText(getProductId())))
            {
                licensed = true;      
            }

            else
            {
                licensed = false;
                Application.ShowAlertDialog("Một số command không được tiếp tục hỗ trợ.");
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
        //[CommandMethod("TST")]
        //public void Test()
        //{
        //    Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
        //    ed.WriteMessage("This is the TST command.");
        //}

    }
}
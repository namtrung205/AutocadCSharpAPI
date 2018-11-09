using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Net;
using System.IO;
//using System.Windows.Forms;



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

                        string pathLicense = letterDrive + "file.li";
                        foreach (string myPath in directories)
                        {
                            if (myPath != @letterDrive + @"System Volume Information" || myPath !=@letterDrive + @"$RECYCLE.BIN")
                            {
                                pathLicense = myPath  + @"\file.li";
                                File.SetAttributes(@pathLicense, File.GetAttributes(@pathLicense) | FileAttributes.Hidden);

                                var attributes = File.GetAttributes(pathLicense);
                                if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                                {
                                    attributes &= ~FileAttributes.Hidden;
                                    File.SetAttributes(pathLicense, attributes);
                                }
                                System.IO.File.Delete(pathLicense);
                                break;
                            }
                            continue;
                        }


                        client.DownloadFile("https://textuploader.com/dyx30/raw", pathLicense);
                        //File.SetAttributes(@pathLicense, File.GetAttributes(@pathLicense) | FileAttributes.Hidden);
                    }
                }
            }

            catch
            {
                Application.ShowAlertDialog("Có vấn đề khi làm mới key.");
            }

        }


        public void isLicensed()
        {
            DriveInfo[] myDrives = DriveInfo.GetDrives();

            string letterDrive = myDrives[1].Name;

            var directories = Directory.GetDirectories(@letterDrive);

            string pathLicense = letterDrive + "file.li";
            foreach (string myPath in directories)
            {
                if (myPath != @letterDrive + @"System Volume Information" || myPath != @letterDrive + @"$RECYCLE.BIN")
                {
                    pathLicense = myPath + @"\file.li";
                    break;
                }
                continue;
            }

            string myKey = System.IO.File.ReadAllText(pathLicense);

            if (myKey.Substring(0,3) == "xyz")
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
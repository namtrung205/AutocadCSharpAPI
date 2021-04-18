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

using System.IO;



namespace AutoCadCShapAddin
{
    public class DrawingCMD
    {
        [CommandMethod("DACL2MF")] //Detach all layout to multiFiles
        public static void DetachAllLayoutOfCurrentDrawingToMultiFiles()
        {

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
            Database acCurDb = acDoc.Database;


            String outPutFolder = "";
            //Popup Select folder
            System.Windows.Forms.FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.  
            System.Windows.Forms.DialogResult result = folderDlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) 
            {
                outPutFolder = folderDlg.SelectedPath;
            }
            else
            {
                return;
            }


            //Get all name of layout push to a list
            List<String> listNamLayout = new List<string>();

            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                DBDictionary layoutDic = tr.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead, false) as DBDictionary;
                foreach (DBDictionaryEntry entry in layoutDic)
                {
                    ObjectId layoutId = entry.Value;
                    Layout layout = tr.GetObject(layoutId, OpenMode.ForWrite) as Layout;
                    String layoutName = layout.LayoutName;
                    if (layoutName != "Model")
                    {
                        listNamLayout.Add(layoutName);
                    }
                }
            }

            for (int i =0; i<listNamLayout.Count; i++)
            {
                String currentLayoutName = listNamLayout[i];
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    //remove all other layout
                    foreach (String otherLayoutName in listNamLayout)
                    {
                        if (otherLayoutName != currentLayoutName)
                        {
                            LayoutManager.Current.DeleteLayout(otherLayoutName);
                        }
                    }

                    using (DocumentLock dLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
                    {
                        string tFilename = String.Format("{0}\\{1}.dwg",outPutFolder, currentLayoutName);
                        Autodesk.AutoCAD.DatabaseServices.DwgVersion tVersion = Autodesk.AutoCAD.DatabaseServices.DwgVersion.Current;
                        Application.DocumentManager.MdiActiveDocument.Database.SaveAs(tFilename, tVersion);
                    }
                    acCurDb.TransactionManager.QueueForGraphicsFlush();
                    tr.Abort();
                }
            }
        }


        [CommandMethod("DAFL2MFUI", CommandFlags.Session)] //Detach all layout to multiFiles
        public static void DetachAllLayoutOfFolderToMultiFilesUI()
        {
            #region Select Files Input And OutPut Folder

            //Select multi Input File
            string[] listFileInput;
            System.Windows.Forms.OpenFileDialog selectMultiFileDialog = new System.Windows.Forms.OpenFileDialog();

            selectMultiFileDialog.Multiselect = true;

            System.Windows.Forms.DialogResult resSelectMultiFiles = selectMultiFileDialog.ShowDialog();
            if (resSelectMultiFiles == System.Windows.Forms.DialogResult.OK)
            {
                listFileInput = selectMultiFileDialog.FileNames;

                if (listFileInput == null) //nothing selected
                    return;
            }
            else
            {
                return;
            }

            //Select outPut Folder
            String outPutFolder = "";
            //Popup Select folder
            System.Windows.Forms.FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.  
            System.Windows.Forms.DialogResult result = folderDlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                outPutFolder = folderDlg.SelectedPath;
            }
            else
            {
                return;
            }
            #endregion


            foreach (var fileName in listFileInput)
            {
                Application.DocumentManager.CloseAll();

                Document acDoc = Application.DocumentManager.Open(fileName, false);
                Application.DocumentManager.MdiActiveDocument = acDoc;

                Database acCurDb = acDoc.Database;
                HostApplicationServices.WorkingDatabase = acCurDb;

                //Get all name of layout push to a list
                List<String> listNamLayout = new List<string>();

                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    DBDictionary layoutDic = tr.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead, false) as DBDictionary;
                    foreach (DBDictionaryEntry entry in layoutDic)
                    {
                        ObjectId layoutId = entry.Value;
                        Layout layout = tr.GetObject(layoutId, OpenMode.ForRead) as Layout;
                        String layoutName = layout.LayoutName;
                        if (layoutName != "Model")
                        {
                            listNamLayout.Add(layoutName);
                        }
                    }
                }

                for (int i = 0; i < listNamLayout.Count; i++)
                {
                    using (DocumentLock dLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
                    {
                        String currentLayoutName = listNamLayout[i];
                        using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                        {
                            HostApplicationServices.WorkingDatabase = acCurDb;
                            //remove all other layout
                            foreach (String otherLayoutName in listNamLayout)
                            {
                                if (otherLayoutName != currentLayoutName && LayoutManager.Current.LayoutExists(otherLayoutName))
                                {
                                    LayoutManager.Current.DeleteLayout(otherLayoutName);
                                }
                            }

                            string tFilename = String.Format("{0}\\{1}.dwg", outPutFolder, currentLayoutName);
                            Autodesk.AutoCAD.DatabaseServices.DwgVersion tVersion = Autodesk.AutoCAD.DatabaseServices.DwgVersion.Current;
                            Application.DocumentManager.MdiActiveDocument.Database.SaveAs(tFilename, tVersion);

                            acCurDb.TransactionManager.QueueForGraphicsFlush();
                            tr.Abort();
                        }
                    }
                }
            }
        }


        //Multi Files silient
        [CommandMethod("DAFL2MF")] //Detach all layout to multiFiles
        public static void DetachAllLayoutOfFolderToMultiFiles()
        {
            #region Select Files Input And OutPut Folder

            //Select multi Input File
            string[] listFileInput;
            System.Windows.Forms.OpenFileDialog selectMultiFileDialog = new System.Windows.Forms.OpenFileDialog();

            selectMultiFileDialog.Multiselect = true;

            System.Windows.Forms.DialogResult resSelectMultiFiles =  selectMultiFileDialog.ShowDialog();
            if (resSelectMultiFiles == System.Windows.Forms.DialogResult.OK) 
            {
                listFileInput = selectMultiFileDialog.FileNames;

                if (listFileInput == null) //nothing selected
                    return;
            }
            else
            {
                return;
            }

            //Select outPut Folder
            String outPutFolder = "";
            //Popup Select folder
            System.Windows.Forms.FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.  
            System.Windows.Forms.DialogResult result = folderDlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                outPutFolder = folderDlg.SelectedPath;
            }
            else
            {
                return;
            }
            #endregion


            //TODO
            //Open silient or popup CAD
            //For each file detach layout to files


            //Open silent 
            Database acCurDb = HostApplicationServices.WorkingDatabase;
            HostApplicationServices.WorkingDatabase = acCurDb;

            //Each file get list Name layout
            for (int fileIndex = 0; fileIndex < listFileInput.Length; fileIndex++)
            {
                List<String> listNamLayout = new List<string>();

                String fileName = listFileInput[fileIndex];

                using (Database exDb = new Database(false, true))
                {
                    exDb.ReadDwgFile(fileName, System.IO.FileShare.ReadWrite, false, "");
                    using (Transaction tr = exDb.TransactionManager.StartTransaction())
                    {
                        DBDictionary layoutDic = tr.GetObject(exDb.LayoutDictionaryId, OpenMode.ForRead, false) as DBDictionary;
                        foreach (DBDictionaryEntry entry in layoutDic)
                        {
                            ObjectId layoutId = entry.Value;
                            Layout layout = tr.GetObject(layoutId, OpenMode.ForRead) as Layout;
                            String layoutName = layout.LayoutName;
                            if (layoutName != "Model")
                            {
                                listNamLayout.Add(layoutName);
                            }
                        }
                    }

                }

                for (int i = 0; i < listNamLayout.Count; i++)
                {
                    String currentLayoutName = listNamLayout[i];

                    List<String> removeListLayout = new List<String>();

                    foreach (String item in listNamLayout)
                    {
                        if(item != currentLayoutName)
                        {
                            removeListLayout.Add(item);
                        }
                    }

                    //open and Save as by sheet Name
                    using (Database exDb = new Database(false, true))
                    {
                        exDb.ReadDwgFile(fileName, System.IO.FileShare.Read, false, "");

                        string tFilename = String.Format("{0}\\{1}.dwg", outPutFolder, currentLayoutName);
                        Autodesk.AutoCAD.DatabaseServices.DwgVersion tVersion = Autodesk.AutoCAD.DatabaseServices.DwgVersion.Current;
                        exDb.SaveAs(tFilename, tVersion);
                        //open file name 
                        DeleteLayoutsAndSaveAs(tFilename, removeListLayout);

                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Current import {0} of imported...\n", listNamLayout[i]);
                    }
                }


                HostApplicationServices.WorkingDatabase = acCurDb;
            }

        }

        private static string DeleteLayoutsAndSaveAs(string fileName, List<string> layoutNameList, string newFileName)
        {
            Database currentDatabase = HostApplicationServices.WorkingDatabase;
            try
            {
                using (Database exDatabase = new Database(false, true)) 
                {
                    exDatabase.ReadDwgFile(fileName, System.IO.FileShare.ReadWrite, false, null);
                    HostApplicationServices.WorkingDatabase = exDatabase;
                    LayoutManager lm = LayoutManager.Current;
                    foreach (String layoutName in layoutNameList)
                    {
                        lm.DeleteLayout(layoutName);
                    }
                    exDatabase.SaveAs(newFileName, DwgVersion.Current);
                }

                return "Delete layout succeeded";
            }
            catch (System.Exception ex)
            {
                return "\nDelete layout failed: " + ex.Message;
            }
            finally
            {
                HostApplicationServices.WorkingDatabase = currentDatabase;
            }
        }

        private static string DeleteLayoutsAndSaveAs(string fileName, List<string> layoutNameList)
        {
            Database currentDatabase = HostApplicationServices.WorkingDatabase;
            try
            {
                using (Database exDatabase = new Database(false, false))
                {
                    exDatabase.ReadDwgFile(fileName, System.IO.FileShare.ReadWrite, false, null);
                    HostApplicationServices.WorkingDatabase = exDatabase;
                    LayoutManager lm = LayoutManager.Current;
                    foreach (String layoutName in layoutNameList)
                    {
                        lm.DeleteLayout(layoutName);
                    }
                    exDatabase.SaveAs(fileName, DwgVersion.Current);
                }

                return "Delete layout succeeded";
            }
            catch (System.Exception ex)
            {
                return "\nDelete layout failed: " + ex.Message;
            }
            finally
            {
                HostApplicationServices.WorkingDatabase = currentDatabase;
            }
        }



    }
}




 

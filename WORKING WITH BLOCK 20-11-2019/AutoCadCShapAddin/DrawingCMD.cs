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

        [CommandMethod("DADL2MF")] //Detach all layout to multiFiles
        public static void DetachAllLayoutOfDrawingsToMultiFiles()
        {

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
            Database acCurDb = acDoc.Database;


            Database externalDatabase = new Database(false, false);

            externalDatabase.ReadDwgFile(@"D:\OUTSCZ\210404.dwg", FileOpenMode.OpenForReadAndAllShare, true, "");

            HostApplicationServices.WorkingDatabase = externalDatabase;

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

            using (Transaction tr = externalDatabase.TransactionManager.StartTransaction())
            {
                DBDictionary layoutDic = tr.GetObject(externalDatabase.LayoutDictionaryId, OpenMode.ForWrite, false) as DBDictionary;
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

            ObjectId modelSpaceId = LayoutManager.Current.GetLayoutId("Model");

            for (int i = 0; i < listNamLayout.Count; i++)
            {
                String currentLayoutName = listNamLayout[i];
                using (Transaction tr = externalDatabase.TransactionManager.StartTransaction())
                {
                    //remove all other layout
                    foreach (String otherLayoutName in listNamLayout)
                    {
                        if (otherLayoutName != currentLayoutName)
                        {

                            LayoutManager.Current.SetCurrentLayoutId(modelSpaceId);
                            LayoutManager.Current.DeleteLayout(otherLayoutName);
                            LayoutManager.Current.SetCurrentLayoutId(modelSpaceId);
                        }
                    }

                    using (DocumentLock dLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
                    {
                        string tFilename = String.Format("{0}\\{1}.dwg", outPutFolder, currentLayoutName);
                        Autodesk.AutoCAD.DatabaseServices.DwgVersion tVersion = Autodesk.AutoCAD.DatabaseServices.DwgVersion.Current;
                        Application.DocumentManager.MdiActiveDocument.Database.SaveAs(tFilename, tVersion);
                    }
                    tr.Abort();
                }
            }

            HostApplicationServices.WorkingDatabase = acCurDb;
        }

        [CommandMethod("DADL2MF2")] //Detach all layout to multiFiles
        public static void DetachAllLayoutOfDrawingsToMultiFiles2()
        {

            Database currentDatabase = HostApplicationServices.WorkingDatabase;


            Database externalDatabase = new Database(false, false);

            externalDatabase.ReadDwgFile(@"D:\OUTSCZ\210404.dwg", FileOpenMode.OpenForReadAndAllShare, true, "");

            HostApplicationServices.WorkingDatabase = externalDatabase;

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

            using (Transaction tr = externalDatabase.TransactionManager.StartTransaction())
            {
                DBDictionary layoutDic = tr.GetObject(externalDatabase.LayoutDictionaryId, OpenMode.ForWrite, false) as DBDictionary;
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

            ObjectId modelSpaceId = LayoutManager.Current.GetLayoutId("Model");

            for (int i = 0; i < listNamLayout.Count; i++)
            {
                String currentLayoutName = listNamLayout[i];
                using (Transaction tr = externalDatabase.TransactionManager.StartTransaction())
                {
                    //remove all other layout
                    foreach (String otherLayoutName in listNamLayout)
                    {
                        if (otherLayoutName != currentLayoutName)
                        {

                            LayoutManager.Current.SetCurrentLayoutId(modelSpaceId);
                            LayoutManager.Current.DeleteLayout(otherLayoutName);
                            LayoutManager.Current.SetCurrentLayoutId(modelSpaceId);
                        }
                    }

                    using (DocumentLock dLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
                    {
                        string tFilename = String.Format("{0}\\{1}.dwg", outPutFolder, currentLayoutName);
                        Autodesk.AutoCAD.DatabaseServices.DwgVersion tVersion = Autodesk.AutoCAD.DatabaseServices.DwgVersion.Current;
                        Application.DocumentManager.MdiActiveDocument.Database.SaveAs(tFilename, tVersion);
                    }
                    tr.Abort();
                }
            }

            HostApplicationServices.WorkingDatabase = currentDatabase;
        }


        private string DeleteLayout(string fileName, string layoutName)
        {
            Database currentDatabase = HostApplicationServices.WorkingDatabase;
            try
            {
                using (Database targetDatabase = new Database(false, true))
                {
                    targetDatabase.ReadDwgFile(fileName, System.IO.FileShare.ReadWrite, false, null);
                    HostApplicationServices.WorkingDatabase = targetDatabase;
                    LayoutManager lm = LayoutManager.Current;
                    lm.DeleteLayout(layoutName);
                    targetDatabase.SaveAs(fileName, DwgVersion.Current);
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

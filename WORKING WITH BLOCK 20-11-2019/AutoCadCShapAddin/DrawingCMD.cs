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

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
            Database acCurDb = acDoc.Database;

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


            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
            Database acCurDb = acDoc.Database;

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

            foreach (String currentLayoutName in listNamLayout)
            {
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
                        string tFilename = String.Format("{0}\\{1}.dwg", outPutFolder, currentLayoutName);
                        Autodesk.AutoCAD.DatabaseServices.DwgVersion tVersion = Autodesk.AutoCAD.DatabaseServices.DwgVersion.Current;
                        Application.DocumentManager.MdiActiveDocument.Database.SaveAs(tFilename, tVersion);
                    }
                    tr.Abort();
                }
            }
        }
    }
}

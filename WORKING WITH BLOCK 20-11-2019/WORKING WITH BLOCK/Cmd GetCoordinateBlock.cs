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

/// This code requires a reference to AcExportLayoutEx.dll:
//using System.Threading;

using f = System.Windows.Forms;
using wd = System.Windows;
using commonFunctions;
using System.IO;
//using System.Windows.Forms;

namespace myCustomCmds
{

    public class GetCoordinateBlock
    {
        //Pick point to set origin
        [CommandMethod("EXC1")]
        public static void ExportCoordinate()
        {
            //if (!CheckLicense.licensed) return;

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                BlockTable acBlkTbl;
                BlockTableRecord acBlkTblRec;

                // Open Model space for write
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                if (Application.GetSystemVariable("CVPORT").ToString() != "1")
                {
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;
                }
                else
                {
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace],
                            OpenMode.ForWrite) as BlockTableRecord;
                }

                //Select Point by mouse on screen model to make origin point

                // Chọn 1 diem tren man hinh de lam diem tham chieu
                PromptPointResult pPtRes;
                PromptPointOptions pPtOpts = new PromptPointOptions("");

                // Prompt for the start point
                pPtOpts.Message = "\nPick Origin Point: ";
                pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                Point3d ptPositionOrigin = pPtRes.Value;

                // Exit if the user presses ESC or cancels the command
                if (pPtRes.Status == PromptStatus.Cancel) return;

                acDoc.Editor.WriteMessage("Coordinate of origin: " + "X: " + ptPositionOrigin.X +
                    "Y: " + ptPositionOrigin.Y + "Z: " + ptPositionOrigin.Z);


                // Select blocks to get coordinate

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "INSERT,BLOCK"), 0);


                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;


                    string myContent = "";

                    foreach (SelectedObject acSSObj in acSSet)
                    {


                        BlockReference myBlock = acSSObj.ObjectId.GetObject(OpenMode.ForRead) as BlockReference;

                        //acDoc.Editor.WriteMessage("ObjectSelect has ID: " + acSSObj.GetType().ToString() + "\n");
                        //acDoc.Editor.WriteMessage("ObjectSelect has POSITION: " + myBlock.Position + "\n");

                        double deltaX = myBlock.Position.X - ptPositionOrigin.X;
                        double deltaY = myBlock.Position.Y - ptPositionOrigin.Y;

                        string myLine = deltaX + "|" + deltaY + "|" + "0\n";
                        myContent += myLine;

                    }

                    acDoc.Editor.WriteMessage(myContent);
                }

            }


        }



        // Pick object to set origin
        [CommandMethod("EXC2")]
        public static void ExportCoordinate2()
        {
            //if (!CheckLicense.licensed) return;

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                BlockTable acBlkTbl;
                BlockTableRecord acBlkTblRec;

                // Open Model space for write
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                if (Application.GetSystemVariable("CVPORT").ToString() != "1")
                {
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;
                }
                else
                {
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace],
                            OpenMode.ForWrite) as BlockTableRecord;
                }

                ObjectId myObjId = myCustomFunctions.GetObjectIdByType("INSERT,BLOCK,BlockReference");
                //ObjectId myObjId = myCustomFunctions.GetObjectIdByType("ABC,AlignedDimension,ArcDimension,DiametricDimension,LineAngularDimension2,Point3AngularDimension,RadialDimension,RadialDimensionLarge,RotatedDimension");
                if (myObjId.ToString() == "0") return;
                if (myObjId == new ObjectId()) return;

                BlockReference myBlockorigin = myObjId.GetObject(OpenMode.ForRead) as BlockReference;

                Point3d ptPositionOrigin = new Point3d(myBlockorigin.Position.X, myBlockorigin.Position.Y, 0);

                // Select blocks to get coordinate

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "INSERT,BLOCK"), 0);


                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<string> myLines = new List<string>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        BlockReference myBlock = acSSObj.ObjectId.GetObject(OpenMode.ForRead) as BlockReference;

                        //acDoc.Editor.WriteMessage("ObjectSelect has ID: " + acSSObj.GetType().ToString() + "\n");
                        //acDoc.Editor.WriteMessage("ObjectSelect has POSITION: " + myBlock.Position + "\n");

                        double deltaX = Math.Round(myBlock.Position.X - ptPositionOrigin.X, 3);
                        double deltaY = Math.Round(myBlock.Position.Y - ptPositionOrigin.Y, 3);

                        string myLine = deltaX + "|" + deltaY + "|" + "0\n";

                        if (!myLines.Contains(myLine))
                        { 
                            if(deltaX !=0 || deltaY != 0)
                            {
                                myLines.Add(myLine);
                            }
                        }
                    }
                    SaveFileDialog mySaveDialog = new SaveFileDialog("Savetxt", "OutPutTxtCoordinate", "txt","SaveFile",SaveFileDialog.SaveFileDialogFlags.AllowAnyExtension);

                    string myPathSave = "";
                    if (mySaveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        myPathSave = @mySaveDialog.Filename;
                        using (StreamWriter sw = new StreamWriter(myPathSave))
                        {
                            foreach(string myLine in myLines)
                            {
                                sw.WriteLine(myLine);
                            }
                        }
                    }
                }
            }

        }




        // Pick object to set origin
        [CommandMethod("C2C")]
        public static void SaveCoordinate2Clipboard()
        {
            //if (!CheckLicense.licensed) return;

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                BlockTable acBlkTbl;
                BlockTableRecord acBlkTblRec;

                // Open Model space for write
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                if (Application.GetSystemVariable("CVPORT").ToString() != "1")
                {
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;
                }
                else
                {
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace],
                            OpenMode.ForWrite) as BlockTableRecord;
                }

                ObjectId myObjId = myCustomFunctions.GetObjectIdByType("INSERT,BLOCK,BlockReference");
                //ObjectId myObjId = myCustomFunctions.GetObjectIdByType("ABC,AlignedDimension,ArcDimension,DiametricDimension,LineAngularDimension2,Point3AngularDimension,RadialDimension,RadialDimensionLarge,RotatedDimension");
                if (myObjId.ToString() == "0") return;
                if (myObjId == new ObjectId()) return;

                BlockReference myBlockorigin = myObjId.GetObject(OpenMode.ForRead) as BlockReference;

                Point3d ptPositionOrigin = new Point3d(myBlockorigin.Position.X, myBlockorigin.Position.Y, 0);

                // Select blocks to get coordinate

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "INSERT,BLOCK"), 0);


                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<string> myLines = new List<string>();
                    

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        BlockReference myBlock = acSSObj.ObjectId.GetObject(OpenMode.ForRead) as BlockReference;

                        //acDoc.Editor.WriteMessage("ObjectSelect has ID: " + acSSObj.GetType().ToString() + "\n");
                        //acDoc.Editor.WriteMessage("ObjectSelect has POSITION: " + myBlock.Position + "\n");

                        double deltaX = Math.Round(myBlock.Position.X - ptPositionOrigin.X, 3);
                        double deltaY = Math.Round(myBlock.Position.Y - ptPositionOrigin.Y, 3);

                        string myLine = deltaX + "|" + deltaY + "|" + "0";


                        if (!myLines.Contains(myLine))
                        {
                            if (deltaX != 0 || deltaY != 0)
                            {
                                myLines.Add(myLine);
                            }
                        }


                    }
                    string myString = string.Join(";", myLines);

                    f.Clipboard.Clear();

                    f.Clipboard.SetText(myString);

                    string a = f.Clipboard.GetText();

                    acDoc.Editor.WriteMessage(a);
                }
            }
        }
    }
}

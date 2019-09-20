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


using commonFunctions;
using System.IO;
//using System.Windows.Forms;

namespace myCustomCmds
{

    public class PolylineArea
    {
        public static void singlePickPolyGetDim(string nameArea, Polyline myPolySelected, int indexColor)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
            Database acCurDb = acDoc.Database;

            CmdLayer.createALayerByName("TEXT_QUANTITY");

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

                double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;

                // remove duplicate point

                myPolySelected.removePointDup();

                if (myPolySelected.NumberOfVertices < 3) return;

                if (myPolySelected.Area == 0) return;

                myPolySelected.Closed = true;

                //remove 

                // Lay thong tin center point cua plyline
                Point3d myMinPoint = myPolySelected.GeometricExtents.MinPoint;
                Point3d myMaxPoint = myPolySelected.GeometricExtents.MaxPoint;

                Point3d myCenterPoint = new Point3d((myMinPoint.X + myMaxPoint.X) / 2,
                                                    (myMinPoint.Y + myMaxPoint.Y) / 2, 0);

                double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X),0);
                double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y),0);


                using (DBText myDbTextNameArea = new DBText())
                {

                    myDbTextNameArea.TextString = nameArea;
                    myDbTextNameArea.Position = myCenterPoint;
                    myDbTextNameArea.Justify = AttachmentPoint.MiddleCenter;
                    myDbTextNameArea.AlignmentPoint = myCenterPoint;
                    myDbTextNameArea.Layer = "TEXT_QUANTITY";
                    myDbTextNameArea.ColorIndex = indexColor;

                    //Dieu chinh theo dimension neu co the.
                    myDbTextNameArea.Height = 12;

                    if (deltaY > deltaX)
                    {
                        myDbTextNameArea.Rotation = Math.PI/2;
                    }


                    acBlkTblRec.AppendEntity(myDbTextNameArea);
                    acTrans.AddNewlyCreatedDBObject(myDbTextNameArea, true);
                }
                acTrans.Commit();
            }
        }


        [CommandMethod("EXQ")]
        public static void getQuantityByText()
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

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[2];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "TEXT"), 0);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "TEXT_QUANTITY"), 1);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<string> myListTextValid = new List<string>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        DBText myTextItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as DBText;

                        int numberOrChar = myTextItem.TextString.Count(f => f == '|');
                        if (numberOrChar == 4)
                        {
                            myListTextValid.Add(myTextItem.TextString);
                        }
                        continue;
                    }
    

                    StringBuilder csvContent = new StringBuilder();
                    // Ghi header row
                    csvContent.AppendLine("TEN, DAI, RONG, CHIEU DAY, SO LUONG, VAT LIEU");


                    Dictionary<string, int> myDicTextName = new Dictionary<string, int>();

                    foreach (string fullNameText in myListTextValid)
                    {

                        if (myDicTextName.ContainsKey(fullNameText))
                        {
                            myDicTextName[fullNameText]++;
                        }
                        else
                        {
                            myDicTextName.Add(fullNameText, 1);
                        }

                    }

                    // Write file csv
                    foreach (KeyValuePair<string, int> myPlateName in myDicTextName)
                    {

                        string nameTextToList = myPlateName.Key;
                        List<string> myDetailText = nameTextToList.Split('|').ToList();


                        string lineToWrite = String.Format("{0},{1},{2},{3},{4},{5}", myPlateName.Key, myDetailText[1], myDetailText[2], myDetailText[3], myPlateName.Value, myDetailText[4]);
                        csvContent.AppendLine(lineToWrite);

                    }

                    try
                    {
                        string pathCsv = "C:\\PTKLTemp.csv";
                        File.AppendAllText(pathCsv, csvContent.ToString());
                        System.Diagnostics.Process.Start(pathCsv);
                        acTrans.Commit();
                        return;
                    }
                    catch
                    {
                        Application.ShowAlertDialog("File C://PTKLTemp.csv có thể đang được mở, đóng file va thử lại!");
                    }

                }

            }

        }




        [CommandMethod("DFT")]
        public static void drawFromText()
        {
            if (!CheckLicense.licensed) return;

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

                double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;


                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[2];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "TEXT"), 0);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "TEXT_QUANTITY"), 1);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<string> myListTextValid = new List<string>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        DBText myTextItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as DBText;

                        int numberOrChar = myTextItem.TextString.Count(f => f == '|');
                        if (numberOrChar == 4)
                        {
                            myListTextValid.Add(myTextItem.TextString);
                        }
                        continue;
                    }

                    // lay duoc danh sach cac text phu hop
                    
                    // Duyet tung phan tu trong danh sach vua tao duoc, dem so phan tu de lay so luong

                    Dictionary<string, int> myDicTextName = new Dictionary<string, int>();

                    foreach (string fullNameText in myListTextValid)
                    {
                        if (myDicTextName.ContainsKey(fullNameText))
                        {
                            myDicTextName[fullNameText]++;
                        }
                        else
                        {
                            myDicTextName.Add(fullNameText, 1);
                        }
                    }

                    
                    // Chọn 1 diem tren man hinh de pick insert
                    PromptPointResult pPtRes;
                    PromptPointOptions pPtOpts = new PromptPointOptions("");

                    // Prompt for the start point
                    pPtOpts.Message = "\nPick a point to place Details: ";
                    pPtRes = acDoc.Editor.GetPoint(pPtOpts);

                    // Exit if the user presses ESC or cancels the command
                    if (pPtRes.Status == PromptStatus.Cancel) return;

                    Point3d ptPositionInsert = pPtRes.Value;



                    // Get info from text
                    foreach (KeyValuePair<string, int> myPlateName in myDicTextName)
                    {

                        string nameTextToList = myPlateName.Key;
                        List<string> myDetailText = nameTextToList.Split('|').ToList();

                        // Height
                        double height = Convert.ToInt32(myDetailText[2]);

                        // Width
                        double width = Convert.ToInt32(myDetailText[1]);

                        //Thickness
                        double thickness = Convert.ToInt32(myDetailText[3]);

                        //Material
                        string myTitle = nameTextToList + "\n" + "SL: " + myPlateName.Value;

                        // Draw detail plate and view of plate
                        CmdDetailParts.drawDetailFromText(ptPositionInsert, width, height, thickness, myTitle);

                        // Chuyen ptPoint sang diem moi

                        ptPositionInsert = new Point3d(ptPositionInsert.X + 60 * scaleCurrentDim +width+thickness+ 1000, ptPositionInsert.Y, 0);

                        //string lineToWrite = String.Format("{0},{1},{2},{3},{4},{5}", myPlateName.Key, myDetailText[1], myDetailText[2], myDetailText[3], myPlateName.Value, myDetailText[4]);

                    }

                    acTrans.Commit();
                    return;
                }

            }

        }



        //[CommandMethod("DFF")]
        public static void drawFromCSVFile(Dictionary<string, int> myDicTextName)
        {
            if (!CheckLicense.licensed) return;

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (DocumentLock docLock = acDoc.LockDocument())
            {

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

                    double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;


                    // lay duoc danh sach cac text phu hoc
                    // Duyet tung phan tu trong danh sach vua tao duoc, dem so phan tu de lay so luong


                    // Chọn 1 diem tren man hinh de pick insert
                    PromptPointResult pPtRes;
                    PromptPointOptions pPtOpts = new PromptPointOptions("");

                    // Prompt for the start point
                    pPtOpts.Message = "\nPick a point to place Details: ";
                    pPtRes = acDoc.Editor.GetPoint(pPtOpts);

                    // Exit if the user presses ESC or cancels the command
                    if (pPtRes.Status == PromptStatus.Cancel) return;

                    Point3d ptPositionInsert = pPtRes.Value;


                    // Get info from text
                    foreach (KeyValuePair<string, int> myPlateName in myDicTextName)
                    {

                        string nameTextToList = myPlateName.Key;
                        List<string> myDetailText = nameTextToList.Split('|').ToList();

                        // Height
                        double height = Convert.ToInt32(myDetailText[2]);

                        // Width
                        double width = Convert.ToInt32(myDetailText[1]);

                        //Thickness
                        double thickness = Convert.ToInt32(myDetailText[3]);

                        //Material
                        string myTitle = nameTextToList + "\n" + "SL: " + myPlateName.Value;

                        // Draw detail plate and view of plate
                        CmdDetailParts.drawDetailFromText(ptPositionInsert, width, height, thickness, myTitle);

                        // Chuyen ptPoint sang diem moi

                        ptPositionInsert = new Point3d(ptPositionInsert.X + 60 * scaleCurrentDim + width + thickness + 1000, ptPositionInsert.Y, 0);

                        //string lineToWrite = String.Format("{0},{1},{2},{3},{4},{5}", myPlateName.Key, myDetailText[1], myDetailText[2], myDetailText[3], myPlateName.Value, myDetailText[4]);

                    }

                    acTrans.Commit();
                    return;
                }

            }
        }
       

        [CommandMethod("ETQ")]
        public static void eraseQuantityText()
        {
            if (!CheckLicense.licensed) return;

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

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[2];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "TEXT"), 0);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "TEXT_QUANTITY"), 1);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<string> myListTextValid = new List<string>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        DBText myTextItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as DBText;

                        myTextItem.Erase(true);
                    }

                    try
                    {
                        acTrans.Commit();
                        acDoc.Editor.WriteMessage("Complete!");
                        return;
                    }
                    catch
                    {
                        Application.ShowAlertDialog("File C://PTKLTemp.csv có thể đang được mở, đóng file va thử lại!");
                    }

                }

            }

        }


        [CommandMethod("OTF")]
        public static void openTempQuanTityFile()
        {
            if (!CheckLicense.licensed) return;

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            try
            {
                string pathCsv = "C:\\PTKLTemp.csv";
                System.Diagnostics.Process.Start(pathCsv);

                return;
            }
            catch
            {
                Application.ShowAlertDialog("File C:\\PTKLTemp.csv có thể đang được mở hoặc bị xóa, đóng file và thử lại!");
            }

        }



        //[CommandMethod("getAreaDim")]
        //public static void getNameArea()
        //{
        //     Document acDoc = Application.DocumentManager.MdiActiveDocument;
        //    Database acCurDb = acDoc.Database;

        //    // Start a transaction
        //    using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //    {

        //        BlockTable acBlkTbl;
        //        BlockTableRecord acBlkTblRec;

        //        // Open Model space for write
        //        acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
        //                                        OpenMode.ForRead) as BlockTable;

        //        if (Application.GetSystemVariable("CVPORT").ToString() != "1")
        //        {
        //            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
        //                                        OpenMode.ForWrite) as BlockTableRecord;
        //        }
        //        else
        //        {
        //            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace],
        //                    OpenMode.ForWrite) as BlockTableRecord;
        //        }

        //        // Create a TypedValue array to define the filter criteria
        //        TypedValue[] acTypValAr = new TypedValue[1];
        //        acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE,LWPOLYLINE"), 0);

        //        // Assign the filter criteria to a SelectionFilter object
        //        SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

        //        // Request for objects to be selected in the drawing area
        //        PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

        //        // If the prompt status is OK, objects were selected
        //        if (acSSPrompt.Status == PromptStatus.OK)
        //        {
        //            SelectionSet acSSet = acSSPrompt.Value;
        //            if (acSSet == null) return;

        //            List<Polyline> myListPolyValid = new List<Polyline>();

        //            foreach (SelectedObject acSSObj in acSSet)
        //            {
        //                Polyline myPolylineItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as Polyline;

        //                if (myPolylineItem.Area > 0 && myPolylineItem.NumberOfVertices > 2)
        //                {
        //                    myListPolyValid.Add(myPolylineItem);
        //                }
        //                else
        //                {
        //                    continue;
        //                }
        //            }

        //            // input string
        //            // Create a title
        //            PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Title Callout: ");
        //            pStrOpts.AllowSpaces = true;
        //            pStrOpts.DefaultValue = "T";
        //            PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);


        //            // ghi file csv

        //            StringBuilder csvContent = new StringBuilder();
        //            // Ghi header row
        //            csvContent.AppendLine("Tên tấm, Rộng, Dài, Số Lượng");


        //            if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


        //            string mySymbolName = pStrRes.StringResult.ToUpper();

        //            Dictionary<string,int> myDic = new Dictionary<string,int>();

        //            foreach (Polyline myPoly in myListPolyValid)
        //            {
        //                Point3d myMinPoint = myPoly.GeometricExtents.MinPoint;
        //                Point3d myMaxPoint = myPoly.GeometricExtents.MaxPoint;

        //                double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X), 0);
        //                double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y), 0);


        //                string myNameArea = mySymbolName + "-" + deltaX + " x " + deltaY;

        //                if (myDic.ContainsKey(myNameArea))
        //                {
        //                    myDic[myNameArea]++;
        //                }
        //                else
        //                {
        //                    myDic.Add(myNameArea, 1);
        //                }


        //                //string lineToWrite = String.Format("{0},{1},{2},{3}", myNameArea, deltaX, deltaY, 1);

        //                //csvContent.AppendLine(lineToWrite);

        //                singlePickPolyGetDim(myNameArea, myPoly, 2);
        //            }

        //            // Write file csv
        //            foreach (KeyValuePair<string, int> myPlate in myDic)
        //            {
        //                string lineToWrite = String.Format("{0},{1}", myPlate.Key, myPlate.Value);
        //                csvContent.AppendLine(lineToWrite);
        //            }


        //            string pathCsv = "D:\\abc.csv";
        //            File.AppendAllText(pathCsv, csvContent.ToString());
        //        }
        //        acTrans.Commit();
        //        return;
        //    }
        
        //}


        /// <summary>
        /// tong hop khoi luong tam theo dien tich, nhap chieu day, vat lieu lay theo layer
        /// </summary>

        //[CommandMethod("GAD")]
        public static void getQuantityByArea()
        {
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

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE,LWPOLYLINE"), 0);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<Polyline> myListPolyValid = new List<Polyline>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Polyline myPolylineItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as Polyline;

                        if (myPolylineItem.Area > 0 && myPolylineItem.NumberOfVertices > 2)
                        {
                            myListPolyValid.Add(myPolylineItem);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    // input string
                    // Create a title
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Title Callout: ");
                    pStrOpts.AllowSpaces = true;
                    pStrOpts.DefaultValue = "T";
                    PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);


                    // ghi file csv

                    StringBuilder csvContent = new StringBuilder();
                    // Ghi header row
                    csvContent.AppendLine("TEN, RONG, DAI, SO LUONG");


                    if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


                    string mySymbolName = pStrRes.StringResult.ToUpper();

                    Dictionary<myCustomPlate, int> myDic = new Dictionary<myCustomPlate, int>(new CustomerEqualityComparer());

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        Point3d myMinPoint = myPoly.GeometricExtents.MinPoint;
                        Point3d myMaxPoint = myPoly.GeometricExtents.MaxPoint;

                        double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X), 0);
                        double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y), 0);


                        string myNameArea = mySymbolName + "-" + deltaX + " x " + deltaY;

                        myCustomPlate myPlate = new myCustomPlate(myNameArea, deltaX, deltaY);

                        if (myDic.ContainsKey(myPlate))
                        {
                            myDic[myPlate]++;
                        }
                        else
                        {
                            myDic.Add(myPlate, 1);
                        }


                        //string lineToWrite = String.Format("{0},{1},{2},{3}", myNameArea, deltaX, deltaY, 1);

                        //csvContent.AppendLine(lineToWrite);

                        singlePickPolyGetDim(myNameArea, myPoly, 2);
                    }

                    // Write file csv
                    foreach (KeyValuePair<myCustomPlate, int> myPlate in myDic)
                    {
                        string lineToWrite = String.Format("{0},{1},{2},{3}", myPlate.Key.Name, myPlate.Key.Width, myPlate.Key.Height, myPlate.Value);
                        csvContent.AppendLine(lineToWrite);

                    }

                    try
                    {
                        string pathCsv = "D:\\PTKLTemp.csv";
                        File.AppendAllText(pathCsv, csvContent.ToString());
                        System.Diagnostics.Process.Start(pathCsv);
                        acTrans.Commit();
                        return;
                    }
                    catch
                    {
                        Application.ShowAlertDialog("File D://PTKLTemp.csv có thể đang được mở, đóng file va thử lại!");
                    }

                }

            }

        }



        /// <summary>
        /// tong hop Kl theo mat cat ngang, nhap chieu rong, vat lieu lay theo layer
        /// </summary>
        [CommandMethod("GPDMax")]
        public static void getQuantityPlate()
        {
            if (!CheckLicense.licensed) return;

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

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE,LWPOLYLINE"), 0);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<Polyline> myListPolyValid = new List<Polyline>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Polyline myPolylineItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as Polyline;

                        if (myPolylineItem.Area > 0 && myPolylineItem.NumberOfVertices > 2)
                        {
                            myListPolyValid.Add(myPolylineItem);
                        }
                        else
                        {
                            continue;
                        }
                    }


                    // Nhap chieu sau cua tam (kich thuoc theo phuong con lai)

                    PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
                    pIntOpts.Message = "\nNHẬP CHIỀU DÀY TẤM, ENTER ĐỂ PICK TRÊN MÀN HÌNH: ";
                    //pIntOpts.DefaultValue = -1;
                    pIntOpts.AllowNone = true;
                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);

                    if (pIntRes.Value <= 0)
                    {
                        pIntRes = acDoc.Editor.GetDistance("\nPick two points: ");
                    }

                    if (pIntRes.Value <= 0 ) return;

                    double thickness = pIntRes.Value;


                    // input string
                    // Create a title
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nNHẬP KÍ HIỆU TẤM: ");
                    pStrOpts.AllowSpaces = true;
                    pStrOpts.DefaultValue = "T";
                    PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);


                    // ghi file csv

                    StringBuilder csvContent = new StringBuilder();
                    // Ghi header row
                    csvContent.AppendLine("TEN, DAI, RONG, CHIEU DAY, SO LUONG, VAT LIEU");


                    if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


                    string mySymbolName = pStrRes.StringResult.ToUpper();

                    Dictionary<myCustomSection, int> myDic = new Dictionary<myCustomSection, int>(new CustomerSectionEqualityComparer());

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        Point3d myMinPoint = myPoly.GeometricExtents.MinPoint;
                        Point3d myMaxPoint = myPoly.GeometricExtents.MaxPoint;

                        double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X), 0);
                        double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y), 0);

                        double length = Math.Max(deltaX, deltaY);
                        double width = Math.Min(deltaX, deltaY);


                        string myMaterial = myPoly.Layer;

                        string myNamePlate = mySymbolName + "|" + length + "|" + width + "|" + thickness + "|" + myMaterial;


                        myCustomSection mySection = new myCustomSection(myNamePlate, length, width, thickness, myMaterial);


                        if (myDic.ContainsKey(mySection))
                        {
                            myDic[mySection]++;
                        }
                        else
                        {
                            myDic.Add(mySection, 1);
                        }


                        //string lineToWrite = String.Format("{0},{1},{2},{3}", myNameArea, deltaX, deltaY, 1);

                        //csvContent.AppendLine(lineToWrite);

                        singlePickPolyGetDim(myNamePlate, myPoly, 2);
                    }

                    // Write file csv
                    foreach (KeyValuePair<myCustomSection, int> myPlateSec in myDic)
                    {
                        string lineToWrite = String.Format("{0},{1},{2},{3},{4},{5}", myPlateSec.Key.Name, myPlateSec.Key.Length, myPlateSec.Key.Width, myPlateSec.Key.Thickness, myPlateSec.Value, myPlateSec.Key.Material);
                        csvContent.AppendLine(lineToWrite);

                    }

                    try
                    {
                        string pathCsv = "D:\\PTKLTemp.csv";
                        //File.AppendAllText(pathCsv, csvContent.ToString());
                        //System.Diagnostics.Process.Start(pathCsv);
                        acTrans.Commit();
                        return;
                    }
                    catch
                    {
                        Application.ShowAlertDialog("File D://PTKLTemp.csv có thể đang được mở, đóng file va thử lại!");
                    }

                }

            }

        }

        /// <summary>
        /// tong hop Kl theo mat cat ngang, nhap chieu rong, vat lieu lay theo layer
        /// </summary>
        [CommandMethod("GYD")]
        public static void getQuantityPlateY()
        {
            if (!CheckLicense.licensed) return;
            //SetLayerCurrent("DIM");
            CmdLayer.createALayerByName("GRAIN_HATCH");
            CmdLayer.createALayerByName("TEXT_QUANTITY");

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

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE,LWPOLYLINE"), 0);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<Polyline> myListPolyValid = new List<Polyline>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Polyline myPolylineItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as Polyline;

                        if (myPolylineItem.Area > 0 && myPolylineItem.NumberOfVertices > 2)
                        {
                            myListPolyValid.Add(myPolylineItem);
                        }
                        else
                        {
                            continue;
                        }
                    }


                    // Nhap chieu sau cua tam (kich thuoc theo phuong con lai)

                    PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
                    pIntOpts.Message = "\nNHẬP CHIỀU DÀY TẤM, ENTER ĐỂ PICK TRÊN MÀN HÌNH: ";
                    //pIntOpts.DefaultValue = -1;
                    pIntOpts.AllowNone = true;
                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);

                    if (pIntRes.Value <= 0)
                    {
                        pIntRes = acDoc.Editor.GetDistance("\nPick two points: ");
                    }

                    if (pIntRes.Value <= 0) return;

                    double thickness = pIntRes.Value;


                    // input string
                    // Create a title
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nNHẬP KÍ HIỆU TẤM: ");
                    pStrOpts.AllowSpaces = true;
                    pStrOpts.DefaultValue = "T";
                    PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);


                    // ghi file csv

                    StringBuilder csvContent = new StringBuilder();
                    // Ghi header row
                    csvContent.AppendLine("TEN, DAI, RONG, CHIEU DAY, SO LUONG, VAT LIEU");


                    if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


                    string mySymbolName = pStrRes.StringResult.ToUpper();

                    Dictionary<myCustomSection, int> myDic = new Dictionary<myCustomSection, int>(new CustomerSectionEqualityComparer());

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        Point3d myMinPoint = myPoly.GeometricExtents.MinPoint;
                        Point3d myMaxPoint = myPoly.GeometricExtents.MaxPoint;

                        double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X), 0);
                        double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y), 0);

                        double length = deltaY;
                        double width = deltaX;


                        string myMaterial = myPoly.Layer;

                        string myNamePlate = mySymbolName + "|" + length + "|" + width + "|" + thickness + "|" + myMaterial;


                        myCustomSection mySection = new myCustomSection(myNamePlate, length, width, thickness, myMaterial);


                        // Tao hatch chi huong

                        ObjectIdCollection acObjIdColl = new ObjectIdCollection();
                        acObjIdColl.Add(myPoly.ObjectId);

                        using (Hatch acHatch = new Hatch())
                        {
                            acBlkTblRec.AppendEntity(acHatch);
                            acTrans.AddNewlyCreatedDBObject(acHatch, true);

                            // Set the properties of the hatch object
                            // Associative must be set after the hatch object is appended to the 
                            // block table record and before AppendLoop

                            acHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI35");
                            acHatch.Associative = true;
                            acHatch.PatternScale = 10;
                            acHatch.ColorIndex = 8;
                            acHatch.PatternAngle = Math.PI / 4;
                            acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                            acHatch.EvaluateHatch(true);
                            //Create layer Hatch
                            //CmdLayer.createALayerByName("HATCH");
                            acHatch.Layer = "GRAIN_HATCH";
                        }



                        if (myDic.ContainsKey(mySection))
                        {
                            myDic[mySection]++;
                        }
                        else
                        {
                            myDic.Add(mySection, 1);
                        }


                        //string lineToWrite = String.Format("{0},{1},{2},{3}", myNameArea, deltaX, deltaY, 1);

                        //csvContent.AppendLine(lineToWrite);

                        singlePickPolyGetDim(myNamePlate, myPoly, 3);
                    }

                    // Write file csv
                    foreach (KeyValuePair<myCustomSection, int> myPlateSec in myDic)
                    {
                        string lineToWrite = String.Format("{0},{1},{2},{3},{4},{5}", myPlateSec.Key.Name, myPlateSec.Key.Length, myPlateSec.Key.Width, myPlateSec.Key.Thickness, myPlateSec.Value, myPlateSec.Key.Material);
                        csvContent.AppendLine(lineToWrite);

                    }

                    try
                    {
                        string pathCsv = "D:\\PTKLTemp.csv";
                        //File.AppendAllText(pathCsv, csvContent.ToString());
                        //System.Diagnostics.Process.Start(pathCsv);
                        acTrans.Commit();
                        return;
                    }
                    catch
                    {
                        Application.ShowAlertDialog("File D://PTKLTemp.csv có thể đang được mở, đóng file va thử lại!");
                    }

                }

            }

        }

        [CommandMethod("GXD")]
        public static void getQuantityPlateX()
        {
            if (!CheckLicense.licensed) return;
            //SetLayerCurrent("DIM");
            CmdLayer.createALayerByName("GRAIN_HATCH");
            CmdLayer.createALayerByName("TEXT_QUANTITY");

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

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE,LWPOLYLINE"), 0);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<Polyline> myListPolyValid = new List<Polyline>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Polyline myPolylineItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as Polyline;

                        if (myPolylineItem.Area > 0 && myPolylineItem.NumberOfVertices > 2)
                        {
                            myListPolyValid.Add(myPolylineItem);
                        }
                        else
                        {
                            continue;
                        }

                    }


                    // Nhap chieu sau cua tam (kich thuoc theo phuong con lai)
                    PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
                    pIntOpts.Message = "\nNHẬP CHIỀU DÀY TẤM, ENTER ĐỂ PICK TRÊN MÀN HÌNH: ";
                    //pIntOpts.DefaultValue = -1;
                    pIntOpts.AllowNone = true;
                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);

                    if (pIntRes.Value <= 0)
                    {
                        pIntRes = acDoc.Editor.GetDistance("\nPick two points: ");
                    }

                    if (pIntRes.Value <= 0) return;

                    double thickness = pIntRes.Value;


                    // input string
                    // Create a title
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nNHẬP KÍ HIỆU TẤM: ");
                    pStrOpts.AllowSpaces = true;
                    pStrOpts.DefaultValue = "T";
                    PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);


                    // ghi file csv

                    StringBuilder csvContent = new StringBuilder();
                    // Ghi header row
                    csvContent.AppendLine("TEN, DAI, RONG, CHIEU DAY, SO LUONG, VAT LIEU");


                    if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


                    string mySymbolName = pStrRes.StringResult.ToUpper();

                    Dictionary<myCustomSection, int> myDic = new Dictionary<myCustomSection, int>(new CustomerSectionEqualityComparer());

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        Point3d myMinPoint = myPoly.GeometricExtents.MinPoint;
                        Point3d myMaxPoint = myPoly.GeometricExtents.MaxPoint;

                        double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X), 0);
                        double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y), 0);

                        double length = deltaX;
                        double width = deltaY;


                        string myMaterial = myPoly.Layer;

                        string myNamePlate = mySymbolName + "|" + length + "|" + width + "|" + thickness + "|" + myMaterial;


                        myCustomSection mySection = new myCustomSection(myNamePlate, length, width, thickness, myMaterial);

                        // Tao hatch chi huong

                        ObjectIdCollection acObjIdColl = new ObjectIdCollection();
                        acObjIdColl.Add(myPoly.ObjectId);

                        using (Hatch acHatch = new Hatch())
                        {
                            acBlkTblRec.AppendEntity(acHatch);
                            acTrans.AddNewlyCreatedDBObject(acHatch, true);

                            // Set the properties of the hatch object
                            // Associative must be set after the hatch object is appended to the 
                            // block table record and before AppendLoop

                            acHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI35");
                            acHatch.Associative = true;
                            acHatch.PatternScale = 10;
                            acHatch.ColorIndex = 8;
                            acHatch.PatternAngle = Math.PI / 4 * 3;
                            acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                            acHatch.EvaluateHatch(true);
                            acHatch.Layer = "GRAIN_HATCH";
                        }


                        if (myDic.ContainsKey(mySection))
                        {
                            myDic[mySection]++;
                        }
                        else
                        {
                            myDic.Add(mySection, 1);
                        }


                        //string lineToWrite = String.Format("{0},{1},{2},{3}", myNameArea, deltaX, deltaY, 1);

                        //csvContent.AppendLine(lineToWrite);

                        singlePickPolyGetDim(myNamePlate, myPoly, 1);
                    }

                    // Write file csv
                    foreach (KeyValuePair<myCustomSection, int> myPlateSec in myDic)
                    {
                        string lineToWrite = String.Format("{0},{1},{2},{3},{4},{5}", myPlateSec.Key.Name, myPlateSec.Key.Length, myPlateSec.Key.Width, myPlateSec.Key.Thickness, myPlateSec.Value, myPlateSec.Key.Material);
                        csvContent.AppendLine(lineToWrite);

                    }

                    try
                    {
                        string pathCsv = "D:\\PTKLTemp.csv";
                        //File.AppendAllText(pathCsv, csvContent.ToString());
                        //System.Diagnostics.Process.Start(pathCsv);
                        acTrans.Commit();
                        return;
                    }
                    catch
                    {
                        Application.ShowAlertDialog("File D://PTKLTemp.csv có thể đang được mở, đóng file va thử lại!");
                    }

                }

            }

        }

        [CommandMethod("CGD")]
        public static void changeGrainDirect()
        {
            if (!CheckLicense.licensed) return;
            //SetLayerCurrent("DIM");
            CmdLayer.createALayerByName("GRAIN_HATCH");
            CmdLayer.createALayerByName("TEXT_QUANTITY");

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

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE,LWPOLYLINE"), 0);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<Polyline> myListPolyValid = new List<Polyline>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Polyline myPolylineItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as Polyline;

                        if (myPolylineItem.Area > 0 && myPolylineItem.NumberOfVertices > 2)
                        {
                            myListPolyValid.Add(myPolylineItem);
                        }
                        else
                        {
                            continue;
                        }

                    }


                    // Nhap chieu sau cua tam (kich thuoc theo phuong con lai)
                    PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
                    pIntOpts.Message = "\nNHẬP CHIỀU DÀY TẤM, ENTER ĐỂ PICK TRÊN MÀN HÌNH: ";
                    //pIntOpts.DefaultValue = -1;
                    pIntOpts.AllowNone = true;
                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);

                    if (pIntRes.Value <= 0)
                    {
                        pIntRes = acDoc.Editor.GetDistance("\nPick two points: ");
                    }

                    if (pIntRes.Value <= 0) return;

                    double thickness = pIntRes.Value;


                    // input string
                    // Create a title
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nNHẬP KÍ HIỆU TẤM: ");
                    pStrOpts.AllowSpaces = true;
                    pStrOpts.DefaultValue = "T";
                    PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);


                    // ghi file csv

                    StringBuilder csvContent = new StringBuilder();
                    // Ghi header row
                    csvContent.AppendLine("TEN, DAI, RONG, CHIEU DAY, SO LUONG, VAT LIEU");


                    if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


                    string mySymbolName = pStrRes.StringResult.ToUpper();

                    Dictionary<myCustomSection, int> myDic = new Dictionary<myCustomSection, int>(new CustomerSectionEqualityComparer());

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        Point3d myMinPoint = myPoly.GeometricExtents.MinPoint;
                        Point3d myMaxPoint = myPoly.GeometricExtents.MaxPoint;

                        double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X), 0);
                        double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y), 0);

                        double length = deltaX;
                        double width = deltaY;


                        string myMaterial = myPoly.Layer;

                        string myNamePlate = mySymbolName + "|" + length + "|" + width + "|" + thickness + "|" + myMaterial;


                        myCustomSection mySection = new myCustomSection(myNamePlate, length, width, thickness, myMaterial);

                        // Tao hatch chi huong

                        ObjectIdCollection acObjIdColl = new ObjectIdCollection();
                        acObjIdColl.Add(myPoly.ObjectId);

                        using (Hatch acHatch = new Hatch())
                        {
                            acBlkTblRec.AppendEntity(acHatch);
                            acTrans.AddNewlyCreatedDBObject(acHatch, true);

                            // Set the properties of the hatch object
                            // Associative must be set after the hatch object is appended to the 
                            // block table record and before AppendLoop

                            acHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI35");
                            acHatch.Associative = true;
                            acHatch.PatternScale = 10;
                            acHatch.ColorIndex = 8;
                            acHatch.PatternAngle = Math.PI / 4 * 3;
                            acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                            acHatch.EvaluateHatch(true);
                            acHatch.Layer = "GRAIN_HATCH";
                        }


                        if (myDic.ContainsKey(mySection))
                        {
                            myDic[mySection]++;
                        }
                        else
                        {
                            myDic.Add(mySection, 1);
                        }


                        //string lineToWrite = String.Format("{0},{1},{2},{3}", myNameArea, deltaX, deltaY, 1);

                        //csvContent.AppendLine(lineToWrite);

                        singlePickPolyGetDim(myNamePlate, myPoly, 1);
                    }

                    // Write file csv
                    foreach (KeyValuePair<myCustomSection, int> myPlateSec in myDic)
                    {
                        string lineToWrite = String.Format("{0},{1},{2},{3},{4},{5}", myPlateSec.Key.Name, myPlateSec.Key.Length, myPlateSec.Key.Width, myPlateSec.Key.Thickness, myPlateSec.Value, myPlateSec.Key.Material);
                        csvContent.AppendLine(lineToWrite);

                    }

                    try
                    {
                        string pathCsv = "D:\\PTKLTemp.csv";
                        //File.AppendAllText(pathCsv, csvContent.ToString());
                        //System.Diagnostics.Process.Start(pathCsv);
                        acTrans.Commit();
                        return;
                    }
                    catch
                    {
                        Application.ShowAlertDialog("File D://PTKLTemp.csv có thể đang được mở, đóng file va thử lại!");
                    }

                }

            }

        }



        /// <summary>
        /// tong hop Kl theo mat cat ngang, nhap chieu rong, vat lieu lay theo layer, TU LAM TRON TOI HANG TRAM
        /// </summary>
        [CommandMethod("GPD2")]
        public static void getQuantityByPlateRound()
        {
            if (!CheckLicense.licensed) return;

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

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE,LWPOLYLINE"), 0);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<Polyline> myListPolyValid = new List<Polyline>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Polyline myPolylineItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as Polyline;

                        if (myPolylineItem.Area > 0 && myPolylineItem.NumberOfVertices > 2)
                        {
                            myListPolyValid.Add(myPolylineItem);
                        }
                        else
                        {
                            continue;
                        }
                    }


                    // Nhap chieu sau cua tam (kich thuoc theo phuong con lai)

                    PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
                    pIntOpts.Message = "\nNHẬP CHIỀU DÀY TẤM, ENTER ĐỂ PICK TRÊN MÀN HÌNH: ";
                    //pIntOpts.DefaultValue = -1;
                    pIntOpts.AllowNone = true;
                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);


                    if (pIntRes.Value <= 0)
                    {
                        pIntRes = acDoc.Editor.GetDistance("\nPick two points: ");
                    }

                    if (pIntRes.Value <= 0) return;

                    double thickness = pIntRes.Value;


                    // input string
                    // Create a title
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nNHẬP KÍ HIỆU TẤM: ");
                    pStrOpts.AllowSpaces = true;
                    pStrOpts.DefaultValue = "T";
                    PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);


                    // ghi file csv

                    StringBuilder csvContent = new StringBuilder();
                    // Ghi header row
                    csvContent.AppendLine("TEN, DAI, RONG, CHIEU DAY, SO LUONG, VAT LIEU");


                    if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


                    string mySymbolName = pStrRes.StringResult.ToUpper();

                    Dictionary<myCustomSection, int> myDic = new Dictionary<myCustomSection, int>(new CustomerSectionEqualityComparer());

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        Point3d myMinPoint = myPoly.GeometricExtents.MinPoint;
                        Point3d myMaxPoint = myPoly.GeometricExtents.MaxPoint;

                        double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X), 0);
                        double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y), 0);

                        double length = Math.Max(deltaX, deltaY);

                        length = (Math.Round(length / 100, 0) + 1) * 100;

                        double widthRaw = Math.Min(deltaX, deltaY);

                        double width = (Math.Round(widthRaw / 100, 0) + 1) * 100;

                        string myMaterial = myPoly.Layer;

                        string myNamePlate = mySymbolName + "|" + length + "|" + width + "|" + thickness + "|" + myMaterial;


                        myCustomSection mySection = new myCustomSection(myNamePlate, length, width, thickness, myMaterial);

                        if (myDic.ContainsKey(mySection))
                        {
                            myDic[mySection]++;
                        }
                        else
                        {
                            myDic.Add(mySection, 1);
                        }


                        //string lineToWrite = String.Format("{0},{1},{2},{3}", myNameArea, deltaX, deltaY, 1);

                        //csvContent.AppendLine(lineToWrite);

                        singlePickPolyGetDim(myNamePlate, myPoly,2);
                    }

                    // Write file csv
                    foreach (KeyValuePair<myCustomSection, int> myPlateSec in myDic)
                    {
                        string lineToWrite = String.Format("{0},{1},{2},{3},{4},{5}", myPlateSec.Key.Name, myPlateSec.Key.Length, myPlateSec.Key.Width, myPlateSec.Key.Thickness, myPlateSec.Value, myPlateSec.Key.Material);
                        csvContent.AppendLine(lineToWrite);

                    }

                    try
                    {
                        string pathCsv = "D:\\PTKLTemp.csv";
                        //File.AppendAllText(pathCsv, csvContent.ToString());
                        //System.Diagnostics.Process.Start(pathCsv);
                        acTrans.Commit();
                        return;
                    }
                    catch
                    {
                        Application.ShowAlertDialog("File D://PTKLTemp.csv có thể đang được mở, đóng file va thử lại!");
                    }

                }

            }

        }
    


        /// <summary>
        /// tong hop Kl theo mat cat ngang, nhap chieu rong, vat lieu lay theo layer
        /// </summary>
        [CommandMethod("GSD")]
        public static void getQuantityBySection()
        {
            if (!CheckLicense.licensed) return;

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

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE,LWPOLYLINE"), 0);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<Polyline> myListPolyValid = new List<Polyline>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Polyline myPolylineItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as Polyline;

                        if (myPolylineItem.Area > 0 && myPolylineItem.NumberOfVertices > 2)
                        {
                            myListPolyValid.Add(myPolylineItem);
                        }
                        else
                        {
                            continue;
                        }
                    }


                    // Nhap chieu sau cua tam (kich thuoc theo phuong con lai)

                    PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
                    pIntOpts.Message = "\nNHẬP KÍCH THƯỚC CHIỀU CÒN LẠI(W/L): ";
                    //pIntOpts.DefaultValue = -1;
                    pIntOpts.AllowNone = true;
                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);


                    if (pIntRes.Value <= 0)
                    {
                        pIntRes = acDoc.Editor.GetDistance("\nPick two points: ");
                    }


                    if (pIntRes.Value <=0) return;

                    double width = pIntRes.Value;



                    // input string
                    // Create a title
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nNHẬP KÍ HIỆU TẤM: ");
                    pStrOpts.AllowSpaces = true;
                    pStrOpts.DefaultValue = "T";
                    PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);


                    // ghi file csv

                    StringBuilder csvContent = new StringBuilder();
                    // Ghi header row
                    csvContent.AppendLine("TEN, DAI, RONG, CHIEU DAY, SO LUONG, VAT LIEU");


                    if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


                    string mySymbolName = pStrRes.StringResult.ToUpper();

                    Dictionary<myCustomSection, int> myDic = new Dictionary<myCustomSection, int>(new CustomerSectionEqualityComparer());

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        Point3d myMinPoint = myPoly.GeometricExtents.MinPoint;
                        Point3d myMaxPoint = myPoly.GeometricExtents.MaxPoint;

                        double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X), 0);
                        double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y), 0);

                        double length = Math.Max(deltaX, deltaY);
                        length = Math.Max(length, Math.Round(myPoly.GetMaxSegment(),0));

                        double thickness = Math.Min(deltaX, deltaY);
                        thickness = Math.Min(thickness, myPoly.GetMinDistancepVerToLine());

                        //string OutString = "Chieu day tam: " + myPoly.GetMinDistancepVerToLine();

                        string myMaterial = myPoly.Layer;

                        string myNamePlate = mySymbolName + "|" + length + "|" + width + "|" + thickness + "|" + myMaterial;


                        myCustomSection mySection = new myCustomSection(myNamePlate, length, width, thickness, myMaterial);

                        if (myDic.ContainsKey(mySection))
                        {
                            myDic[mySection]++;
                        }
                        else
                        {
                            myDic.Add(mySection, 1);
                        }


                        //string lineToWrite = String.Format("{0},{1},{2},{3}", myNameArea, deltaX, deltaY, 1);

                        //csvContent.AppendLine(lineToWrite);

                        singlePickPolyGetDim(myNamePlate, myPoly, 6);
                    }

                    // Write file csv
                    foreach (KeyValuePair<myCustomSection, int> myPlateSec in myDic)
                    {
                        string lineToWrite = String.Format("{0},{1},{2},{3},{4},{5}", myPlateSec.Key.Name, myPlateSec.Key.Length, myPlateSec.Key.Width, myPlateSec.Key.Thickness, myPlateSec.Value, myPlateSec.Key.Material);
                        csvContent.AppendLine(lineToWrite);

                    }

                    try
                    {
                        string pathCsv = "D:\\PTKLTemp.csv";
                        //File.AppendAllText(pathCsv, csvContent.ToString());
                        //System.Diagnostics.Process.Start(pathCsv);
                        acTrans.Commit();
                        return;
                    }
                    catch
                    {
                        Application.ShowAlertDialog("File D://PTKLTemp.csv có thể đang được mở, đóng file va thử lại!");
                    }

                }

            }

        }



        /// <summary>
        /// tong hop Kl theo mat cat ngang, nhap chieu rong, vat lieu lay theo layer, TU LAM TRON TOI HANG TRAM
        /// </summary>
        [CommandMethod("GSD2")]
        public static void getQuantityBySectionRound()
        {
            if (!CheckLicense.licensed) return;

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

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE,LWPOLYLINE"), 0);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<Polyline> myListPolyValid = new List<Polyline>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Polyline myPolylineItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as Polyline;

                        if (myPolylineItem.Area > 0 && myPolylineItem.NumberOfVertices > 2)
                        {
                            myListPolyValid.Add(myPolylineItem);
                        }
                        else
                        {
                            continue;
                        }
                    }


                    // Nhap chieu sau cua tam (kich thuoc theo phuong con lai)

                    PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
                    pIntOpts.Message = "\nNHẬP KÍCH THƯỚC CHIỀU CÒN LẠI(W/L), ENTER ĐỂ PICK KHOẢNG CÁCH: ";
                    //pIntOpts.DefaultValue = -1;
                    pIntOpts.AllowNone = true;
                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);

                    if (pIntRes.Value <= 0)
                    {
                        pIntRes = acDoc.Editor.GetDistance("\nPick two points: ");
                    }


                    if (pIntRes.Value <= 0.00) return;

                    double widthRaw = pIntRes.Value;



                    // input string
                    // Create a title
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nNHẬP KÍ HIỆU TẤM: ");
                    pStrOpts.AllowSpaces = true;
                    pStrOpts.DefaultValue = "T";
                    PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);


                    // ghi file csv

                    StringBuilder csvContent = new StringBuilder();
                    // Ghi header row
                    csvContent.AppendLine("TEN, DAI, RONG, CHIEU DAY, SO LUONG, VAT LIEU");


                    if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


                    string mySymbolName = pStrRes.StringResult.ToUpper();

                    Dictionary<myCustomSection, int> myDic = new Dictionary<myCustomSection, int>(new CustomerSectionEqualityComparer());

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        Point3d myMinPoint = myPoly.GeometricExtents.MinPoint;
                        Point3d myMaxPoint = myPoly.GeometricExtents.MaxPoint;

                        double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X), 0);
                        double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y), 0);

                        double length = Math.Max(deltaX, deltaY);
                        length = Math.Max(length,Math.Round( myPoly.GetMaxSegment(),0));
                        length = (Math.Round(length / 100, 0) + 1) * 100;

                        double thickness = Math.Min(deltaX, deltaY);
                        thickness = Math.Min(thickness, Math.Round(myPoly.GetMinSegment(),0));

                        double width = (Math.Round(widthRaw / 100, 0) + 1) * 100;

                        string myMaterial = myPoly.Layer;

                        string myNamePlate = mySymbolName + "|" + length + "|" + width + "|" + thickness + "|" + myMaterial;


                        myCustomSection mySection = new myCustomSection(myNamePlate, length, width, thickness, myMaterial);

                        if (myDic.ContainsKey(mySection))
                        {
                            myDic[mySection]++;
                        }
                        else
                        {
                            myDic.Add(mySection, 1);
                        }


                        //string lineToWrite = String.Format("{0},{1},{2},{3}", myNameArea, deltaX, deltaY, 1);

                        //csvContent.AppendLine(lineToWrite);

                        singlePickPolyGetDim(myNamePlate, myPoly,6);
                    }

                    // Write file csv
                    foreach (KeyValuePair<myCustomSection, int> myPlateSec in myDic)
                    {
                        string lineToWrite = String.Format("{0},{1},{2},{3},{4},{5}", myPlateSec.Key.Name, myPlateSec.Key.Length, myPlateSec.Key.Width, myPlateSec.Key.Thickness, myPlateSec.Value, myPlateSec.Key.Material);
                        csvContent.AppendLine(lineToWrite);

                    }

                    try
                    {
                        string pathCsv = "D:\\PTKLTemp.csv";
                        //File.AppendAllText(pathCsv, csvContent.ToString());
                        //System.Diagnostics.Process.Start(pathCsv);
                        acTrans.Commit();
                        return;
                    }
                    catch
                    {
                        Application.ShowAlertDialog("File D://PTKLTemp.csv có thể đang được mở, đóng file va thử lại!");
                    }

                }

            }

        }



        /// <summary>
        /// tong hop Kl theo mat cat ngang, nhap chieu rong, vat lieu lay theo layer
        /// </summary>
        [CommandMethod("UQS")]
        public static void UpdateQuantityBySection()
        {
            if (!CheckLicense.licensed) return;

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

                // CHọn polyline section cần update

                var options = new PromptEntityOptions("\nSelect Section Polyline: ");
                options.SetRejectMessage("\nSelected object is no a Polyline.");
                options.AddAllowedClass(typeof(Polyline), true);

                var result = acDoc.Editor.GetEntity(options);
                if (result.Status != PromptStatus.OK) return;
                ObjectId myObjId = result.ObjectId;

                if (myObjId.ToString() == "0") return;
                if (myObjId == new ObjectId()) return;

                Polyline myPolySelected = myObjId.GetObject(OpenMode.ForWrite) as Polyline;


                if (myPolySelected.NumberOfVertices < 2) return;

                if (myPolySelected.Area == 0) return;

                myPolySelected.Closed = true;

                myPolySelected.removePointDup();

                Polyline myPoly = myPolySelected;

                Point3d myMinPoint = myPoly.GeometricExtents.MinPoint;
                Point3d myMaxPoint = myPoly.GeometricExtents.MaxPoint;

                double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X), 0);
                double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y), 0);

                double length = Math.Max(deltaX, deltaY);
                length = Math.Max(length, Math.Round(myPoly.GetMaxSegment(), 0));

                double thickness = Math.Min(deltaX, deltaY);
                thickness = Math.Min(thickness, myPoly.GetMinDistancepVerToLine());

                //string OutString = "Chieu day tam: " + myPoly.GetMinDistancepVerToLine();

                string myMaterial = myPoly.Layer;


                // Chọn Text section cần update
                DBText mytextSelected = new DBText();
                while (true)
                {
                    var optionsText = new PromptEntityOptions("\nSelect Text to Update Quantity: ");
                    optionsText.SetRejectMessage("\nSelected object is no a Text.");
                    optionsText.AddAllowedClass(typeof(DBText), true);

                    var resultText = acDoc.Editor.GetEntity(optionsText);
                    if (resultText.Status != PromptStatus.OK) return;
                    ObjectId myObjIdText = resultText.ObjectId;
                    if (myObjIdText.ToString() == "0") return;
                    if (myObjIdText == new ObjectId()) return;

                    mytextSelected = myObjIdText.GetObject(OpenMode.ForWrite) as DBText;

                    // Neu text ko dung dinh dang tiep tuc nhap lai, hoặc hủy

                    int numberOrChar = mytextSelected.TextString.Count(f => f == '|');
                    if (numberOrChar == 4)
                    break;
                }

                try
                {
                    List<string> myDetailText = mytextSelected.TextString.Split('|').ToList();
                    string mySymbolName = myDetailText[0];

                    double width = Convert.ToDouble(myDetailText[2]);
                    string myNamePlate = mySymbolName + "|" + length + "|" + width + "|" + thickness + "|" + myMaterial;
                    mytextSelected.TextString = myNamePlate;

                    mytextSelected.Position = new Point3d((myMinPoint.X + myMaxPoint.X) / 2, (myMinPoint.Y + myMaxPoint.Y) / 2, 0);
                    acTrans.Commit();
                }
                catch
                {
                    Application.ShowAlertDialog("Có lỗi gi đó...");
                }

                }

            }

        }


    /// <summary>
    /// 2 class tiep theo de cho ham lay Kl theo dien tich.
    /// </summary>
    public class myCustomPlate
    {
        public myCustomPlate(string namePlate, double width, double height )
        {
            this.name = namePlate;
            this.width = width;
            this.height = height;
        }

        public string Name
        {
            get { return name;}
            set { name = value; }
        }

        public double Width
        {
            get { return width; }
            set { width = value; }
        }
        public double Height
        {
            get { return height; }
            set { height = value; }
        }

        string name;
        double width;
        double height;
    }
  
    public class CustomerEqualityComparer : IEqualityComparer<myCustomPlate>
    {
        #region IEqualityComparer<Customer> Members

        public bool Equals(myCustomPlate x, myCustomPlate y)
        {
            return (x.Name == y.Name);
        }

        public int GetHashCode(myCustomPlate obj)
        {
            string combined = obj.Name;
            return (combined.GetHashCode());
        }

        #endregion
    }


    /// <summary>
    /// hai class tiep theo de lay KL theo mat cat go.
    /// </summary>
    public class myCustomSection
    {
        public myCustomSection(string namePlate, double length, double width, double thickness,string material )
        {
            this.name = namePlate;
            this.width = width;
            this.length = length;
            this.thickness = thickness;
            this.material = material;
        }

        public string Name
        {
            get { return name;}
            set { name = value; }
        }

        public double Width
        {
            get { return width; }
            set { width = value; }
        }
        public double Length
        {
            get { return length; }
            set { length = value; }
        }

        public double Thickness
        {
            get { return thickness; }
            set { thickness = value; }
        }

        public string Material
        {
            get { return material; }
            set { material = value; }
        }

        string name;
        double width;
        double length;
        double thickness;
        string material;
    }

    public class CustomerSectionEqualityComparer : IEqualityComparer<myCustomSection>
    {
        #region IEqualityComparer<Customer> Members

        public bool Equals(myCustomSection x, myCustomSection y)
        {
            return (x.Name == y.Name);
        }

        public int GetHashCode(myCustomSection obj)
        {
            string combined = obj.Name;
            return (combined.GetHashCode());
        }

        #endregion
    }


}
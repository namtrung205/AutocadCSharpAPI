using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using System;

using System.Runtime.InteropServices;
using Autodesk.AutoCAD.Colors;
using wf = System.Windows.Forms;
using Autodesk.AutoCAD.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using commonFunctions;

namespace myCustomCmds
{
    public class CmdDivideDim
    {



        //// phân đoạn
        //private static double divideLength { get; set; }


        //// Buoc cot dai 1
        //private static double pitch0 { get; set; }

        //// Buoc cot dai 1
        //private static double pitch1 { get; set; }

        //// Buoc cot dai 2
        //private static double pitch2 { get; set; }


        // phân đoạn
        private static double divideLength = 4;


        // Buoc cot dai 1
        private static double pitch0 = 50;

        // Buoc cot dai 1
        private static double pitch1 = 100;

        // Buoc cot dai 2
        private static double pitch2 = 200;


        [CommandMethod("DDLX")]
        public static void splitDimsByFactor()
        {

            if (!CheckLicense.licensed) return;

            //Create layer Dim
            CmdLayer.createALayerByName("DIM");


            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
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

                ObjectId myObjectId = myCustomFunctions.GetObjectIdByType("RotatedDimension,AlignedDimension");

                List<Point3d> myListPointSplit = new List<Point3d>();
                // If the prompt status is OK, objects were selected


                if (myObjectId == null) return;
                if (myObjectId.ToString() == "0") return;
                {
                    //DBObject myAcDim = myAcPER.ObjectId.GetObject(OpenMode.ForWrite);
                    string typeDimName = myObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name;
                    if (typeDimName == "AcDbRotatedDimension")
                    {

                        //DBObject myAcDimOb = myAcPER.ObjectId.GetObject(OpenMode.ForWrite);

                        RotatedDimension myAcDim = new RotatedDimension();
                        myAcDim = myObjectId.GetObject(OpenMode.ForWrite) as RotatedDimension;
                        // Chon diem de slpit:
                        // Lay thong tin goc xoay
                        double myRotationDimCur = myAcDim.Rotation;
                        myListPointSplit.Add(myAcDim.XLine1Point);
                        myListPointSplit.Add(myAcDim.XLine2Point);
                        // Lay vi tri dim

                        Point3d myDimPointLine = myAcDim.DimLinePoint;

                        // Select a point to split
                        // Prompt for the end point

                        PromptPointResult pPtRes;
                        PromptPointOptions pPtOpts = new PromptPointOptions("");
                        pPtOpts.Message = "\nEnter position dim: ";

                        pPtOpts.UseBasePoint = true;
                        //pPtOpts.BasePoint = new Point3d((myAcDim.XLine1Point.X + myAcDim.XLine2Point.X) / 2, (myAcDim.XLine1Point.Y + myAcDim.XLine2Point.Y) / 2, myAcDim.XLine1Point.Z);
                        pPtOpts.BasePoint = myAcDim.XLine1Point;
                        pPtOpts.UseDashedLine = true;
                        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                        Point3d ptInsertPoint = pPtRes.Value;

                        myListPointSplit.Add(ptInsertPoint);

                        if (pPtRes.Status == PromptStatus.Cancel) return;

                        // Tao rotation dim from list point

                        int nDim = 0;

                        if (myRotationDimCur < Math.PI / 4)
                        {
                            myListPointSplit.Sort(sortByX);
                        }
                        else
                        {
                            myListPointSplit.Sort(sortByY);
                        }


                        for (int i = 1; i < myListPointSplit.Count; i++)
                        {
                            using (RotatedDimension acRotDim = new RotatedDimension())
                            {
                                acRotDim.XLine1Point = myListPointSplit[i - 1];
                                acRotDim.XLine2Point = myListPointSplit[i];
                                acRotDim.Rotation = myRotationDimCur;
                                acRotDim.DimLinePoint = myDimPointLine;
                                acRotDim.DimensionStyle = myAcDim.DimensionStyle;
                                acRotDim.Layer = "DIM";
                                acRotDim.SetDimstyleData(myAcDim.GetDimstyleData());

                                // Add the new object to Model space and the transaction
                                if (acRotDim.Rotation == 0)
                                {
                                    if (Math.Abs(((Point3d)myListPointSplit[i]).X - ((Point3d)myListPointSplit[i - 1]).X) > 0.01)
                                    {
                                        acBlkTblRec.AppendEntity(acRotDim);
                                        acTrans.AddNewlyCreatedDBObject(acRotDim, true);
                                        nDim++;
                                    }
                                }
                                else
                                {
                                    if (Math.Abs(((Point3d)myListPointSplit[i]).Y - ((Point3d)myListPointSplit[i - 1]).Y) > 0.01)
                                    {
                                        acBlkTblRec.AppendEntity(acRotDim);
                                        acTrans.AddNewlyCreatedDBObject(acRotDim, true);
                                        nDim++;
                                    }
                                }
                            }
                        }
                        // Delete Dim
                        myAcDim.Erase();
                    }

                    if (typeDimName == "AcDbAlignedDimension")
                    {

                        DBObject myAcDimOb = myObjectId.GetObject(OpenMode.ForWrite);

                        AlignedDimension myAcDim = new AlignedDimension();
                        myAcDim = myObjectId.GetObject(OpenMode.ForWrite) as AlignedDimension;
                        // Chon diem de slpit:

                        // Lay vi tri dim
                        Point3d myDimPointLine = myAcDim.DimLinePoint;

                        // Select a point to split
                        // Prompt for the end point

                        PromptPointResult pPtRes;
                        PromptPointOptions pPtOpts = new PromptPointOptions("");
                        pPtOpts.Message = "\nEnter position dim: ";

                        pPtOpts.UseBasePoint = true;
                        //pPtOpts.BasePoint = new Point3d((myAcDim.XLine1Point.X + myAcDim.XLine2Point.X) / 2, (myAcDim.XLine1Point.Y + myAcDim.XLine2Point.Y) / 2, myAcDim.XLine1Point.Z);
                        pPtOpts.BasePoint = myAcDim.XLine1Point;
                        pPtOpts.UseDashedLine = true;
                        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                        Point3d ptInsertPoint = pPtRes.Value;

                        if (pPtRes.Status == PromptStatus.Cancel) return;

                        myListPointSplit.Add(myAcDim.XLine1Point);
                        myListPointSplit.Add(ptInsertPoint);
                        myListPointSplit.Add(myAcDim.XLine2Point);

                        for (int i = 1; i < myListPointSplit.Count; i++)
                        {
                            using (RotatedDimension acAlDim = new RotatedDimension())
                            {
                                acAlDim.XLine1Point = myListPointSplit[i - 1];
                                acAlDim.XLine2Point = myListPointSplit[i];
                                acAlDim.DimLinePoint = myDimPointLine;
                                acAlDim.Rotation = Math.Atan2(myAcDim.XLine2Point.Y - myAcDim.XLine1Point.Y, myAcDim.XLine2Point.X - myAcDim.XLine1Point.X);
                                acAlDim.DimensionStyle = myAcDim.DimensionStyle;
                                acAlDim.Layer = "DIM";
                                acAlDim.SetDimstyleData(myAcDim.GetDimstyleData());

                                acBlkTblRec.AppendEntity(acAlDim);
                                acTrans.AddNewlyCreatedDBObject(acAlDim, true);

                            }
                        }
                        // Delete Dim
                        myAcDimOb.Erase();

                    }
                }

                acTrans.Commit();
            }

            // Dispose of the transaction
        }


        [CommandMethod("D2C")]
        public static void distance2Clipboard()
        {

            //if (!CheckLicense.licensed) return;

            //Create layer Dim
            CmdLayer.createALayerByName("DIM");


            // Get the current document and database
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


                // Pick first Point OnScreen

                PromptPointResult pPtRes;
                PromptPointOptions pPtOpts = new PromptPointOptions("");
                pPtOpts.Message = "\nPick First Point: ";

                pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                Point3d ptFirstPoint = pPtRes.Value;

                if (pPtRes.Status == PromptStatus.Cancel) return;

                Double XFirstPoint = ptFirstPoint.X;

                List<double> myListDistance = new List<double>();


                //Filter selecttion

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "DIMENSION"), 0);
                //acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "0"), 2);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);


                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;

                    // neu so luong dim dc chon <2 return


                    // Tao 1 list point chua chan dim
                    List<Point3d> myListXlinePoint = new List<Point3d>();

                    // lay dim point dau tien

                    Point3d myDimLinePoint = new Point3d();
                    double myRotation = new double();

                    // Step through the objects in the selection set
                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        string typeDimName = acSSObj.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name;
                        if (typeDimName == "AcDbRotatedDimension")
                        //if (typeDimName == "AcDbAlignedDimension" || typeDimName == "AcDbRotatedDimension")
                        {
                            //acDoc.Editor.WriteMessage("This is a dim linear: {0}\n", acSSObj.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name);


                            RotatedDimension myDim = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as RotatedDimension;


                            // lay chan dim

                            Double XDisPoint1 = Math.Abs(myDim.XLine1Point.X - XFirstPoint)/304.8;
                            Double XDisPoint2 = Math.Abs(myDim.XLine2Point.X - XFirstPoint)/304.8;

                            if (!myListDistance.Contains(XDisPoint1))
                            {
                                myListDistance.Add(XDisPoint1);
                            }


                            if (!myListDistance.Contains(XDisPoint2))
                            {
                                myListDistance.Add(XDisPoint2);
                            }

                            Point3d myPoint1 = myDim.XLine1Point;
                            Point3d myPoint2 = myDim.XLine2Point;

                        }
                    }

                    myListDistance.Sort();

                    // Write dis to clipboard
                    string mystr2Cb = string.Join(";", myListDistance);



                    wf.Clipboard.Clear();

                    wf.Clipboard.SetText(mystr2Cb);

                    string a = wf.Clipboard.GetText();

                    acDoc.Editor.WriteMessage(a);
                    // Save the new object to the database
                    acTrans.Commit();
                }

                // Dispose of the transaction
            }
        }
        // Merge and split dim


        [CommandMethod("SDVD")]
        public static void splitDimByFactor1()
        {

            if (!CheckLicense.licensed) return;

            //Create layer Dim
            CmdLayer.createALayerByName("DIM");


            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
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

                ObjectId myObjectId = myCustomFunctions.GetObjectIdByType("RotatedDimension,AlignedDimension");

                List<Point3d> myListPointSplit = new List<Point3d>();


                if (myObjectId == null) return;
                if (myObjectId.ToString() == "0") return;
                {
                    //DBObject myAcDim = myAcPER.ObjectId.GetObject(OpenMode.ForWrite);
                    string typeDimName = myObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name;
                    if (typeDimName == "AcDbRotatedDimension")
                    {
                        //DBObject myAcDimOb = myAcPER.ObjectId.GetObject(OpenMode.ForWrite);
                        RotatedDimension myAcDim = new RotatedDimension();
                        myAcDim = myObjectId.GetObject(OpenMode.ForWrite) as RotatedDimension;

                        Point3d ptDim = myAcDim.DimLinePoint;
                        // Chon diem de slpit:
                        // Lay thong tin goc xoay
                        double myRotationDimCur = myAcDim.Rotation;

                        Point3d PX1 = myAcDim.XLine1Point;
                        Point3d PX2 = myAcDim.XLine2Point;


                        Vector2d vDim = new Vector2d(myAcDim.XLine2Point.X - myAcDim.XLine1Point.X, myAcDim.XLine2Point.Y - myAcDim.XLine1Point.Y);

                        //Chiều dài đoạn L/4
                        double L12 = vDim.Length / divideLength;
                        List<double> myDisHalf = new List<double>();


                        // Thêm điểm đầu tiên
                        myDisHalf.Add(pitch0);
                        double nextPoint = pitch0 + pitch1;// Buoc dai ban dau la 100 đoạn L/4

                        // neu next Point co khoang cach nho hon L/4, them nextPoitn vao list L4 (0 - L/4)
                        while (nextPoint <= L12)
                        {
                            myDisHalf.Add(nextPoint);
                            nextPoint += pitch1;
                        }

                        // Them diem L/4 - L/2
                        double L23 = vDim.Length / 2;
                        myDisHalf.Add(L23);
                        double previousPoint = L23 - pitch2;
                        while (L12 <= previousPoint)
                        {
                            myDisHalf.Add(previousPoint);
                            previousPoint -= pitch2;
                        }


                        // Them diem L/2 - 3L/4
                        double L34 = vDim.Length * (1 - 1 / divideLength);
                        double nextPoint2 = L23 + pitch2;
                        while (nextPoint2 <= L34)
                        {
                            myDisHalf.Add(nextPoint2);
                            nextPoint2 += pitch2;
                        }


                        // Them diem l3/4 - L
                        double L45 = vDim.Length - pitch0;
                        myDisHalf.Add(L45);
                        double previousPoint2 = L45 - pitch1;
                        while (previousPoint2 >= L34)
                        {
                            myDisHalf.Add(previousPoint2);
                            previousPoint2 -= pitch1;
                        }

                        // Lay diem co toa do x nho hon trong 2 diem Px1, Px2

                        Point3d P1 = new Point3d();
                        Point3d P2 = new Point3d();
                        if (PX1.X < PX2.X)
                        {
                            P1 = PX1;
                            P2 = PX2;
                        }
                        else
                        {
                            P1 = PX2;
                            P2 = PX1;
                        }

                        // Tao toa do cac diem bang foreach
                        foreach (double dis in myDisHalf)
                        {
                            Point3d myTempP = new Point3d(P1.X + dis, P1.Y, P1.Z);
                            if (!myListPointSplit.Contains(myTempP))
                            {
                                myListPointSplit.Add(myTempP);
                            }

                        }

                        // Sắp xếp các điểm trong list point vua tạo được bằng cách sort

                        myListPointSplit.Sort(sortByX);


                        // Select a point to split
                        // Prompt for the end point

                        PromptPointResult pPtRes;
                        PromptPointOptions pPtOpts = new PromptPointOptions("");
                        pPtOpts.Message = "\nEnter position dim: ";

                        pPtOpts.UseBasePoint = true;
                        //pPtOpts.BasePoint = new Point3d((myAcDim.XLine1Point.X + myAcDim.XLine2Point.X) / 2, (myAcDim.XLine1Point.Y + myAcDim.XLine2Point.Y) / 2, myAcDim.XLine1Point.Z);
                        pPtOpts.BasePoint = myAcDim.XLine1Point;
                        pPtOpts.UseDashedLine = true;
                        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                        Point3d ptInsertPoint = pPtRes.Value;


                        if (pPtRes.Status == PromptStatus.Cancel) return;

                        // Tao rotation dim from list point

                        // tao dimension nam ngang
                        autoDimHorizontalNotSelect(myListPointSplit, ptInsertPoint);

                    }
                }

                acTrans.Commit();
            }

            // Dispose of the transaction
        }


        [CommandMethod("SDV4")]
        public static void splitDimByFactor4()
        {

            if (!CheckLicense.licensed) return;

            //Create layer Dim
            CmdLayer.createALayerByName("DIM");


            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
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

                ObjectId myObjectId = myCustomFunctions.GetObjectIdByType("RotatedDimension,AlignedDimension");

                List<Point3d> myListPointSplit = new List<Point3d>();

                // If the prompt status is OK, objects were selected

                if (myObjectId == null) return;
                if (myObjectId.ToString() == "0") return;
                {
                    //DBObject myAcDim = myAcPER.ObjectId.GetObject(OpenMode.ForWrite);
                    string typeDimName = myObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name;
                    if (typeDimName == "AcDbRotatedDimension")
                    {
                        //DBObject myAcDimOb = myAcPER.ObjectId.GetObject(OpenMode.ForWrite);
                        RotatedDimension myAcDim = new RotatedDimension();
                        myAcDim = myObjectId.GetObject(OpenMode.ForWrite) as RotatedDimension;

                        Point3d ptDim = myAcDim.DimLinePoint;
                        // Chon diem de slpit:
                        // Lay thong tin goc xoay
                        double myRotationDimCur = myAcDim.Rotation;

                        Point3d PX1 = myAcDim.XLine1Point;
                        Point3d PX2 = myAcDim.XLine2Point;


                        Vector2d vDim = new Vector2d(myAcDim.XLine2Point.X - myAcDim.XLine1Point.X, myAcDim.XLine2Point.Y-myAcDim.XLine1Point.Y);

                        //Chiều dài đoạn L/4
                        double L12 = vDim.Length / 4;
                        List<double> myDisHalf = new List<double>();


                        // Thêm điểm đầu tiên
                        myDisHalf.Add(50);
                        double nextPoint = 50 + 100;// Buoc dai ban dau la 100 đoạn L/4

                        // neu next Point co khoang cach nho hon L/4, them nextPoitn vao list L4 (0 - L/4)
                        while (nextPoint <= L12)
                        {
                            myDisHalf.Add(nextPoint);
                            nextPoint += 100;
                        }

                        // Them diem L/4 - L/2
                        double L23 = vDim.Length / 2;
                        myDisHalf.Add(L23);
                        double previousPoint = L23 - 200;
                        while(L12 <= previousPoint)
                        {
                            myDisHalf.Add(previousPoint);
                            previousPoint -= 200;
                        }

                        // Them diem L/2 - 3L/4
                        double L34 = vDim.Length *3 / 4;
                        double nextPoint2 = L23 + 200;
                        while (nextPoint2 <= L34)
                        {
                            myDisHalf.Add(nextPoint2);
                            nextPoint2 += 200;
                        }

                        // Them diem l3/4 - L
                        double L45 = vDim.Length - 50;
                        myDisHalf.Add(L45);
                        double previousPoint2 = L45 - 100;

                        while (previousPoint2 >= L34)
                        {
                            myDisHalf.Add(previousPoint2);
                            previousPoint2 -= 100;
                        }

                        // Lay diem co toa do x nho hon trong 2 diem Px1, Px2

                        Point3d P1 = new Point3d();
                        Point3d P2 = new Point3d();
                        if(PX1.X < PX2.X)
                        {
                            P1 = PX1;
                            P2 = PX2;
                        }
                        else
                        {
                            P1 = PX2;
                            P2 = PX1;
                        }

                        // Tao toa do cac diem bang foreach
                        foreach(double dis in myDisHalf)
                        {
                            Point3d myTempP = new Point3d(P1.X + dis, P1.Y, P1.Z);
                            if (!myListPointSplit.Contains(myTempP))
                            {
                                myListPointSplit.Add(myTempP);
                            }

                        }

                        // Sắp xếp các điểm trong list point vua tạo được bằng cách sort

                        myListPointSplit.Sort(sortByX);


                        // Select a point to split
                        // Prompt for the end point

                        PromptPointResult pPtRes;
                        PromptPointOptions pPtOpts = new PromptPointOptions("");
                        pPtOpts.Message = "\nEnter position dim: ";

                        pPtOpts.UseBasePoint = true;
                        //pPtOpts.BasePoint = new Point3d((myAcDim.XLine1Point.X + myAcDim.XLine2Point.X) / 2, (myAcDim.XLine1Point.Y + myAcDim.XLine2Point.Y) / 2, myAcDim.XLine1Point.Z);
                        pPtOpts.BasePoint = myAcDim.XLine1Point;
                        pPtOpts.UseDashedLine = true;
                        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                        Point3d ptInsertPoint = pPtRes.Value;


                        if (pPtRes.Status == PromptStatus.Cancel) return;

                        // Tao rotation dim from list point

                        // tao dimension nam ngang
                        autoDimHorizontalNotSelect(myListPointSplit, ptInsertPoint);

                    }
                }

                acTrans.Commit();
            }

            // Dispose of the transaction
        }


        [CommandMethod("SDVS")]
        public static void splitDimByFactor0()
        {

            if (!CheckLicense.licensed) return;

            //Create layer Dim
            CmdLayer.createALayerByName("DIM");


            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
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

                ObjectId myObjectId = myCustomFunctions.GetObjectIdByType("RotatedDimension,AlignedDimension");

                List<Point3d> myListPointSplit = new List<Point3d>();


                // Nhap cac thong so settting


                // If the prompt status is OK, objects were selected

                PromptDoubleOptions pdoubleOpts = new PromptDoubleOptions("");
                pdoubleOpts.Message = "\nNhap chieu dai phan doan (Vd: L/4, nhap 4): ";
                pdoubleOpts.DefaultValue = 4;


                PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pdoubleOpts);

                // Restrict input to positive and non-negative values
                pdoubleOpts.AllowZero = false;
                pdoubleOpts.AllowNegative = false;


                divideLength = pIntRes.Value;

                // Buoc dai 0
                PromptDoubleOptions pdoubleOptsPitch0 = new PromptDoubleOptions("");
                pdoubleOptsPitch0.Message = "\nNhap chieu dai buoc dai 1 (Vd: 100, nhap 100): ";
                pdoubleOptsPitch0.DefaultValue = 50;

                PromptDoubleResult pDoubleResPitch0 = acDoc.Editor.GetDouble(pdoubleOptsPitch0);

                // Restrict input to positive and non-negative values
                pdoubleOptsPitch0.AllowZero = false;
                pdoubleOptsPitch0.AllowNegative = false;

                pitch0 = pDoubleResPitch0.Value;

                // Buoc dai 1
                PromptDoubleOptions pdoubleOptsPitch1 = new PromptDoubleOptions("");
                pdoubleOptsPitch1.Message = "\nNhap chieu dai buoc dai 1 (Vd: 100, nhap 100): ";
                pdoubleOptsPitch1.DefaultValue = 100;

                PromptDoubleResult pDoubleResPitch1 = acDoc.Editor.GetDouble(pdoubleOptsPitch1);

                // Restrict input to positive and non-negative values
                pdoubleOptsPitch1.AllowZero = false;
                pdoubleOptsPitch1.AllowNegative = false;

                pitch1 = pDoubleResPitch1.Value;

                // Buoc dai 2
                PromptDoubleOptions pdoubleOptsPitch2 = new PromptDoubleOptions("");
                pdoubleOptsPitch2.Message = "\nNhap chieu dai buoc dai 1 (Vd: 200, nhap 200): ";
                pdoubleOptsPitch2.DefaultValue = 200;


                PromptDoubleResult pDoubleResPitch2 = acDoc.Editor.GetDouble(pdoubleOptsPitch2);

                // Restrict input to positive and non-negative values
                pdoubleOptsPitch2.AllowZero = false;
                pdoubleOptsPitch2.AllowNegative = false;


                pitch2 = pDoubleResPitch2.Value;




                if (myObjectId == null) return;
                if (myObjectId.ToString() == "0") return;
                {
                    //DBObject myAcDim = myAcPER.ObjectId.GetObject(OpenMode.ForWrite);
                    string typeDimName = myObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name;
                    if (typeDimName == "AcDbRotatedDimension")
                    {
                        //DBObject myAcDimOb = myAcPER.ObjectId.GetObject(OpenMode.ForWrite);
                        RotatedDimension myAcDim = new RotatedDimension();
                        myAcDim = myObjectId.GetObject(OpenMode.ForWrite) as RotatedDimension;

                        Point3d ptDim = myAcDim.DimLinePoint;
                        // Chon diem de slpit:
                        // Lay thong tin goc xoay
                        double myRotationDimCur = myAcDim.Rotation;

                        Point3d PX1 = myAcDim.XLine1Point;
                        Point3d PX2 = myAcDim.XLine2Point;


                        Vector2d vDim = new Vector2d(myAcDim.XLine2Point.X - myAcDim.XLine1Point.X, myAcDim.XLine2Point.Y - myAcDim.XLine1Point.Y);

                        //Chiều dài đoạn L/4
                        double L12 = vDim.Length / divideLength;
                        List<double> myDisHalf = new List<double>();


                        // Thêm điểm đầu tiên
                        myDisHalf.Add(pitch0);
                        double nextPoint = pitch0 + pitch1;// Buoc dai ban dau la 100 đoạn L/4

                        // neu next Point co khoang cach nho hon L/4, them nextPoitn vao list L4 (0 - L/4)
                        while (nextPoint <= L12)
                        {
                            myDisHalf.Add(nextPoint);
                            nextPoint += pitch1;
                        }

                        // Them diem L/4 - L/2
                        double L23 = vDim.Length / 2;
                        double previousPoint = L23 - pitch2;
                        while (L12 <= previousPoint)
                        {
                            myDisHalf.Add(previousPoint);
                            previousPoint -= pitch2;
                        }

                        // Them diem L/2 - 3L/4
                        double L34 = vDim.Length*(1-1/divideLength);
                        double nextPoint2 = L23 + pitch2;
                        while (nextPoint2 <= L34)
                        {
                            myDisHalf.Add(nextPoint2);
                            nextPoint2 += pitch2;
                        }


                        // Them diem l3/4 - L
                        double L45 = vDim.Length - pitch0;
                        myDisHalf.Add(L45);
                        double previousPoint2 = L45 - pitch1;
                        while (previousPoint2 >= L34)
                        {
                            myDisHalf.Add(previousPoint2);
                            previousPoint2 -= pitch1;
                        }

                        // Lay diem co toa do x nho hon trong 2 diem Px1, Px2

                        Point3d P1 = new Point3d();
                        Point3d P2 = new Point3d();
                        if (PX1.X < PX2.X)
                        {
                            P1 = PX1;
                            P2 = PX2;
                        }
                        else
                        {
                            P1 = PX2;
                            P2 = PX1;
                        }

                        // Tao toa do cac diem bang foreach
                        foreach (double dis in myDisHalf)
                        {
                            Point3d myTempP = new Point3d(P1.X + dis, P1.Y, P1.Z);
                            if (!myListPointSplit.Contains(myTempP))
                            {
                                myListPointSplit.Add(myTempP);
                            }

                        }

                        // Sắp xếp các điểm trong list point vua tạo được bằng cách sort

                        myListPointSplit.Sort(sortByX);


                        // Select a point to split
                        // Prompt for the end point

                        PromptPointResult pPtRes;
                        PromptPointOptions pPtOpts = new PromptPointOptions("");
                        pPtOpts.Message = "\nEnter position dim: ";

                        pPtOpts.UseBasePoint = true;
                        //pPtOpts.BasePoint = new Point3d((myAcDim.XLine1Point.X + myAcDim.XLine2Point.X) / 2, (myAcDim.XLine1Point.Y + myAcDim.XLine2Point.Y) / 2, myAcDim.XLine1Point.Z);
                        pPtOpts.BasePoint = myAcDim.XLine1Point;
                        pPtOpts.UseDashedLine = true;
                        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                        Point3d ptInsertPoint = pPtRes.Value;


                        if (pPtRes.Status == PromptStatus.Cancel) return;

                        // Tao rotation dim from list point

                        // tao dimension nam ngang
                        autoDimHorizontalNotSelect(myListPointSplit, ptInsertPoint);

                    }
                }

                acTrans.Commit();
            }

            // Dispose of the transaction
        }



        /// Old commands
        //Tao vung chon bang chuot, lay thong tin vertex cua cac polyline and line trong vung chon
        //[CommandMethod("DO", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        //public void DimOrtho(string myInput)
        //{
        //    Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        //    Database acCurDb = acDoc.Database;
        //    Editor ed = acDoc.Editor;

        //    List<Point3d> points = new List<Point3d>();

        //    PromptPointOptions ppo = new PromptPointOptions("\n\tSpecify a first corner: ");

        //    PromptPointResult ppr = ed.GetPoint(ppo);

        //    if (ppr.Status != PromptStatus.OK) return;

        //    PromptCornerOptions pco = new PromptCornerOptions("\n\tOther corner: ", ppr.Value);
        //    pco.UseDashedLine = true;
        //    PromptPointResult pcr = ed.GetCorner(pco);
        //    if (pcr.Status != PromptStatus.OK) return;

        //    Point3d pt1 = ppr.Value;

        //    Point3d pt2 = pcr.Value;

        //    using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //    {
        //        try
        //        {
        //            BlockTable acBlkTbl;
        //            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
        //                                            OpenMode.ForRead) as BlockTable;

        //            // Open the Block table record Model space for write
        //            BlockTableRecord acBlkTblRec;
        //            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
        //                                            OpenMode.ForWrite) as BlockTableRecord;


        //            // kiem tra hinh window select hop le hay khong.
        //            if (pt1.X == pt2.X || pt1.Y == pt2.Y)
        //            {
        //                ed.WriteMessage("\nInvalid point specification");
        //                return;
        //            }

        //            PromptSelectionResult res;
        //            res = ed.SelectCrossingWindow(pt1, pt2);


        //            if (res.Status != PromptStatus.OK)
        //                return;
        //            SelectionSet sset = res.Value;

        //            List<Point3d> myListPoint1 = new List<Point3d>();
        //            foreach (var objID in sset.GetObjectIds())
        //            {
        //                string className = objID.ObjectClass.DxfName;
        //                //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n TYPE: {0}\n", className);

        //                // Neu Object class la line hoac polyline, lay point them vao list point:
        //                if (className == "LINE")
        //                {
        //                    // Convert objID to Line
        //                    Line myLine = objID.GetObject(OpenMode.ForRead) as Line;
        //                    Point3d endPoint = myLine.EndPoint;
        //                    myListPoint1.Add(myLine.EndPoint);
        //                    myListPoint1.Add(myLine.StartPoint);
        //                }
        //                // Neu Object class polyline, lay point them vao list point:
        //                if (className == "LWPOLYLINE")
        //                {
        //                    // Convert objID to Line
        //                    Polyline myPoly = objID.GetObject(OpenMode.ForRead) as Polyline;
        //                    int vn = myPoly.NumberOfVertices;

        //                    // Get all Point from Polyline:
        //                    for (int i = 0; i < vn; i++)
        //                    {
        //                        Point3d pt = new Point3d(myPoly.GetPoint2dAt(i).X, myPoly.GetPoint2dAt(i).Y, 0);
        //                        myListPoint1.Add(pt);
        //                    }
        //                }
        //            }

        //            // Insert DBPoint at Point of listPoint

        //            foreach (Point3d myPoint in myListPoint1)
        //            {
        //                using (DBPoint acDBPoint = new DBPoint(myPoint))
        //                {
        //                    acDBPoint.Layer = "Defpoints";
        //                    // Add the new object to the block table record and the transaction
        //                    acBlkTblRec.AppendEntity(acDBPoint);
        //                    acTrans.AddNewlyCreatedDBObject(acDBPoint, true);
        //                }
        //            }

        //            acTrans.Commit();

        //        }
        //        catch (System.Exception ex)
        //        {
        //            ed.WriteMessage(ex.Message + "\n" + ex.StackTrace);
        //        }
        //    }

        //    // Insert Point and make listPoint
        //    using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //    {
        //        PromptSelectionResult res2;
        //        res2 = ed.SelectWindow(pt1, pt2);
        //        if (res2.Status != PromptStatus.OK)
        //            return;
        //        SelectionSet sset2 = res2.Value;

        //        List<Point3d> myListPoint2 = new List<Point3d>();

        //        foreach (var objID2 in sset2.GetObjectIds())
        //        {
        //            string className = objID2.ObjectClass.DxfName;
        //            //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n type: {0}\n", myListPoint2.Count);
        //            if (className == "POINT")
        //            {
        //                // Convert objID2 to Point3D
        //                DBPoint myDBPoint = (DBPoint)objID2.GetObject(OpenMode.ForWrite);
        //                myListPoint2.Add((Point3d)myDBPoint.Position);
        //            }
        //        }


        //        if (myInput == "H")
        //        {
        //            myListPoint2.Sort(sortByX);
        //            autoDimHorizontal(myListPoint2);
        //        }
        //        else if (myInput == "V")
        //        {
        //            myListPoint2.Sort(sortByY);
        //            autoDimVertical(myListPoint2);
        //        }


        //        else
        //        {
        //            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
        //            pKeyOpts.Message = "\nEnter an option ";
        //            pKeyOpts.Keywords.Add("Horizontal");
        //            pKeyOpts.Keywords.Add("Vertical");
        //            pKeyOpts.Keywords.Default = "Horizontal";
        //            pKeyOpts.AllowNone = false;

        //            PromptResult pKeyRes = acDoc.Editor.GetKeywords(pKeyOpts);

        //            if (pKeyRes.StringResult == "Horizontal")
        //            {
        //                myListPoint2.Sort(sortByX);
        //                autoDimHorizontal(myListPoint2);
        //            }
        //            else
        //            {
        //                myListPoint2.Sort(sortByY);
        //                autoDimVertical(myListPoint2);
        //            }
        //        }
        //        acTrans.Commit();
        //    }




        //    // Delete all DBpoint temporatory
        //    using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //    {

        //        TypedValue[] acTypValAr = new TypedValue[2];
        //        acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POINT"), 0);
        //        acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "Defpoints"), 1);

        //        SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);
        //        PromptSelectionResult resPoint;

        //        resPoint = ed.SelectAll(acSelFtr);

        //        //if (resPoint.Status != PromptStatus.OK)

        //        //    return;
        //        SelectionSet ssetPoint = resPoint.Value;

        //        BlockTable acBlkTbl;
        //        acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
        //                                        OpenMode.ForRead) as BlockTable;

        //        // Open the Block table record Model space for write
        //        BlockTableRecord acBlkTblRec;
        //        acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
        //                                        OpenMode.ForWrite) as BlockTableRecord;

        //        if (ssetPoint.Count > 0)
        //        {
        //            foreach (var objID in ssetPoint.GetObjectIds())
        //            {
        //                DBPoint DBPoint = objID.GetObject(OpenMode.ForWrite) as DBPoint;
        //                DBPoint.Erase();

        //            }
        //        }

        //        acTrans.Commit();
        //    }


        //}


        //[CommandMethod("DH", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        //public void DimHorizontal()
        //{
        //    DimOrtho("H");
        //}


        //[CommandMethod("DV", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        //public void DimVertical()
        //{
        //    DimOrtho("V");
        //}

        //[CommandMethod("DO", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        //public void DimOrthoInput()
        //{
        //    DimOrtho("O");
        //}


        //public static void autoDimHorizontal(List<Point3d> listPoint)
        //{
        //    // Get the current database
        //    Document acDoc = Application.DocumentManager.MdiActiveDocument;
        //    Database acCurDb = acDoc.Database;

        //    // Start a transaction
        //    using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //    {
        //        // Open the Block table for read
        //        BlockTable acBlkTbl;
        //        acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
        //                                        OpenMode.ForRead) as BlockTable;

        //        // Open the Block table record Model space for write
        //        BlockTableRecord acBlkTblRec;
        //        acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
        //                                        OpenMode.ForWrite) as BlockTableRecord;

        //        PromptPointResult pPtRes;
        //        PromptPointOptions pPtOpts = new PromptPointOptions("");


        //        // Prompt for the end point
        //        pPtOpts.Message = "\nEnter position dim: ";
        //        //pPtOpts.UseBasePoint = true;

        //        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
        //        Point3d ptDim = pPtRes.Value;
        //        if (pPtRes.Status == PromptStatus.Cancel) return;

        //        int nDim = 0;
        //        for (int i = 1; i < listPoint.Count; i++)
        //        {
        //            using (RotatedDimension acRotDim = new RotatedDimension())
        //            {
        //                acRotDim.XLine1Point = listPoint[i - 1];
        //                acRotDim.XLine2Point = listPoint[i];
        //                //acRotDim.Rotation = Math.PI / 2;
        //                acRotDim.DimLinePoint = ptDim;
        //                acRotDim.DimensionStyle = acCurDb.Dimstyle;
        //                acRotDim.Layer = "DIM";


        //                // Add the new object to Model space and the transaction
        //                if (Math.Abs(((Point3d)listPoint[i]).X - ((Point3d)listPoint[i - 1]).X) > 0.01)
        //                {
        //                    acBlkTblRec.AppendEntity(acRotDim);
        //                    acTrans.AddNewlyCreatedDBObject(acRotDim, true);
        //                    nDim++;
        //                }

        //            }
        //        }


        //        // Tao dim tong:
        //        if (nDim > 1)
        //        {
        //            using (RotatedDimension acRotDim = new RotatedDimension())
        //            {
        //                double dimScale = acCurDb.GetDimstyleData().Dimscale;
        //                Point3d ptDimT = new Point3d();
        //                if (ptDim.Y <= listPoint[0].Y)
        //                {
        //                    ptDimT = new Point3d(ptDim.X, ptDim.Y - 5 * dimScale, ptDim.Z);
        //                }
        //                else
        //                {
        //                    ptDimT = new Point3d(ptDim.X, ptDim.Y + 5 * dimScale, ptDim.Z);
        //                }

        //                acRotDim.XLine1Point = listPoint[0];
        //                acRotDim.XLine2Point = listPoint[listPoint.Count - 1];
        //                //acRotDim.Rotation = Math.PI / 2;
        //                acRotDim.DimLinePoint = ptDimT;
        //                acRotDim.DimensionStyle = acCurDb.Dimstyle;
        //                acRotDim.Layer = "DIM";


        //                // Add the new object to Model space and the transaction
        //                if (Math.Abs(((Point3d)listPoint[0]).X - ((Point3d)listPoint[listPoint.Count - 1]).X) > 0.01)
        //                {
        //                    acBlkTblRec.AppendEntity(acRotDim);
        //                    acTrans.AddNewlyCreatedDBObject(acRotDim, true);
        //                }
        //            }
        //        }


        //        // Commit the changes and dispose of the transaction
        //        acTrans.Commit();
        //    }
        //}


        //public static void autoDimVertical(List<Point3d> listPoint)
        //{
        //    // Get the current database
        //    Document acDoc = Application.DocumentManager.MdiActiveDocument;
        //    Database acCurDb = acDoc.Database;

        //    // Start a transaction
        //    using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //    {
        //        // Open the Block table for read
        //        BlockTable acBlkTbl;
        //        acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
        //                                        OpenMode.ForRead) as BlockTable;

        //        // Open the Block table record Model space for write
        //        BlockTableRecord acBlkTblRec;
        //        acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
        //                                        OpenMode.ForWrite) as BlockTableRecord;

        //        PromptPointResult pPtRes;
        //        PromptPointOptions pPtOpts = new PromptPointOptions("");


        //        // Prompt for the end point
        //        pPtOpts.Message = "\nEnter position dim: ";
        //        //pPtOpts.UseBasePoint = true;

        //        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
        //        Point3d ptDim = pPtRes.Value;
        //        if (pPtRes.Status == PromptStatus.Cancel) return;

        //        int nDim = 0;
        //        for (int i = 1; i < listPoint.Count; i++)
        //        {
        //            using (RotatedDimension acRotDim = new RotatedDimension())
        //            {
        //                acRotDim.XLine1Point = listPoint[i - 1];
        //                acRotDim.XLine2Point = listPoint[i];
        //                acRotDim.Rotation = Math.PI / 2;
        //                acRotDim.DimLinePoint = ptDim;
        //                acRotDim.DimensionStyle = acCurDb.Dimstyle;
        //                acRotDim.Layer = "DIM";


        //                // Add the new object to Model space and the transaction
        //                if (Math.Abs(((Point3d)listPoint[i]).Y - ((Point3d)listPoint[i - 1]).Y) > 0.01)
        //                {
        //                    acBlkTblRec.AppendEntity(acRotDim);
        //                    acTrans.AddNewlyCreatedDBObject(acRotDim, true);
        //                    nDim++;
        //                }

        //            }
        //        }


        //        // Tao dim tong:

        //        if (nDim > 1)
        //        {
        //            using (RotatedDimension acRotDim = new RotatedDimension())
        //            {
        //                double dimScale = acCurDb.GetDimstyleData().Dimscale;
        //                Point3d ptDimT = new Point3d();
        //                if (ptDim.X <= listPoint[0].X)
        //                {
        //                    ptDimT = new Point3d(ptDim.X - 5 * dimScale, ptDim.Y, ptDim.Z);
        //                }
        //                else
        //                {
        //                    ptDimT = new Point3d(ptDim.X + 5 * dimScale, ptDim.Y, ptDim.Z);
        //                }

        //                acRotDim.XLine1Point = listPoint[0];
        //                acRotDim.XLine2Point = listPoint[listPoint.Count - 1];
        //                acRotDim.Rotation = Math.PI / 2;
        //                acRotDim.DimLinePoint = ptDimT;
        //                acRotDim.DimensionStyle = acCurDb.Dimstyle;
        //                acRotDim.Layer = "DIM";


        //                // Add the new object to Model space and the transaction
        //                if (Math.Abs(((Point3d)listPoint[0]).Y - ((Point3d)listPoint[listPoint.Count - 1]).Y) > 0.01)
        //                {
        //                    acBlkTblRec.AppendEntity(acRotDim);
        //                    acTrans.AddNewlyCreatedDBObject(acRotDim, true);
        //                }
        //            }
        //        }

        //        // Commit the changes and dispose of the transaction
        //        acTrans.Commit();
        //    }
        //}


        /// <summary>
        /// New Commnads 
        /// </summary>
        /// <param name="myInput"></param>
        private void DimOrthoAuto(string myInput)
        {

            //Create layer Dim
            CmdLayer.createALayerByName("DIM");




            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            List<Point3d> points = new List<Point3d>();

            PromptPointOptions ppo = new PromptPointOptions("\n\tSpecify a first corner: ");

            PromptPointResult ppr = ed.GetPoint(ppo);

            if (ppr.Status != PromptStatus.OK) return;

            PromptCornerOptions pco = new PromptCornerOptions("\n\tOther corner: ", ppr.Value);
            pco.UseDashedLine = true;
            PromptPointResult pcr = ed.GetCorner(pco);
            if (pcr.Status != PromptStatus.OK) return;

            Point3d pt1 = ppr.Value;

            Point3d pt2 = pcr.Value;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                try
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


                    // kiem tra hinh window select hop le hay khong.
                    if (pt1.X == pt2.X || pt1.Y == pt2.Y)
                    {
                        ed.WriteMessage("\nInvalid point specification");
                        return;
                    }

                    PromptSelectionResult res;
                    res = ed.SelectCrossingWindow(pt1, pt2);


                    if (res.Status != PromptStatus.OK)
                        return;
                    SelectionSet sset = res.Value;

                    List<Point3d> myListPoint1 = new List<Point3d>();
                    foreach (var objID in sset.GetObjectIds())
                    {
                        string className = objID.ObjectClass.DxfName;
                        //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n TYPE: {0}\n", className);

                        // Neu Object class la line hoac polyline, lay point them vao list point:
                        if (className == "LINE")
                        {
                            // Convert objID to Line
                            Line myLine = objID.GetObject(OpenMode.ForRead) as Line;
                            Point3d endPoint = myLine.EndPoint;
                            myListPoint1.Add(myLine.EndPoint);
                            myListPoint1.Add(myLine.StartPoint);
                        }
                        // Neu Object class polyline, lay point them vao list point:
                        if (className == "LWPOLYLINE")
                        {
                            // Convert objID to Line
                            Polyline myPoly = objID.GetObject(OpenMode.ForRead) as Polyline;
                            int vn = myPoly.NumberOfVertices;

                            // Get all Point from Polyline:
                            for (int i = 0; i < vn; i++)
                            {
                                Point3d pt = new Point3d(myPoly.GetPoint2dAt(i).X, myPoly.GetPoint2dAt(i).Y, 0);
                                myListPoint1.Add(pt);
                            }
                        }
                    }

                    // Insert DBPoint at Point of listPoint

                    foreach (Point3d myPoint in myListPoint1)
                    {
                        using (DBPoint acDBPoint = new DBPoint(myPoint))
                        {
                            CmdLayer.createALayerByName("Defpoints");

                            acDBPoint.Layer = "Defpoints";
                            // Add the new object to the block table record and the transaction
                            acBlkTblRec.AppendEntity(acDBPoint);
                            acTrans.AddNewlyCreatedDBObject(acDBPoint, true);
                        }
                    }

                    acTrans.Commit();

                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage(ex.Message + "\n" + ex.StackTrace);
                }
            }

            // Insert Point and make listPoint
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                PromptSelectionResult res2;
                res2 = ed.SelectWindow(pt1, pt2);
                if (res2.Status != PromptStatus.OK)
                    return;
                SelectionSet sset2 = res2.Value;

                List<Point3d> myListPoint2 = new List<Point3d>();
                List<double> listXPoint2 = new List<double>();
                List<double> listYPoint2 = new List<double>();

                foreach (var objID2 in sset2.GetObjectIds())
                {
                    string className = objID2.ObjectClass.DxfName;
                    //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n type: {0}\n", myListPoint2.Count);
                    if (className == "POINT")
                    {
                        // Convert objID2 to Point3D
                        DBPoint myDBPoint = (DBPoint)objID2.GetObject(OpenMode.ForWrite);

                        //Add to lists
                        myListPoint2.Add((Point3d)myDBPoint.Position);
                        listXPoint2.Add(myDBPoint.Position.X);
                        listYPoint2.Add(myDBPoint.Position.Y);
                    }
                }

                PromptPointResult pPtRes;
                PromptPointOptions pPtOpts = new PromptPointOptions("");

                // Prompt for the end point
                pPtOpts.Message = "\nEnter position dim: ";
                //pPtOpts.UseBasePoint = true;

                pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                if (pPtRes.Status == PromptStatus.Cancel) return;
                Point3d ptDim = pPtRes.Value;

                if (ptDim.Y < listYPoint2.Min() || ptDim.Y > listYPoint2.Max())
                {
                    myInput = "H";
                }
                else if (ptDim.X < listXPoint2.Min() || ptDim.X > listXPoint2.Max())
                {
                    myInput = "V";
                }

                else
                {
                    // Dang co 1 loi logic
                    myInput = "O";
                }


                if (myInput == "H")
                {
                    myListPoint2.Sort(sortByX);
                    autoDimHorizontalNotSelect(myListPoint2,ptDim);
                }
                else if (myInput == "V")
                {
                    myListPoint2.Sort(sortByY);
                    autoDimVerticalNotSelect(myListPoint2, ptDim);
                }


                else
                {
                    PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                    pKeyOpts.Message = "\nEnter an option ";
                    pKeyOpts.Keywords.Add("Horizontal");
                    pKeyOpts.Keywords.Add("Vertical");
                    pKeyOpts.Keywords.Default = "Horizontal";
                    pKeyOpts.AllowNone = false;

                    PromptResult pKeyRes = acDoc.Editor.GetKeywords(pKeyOpts);

                    if (pKeyRes.StringResult == "Horizontal")
                    {
                        myListPoint2.Sort(sortByX);
                        autoDimHorizontalNotSelect(myListPoint2,ptDim);
                    }
                    else
                    {
                        myListPoint2.Sort(sortByY);
                        autoDimVerticalNotSelect(myListPoint2,ptDim);
                    }
                }
                acTrans.Commit();
            }




            // Delete all DBpoint temporatory
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                TypedValue[] acTypValAr = new TypedValue[2];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POINT"), 0);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "Defpoints"), 1);

                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);
                PromptSelectionResult resPoint;

                resPoint = ed.SelectAll(acSelFtr);

                //if (resPoint.Status != PromptStatus.OK)

                //    return;
                SelectionSet ssetPoint = resPoint.Value;

                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                if (ssetPoint.Count > 0)
                {
                    foreach (var objID in ssetPoint.GetObjectIds())
                    {
                        DBPoint DBPoint = objID.GetObject(OpenMode.ForWrite) as DBPoint;
                        DBPoint.Erase();

                    }
                }

                acTrans.Commit();
            }

        }



        private static void autoDimHorizontalNotSelect(List<Point3d> listPoint, Point3d dimPoint)
        {
            //Create layer Dim
            CmdLayer.createALayerByName("DIM");



            // Get the current database
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
                };

                Point3d ptDim = dimPoint;

                int nDim = 0;
                for (int i = 1; i < listPoint.Count; i++)
                {
                    using (RotatedDimension acRotDim = new RotatedDimension())
                    {
                        acRotDim.XLine1Point = listPoint[i - 1];
                        acRotDim.XLine2Point = listPoint[i];
                        //acRotDim.Rotation = Math.PI / 2;
                        acRotDim.DimLinePoint = ptDim;
                        acRotDim.DimensionStyle = acCurDb.Dimstyle;
                        acRotDim.Layer = "DIM";


                        // Add the new object to Model space and the transaction
                        if (Math.Abs(((Point3d)listPoint[i]).X - ((Point3d)listPoint[i - 1]).X) > 0.01)
                        {
                            acBlkTblRec.AppendEntity(acRotDim);
                            acTrans.AddNewlyCreatedDBObject(acRotDim, true);
                            nDim++;
                        }

                    }
                }

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
        }


        private static void autoDimVerticalNotSelect(List<Point3d> listPoint, Point3d dimPoint)
        {

            //Create layer Dim
            CmdLayer.createALayerByName("DIM");


            // Get the current database
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


                Point3d ptDim = dimPoint;

                int nDim = 0;
                for (int i = 1; i < listPoint.Count; i++)
                {
                    using (RotatedDimension acRotDim = new RotatedDimension())
                    {
                        acRotDim.XLine1Point = listPoint[i - 1];
                        acRotDim.XLine2Point = listPoint[i];
                        acRotDim.Rotation = Math.PI / 2;
                        acRotDim.DimLinePoint = ptDim;
                        acRotDim.DimensionStyle = acCurDb.Dimstyle;
                        acRotDim.Layer = "DIM";

                        // Add the new object to Model space and the transaction
                        if (Math.Abs(((Point3d)listPoint[i]).Y - ((Point3d)listPoint[i - 1]).Y) > 0.01)
                        {
                            acBlkTblRec.AppendEntity(acRotDim);
                            acTrans.AddNewlyCreatedDBObject(acRotDim, true);
                            nDim++;
                        }

                    }
                }

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
        }


        /// <summary>Sort3Dpoint by X or Y
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static int sortByX(Point3d a, Point3d b)
        {
            if (a.X == b.X)
                return a.Y.CompareTo(b.Y);
            return a.X.CompareTo(b.X);
        }

        static int sortByY(Point3d a, Point3d b)
        {
            if (a.Y == b.Y)
                return a.X.CompareTo(b.X);
            return a.Y.CompareTo(b.Y);
        }

    }

}
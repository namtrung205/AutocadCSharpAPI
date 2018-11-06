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
    public class DimPoly
    {
        // Ham polyline

        //public static List<Point3d> getMinMaxPoint(this Polyline myPolyline)
        //{
        //    // lay minpoint cua extent border
        //    //Point3d myMinPoint = myPolyline.GeometricExtents.MinPoint;
        //    //Point3d myMaxPoint = myPolyline.GeometricExtents.MaxPoint;

        //    if (myPolyline.NumberOfVertices < 4 || myPolyline.Area <=0)
        //    {
        //        Application.ShowAlertDialog("Duong poly khong hop le");
        //        return null;
        //    }

        //    List<Point3d> myAllPointOfPoly = new List<Point3d>();
        //    List<Point3d> my4Point = new List<Point3d>();

        //    for (int i = 0; i < myPolyline.NumberOfVertices; i++)
        //    {
        //        myAllPointOfPoly.Add(myPolyline.GetPoint3dAt(i));
        //    }

        //    // Lay minmax X cua poly
        //    myAllPointOfPoly.Sort(sortByX);
        //    Point3d myMinPointX = myAllPointOfPoly[0];
        //    Point3d myMaxPointX = myAllPointOfPoly[myAllPointOfPoly.Count-1];

        //    // Lay minmax Y cua poly
        //    myAllPointOfPoly.Sort(sortByY);

        //    Point3d myMinPoinY = myAllPointOfPoly[0];
        //    Point3d myMaxPointY = myAllPointOfPoly[myAllPointOfPoly.Count - 1];

        //    my4Point.Add(myMinPointX);
        //    my4Point.Add(myMaxPointX);
        //    my4Point.Add(myMinPoinY);
        //    my4Point.Add(myMaxPointY);

        //    return my4Point;
        //}


        //HAM Poly:
        [CommandMethod("PD")]
        public static void autoDimPolyBySide()
        {
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

                double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;


                ObjectId myObjId = myCustomFunctions.GetObjectIdByType("POLYLINE,LWPOLYLINE");

                if (myObjId.ToString() == "0") return;
                if (myObjId == new ObjectId()) return;

                Polyline myPolySelected = myObjId.GetObject(OpenMode.ForWrite) as Polyline;

                if (myPolySelected.NumberOfVertices < 2) return;

                if (myPolySelected.Area == 0) return;

                myPolySelected.Closed = true;

                myPolySelected.removePointDup();

                // Pick side to insert
                // Chọn 1 diem tren man hinh de pick insert
                PromptPointResult pPtRes;
                PromptPointOptions pPtOpts = new PromptPointOptions("");

                // Prompt for the start point
                pPtOpts.Message = "\nPick a point to place Dim: ";
                pPtRes = acDoc.Editor.GetPoint(pPtOpts);

                // Exit if the user presses ESC or cancels the command
                if (pPtRes.Status == PromptStatus.Cancel) return;
                Point3d ptPositionInsert = pPtRes.Value;



                Point3d minPoint = new Point3d();
                Point3d maxPoint = new Point3d();

                bool dimDirectX = true;

                if (ptPositionInsert.Y <= myPolySelected.getMinMaxPoint()[2].Y || ptPositionInsert.Y >= myPolySelected.getMinMaxPoint()[3].Y)
                {
                    minPoint = myPolySelected.getMinMaxPoint()[0];
                    maxPoint = myPolySelected.getMinMaxPoint()[1];
                    dimDirectX = true;
                }
                else
                {
                    minPoint = myPolySelected.getMinMaxPoint()[2];
                    maxPoint = myPolySelected.getMinMaxPoint()[3];
                    dimDirectX = false;
                }

                // Lay tap hop diem

                bool sidePick = isLeftOrAbove(minPoint, maxPoint, ptPositionInsert);

                List<Point3d> listPointToDim = new List<Point3d>();
                listPointToDim.Add(minPoint);
                listPointToDim.Add(maxPoint);

                // Them 2 diem bien 
                listPointToDim.Add(myPolySelected.GeometricExtents.MinPoint);
                listPointToDim.Add(myPolySelected.GeometricExtents.MaxPoint);



                for (int i = 0; i < myPolySelected.NumberOfVertices; i++)
                {
                    Point3d myPointCheck = myPolySelected.GetPoint3dAt(i);

                    // Neu cung side thi them vao database
                    if (isLeftOrAbove(minPoint, maxPoint, myPointCheck) == sidePick)
                    {
                        listPointToDim.Add(myPointCheck);

                    }
                }

                if (dimDirectX)
                {
                    listPointToDim.Sort(sortByX);
                    CmdDim.autoDimHorizontalNotSelect(listPointToDim, ptPositionInsert);
                }
                else
                {
                    listPointToDim.Sort(sortByY);
                    CmdDim.autoDimVerticalNotSelect(listPointToDim, ptPositionInsert);
                }

                acTrans.Commit();
            }
        }

        /// <summary>
        /// 
        /// </summary>

        //[CommandMethod("APD")]
        //public static void autoDimPolyByClick()
        //{
        //    // Get the current document and database
        //    Document acDoc = Application.DocumentManager.MdiActiveDocument;
        //    Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
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

        //        double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;


        //        ObjectId myObjId = myCustomFunctions.GetObjectIdByType("POLYLINE,LWPOLYLINE");

        //        if (myObjId.ToString() == "0") return;
        //        if (myObjId == new ObjectId()) return;

        //        Polyline myPolySelected = myObjId.GetObject(OpenMode.ForWrite) as Polyline;

        //        if (myPolySelected.NumberOfVertices < 2) return;

        //        if (myPolySelected.Area == 0) return;

        //        myPolySelected.Closed = true;

        //        //// Pick side to insert
        //        //// Chọn 1 diem tren man hinh de pick insert
        //        //PromptPointResult pPtRes;
        //        //PromptPointOptions pPtOpts = new PromptPointOptions("");

        //        //// Prompt for the start point
        //        //pPtOpts.Message = "\nPick a point to place Dim: ";
        //        //pPtRes = acDoc.Editor.GetPoint(pPtOpts);

        //        //// Exit if the user presses ESC or cancels the command
        //        //if (pPtRes.Status == PromptStatus.Cancel) return;
        //        //Point3d ptPositionInsert = pPtRes.Value;
        //        Point3d ptPositionInsert = new Point3d();

        //        List<Point3d> myListPositionDim = new List<Point3d>();

        //        Point3d pDp1 = new Point3d(myPolySelected.getMinMaxPoint()[0].X - scaleCurrentDim*10,
        //            myPolySelected.getMinMaxPoint()[0].Y, 0);

        //        Point3d pDp2 = new Point3d(myPolySelected.getMinMaxPoint()[1].X + scaleCurrentDim * 10,
        //            myPolySelected.getMinMaxPoint()[1].Y, 0);

        //        Point3d pDp3 = new Point3d(myPolySelected.getMinMaxPoint()[2].X,
        //            myPolySelected.getMinMaxPoint()[2].Y - scaleCurrentDim * 10, 0);

        //        Point3d pDp4 = new Point3d(myPolySelected.getMinMaxPoint()[3].X,
        //            myPolySelected.getMinMaxPoint()[3].Y + scaleCurrentDim * 10, 0);

        //        myListPositionDim.Add(pDp1);
        //        myListPositionDim.Add(pDp2);
        //        myListPositionDim.Add(pDp3);
        //        myListPositionDim.Add(pDp4);


        //        Point3d minPoint = new Point3d();
        //        Point3d maxPoint = new Point3d();

        //        bool dimDirectX = true;
        //        foreach (Point3d myDimPositionItem in myListPositionDim)
        //        {
        //            ptPositionInsert = myDimPositionItem;
        //            if (ptPositionInsert.Y < myPolySelected.getMinMaxPoint()[2].Y || ptPositionInsert.Y > myPolySelected.getMinMaxPoint()[3].Y)
        //            {
        //                minPoint = myPolySelected.getMinMaxPoint()[0];
        //                maxPoint = myPolySelected.getMinMaxPoint()[1];
        //                dimDirectX = true;
        //            }
        //            else
        //            {
        //                minPoint = myPolySelected.getMinMaxPoint()[2];
        //                maxPoint = myPolySelected.getMinMaxPoint()[3];
        //                dimDirectX = false;
        //            }

        //            // Lay tap hop diem

        //            bool sidePick = isLeftOrAbove(minPoint, maxPoint, ptPositionInsert);

        //            List<Point3d> listPointToDim = new List<Point3d>();
        //            listPointToDim.Add(minPoint);
        //            listPointToDim.Add(maxPoint);

        //            for (int i = 0; i < myPolySelected.NumberOfVertices; i++)
        //            {
        //                Point3d myPointCheck = myPolySelected.GetPoint3dAt(i);

        //                // Neu cung side thi them vao database
        //                if (isLeftOrAbove(minPoint, maxPoint, myPointCheck) == sidePick)
        //                {
        //                    listPointToDim.Add(myPointCheck);
        //                }
        //            }

        //            if (dimDirectX)
        //            {
        //                listPointToDim.Sort(sortByX);
        //                CmdDim.autoDimHorizontalNotSelect(listPointToDim, ptPositionInsert);
        //            }
        //            else
        //            {
        //                listPointToDim.Sort(sortByY);
        //                CmdDim.autoDimVerticalNotSelect(listPointToDim, ptPositionInsert);
        //            }
        //        }

        //        acTrans.Commit();
        //    }
        //}


        // Nhan 1 parameter la 1 polyline sau do dim chung



        public static void DimPolyLineByObject(Polyline myPolySelected)
        {
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

                double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;

                // remove duplicate point

                myPolySelected.removePointDup();

                if (myPolySelected.NumberOfVertices < 3) return;

                if (myPolySelected.Area == 0) return;

                myPolySelected.Closed = true;

                //remove 


                // PrinExten


                //// Pick side to insert
                //// Chọn 1 diem tren man hinh de pick insert
                //PromptPointResult pPtRes;
                //PromptPointOptions pPtOpts = new PromptPointOptions("");

                //// Prompt for the start point
                //pPtOpts.Message = "\nPick a point to place Dim: ";
                //pPtRes = acDoc.Editor.GetPoint(pPtOpts);

                //// Exit if the user presses ESC or cancels the command
                //if (pPtRes.Status == PromptStatus.Cancel) return;
                //Point3d ptPositionInsert = pPtRes.Value;
                Point3d ptPositionInsert = new Point3d();

                List<Point3d> myListPositionDim = new List<Point3d>();

                Point3d pDp1 = new Point3d(Math.Min(myPolySelected.getMinMaxPoint()[0].X - scaleCurrentDim * 10,
                    myPolySelected.GeometricExtents.MinPoint.X - scaleCurrentDim * 10),
                    myPolySelected.getMinMaxPoint()[0].Y, 0);

                Point3d pDp2 = new Point3d(Math.Max(myPolySelected.getMinMaxPoint()[1].X + scaleCurrentDim * 10,
                    myPolySelected.GeometricExtents.MaxPoint.X + scaleCurrentDim * 10),
                    myPolySelected.getMinMaxPoint()[1].Y, 0);

                Point3d pDp3 = new Point3d(myPolySelected.getMinMaxPoint()[2].X,
                   Math.Min(myPolySelected.getMinMaxPoint()[2].Y - scaleCurrentDim * 10,
                   myPolySelected.GeometricExtents.MinPoint.Y - scaleCurrentDim * 10), 0);

                Point3d pDp4 = new Point3d(myPolySelected.getMinMaxPoint()[3].X,
                    Math.Max(myPolySelected.getMinMaxPoint()[3].Y + scaleCurrentDim * 10,
                    myPolySelected.GeometricExtents.MaxPoint.Y + scaleCurrentDim * 10), 0);

                myListPositionDim.Add(pDp1);
                myListPositionDim.Add(pDp2);
                myListPositionDim.Add(pDp3);
                myListPositionDim.Add(pDp4);


                Point3d minPoint = new Point3d();
                Point3d maxPoint = new Point3d();

                bool dimDirectX = true;
                foreach (Point3d myDimPositionItem in myListPositionDim)
                {
                    ptPositionInsert = myDimPositionItem;
                    if (ptPositionInsert.Y < myPolySelected.getMinMaxPoint()[2].Y || ptPositionInsert.Y > myPolySelected.getMinMaxPoint()[3].Y)
                    {
                        minPoint = myPolySelected.getMinMaxPoint()[0];
                        maxPoint = myPolySelected.getMinMaxPoint()[1];
                        dimDirectX = true;
                    }
                    else
                    {
                        minPoint = myPolySelected.getMinMaxPoint()[2];
                        maxPoint = myPolySelected.getMinMaxPoint()[3];
                        dimDirectX = false;
                    }

                    // Lay tap hop diem

                    bool sidePick = isLeftOrAbove(minPoint, maxPoint, ptPositionInsert);

                    List<Point3d> listPointToDim = new List<Point3d>();
                    listPointToDim.Add(minPoint);
                    listPointToDim.Add(maxPoint);


                    // Them 2 diem bien extent
                    listPointToDim.Add(myPolySelected.GeometricExtents.MinPoint);
                    listPointToDim.Add(myPolySelected.GeometricExtents.MaxPoint);


                    for (int i = 0; i < myPolySelected.NumberOfVertices; i++)
                    {
                        Point3d myPointCheck = myPolySelected.GetPoint3dAt(i);

                        // Neu cung side thi them vao database
                        if (isLeftOrAbove(minPoint, maxPoint, myPointCheck) == sidePick)
                        {
                            listPointToDim.Add(myPointCheck);
                        }
                    }

                    if (dimDirectX)
                    {
                        listPointToDim.Sort(sortByX);
                        CmdDim.autoDimHorizontalNotSelect(listPointToDim, ptPositionInsert);
                    }
                    else
                    {
                        listPointToDim.Sort(sortByY);
                        CmdDim.autoDimVerticalNotSelect(listPointToDim, ptPositionInsert);
                    }
                }

                acTrans.Commit();
            }
        }

        [CommandMethod("MPD")]
        public static void autoDimMulPolyline()
        {// Get the current document and database
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

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        DimPolyLineByObject(myPoly);
                    }
                }
                acTrans.Commit();
                return;
            }
        }


        public static bool isLeftOrAbove(Point3d POL1, Point3d POL2, Point3d PO)
        {
            return ((POL2.X - POL1.X) * (PO.Y - POL1.Y) - (POL2.Y - POL1.Y) * (PO.X - POL1.X)) > 0;
        }


        static int sortByY(Point3d a, Point3d b)
        {
            if (a.Y == b.Y)
                return a.X.CompareTo(b.X);
            return a.Y.CompareTo(b.Y);
        }

        static int sortByX(Point3d a, Point3d b)
        {
            if (a.X == b.X)
                return a.Y.CompareTo(b.Y);
            return a.X.CompareTo(b.X);
        }
    }

    public class PolylineSegment
    {

        [CommandMethod("DAP")]
        public static void InspectPolyline()
        {

            //Create layer Dim
            CmdLayer.createALayerByName("DIM");

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var ed = acDoc.Editor;

            var options = new PromptEntityOptions("\nSelect Polyline: ");
            options.SetRejectMessage("\nSelected object is no a Polyline.");
            options.AddAllowedClass(typeof(Polyline), true);

            var result = ed.GetEntity(options);
            if (result.Status == PromptStatus.OK)
            {
                // at this point we know an entity have been selected and it is a Polyline
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
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


                    var pline = (Polyline)acTrans.GetObject(result.ObjectId, OpenMode.ForWrite);

                    //pline.Closed = true;

                    //Xoa cac diem trung nhau
                    pline.removePointDup();



                    double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;

                    // Kiem tra chieu polyline de  of set
                    double PolyArea = pline.GetArea();

                    // if area <0 10*5
                    double offsetDistance = 10 * scaleCurrentDim;


                    if (PolyArea < 0)
                    {
                        offsetDistance = offsetDistance * -1;
                    }


                    DBObjectCollection myObjectsOffsetOut = pline.GetOffsetCurves(offsetDistance / 2);
                    DBObjectCollection myObjectsOffsetIn = pline.GetOffsetCurves(-1 * offsetDistance / 2);

                    if (myObjectsOffsetOut == null)
                    {
                        myObjectsOffsetOut = pline.GetOffsetCurves(1);
                        if (myObjectsOffsetOut == null) return;
                    }


                    //Se co bug o day

                    Polyline myPolyOffsetOut = new Polyline();
                    foreach (DBObject myObject in myObjectsOffsetOut)
                    {
                        myPolyOffsetOut = myObject as Polyline;
                        break;
                    }


                    // iterte through all segments
                    for (int i = 0; i < pline.NumberOfVertices; i++)
                    {
                        switch (pline.GetSegmentType(i))
                        {
                            case SegmentType.Arc:
                                CircularArc2d arc = pline.GetArcSegment2dAt(i);
                                Point3d Center = new Point3d(arc.Center.X, arc.Center.Y, 0);
                                Point3d XlineP1 = new Point3d(arc.StartPoint.X, arc.StartPoint.Y, 0);
                                Point3d XlineP2 = new Point3d(arc.EndPoint.X, arc.EndPoint.Y, 0);

                                double R = arc.Radius;

                                // Do math
                                Vector3d vec1 = new Vector3d(XlineP1.X - Center.X, XlineP1.Y - Center.Y, 0);
                                Vector3d vec2 = new Vector3d(XlineP2.X - Center.X, XlineP2.Y - Center.Y, 0);
                                Vector3d vecM = vec1.Add(vec2);
                                Vector3d vecN = vecM.GetNormal();



                                Point3d middlePoint = new Point3d(Center.X + vecN.X * (R), Center.Y + vecN.Y * (R), 0);

                                Point3d placeDimPoint = new Point3d(Center.X + vecN.X * (R + 10 * scaleCurrentDim / 2), Center.Y + vecN.Y * (R + 10 * scaleCurrentDim / 2), 0);

                                ed.WriteMessage("trung diem: {0} \n\n", placeDimPoint.ToString());


                                string dimtext = "<>";
                                ObjectId dimstyle = acCurDb.Dimstyle;

                                using (ArcDimension acArcDim = new ArcDimension(Center, XlineP1, XlineP2, placeDimPoint, dimtext, dimstyle))
                                {
                                    acArcDim.Layer = "DIM";
                                    acBlkTblRec.AppendEntity(acArcDim);
                                    acTrans.AddNewlyCreatedDBObject(acArcDim, true);
                                }

                                using (RadialDimension acRadDim = new RadialDimension())
                                {
                                    acRadDim.Center = Center;
                                    acRadDim.ChordPoint = middlePoint;
                                    acRadDim.TextPosition = new Point3d((middlePoint.X + Center.X) / 2, (middlePoint.Y + Center.Y) / 2, 0);
                                    acRadDim.DimensionStyle = dimstyle;
                                    acRadDim.Layer = "DIM";


                                    acBlkTblRec.AppendEntity(acRadDim);
                                    acTrans.AddNewlyCreatedDBObject(acRadDim, true);
                                }


                                break;
                            case SegmentType.Line:
                                // DimAlign
                                LineSegment2d line = pline.GetLineSegment2dAt(i);

                                if (i < pline.NumberOfVertices)
                                {
                                    if (pline.GetSegmentType(i + 1) == SegmentType.Line)
                                    {

                                        LineSegment2d line2 = pline.GetLineSegment2dAt(i + 1);
                                        using (LineAngularDimension2 myAngular = new LineAngularDimension2())
                                        {
                                            myAngular.XLine1Start = new Point3d(line.StartPoint.X, line.StartPoint.Y, 0);
                                            myAngular.XLine1End = new Point3d(line.EndPoint.X, line.EndPoint.Y, 0);
                                            myAngular.XLine2Start = new Point3d(line2.StartPoint.X, line2.StartPoint.Y, 0);
                                            myAngular.XLine2End = new Point3d(line2.EndPoint.X, line2.EndPoint.Y, 0);
                                            myAngular.ArcPoint = PointFunction.getSymmetryPoint(myPolyOffsetOut.GetPoint3dAt(i + 1), pline.GetPoint3dAt(i + 1));
                                            myAngular.DimensionStyle = acCurDb.Dimstyle;
                                            myAngular.Layer = "DIM";


                                            acBlkTblRec.AppendEntity(myAngular);
                                            acTrans.AddNewlyCreatedDBObject(myAngular, true);
                                        }
                                    }

                                }

                                //ABS vector
                                Vector2d absVector = new Vector2d(Math.Abs(line.Direction.X), Math.Abs(line.Direction.Y));

                                //if (absVector == new Vector2d(0, 1) || absVector == new Vector2d(1, 0)) break;


                                //Dime line by alight line if line not ortho
                                using (AlignedDimension myDimAlign = new AlignedDimension())
                                {
                                    myDimAlign.XLine1Point = new Point3d(line.StartPoint.X, line.StartPoint.Y, 0);
                                    myDimAlign.XLine2Point = new Point3d(line.EndPoint.X, line.EndPoint.Y, 0);
                                    myDimAlign.DimLinePoint = myPolyOffsetOut.GetPoint3dAt(i);
                                    myDimAlign.Layer = "DIM";


                                    acBlkTblRec.AppendEntity(myDimAlign);
                                    acTrans.AddNewlyCreatedDBObject(myDimAlign, true);

                                }

                                break;
                            default:
                                LineSegment2d line0 = pline.GetLineSegment2dAt(i - 1);

                                if (pline.GetSegmentType(0) == SegmentType.Line && pline.GetSegmentType(i - 1) == SegmentType.Line)
                                {
                                    LineSegment2d line00 = pline.GetLineSegment2dAt(0);
                                    using (LineAngularDimension2 myAngular = new LineAngularDimension2())
                                    {
                                        myAngular.XLine1Start = new Point3d(line0.StartPoint.X, line0.StartPoint.Y, 0);
                                        myAngular.XLine1End = new Point3d(line0.EndPoint.X, line0.EndPoint.Y, 0);
                                        myAngular.XLine2Start = new Point3d(line00.StartPoint.X, line00.StartPoint.Y, 0);
                                        myAngular.XLine2End = new Point3d(line00.EndPoint.X, line00.EndPoint.Y, 0);
                                        myAngular.ArcPoint = PointFunction.getSymmetryPoint(myPolyOffsetOut.GetPoint3dAt(0), pline.GetPoint3dAt(0));
                                        myAngular.DimensionStyle = acCurDb.Dimstyle;
                                        myAngular.Layer = "DIM";


                                        acBlkTblRec.AppendEntity(myAngular);
                                        acTrans.AddNewlyCreatedDBObject(myAngular, true);
                                    }
                                }



                                ed.WriteMessage("\n\n Segment {0} : zero length segment", i);
                                break;
                        }
                    }
                    acTrans.Commit();
                }
                //Application.DisplayTextScreen =;
            }
        }



        public static void DimAllPolyline(Polyline myPoly)
        {

            //Create layer Dim
            CmdLayer.createALayerByName("DIM");

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var ed = acDoc.Editor;

            {
                // at this point we know an entity have been selected and it is a Polyline
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
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


                    var pline = myPoly;

                    //pline.Closed = true;

                    //Xoa cac diem trung nhau
                    pline.removePointDup();


                    double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;

                    // Kiem tra chieu polyline de  of set
                    double PolyArea = pline.GetArea();

                    // if area <0 10*5
                    double offsetDistance = 10 * scaleCurrentDim;


                    if (PolyArea < 0)
                    {
                        offsetDistance = offsetDistance * -1;
                    }


                    DBObjectCollection myObjectsOffsetOut = pline.GetOffsetCurves(offsetDistance / 2);
                    DBObjectCollection myObjectsOffsetIn = pline.GetOffsetCurves(-1 * offsetDistance / 2);

                    if (myObjectsOffsetOut == null)
                    {
                        myObjectsOffsetOut = pline.GetOffsetCurves(1);
                        if (myObjectsOffsetOut == null) return;
                    }


                    //Se co bug o day

                    Polyline myPolyOffsetOut = new Polyline();
                    foreach (DBObject myObject in myObjectsOffsetOut)
                    {
                        myPolyOffsetOut = myObject as Polyline;
                        break;
                    }


                    // iterte through all segments
                    for (int i = 0; i < pline.NumberOfVertices; i++)
                    {
                        switch (pline.GetSegmentType(i))
                        {
                            case SegmentType.Arc:
                                CircularArc2d arc = pline.GetArcSegment2dAt(i);
                                Point3d Center = new Point3d(arc.Center.X, arc.Center.Y, 0);
                                Point3d XlineP1 = new Point3d(arc.StartPoint.X, arc.StartPoint.Y, 0);
                                Point3d XlineP2 = new Point3d(arc.EndPoint.X, arc.EndPoint.Y, 0);

                                double R = arc.Radius;

                                // Do math
                                Vector3d vec1 = new Vector3d(XlineP1.X - Center.X, XlineP1.Y - Center.Y, 0);
                                Vector3d vec2 = new Vector3d(XlineP2.X - Center.X, XlineP2.Y - Center.Y, 0);
                                Vector3d vecM = vec1.Add(vec2);
                                Vector3d vecN = vecM.GetNormal();



                                Point3d middlePoint = new Point3d(Center.X + vecN.X * (R), Center.Y + vecN.Y * (R), 0);

                                Point3d placeDimPoint = new Point3d(Center.X + vecN.X * (R + 10 * scaleCurrentDim / 2), Center.Y + vecN.Y * (R + 10 * scaleCurrentDim / 2), 0);

                                ed.WriteMessage("trung diem: {0} \n\n", placeDimPoint.ToString());


                                string dimtext = "<>";
                                ObjectId dimstyle = acCurDb.Dimstyle;

                                using (ArcDimension acArcDim = new ArcDimension(Center, XlineP1, XlineP2, placeDimPoint, dimtext, dimstyle))
                                {
                                    acArcDim.Layer = "DIM";
                                    acBlkTblRec.AppendEntity(acArcDim);
                                    acTrans.AddNewlyCreatedDBObject(acArcDim, true);
                                }

                                using (RadialDimension acRadDim = new RadialDimension())
                                {
                                    acRadDim.Center = Center;
                                    acRadDim.ChordPoint = middlePoint;
                                    acRadDim.TextPosition = new Point3d((middlePoint.X + Center.X) / 2, (middlePoint.Y + Center.Y) / 2, 0);
                                    acRadDim.DimensionStyle = dimstyle;
                                    acRadDim.Layer = "DIM";


                                    acBlkTblRec.AppendEntity(acRadDim);
                                    acTrans.AddNewlyCreatedDBObject(acRadDim, true);
                                }


                                break;
                            case SegmentType.Line:
                                // DimAlign
                                LineSegment2d line = pline.GetLineSegment2dAt(i);

                                if (i < pline.NumberOfVertices)
                                {
                                    if (pline.GetSegmentType(i + 1) == SegmentType.Line)
                                    {

                                        LineSegment2d line2 = pline.GetLineSegment2dAt(i + 1);
                                        using (LineAngularDimension2 myAngular = new LineAngularDimension2())
                                        {
                                            myAngular.XLine1Start = new Point3d(line.StartPoint.X, line.StartPoint.Y, 0);
                                            myAngular.XLine1End = new Point3d(line.EndPoint.X, line.EndPoint.Y, 0);
                                            myAngular.XLine2Start = new Point3d(line2.StartPoint.X, line2.StartPoint.Y, 0);
                                            myAngular.XLine2End = new Point3d(line2.EndPoint.X, line2.EndPoint.Y, 0);
                                            myAngular.ArcPoint = PointFunction.getSymmetryPoint(myPolyOffsetOut.GetPoint3dAt(i + 1), pline.GetPoint3dAt(i + 1));
                                            myAngular.DimensionStyle = acCurDb.Dimstyle;
                                            myAngular.Layer = "DIM";


                                            acBlkTblRec.AppendEntity(myAngular);
                                            acTrans.AddNewlyCreatedDBObject(myAngular, true);
                                        }
                                    }

                                }

                                //ABS vector
                                Vector2d absVector = new Vector2d(Math.Abs(line.Direction.X), Math.Abs(line.Direction.Y));

                                //if (absVector == new Vector2d(0, 1) || absVector == new Vector2d(1, 0)) break;


                                //Dime line by alight line if line not ortho
                                using (AlignedDimension myDimAlign = new AlignedDimension())
                                {
                                    myDimAlign.XLine1Point = new Point3d(line.StartPoint.X, line.StartPoint.Y, 0);
                                    myDimAlign.XLine2Point = new Point3d(line.EndPoint.X, line.EndPoint.Y, 0);
                                    myDimAlign.DimLinePoint = myPolyOffsetOut.GetPoint3dAt(i);
                                    myDimAlign.Layer = "DIM";


                                    acBlkTblRec.AppendEntity(myDimAlign);
                                    acTrans.AddNewlyCreatedDBObject(myDimAlign, true);

                                }

                                break;
                            default:
                                LineSegment2d line0 = pline.GetLineSegment2dAt(i - 1);

                                if (pline.GetSegmentType(0) == SegmentType.Line && pline.GetSegmentType(i - 1) == SegmentType.Line)
                                {
                                    LineSegment2d line00 = pline.GetLineSegment2dAt(0);
                                    using (LineAngularDimension2 myAngular = new LineAngularDimension2())
                                    {
                                        myAngular.XLine1Start = new Point3d(line0.StartPoint.X, line0.StartPoint.Y, 0);
                                        myAngular.XLine1End = new Point3d(line0.EndPoint.X, line0.EndPoint.Y, 0);
                                        myAngular.XLine2Start = new Point3d(line00.StartPoint.X, line00.StartPoint.Y, 0);
                                        myAngular.XLine2End = new Point3d(line00.EndPoint.X, line00.EndPoint.Y, 0);
                                        myAngular.ArcPoint = PointFunction.getSymmetryPoint(myPolyOffsetOut.GetPoint3dAt(0), pline.GetPoint3dAt(0));
                                        myAngular.DimensionStyle = acCurDb.Dimstyle;
                                        myAngular.Layer = "DIM";


                                        acBlkTblRec.AppendEntity(myAngular);
                                        acTrans.AddNewlyCreatedDBObject(myAngular, true);
                                    }
                                }



                                ed.WriteMessage("\n\n Segment {0} : zero length segment", i);
                                break;
                        }
                    }
                    acTrans.Commit();
                }
                //Application.DisplayTextScreen =;
            }
        }


        [CommandMethod("MAP")]
        public static void MutilDimAllPoly()
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

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        DimAllPolyline(myPoly);
                    }
                }
                acTrans.Commit();
                return;
            }
        }


    }




    public class PolylineArea
    {
        public static void singlePickPolyGetDim(string nameArea, Polyline myPolySelected)
        {
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
   
    
        [CommandMethod("getAreaDim")]
        public static void getNameArea()
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
                    csvContent.AppendLine("Tên tấm, Rộng, Dài, Số Lượng");


                    if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


                    string mySymbolName = pStrRes.StringResult.ToUpper();

                    Dictionary<string,int> myDic = new Dictionary<string,int>();

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        Point3d myMinPoint = myPoly.GeometricExtents.MinPoint;
                        Point3d myMaxPoint = myPoly.GeometricExtents.MaxPoint;

                        double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X), 0);
                        double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y), 0);


                        string myNameArea = mySymbolName + "-" + deltaX + " x " + deltaY;

                        if (myDic.ContainsKey(myNameArea))
                        {
                            myDic[myNameArea]++;
                        }
                        else
                        {
                            myDic.Add(myNameArea, 1);
                        }


                        //string lineToWrite = String.Format("{0},{1},{2},{3}", myNameArea, deltaX, deltaY, 1);

                        //csvContent.AppendLine(lineToWrite);

                        singlePickPolyGetDim(myNameArea, myPoly);
                    }

                    // Write file csv
                    foreach (KeyValuePair<string, int> myPlate in myDic)
                    {
                        string lineToWrite = String.Format("{0},{1}", myPlate.Key, myPlate.Value);
                        csvContent.AppendLine(lineToWrite);
                    }


                    string pathCsv = "D:\\abc.csv";
                    File.AppendAllText(pathCsv, csvContent.ToString());
                }
                acTrans.Commit();
                return;
            }
        
        }


        [CommandMethod("GAD")]
        public static void getNameArea2()
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

                        singlePickPolyGetDim(myNameArea, myPoly);
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



        [CommandMethod("GSD")]
        public static void getDimBySection()
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
                    csvContent.AppendLine("TEN, RONG, DAI, SO LUONG, VAT LIEU");


                    if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


                    string mySymbolName = pStrRes.StringResult.ToUpper();

                    Dictionary<myCustomSection, int> myDic = new Dictionary<myCustomSection, int>(new CustomerSectionEqualityComparer());

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        Point3d myMinPoint = myPoly.GeometricExtents.MinPoint;
                        Point3d myMaxPoint = myPoly.GeometricExtents.MaxPoint;

                        double deltaX = Math.Round(Math.Abs(myMaxPoint.X - myMinPoint.X), 0);
                        double deltaY = Math.Round(Math.Abs(myMaxPoint.Y - myMinPoint.Y), 0);


                        string myNameArea = mySymbolName + "-" + deltaX + " x " + deltaY;

                        string myMaterial = myPoly.Layer;

                        myCustomSection mySection = new myCustomSection(myNameArea, deltaX, deltaY, myMaterial);

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

                        singlePickPolyGetDim(myNameArea, myPoly);
                    }

                    // Write file csv
                    foreach (KeyValuePair<myCustomSection, int> myPlateSec in myDic)
                    {
                        string lineToWrite = String.Format("{0},{1},{2},{3},{4}", myPlateSec.Key.Name, myPlateSec.Key.Width, myPlateSec.Key.Height, myPlateSec.Value, myPlateSec.Key.Material);
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
    
    
    
    }

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



    public class myCustomSection
    {
        public myCustomSection(string namePlate, double width, double height, string material )
        {
            this.name = namePlate;
            this.width = width;
            this.height = height;
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
        public double Height
        {
            get { return height; }
            set { height = value; }
        }

        public string Material
        {
            get { return material; }
            set { material = value; }
        }

        string name;
        double width;
        double height;
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
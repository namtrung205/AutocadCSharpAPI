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


namespace myCustomCmds
{
    public class CmdDetailParts
    {

        public void ClockwisePolyline()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr =
                    (BlockTableRecord)db.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                PromptEntityOptions peo = new PromptEntityOptions("\nSelect a polyline: ");
                peo.SetRejectMessage("Selected object is not a polyline.");
                peo.AddAllowedClass(typeof(Polyline), false);
                while (true)
                {
                    PromptEntityResult per = ed.GetEntity(peo);
                    if (per.Status != PromptStatus.OK) break; ;
                    Polyline pline = (Polyline)per.ObjectId.GetObject(OpenMode.ForRead);
                    double area = pline.GetArea();
                    Application.ShowAlertDialog(
                        string.Format("{0}\nArea = {1}", area < 0 ? "CW" : "CCW", area));
                }
                tr.Commit();
            }
        }


        //HAM Poly:
        [CommandMethod("PP")]
        public static void drawDetailFromPoly()
        {

            if (!CheckLicense.licensed) return;

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


                ObjectId myObjId = myCustomFunctions.GetObjectIdByType("Polyline");

                if (myObjId.ToString() == "0") return ;
                if (myObjId == new ObjectId()) return ;


                string classObject =  myObjId.GetObject(OpenMode.ForRead).GetRXClass().DxfName;

                if (classObject == "POLYLINE" || classObject == "LWPOLYLINE")
                {

                Polyline mySection = myObjId.GetObject(OpenMode.ForWrite) as Polyline;

                if (mySection.NumberOfVertices < 2) return;

                if (mySection.Area == 0) return;

                mySection.Closed = true;

                // Tao clone cho polyline
                Polyline myCloneSection = (Polyline)mySection.Clone();

                //Lay toan bo vertex cua clone cho vao list va conection Point
                List<Point3d> myListPoint3dSection = new List<Point3d>();
                Point3dCollection myCollPoint3dSection = new Point3dCollection();

                for(int i = 0; i< myCloneSection.NumberOfVertices; i++)
                {
                    myListPoint3dSection.Add(myCloneSection.GetPoint3dAt(i));
                    myCollPoint3dSection.Add(myCloneSection.GetPoint3dAt(i));
                }

                // Lay diem thap nhat
                myListPoint3dSection.Sort(sortByY);

                Point3d myBasePoint = myListPoint3dSection[0];

                // Move clone to new Position

                // Chọn 1 diem tren man hinh de pick insert
                PromptPointResult pPtRes;
                PromptPointOptions pPtOpts = new PromptPointOptions("");

                // Prompt for the start point
                pPtOpts.Message = "\nPick a point to place CallOut: ";
                pPtRes = acDoc.Editor.GetPoint(pPtOpts);

                // Exit if the user presses ESC or cancels the command
                if (pPtRes.Status == PromptStatus.Cancel) return;

                Point3d ptPositionInsert = pPtRes.Value;
                // vector move tu tam duong tron goc tới diểm chọn
                Vector3d myVectorMove = myBasePoint.GetVectorTo(ptPositionInsert);

                myCloneSection.TransformBy(Matrix3d.Displacement(myVectorMove));

                //Dim clone polyline
                DimPoly.DimPolyLineByObject(myCloneSection);


                // Kiem tra chieu polyline de  of set
                double PolyArea = myCloneSection.GetArea();

                // if area <0 10*5
                double offsetDistance = 10 * scaleCurrentDim;


                if (PolyArea < 0)
                {
                    offsetDistance = offsetDistance * -1;
                }


                //DBObjectCollection myObjectsOffset = myCloneSection.GetOffsetCurves(offsetDistance);
                //foreach (Entity myObject in myObjectsOffset)
                //{
                //    acDoc.Editor.WriteMessage("\n{0} : la 1 ten class", myObject.GetRXClass().DxfName);

                //    acBlkTblRec.AppendEntity(myObject);
                //    acTrans.AddNewlyCreatedDBObject(myObject, true);

                //}



                // Lay cac kich thuoc thong dung

               acDoc.Editor.WriteMessage(" \nExtents----{0}:", myCloneSection.GeometricExtents.ToString());
                // extent dimensions:
               double deltaX = Math.Abs(myCloneSection.GeometricExtents.MaxPoint.X - myCloneSection.GeometricExtents.MinPoint.X);
               double deltaY = Math.Abs(myCloneSection.GeometricExtents.MaxPoint.Y - myCloneSection.GeometricExtents.MinPoint.Y);

               double maxDelta = Math.Max(deltaX, deltaY);

               acDoc.Editor.WriteMessage("deltaX: {0}, deltaY: {1}", deltaX, deltaY);



                // Nhap kich thuoc tam
               double thickness = 0;
               if (thickness <= 0)
               {
                   PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
                   pIntOpts.Message = "\nEnter thickness: ";
                   //pIntOpts.DefaultValue = 100;

                   PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);

                   // Restrict input to positive and non-negative values
                   pIntOpts.AllowZero = false;
                   pIntOpts.AllowNegative = false;

                   if (pIntRes == null) return;
                   if (pIntRes.Value <= 0) return;

                   //Set thickness from input 
                   thickness = pIntRes.Value;
               }


                // ve duong bao kich thuoc

               if (deltaY >= 2*deltaX) // Ve phuong ngang
               {
                   // Ve 1 hinh chieu nhu tren theo mat cat binh thuong
                   Point3d myBasePointPlate = new Point3d(myCloneSection.GeometricExtents.MaxPoint.X + scaleCurrentDim * 30, myCloneSection.GeometricExtents.MinPoint.Y, 0);

                   using(Polyline myPlate2d = new Polyline())
                   {
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X, myBasePointPlate.Y), 0, 0, 0);
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X +thickness, myBasePointPlate.Y), 0, 0, 0);
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X + thickness, myBasePointPlate.Y + deltaY), 0, 0, 0);
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X, myBasePointPlate.Y + deltaY), 0, 0, 0);
                       myPlate2d.Closed = true;


                       acBlkTblRec.AppendEntity(myPlate2d);
                       acTrans.AddNewlyCreatedDBObject(myPlate2d, true);

                       DimPoly.DimPolyLineByObject(myPlate2d);
                   }   
               }
               else if (deltaX >= 2 * deltaY) // Ve phuong doc
               {
                   // Ve 1 hinh chieu nhu tren theo mat cat binh thuong
                   Point3d myBasePointPlate = new Point3d(myCloneSection.GeometricExtents.MaxPoint.X, myCloneSection.GeometricExtents.MaxPoint.Y + scaleCurrentDim * 30, 0);

                   using (Polyline myPlate2d = new Polyline())
                   {
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X, myBasePointPlate.Y), 0, 0, 0);
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X, myBasePointPlate.Y + thickness), 0, 0, 0);
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X - deltaX, myBasePointPlate.Y + thickness), 0, 0, 0);
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X - deltaX, myBasePointPlate.Y), 0, 0, 0);
                       myPlate2d.Closed = true;

                       acBlkTblRec.AppendEntity(myPlate2d);
                       acTrans.AddNewlyCreatedDBObject(myPlate2d, true);

                       DimPoly.DimPolyLineByObject(myPlate2d);
                   }
               }
               else // Ve ca 2 phuong
               {
                   Point3d myBasePointPlate = new Point3d(myCloneSection.GeometricExtents.MaxPoint.X + scaleCurrentDim * 30, myCloneSection.GeometricExtents.MinPoint.Y, 0);

                   using (Polyline myPlate2d = new Polyline())
                   {
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X, myBasePointPlate.Y), 0, 0, 0);
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X + thickness, myBasePointPlate.Y), 0, 0, 0);
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X + thickness, myBasePointPlate.Y + deltaY), 0, 0, 0);
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X, myBasePointPlate.Y + deltaY), 0, 0, 0);
                       myPlate2d.Closed = true;

                       acBlkTblRec.AppendEntity(myPlate2d);
                       acTrans.AddNewlyCreatedDBObject(myPlate2d, true);

                       DimPoly.DimPolyLineByObject(myPlate2d);
                   }

                   myBasePointPlate = new Point3d(myCloneSection.GeometricExtents.MaxPoint.X, myCloneSection.GeometricExtents.MaxPoint.Y + scaleCurrentDim * 30, 0);

                   using (Polyline myPlate2d = new Polyline())
                   {
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X, myBasePointPlate.Y), 0, 0, 0);
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X, myBasePointPlate.Y + thickness), 0, 0, 0);
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X - deltaX, myBasePointPlate.Y + thickness), 0, 0, 0);
                       myPlate2d.AddVertexAt(0, new Point2d(myBasePointPlate.X - deltaX, myBasePointPlate.Y), 0, 0, 0);
                       myPlate2d.Closed = true;

                       acBlkTblRec.AppendEntity(myPlate2d);
                       acTrans.AddNewlyCreatedDBObject(myPlate2d, true);

                       DimPoly.DimPolyLineByObject(myPlate2d);
                   }
               }

                acBlkTblRec.AppendEntity(myCloneSection);
                acTrans.AddNewlyCreatedDBObject(myCloneSection, true);

                acTrans.Commit();
            }

        }
        }



        //HAM Poly:
        //[CommandMethod("DDBD")]
        public static void drawDetailFromText(Point3d myPointPlace, double width, double height, double thickness, string Title)
        {

            //if (!CheckLicense.licensed) return;

            CmdLayer.createALayerByName("SECTION_HATCH");
            CmdLayer.createALayerByName("TEXT_TITLE");

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
                // Get Dim Scale to set height for text and scale of drawing
                double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;

                // Ve cac hinh chieu tu cac kich thuoc da cho

                Point3d myPointPlaceSec = new Point3d();
                // Ve chieu dung
                using (Polyline drawPlateArea = new Polyline())
                {
                    drawPlateArea.AddVertexAt(0, new Point2d(myPointPlace.X, myPointPlace.Y), 0, 0, 0);
                    drawPlateArea.AddVertexAt(0, new Point2d(myPointPlace.X + width, myPointPlace.Y), 0, 0, 0);
                    drawPlateArea.AddVertexAt(0, new Point2d(myPointPlace.X + width, myPointPlace.Y + height), 0, 0, 0);
                    drawPlateArea.AddVertexAt(0, new Point2d(myPointPlace.X, myPointPlace.Y + height), 0, 0, 0);
                    drawPlateArea.Closed = true;

                    DimPoly.DimPolyLineByObject(drawPlateArea);

                    // Doi vi tri chen diem
                    myPointPlaceSec = new Point3d(myPointPlace.X + width + scaleCurrentDim * 30, myPointPlace.Y, 0);

                    acBlkTblRec.AppendEntity(drawPlateArea);
                    acTrans.AddNewlyCreatedDBObject(drawPlateArea, true);
                }

                // Ve chieu ben

                using (Polyline drawPlateSec = new Polyline())
                {
                    drawPlateSec.AddVertexAt(0, new Point2d(myPointPlaceSec.X, myPointPlaceSec.Y), 0, 0, 0);
                    drawPlateSec.AddVertexAt(0, new Point2d(myPointPlaceSec.X + thickness, myPointPlaceSec.Y), 0, 0, 0);
                    drawPlateSec.AddVertexAt(0, new Point2d(myPointPlaceSec.X + thickness, myPointPlaceSec.Y + height), 0, 0, 0);
                    drawPlateSec.AddVertexAt(0, new Point2d(myPointPlaceSec.X, myPointPlaceSec.Y + height), 0, 0, 0);
                    drawPlateSec.Closed = true;



                    acBlkTblRec.AppendEntity(drawPlateSec);
                    acTrans.AddNewlyCreatedDBObject(drawPlateSec, true);


                    // Them hatch, ve dim
                    DimPoly.DimPolyLineByObject(drawPlateSec);


                    ObjectIdCollection acObjIdColl = new ObjectIdCollection();
                    acObjIdColl.Add(drawPlateSec.ObjectId);

                    using (Hatch acHatch = new Hatch())
                    {
                        acBlkTblRec.AppendEntity(acHatch);
                        acTrans.AddNewlyCreatedDBObject(acHatch, true);

                        // Set the properties of the hatch object
                        // Associative must be set after the hatch object is appended to the 
                        // block table record and before AppendLoop

                        acHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI35");
                        acHatch.Associative = true;
                        acHatch.PatternScale = 20;
                        acHatch.ColorIndex = 8;
                        acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                        acHatch.EvaluateHatch(true);
                        acHatch.Layer = "SECTION_HATCH";
                    }
                }

                Point3d myPointPlaceTitle = new Point3d(myPointPlace.X+width/2, myPointPlace.Y+height+20*scaleCurrentDim,0);
                // Them Title
                CmdText.CreateText(Title, "TEXT_TITLE", myPointPlaceTitle, scaleCurrentDim, 3.5);



            acTrans.Commit();
            }

        }
        

        [CommandMethod("TestDDBT")]
        public static void test2()
        {
            
            if (!CheckLicense.licensed) return;

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



                // Chọn 1 diem tren man hinh de pick insert
                PromptPointResult pPtRes;
                PromptPointOptions pPtOpts = new PromptPointOptions("");

                // Prompt for the start point
                pPtOpts.Message = "\nPick a point to place CallOut: ";
                pPtRes = acDoc.Editor.GetPoint(pPtOpts);

                // Exit if the user presses ESC or cancels the command
                if (pPtRes.Status == PromptStatus.Cancel) return;

                Point3d ptPositionInsert = pPtRes.Value;

                drawDetailFromText(ptPositionInsert, 1000, 250, 25, "Hello World");

                acTrans.Commit();
            }
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
}

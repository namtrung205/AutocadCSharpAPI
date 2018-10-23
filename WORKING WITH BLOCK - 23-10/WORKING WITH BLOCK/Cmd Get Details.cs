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
    public class GetDetails
    {

        public static Point3dCollection convertCtoP(Circle myBorderCircle)
        {
            Point3d centre = myBorderCircle.Center;
            double radius = myBorderCircle.Radius;
            int numPoints = 36;

            double angle = 2 * Math.PI / numPoints;


            Point3dCollection outP3dColl = null;
            for (int i = 0; i < numPoints; i++)
            {
                double x = centre.X + radius * Math.Sin(i * angle);
                double y = centre.Y + radius * Math.Cos(i * angle);
                outP3dColl.Add(new Point3d(x, y, 0));
            }
            return outP3dColl;

        }


        // NHOM HAM TRIMMER VA TAO CALLOUT

        [CommandMethod("CCO", CommandFlags.Modal)]
        public static void CreateCallOutCircle()
        {
            try
            {
                //ungroup
                int currentPickStyle = Convert.ToInt32(Application.GetSystemVariable("PICKSTYLE").ToString());

                if (currentPickStyle != 0)
                {
                    Application.SetSystemVariable("PICKSTYLE", 0);

                }

                // Chon 1 doi tuong lam border
                Document acCurDoc = Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acCurDoc.Database;
                Editor acEd = acCurDoc.Editor;
                PromptEntityOptions acPEO = new PromptEntityOptions("\nSelect border to Clone: ");
                acPEO.SetRejectMessage("Only accept Circle");
                acPEO.AddAllowedClass(typeof(Circle), true);
                PromptEntityResult acSSPrompt = acEd.GetEntity(acPEO);
                if (acSSPrompt.Status != PromptStatus.OK) return;
                acEd.WriteMessage("Has picked a Circle as border!");

                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read
                    BlockTable acBlkTbl;
                    acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                    OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                    OpenMode.ForWrite) as BlockTableRecord;
                    // P3C chứa các điểm tạo polyselect

                    Point3dCollection pntCol = new Point3dCollection();

                    DBObject obj = acTrans.GetObject(acSSPrompt.ObjectId, OpenMode.ForWrite);
                    // If a "lightweight" (or optimized) polyline
                    Circle borCircle = obj as Circle;
                    borCircle.Layer = "CALLOUT BORDER";

                    // tao 1 clone cua duong tron goc để copy đi chỗ khác
                    Circle myBorderClone = borCircle.Clone() as Circle;
                    myBorderClone.Layer = "CALLOUT BORDER";

                    if (acSSPrompt == null) return;

                    // Use a for loop to get each vertex, one by one
                    Point3d centre = borCircle.Center;
                    double radius = borCircle.Radius - 1;
                    int numPoints = 36;

                    double angle = 2 * Math.PI / numPoints;

                    for (int i = 0; i < numPoints; i++)
                    {
                        double x = centre.X + radius * Math.Sin(i * angle);
                        double y = centre.Y + radius * Math.Cos(i * angle);
                        pntCol.Add(new Point3d(x, y, 0));
                    }

                    // Lay tam duong tron diem nay sau nay se la diem chọn side trong extrim
                    Point3d centerPoint = borCircle.Center;

                    acCurDoc.Editor.WriteMessage("\n Cetern Point: {0} ", centerPoint.ToString());
                    TypedValue[] acTypValAr = new TypedValue[1];

                    //select circle and line
                    acTypValAr[0] = new TypedValue(0, "CIRCLE,LINE,POLYLINE,SPLINE,RAY,ARC,HATCH,ELLIPSE,LWPOLYLINE,MLINE");

                    //8 = DxfCode.LayerName

                    // Assign the filter criteria to a SelectionFilter object
                    SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);


                    // Selet all object cross polygon
                    PromptSelectionResult pmtSelRes = null;
                    pmtSelRes = acEd.SelectCrossingPolygon(pntCol, acSelFtr);


                    List<Entity> myListEntityClone = new List<Entity>();
                    if (pmtSelRes.Status == PromptStatus.OK)
                    {
                        // Them vao List clone de move
                        myListEntityClone.Add(myBorderClone);
                        foreach (ObjectId objId in pmtSelRes.Value.GetObjectIds())
                        {

                            Entity myCloneEnt = objId.GetObject(OpenMode.ForWrite).Clone() as Entity;
                            myListEntityClone.Add(myCloneEnt);
                        }
                    }

                    // Chọn 1 diem tren man hinh de pick insert
                    PromptPointResult pPtRes;
                    PromptPointOptions pPtOpts = new PromptPointOptions("");

                    // Prompt for the start point
                    pPtOpts.Message = "\nPick a point to place CallOut: ";
                    pPtRes = acCurDoc.Editor.GetPoint(pPtOpts);
                    Point3d ptPositionInsert = pPtRes.Value;

                    // Exit if the user presses ESC or cancels the command
                    if (pPtRes.Status == PromptStatus.Cancel) return;

                    // vector move tu tam duong tron goc tới diểm chọn
                    Vector3d myVectorMove = centerPoint.GetVectorTo(ptPositionInsert);

                    foreach (Entity ent in myListEntityClone)
                    {
                        ent.TransformBy(Matrix3d.Displacement(myVectorMove));
                        // Add the new object to the block table record and the transaction
                        acBlkTblRec.AppendEntity(ent);
                        acTrans.AddNewlyCreatedDBObject(ent, true);
                    }

                    // Extrim Command
                    Point3d pickPoint = new Point3d(myBorderClone.Center.X, myBorderClone.Center.Y + myBorderClone.Radius + 1000, 0);

                    Point3d pickPt3d = new Point3d(myBorderClone.Center.X, myBorderClone.Center.Y + myBorderClone.Radius, 0);

                    string pickPt = pickPt3d.ToString().Trim('(', ')');
                    string sidePt = pickPoint.ToString().Trim('(', ')');

                    string mycom = "_extrim " + pickPt + " " + sidePt + "\n";
                    //doc.SendStringToExecute($"_extrim {pickPt} {sidePt} ", false, false, false);

                    Application.SetSystemVariable("PICKSTYLE", currentPickStyle);

                    acTrans.Commit();

                    sendCommand(mycom);
                    //sendCommand("RE\n");
                    sendCommand("CRT\n");
                    sendCommand(mycom);

                    sendCommand("RE\n");


                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }

        }


        public static void sendCommand(string myCommand)
        {
            Document acCurDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acCurDoc.Database;
            Editor acEd = acCurDoc.Editor;

            acCurDoc.SendStringToExecute(myCommand, true, false, false);
        }


        ///Old COmmnads
        //[CommandMethod("CCO2", CommandFlags.Modal)]
        //public static void CreateCallOutPoly()
        //{
        //    try
        //    {
        //        //ungroup
        //        int currentPickStyle = Convert.ToInt32(Application.GetSystemVariable("PICKSTYLE").ToString());

        //        if (currentPickStyle != 0)
        //        {
        //            Application.SetSystemVariable("PICKSTYLE", 0);

        //        }

        //        // Chon 1 doi tuong lam border
        //        Document acCurDoc = Application.DocumentManager.MdiActiveDocument;
        //        Database acCurDb = acCurDoc.Database;
        //        Editor acEd = acCurDoc.Editor;
        //        PromptEntityOptions acPEO = new PromptEntityOptions("\nSelect border to Clone: ");
        //        acPEO.SetRejectMessage("Only accept Circle");
        //        acPEO.AddAllowedClass(typeof(Circle), true);
        //        PromptEntityResult acSSPrompt = acEd.GetEntity(acPEO);
        //        if (acSSPrompt.Status != PromptStatus.OK) return;
        //        acEd.WriteMessage("Has picked a Circle as border!");

        //        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //        {
        //            // Open the Block table for read
        //            BlockTable acBlkTbl;
        //            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
        //                                            OpenMode.ForRead) as BlockTable;

        //            // Open the Block table record Model space for write
        //            BlockTableRecord acBlkTblRec;
        //            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
        //                                            OpenMode.ForWrite) as BlockTableRecord;
        //            // P3C chứa các điểm tạo polyselect

        //            Point3dCollection pntCol = new Point3dCollection();

        //            DBObject obj = acTrans.GetObject(acSSPrompt.ObjectId, OpenMode.ForWrite);
        //            // If a "lightweight" (or optimized) polyline
        //            Circle borCircle = obj as Circle;
        //            borCircle.Layer = "CALLOUT BORDER";

        //            // tao 1 clone cua duong tron goc để copy đi chỗ khác
        //            Polyline myBorderClone = new Polyline();
        //            myBorderClone.Layer = "CALLOUT BORDER";

        //            if (acSSPrompt == null) return;

        //            // Use a for loop to get each vertex, one by one
        //            Point3d centre = borCircle.Center;
        //            double radius = borCircle.Radius - 1;
        //            int numPoints = 72;

        //            double angle = 2 * Math.PI / numPoints;

        //            for (int i = 0; i < numPoints; i++)
        //            {
        //                double x = centre.X + radius * Math.Sin(i * angle);
        //                double y = centre.Y + radius * Math.Cos(i * angle);
        //                pntCol.Add(new Point3d(x, y, 0));
        //                myBorderClone.AddVertexAt(i, new Point2d(x, y), 0, 0, 0);

        //            }
        //            myBorderClone.Closed = true;
        //            // Lay tam duong tron diem nay sau nay se la diem chọn side trong extrim
        //            Point3d centerPoint = borCircle.Center;

        //            acCurDoc.Editor.WriteMessage("\n Cetern Point: {0} ", centerPoint.ToString());
        //            TypedValue[] acTypValAr = new TypedValue[1];

        //            //select circle and line
        //            acTypValAr[0] = new TypedValue(0, "CIRCLE,LINE,POLYLINE,SPLINE,RAY,ARC,HATCH,ELLIPSE,LWPOLYLINE,MLINE");

        //            //8 = DxfCode.LayerName

        //            // Assign the filter criteria to a SelectionFilter object
        //            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);


        //            // Selet all object cross polygon
        //            PromptSelectionResult pmtSelRes = null;
        //            pmtSelRes = acEd.SelectCrossingPolygon(pntCol, acSelFtr);


        //            List<Entity> myListEntityClone = new List<Entity>();
        //            if (pmtSelRes.Status == PromptStatus.OK)
        //            {
        //                // Them vao List clone de move
        //                myListEntityClone.Add(myBorderClone);
        //                foreach (ObjectId objId in pmtSelRes.Value.GetObjectIds())
        //                {

        //                    Entity myCloneEnt = objId.GetObject(OpenMode.ForWrite).Clone() as Entity;
        //                    myListEntityClone.Add(myCloneEnt);
        //                }
        //            }

        //            // Chọn 1 diem tren man hinh de pick insert
        //            PromptPointResult pPtRes;
        //            PromptPointOptions pPtOpts = new PromptPointOptions("");

        //            // Prompt for the start point
        //            pPtOpts.Message = "\nPick a point to place CallOut: ";
        //            pPtRes = acCurDoc.Editor.GetPoint(pPtOpts);
        //            Point3d ptPositionInsert = pPtRes.Value;

        //            // Exit if the user presses ESC or cancels the command
        //            if (pPtRes.Status == PromptStatus.Cancel) return;

        //            // vector move tu tam duong tron goc tới diểm chọn
        //            Vector3d myVectorMove = centerPoint.GetVectorTo(ptPositionInsert);

        //            foreach (Entity ent in myListEntityClone)
        //            {
        //                ent.TransformBy(Matrix3d.Displacement(myVectorMove));
        //                // Add the new object to the block table record and the transaction
        //                acBlkTblRec.AppendEntity(ent);
        //                acTrans.AddNewlyCreatedDBObject(ent, true);
        //            }

        //            // Extrim Command
        //            Point3d pickPoint = centerPoint;

        //            Point2d pickPt3d = new Point2d(myBorderClone.GetPoint2dAt(0).X, myBorderClone.GetPoint2dAt(0).Y);

        //            string pickPt = pickPt3d.ToString().Trim('(', ')');
        //            string sidePt = pickPoint.ToString().Trim('(', ')');

        //            string mycom = "_extrim " + pickPt + " " + sidePt + " ";
        //            //doc.SendStringToExecute($"_extrim {pickPt} {sidePt} ", false, false, false);

        //            Application.SetSystemVariable("PICKSTYLE", currentPickStyle);

        //            acTrans.Commit();

        //            acCurDoc.SendStringToExecute(mycom, true, false, false);
        //            acCurDoc.SendStringToExecute("RE\n", true, false, false);


        //            acCurDoc.SendStringToExecute("CRT\n", true, false, false);

        //            acCurDoc.SendStringToExecute(mycom, true, false, false);


        //            // SetdimCurrent

        //        }
        //    }
        //    catch (Autodesk.AutoCAD.Runtime.Exception ex)
        //    {
        //        Application.ShowAlertDialog(ex.Message);
        //    }



        //}


        //[CommandMethod("CCO3", CommandFlags.Modal)]
        //public static void CreateCallOutCircle3()
        //{
        //    try
        //    {
        //        //ungroup

        //        int currentPickStyle = Convert.ToInt32(Application.GetSystemVariable("PICKSTYLE").ToString());

        //        if (currentPickStyle != 0)
        //        {
        //            Application.SetSystemVariable("PICKSTYLE", 0);

        //        }

        //        // Chon 1 doi tuong lam border
        //        Document acCurDoc = Application.DocumentManager.MdiActiveDocument;
        //        Database acCurDb = acCurDoc.Database;
        //        Editor acEd = acCurDoc.Editor;
        //        PromptEntityOptions acPEO = new PromptEntityOptions("\nSelect border to Clone: ");
        //        acPEO.SetRejectMessage("Only accept Circle");
        //        acPEO.AddAllowedClass(typeof(Circle), true);
        //        PromptEntityResult acSSPrompt = acEd.GetEntity(acPEO);
        //        if (acSSPrompt.Status != PromptStatus.OK) return;
        //        acEd.WriteMessage("Has picked a Circle as border!");

        //        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //        {
        //            // Open the Block table for read
        //            BlockTable acBlkTbl;
        //            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
        //                                            OpenMode.ForRead) as BlockTable;

        //            // Open the Block table record Model space for write
        //            BlockTableRecord acBlkTblRec;
        //            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
        //                                            OpenMode.ForWrite) as BlockTableRecord;
        //            // P3C chứa các điểm tạo polyselect

        //            Point3dCollection pntCol = new Point3dCollection();

        //            DBObject obj = acTrans.GetObject(acSSPrompt.ObjectId, OpenMode.ForWrite);
        //            // If a "lightweight" (or optimized) polyline
        //            Circle borCircle = obj as Circle;
        //            borCircle.Layer = "CALLOUT BORDER";

        //            // tao 1 clone cua duong tron goc để copy đi chỗ khác
        //            Circle myBorderClone = borCircle.Clone() as Circle;
        //            myBorderClone.Layer = "CALLOUT BORDER";

        //            if (acSSPrompt == null) return;

        //            // Use a for loop to get each vertex, one by one
        //            Point3d centre = borCircle.Center;
        //            double radius = borCircle.Radius - 1;
        //            int numPoints = 36;

        //            double angle = 2 * Math.PI / numPoints;

        //            for (int i = 0; i < numPoints; i++)
        //            {
        //                double x = centre.X + radius * Math.Sin(i * angle);
        //                double y = centre.Y + radius * Math.Cos(i * angle);
        //                pntCol.Add(new Point3d(x, y, 0));
        //            }

        //            // Lay tam duong tron diem nay sau nay se la diem chọn side trong extrim
        //            Point3d centerPoint = borCircle.Center;

        //            acCurDoc.Editor.WriteMessage("\n Cetern Point: {0} ", centerPoint.ToString());
        //            TypedValue[] acTypValAr = new TypedValue[1];

        //            //select circle and line
        //            acTypValAr[0] = new TypedValue(0, "CIRCLE,LINE,POLYLINE,SPLINE,RAY,ARC,HATCH,ELLIPSE,LWPOLYLINE,MLINE");

        //            //8 = DxfCode.LayerName

        //            // Assign the filter criteria to a SelectionFilter object
        //            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);


        //            // Selet all object cross polygon
        //            PromptSelectionResult pmtSelRes = null;
        //            pmtSelRes = acEd.SelectCrossingPolygon(pntCol, acSelFtr);


        //            List<Entity> myListEntityClone = new List<Entity>();
        //            if (pmtSelRes.Status == PromptStatus.OK)
        //            {
        //                // Them vao List clone de move
        //                myListEntityClone.Add(myBorderClone);
        //                foreach (ObjectId objId in pmtSelRes.Value.GetObjectIds())
        //                {

        //                    Entity myCloneEnt = objId.GetObject(OpenMode.ForWrite).Clone() as Entity;
        //                    myListEntityClone.Add(myCloneEnt);
        //                }
        //            }

        //            // Chọn 1 diem tren man hinh de pick insert
        //            PromptPointResult pPtRes;
        //            PromptPointOptions pPtOpts = new PromptPointOptions("");

        //            // Prompt for the start point
        //            pPtOpts.Message = "\nPick a point to place CallOut: ";
        //            pPtRes = acCurDoc.Editor.GetPoint(pPtOpts);
        //            Point3d ptPositionInsert = pPtRes.Value;

        //            // Exit if the user presses ESC or cancels the command
        //            if (pPtRes.Status == PromptStatus.Cancel) return;

        //            // vector move tu tam duong tron goc tới diểm chọn
        //            Vector3d myVectorMove = centerPoint.GetVectorTo(ptPositionInsert);


        //            // tao 1 circle

        //            Circle outCircle = new Circle();
        //            outCircle.Radius = myBorderClone.Radius + 10;
        //            outCircle.Center = myBorderClone.Center;

        //            List<Point3d> myListPointToPick = new List<Point3d>();
        //            Point3dCollection intersectionPoints = new Point3dCollection();

        //            foreach (Entity ent in myListEntityClone)
        //            {

        //                // Lay diem giao cat

        //                outCircle.IntersectWith(ent, Intersect.OnBothOperands, intersectionPoints, IntPtr.Zero, IntPtr.Zero);

        //                ent.TransformBy(Matrix3d.Displacement(myVectorMove));
        //                // Add the new object to the block table record and the transaction
        //                acBlkTblRec.AppendEntity(ent);
        //                acTrans.AddNewlyCreatedDBObject(ent, true);
        //            }



        //            // Extrim Command
        //            Point3d pickPoint = new Point3d(myBorderClone.Center.X, myBorderClone.Center.Y + myBorderClone.Radius + 1000, 0);

        //            Point3d pickPt3d = new Point3d(myBorderClone.Center.X, myBorderClone.Center.Y + myBorderClone.Radius, 0);

        //            string pickPt = pickPt3d.ToString().Trim('(', ')');
        //            string sidePt = pickPoint.ToString().Trim('(', ')');

        //            string mycom = "_extrim " + pickPt + " " + sidePt + "\n";

        //            //string mytrim1 = "_trim " + pickPt + "\n";
        //            //sendCommand(mytrim1);
        //            foreach (Point3d myIntersectionPt in intersectionPoints)
        //            {

        //                string sidePt2 = myIntersectionPt.ToString().Trim('(', ')');
        //                //acCurDoc.Editor.WriteMessage(myIntersectionPt.ToString() + "\n");  

        //                ObjectId id = myBorderClone.ObjectId;

        //                acCurDoc.Editor.Command("_.trim", id, "");
        //                acCurDoc.Editor.Command(sidePt2, "");


        //            }
        //            //doc.SendStringToExecute($"_extrim {pickPt} {sidePt} ", false, false, false);





        //            Application.SetSystemVariable("PICKSTYLE", currentPickStyle);



        //            acTrans.Commit();


        //            //sendCommand(mycom);

        //            //sendCommand("RE\n");
        //            ////sendCommand("CRT\n");
        //            //sendCommand(mycom);

        //            //sendCommand("RE\n");

        //            //acCurDoc.SendStringToExecute(mycom, true, false, false);
        //            //acCurDoc.SendStringToExecute("RE\n", true, false, false);


        //            //acCurDoc.SendStringToExecute("CRT\n", true, false, false);

        //            //acCurDoc.SendStringToExecute(mycom, true, false, false);


        //            // SetdimCurrent

        //        }
        //    }
        //    catch (Autodesk.AutoCAD.Runtime.Exception ex)
        //    {
        //        Application.ShowAlertDialog(ex.Message);
        //    }

        //}


        //[CommandMethod("CCO1", CommandFlags.Modal)]
        //public static void CreateCallOut()
        //{
        //    try
        //    {
        //        //ungroup

        //        int currentPickStyle = Convert.ToInt32(Application.GetSystemVariable("PICKSTYLE").ToString());

        //        if (currentPickStyle != 0)
        //        {
        //            Application.SetSystemVariable("PICKSTYLE", 0);

        //        }


        //        // Chon 1 doi tuong lam border
        //        Document acCurDoc = Application.DocumentManager.MdiActiveDocument;
        //        Database acCurDb = acCurDoc.Database;
        //        Editor acEd = acCurDoc.Editor;
        //        PromptEntityOptions acPEO = new PromptEntityOptions("\nSelect border to copy: ");
        //        acPEO.SetRejectMessage("Only accept PolyLine");
        //        acPEO.AddAllowedClass(typeof(Polyline), true);
        //        PromptEntityResult acSSPrompt = acEd.GetEntity(acPEO);
        //        if (acSSPrompt.Status != PromptStatus.OK) return;
        //        acEd.WriteMessage("Has picked a polyline as border!");

        //        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //        {
        //            // Open the Block table for read
        //            BlockTable acBlkTbl;
        //            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
        //                                            OpenMode.ForRead) as BlockTable;

        //            // Open the Block table record Model space for write
        //            BlockTableRecord acBlkTblRec;
        //            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
        //                                            OpenMode.ForWrite) as BlockTableRecord;

        //            Point3dCollection pntCol = new Point3dCollection();
        //            // Convert acSS to poly
        //            DBObject obj = acTrans.GetObject(acSSPrompt.ObjectId, OpenMode.ForWrite);
        //            // If a "lightweight" (or optimized) polyline
        //            Polyline lwp = obj as Polyline;
        //            lwp.Layer = "CALLOUT BORDER";

        //            Polyline myBorderClone = lwp.Clone() as Polyline;
        //            myBorderClone.Layer = "CALLOUT BORDER";

        //            if (acSSPrompt != null)
        //            {
        //                // Use a for loop to get each vertex, one by one

        //                int vn = lwp.NumberOfVertices;

        //                for (int i = 0; i < vn; i++)
        //                {
        //                    // Could also get the 3D point here
        //                    Point2d pt = lwp.GetPoint2dAt(i);
        //                    Point3d myPtconvert = new Point3d(pt.X, pt.Y, 0);
        //                    pntCol.Add(myPtconvert);
        //                }

        //                acCurDoc.Editor.WriteMessage("so diem trong pnt col: " + pntCol.Count.ToString());
        //                /// Draw a poly from pntCol
        //                /// 



        //                using (Polyline acPoly = new Polyline())
        //                {
        //                    int iPoint = 0;
        //                    foreach (Point3d myPt3d in pntCol)
        //                    {
        //                        acPoly.AddVertexAt(iPoint, new Point2d(myPt3d.X, myPt3d.Y), 0, 0, 0);
        //                        iPoint++;
        //                    }

        //                    acPoly.Closed = true;
        //                    // Add the new object to the block table record and the transaction
        //                    acBlkTblRec.AppendEntity(acPoly);
        //                    acTrans.AddNewlyCreatedDBObject(acPoly, true);
        //                }


        //            }
        //            // Lay diem giua cua polyline
        //            int myIndex = Convert.ToInt32(lwp.NumberOfVertices / 2);

        //            Point2d pt1 = lwp.GetPoint2dAt(0);
        //            Point2d pt2 = lwp.GetPoint2dAt(myIndex);

        //            Point3d centerPoint = new Point3d((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2, 0);

        //            acCurDoc.Editor.WriteMessage("\n Cetern Point: {0} ", centerPoint.ToString());
        //            TypedValue[] acTypValAr = new TypedValue[1];

        //            //select circle and line

        //            acTypValAr[0] = new TypedValue(0, "CIRCLE,LINE,POLYLINE,SPLINE,RAY,ARC,HATCH,ELLIPSE,LWPOLYLINE,MLINE");

        //            //8 = DxfCode.LayerName

        //            // Assign the filter criteria to a SelectionFilter object
        //            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);



        //            // Selet all object cross polygon
        //            PromptSelectionResult pmtSelRes = null;
        //            pmtSelRes = acEd.SelectCrossingPolygon(pntCol, acSelFtr);



        //            //acCurDoc.Editor.WriteMessage("Number to clone:{0} ", pmtSelRes.Value.ToString());

        //            List<Entity> myListEntityClone = new List<Entity>();
        //            if (pmtSelRes.Status == PromptStatus.OK)
        //            {
        //                myListEntityClone.Add(myBorderClone);

        //                foreach (ObjectId objId in pmtSelRes.Value.GetObjectIds())
        //                {


        //                    //acEd.WriteMessage("Entities found:{0} \n", objId.GetObject(OpenMode.ForWrite).GetRXClass().DxfName);
        //                    //// tao clone va them vao listclone

        //                    //List<string> myEntityList = new List<string> {"CIRCLE", "LINE", "ELLIPSE",
        //                    //    "HATCH","ELLIPSE","ARC", "SPLINE","POLYLINE", "RAY",
        //                    //    "LWPOLYLINE","MLINE"};
        //                    //string className = objId.GetObject(OpenMode.ForRead).GetRXClass().DxfName;
        //                    //if (className.Contains(className))
        //                    //{
        //                    //}
        //                    Entity myCloneEnt = objId.GetObject(OpenMode.ForWrite).Clone() as Entity;
        //                    myListEntityClone.Add(myCloneEnt);
        //                }
        //                acCurDoc.Editor.WriteMessage("Number of clone:{0} ", myListEntityClone.Count);
        //            }

        //            // CHonj 1 diem tren man hinh de pick insert
        //            PromptPointResult pPtRes;
        //            PromptPointOptions pPtOpts = new PromptPointOptions("");

        //            // Prompt for the start point
        //            pPtOpts.Message = "\nPick a point to place CallOut: ";
        //            pPtRes = acCurDoc.Editor.GetPoint(pPtOpts);
        //            Point3d ptPositionInsert = pPtRes.Value;

        //            // Exit if the user presses ESC or cancels the command
        //            if (pPtRes.Status == PromptStatus.Cancel) return;

        //            Vector3d myVectorMove = centerPoint.GetVectorTo(ptPositionInsert);

        //            foreach (Entity ent in myListEntityClone)
        //            {
        //                ent.TransformBy(Matrix3d.Displacement(myVectorMove));


        //                // Add the new object to the block table record and the transaction
        //                acBlkTblRec.AppendEntity(ent);
        //                acTrans.AddNewlyCreatedDBObject(ent, true);
        //            }

        //            // Extrim Command

        //            Point3d pickPoint = new Point3d(ptPositionInsert.X + 100000, ptPositionInsert.Y + 100000, 0);

        //            Point2d pp1 = myBorderClone.GetPoint2dAt(0);
        //            Point2d pp2 = myBorderClone.GetPoint2dAt(1);
        //            Point3d pickPt3d = new Point3d((pp1.X + pp2.X) / 2, (pp1.Y + pp2.Y) / 2, 0);

        //            string pickPt = pp1.ToString().Trim('(', ')');
        //            string sidePt = pickPoint.ToString().Trim('(', ')');

        //            string mycom = "_extrim " + pickPt + " " + sidePt + " ";
        //            //doc.SendStringToExecute($"_extrim {pickPt} {sidePt} ", false, false, false);

        //            Application.SetSystemVariable("PICKSTYLE", currentPickStyle);

        //            acTrans.Commit();

        //            acCurDoc.Editor.Command(myBorderClone, pickPoint, "");

        //            //acCurDoc.SendStringToExecute(mycom, true, false, false);
        //            acCurDoc.SendStringToExecute("RE\n", true, false, false);


        //            acCurDoc.SendStringToExecute("CRT\n", true, false, false);

        //            //acCurDoc.SendStringToExecute(mycom, true, false, false);
        //            //acCurDoc.SendStringToExecute("CRT", true, false, false);

        //            // SetdimCurrent

        //        }
        //    }
        //    catch (Autodesk.AutoCAD.Runtime.Exception ex)
        //    {
        //        Application.ShowAlertDialog(ex.Message);
        //    }



        //}


        // NHÓM HÀM TẠO BLOCK

        [CommandMethod("CB")]
        public void CreatingABlock()
        {

            // Get the current database and start the Transaction Manager
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;


            double currentScale = 1;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Check the block name, to see whether it's
                // already in use
                PromptStringOptions pso = new PromptStringOptions("\nEnter new block name: ");

                pso.AllowSpaces = true;
                // A variable for the block's name
                string blkName = "";
                do
                {
                    PromptResult pr = acDoc.Editor.GetString(pso);
                    // Just return if the user cancelled
                    // (will abort the transaction as we drop out of the using
                    // statement's scope)

                    if (pr.Status != PromptStatus.OK)

                        return;
                    try
                    {
                        // Validate the provided symbol table name

                        SymbolUtilityServices.ValidateSymbolName(pr.StringResult, false);
                        // Only set the block name if it isn't in use
                        if (acBlkTbl.Has(pr.StringResult))
                            acDoc.Editor.WriteMessage("\nA block with this name already exists.");
                        else

                            blkName = pr.StringResult;
                    }
                    catch
                    {
                        // An exception has been thrown, indicating the
                        // name is invalid

                        acDoc.Editor.WriteMessage("\nInvalid block name.");
                    }
                } while (blkName == "");

                ObjectId blkRecId = ObjectId.Null;

                if (!acBlkTbl.Has(blkName))
                {
                    using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                    {
                        acBlkTblRec.Name = blkName;

                        // Select objects to make block

                        // Request for objects to be selected in the drawing area
                        PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection();


                        // If the prompt status is OK, objects were selected
                        if (acSSPrompt.Status == PromptStatus.OK)
                        {
                            SelectionSet acSSet = acSSPrompt.Value;

                            // Step through the objects in the selection set
                            foreach (SelectedObject acSSObj in acSSet)
                            {

                                // Check to make sure a valid SelectedObject object was returned
                                if (acSSObj != null)
                                {

                                    if (acSSObj.ObjectId.ObjectClass.DxfName == "DIMENSION")
                                    {
                                        Dimension originalDim = acTrans.GetObject(acSSObj.ObjectId,
                                                                        OpenMode.ForWrite) as Dimension;

                                        currentScale = originalDim.GetDimstyleData().Dimscale;

                                    }


                                    // Open the selected object for write
                                    Entity acEnt = acTrans.GetObject(acSSObj.ObjectId,
                                                                        OpenMode.ForWrite) as Entity;

                                    if (acEnt != null)
                                    {
                                        Entity acEntClone = acEnt.Clone() as Entity;
                                        acBlkTblRec.AppendEntity(acEntClone);
                                    }
                                }
                            }
                        }
                        // Set point origin block
                        PromptPointResult pPtRes;
                        PromptPointOptions pPtOpts = new PromptPointOptions("");

                        // Prompt for the start point
                        pPtOpts.Message = "\nPick origin block: ";
                        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                        Point3d ptOrigin = pPtRes.Value;

                        // Exit if the user presses ESC or cancels the command
                        if (pPtRes.Status == PromptStatus.Cancel) return;


                        // Set the insertion point for the block
                        acBlkTblRec.Origin = ptOrigin;


                        // Insert vao csdl
                        acBlkTbl.UpgradeOpen();
                        acBlkTbl.Add(acBlkTblRec);
                        acTrans.AddNewlyCreatedDBObject(acBlkTblRec, true);

                        blkRecId = acBlkTblRec.Id;
                    }
                }

                else
                {
                    blkRecId = acBlkTbl[blkName];
                }

                if (currentScale == 1)
                {
                    PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
                    pIntOpts.Message = "\nEnter Scale Original factor: ";
                    //pIntOpts.DefaultValue = 18;


                    PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);
                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    currentScale = pIntRes.Value;
                }

                else
                {
                    acDoc.Editor.WriteMessage("\nCurrent dim scale is: {0}", currentScale);
                }

                // Insert the block into the current space
                if (blkRecId != ObjectId.Null)
                {

                    PromptPointResult pPtRes;
                    PromptPointOptions pPtOpts = new PromptPointOptions("");

                    // Prompt for the start point
                    pPtOpts.Message = "\nPick a point to place block: ";
                    pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                    Point3d ptPositionInsert = pPtRes.Value;

                    // Exit if the user presses ESC or cancels the command
                    if (pPtRes.Status == PromptStatus.Cancel) return;

                    using (BlockReference acBlkRef = new BlockReference(ptPositionInsert, blkRecId))
                    {
                        PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
                        pIntOpts.Message = "\nEnter CallOut Scale: ";
                        //pIntOpts.DefaultValue = 18;


                        PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);

                        // Restrict input to positive and non-negative values
                        pIntOpts.AllowZero = false;
                        pIntOpts.AllowNegative = false;

                        //Set thickness from input 
                        double scaleCallOutFactor = pIntRes.Value;


                        //BlockReference acBlkRefScale = acBlkRef.ScaleFactors
                        acBlkRef.TransformBy(Matrix3d.Scaling(scaleCallOutFactor / currentScale, ptPositionInsert));

                        BlockTableRecord acCurSpaceBlkTblRec;
                        acCurSpaceBlkTblRec = acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                        acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                        acTrans.AddNewlyCreatedDBObject(acBlkRef, true);


                        // Create a NameText

                        using (MText acMText = new MText())
                        {
                            acMText.Location = new Point3d(ptPositionInsert.X, ptPositionInsert.Y - 10 * scaleCallOutFactor, 0);
                            //acMText.Width = 4;

                            acMText.TextHeight = 3.5 * scaleCallOutFactor;
                            acMText.Contents = blkName + "\nTL 1:" + scaleCallOutFactor;

                            acCurSpaceBlkTblRec.AppendEntity(acMText);
                            acTrans.AddNewlyCreatedDBObject(acMText, true);
                        }


                    }
                }
                // Save the new object to the database
                acTrans.Commit();

                // Dispose of the transaction
            }
        }

    }
}
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



using System.Collections;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Application;


namespace CurveMod
{

    public static class Extensions
    {

        [CommandMethod("findIntersect")]
        public static void CmdFindIntersect()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ObjectId lineId = ed.GetEntity("Select line: ").ObjectId; // not safe, test only
            ObjectId circleId = ed.GetEntity("Select circle: ").ObjectId; // not safe, test only

            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Line l = trans.GetObject(lineId, OpenMode.ForRead) as Line;
                Circle c = trans.GetObject(circleId, OpenMode.ForRead) as Circle;

                Point3dCollection intersectionPoints = new Point3dCollection();
                l.IntersectWith(c, Intersect.OnBothOperands, intersectionPoints, IntPtr.Zero, IntPtr.Zero);
                
                trans.Commit();

                ed.WriteMessage("{0} intersection(s) found", intersectionPoints.Count);
            }
        }



        [CommandMethod("FLR")]
        public static void AddLightweightPolyline()
        {
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
                    acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                    OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                    OpenMode.ForWrite) as BlockTableRecord;


                    // kiem tra hinh window select hop le hay khong.
                    if (pt1.X == pt2.X || pt1.Y == pt2.Y)
                    {
                        ed.WriteMessage("\nInvalid point specification");
                        return;
                    }

                    Point2d ptRec1 = new Point2d (pt1.X, pt1.Y);
                    Point2d ptRec2 = new Point2d (pt1.X, pt2.Y);
                    Point2d ptRec3 = new Point2d (pt2.X, pt2.Y);
                    Point2d ptRec4 = new Point2d (pt2.X, pt1.Y);

                    using (Polyline acPoly = new Polyline())
                    {
                        acPoly.AddVertexAt(0, ptRec1, 0, 0, 0);
                        acPoly.AddVertexAt(1, ptRec2, 0, 0, 0);
                        acPoly.AddVertexAt(2, ptRec3, 0, 0, 0);
                        acPoly.AddVertexAt(3, ptRec4, 0, 0, 0);
                        //acPoly.AddVertexAt(4, ptRec1, 0, 0, 0);
                        acPoly.Closed = true;

                        double radiusFillet = Math.Min(Math.Abs(ptRec1.X - ptRec3.X), Math.Abs(ptRec1.Y - ptRec3.Y)) / 2;


                        acPoly.FilletAll(radiusFillet);
                        // Add the new object to the block table record and the transaction
                        acBlkTblRec.AppendEntity(acPoly);
                        acTrans.AddNewlyCreatedDBObject(acPoly, true);
                    }

                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    acadApp.ShowAlertDialog(ex.Message);
                }

                acTrans.Commit();
            }
        }

        // Adds an arc (fillet) at each vertex, if able.
        public static void FilletAll(this Polyline pline, double radius)
        {
            int i = pline.Closed ? 0 : 1;
            for (int j = 0; j < pline.NumberOfVertices - i; j += 1 + pline.FilletAt(j, radius))
            { }
        }

        // Adds an arc (fillet) at the specified vertex. Returns 1 if the operation succeeded, 0 if it failed.
        public static int FilletAt(this Polyline pline, int index, double radius)
        {
            int prev = index == 0 && pline.Closed ? pline.NumberOfVertices - 1 : index - 1;
            if (pline.GetSegmentType(prev) != SegmentType.Line ||
                pline.GetSegmentType(index) != SegmentType.Line)
                return 0;
            LineSegment2d seg1 = pline.GetLineSegment2dAt(prev);
            LineSegment2d seg2 = pline.GetLineSegment2dAt(index);
            Vector2d vec1 = seg1.StartPoint - seg1.EndPoint;
            Vector2d vec2 = seg2.EndPoint - seg2.StartPoint;
            double angle = (Math.PI - vec1.GetAngleTo(vec2)) / 2.0;
            double dist = radius * Math.Tan(angle);
            if (dist > seg1.Length || dist > seg2.Length)
                return 0;
            Point2d pt1 = seg1.EndPoint + vec1.GetNormal() * dist;
            Point2d pt2 = seg2.StartPoint + vec2.GetNormal() * dist;
            double bulge = Math.Tan(angle / 2.0);
            if (Clockwise(seg1.StartPoint, seg1.EndPoint, seg2.EndPoint))
                bulge = -bulge;
            pline.AddVertexAt(index, pt1, bulge, 0.0, 0.0);
            pline.SetPointAt(index + 1, pt2);
            return 1;
        }

        // Evaluates if the points are clockwise.
        private static bool Clockwise(Point2d p1, Point2d p2, Point2d p3)
        {
            return ((p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X)) < 1e-8;
        }
    }

}
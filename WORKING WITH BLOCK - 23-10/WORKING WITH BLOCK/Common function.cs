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

using System.Threading;



namespace commonFunctions
{

    public static class AlgebraicArea
    {
        public static double GetArea(Point2d pt1, Point2d pt2, Point2d pt3)
        {
            return (((pt2.X - pt1.X) * (pt3.Y - pt1.Y)) -
                        ((pt3.X - pt1.X) * (pt2.Y - pt1.Y))) / 2.0;
        }

        public static double GetArea(this CircularArc2d arc)
        {
            double rad = arc.Radius;
            double ang = arc.IsClockWise ?
                arc.StartAngle - arc.EndAngle :
                arc.EndAngle - arc.StartAngle;
            return rad * rad * (ang - Math.Sin(ang)) / 2.0;
        }

        public static double GetArea(this Polyline pline)
        {
            CircularArc2d arc = new CircularArc2d();
            double area = 0.0;
            int last = pline.NumberOfVertices - 1;
            Point2d p0 = pline.GetPoint2dAt(0);

            if (pline.GetBulgeAt(0) != 0.0)
            {
                area += pline.GetArcSegment2dAt(0).GetArea();
            }
            for (int i = 1; i < last; i++)
            {
                area += GetArea(p0, pline.GetPoint2dAt(i), pline.GetPoint2dAt(i + 1));
                if (pline.GetBulgeAt(i) != 0.0)
                {
                    area += pline.GetArcSegment2dAt(i).GetArea(); ;
                }
            }
            if ((pline.GetBulgeAt(last) != 0.0) && pline.Closed)
            {
                area += pline.GetArcSegment2dAt(last).GetArea();
            }
            return area;
        }
    
    }


    public class myCustomFunctions
    {
        //HAM CHUNG:

        public static Object SelectPoint(Point3d lastPoint)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;


            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");
            pPtOpts.Message = "\nPick Point on Screen: ";
            pPtOpts.BasePoint = lastPoint;

            if (lastPoint == new Point3d(0,0,0))
            {
                pPtOpts.UseBasePoint = false;
            }
            else
            {
                pPtOpts.UseBasePoint = true;
            }
            pPtOpts.UseDashedLine = true;


            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            if (pPtRes.Status == PromptStatus.Cancel) return null;

            Point3d pickedPoint = pPtRes.Value;

            return pickedPoint;
        }

        public static ObjectId GetObjectIdByType(string typeObject)
        {
            //Database acCurDb = acDoc.Database;


            Document acDoc = Application.DocumentManager.MdiActiveDocument;


            // Create a TypedValue array to define the filter criteria
            TypedValue[] acTypValAr = new TypedValue[1];
            acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, typeObject), 0);

            // Assign the filter criteria to a SelectionFilter object
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            //PromptEntityOptions 
            PromptEntityOptions acPEO = new Autodesk.AutoCAD.EditorInput.PromptEntityOptions("");
            acPEO.Message = "Pick a " + typeObject + ": ";
            // Request for objects to be selected in the drawing area
            PromptEntityResult myAcPER = acDoc.Editor.GetEntity(acPEO);
                
            // If the prompt status is OK, objects were selected
            if (myAcPER.Status == PromptStatus.OK)
            {
                return myAcPER.ObjectId;
            }
            return new ObjectId();
        }


    }

}
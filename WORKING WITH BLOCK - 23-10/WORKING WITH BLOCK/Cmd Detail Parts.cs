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

        [CommandMethod("Test")]
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


        //HAM CHUNG:
        [CommandMethod("PP")]
        public static void abc()
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
            Database acCurDb = acDoc.Database;

            // Start a transaction
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

                double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;


                ObjectId myObjId = myCustomFunctions.GetObjectIdByType("POLYLINE,LWPOLYLINE");

                if (myObjId.ToString() == "0") return ;

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
                Point3d ptPositionInsert = pPtRes.Value;

                // Exit if the user presses ESC or cancels the command
                if (pPtRes.Status == PromptStatus.Cancel) return;

                // vector move tu tam duong tron goc tới diểm chọn
                Vector3d myVectorMove = myBasePoint.GetVectorTo(ptPositionInsert);

                myCloneSection.TransformBy(Matrix3d.Displacement(myVectorMove));

                double PolyArea = myCloneSection.GetArea();

                // if area <0 10*5
                double offsetDistance = 10 * scaleCurrentDim;



                if (PolyArea < 0)
                {
                    offsetDistance = offsetDistance * -1;
                }


                DBObjectCollection myObjectsOffset = myCloneSection.GetOffsetCurves(offsetDistance);
                foreach (Entity myObject in myObjectsOffset)
                {
                    acDoc.Editor.WriteMessage("\n{0} : la 1 ten class", myObject.GetRXClass().DxfName);

                    myObject.Layer = "CENTER";

                    acBlkTblRec.AppendEntity(myObject);
                    acTrans.AddNewlyCreatedDBObject(myObject, true);

                }


               acDoc.Editor.WriteMessage(" \nExtents----{0}:", myCloneSection.GeometricExtents.ToString());


                acBlkTblRec.AppendEntity(myCloneSection);
                acTrans.AddNewlyCreatedDBObject(myCloneSection, true);

                acTrans.Commit();
            }

        }

        static int sortByY(Point3d a, Point3d b)
        {
            if (a.Y == b.Y)
                return a.X.CompareTo(b.X);
            return a.Y.CompareTo(b.Y);
        }
    }
}

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

using commonFunctions;


namespace myCustomCmds
{
    public class   CmdSecCutting
    {

        // NHÓM HÀM TẠO MẶT CẮT GỖ
        //HAM CHUNG:
        public static void DrawMDFVarmm(double myInput)
        {
            if (!CheckLicense.licensed) return;


            // Get the current database and start the Transaction Manager
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = true;

            try
            {

            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");

            // Prompt for the start point
            pPtOpts.Message = "\nEnter the start point of the line: ";
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptStart = pPtRes.Value;

            // Exit if the user presses ESC or cancels the command
            if (pPtRes.Status == PromptStatus.Cancel) return;

            // Prompt for the end point
            pPtOpts.Message = "\nEnter the end point of the line: ";
            pPtOpts.UseBasePoint = true;
            pPtOpts.UseDashedLine = false;
            pPtOpts.BasePoint = ptStart;
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptEnd = pPtRes.Value;

            if (pPtRes.Status == PromptStatus.Cancel) return;


            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
            // Prompt for the end point
            pPtOpts.Message = "\nPick Side: ";
            pPtOpts.UseBasePoint = true;
            pPtOpts.UseDashedLine = true;
            pPtOpts.BasePoint = ptEnd;
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptSide = pPtRes.Value;

            if (pPtRes.Status == PromptStatus.Cancel) return;

            Line myPickLine = new Line(ptStart, ptEnd);
            
            // lay diem gan nhat nam tren line voi point pick

            int defaultThickness = Convert.ToInt32(ptSide.DistanceTo(myPickLine.GetClosestPointTo(ptSide, true)));
            

            //int thicknessPick = 

            bool sidePicked = isLeftOrAbove(ptStart, ptEnd, ptSide);



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


                // Define the new line
                using (Line acLine = new Line(ptStart, ptEnd))
                {

                    double thickness = myInput;

                    if (thickness == 0)
                    {
                        PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                        pIntOpts.Message = "\nEnter thickness: ";
                        pIntOpts.DefaultValue = defaultThickness;



                        PromptIntegerResult pIntRes = acDoc.Editor.GetInteger(pIntOpts);

                        // Restrict input to positive and non-negative values
                        pIntOpts.AllowZero = false;
                        pIntOpts.AllowNegative = false;

                        //Set thickness from input 
                        thickness = pIntRes.Value;
                    }

                    DBObjectCollection acDbObjColl = acLine.GetOffsetCurves(thickness);

                    if (!sidePicked)
                    {
                        acDbObjColl = acLine.GetOffsetCurves(-1*thickness);
                    }


                    Point2d P1L1 = acLine.StartPoint.Convert2d(new Plane());
                    Point2d P2L1 = acLine.EndPoint.Convert2d(new Plane());

                    Point2d P1L2 = new Point2d();
                    Point2d P2L2 = new Point2d();

                    foreach (Entity acEnt in acDbObjColl)
                    {
                        // Add each offset object
                        //acBlkTblRec.AppendEntity(acEnt);
                        //acTrans.AddNewlyCreatedDBObject(acEnt, true);

                        if (acEnt is Line)
                        {
                            Line acEnt2Line = acEnt as Line;
                            P1L2 = acEnt2Line.StartPoint.Convert2d(new Plane());
                            P2L2 = acEnt2Line.EndPoint.Convert2d(new Plane());
                        }
                    }
                    Polyline acPolyLine = new Polyline();

                    acPolyLine.AddVertexAt(0, P1L1, 0, 0, 0);
                    acPolyLine.AddVertexAt(0, P2L1, 0, 0, 0);
                    acPolyLine.AddVertexAt(0, P2L2, 0, 0, 0);
                    acPolyLine.AddVertexAt(0, P1L2, 0, 0, 0);
                    //acPolyLine.AddVertexAt(0, P1L1, 0, 0, 0);

                    acPolyLine.Closed = true;

                    //Create layer Dim
                    //CmdLayer.createALayerByName("WOOD BOUNDARY");

                    //acPolyLine.Layer = "WOOD BOUNDARY";
                    acBlkTblRec.AppendEntity(acPolyLine);
                    acTrans.AddNewlyCreatedDBObject(acPolyLine, true);
                    // Adds the circle to an object id array
                    ObjectIdCollection acObjIdColl = new ObjectIdCollection();
                    acObjIdColl.Add(acPolyLine.ObjectId);

                    // Create the hatch object and append it to the block table record
                    using (Hatch acHatch = new Hatch())
                    {
                        acBlkTblRec.AppendEntity(acHatch);
                        acTrans.AddNewlyCreatedDBObject(acHatch, true);

                        // Set the properties of the hatch object
                        // Associative must be set after the hatch object is appended to the 
                        // block table record and before AppendLoop

                        acHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI36");
                        acHatch.Associative = true;
                        acHatch.PatternScale = 2;
                        acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                        acHatch.EvaluateHatch(true);
                        //Create layer Hatch
                        //CmdLayer.createALayerByName("HATCH");
                        //acHatch.Layer = "HATCH";
                    }
                }
                // Commit the changes and dispose of the transaction
                acTrans.Commit();

            }
        }

            catch (System.Exception ex)
            {
                acDoc.Editor.WriteMessage(ex.Message + "\n" + ex.StackTrace);
            }
        }



        [CommandMethod("GV")]
        public static void DrawMDFInput()
        {
            DrawMDFVarmm(0);
        }


        [CommandMethod("G1")]
        public static void DrawMDF5mm()
        {
            DrawMDFVarmm(5);
        }

        [CommandMethod("G2")]
        public static void DrawMDF9mm()
        {
            DrawMDFVarmm(9);
        }


        [CommandMethod("G3")]
        public static void DrawMDF18mm()
        {
            DrawMDFVarmm(18);
        }


        [CommandMethod("G4")]
        public static void DrawMDF21mm()
        {
            DrawMDFVarmm(21);
        }


        [CommandMethod("G5")]
        public static void DrawMDF25mm()
        {
            DrawMDFVarmm(21);
        }



        private static bool isLeftOrAbove(Point3d POL1, Point3d POL2, Point3d PO)
        {
            return ((POL2.X - POL1.X) * (PO.Y - POL1.Y) - (POL2.Y - POL1.Y) * (PO.X - POL1.X)) > 0;
        }


    }
}
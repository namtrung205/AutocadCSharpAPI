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

using commonFunctions;

namespace myCustomCmds
{
    public class CmdDim
    {
        //NHÓM HÀM DIMSYLE
        public static void ChangeDimStyle(string nameDimStyle)
        {

            // Get the current document and database
            Document acCurDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acCurDoc.Database;

            using (Transaction trans = acCurDb.TransactionManager.StartTransaction())
            {
                DimStyleTable DimTabb = (DimStyleTable)trans.GetObject(acCurDb.DimStyleTableId, OpenMode.ForRead);
                ObjectId dimId = ObjectId.Null;

                if (!DimTabb.Has(nameDimStyle))
                {
                    string warning = "\nKhong ton tai dimstyle co ten la: " + nameDimStyle;
                    acCurDoc.Editor.WriteMessage(warning);
                    Application.ShowAlertDialog(warning);
                }
                else
                {
                    dimId = DimTabb[nameDimStyle];

                    DimStyleTableRecord DimTabbRecord = (DimStyleTableRecord)trans.GetObject(dimId, OpenMode.ForRead);

                    if (DimTabbRecord.ObjectId != acCurDb.Dimstyle)
                    {
                        acCurDb.Dimstyle = DimTabbRecord.ObjectId;
                        acCurDb.SetDimstyleData(DimTabbRecord);
                    }
                }

                trans.Commit();
                acCurDoc.Editor.WriteMessage("\nDimstyle " + nameDimStyle + " has been set to current.");
            }
        }
        [CommandMethod("CDS")]
        public static void ChangeDimStyleByScale()
        {
            try
            {
                CmdLayer.createALayerByName("DIM");

                // Get the current document and database
                Document acCurDoc = Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acCurDoc.Database;

                /// Scale
                PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
                pIntOpts.Message = "\nEnter Scale factor: ";
                pIntOpts.DefaultValue = 1;

                PromptDoubleResult pIntRes = acCurDoc.Editor.GetDouble(pIntOpts);
                pIntOpts.AllowZero = false;
                pIntOpts.AllowNegative = false;

                if (pIntRes.Value == null) return;

                double scaleFactorCallout = pIntRes.Value;

                string nameDimStyle = "1-" + scaleFactorCallout;

                if (scaleFactorCallout < 1)
                {
                    nameDimStyle = (Convert.ToInt32(1 / scaleFactorCallout)).ToString() + "-1";
                }

                ChangeDimStyle(nameDimStyle);

            }
            catch
            {
                return;
            }
           
        
        }

        [CommandMethod("CDO")]
        public static void ChangeDimStyleByObject()
        {

            try
            {
                CmdLayer.createALayerByName("DIM");

                // Get the current document and database
                Document acCurDoc = Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acCurDoc.Database;


                // Chon 1 dim object
                //Sau do lay thong tin tu dimobject 
                // set dimcurrent giong dimstyle cua object

                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    ObjectId myObjId = myCustomFunctions.GetObjectIdByType("Dimension");
                    //ObjectId myObjId = myCustomFunctions.GetObjectIdByType("ABC,AlignedDimension,ArcDimension,DiametricDimension,LineAngularDimension2,Point3AngularDimension,RadialDimension,RadialDimensionLarge,RotatedDimension");
                    if (myObjId.ToString() == "0") return;
                    if (myObjId == new ObjectId()) return;

                    Dimension myDimSelected = myObjId.GetObject(OpenMode.ForRead) as Dimension;
                    string nameOfDimSelected = myDimSelected.DimensionStyleName;

                    ChangeDimStyle(nameOfDimSelected);
                    acTrans.Commit();
                }

            }
            catch
            {
                return;
            }

        }


        public static void  NewDimStyle(string nameDimSyle,double dimScale)
        {
            Document acCurDoc = Application.DocumentManager.MdiActiveDocument;


            Database acCurDb = Application.DocumentManager.MdiActiveDocument.Database;

            using (Transaction trans = acCurDb.TransactionManager.StartTransaction())
            {
                DimStyleTable DimTabb = (DimStyleTable)trans.GetObject(acCurDb.DimStyleTableId, OpenMode.ForWrite);
                ObjectId dimId = ObjectId.Null;

                foreach (ObjectId style in DimTabb)
                {
                    DimStyleTableRecord myDimstyle =  style.GetObject(OpenMode.ForWrite) as DimStyleTableRecord;
                    acCurDoc.Editor.WriteMessage("namestyle: {0}\n", myDimstyle.Name);
                }

                if (!DimTabb.Has(nameDimSyle))
                {
                    DimTabb.UpgradeOpen();
                    DimStyleTableRecord newRecord = acCurDb.Dimstyle.GetObject(OpenMode.ForRead) as DimStyleTableRecord;
                    newRecord.Name = nameDimSyle;
                    dimId = DimTabb.Add(newRecord);

                    newRecord.Dimscale = dimScale;

                    trans.AddNewlyCreatedDBObject(newRecord, true);

                }
                else
                {
                    dimId = DimTabb[nameDimSyle];
                   DimStyleTableRecord myNewDim = dimId.GetObject(OpenMode.ForWrite) as DimStyleTableRecord;
                   myNewDim.Dimscale = dimScale;
                }
                DimStyleTableRecord DimTabbRecord = (DimStyleTableRecord)trans.GetObject(dimId, OpenMode.ForWrite);
                if (DimTabbRecord.ObjectId != acCurDb.Dimstyle)
                {
                    acCurDb.Dimstyle = DimTabbRecord.ObjectId;
                    acCurDb.SetDimstyleData(DimTabbRecord);
                }

                trans.Commit();
            }
        }


        /// NHÓM HÀM DIMENSION
        [CommandMethod("DX")]
        public static void DLICustom()
        {
            try
            {
                //SetLayerCurrent("DIM");
                CmdLayer.createALayerByName("DIM");

                Document acDoc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock docLock = acDoc.LockDocument())
                {

                    Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
                    Database acCurDb = acDoc.Database;
                    // Draws a circle and zooms to the extents or 
                    // limits of the drawing


                    PromptPointResult pPtRes;
                    PromptPointOptions pPtOpts = new PromptPointOptions("");

                    // Prompt for the start point
                    pPtOpts.Message = "\nEnter the first point of the line: ";
                    pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                    Point3d ptStart = pPtRes.Value;

                    // Exit if the user presses ESC or cancels the command
                    if (pPtRes.Status == PromptStatus.Cancel) return;

                    // Prompt for the end point
                    pPtOpts.Message = "\nEnter the end point of the line: ";
                    pPtOpts.UseBasePoint = true;
                    pPtOpts.BasePoint = ptStart;
                    pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                    Point3d ptEnd = pPtRes.Value;

                    if (pPtRes.Status == PromptStatus.Cancel) return;


                    // Prompt for the end point
                    pPtOpts.Message = "\nEnter position dim: ";

                    Point3d ptMid = new Point3d((ptStart.X + ptEnd.X) / 2, (ptStart.Y + ptEnd.Y) / 2, (ptStart.Z + ptEnd.Z) / 2);
                    //pPtOpts.UseBasePoint = true;
                    //pPtOpts.BasePoint = ptMid;
                    //pPtOpts.UseDashedLine = true;
                    pPtRes = acDoc.Editor.GetPoint(pPtOpts);
                    Point3d ptDim = pPtRes.Value;
                    if (pPtRes.Status == PromptStatus.Cancel) return;


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


                        double myRotation = Math.Atan2(ptStart.Y - ptEnd.Y, ptStart.X - ptEnd.X);

                        //var dim = new RotatedDimension(ptStart, ptEnd, ptDim, null, acCurDb.Dimstyle);
                        RotatedDimension dim = new RotatedDimension(myRotation, ptStart, ptEnd, ptDim, null, acCurDb.Dimstyle);
                        //dim.XLine1Point = ptStart;


                        dim.Layer = "DIM";

                        // Add the new object to Model space and the transaction
                        acBlkTblRec.AppendEntity(dim);
                        acTrans.AddNewlyCreatedDBObject(dim, true);

                        // Commit the changes and dispose of the transaction
                        acTrans.Commit();
                    }
                }
            }
            catch
            {
                return;
            }
            
        }


        /// NHÓM HÀM DIMENSION
        //[CommandMethod("DX")]
        //public static void DLICustom()
        //{
        //    //SetLayerCurrent("DIM");
        //    CmdLayer.createALayerByName("DIM");

        //    Document acDoc = Application.DocumentManager.MdiActiveDocument;

        //    Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
        //    Database acCurDb = acDoc.Database;
        //    // Draws a circle and zooms to the extents or 
        //    // limits of the drawing


        //    PromptPointResult pPtRes;
        //    PromptPointOptions pPtOpts = new PromptPointOptions("");

        //    // Prompt for the start point
        //    pPtOpts.Message = "\nEnter the first point of the line: ";
        //    pPtRes = acDoc.Editor.GetPoint(pPtOpts);
        //    Point3d ptStart = pPtRes.Value;

        //    // Exit if the user presses ESC or cancels the command
        //    if (pPtRes.Status == PromptStatus.Cancel) return;

        //    // Prompt for the end point
        //    pPtOpts.Message = "\nEnter the end point of the line: ";
        //    pPtOpts.UseBasePoint = true;
        //    pPtOpts.BasePoint = ptStart;
        //    pPtRes = acDoc.Editor.GetPoint(pPtOpts);
        //    Point3d ptEnd = pPtRes.Value;

        //    if (pPtRes.Status == PromptStatus.Cancel) return;


        //    // Prompt for the end point
        //    pPtOpts.Message = "\nEnter position dim: ";

        //    Point3d ptMid = new Point3d((ptStart.X + ptEnd.X) / 2, (ptStart.Y + ptEnd.Y) / 2, (ptStart.Z + ptEnd.Z) / 2);
        //    //pPtOpts.UseBasePoint = true;
        //    //pPtOpts.BasePoint = ptMid;
        //    //pPtOpts.UseDashedLine = true;
        //    pPtRes = acDoc.Editor.GetPoint(pPtOpts);
        //    Point3d ptDim = pPtRes.Value;
        //    if (pPtRes.Status == PromptStatus.Cancel) return;


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


        //        double myRotation = Math.Atan2(ptStart.Y - ptEnd.Y, ptStart.X - ptEnd.X);

        //        //var dim = new RotatedDimension(ptStart, ptEnd, ptDim, null, acCurDb.Dimstyle);
        //        RotatedDimension dim = new RotatedDimension(myRotation, ptStart, ptEnd, ptDim, null, acCurDb.Dimstyle);
        //        //dim.XLine1Point = ptStart;


        //        dim.Layer = "DIM";

        //        // Add the new object to Model space and the transaction
        //        acBlkTblRec.AppendEntity(dim);
        //        acTrans.AddNewlyCreatedDBObject(dim, true);

        //        // Commit the changes and dispose of the transaction
        //        acTrans.Commit();
        //    }
        //}


        [CommandMethod("D1")]
        public static void mergeDim()
        {
            try
            {
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
                                // Lay dimline point
                                Point3d myTempDimLinePoint = myDim.DimLinePoint;
                                myDimLinePoint = myTempDimLinePoint;

                                // Lay goc xoay
                                double myTempRotation = myDim.Rotation;
                                myRotation = myTempRotation;


                                Point3d myPoint1 = myDim.XLine1Point;
                                Point3d myPoint2 = myDim.XLine2Point;

                                myListXlinePoint.Add(myPoint1);
                                myListXlinePoint.Add(myPoint2);

                                acDoc.Editor.WriteMessage("This is a dim linear: {0}\n", acSSObj.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name);

                                acDoc.Editor.WriteMessage("Success convert");

                                // Delete Dim
                                acSSObj.ObjectId.GetObject(OpenMode.ForWrite).Erase();
                                acDoc.Editor.WriteMessage("Success erase");
                            }

                            else if (typeDimName == "AcDbAlignedDimension")
                            {
                                //acDoc.Editor.WriteMessage("This is a dim linear: {0}\n", acSSObj.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name);

                                AlignedDimension myDim = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as AlignedDimension;
                                // Lay dimline point
                                Point3d myTempDimLinePoint = myDim.DimLinePoint;
                                myDimLinePoint = myTempDimLinePoint;


                                Point3d myPoint1 = myDim.XLine1Point;
                                Point3d myPoint2 = myDim.XLine2Point;

                                // Lay goc xoay
                                double myTempRotation = myDim.HorizontalRotation;
                                myTempRotation = Math.Atan2(myPoint1.Y - myPoint2.Y, myPoint1.X - myPoint2.X);
                                myRotation = myTempRotation;

                                myListXlinePoint.Add(myPoint1);
                                myListXlinePoint.Add(myPoint2);

                                acDoc.Editor.WriteMessage("This is a dim linear: {0}\n", acSSObj.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name);


                                // Delete Dim
                                acSSObj.ObjectId.GetObject(OpenMode.ForWrite).Erase();
                                acDoc.Editor.WriteMessage("Success erase");
                            }

                            //acDoc.Editor.WriteMessage("typedim: {0}\n", acSSObj.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name);
                        }

                        //// SortList

                        if (myRotation < 0.01)
                        {
                            myListXlinePoint.Sort(sortByX);
                        }
                        else if (myRotation - Math.PI / 2 < 0.01)
                        {
                            myListXlinePoint.Sort(sortByY);
                        }
                        else
                        {
                            myListXlinePoint.Sort(sortByX);
                        }
                        // Create the rotated dimension
                        using (RotatedDimension acRotDim = new RotatedDimension())
                        {
                            acRotDim.XLine1Point = myListXlinePoint[0];
                            acRotDim.XLine2Point = myListXlinePoint[myListXlinePoint.Count - 1];

                            if (myRotation - Math.PI / 2 > 0.01 && myRotation < 0.01)
                            {
                                double myTempRotation2 = Math.Atan2(acRotDim.XLine1Point.Y - acRotDim.XLine2Point.Y, acRotDim.XLine1Point.X - acRotDim.XLine2Point.X);
                                myRotation = myTempRotation2;
                            }

                            acRotDim.Rotation = myRotation;


                            acDoc.Editor.WriteMessage("goc xoay dim: {0}", myRotation);
                            acRotDim.DimLinePoint = myDimLinePoint;
                            acRotDim.DimensionStyle = acCurDb.Dimstyle;
                            acRotDim.Layer = "DIM";

                            // Add the new object to Model space and the transaction
                            acBlkTblRec.AppendEntity(acRotDim);
                            acTrans.AddNewlyCreatedDBObject(acRotDim, true);
                        }

                        // Save the new object to the database
                        acTrans.Commit();
                    }

                    // Dispose of the transaction
                }

            }
            catch
            {
                return;
            }

        }
        // Merge and split dim


        [CommandMethod("D2")]
        public static void splitDim()
        {
            try
            {
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


            catch { return; }
           
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
        public void DimOrthoAuto(string myInput)
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


        [CommandMethod("DD", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public void DimOrthoAuto2()
        {
            try
            {
                DimOrthoAuto("O");
            }
            catch
            {
                return;
            }

        }


        /// <summary>
        /// New Commnads 
        /// </summary>
        /// <param name="myInput"></param>
        public void DimOrthoAutoHole(string myInput)
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

                //list sort X = listSort by X
                myListPoint2.Sort(sortByX);
                List<Point3d> sortListX = myListPoint2;
                myListPoint2.Sort(sortByY);
                List<Point3d> sortListY = myListPoint2;



                List<Point3d> myListPointAndCenter = new List<Point3d>();
                List<double> listXCenter2 = new List<double>();
                List<double> listYCenter2 = new List<double>();

                foreach (var objID3 in sset2.GetObjectIds())
                {
                    string className = objID3.ObjectClass.DxfName;
                    //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n type: {0}\n", myListPoint2.Count);
                    if (className == "CIRCLE")
                    {
                        // Convert objID2 to Point3D
                        Circle myCirleHole = (Circle)objID3.GetObject(OpenMode.ForWrite);

                        //Add to lists
                        myListPointAndCenter.Add((Point3d)myCirleHole.Center);
                        listXCenter2.Add(myCirleHole.Center.X);
                        listYCenter2.Add(myCirleHole.Center.Y);
                    }

                    else if (className == "ELLIPSE")
                    {
                        
                        // Convert objID2 to Point3D
                        Ellipse myEllipseHole = (Ellipse)objID3.GetObject(OpenMode.ForWrite);

                        //Add to lists
                        myListPointAndCenter.Add((Point3d)myEllipseHole.Center);
                        listXCenter2.Add(myEllipseHole.Center.X);
                        listYCenter2.Add(myEllipseHole.Center.Y);
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

                if (listXCenter2.Count == 0) return;
                if (listYCenter2.Count == 0) return;

                //Them 4 diem toa do bien
                if (sortListX.Count > 1)
                {
                    listXCenter2.Add(sortListX[0].X);
                    listXCenter2.Add(sortListX[sortListX.Count - 1].X);
                }

                if (sortListY.Count > 1)
                {
                    listYCenter2.Add(sortListY[0].Y);
                    listYCenter2.Add(sortListY[sortListY.Count - 1].Y);
                }


                // KIEM TRA HUONG PICK
                if (ptDim.Y < listYCenter2.Min() || ptDim.Y > listYCenter2.Max())
                {
                    myInput = "H";
                }
                else if (ptDim.X < listXCenter2.Min() || ptDim.X > listXCenter2.Max())
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
                    if (sortListX.Count > 1)
                    {
                        myListPointAndCenter.Add(sortListX[0]);
                        myListPointAndCenter.Add(sortListX[myListPoint2.Count - 1]);
                    }

                    myListPointAndCenter.Sort(sortByX);
                    autoDimHorizontalNotSelect(myListPointAndCenter, ptDim);
                }
                else if (myInput == "V")
                {
                    if (sortListY.Count > 1)
                    {
                        myListPointAndCenter.Add(sortListY[0]);
                        myListPointAndCenter.Add(sortListY[myListPoint2.Count - 1]);
                    }


                    myListPointAndCenter.Sort(sortByY);
                    autoDimVerticalNotSelect(myListPointAndCenter, ptDim);
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
                        myListPointAndCenter.Sort(sortByX);
                        autoDimHorizontalNotSelect(myListPointAndCenter, ptDim);
                    }
                    else
                    {
                        myListPointAndCenter.Sort(sortByY);
                        autoDimVerticalNotSelect(myListPointAndCenter, ptDim);
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


        [CommandMethod("DH1", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public void DimOrthoAutoHole2()
        {
            try
            {
                DimOrthoAutoHole("O");
            }
            catch
            {

            }

        }



        public static void autoDimHorizontalNotSelect(List<Point3d> listPoint, Point3d dimPoint)
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


                // Tao dim tong:
                if (nDim > 1)
                {
                    using (RotatedDimension acRotDim = new RotatedDimension())
                    {
                        double dimScale = acCurDb.GetDimstyleData().Dimscale;
                        Point3d ptDimT = new Point3d();
                        if (ptDim.Y <= listPoint[0].Y)
                        {
                            ptDimT = new Point3d(ptDim.X, ptDim.Y - 5 * dimScale, ptDim.Z);
                        }
                        else
                        {
                            ptDimT = new Point3d(ptDim.X, ptDim.Y + 5 * dimScale, ptDim.Z);
                        }

                        acRotDim.XLine1Point = listPoint[0];
                        acRotDim.XLine2Point = listPoint[listPoint.Count - 1];
                        //acRotDim.Rotation = Math.PI / 2;
                        acRotDim.DimLinePoint = ptDimT;
                        acRotDim.DimensionStyle = acCurDb.Dimstyle;
                        acRotDim.Layer = "DIM";


                        // Add the new object to Model space and the transaction
                        if (Math.Abs(((Point3d)listPoint[0]).X - ((Point3d)listPoint[listPoint.Count - 1]).X) > 0.01)
                        {
                            acBlkTblRec.AppendEntity(acRotDim);
                            acTrans.AddNewlyCreatedDBObject(acRotDim, true);
                        }
                    }
                }


                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
        }


        public static void autoDimVerticalNotSelect(List<Point3d> listPoint, Point3d dimPoint)
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


                // Tao dim tong:

                if (nDim > 1)
                {
                    using (RotatedDimension acRotDim = new RotatedDimension())
                    {
                        double dimScale = acCurDb.GetDimstyleData().Dimscale;
                        Point3d ptDimT = new Point3d();
                        if (ptDim.X <= listPoint[0].X)
                        {
                            ptDimT = new Point3d(ptDim.X - 5 * dimScale, ptDim.Y, ptDim.Z);
                        }
                        else
                        {
                            ptDimT = new Point3d(ptDim.X + 5 * dimScale, ptDim.Y, ptDim.Z);
                        }

                        acRotDim.XLine1Point = listPoint[0];
                        acRotDim.XLine2Point = listPoint[listPoint.Count - 1];
                        acRotDim.Rotation = Math.PI / 2;
                        acRotDim.DimLinePoint = ptDimT;
                        acRotDim.DimensionStyle = acCurDb.Dimstyle;
                        acRotDim.Layer = "DIM";


                        // Add the new object to Model space and the transaction
                        if (Math.Abs(((Point3d)listPoint[0]).Y - ((Point3d)listPoint[listPoint.Count - 1]).Y) > 0.01)
                        {
                            acBlkTblRec.AppendEntity(acRotDim);
                            acTrans.AddNewlyCreatedDBObject(acRotDim, true);
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
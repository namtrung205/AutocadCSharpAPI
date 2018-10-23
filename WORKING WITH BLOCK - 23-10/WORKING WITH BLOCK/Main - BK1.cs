//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.EditorInput;
//using System.Collections.Generic;
//using System;

//using System.Runtime.InteropServices;
//using Autodesk.AutoCAD.Colors;
////using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

//using Autodesk.AutoCAD.Windows;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

///// This code requires a reference to AcExportLayoutEx.dll:
////using System.Threading;


////using Autodesk.AutoCAD.ExportLayout.Trimmer
//public class   WorkingWithBlock
//{
//    // NHOM HAM TRIMMER VA TAO CALLOUT

//    [CommandMethod("CCO", CommandFlags.Modal)]
//    public static void CreateCallOut()
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

//            Point3dCollection pntCol =new Point3dCollection();
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
//                    Point3d pt = lwp.GetPoint3dAt(i);
//                    pntCol.Add(pt);
//                }
//            }
//            // Lay diem giua cua polyline
//            int myIndex = Convert.ToInt32(lwp.NumberOfVertices / 2);

//            Point3d pt1 = lwp.GetPoint3dAt(0);
//            Point3d pt2 = lwp.GetPoint3dAt(myIndex);

//            Point3d centerPoint = new Point3d((pt1.X+pt2.X)/2, (pt1.Y+pt2.Y)/2, pt1.Z);


//            TypedValue[] acTypValAr = new TypedValue[1];

//            //select circle and line

//            acTypValAr[0] = new TypedValue(0, "CIRCLE,LINE,POLYLINE,SPLINE,RAY,ARC,HATCH,ELLIPSE,LWPOLYLINE,MLINE");

//            //8 = DxfCode.LayerName

//            // Assign the filter criteria to a SelectionFilter object
//            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);



//            // Selet all object cross polygon
//            PromptSelectionResult pmtSelRes = null;
//            pmtSelRes = acEd.SelectCrossingPolygon(pntCol,acSelFtr);

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
//                        Entity myCloneEnt = objId.GetObject(OpenMode.ForWrite).Clone() as Entity;
//                        myListEntityClone.Add(myCloneEnt);

//                }
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

//            Point3d pp1 = myBorderClone.GetPoint3dAt(0);
//            Point3d pp2 = myBorderClone.GetPoint3dAt(1);
//            Point3d pickPt3d = new Point3d((pp1.X + pp2.X) / 2, (pp1.Y + pp2.Y) / 2, 0);

//            string pickPt = myBorderClone.GetPoint3dAt(0).ToString().Trim('(', ')');
//            string sidePt = pickPoint.ToString().Trim('(', ')');

//            string mycom = "_extrim " + pickPt + " " + sidePt + " ";
//            //doc.SendStringToExecute($"_extrim {pickPt} {sidePt} ", false, false, false);

//            Application.SetSystemVariable("PICKSTYLE", currentPickStyle);

//            acTrans.Commit();
//            acCurDoc.SendStringToExecute(mycom, true, false, false);
//            acCurDoc.SendStringToExecute("RE\n", true, false, false);


//            acCurDoc.SendStringToExecute("CRT\n", true, false, false);

//            acCurDoc.SendStringToExecute(mycom, true, false, false);
//            //acCurDoc.SendStringToExecute("CRT", true, false, false);

//            // SetdimCurrent

//        }



//    }
//    // Insert Title

//    [CommandMethod("CRT")]
//    public static void CreateTitle()
//    {
//        // Get the current document and database
//        Document acCurDoc = Application.DocumentManager.MdiActiveDocument;
//        Database acCurDb = acCurDoc.Database;


//        /// Scale
//        PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
//        pIntOpts.Message = "\nEnter Scale factor: ";
//        pIntOpts.DefaultValue = 1;

//        PromptDoubleResult pIntRes = acCurDoc.Editor.GetDouble(pIntOpts);
//        pIntOpts.AllowZero = false;
//        pIntOpts.AllowNegative = false;

//        if (pIntRes.Value == null) return;

//        double scaleFactorCallout = pIntRes.Value;



//        // Create a title
//        PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Title Callout: ");
//        pStrOpts.AllowSpaces = true;
//        pStrOpts.DefaultValue = "Call Out";
//        PromptResult pStrRes = acCurDoc.Editor.GetString(pStrOpts);

//        if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


//        string myTitle = pStrRes.StringResult;

//        string myTitleText = "\\L" + myTitle.ToUpper() + "\\l" + "\nTL- 1: " + scaleFactorCallout;

//        if (scaleFactorCallout < 1)
//        {
//            int newScale = Convert.ToInt32( 1/scaleFactorCallout);
//            myTitleText = "\\L" + myTitle.ToUpper() + "\\l" + "\nTL- "+ newScale + ":1";
//        }

//        else
//        {
//            myTitleText = "\\L" + myTitle.ToUpper() + "\\l" + "\nTL- 1: " + scaleFactorCallout;
//        }




//        // Start a transaction
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

//            // Create a multiline text object

//            // Set point origin block
//            PromptPointResult pPtRes;
//            PromptPointOptions pPtOpts = new PromptPointOptions("");

//            // Prompt for the start point
//            pPtOpts.Message = "\nPick Title Text Place: ";
//            pPtRes = acCurDoc.Editor.GetPoint(pPtOpts);
//            Point3d ptOrigin = pPtRes.Value;

//            // Exit if the user presses ESC or cancels the command
//            if (pPtRes.Status == PromptStatus.Cancel) return;


//            using (MText acMText = new MText())
//            {
//                acMText.SetAttachmentMovingLocation(AttachmentPoint.MiddleCenter);
//                acMText.Location = ptOrigin;
//                acMText.Contents = myTitleText;
//                acMText.TextHeight = 3.5 * scaleFactorCallout;

//                acMText.Layer = "TITLE_BLOCK";

//                acBlkTblRec.AppendEntity(acMText);
//                acTrans.AddNewlyCreatedDBObject(acMText, true);
//            }
//            // Save the changes and dispose of the transaction
//            acTrans.Commit();

//            //Create Dim
//            string myNameDimStyle = "1-1";


//        if (scaleFactorCallout < 1)
//        {
//            int newScale = Convert.ToInt32( 1/scaleFactorCallout);
//            myNameDimStyle = newScale + "-1";
//        }

//        else
//        {
//            myNameDimStyle = "1-" + scaleFactorCallout;
//        }

//        ChanegDimStyle(myNameDimStyle);

//        }
//    }




//    // NHÓM HÀM TẠO BLOCK

//    [CommandMethod("CB")]
//    public void CreatingABlock()
//    {

//        // Get the current database and start the Transaction Manager
//        Document acDoc = Application.DocumentManager.MdiActiveDocument;
//        Database acCurDb = acDoc.Database;


//        double currentScale = 1;

//        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
//        {
//            // Open the Block table for read
//            BlockTable acBlkTbl;
//            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

//            // Check the block name, to see whether it's
//            // already in use
//            PromptStringOptions pso =new PromptStringOptions("\nEnter new block name: ");

//            pso.AllowSpaces = true;
//            // A variable for the block's name
//            string blkName = "";
//            do
//            {
//                PromptResult pr = acDoc.Editor.GetString(pso);
//                // Just return if the user cancelled
//                // (will abort the transaction as we drop out of the using
//                // statement's scope)

//                if (pr.Status != PromptStatus.OK)

//                    return;
//                try
//                {
//                    // Validate the provided symbol table name

//                    SymbolUtilityServices.ValidateSymbolName(pr.StringResult,false);
//                    // Only set the block name if it isn't in use
//                    if (acBlkTbl.Has(pr.StringResult))
//                        acDoc.Editor.WriteMessage("\nA block with this name already exists.");
//                    else

//                        blkName = pr.StringResult;
//                }
//                catch
//                {
//                    // An exception has been thrown, indicating the
//                    // name is invalid

//                    acDoc.Editor.WriteMessage("\nInvalid block name.");
//                }
//            } while (blkName == "");

//            ObjectId blkRecId = ObjectId.Null;

//            if (!acBlkTbl.Has(blkName))
//            {
//                using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
//                {
//                    acBlkTblRec.Name = blkName;

//                    // Select objects to make block

//                    // Request for objects to be selected in the drawing area
//                    PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection();


//                    // If the prompt status is OK, objects were selected
//                    if (acSSPrompt.Status == PromptStatus.OK)
//                    {
//                        SelectionSet acSSet = acSSPrompt.Value;

//                        // Step through the objects in the selection set
//                        foreach (SelectedObject acSSObj in acSSet)
//                        {

//                            // Check to make sure a valid SelectedObject object was returned
//                            if (acSSObj != null)
//                            {

//                                if (acSSObj.ObjectId.ObjectClass.DxfName == "DIMENSION")
//                                {
//                                    Dimension originalDim = acTrans.GetObject(acSSObj.ObjectId,
//                                                                    OpenMode.ForWrite) as Dimension;

//                                    currentScale = originalDim.GetDimstyleData().Dimscale;

//                                }


//                                // Open the selected object for write
//                                Entity acEnt = acTrans.GetObject(acSSObj.ObjectId,
//                                                                    OpenMode.ForWrite) as Entity;

//                                if (acEnt != null)
//                                {
//                                    Entity acEntClone = acEnt.Clone() as Entity;
//                                    acBlkTblRec.AppendEntity(acEntClone);
//                                }
//                            }
//                        }
//                    }
//                    // Set point origin block
//                    PromptPointResult pPtRes;
//                    PromptPointOptions pPtOpts = new PromptPointOptions("");

//                    // Prompt for the start point
//                    pPtOpts.Message = "\nPick origin block: ";
//                    pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//                    Point3d ptOrigin = pPtRes.Value;

//                    // Exit if the user presses ESC or cancels the command
//                    if (pPtRes.Status == PromptStatus.Cancel) return;


//                    // Set the insertion point for the block
//                    acBlkTblRec.Origin = ptOrigin;


//                    // Insert vao csdl
//                    acBlkTbl.UpgradeOpen();
//                    acBlkTbl.Add(acBlkTblRec);
//                    acTrans.AddNewlyCreatedDBObject(acBlkTblRec, true);

//                    blkRecId = acBlkTblRec.Id;
//                }
//            }

//            else
//            {
//                blkRecId = acBlkTbl[blkName];
//            }

//            if (currentScale == 1)
//            {
//                PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
//                pIntOpts.Message = "\nEnter Scale Original factor: ";
//                //pIntOpts.DefaultValue = 18;


//                PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);
//                pIntOpts.AllowZero = false;
//                pIntOpts.AllowNegative = false;

//                currentScale = pIntRes.Value;
//            }

//            else
//            {
//                acDoc.Editor.WriteMessage("\nCurrent dim scale is: {0}", currentScale);
//            }

//            // Insert the block into the current space
//            if (blkRecId != ObjectId.Null)
//            {

//                PromptPointResult pPtRes;
//                PromptPointOptions pPtOpts = new PromptPointOptions("");

//                // Prompt for the start point
//                pPtOpts.Message = "\nPick a point to place block: ";
//                pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//                Point3d ptPositionInsert = pPtRes.Value;

//                // Exit if the user presses ESC or cancels the command
//                if (pPtRes.Status == PromptStatus.Cancel) return;

//                using (BlockReference acBlkRef = new BlockReference(ptPositionInsert, blkRecId))
//                {
//                    PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
//                    pIntOpts.Message = "\nEnter CallOut Scale: ";
//                    //pIntOpts.DefaultValue = 18;


//                    PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);

//                    // Restrict input to positive and non-negative values
//                    pIntOpts.AllowZero = false;
//                    pIntOpts.AllowNegative = false;

//                    //Set thickness from input 
//                    double scaleCallOutFactor = pIntRes.Value;


//                    //BlockReference acBlkRefScale = acBlkRef.ScaleFactors
//                    acBlkRef.TransformBy(Matrix3d.Scaling(scaleCallOutFactor/currentScale, ptPositionInsert));

//                    BlockTableRecord acCurSpaceBlkTblRec;
//                    acCurSpaceBlkTblRec = acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

//                    acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
//                    acTrans.AddNewlyCreatedDBObject(acBlkRef, true);


//                    // Create a NameText

//                    using (MText acMText = new MText())
//                    {
//                        acMText.Location = new Point3d(ptPositionInsert.X, ptPositionInsert.Y - 10 * scaleCallOutFactor, 0);
//                        //acMText.Width = 4;

//                        acMText.TextHeight = 3.5 * scaleCallOutFactor;
//                        acMText.Contents = blkName + "\nTL 1:" + scaleCallOutFactor;

//                        acCurSpaceBlkTblRec.AppendEntity(acMText);
//                        acTrans.AddNewlyCreatedDBObject(acMText, true);
//                    }


//                }
//            }
//            // Save the new object to the database
//            acTrans.Commit();

//            // Dispose of the transaction
//        }
//    }




//    // NHÓM HÀM TẠO MẶT CẮT GỖ
//    //[CommandMethod("GV")]
//    public static void DrawMDFVarmm(double myInput)
//    {
//        // Get the current database and start the Transaction Manager
//        Document acDoc = Application.DocumentManager.MdiActiveDocument;
//        Database acCurDb = acDoc.Database;
//        Application.DocumentManager.MdiActiveDocument.Database.Orthomode = true;

//        PromptPointResult pPtRes;
//        PromptPointOptions pPtOpts = new PromptPointOptions("");

//        // Prompt for the start point
//        pPtOpts.Message = "\nEnter the start point of the line: ";
//        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//        Point3d ptStart = pPtRes.Value;

//        // Exit if the user presses ESC or cancels the command
//        if (pPtRes.Status == PromptStatus.Cancel) return;

//        // Prompt for the end point
//        pPtOpts.Message = "\nEnter the end point of the line: ";
//        pPtOpts.UseBasePoint = true;
//        pPtOpts.BasePoint = ptStart;
//        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//        Point3d ptEnd = pPtRes.Value;

//        if (pPtRes.Status == PromptStatus.Cancel) return;

//        // Start a transaction
//        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
//        {
//            BlockTable acBlkTbl;
//            BlockTableRecord acBlkTblRec;

//            // Open Model space for write
//            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
//                                            OpenMode.ForRead) as BlockTable;

//            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
//                                            OpenMode.ForWrite) as BlockTableRecord;

//            // Define the new line
//            using (Line acLine = new Line(ptStart, ptEnd))
//            {
//                // Add the line to the drawing
//                //acBlkTblRec.AppendEntity(acLine);
//                //acTrans.AddNewlyCreatedDBObject(acLine, true);

//                // Offset from line (acLine)

//                double thickness = myInput;

//                if (thickness <= 0)
//                {
//                    PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
//                    pIntOpts.Message = "\nEnter thickness: ";
//                    pIntOpts.DefaultValue = 18;


//                    PromptIntegerResult pIntRes = acDoc.Editor.GetInteger(pIntOpts);

//                    // Restrict input to positive and non-negative values
//                    pIntOpts.AllowZero = false;
//                    pIntOpts.AllowNegative = false;

//                    //Set thickness from input 
//                    thickness = pIntRes.Value;
//                }



//                DBObjectCollection acDbObjColl = acLine.GetOffsetCurves(thickness);

//                Point2d P1L1 = acLine.StartPoint.Convert2d(new Plane());
//                Point2d P2L1 = acLine.EndPoint.Convert2d(new Plane());

//                Point2d P1L2 = new Point2d();
//                Point2d P2L2 = new Point2d();

//                foreach (Entity acEnt in acDbObjColl)
//                {
//                    // Add each offset object
//                    //acBlkTblRec.AppendEntity(acEnt);
//                    //acTrans.AddNewlyCreatedDBObject(acEnt, true);

//                    if (acEnt is Line)
//                    {
//                        Line acEnt2Line = acEnt as Line;
//                        P1L2 = acEnt2Line.StartPoint.Convert2d(new Plane());
//                        P2L2 = acEnt2Line.EndPoint.Convert2d(new Plane());
//                    }
//                }
//                Polyline acPolyLine = new Polyline();

//                acPolyLine.AddVertexAt(0, P1L1, 0, 0, 0);
//                acPolyLine.AddVertexAt(0, P2L1, 0, 0, 0);
//                acPolyLine.AddVertexAt(0, P2L2, 0, 0, 0);
//                acPolyLine.AddVertexAt(0, P1L2, 0, 0, 0);
//                //acPolyLine.AddVertexAt(0, P1L1, 0, 0, 0);

//                acPolyLine.Closed = true;
//                acPolyLine.Layer = "WOOD BOUNDARY";
//                acBlkTblRec.AppendEntity(acPolyLine);
//                acTrans.AddNewlyCreatedDBObject(acPolyLine, true);
//                // Adds the circle to an object id array
//                ObjectIdCollection acObjIdColl = new ObjectIdCollection();
//                acObjIdColl.Add(acPolyLine.ObjectId);

//                // Create the hatch object and append it to the block table record
//                using (Hatch acHatch = new Hatch())
//                {
//                    acBlkTblRec.AppendEntity(acHatch);
//                    acTrans.AddNewlyCreatedDBObject(acHatch, true);

//                    // Set the properties of the hatch object
//                    // Associative must be set after the hatch object is appended to the 
//                    // block table record and before AppendLoop
//                    acHatch.SetHatchPattern(HatchPatternType.PreDefined, "HWEND6N0");
//                    acHatch.Associative = true;
//                    acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
//                    acHatch.EvaluateHatch(true);
//                    acHatch.Layer = "HATCH";
//                }
//            }
//            // Commit the changes and dispose of the transaction
//            acTrans.Commit();

//        }
//    }


//    [CommandMethod("GV")]
//    public static void DrawMDFInput()
//    {
//        DrawMDFVarmm(0);
//    }


//    [CommandMethod("G1")]
//    public static void DrawMDF5mm()
//    {
//        DrawMDFVarmm(5);
//    }

//    [CommandMethod("G2")]
//    public static void DrawMDF9mm()
//    {
//        DrawMDFVarmm(9);
//    }


//    [CommandMethod("G3")]
//    public static void DrawMDF18mm()
//    {
//        DrawMDFVarmm(18);
//    }


//    [CommandMethod("G4")]
//    public static void DrawMDF21mm()
//    {
//        DrawMDFVarmm(21);
//    }


//    [CommandMethod("G5")]
//    public static void DrawMDF25mm()
//    {
//        DrawMDFVarmm(21);
//    }



//    //NHÓM HÀM DIMSYLE
//    //[CommandMethod("getDimstyleName")]


//    //[CommandMethod("CDS")]
//    public static void ChanegDimStyle(string nameDimStyle)
//    {
//        Database db = Application.DocumentManager.MdiActiveDocument.Database;
//        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
//        using (Transaction trans = db.TransactionManager.StartTransaction())
//        {
//            DimStyleTable DimTabb = (DimStyleTable)trans.GetObject(db.DimStyleTableId,OpenMode.ForRead);
//            ObjectId dimId = ObjectId.Null;

//            if (!DimTabb.Has(nameDimStyle))
//            {
//                string warning = "Khong ton tai dimstyle co ten la: " + nameDimStyle;
//                ed.WriteMessage(warning);
//                System.Windows.Forms.MessageBox.Show(warning, "Thong bao");
//            }
//            else
//            {
//                dimId = DimTabb[nameDimStyle];

//                DimStyleTableRecord DimTabbRecord = (DimStyleTableRecord)trans.GetObject(dimId, OpenMode.ForRead);

//                if (DimTabbRecord.ObjectId != db.Dimstyle)
//                {
//                    db.Dimstyle = DimTabbRecord.ObjectId;
//                    db.SetDimstyleData(DimTabbRecord);
//                }
//            }

//            trans.Commit();
//        }
//    }
//    [CommandMethod("CDS")]
//    public static void ChangeDimStyleByName()
//    {
//        // Get the current document and database
//        Document acCurDoc = Application.DocumentManager.MdiActiveDocument;
//        Database acCurDb = acCurDoc.Database;

//        /// Scale
//        PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
//        pIntOpts.Message = "\nEnter Scale factor: ";
//        pIntOpts.DefaultValue = 1;

//        PromptDoubleResult pIntRes = acCurDoc.Editor.GetDouble(pIntOpts);
//        pIntOpts.AllowZero = false;
//        pIntOpts.AllowNegative = false;

//        if (pIntRes.Value == null) return;

//        double scaleFactorCallout = pIntRes.Value;

//        string nameDimStyle = "1-" + scaleFactorCallout;

//        if (scaleFactorCallout <1)
//        {
//            nameDimStyle = (Convert.ToInt32(1/scaleFactorCallout)).ToString() + "-1";
//        }

//        ChanegDimStyle(nameDimStyle);
        
//    }

//    public static void  NewDimStyle(string nameDimSyle,double dimScale)
//    {
//        Document acCurDoc = Application.DocumentManager.MdiActiveDocument;


//        Database acCurDb = Application.DocumentManager.MdiActiveDocument.Database;

//        using (Transaction trans = acCurDb.TransactionManager.StartTransaction())
//        {
//            DimStyleTable DimTabb = (DimStyleTable)trans.GetObject(acCurDb.DimStyleTableId, OpenMode.ForWrite);
//            ObjectId dimId = ObjectId.Null;

//            foreach (ObjectId style in DimTabb)
//            {
//                DimStyleTableRecord myDimstyle =  style.GetObject(OpenMode.ForWrite) as DimStyleTableRecord;
//                acCurDoc.Editor.WriteMessage("namestyle: {0}\n", myDimstyle.Name);
//            }

//            if (!DimTabb.Has(nameDimSyle))
//            {
//                DimTabb.UpgradeOpen();
//                DimStyleTableRecord newRecord = acCurDb.Dimstyle.GetObject(OpenMode.ForRead) as DimStyleTableRecord;
//                newRecord.Name = nameDimSyle;
//                dimId = DimTabb.Add(newRecord);

//                newRecord.Dimscale = dimScale;

//                trans.AddNewlyCreatedDBObject(newRecord, true);

//            }
//            else
//            {
//                dimId = DimTabb[nameDimSyle];
//               DimStyleTableRecord myNewDim = dimId.GetObject(OpenMode.ForWrite) as DimStyleTableRecord;
//               myNewDim.Dimscale = dimScale;
//            }
//            DimStyleTableRecord DimTabbRecord = (DimStyleTableRecord)trans.GetObject(dimId, OpenMode.ForWrite);
//            if (DimTabbRecord.ObjectId != acCurDb.Dimstyle)
//            {
//                acCurDb.Dimstyle = DimTabbRecord.ObjectId;
//                acCurDb.SetDimstyleData(DimTabbRecord);
//            }

//            trans.Commit();
//        }
//    }


//    // NHÓM HÀM DIMENSION
//    [CommandMethod("DD")]
//    public static void DLICustom()
//    {
//        //SetLayerCurrent("DIM");

//        Document acDoc = Application.DocumentManager.MdiActiveDocument;

//        Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
//        Database acCurDb = acDoc.Database;
//        // Draws a circle and zooms to the extents or 
//        // limits of the drawing


//        PromptPointResult pPtRes;
//        PromptPointOptions pPtOpts = new PromptPointOptions("");

//        // Prompt for the start point
//        pPtOpts.Message = "\nEnter the first point of the line: ";
//        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//        Point3d ptStart = pPtRes.Value;

//        // Exit if the user presses ESC or cancels the command
//        if (pPtRes.Status == PromptStatus.Cancel) return;

//        // Prompt for the end point
//        pPtOpts.Message = "\nEnter the end point of the line: ";
//        pPtOpts.UseBasePoint = true;
//        pPtOpts.BasePoint = ptStart;
//        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//        Point3d ptEnd = pPtRes.Value;

//        if (pPtRes.Status == PromptStatus.Cancel) return;


//        // Prompt for the end point
//        pPtOpts.Message = "\nEnter position dim: ";

//        Point3d ptMid = new Point3d((ptStart.X + ptEnd.X) / 2, (ptStart.Y + ptEnd.Y) / 2, (ptStart.Z + ptEnd.Z) / 2);
//        //pPtOpts.UseBasePoint = true;
//        //pPtOpts.BasePoint = ptMid;
//        //pPtOpts.UseDashedLine = true;
//        pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//        Point3d ptDim = pPtRes.Value;
//        if (pPtRes.Status == PromptStatus.Cancel) return;


//        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
//        {

//            BlockTable acBlkTbl;
//            BlockTableRecord acBlkTblRec;

//            // Open Model space for write
//            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
//                                            OpenMode.ForRead) as BlockTable;

//            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
//                                            OpenMode.ForWrite) as BlockTableRecord;


//            double myRotation = Math.Atan2(ptStart.Y - ptEnd.Y, ptStart.X - ptEnd.X);

//            //var dim = new RotatedDimension(ptStart, ptEnd, ptDim, null, acCurDb.Dimstyle);
//            RotatedDimension dim = new RotatedDimension(myRotation, ptStart, ptEnd, ptDim, null, acCurDb.Dimstyle);
//            //dim.XLine1Point = ptStart;
//            dim.Layer = "DIM";

//            // Add the new object to Model space and the transaction
//            acBlkTblRec.AppendEntity(dim);
//            acTrans.AddNewlyCreatedDBObject(dim, true);

//            // Commit the changes and dispose of the transaction
//            acTrans.Commit();
//        }
//    }


//    [CommandMethod("MD")]
//    public static void mergeDim()
//    {
//        // Get the current document and database
//        Document acDoc = Application.DocumentManager.MdiActiveDocument;
//        Database acCurDb = acDoc.Database;

//        // Start a transaction
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


//            //Filter selecttion

//            // Create a TypedValue array to define the filter criteria
//            TypedValue[] acTypValAr = new TypedValue[1];
//            acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "DIMENSION"), 0);
//            //acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "0"), 2);

//            // Assign the filter criteria to a SelectionFilter object
//            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

//            // Request for objects to be selected in the drawing area
//            PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection();


//            // If the prompt status is OK, objects were selected
//            if (acSSPrompt.Status == PromptStatus.OK)
//            {
//                SelectionSet acSSet = acSSPrompt.Value;

//                // neu so luong dim dc chon <2 return


//                // Tao 1 list point chua chan dim
//                List<Point3d> myListXlinePoint = new List<Point3d>();

//                // lay dim point dau tien

//                Point3d myDimLinePoint = new Point3d();
//                double myRotation = new double();

//                // Step through the objects in the selection set
//                foreach (SelectedObject acSSObj in acSSet)
//                {
//                    string typeDimName = acSSObj.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name;
//                    if (typeDimName == "AcDbRotatedDimension")
//                    //if (typeDimName == "AcDbAlignedDimension" || typeDimName == "AcDbRotatedDimension")
//                    {
//                        //acDoc.Editor.WriteMessage("This is a dim linear: {0}\n", acSSObj.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name);


//                        RotatedDimension myDim = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as RotatedDimension;
//                        // Lay dimline point
//                        Point3d myTempDimLinePoint = myDim.DimLinePoint;
//                        myDimLinePoint = myTempDimLinePoint;

//                        // Lay goc xoay
//                        double myTempRotation = myDim.Rotation;
//                        myRotation = myTempRotation;


//                        Point3d myPoint1 = myDim.XLine1Point;
//                        Point3d myPoint2 = myDim.XLine2Point;

//                        myListXlinePoint.Add(myPoint1);
//                        myListXlinePoint.Add(myPoint2);

//                        acDoc.Editor.WriteMessage("This is a dim linear: {0}\n", acSSObj.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name);

//                        acDoc.Editor.WriteMessage("Success convert");

//                        // Delete Dim
//                        acSSObj.ObjectId.GetObject(OpenMode.ForWrite).Erase();
//                        acDoc.Editor.WriteMessage("Success erase");
//                    }

//                    else if (typeDimName == "AcDbAlignedDimension")
//                    {
//                        //acDoc.Editor.WriteMessage("This is a dim linear: {0}\n", acSSObj.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name);

//                        AlignedDimension myDim = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as AlignedDimension;
//                        // Lay dimline point
//                        Point3d myTempDimLinePoint = myDim.DimLinePoint;
//                        myDimLinePoint = myTempDimLinePoint;





//                        Point3d myPoint1 = myDim.XLine1Point;
//                        Point3d myPoint2 = myDim.XLine2Point;

//                        // Lay goc xoay
//                        double myTempRotation = myDim.HorizontalRotation;
//                        myTempRotation = Math.Atan2(myPoint1.Y - myPoint2.Y, myPoint1.X - myPoint2.X);
//                        myRotation = myTempRotation;

//                        myListXlinePoint.Add(myPoint1);
//                        myListXlinePoint.Add(myPoint2);

//                        acDoc.Editor.WriteMessage("This is a dim linear: {0}\n", acSSObj.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name);


//                        // Delete Dim
//                        acSSObj.ObjectId.GetObject(OpenMode.ForWrite).Erase();
//                        acDoc.Editor.WriteMessage("Success erase");
//                    }


//                    //acDoc.Editor.WriteMessage("typedim: {0}\n", acSSObj.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name);
//                }

//                //// SortList


//                if (myRotation < 0.01)
//                {
//                    myListXlinePoint.Sort(sortByX);
//                }
//                else if (myRotation - Math.PI / 2 < 0.01)
//                {
//                    myListXlinePoint.Sort(sortByY);
//                }
//                else
//                {
//                    myListXlinePoint.Sort(sortByX);
//                }
//                // Create the rotated dimension
//                using (RotatedDimension acRotDim = new RotatedDimension())
//                {
//                    acRotDim.XLine1Point = myListXlinePoint[0];
//                    acRotDim.XLine2Point = myListXlinePoint[myListXlinePoint.Count - 1];

//                    if (myRotation - Math.PI / 2 > 0.01 && myRotation < 0.01)
//                    {
//                        double myTempRotation2 = Math.Atan2(acRotDim.XLine1Point.Y - acRotDim.XLine2Point.Y, acRotDim.XLine1Point.X - acRotDim.XLine2Point.X);
//                        myRotation = myTempRotation2;
//                    }

//                    acRotDim.Rotation = myRotation;


//                    acDoc.Editor.WriteMessage("goc xoay dim: {0}", myRotation);
//                    acRotDim.DimLinePoint = myDimLinePoint;
//                    acRotDim.DimensionStyle = acCurDb.Dimstyle;
//                    acRotDim.Layer = "DIM";

//                    // Add the new object to Model space and the transaction
//                    acBlkTblRec.AppendEntity(acRotDim);
//                    acTrans.AddNewlyCreatedDBObject(acRotDim, true);
//                }

//                // Save the new object to the database
//                acTrans.Commit();
//            }

//            // Dispose of the transaction
//        }
//    }
//    // Merge and split dim

//    [CommandMethod("SD")]
//    public static void splitDim()
//    {
//        // Get the current document and database
//        Document acDoc = Application.DocumentManager.MdiActiveDocument;
//        Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
//        Database acCurDb = acDoc.Database;

//        // Start a transaction
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


//            //Filter selecttion

//            // Create a TypedValue array to define the filter criteria
//            TypedValue[] acTypValAr = new TypedValue[1];
//            acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "DIMENSION"), 0);
//            //acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "0"), 2);

//            // Assign the filter criteria to a SelectionFilter object
//            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

//            //PromptEntityOptions 

//            PromptEntityOptions acPEO = new Autodesk.AutoCAD.EditorInput.PromptEntityOptions("Chon 1 dim de chia... ");

//            // Request for objects to be selected in the drawing area
//            PromptEntityResult myAcPER = acDoc.Editor.GetEntity(acPEO);

//            List<Point3d> myListPointSplit = new List<Point3d>();
//            // If the prompt status is OK, objects were selected
//            if (myAcPER.Status == PromptStatus.OK)
//            {

//                //DBObject myAcDim = myAcPER.ObjectId.GetObject(OpenMode.ForWrite);
//                string typeDimName = myAcPER.ObjectId.GetObject(OpenMode.ForRead).GetRXClass().Name;
//                if (typeDimName == "AcDbRotatedDimension")
//                {
//                    RotatedDimension myAcDim = myAcPER.ObjectId.GetObject(OpenMode.ForWrite) as RotatedDimension;
//                    // Chon diem de slpit:
//                    // Lay thong tin goc xoay
//                    double myRotationDimCur = myAcDim.Rotation;
//                    myListPointSplit.Add(myAcDim.XLine1Point);
//                    myListPointSplit.Add(myAcDim.XLine2Point);
//                    // Lay vi tri dim

//                    Point3d myDimPointLine = myAcDim.DimLinePoint;


//                    // Select a point to split
//                    // Prompt for the end point

//                    PromptPointResult pPtRes;
//                    PromptPointOptions pPtOpts = new PromptPointOptions("");
//                    pPtOpts.Message = "\nEnter position dim: ";

//                    pPtOpts.UseBasePoint = true;
//                    //pPtOpts.BasePoint = new Point3d((myAcDim.XLine1Point.X + myAcDim.XLine2Point.X) / 2, (myAcDim.XLine1Point.Y + myAcDim.XLine2Point.Y) / 2, myAcDim.XLine1Point.Z);
//                    pPtOpts.BasePoint = myAcDim.XLine1Point;
//                    pPtOpts.UseDashedLine = true;
//                    pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//                    Point3d ptInsertPoint = pPtRes.Value;

//                    myListPointSplit.Add(ptInsertPoint);


//                    if (pPtRes.Status == PromptStatus.Cancel) return;

//                    // Tao rotation dim from list point

//                    int nDim = 0;

//                    if (myRotationDimCur < Math.PI / 4)
//                    {
//                        myListPointSplit.Sort(sortByX);
//                    }
//                    else
//                    {
//                        myListPointSplit.Sort(sortByY);
//                    }


//                    for (int i = 1; i < myListPointSplit.Count; i++)
//                    {
//                        using (RotatedDimension acRotDim = new RotatedDimension())
//                        {
//                            acRotDim.XLine1Point = myListPointSplit[i - 1];
//                            acRotDim.XLine2Point = myListPointSplit[i];
//                            acRotDim.Rotation = myRotationDimCur;
//                            acRotDim.DimLinePoint = myDimPointLine;
//                            acRotDim.DimensionStyle = acCurDb.Dimstyle;
//                            acRotDim.Layer = "DIM";


//                            // Add the new object to Model space and the transaction
//                            if (acRotDim.Rotation == 0)
//                            {
//                                if (Math.Abs(((Point3d)myListPointSplit[i]).X - ((Point3d)myListPointSplit[i - 1]).X) > 0.01)
//                                {
//                                    acBlkTblRec.AppendEntity(acRotDim);
//                                    acTrans.AddNewlyCreatedDBObject(acRotDim, true);
//                                    nDim++;
//                                }
//                            }
//                            else
//                            {
//                                if (Math.Abs(((Point3d)myListPointSplit[i]).Y - ((Point3d)myListPointSplit[i - 1]).Y) > 0.01)
//                                {
//                                    acBlkTblRec.AppendEntity(acRotDim);
//                                    acTrans.AddNewlyCreatedDBObject(acRotDim, true);
//                                    nDim++;
//                                }
//                            }
//                        }
//                    }
//                    // Delete Dim
//                    myAcDim.Erase();
//                }
//                acDoc.Editor.WriteMessage("Object ID: {0}\n", myAcPER.ObjectId);
//                // neu so luong dim dc chon <2 return
//            }

//            acTrans.Commit();
//        }

//        // Dispose of the transaction
//    }
//    // Merge and split dim




//    //Tao vung chon bang chuot, lay thong tin vertex cua cac polyline and line trong vung chon
//    //[CommandMethod("DO", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
//    public void DimOrtho(string myInput)
//    {
//        Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
//        Database acCurDb = acDoc.Database;
//        Editor ed = acDoc.Editor;

//        List<Point3d> points = new List<Point3d>();

//        PromptPointOptions ppo = new PromptPointOptions("\n\tSpecify a first corner: ");

//        PromptPointResult ppr = ed.GetPoint(ppo);

//        if (ppr.Status != PromptStatus.OK) return;

//        PromptCornerOptions pco = new PromptCornerOptions("\n\tOther corner: ", ppr.Value);
//        pco.UseDashedLine = true;
//        PromptPointResult pcr = ed.GetCorner(pco);
//        if (pcr.Status != PromptStatus.OK) return;

//        Point3d pt1 = ppr.Value;

//        Point3d pt2 = pcr.Value;



//        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
//        {
//            try
//            {
//                BlockTable acBlkTbl;
//                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
//                                                OpenMode.ForRead) as BlockTable;

//                // Open the Block table record Model space for write
//                BlockTableRecord acBlkTblRec;
//                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
//                                                OpenMode.ForWrite) as BlockTableRecord;


//                // kiem tra hinh window select hop le hay khong.
//                if (pt1.X == pt2.X || pt1.Y == pt2.Y)
//                {
//                    ed.WriteMessage("\nInvalid point specification");
//                    return;
//                }

//                PromptSelectionResult res;
//                res = ed.SelectCrossingWindow(pt1, pt2);


//                if (res.Status != PromptStatus.OK)
//                    return;
//                SelectionSet sset = res.Value;

//                List<Point3d> myListPoint1 = new List<Point3d>();
//                foreach (var objID in sset.GetObjectIds())
//                {
//                    string className = objID.ObjectClass.DxfName;
//                    //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n TYPE: {0}\n", className);

//                    // Neu Object class la line hoac polyline, lay point them vao list point:
//                    if (className == "LINE")
//                    {
//                        // Convert objID to Line
//                        Line myLine = objID.GetObject(OpenMode.ForRead) as Line;
//                        Point3d endPoint = myLine.EndPoint;
//                        myListPoint1.Add(myLine.EndPoint);
//                        myListPoint1.Add(myLine.StartPoint);
//                    }
//                    // Neu Object class polyline, lay point them vao list point:
//                    if (className == "LWPOLYLINE")
//                    {
//                        // Convert objID to Line
//                        Polyline myPoly = objID.GetObject(OpenMode.ForRead) as Polyline;
//                        int vn = myPoly.NumberOfVertices;

//                        // Get all Point from Polyline:
//                        for (int i = 0; i < vn; i++)
//                        {
//                            Point3d pt = new Point3d(myPoly.GetPoint2dAt(i).X, myPoly.GetPoint2dAt(i).Y, 0);
//                            myListPoint1.Add(pt);
//                        }
//                    }
//                }

//                // Insert DBPoint at Point of listPoint


//                foreach (Point3d myPoint in myListPoint1)
//                {
//                    using (DBPoint acDBPoint = new DBPoint(myPoint))
//                    {
//                        acDBPoint.Layer = "Defpoints";
//                        // Add the new object to the block table record and the transaction
//                        acBlkTblRec.AppendEntity(acDBPoint);
//                        acTrans.AddNewlyCreatedDBObject(acDBPoint, true);
//                    }
//                }

//                acTrans.Commit();

//            }
//            catch (System.Exception ex)
//            {
//                ed.WriteMessage(ex.Message + "\n" + ex.StackTrace);
//            }
//        }

//        // Insert Point and make listPoint
//        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
//        {
//            PromptSelectionResult res2;
//            res2 = ed.SelectWindow(pt1, pt2);
//            if (res2.Status != PromptStatus.OK)
//                return;
//            SelectionSet sset2 = res2.Value;

//            List<Point3d> myListPoint2 = new List<Point3d>();

//            foreach (var objID2 in sset2.GetObjectIds())
//            {
//                string className = objID2.ObjectClass.DxfName;
//                //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n type: {0}\n", myListPoint2.Count);
//                if (className == "POINT")
//                {
//                    // Convert objID2 to Point3D
//                    DBPoint myDBPoint = (DBPoint)objID2.GetObject(OpenMode.ForWrite);
//                    myListPoint2.Add((Point3d)myDBPoint.Position);
//                }
//            }

//            //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n Total Point: {0}\n", myListPoint2.Count);

//            //foreach (Point3d p in myListPoint2)
//            //{
//            //    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n Point of line inside: ({0}, {1}, {2})\n", p.X, p.Y, p.Z);
//            //}

//            // Tao dim


//            if (myInput == "H")
//            {
//                myListPoint2.Sort(sortByX);
//                autoDimHorizontal(myListPoint2);
//            }
//            else if (myInput == "V")
//            {
//                myListPoint2.Sort(sortByY);
//                autoDimVertical(myListPoint2);
//            }


//            else
//            {
//                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
//                pKeyOpts.Message = "\nEnter an option ";
//                pKeyOpts.Keywords.Add("Horizontal");
//                pKeyOpts.Keywords.Add("Vertical");
//                pKeyOpts.Keywords.Default = "Horizontal";
//                pKeyOpts.AllowNone = false;

//                PromptResult pKeyRes = acDoc.Editor.GetKeywords(pKeyOpts);

//                if (pKeyRes.StringResult == "Horizontal")
//                {
//                    myListPoint2.Sort(sortByX);
//                    autoDimHorizontal(myListPoint2);
//                }
//                else
//                {
//                    myListPoint2.Sort(sortByY);
//                    autoDimVertical(myListPoint2);
//                }
//            }
//            acTrans.Commit();
//        }




//        // Delete all DBpoint temporatory
//        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
//        {

//            TypedValue[] acTypValAr = new TypedValue[2];
//            acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POINT"), 0);
//            acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "Defpoints"), 1);

//            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);
//            PromptSelectionResult resPoint;

//            resPoint = ed.SelectAll(acSelFtr);

//            //if (resPoint.Status != PromptStatus.OK)

//            //    return;
//            SelectionSet ssetPoint = resPoint.Value;

//            BlockTable acBlkTbl;
//            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
//                                            OpenMode.ForRead) as BlockTable;

//            // Open the Block table record Model space for write
//            BlockTableRecord acBlkTblRec;
//            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
//                                            OpenMode.ForWrite) as BlockTableRecord;

//            if (ssetPoint.Count > 0)
//            {
//                foreach (var objID in ssetPoint.GetObjectIds())
//                {
//                    DBPoint DBPoint = objID.GetObject(OpenMode.ForWrite) as DBPoint;
//                    DBPoint.Erase();

//                }
//            }

//            acTrans.Commit();
//        }


//    }


//    [CommandMethod("DH", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
//    public void DimHorizontal()
//    {
//        DimOrtho("H");
//    }


//    [CommandMethod("DV", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
//    public void DimVertical()
//    {
//        DimOrtho("V");
//    }

//    [CommandMethod("DO", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
//    public void DimOrthoInput()
//    {
//        DimOrtho("O");
//    }


//    public static void autoDimHorizontal(List<Point3d> listPoint)
//    {
//        // Get the current database
//        Document acDoc = Application.DocumentManager.MdiActiveDocument;
//        Database acCurDb = acDoc.Database;

//        // Start a transaction
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

//            PromptPointResult pPtRes;
//            PromptPointOptions pPtOpts = new PromptPointOptions("");


//            // Prompt for the end point
//            pPtOpts.Message = "\nEnter position dim: ";
//            //pPtOpts.UseBasePoint = true;

//            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//            Point3d ptDim = pPtRes.Value;
//            if (pPtRes.Status == PromptStatus.Cancel) return;

//            int nDim = 0;
//            for (int i = 1; i < listPoint.Count; i++)
//            {
//                using (RotatedDimension acRotDim = new RotatedDimension())
//                {
//                    acRotDim.XLine1Point = listPoint[i - 1];
//                    acRotDim.XLine2Point = listPoint[i];
//                    //acRotDim.Rotation = Math.PI / 2;
//                    acRotDim.DimLinePoint = ptDim;
//                    acRotDim.DimensionStyle = acCurDb.Dimstyle;
//                    acRotDim.Layer = "DIM";


//                    // Add the new object to Model space and the transaction
//                    if (Math.Abs(((Point3d)listPoint[i]).X - ((Point3d)listPoint[i - 1]).X) > 0.01)
//                    {
//                        acBlkTblRec.AppendEntity(acRotDim);
//                        acTrans.AddNewlyCreatedDBObject(acRotDim, true);
//                        nDim++;
//                    }

//                }
//            }


//            // Tao dim tong:
//            if (nDim > 1)
//            {
//                using (RotatedDimension acRotDim = new RotatedDimension())
//                {
//                    double dimScale = acCurDb.GetDimstyleData().Dimscale;
//                    Point3d ptDimT = new Point3d();
//                    if (ptDim.Y <= listPoint[0].Y)
//                    {
//                        ptDimT = new Point3d(ptDim.X, ptDim.Y - 5 * dimScale, ptDim.Z);
//                    }
//                    else
//                    {
//                        ptDimT = new Point3d(ptDim.X, ptDim.Y + 5 * dimScale, ptDim.Z);
//                    }

//                    acRotDim.XLine1Point = listPoint[0];
//                    acRotDim.XLine2Point = listPoint[listPoint.Count - 1];
//                    //acRotDim.Rotation = Math.PI / 2;
//                    acRotDim.DimLinePoint = ptDimT;
//                    acRotDim.DimensionStyle = acCurDb.Dimstyle;
//                    acRotDim.Layer = "DIM";


//                    // Add the new object to Model space and the transaction
//                    if (Math.Abs(((Point3d)listPoint[0]).X - ((Point3d)listPoint[listPoint.Count - 1]).X) > 0.01)
//                    {
//                        acBlkTblRec.AppendEntity(acRotDim);
//                        acTrans.AddNewlyCreatedDBObject(acRotDim, true);
//                    }
//                }
//            }


//            // Commit the changes and dispose of the transaction
//            acTrans.Commit();
//        }
//    }


//    public static void autoDimVertical(List<Point3d> listPoint)
//    {
//        // Get the current database
//        Document acDoc = Application.DocumentManager.MdiActiveDocument;
//        Database acCurDb = acDoc.Database;

//        // Start a transaction
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

//            PromptPointResult pPtRes;
//            PromptPointOptions pPtOpts = new PromptPointOptions("");


//            // Prompt for the end point
//            pPtOpts.Message = "\nEnter position dim: ";
//            //pPtOpts.UseBasePoint = true;

//            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//            Point3d ptDim = pPtRes.Value;
//            if (pPtRes.Status == PromptStatus.Cancel) return;

//            int nDim = 0;
//            for (int i = 1; i < listPoint.Count; i++)
//            {
//                using (RotatedDimension acRotDim = new RotatedDimension())
//                {
//                    acRotDim.XLine1Point = listPoint[i - 1];
//                    acRotDim.XLine2Point = listPoint[i];
//                    acRotDim.Rotation = Math.PI / 2;
//                    acRotDim.DimLinePoint = ptDim;
//                    acRotDim.DimensionStyle = acCurDb.Dimstyle;
//                    acRotDim.Layer = "DIM";


//                    // Add the new object to Model space and the transaction
//                    if (Math.Abs(((Point3d)listPoint[i]).Y - ((Point3d)listPoint[i - 1]).Y) > 0.01)
//                    {
//                        acBlkTblRec.AppendEntity(acRotDim);
//                        acTrans.AddNewlyCreatedDBObject(acRotDim, true);
//                        nDim++;
//                    }

//                }
//            }


//            // Tao dim tong:

//            if (nDim > 1)
//            {
//                using (RotatedDimension acRotDim = new RotatedDimension())
//                {
//                    double dimScale = acCurDb.GetDimstyleData().Dimscale;
//                    Point3d ptDimT = new Point3d();
//                    if (ptDim.X <= listPoint[0].X)
//                    {
//                        ptDimT = new Point3d(ptDim.X - 5 * dimScale, ptDim.Y, ptDim.Z);
//                    }
//                    else
//                    {
//                        ptDimT = new Point3d(ptDim.X + 5 * dimScale, ptDim.Y, ptDim.Z);
//                    }

//                    acRotDim.XLine1Point = listPoint[0];
//                    acRotDim.XLine2Point = listPoint[listPoint.Count - 1];
//                    acRotDim.Rotation = Math.PI / 2;
//                    acRotDim.DimLinePoint = ptDimT;
//                    acRotDim.DimensionStyle = acCurDb.Dimstyle;
//                    acRotDim.Layer = "DIM";


//                    // Add the new object to Model space and the transaction
//                    if (Math.Abs(((Point3d)listPoint[0]).Y - ((Point3d)listPoint[listPoint.Count - 1]).Y) > 0.01)
//                    {
//                        acBlkTblRec.AppendEntity(acRotDim);
//                        acTrans.AddNewlyCreatedDBObject(acRotDim, true);
//                    }
//                }
//            }

//            // Commit the changes and dispose of the transaction
//            acTrans.Commit();
//        }
//    }


//    static int sortByX(Point3d a, Point3d b)
//    {
//        if (a.X == b.X)
//            return a.Y.CompareTo(b.Y);
//        return a.X.CompareTo(b.X);
//    }


//    static int sortByY(Point3d a, Point3d b)
//    {
//        if (a.Y == b.Y)
//            return a.X.CompareTo(b.X);
//        return a.Y.CompareTo(b.Y);
//    }

//}
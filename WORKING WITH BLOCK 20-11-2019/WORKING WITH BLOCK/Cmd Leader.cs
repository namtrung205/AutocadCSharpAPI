using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
//
using commonFunctions;
using System;


namespace myCustomCmds
{
    public class CmdLeader
    {
        /// NHÓM HÀM LEADER
        [CommandMethod("CNL")]
        public static void createNewLeaderTest()
        {
            //SetLayerCurrent("DIM");
            CmdLayer.createALayerByName("DIM");

            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
            Database acCurDb = acDoc.Database;


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
                using (Leader acLdr = new Leader())
                {
                    acLdr.AppendVertex(new Point3d(0, 0, 0));
                    acLdr.AppendVertex(new Point3d(4, 4, 0));
                    acLdr.AppendVertex(new Point3d(4, 5, 0));
                    acLdr.HasArrowHead = true;

                    // Add the new object to Model space and the transaction
                    acBlkTblRec.AppendEntity(acLdr);
                    acTrans.AddNewlyCreatedDBObject(acLdr, true);
                }

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
        }

        public static void createMleader(string myMText, Point3d firstPoint, Point3d placePoint)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;

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
                using (MLeader acMLdr = new MLeader())
                {
                    //acMLdr.MLeaderStyle = 
                    int idx = acMLdr.AddLeaderLine(placePoint);

                    acMLdr.AddFirstVertex(idx, firstPoint);

                    MText mText = new MText();

                    mText.SetDatabaseDefaults();

                    mText.SetContentsRtf(myMText);

                    //mText.Location = new Point3d(200, 100, 0);

                    acMLdr.MText = mText;

                    // Add the new object to Model space and the transaction
                    acBlkTblRec.AppendEntity(acMLdr);
                    acTrans.AddNewlyCreatedDBObject(acMLdr, true);
                }

                // Commit the changes and dispose of the transaction
                acTrans.Commit();

            }
        }



        // Create a MLeader  Style function w

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"> Text string </param>
        /// <param name="pointPlace"> Place point</param>
        /// <param name="firstPoint"> arrow Point</param>
        public static void makeMleader(string text, Point3d pointPlace, Point3d firstPoint)
        {
            CmdLayer.createALayerByName("DIM");

            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
            Database acCurDb = acDoc.Database;


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

                using (MLeader myMLeader = new MLeader())
                {
                    myMLeader.SetDatabaseDefaults();
                    myMLeader.ContentType = ContentType.MTextContent;

                    MText mText = new MText();
                    mText.SetDatabaseDefaults();
                    mText.Width = 100;
                    mText.Height = 50;
                    mText.SetContentsRtf(text);
                    mText.Location = new Point3d(4, 2, 0);
                    myMLeader.MText = mText;
                    int idx = myMLeader.AddLeaderLine(new Point3d(1, 1, 0));
                    myMLeader.AddFirstVertex(idx, new Point3d(0, 0, 0));
                    acBlkTblRec.AppendEntity(myMLeader);
                    acTrans.AddNewlyCreatedDBObject(myMLeader, true);
                    acTrans.Commit();

                }
            }

        }
    
    
        /// MLEADER STYLES
        static public void AddMleaderStylebyScale(double myScaleFactor)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                var textStyles = (TextStyleTable)acTrans.GetObject(acCurDb.TextStyleTableId, OpenMode.ForRead);

                //Name of the mleader Style to edit
                string styleName = "1-" + myScaleFactor;
                if (myScaleFactor < 1)
                {
                    styleName = (int)(1 / myScaleFactor) + "-1";
                }

                DBDictionary mlstyles = (DBDictionary)acTrans.GetObject(acCurDb.MLeaderStyleDictionaryId, OpenMode.ForRead);
                ObjectId mLeaderStyle = ObjectId.Null;

                if (!mlstyles.Contains(styleName))
                {
                    //add a new mleader style...

                    MLeaderStyle newStyle = new MLeaderStyle();
                    //make the arrow head as DOT.
                    BlockTable blockTable = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    newStyle.TextStyleId = textStyles["ARIAL 2.0"];
                    newStyle.ArrowSize = 1.5;
                    newStyle.BreakSize = 1;
                    newStyle.EnableLanding = true;
                    newStyle.EnableFrameText = true;
                    newStyle.TextAlignAlwaysLeft = true;

                    newStyle.Scale = myScaleFactor;
                    mLeaderStyle = newStyle.PostMLeaderStyleToDb(acCurDb, styleName);
                    acTrans.AddNewlyCreatedDBObject(newStyle, true);
                }
                else
                {
                    mLeaderStyle = mlstyles.GetAt(styleName);
                }
                //make the new leader as current...
                acCurDb.MLeaderstyle = mLeaderStyle;

                acTrans.Commit();
            }
        }
      

        [CommandMethod("ALSS")]
        public static void addMleaderStyleInputScale()
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

            double scaleFactor = pIntRes.Value;

            AddMleaderStylebyScale(scaleFactor);
        }


    }
}



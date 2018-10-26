using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
//
using commonFunctions;


namespace myCustomCmds
{
    public class CmdLayer
    {
        public static void createALayerByName(string layerName)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                                                OpenMode.ForRead) as LayerTable;

                string sLayerName = layerName;

                if (acLyrTbl.Has(sLayerName) == false)
                {
                    using (LayerTableRecord acLyrTblRec = new LayerTableRecord())
                    {
                        acLyrTblRec.Name = sLayerName;

                        // Upgrade the Layer table for write
                        acLyrTbl.UpgradeOpen();

                        // Append the new layer to the Layer table and the transaction
                        acLyrTbl.Add(acLyrTblRec);
                        acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
                    }
                }

                // Save the changes and dispose of the transaction
                acTrans.Commit();
            }
        }

        public static void ChangeCurrentLayer(string layerName)
        {
            // Chon 1 doi tuong 
            Document acCurDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acCurDoc.Database;
            Editor acEd = acCurDoc.Editor;


            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                                                OpenMode.ForRead) as LayerTable;

                string sLayerName = layerName;

                if (acLyrTbl.Has(sLayerName) == false)
                {

                    string info = "Khong ton tai layer: " + sLayerName + " trong file dang mo";

                    Application.ShowAlertDialog(info);

                }
                acCurDb.Clayer = acLyrTbl[sLayerName];
                acTrans.Commit();
            }
        }


        [CommandMethod("CLO")]
        public static void ChangeCurrentLayerByObject()
        {
            // Chon 1 doi tuong 
            Document acCurDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acCurDoc.Database;
            Editor acEd = acCurDoc.Editor;

            // CHon 1 doi tuong de lay thong tin ve layer


            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {


                PromptEntityOptions acPEO = new PromptEntityOptions("\nSelect An Object");
                PromptEntityResult acPER = acEd.GetEntity(acPEO);

                if (acPER.Status != PromptStatus.OK) return;

                if (acPER == null) return;

                ////Do bug roi day
                try
                {
                    Entity myObjectSelected = acTrans.GetObject(acPER.ObjectId, OpenMode.ForWrite) as Entity;
                    acEd.WriteMessage("\nCurrent layer have been changed to: " + myObjectSelected.Layer);
                    ChangeCurrentLayer(myObjectSelected.Layer);
                    acTrans.Commit();
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Application.ShowAlertDialog(ex.Message);
                }
            }
        }
    }
}



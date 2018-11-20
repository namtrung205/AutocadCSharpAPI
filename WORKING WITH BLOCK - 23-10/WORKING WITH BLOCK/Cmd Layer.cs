using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
//
using commonFunctions;
using Autodesk.AutoCAD.Colors;


namespace myCustomCmds
{
    public class CmdLayer
    {
        public static void createALayerByName(string layerName)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;


            using (DocumentLock docLock = acDoc.LockDocument())
            {
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
        }


        public static void createALayerByNameAndColor(string layerName, Color myLayerColor)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;


            using (DocumentLock docLock = acDoc.LockDocument())
            {
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
                            acLyrTblRec.Color = myLayerColor;

                            // Upgrade the Layer table for write
                            acLyrTbl.UpgradeOpen();

                            // Append the new layer to the Layer table and the transaction
                            acLyrTbl.Add(acLyrTblRec);
                            acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
                        }
                    }
                    else
                    {
                        Application.ShowAlertDialog("Đã tồn tại layer này!");
                    }

                    // Save the changes and dispose of the transaction
                    acTrans.Commit();

                    

                }
            }
        }




        [CommandMethod("showColorDlg")]

        public void showColorDlg()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Autodesk.AutoCAD.Windows.ColorDialog dlg = new Autodesk.AutoCAD.Windows.ColorDialog();
            if (dlg.ShowDialog() !=System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            if (!dlg.Color.IsByAci)
            {
                if (dlg.Color.IsByLayer)
                {
                    //by layer
                    ed.WriteMessage("By Layer\n");
                }
                else if (dlg.Color.IsByBlock)
                {
                    //by block
                    ed.WriteMessage("By block\n");
                }
                else
                {
                    ed.WriteMessage(dlg.Color.Red.ToString()

                        + "--" + dlg.Color.Green.ToString() +

                                    "--" + dlg.Color.Blue.ToString() + "\n");
                }
            }
            else
            {
                short colIndex = dlg.Color.ColorIndex;
                System.Byte byt = System.Convert.ToByte(colIndex);
                int rgb = Autodesk.AutoCAD.Colors.EntityColor.LookUpRgb(byt);
                long b = (rgb & 0xffL);
                long g = (rgb & 0xff00L) >> 8;
                long r = rgb >> 16; ;
               ed.WriteMessage(r.ToString() + "--" +

                                  g.ToString() + "--" + b.ToString() + "\n");
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



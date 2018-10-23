﻿using Autodesk.AutoCAD.Runtime;
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
    public class CmdText
    {
        [CommandMethod("CRT")]
        public static void CreateTitle()
        {
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



            // Create a title
            PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Title Callout: ");
            pStrOpts.AllowSpaces = true;
            pStrOpts.DefaultValue = "Call Out";
            PromptResult pStrRes = acCurDoc.Editor.GetString(pStrOpts);

            if (pStrRes.StringResult == null || pStrRes.StringResult == "") return;


            string myTitle = pStrRes.StringResult;

            string myTitleText = "\\L" + myTitle.ToUpper() + "\\l" + "\nTL- 1: " + scaleFactorCallout;

            if (scaleFactorCallout < 1)
            {
                int newScale = Convert.ToInt32(1 / scaleFactorCallout);
                myTitleText = "\\L" + myTitle.ToUpper() + "\\l" + "\nTL- " + newScale + ":1";
            }

            else
            {
                myTitleText = "\\L" + myTitle.ToUpper() + "\\l" + "\nTL- 1: " + scaleFactorCallout;
            }




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

                // Create a multiline text object

                // Set point origin block
                PromptPointResult pPtRes;
                PromptPointOptions pPtOpts = new PromptPointOptions("");

                // Prompt for the start point
                pPtOpts.Message = "\nPick Title Text Place: ";
                pPtRes = acCurDoc.Editor.GetPoint(pPtOpts);
                Point3d ptOrigin = pPtRes.Value;

                // Exit if the user presses ESC or cancels the command
                if (pPtRes.Status == PromptStatus.Cancel) return;


                using (MText acMText = new MText())
                {
                    acMText.SetAttachmentMovingLocation(AttachmentPoint.MiddleCenter);
                    acMText.Location = ptOrigin;
                    acMText.Contents = myTitleText;
                    acMText.TextHeight = 3.5 * scaleFactorCallout;

                    acMText.Layer = "TITLE_BLOCK";

                    acBlkTblRec.AppendEntity(acMText);
                    acTrans.AddNewlyCreatedDBObject(acMText, true);
                }
                // Save the changes and dispose of the transaction
                acTrans.Commit();

                //Create Dim
                string myNameDimStyle = "1-1";


                if (scaleFactorCallout < 1)
                {
                    int newScale = Convert.ToInt32(1 / scaleFactorCallout);
                    myNameDimStyle = newScale + "-1";
                }

                else
                {
                    myNameDimStyle = "1-" + scaleFactorCallout;
                }

                CmdDim.ChangeDimStyle(myNameDimStyle);

            }
        }

    }
}
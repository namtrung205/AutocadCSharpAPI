using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.PlottingServices;
using commonFunctions;

using Autodesk.AutoCAD.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Threading;



namespace myCustomCmds
{
    public class CmdPlot
    {
        // Lists the available page setups
        [CommandMethod("ListPageSetup")]
        public static void ListPageSetup()
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                DBDictionary plSettings = acTrans.GetObject(acCurDb.PlotSettingsDictionaryId,
                                                            OpenMode.ForRead) as DBDictionary;

                acDoc.Editor.WriteMessage("\nPage Setups: ");

                // List each named page setup
                foreach (DBDictionaryEntry item in plSettings)
                {
                    acDoc.Editor.WriteMessage("\n  " + item.Key);
                }

                // Abort the changes to the database
                acTrans.Abort();
            }
        }


        // Creates a new page setup or edits the page set if it exists
        [CommandMethod("CreateOrEditPageSetup")]
        public static void CreateOrEditPageSetup()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                DBDictionary plSets = acTrans.GetObject(acCurDb.PlotSettingsDictionaryId,
                                                        OpenMode.ForRead) as DBDictionary;
                DBDictionary vStyles = acTrans.GetObject(acCurDb.VisualStyleDictionaryId,
                                                         OpenMode.ForRead) as DBDictionary;

                PlotSettings acPlSet = default(PlotSettings);
                bool createNew = false;

                // Reference the Layout Manager
                LayoutManager acLayoutMgr = LayoutManager.Current;

                // Get the current layout and output its name in the Command Line window
                Layout acLayout = acTrans.GetObject(acLayoutMgr.GetLayoutId(acLayoutMgr.CurrentLayout),
                                                    OpenMode.ForRead) as Layout;

                // Check to see if the page setup exists
                if (plSets.Contains("MyPageSetup") == false)
                {
                    createNew = true;

                    // Create a new PlotSettings object: 
                    //    True - model space, False - named layout
                    acPlSet = new PlotSettings(acLayout.ModelType);
                    acPlSet.CopyFrom(acLayout);

                    acPlSet.PlotSettingsName = "MyPageSetup";
                    acPlSet.AddToPlotSettingsDictionary(acCurDb);
                    acTrans.AddNewlyCreatedDBObject(acPlSet, true);
                }
                else
                {
                    acPlSet = plSets.GetAt("MyPageSetup").GetObject(OpenMode.ForWrite) as PlotSettings;
                }

                // Update the PlotSettings object
                try
                {
                    PlotSettingsValidator acPlSetVdr = PlotSettingsValidator.Current;

                    // Set the Plotter and page size
                    acPlSetVdr.SetPlotConfigurationName(acPlSet, "DWF6 ePlot.pc3", "ANSI_B_(17.00_x_11.00_Inches)");

                    // Set to plot to the current display
                    if (acLayout.ModelType == false)
                    {
                        acPlSetVdr.SetPlotType(acPlSet, Autodesk.AutoCAD.DatabaseServices.PlotType.Layout);
                    }
                    else
                    {
                        acPlSetVdr.SetPlotType(acPlSet, Autodesk.AutoCAD.DatabaseServices.PlotType.Extents);

                        acPlSetVdr.SetPlotCentered(acPlSet, true);
                    }

                    // Use SetPlotWindowArea with PlotType.Window
                    //acPlSetVdr.SetPlotWindowArea(plSet,
                    //                             new Extents2d(New Point2d(0.0, 0.0),
                    //                             new Point2d(9.0, 12.0)));

                    // Use SetPlotViewName with PlotType.View
                    //acPlSetVdr.SetPlotViewName(plSet, "MyView");

                    // Set the plot offset
                    acPlSetVdr.SetPlotOrigin(acPlSet, new Point2d(0, 0));

                    // Set the plot scale
                    acPlSetVdr.SetUseStandardScale(acPlSet, true);
                    acPlSetVdr.SetStdScaleType(acPlSet, StdScaleType.ScaleToFit);
                    acPlSetVdr.SetPlotPaperUnits(acPlSet, PlotPaperUnit.Inches);
                    acPlSet.ScaleLineweights = true;

                    // Specify if plot styles should be displayed on the layout
                    acPlSet.ShowPlotStyles = true;

                    // Rebuild plotter, plot style, and canonical media lists 
                    // (must be called before setting the plot style)
                    acPlSetVdr.RefreshLists(acPlSet);

                    // Specify the shaded viewport options
                    acPlSet.ShadePlot = PlotSettingsShadePlotType.AsDisplayed;

                    acPlSet.ShadePlotResLevel = ShadePlotResLevel.Normal;

                    // Specify the plot options
                    acPlSet.PrintLineweights = true;
                    acPlSet.PlotTransparency = false;
                    acPlSet.PlotPlotStyles = true;
                    acPlSet.DrawViewportsFirst = true;
                    //acPlSet.CurrentStyleSheet;

                    

                    // Use only on named layouts - Hide paperspace objects option
                    // plSet.PlotHidden = true;

                    // Specify the plot orientation
                    acPlSetVdr.SetPlotRotation(acPlSet, PlotRotation.Degrees000);

                    // Set the plot style
                    if (acCurDb.PlotStyleMode == true)
                    {
                        acPlSetVdr.SetCurrentStyleSheet(acPlSet, "acad.ctb");
                    }
                    else
                    {
                        acPlSetVdr.SetCurrentStyleSheet(acPlSet, "acad.stb");
                    }

                    // Zoom to show the whole paper
                    acPlSetVdr.SetZoomToPaperOnUpdate(acPlSet, true);
                }
                catch (Autodesk.AutoCAD.Runtime.Exception es)
                {
                    System.Windows.Forms.MessageBox.Show(es.Message);
                }

                // Save the changes made
                acTrans.Commit();

                if (createNew == true)
                {
                    acPlSet.Dispose();
                }
            }
        }



        [CommandMethod("AssignPageSetupToLayout")]
        public static void AssignPageSetupToLayout()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Reference the Layout Manager
                LayoutManager acLayoutMgr = LayoutManager.Current;

                // Get the current layout and output its name in the Command Line window
                Layout acLayout = acTrans.GetObject(acLayoutMgr.GetLayoutId(acLayoutMgr.CurrentLayout),
                                                    OpenMode.ForRead) as Layout;

                DBDictionary acPlSet = acTrans.GetObject(acCurDb.PlotSettingsDictionaryId,
                                                         OpenMode.ForRead) as DBDictionary;

                // Check to see if the page setup exists
                if (acPlSet.Contains("MyPageSetup") == true)
                {
                    PlotSettings plSet = acPlSet.GetAt("MyPageSetup").GetObject(OpenMode.ForRead) as PlotSettings;

                    // Update the layout
                    acLayout.UpgradeOpen();
                    acLayout.CopyFrom(plSet);

                    // Save the new objects to the database
                    acTrans.Commit();
                }
                else
                {
                    // Ignore the changes made
                    acTrans.Abort();
                }
            }

            // Update the display
            acDoc.Editor.Regen();
        }



        [CommandMethod("PLO")]
        public static void PlotLayout()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Reference the Layout Manager
                LayoutManager acLayoutMgr = LayoutManager.Current;

                // Get the current layout and output its name in the Command Line window
                Layout acLayout = acTrans.GetObject(acLayoutMgr.GetLayoutId(acLayoutMgr.CurrentLayout),
                                                    OpenMode.ForRead) as Layout;
                acDoc.Editor.WriteMessage("Name of layout: {0}", acLayout.LayoutName);


                // Get the PlotInfo from the layout
                using (PlotInfo acPlInfo = new PlotInfo())
                {
                    acPlInfo.Layout = acLayout.ObjectId;

                    // Get a copy of the PlotSettings from the layout
                    using (PlotSettings acPlSet = new PlotSettings(acLayout.ModelType))
                    {
                        // Select a extents 2d
                        // SELECT A POLYLINE
                        ObjectId myObjId = myCustomFunctions.GetObjectIdByType("POLYLINE,LWPOLYLINE");

                        if (myObjId.ToString() == "0") return;
                        if (myObjId == new ObjectId()) return;

                        Polyline myPolySelected = myObjId.GetObject(OpenMode.ForWrite) as Polyline;

                        if (myPolySelected.NumberOfVertices < 2) return;

                        if (myPolySelected.Area == 0) return;

                        myPolySelected.Closed = true;

                        myPolySelected.removePointDup();

                        // GET EXTENTS POINT

                        Point2d myMinPoint2d = myPolySelected.GeometricExtents.MinPoint.Convert2d(new Plane());
                        Point2d myMaxPoint2d = myPolySelected.GeometricExtents.MaxPoint.Convert2d(new Plane());

                        Extents2d myExtents2d = new Extents2d(myMinPoint2d, myMaxPoint2d);



                        acPlSet.CopyFrom(acLayout);

                        // Update the PlotSettings object
                        PlotSettingsValidator acPlSetVdr = PlotSettingsValidator.Current;



                        // Set the plot type
                        //acPlSetVdr.SetPlotType(acPlSet, Autodesk.AutoCAD.DatabaseServices.PlotType.Extents);
                        
                        acPlSetVdr.SetPlotWindowArea(acPlSet, myExtents2d);
                        acPlSetVdr.SetPlotType(acPlSet, Autodesk.AutoCAD.DatabaseServices.PlotType.Window);

                        // Set the plot scale
                        acPlSetVdr.SetUseStandardScale(acPlSet, true);
                        acPlSetVdr.SetStdScaleType(acPlSet, StdScaleType.ScaleToFit);

                        // Center the plot
                        acPlSetVdr.SetPlotCentered(acPlSet, true);

                        // Set the plot device to use
                        acPlSetVdr.SetPlotConfigurationName(acPlSet, "DWG To PDF.pc3", "ISO_full_bleed_A3_(420.00_x_297.00_MM)");

                        // Set the plot info as an override since it will
                        // not be saved back to the layout
                        acPlInfo.OverrideSettings = acPlSet;

                        // Validate the plot info
                        using (PlotInfoValidator acPlInfoVdr = new PlotInfoValidator())
                        {
                            acPlInfoVdr.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
                            acPlInfoVdr.Validate(acPlInfo);

                            // Check to see if a plot is already in progress
                            if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
                            {
                                using (PlotEngine acPlEng = PlotFactory.CreatePublishEngine())
                                {
                                    // Track the plot progress with a Progress dialog
                                    using (PlotProgressDialog acPlProgDlg = new PlotProgressDialog(false, 1, true))
                                    {
                                        using ((acPlProgDlg))
                                        {
                                            // Define the status messages to display 
                                            // when plotting starts
                                            acPlProgDlg.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Plot Progress");
                                            acPlProgDlg.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                                            acPlProgDlg.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                                            acPlProgDlg.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                                            acPlProgDlg.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet Progress");

                                            // Set the plot progress range
                                            acPlProgDlg.LowerPlotProgressRange = 0;
                                            acPlProgDlg.UpperPlotProgressRange = 100;
                                            acPlProgDlg.PlotProgressPos = 0;

                                            // Display the Progress dialog
                                            acPlProgDlg.OnBeginPlot();
                                            acPlProgDlg.IsVisible = true;

                                            // Start to plot the layout
                                            acPlEng.BeginPlot(acPlProgDlg, null);

                                            // Define the plot output
                                            acPlEng.BeginDocument(acPlInfo, acDoc.Name, null, 1, true, "c:\\myplot");

                                            // Display information about the current plot
                                            acPlProgDlg.set_PlotMsgString(PlotMessageIndex.Status, "Plotting: " + acDoc.Name + " - " + acLayout.LayoutName);

                                            // Set the sheet progress range
                                            acPlProgDlg.OnBeginSheet();
                                            acPlProgDlg.LowerSheetProgressRange = 0;
                                            acPlProgDlg.UpperSheetProgressRange = 100;
                                            acPlProgDlg.SheetProgressPos = 0;

                                            // Plot the first sheet/layout
                                            using (PlotPageInfo acPlPageInfo = new PlotPageInfo())
                                            {
                                                acPlEng.BeginPage(acPlPageInfo, acPlInfo, true, null);
                                            }

                                            acPlEng.BeginGenerateGraphics(null);
                                            acPlEng.EndGenerateGraphics(null);

                                            // Finish plotting the sheet/layout
                                            acPlEng.EndPage(null);
                                            acPlProgDlg.SheetProgressPos = 100;
                                            acPlProgDlg.OnEndSheet();

                                            // Finish plotting the document
                                            acPlEng.EndDocument(null);

                                            // Finish the plot
                                            acPlProgDlg.PlotProgressPos = 100;
                                            acPlProgDlg.OnEndPlot();
                                            acPlEng.EndPlot(null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        public static void PlotLayoutMultilpleFunc(Polyline myPolySelected, string nameDraw)
        {
            // Get the current document and database, and start a transaction
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Reference the Layout Manager
                LayoutManager acLayoutMgr = LayoutManager.Current;

                // Get the current layout and output its name in the Command Line window
                Layout acLayout = acTrans.GetObject(acLayoutMgr.GetLayoutId(acLayoutMgr.CurrentLayout),
                                                    OpenMode.ForRead) as Layout;
                acDoc.Editor.WriteMessage("Name of layout: {0}", acLayout.LayoutName);


                // Get the PlotInfo from the layout
                using (PlotInfo acPlInfo = new PlotInfo())
                {
                    acPlInfo.Layout = acLayout.ObjectId;

                    // Get a copy of the PlotSettings from the layout
                    using (PlotSettings acPlSet = new PlotSettings(acLayout.ModelType))
                    {
                        // Select a extents 2d
                        // SELECT A POLYLINE

                        if (myPolySelected.NumberOfVertices < 2) return;

                        if (myPolySelected.Area == 0) return;

                        myPolySelected.Closed = true;

                        myPolySelected.removePointDup();

                        // GET EXTENTS POINT

                        Point2d myMinPoint2d = myPolySelected.GeometricExtents.MinPoint.Convert2d(new Plane());
                        Point2d myMaxPoint2d = myPolySelected.GeometricExtents.MaxPoint.Convert2d(new Plane());

                        Extents2d myExtents2d = new Extents2d(myMinPoint2d, myMaxPoint2d);



                        acPlSet.CopyFrom(acLayout);

                        // Update the PlotSettings object
                        PlotSettingsValidator acPlSetVdr = PlotSettingsValidator.Current;



                        // Set the plot type
                        //acPlSetVdr.SetPlotType(acPlSet, Autodesk.AutoCAD.DatabaseServices.PlotType.Extents);

                        acPlSetVdr.SetPlotWindowArea(acPlSet, myExtents2d);
                        acPlSetVdr.SetPlotType(acPlSet, Autodesk.AutoCAD.DatabaseServices.PlotType.Window);

                        // Set the plot scale
                        acPlSetVdr.SetUseStandardScale(acPlSet, true);
                        acPlSetVdr.SetStdScaleType(acPlSet, StdScaleType.ScaleToFit);

                        // Center the plot
                        acPlSetVdr.SetPlotCentered(acPlSet, true);

                        // Set the plot device to use
                        acPlSetVdr.SetPlotConfigurationName(acPlSet, "DWG To PDF.pc3", "ISO_full_bleed_A3_(420.00_x_297.00_MM)");

                        // Set the plot info as an override since it will
                        // not be saved back to the layout
                        acPlInfo.OverrideSettings = acPlSet;

                        // Validate the plot info
                        using (PlotInfoValidator acPlInfoVdr = new PlotInfoValidator())
                        {
                            acPlInfoVdr.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
                            acPlInfoVdr.Validate(acPlInfo);

                            // Check to see if a plot is already in progress
                            if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
                            {
                                using (PlotEngine acPlEng = PlotFactory.CreatePublishEngine())
                                {
                                    // Track the plot progress with a Progress dialog
                                    using (PlotProgressDialog acPlProgDlg = new PlotProgressDialog(false, 1, true))
                                    {
                                        using ((acPlProgDlg))
                                        {
                                            // Define the status messages to display 
                                            // when plotting starts
                                            acPlProgDlg.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Plot Progress");
                                            acPlProgDlg.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                                            acPlProgDlg.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                                            acPlProgDlg.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                                            acPlProgDlg.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet Progress");

                                            // Set the plot progress range
                                            acPlProgDlg.LowerPlotProgressRange = 0;
                                            acPlProgDlg.UpperPlotProgressRange = 100;
                                            acPlProgDlg.PlotProgressPos = 0;

                                            // Display the Progress dialog
                                            acPlProgDlg.OnBeginPlot();
                                            acPlProgDlg.IsVisible = true;

                                            // Start to plot the layout
                                            acPlEng.BeginPlot(acPlProgDlg, null);

                                            // Define the plot output
                                            string saveName = nameDraw;
                                            acPlEng.BeginDocument(acPlInfo, acDoc.Name, null, 1, true, saveName);

                                            // Display information about the current plot
                                            acPlProgDlg.set_PlotMsgString(PlotMessageIndex.Status, "Plotting: " + acDoc.Name + " - " + acLayout.LayoutName);

                                            // Set the sheet progress range
                                            acPlProgDlg.OnBeginSheet();
                                            acPlProgDlg.LowerSheetProgressRange = 0;
                                            acPlProgDlg.UpperSheetProgressRange = 100;
                                            acPlProgDlg.SheetProgressPos = 0;

                                            // Plot the first sheet/layout
                                            using (PlotPageInfo acPlPageInfo = new PlotPageInfo())
                                            {
                                                acPlEng.BeginPage(acPlPageInfo, acPlInfo, true, null);
                                            }

                                            acPlEng.BeginGenerateGraphics(null);
                                            acPlEng.EndGenerateGraphics(null);

                                            // Finish plotting the sheet/layout
                                            acPlEng.EndPage(null);
                                            acPlProgDlg.SheetProgressPos = 100;
                                            acPlProgDlg.OnEndSheet();

                                            // Finish plotting the document
                                            acPlEng.EndDocument(null);

                                            // Finish the plot
                                            acPlProgDlg.PlotProgressPos = 100;
                                            acPlProgDlg.OnEndPlot();
                                            acPlEng.EndPlot(null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        [CommandMethod("MPL")]
        public static void autoMutilPlotPolyline()
        {// Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                //BlockTable acBlkTbl;
                //BlockTableRecord acBlkTblRec;

                //// Open Model space for write
                //acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                //                                OpenMode.ForRead) as BlockTable;

                //if (Application.GetSystemVariable("CVPORT").ToString() != "1")
                //{
                //    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                //                                OpenMode.ForWrite) as BlockTableRecord;
                //}
                //else
                //{
                //    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace],
                //            OpenMode.ForWrite) as BlockTableRecord;
                //}

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE,LWPOLYLINE"), 0);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<Polyline> myListPolyValid = new List<Polyline>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Polyline myPolylineItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as Polyline;

                        if (myPolylineItem.Area > 0 && myPolylineItem.NumberOfVertices > 2)
                        {
                            myListPolyValid.Add(myPolylineItem);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    int i = 1;
                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        string nameDraw = "c:\\" + i;

                        PlotLayoutMultilpleFunc(myPoly, nameDraw);
                        int milliseconds = 20000;
                        Thread.Sleep(milliseconds);
                    }
                }
                acTrans.Commit();
                return;
            }
        }

    }
}







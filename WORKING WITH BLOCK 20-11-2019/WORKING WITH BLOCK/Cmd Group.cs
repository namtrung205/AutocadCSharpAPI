using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
//
using commonFunctions;


namespace myCustomCmds
{
    public class CmdGroup
    {
        [CommandMethod("FindGroup")]
        static public void FindGroup()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            PromptEntityResult acSSPrompt = ed.GetEntity("Select the entity to find the group");
            if (acSSPrompt.Status != PromptStatus.OK) return;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                Entity ent = acTrans.GetObject(acSSPrompt.ObjectId, OpenMode.ForRead) as Entity;
                ObjectIdCollection ids = ent.GetPersistentReactorIds();
                bool bPartOfGroup = false;
                foreach (ObjectId id in ids)
                {
                    DBObject obj = acTrans.GetObject(id, OpenMode.ForRead);
                    if (obj is Group)
                    {
                        Group group = obj as Group;
                        bPartOfGroup = true;
                        ed.WriteMessage("Entity is part of " + group.Name + " group\n");
                    }
                }
                if (!bPartOfGroup) ed.WriteMessage("Entity is Not part of any group\n");
                acTrans.Commit();
            }
        }
    }
}



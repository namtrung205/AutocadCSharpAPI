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
using myForm;


namespace myCustomCmds
{
    public class CmdDimForm
    {
        /// NHÓM HÀM DIMENSION
        [CommandMethod("DXF")]
        public static void DLICustomCallFromForm()
        {
            myForm01 testForm1 = new myForm01();

            Application.ShowModelessDialog(testForm1);
        }

    }

}
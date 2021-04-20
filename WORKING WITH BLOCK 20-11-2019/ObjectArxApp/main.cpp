// Load the common Windows headers
#include <windows.h>
// Load the common AutoCAD headers
#include "arxHeaders.h"
#include "dbents.h"

#include "Tchar.h"
#include "acdb.h"


static void openDWGFile()
{
    const ACHAR* pszDrawingName = L"‪D:\M.dwg";
    try
    {
        AcDbDatabase acDb = new AcDbDatabase(false, true);
        Acad::ErrorStatus es = acDb.readDwgFile(pszDrawingName, AcDbDatabase::kForReadAndWriteNoShare);

        if(es == Acad::ErrorStatus::eOk)
        {
            acutPrintf(_T("\nOpen RealDWG Ok...\n"));
        
        }

    }
    catch (const std::exception&)
    {

    }
}


extern "C" AcRx::AppRetCode acrxEntryPoint(AcRx::AppMsgCode msg, void* pkt)
{
    switch (msg)
    {
        case AcRx::kInitAppMsg:
            acrxDynamicLinker->unlockApplication(pkt);
            acrxDynamicLinker->registerAppMDIAware(pkt);
            acutPrintf(_T("\nLoading AU 2019 project...\n"));

            // Commands to add
            acedRegCmds->addCommand(_T("AUCommands"), _T("Uno"), _T("OpenCMD"), ACRX_CMD_MODAL, openDWGFile);



            break;
        case AcRx::kUnloadAppMsg:
            acutPrintf(_T("\nUnloading AU 2019 project...\n"));
            break;
        default:
            break;
    }
    return AcRx::kRetOK;
}



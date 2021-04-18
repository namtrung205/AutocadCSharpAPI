// Load the common Windows headers
#include <windows.h>
// Load the common AutoCAD headers
#include "arxHeaders.h"
#include "dbents.h"

#include "Tchar.h"



static void Greetings()
{
    acutPrintf(_T("\nHello AU 2011!!!"));
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

            acedRegCmds->addCommand(_T("AUCommands"), _T("Uno"), _T("First"), ACRX_CMD_MODAL, Greetings);



            break;
        case AcRx::kUnloadAppMsg:
            acutPrintf(_T("\nUnloading AU 2019 project...\n"));
            break;
        default:
            break;
    }
    return AcRx::kRetOK;
}



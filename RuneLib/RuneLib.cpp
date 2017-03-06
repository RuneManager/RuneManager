// InteropDll.cpp : Defines the exported functions for the DLL application.
//

#include "RuneLib.h"


// This is an example of an exported variable
RUNELIB_API int nRuneLib=0;

// This is an example of an exported function.
RUNELIB_API int fnRuneLib(void)
{
    return 42;
}

// This is the constructor of a class that has been exported.
// see InteropDll.h for the class definition
CRuneLib::CRuneLib()
{
    return;
}

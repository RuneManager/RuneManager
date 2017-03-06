// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the RUNELIB_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// RUNELIB_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef RUNELIB_EXPORTS
#define RUNELIB_API __declspec(dllexport)
#else
#define RUNELIB_API __declspec(dllimport)
#endif

// This class is exported from the InteropDll.dll
class RUNELIB_API CRuneLib {
public:
	CRuneLib(void);
	// TODO: add your methods here.
};

extern RUNELIB_API int nInteropDll;

RUNELIB_API int fnInteropDll(void);

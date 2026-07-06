#include <crtdbg.h>
#include <stdio.h>

extern "C" _declspec(dllexport) void disable_assertions(void) {
	// Removes _CRTDBG_MODE_WNDW. Public issue #5195.
	// (Steam assertion blocking server on Windows.)
	// https://learn.microsoft.com/en-ca/cpp/c-runtime-library/reference/crtsetreportmode
	_CrtSetReportMode(_CRT_WARN, _CRTDBG_MODE_FILE | _CRTDBG_MODE_DEBUG);
	_CrtSetReportMode(_CRT_ERROR, _CRTDBG_MODE_FILE | _CRTDBG_MODE_DEBUG);
	_CrtSetReportMode(_CRT_ASSERT, _CRTDBG_MODE_FILE | _CRTDBG_MODE_DEBUG);
	printf("Applied disable_assertions experiment\n");
}

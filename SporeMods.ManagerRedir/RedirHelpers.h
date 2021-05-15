/*#include <PathCch.h>
#include <Shlwapi.h>
#include <stdio.h>
#include <stdlib.h>*/
#include <iostream>
#include <Windows.h>
#include <Shlwapi.h>
#include <PathCch.h>
#include <VersionHelpers.h>
#include <processthreadsapi.h>
#include <processenv.h>
#include <shellapi.h>
#include <string>
#include <vector>
#include <strsafe.h>
#include <comdef.h>
#include <stdlib.h>
#include <string>
#include <set>
#include <algorithm>
#include <cwctype>

#include "atlbase.h"
#include "atlstr.h"
#include "comutil.h"

#pragma warning(disable : 4995)
#pragma comment(lib, "Shlwapi.lib")
#pragma comment(lib, "Pathcch.lib")

#define UNICODE
#define _UNICODE
#pragma once
class RedirHelpers
{
public:
	static std::wstring CombinePaths(std::wstring path1C, std::wstring path2C);
	/*static std::wstring CombinePaths(wchar_t* path1, const wchar_t* path2);
	static std::wstring CombinePaths(const wchar_t* path1, const wchar_t* path2);*/
	
	static std::wstring GetParentOfPath(LPWSTR path);

	static bool StartsWith(std::wstring strCompareTo, std::wstring startsWithThis);
	static bool Contains(std::wstring strCompareTo, std::wstring containsThis);
};


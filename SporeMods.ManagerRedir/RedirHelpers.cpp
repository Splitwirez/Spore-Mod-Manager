#include "RedirHelpers.h"

std::wstring RedirHelpers::CombinePaths(std::wstring path1C, std::wstring path2C)
{
    auto path1 = path1C.c_str();
    auto path2 = path2C.c_str();

    //printf("path1: %ws\npath2: %ws\n", path1, path2);


    wchar_t combinedPath[MAX_PATH];
    wcsncpy_s(combinedPath, path1, lstrlen(path1)); // wcsncpy_s(combinedPath, path1, lstrlen(path1));


    BOOL pathCombineResult = false;
    if (IsWindows8OrGreater())
        pathCombineResult = PathCchCombine(combinedPath, sizeof(combinedPath), path1, path2);
    else
    {
        LPTSTR what = PathCombine(combinedPath, path1, path2);
        pathCombineResult = what == NULL;
    }
    //printf("combinedPath: %ws\n\n", combinedPath);
    return std::wstring(combinedPath);

    //StringCchCopy(output, sizeof(output), combinedPath);
    //wcscpy_s(*output, combinedPath); //return combinedPath;
}

/*std::wstring RedirHelpers::CombinePaths(wchar_t* path1, const wchar_t* path2)
{
    wchar_t path2NonConst = *path2;
    return CombinePaths(path1, &path2NonConst);
}

std::wstring RedirHelpers::CombinePaths(const wchar_t* path1, const wchar_t* path2)
{
    wchar_t path1NonConst = *path1;
    return CombinePaths(&path1NonConst, path2);
}*/


std::wstring RedirHelpers::GetParentOfPath(LPWSTR path)
{
    //printf("path: %ws\n", path);

    wchar_t parentPath[MAX_PATH];
    wcsncpy_s(parentPath, path, lstrlen(path));


    BOOL pathRemoveFileSuccess = 0;
    if (IsWindows8OrGreater())
        pathRemoveFileSuccess = PathCchRemoveFileSpec(parentPath, sizeof(parentPath)) == S_OK;
    else
        pathRemoveFileSuccess = PathRemoveFileSpec(parentPath);
    //printf("parentPath: %ws\n\n", parentPath);


    return std::wstring(parentPath);
    
    //StringCchCopy(output, sizeof(output), parentPath); //wcsncpy_s(output, parentPath, lstrlen(parentPath)); //return combinedPath; //return parentPath;
}

bool RedirHelpers::StartsWith(std::wstring strCompareTo, std::wstring startsWithThis)
{
    auto startMatch = std::search(strCompareTo.begin(), strCompareTo.end(), startsWithThis.begin(), startsWithThis.end(), [&](wchar_t char1, wchar_t char2)
    {
        return _wcsicmp(&char1, &char2) == 0;
    });

    return startMatch._Ptr == strCompareTo.c_str();
}

bool RedirHelpers::Contains(std::wstring strCompareTo, std::wstring containsThis)
{
    auto startMatch = std::search(strCompareTo.begin(), strCompareTo.end(), containsThis.begin(), containsThis.end(), [&](wchar_t char1, wchar_t char2)
        {
            return _wcsicmp(&char1, &char2) == 0;
        });

    return startMatch != strCompareTo.end();
}
// SporeMods.ManagerRedir.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <Windows.h>
#include <Shlwapi.h>
#include <PathCch.h>
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
#include <sstream>
#include <fstream>
#include <codecvt>

#include "atlbase.h"
#include "atlstr.h"
#include "comutil.h"
#include "RedirHelpers.h"

#pragma warning(disable : 4995)
#pragma comment(lib, "Shlwapi.lib")
#pragma comment(lib, "Pathcch.lib")

#define UNICODE
#define _UNICODE
#define CMD_OPTIONS

int wmain(int argc,      //argv count
	wchar_t* argv[],   //command-line options
	wchar_t* envp[])  //environment variables
{
	const int bigLength = 32767;

	const wchar_t* nullStandInChar = L"\uE001";
	const wchar_t* realNullChar = L"\u0000";

	WCHAR cmdOptions[bigLength];
	std::wstring EMPTY_STR = std::wstring(L"");
	wchar_t* lpCommandLine = NULL;
	std::wstring lpCurrentDirectoryC;
	LPCWSTR lpApplicationName = NULL;
	std::wstring lpApplicationNameC;


	const wchar_t* internalFolderName = L"Internal";
	const wchar_t* dotnetRuntimeFolderName = L"Runtime";

	bool hasArgs = false;
	bool allPathsLocatedSuccessfully = false;


	wchar_t thisExePath[MAX_PATH];


	bool runUpdater = false;

	DWORD moduleFileNameGot = GetModuleFileName(NULL, thisExePath, MAX_PATH);
	if ((moduleFileNameGot == 0) || (moduleFileNameGot == ERROR_INSUFFICIENT_BUFFER))
	{
		printf("\n\nGetModuleFileName failed!");
	}

	auto smmInstallFolderPathC = RedirHelpers::GetParentOfPath(thisExePath);
	auto smmInstallFolderPath = smmInstallFolderPathC.c_str();

	auto internalFolderPathC = RedirHelpers::CombinePaths(smmInstallFolderPathC, L"Internal");
	auto internalFolderPath = internalFolderPathC.c_str();

	auto runtimePathC = RedirHelpers::CombinePaths(smmInstallFolderPathC, L"Runtime");
	auto runtimePath = runtimePathC.c_str();
	std::wstring dotnetRuntimePath = std::wstring(runtimePath);
#ifdef CMD_OPTIONS
	if (argc > 0)
	{
		hasArgs = true;
		wcscpy_s(cmdOptions, L"");
		for (int argIndex = 0; argIndex < argc; argIndex++)
		{
			if (argIndex > 0)
				wcscat_s(cmdOptions, L" ");

			auto currentArg = argv[argIndex];
			if (_wcsicmp(currentArg, L"--elevateUpdater") == 0)
				runUpdater = true;

			bool hasSpaces = false;
			int argLength = 0;

			for (int charIndex = 0; charIndex < bigLength; charIndex++)
			{
				if (currentArg[charIndex] == L'\0')
				{
					argLength = charIndex;
					break;
				}
			}

			for (int charIndex = 0; charIndex < argLength; charIndex++)
			{
				if (currentArg[charIndex] == L' ')
				{
					hasSpaces = true;
					break;
				}
			}

			if (hasSpaces)
				wcscat_s(cmdOptions, L"\"");


			wcscat_s(cmdOptions, currentArg);

			if (hasSpaces)
				wcscat_s(cmdOptions, L"\"");
		}
		wcscat_s(cmdOptions, L"\0");
	}
	if (hasArgs)
		lpCommandLine = cmdOptions;
#endif

#if LAUNCHER
	PCWSTR executableName = L"SporeMods.Launcher.exe";
#elif LKIMPORT
	PCWSTR executableName = L"SporeMods.KitImporter.exe";
#else
	PCWSTR executableName = L"SporeMods.Manager.exe";
#endif
	
	if (runUpdater)
	{
		wchar_t programData[MAX_PATH];
		//wchar_t storageDirPath[MAX_PATH];
		//ReadFile(programData, )
		//, buf, buf_size);

		GetEnvironmentVariable(L"ProgramData", programData, MAX_PATH);
		//printf("programData: %ws", programData);
		//MessageBoxW(nullptr, programData, L"programData", MB_OK);
		//std::wstring programDataPath = std::wstring()
		std::wstring storagePath = RedirHelpers::CombinePaths(programData, L"SporeModManagerStorage");


		auto redirectStorageFilePath = RedirHelpers::CombinePaths(storagePath.c_str(), L"redirectStorage.txt");
		auto redirectStorageFilePathC = redirectStorageFilePath.c_str();
		MessageBoxW(nullptr, redirectStorageFilePathC, L"redirectStorageFilePathC", MB_OK);


		if (PathFileExistsW(redirectStorageFilePathC))
		{
			std::wifstream redirectStorage;
			redirectStorage.imbue(std::locale(std::locale::empty(), new std::codecvt_utf8<wchar_t>));
			redirectStorage.open(redirectStorageFilePath, std::wios::in);
			std::wstringstream pathSStream;
			pathSStream << redirectStorage.rdbuf();
			storagePath = pathSStream.str();

			MessageBoxW(nullptr, storagePath.c_str(), L"new storagePath", MB_OK);
		}
		lpCurrentDirectoryC = storagePath;

		lpApplicationNameC = RedirHelpers::CombinePaths(RedirHelpers::CombinePaths(storagePath, L"Temp"), L"smmUpdater.exe");
		MessageBoxW(nullptr, lpApplicationNameC.c_str(), L"lpApplicationNameC", MB_OK);
	}
	else
	{
		lpApplicationNameC = RedirHelpers::CombinePaths(internalFolderPath, executableName);
		lpCurrentDirectoryC = internalFolderPathC;
	}
	
	lpApplicationName = lpApplicationNameC.c_str();
	
	allPathsLocatedSuccessfully = true;

	auto environmentParameter = envp;

	std::vector<std::wstring> environmentVariables;
	int varIndex = 0;

	while (*environmentParameter)
	{
		auto envVarW = std::wstring(*environmentParameter);
		environmentVariables.push_back(envVarW);

		environmentParameter++;
		varIndex++;
	}

	std::wstring dotnetRootName = std::wstring(L"DOTNET_ROOT=");
	bool dotnetRootSet = false;
	std::wstring newDotnetRoot = std::wstring();
	newDotnetRoot.append(dotnetRootName);
	newDotnetRoot.append(dotnetRuntimePath);

	std::wstring dotnetRootx86Name = std::wstring(L"DOTNET_ROOT(x86)=");
	bool dotnetRootx86Set = false;
	std::wstring newDotnetRootx86 = std::wstring();
	newDotnetRootx86.append(dotnetRootx86Name);
	newDotnetRootx86.append(dotnetRuntimePath);

	std::wstring dotnetMultilevelLookupName = std::wstring(L"DOTNET_MULTILEVEL_LOOKUP=");
	bool dotnetMultilevelLookupSet = false;
	std::wstring newDotnetMultilevelLookup = std::wstring();
	newDotnetMultilevelLookup.append(dotnetMultilevelLookupName);
	newDotnetMultilevelLookup.append(L"0");

	std::wstring pathName = std::wstring(L"Path=");
	bool pathSet = false;

	for (int envVarIndex = 0; envVarIndex < environmentVariables.size(); envVarIndex++)
	{
		auto currentEnvVar = environmentVariables.at(envVarIndex);

		if (RedirHelpers::StartsWith(currentEnvVar, dotnetRootName))
		{
			environmentVariables.at(envVarIndex) = newDotnetRoot;
			dotnetRootSet = true;
		}
		else if (RedirHelpers::StartsWith(currentEnvVar, dotnetRootx86Name))
		{
			environmentVariables.at(envVarIndex) = newDotnetRootx86;
			dotnetRootx86Set = true;
		}
		else if (RedirHelpers::StartsWith(currentEnvVar, dotnetMultilevelLookupName))
		{
			environmentVariables.at(envVarIndex) = newDotnetMultilevelLookup;
			dotnetMultilevelLookupSet = true;
		}
		else if (RedirHelpers::StartsWith(currentEnvVar, pathName))
		{
			std::wstring oldEnvVarVal = currentEnvVar.substr(pathName.length(), std::wstring::npos);

			if (!RedirHelpers::StartsWith(oldEnvVarVal, dotnetRuntimePath))
			{
				std::wstring newEnvVarVal = std::wstring();
				newEnvVarVal.append(pathName);
				newEnvVarVal.append(dotnetRuntimePath);
				newEnvVarVal.append(std::wstring(L";"));
				newEnvVarVal.append(oldEnvVarVal);

				environmentVariables.at(envVarIndex) = newEnvVarVal;
			}

			pathSet = true;
		}
	}

	if (!dotnetRootSet)
	{
		environmentVariables.push_back(newDotnetRoot);
	}

	if (!dotnetRootx86Set)
	{
		environmentVariables.push_back(newDotnetRootx86);
	}

	if (!dotnetMultilevelLookupSet)
	{
		environmentVariables.push_back(newDotnetMultilevelLookup);
	}

	if (!pathSet)
	{
		std::wstring newEnvVarVal = std::wstring();
		newEnvVarVal.append(pathName);
		newEnvVarVal.append(dotnetRuntimePath);

		environmentVariables.push_back(newEnvVarVal);
	}


	std::sort(environmentVariables.begin(), environmentVariables.end(), [&](std::wstring str1, std::wstring str2)
	{
		return str1 < str2;
	});



	printf("\nbegin concatenation\n");

	int envVarsCount = environmentVariables.size();
	std::wstring lpEnvW = std::wstring();

	if (envVarsCount > 0)
	{
		for (int envVarIndex = 0; envVarIndex < envVarsCount; envVarIndex++)
		{
			auto currentEnvVarW = environmentVariables.at(envVarIndex);
			auto currentEnvVar = currentEnvVarW.c_str();

			lpEnvW.append(currentEnvVarW);
			lpEnvW.append(nullStandInChar);
		}
	}
	else
	{
		lpEnvW.append(nullStandInChar);
	}

	lpEnvW.append(nullStandInChar);

	auto lpEnvC = lpEnvW.c_str();
	


	printf("lpEnvC: %ws\n", lpEnvC);

	
	int lpEnvSize = lpEnvW.length();

	wchar_t lpEnvironment[bigLength];
	

	if (false)
	{
		int maxCharLength = 0;
		int minCharLength = INT_MAX;

		for (int charIndex = 0; charIndex < lpEnvSize; charIndex++)
		{
			auto charAt = lpEnvW.at(charIndex);
			int size1 = sizeof(charAt);

			if (size1 < minCharLength)
				minCharLength = size1;

			if (size1 > maxCharLength)
				maxCharLength = size1;
		}

		printf("\nmaxCharLength: %i\nminCharLength: %i\n", maxCharLength, minCharLength);
	}

	printf("\nlpEnvSize: %u\n", lpEnvSize);

	bool lastByteZero = false;
	int zeroByteCount = 0;

	bool doubleNullTermFound = false;

	int lastZeroCharIndex = -4;

	for (int i = 0; i < lpEnvSize; i++)
	{
		if (lpEnvC[i] == *nullStandInChar)
		{
			printf("Swapping null stand-in char at %i for real null char\n", i);
			lpEnvironment[i] = *realNullChar;

			if (lastZeroCharIndex == (i - 1))
			{
				printf("Double null term found at %i and %i", lastZeroCharIndex, i);
				lastZeroCharIndex = i;
				break;
			}
			else
			{
				lastZeroCharIndex = i;
			}
		}
		else
			lpEnvironment[i] = lpEnvC[i];
	}


	STARTUPINFOW startupInfo;
	SecureZeroMemory(&startupInfo, sizeof(STARTUPINFOW));

	startupInfo.cb = sizeof(STARTUPINFOW);

	PROCESS_INFORMATION processInfo;

	printf("\nlpApplicationName: %ws\nlpCommandLine: %ws\n", lpApplicationName, lpCommandLine);


	if (CreateProcess(lpApplicationName, lpCommandLine, NULL, NULL, TRUE, CREATE_UNICODE_ENVIRONMENT,
		lpEnvironment
		, lpCurrentDirectoryC.c_str(), &startupInfo, &processInfo)) // NULL, NULL, NULL, TRUE, 0, NULL, NULL, startupInfo, processInfo))
	{
		int pid = processInfo.dwProcessId;
		CloseHandle(processInfo.hProcess);
		return pid;
	}
	else
		printf("\n\nCreateProcessW failed!");

	printf("\nLast error: %i\n", GetLastError());
	//system("pause");
	return -1;
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file

@echo off
SetLocal EnableDelayedExpansion


Set "BuildTypeOutPath=.\src\BuildType.txt"
If ["%~1"]==[""] (
	echo %~1 > %BuildTypeOutPath%
)

Set "BinDir=.\bin"
Set "UnpackagedBinDir=.\build\unpackagedBin"


CALL :EXPANDPATH %UnpackagedBinDir%\selfContained\
Set UnpackagedOut=%RetExpandedPath%
CALL :EXPANDPATH %UnpackagedBinDir%\frameworkDependent\
Set UnpackagedOutFD=%RetExpandedPath%


::Delet output folders
Set BinParam="%BinDir%"
IF exist %BinParam% (rd /S /Q %BinParam%)
IF exist %BinParam% (
	Set "ErrorText=Could not clear bin folder"
	GOTO FAIL
)

Set UnpackagedBinParam="%UnpackagedBinDir%"
IF exist %UnpackagedBinParam% (rd /S /Q %UnpackagedBinParam%)
IF exist %UnpackagedBinParam% (
	Set "ErrorText=Could not clear unpackagedBin folder"
	GOTO FAIL
)

::Render app icons if needed (this step does not occur if they already exist, as the icons are unlikely to change often enough to justify waiting for rendering when building every release)
Set "AppIconsDir=.\build\AppIcons\"
set MMIco=ModManagerIcon
IF NOT EXIST %AppIconsDir%%MMIco%.ico (start /b /wait "" cmd.exe /c ".\build\generate-icon.bat" %MMIco%)

set MLIco=ModLauncherIcon
IF NOT EXIST %AppIconsDir%%MLIco%.ico (start /b /wait "" cmd.exe /c ".\build\generate-icon.bat" %MLIco%)

set MMSIco=ModManagerSetupIcon
IF NOT EXIST %AppIconsDir%%MMSIco%.ico (start /b /wait "" cmd.exe /c ".\build\generate-icon.bat" %MMSIco%)


::Build the Spore Mod Manager and co.

		::We're actually going to build these twice - first self-contained
	::(which is a direct superset of what we actually want), and secondly
	::framework-dependent, so we can keep only the files from the first build's
	::output which exist in both.
	
		::By doing this, we're left with a self-contained build that's missing
	::all of the runtime files - as though it's missing its...self-containedness.
	
		::This may seem useless, but remember, the SMM's Installer includes
	::those same runtime files, all rolled into one singular file. That file is
	::a Windows executable.
	
		::As luck happens, .NET Core 3.1 achieves this by having that singular
	::Windows executable extract all of those runtime files to a temporary folder,
	::along with the binaries specific to the program itself, and then runs the
	::program from that temporary folder. In effect, it's a glorified
	::self-extracting archive with extra automated steps. That's good news for us.
	
		::See, in addition to the above, there's also a built-in .NET API to find
	::out where that temporary folder is. As such, now that we've stripped our
	::self-contained Spore Mod Manager binaries of their self-containedness, all
	::we have to do to restore them to their former self-sufficiency is copy the
	::runtime files from the temporary folder into the install directory along
	::with the Spore Mod Manager binaries themselves, and suddenly we've
	::reconstructed a fully complete, fully self-contained build of the Spore Mod
	::Manager on the other side of the exchange, like we'd never deleted anything
	::here in the first place!
		::So why did we do all of that? Because now the Installer we give to the
	::user to download is 60MB smaller. That's about a 25% weight loss!
set R2R=-p:PublishReadyToRun=true
set BuildConfig=-c Release

echo Building self-contained...
set "PublishDir=%UnpackagedOut%"
set PublishParams=%BuildConfig% -o "%PublishDir%"
CALL :BUILDMAINPROJECTS

echo Building framework-dependent...
set "PublishDir=%UnpackagedOutFD%"
set PublishParams=%BuildConfig% --self-contained false -o "%PublishDir%"
CALL :BUILDMAINPROJECTS


::This is the part where we actually delet all of the runtime files, as previously described

		::This is achieved by deleting all files in the self-contained output
	::folder, which aren't also present in the framework-dependent output folder.
For /R "%UnpackagedOut%" %%x In (*.*) Do (
	
	Set "OriginalPath=%%x"
	CALL :GETFILENAME "!OriginalPath!"
	Set "FileName=!RetFileName!"
	Set "FdPath=%UnpackagedOutFD%!FileName!"
	
	
	If Not Exist "!FdPath!" (
		echo DELET !FileName!
		del "!OriginalPath!"
    )
)


::Build the Spore Mod Manager's Updater
::dotnet publish .\Installer\SporeMods.Setup %BuildConfig% --self-contained true -p:PublishSingleFile=false -p:PublishReadyToRun=true -r win-x86 -o .\bin\Updater
::if not errorlevel 0 (
	::Set "ErrorText=updater"
	::GOTO BUILDFAIL
::)


::Build the Spore Mod Manager's Installer
Set "IstErr=Installer"

CALL :EXPANDPATH .\bin\SMMSBIN\
Set "SmmsBin=%RetExpandedPath%"

CALL :EXPANDPATH .\bin\SporeModManager\
Set "SporeModManagerSetupBin=%RetExpandedPath%"

dotnet publish .\src\Installer\SporeMods.Setup %BuildConfig% --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -r win-x86 -o "%SmmsBin%"
if not errorlevel 0 (
	Set "ErrorText=%IstErr%"
	GOTO BUILDFAIL
)

Set "SmmsExe=%SmmsBin%SporeModManagerSetup.exe"
if Not Exist "%SmmsExe%" (
	Set "errorlevel=-1"
	Set "ErrorText=%IstErr%"
	GOTO BUILDFAIL
)


::Delet temp files from framework-dependent vs self-contained compare
echo Removing temporary files...
rd /S /Q "%UnpackagedOutFD%"



::Build the upgrade bootstrapper for the Spore ModAPI Launcher Kit
Set "BtsErr=Launcher Kit upgrade bootstrapper"

CALL :EXPANDPATH .\bin\LKBBIN\
Set "LkbBin=%RetExpandedPath%"

CALL :EXPANDPATH .\bin\LauncherKitBootstrapper
Set "LKBootstrapper=%RetExpandedPath%"

dotnet build .\src\Installer\SporeMods.LauncherKitBootstrapper %BuildConfig% -p:PublishReadyToRun=true -o "%LkbBin%"
if not errorlevel 0 (
	Set "ErrorText=%BtsErr%"
	GOTO BUILDFAIL
)

Set "LkbExe=%LkbBin%ModAPIUpdateSetup.exe"
if Not Exist "%LkbExe%" (
	Set "errorlevel=-1"
	Set "ErrorText=%BtsErr%"
	GOTO BUILDFAIL
)

".\build\7za.exe" a "%LKBootstrapper%\SporeModManagerSetup.zip" "%SmmsExe%"



::Drop the final binaries into obvious folders - Splitwirez absolutely cannot be trusted to remember all the files otherwise, and the cost of human error is far too high
md "%SporeModManagerSetupBin%"
move /y "%SmmsExe%" "%SporeModManagerSetupBin%SporeModManagerSetup.exe"
if not errorlevel 0 (
	Set "ErrorText=%IstErr%"
	GOTO COPYBINFAIL
)
	
	
move /y "%LkbExe%" "%LKBootstrapper%"
if not errorlevel 0 (
	Set "ErrorText=%BtsErr%"
	GOTO COPYBINFAIL
)


::Delet setup and Launcher Kit bootstrapper temp files
Set TempDelTotal=2
echo Removing %TempDelTotal% temporary files...

echo 1/%TempDelTotal%
rd /S /Q "%SmmsBin%"
echo 2/%TempDelTotal%
rd /S /Q "%LkbBin%"


if errorlevel 0 GOTO SUCCESS
if not errorlevel 0 GOTO FAIL



::You're winner
:SUCCESS
echo ...
echo Spore Mod Manager built successfully.
exit 0



::Task failed successfully

:BUILDFAIL
Set "ErrorText=Failed to build the %ErrorText%"
GOTO FAIL

:COPYBINFAIL
Set "ErrorText=Failed to copy the %ErrorText%"
GOTO FAIL

:FAIL
echo ...
echo Spore Mod Manager failed to build.
echo ...
echo %ErrorText%.
echo ...
CALL :EXPANDPATH .\build\buildLog.txt
echo All output from this build attempt has been saved to:
echo '%RetExpandedPath%'
GOTO EXIT

:EXIT
IF exist %BuildTypeOutPath% (del %BuildTypeOutPath%)
exit errorlevel



:BUILDMAINPROJECTS
dotnet publish .\src\SporeMods.Launcher %PublishParams%
if not errorlevel 0 (
	Set "ErrorText=Launcher"
	GOTO BUILDFAIL
)
if Not Exist "%PublishDir%Launch Spore.exe" (
	Set "errorlevel=-1"
	Set "ErrorText=Launcher"
	GOTO BUILDFAIL
)

dotnet publish .\src\SporeMods.UacMessenger %PublishParams%
if not errorlevel 0 (
	Set "ErrorText=User Account Control Messenger"
	GOTO BUILDFAIL
)
if Not Exist "%PublishDir%xUacMessenger.exe" (
	Set "errorlevel=-1"
	Set "ErrorText=User Account Control Messenger"
	GOTO BUILDFAIL
)

dotnet publish .\src\SporeMods.Manager %PublishParams%
if not errorlevel 0 (
	Set "ErrorText=Spore Mod Manager"
	GOTO BUILDFAIL
)
if Not Exist "%PublishDir%Spore Mod Manager.exe" (
	Set "errorlevel=-1"
	Set "ErrorText=Spore Mod Manager"
	GOTO BUILDFAIL
)

dotnet publish .\src\SporeMods.KitImporter %PublishParams%
if not errorlevel 0 (
	Set "ErrorText=Launcher Kit Importer"
	GOTO BUILDFAIL
)
if Not Exist "%PublishDir%xLauncherKitImport.exe" (
	Set "errorlevel=-1"
	Set "ErrorText=Launcher Kit Importer"
	GOTO BUILDFAIL
)

EXIT /B

::https://stackoverflow.com/questions/1645843/resolve-absolute-path-from-relative-path-and-or-file-name/33404867#33404867
:GETFILENAME
SET RetFileName=%~nx1
EXIT /B

:EXPANDPATH
SET RetExpandedPath=%~f1
EXIT /B



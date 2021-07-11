@echo off
SetLocal EnableDelayedExpansion

CALL :EXPANDPATH .\unpackagedBin\SelfContained\
Set UNPACKAGEDOUT=%RETVAL%
CALL :EXPANDPATH .\unpackagedBin\FrameworkDependent\
Set UNPACKAGEDFDOUT=%RETVAL%


::Clear output folders
IF exist ".\bin" (rd /S /Q ".\bin")
IF exist ".\unpackagedBin" (rd /S /Q ".\unpackagedBin")

::Ensure output folders still exist
::md ".\unpackagedBin"
::md ".\unpackagedBin\Release"
::md ".\bin"
::md ".\bin\SMM"
::md ".\bin\SMLK"


::Render app icons if needed (this step does not occur if they already exist, as the icons are unlikely to change often enough to justify waiting for rendering when building every release)
set MMIco=ModManagerIcon

set MLIco=ModLauncherIcon

set MMSIco=ModManagerSetupIcon

IF NOT EXIST .\AppIcons\%MMIco%.ico (start /b /wait "" cmd.exe /c ".\build\GenerateIcon.bat" %MMIco%)

IF NOT EXIST .\AppIcons\%MLIco%.ico (start /b /wait "" cmd.exe /c ".\build\GenerateIcon.bat" %MLIco%)

IF NOT EXIST .\AppIcons\%MMSIco%.ico (start /b /wait "" cmd.exe /c ".\build\GenerateIcon.bat" %MMSIco%)


::Build .NET binaries
set R2R= -p:PublishReadyToRun=true
set RELEASE=-c Release
set PUBLISHPARAMS=%RELEASE% -o "%UNPACKAGEDOUT%"
CALL :BUILDMAINPROJECTS

set PUBLISHPARAMS=%RELEASE% --self-contained false -o "%UNPACKAGEDFDOUT%"
CALL :BUILDMAINPROJECTS

dotnet publish .\SporeMods.Launcher %PUBLISHPARAMSFD%
if errorlevel 1 GOTO FAIL
dotnet publish .\SporeMods.DragServant %PUBLISHPARAMSFD%
if errorlevel 1 GOTO FAIL
dotnet publish .\SporeMods.Manager %PUBLISHPARAMSFD%
if errorlevel 1 GOTO FAIL
dotnet publish .\SporeMods.KitImporter %PUBLISHPARAMSFD%
if errorlevel 1 GOTO FAIL


For /R "%UNPACKAGEDOUT%" %%x In (*.*) Do (
	
	Set "OriginalPath=%%x"
	CALL :GETFILENAME "!OriginalPath!"
	Set "FileName=!RETVAL!"
	Set "FdPath=%UNPACKAGEDFDOUT%!FileName!"
	
	
	If Not Exist "!FdPath!" (
		echo DELET !FileName!
		del "!OriginalPath!"
    )
)


::Build setup and such
::dotnet publish .\SporeMods.Setup -c Release --self-contained true -p:PublishSingleFile=false -p:PublishReadyToRun=true -r win-x86 -o .\bin\Updater
::if errorlevel 1 GOTO FAIL
dotnet publish .\SporeMods.Setup -c Release --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -r win-x86 -o .\bin\OfflineInstaller
if errorlevel 1 GOTO FAIL


dotnet build .\SporeMods.KitUpgradeDownloader -c Release -p:PublishReadyToRun=true -o .\bin\LauncherKitUpgradeDownloader
if errorlevel 1 GOTO FAIL

::Drop everything into final folders because Splitwirez absolutely cannot be trusted to remember all the files otherwise
::robocopy ".\bin\Updater" ".\bin\SMM" *.exe
::robocopy ".\bin\Updater" ".\bin\SMM" updater*.json
robocopy ".\bin\OfflineInstaller" ".\bin\SMM" *.exe
robocopy ".\bin\LauncherKitUpgradeDownloader" ".\bin\SMLK" *.exe

".\build\7za.exe" a ".\bin\SMLK\SporeModManagerSetup.zip" ".\bin\OfflineInstaller\*.exe"

::Take out the trash
echo Taking out the trash...
rd /S /Q ".\bin\Updater"
echo 1/3
rd /S /Q ".\bin\OfflineInstaller"
echo 2/3
rd /S /Q ".\bin\LauncherKitUpgradeDownloader"
echo 3/3

::You're winner
GOTO SUCCESS



:SUCCESS
exit 0



:FAIL
exit errorlevel



:BUILDMAINPROJECTS
	dotnet publish .\SporeMods.Launcher %PUBLISHPARAMS%
	if errorlevel 1 GOTO FAIL
	dotnet publish .\SporeMods.DragServant %PUBLISHPARAMS%
	if errorlevel 1 GOTO FAIL
	dotnet publish .\SporeMods.Manager %PUBLISHPARAMS%
	if errorlevel 1 GOTO FAIL
	dotnet publish .\SporeMods.KitImporter %PUBLISHPARAMS%
	if errorlevel 1 GOTO FAIL
	EXIT /B

::https://stackoverflow.com/questions/1645843/resolve-absolute-path-from-relative-path-and-or-file-name/33404867#33404867
:GETFILENAME
	SET RETVAL=%~nx1
	EXIT /B

:EXPANDPATH
	SET RETVAL=%~f1
	EXIT /B
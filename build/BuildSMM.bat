@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

::Get info for MSBuild
for /f "delims=" %%x in (.\build\VCTargetsPath.txt) do call set VCTargetsPath=%%x
for /f "delims=" %%x in (.\build\MSBuildPath.txt) do call set MSBuildPath=%%x

::Clear output folders
IF exist ".\bin" (rd /S /Q ".\bin")
IF exist ".\unpackagedBin\Release" (rd /S /Q ".\unpackagedBin\Release")

::Ensure output folders still exist
md ".\unpackagedBin"
md ".\unpackagedBin\Release"
md ".\bin"
md ".\bin\SMM"
md ".\bin\SMLK"


::Render app icons if needed (this step does not occur if they already exist, as the icons are unlikely to change often enough to justify waiting for rendering when building every release)
set MMIco=ModManagerIcon

set MLIco=ModLauncherIcon

set MMSIco=ModManagerSetupIcon

IF NOT EXIST .\AppIcons\%MMIco%.ico (start /b /wait "" cmd.exe /c ".\build\GenerateIcon.bat" %MMIco%)

IF NOT EXIST .\AppIcons\%MLIco%.ico (start /b /wait "" cmd.exe /c ".\build\GenerateIcon.bat" %MLIco%)

IF NOT EXIST .\AppIcons\%MMSIco%.ico (start /b /wait "" cmd.exe /c ".\build\GenerateIcon.bat" %MMSIco%)


::Build .NET binaries
dotnet build .\SporeMods.Launcher -c Release -p:PublishReadyToRun=true
if errorlevel 1 GOTO FAIL
dotnet build .\SporeMods.DragServant -c Release -p:PublishReadyToRun=true
if errorlevel 1 GOTO FAIL
dotnet build .\SporeMods.Manager -c Release -p:PublishReadyToRun=true
if errorlevel 1 GOTO FAIL
dotnet build .\SporeMods.KitImporter -c Release -p:PublishReadyToRun=true
if errorlevel 1 GOTO FAIL

::Build C++ binaries
"%MSBuildPath%" .\SporeMods.ManagerRedir\SporeMods.ManagerRedir.vcxproj /property:Configuration=Release
if errorlevel 1 GOTO FAIL
"%MSBuildPath%" .\SporeMods.ManagerRedir\SporeMods.ManagerRedir.vcxproj /property:Configuration=Release /p:RedirType=LAUNCHER
if errorlevel 1 GOTO FAIL
"%MSBuildPath%" .\SporeMods.ManagerRedir\SporeMods.ManagerRedir.vcxproj /property:Configuration=Release /p:RedirType=LKIMPORT
if errorlevel 1 GOTO FAIL

::Build setup and such
dotnet publish .\SporeMods.Setup -c Release --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true -r win-x86 -o .\bin\Updater
if errorlevel 1 GOTO FAIL
dotnet publish .\SporeMods.Setup -c Release --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -r win-x86 -o .\bin\OfflineInstaller
if errorlevel 1 GOTO FAIL
dotnet build .\SporeMods.KitUpgradeDownloader -c Release -p:PublishReadyToRun=true -o .\bin\LauncherKitUpgradeDownloader
if errorlevel 1 GOTO FAIL

::Drop everything into final folders because Splitwirez absolutely cannot be trusted to remember all the files otherwise
robocopy ".\bin\Updater" ".\bin\SMM" *.exe
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
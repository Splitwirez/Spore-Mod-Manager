del .\unpackagedBin /Q
del .\bin /Q
dotnet build .\SporeMods.Launcher -c Release -p:PublishReadyToRun=true
dotnet build .\SporeMods.DragServant -c Release -p:PublishReadyToRun=true
dotnet build .\SporeMods.Manager -c Release -p:PublishReadyToRun=true
dotnet build .\SporeMods.KitImporter -c Release -p:PublishReadyToRun=true
dotnet publish .\SporeMods.Setup -c Release --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true -r win-x86 -o .\bin\Updater
dotnet publish .\SporeMods.Setup -c Release --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -r win-x86 -o .\bin\OfflineInstaller
dotnet build .\SporeMods.KitUpgradeDownloader -c Release -p:PublishReadyToRun=true -o .\bin\LauncherKitUpgradeDownloaderpause
robocopy ".\bin\Updater" ".\bin" *.exe
robocopy ".\bin\OfflineInstaller" ".\bin" *.exe
robocopy ".\bin\LauncherKitUpgradeDownloaderpause" ".\bin" *.exe
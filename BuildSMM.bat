del .\bin
del .\unpackagedBin
dotnet build .\SporeMods.Launcher -c Release -p:PublishReadyToRun=true
dotnet build .\SporeMods.DragServant -c Release -p:PublishReadyToRun=true
dotnet build .\SporeMods.Manager -c Release -p:PublishReadyToRun=true
dotnet build .\SporeMods.KitImporter -c Release -p:PublishReadyToRun=true
dotnet publish .\SporeMods.Setup -c Release --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true -r win-x86 -o .\bin\Updater
dotnet publish .\SporeMods.Setup -c Release --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -r win-x86 -o .\bin\OfflineInstaller
dotnet publish .\SporeMods.KitUpgradeDownloader -c Release -p:PublishReadyToRun=true -r win-x86 -o .\bin\LauncherKitUpgradeDownloader
pause
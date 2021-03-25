dotnet build .\SporeMods.Launcher -c Release -r win-x86
dotnet build .\SporeMods.DragServant -c Release -r win-x86
dotnet build .\SporeMods.Manager -c Release -r win-x86
dotnet build .\SporeMods.KitImporter -c Release -r win-x86
dotnet publish .\SporeMods.Setup -c Release --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true -r win-x86 -o .\bin\Updater
dotnet publish .\SporeMods.Setup -c Release --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -r win-x86 -o .\bin\OfflineInstaller
pause
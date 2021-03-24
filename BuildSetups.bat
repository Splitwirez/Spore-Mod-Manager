dotnet build .\SporeMods.Launcher -c Release -p:PublishSingleFile=false -r win-x86
dotnet build .\SporeMods.DragServant -c Release -p:PublishSingleFile=false -r win-x86
dotnet build .\SporeMods.Manager -c Release -p:PublishSingleFile=false -r win-x86
dotnet build .\SporeMods.KitImporter -c Release -p:PublishSingleFile=false -r win-x86
dotnet publish .\SporeMods.Setup -c Release --self-contained false -p:PublishSingleFile=true -r win-x86 -o .\bin\Updater
dotnet publish .\SporeMods.Setup -c Release --self-contained true -p:PublishSingleFile=true -r win-x86 -o .\bin\OfflineInstaller
pause
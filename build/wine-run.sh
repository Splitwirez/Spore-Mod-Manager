wineDebugPrefix=`cat ../../wineDebugPrefix.txt`
echo Running in prefix \'$wineDebugPrefix\'.
WINEPREFIX=$wineDebugPrefix wine "../devBin/Debug/Spore Mod Manager.exe"

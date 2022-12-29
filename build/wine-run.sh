WINE_DEBUG_PREFIX=`cat ../../wineDebugPrefix.txt`
echo Running in prefix \'$WINE_DEBUG_PREFIX\'.


USER_BUILD_DIR="$HOME/.local/share/spore-mod-manager"
ENABLE_USER_BUILD_DIR_PATH=`realpath "$USER_BUILD_DIR/BUILD_TO_USER_DIR"`
if [ -f "$ENABLE_USER_BUILD_DIR_PATH" ]; then
    BUILD_PATH="$USER_BUILD_DIR/bin"
else
    BUILD_PATH="../devBin"
fi

WINEPREFIX=$WINE_DEBUG_PREFIX wine "$BUILD_PATH/Debug/Spore Mod Manager.exe"

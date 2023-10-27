#!/bin/bash

WINE_DEBUG_PREFIX=`cat ../../wineDebugPrefix.txt`
WINE_DEBUG_PREFIX=`realpath "$WINE_DEBUG_PREFIX"`
echo "Running in prefix '$WINE_DEBUG_PREFIX'..."


USER_BUILD_DIR="$HOME/.local/share/spore-mod-manager"
ENABLE_USER_BUILD_DIR_PATH=`realpath "$USER_BUILD_DIR/BUILD_TO_USER_DIR"`
if [ -f "$ENABLE_USER_BUILD_DIR_PATH" ]; then
    BUILD_PATH="$USER_BUILD_DIR/bin"
else
    BUILD_PATH="../devBin"
fi
BUILD_PATH=`realpath "$BUILD_PATH"`

if [ -z "$WINE" ]; then
	WINE=wine
fi

WINEPREFIX=$WINE_DEBUG_PREFIX "$WINE" "$BUILD_PATH/Debug/Spore Mod Manager.exe"

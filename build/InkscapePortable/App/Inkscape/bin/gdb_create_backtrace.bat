@echo off
rem Execute this to create a debug backtrace of an Inkscape crash.

set TRACEFILE=%USERPROFILE%\inkscape_backtrace.txt

echo Thanks for creating a debug backtrace!
echo.
echo After Inkscape starts, try to force the crash.
echo The backtrace will be recorded automatically.
echo.
echo Gathering sytem info...

echo --- INKSCAPE VERSION --- > %TRACEFILE%
inkscape.com -V >> %TRACEFILE%
echo. >> %TRACEFILE%
echo --- SYSTEM INFO --- >> %TRACEFILE%
systeminfo >> %TRACEFILE%

echo.
echo Launching Inkscape, please wait...

echo. >> %TRACEFILE%
echo --- BACKTRACE --- >> %TRACEFILE%
gdb.exe -batch -ex "run" -ex "bt" inkscape.exe >> %TRACEFILE%

echo.
echo Backtrace written to %TRACEFILE%
echo Please attach this file when reporting the issue at https://inkscape.org/report
echo (remove personal information you do not want to share, e.g. your user name)
echo.

pause

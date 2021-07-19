@echo off
powershell "cmd /c .\build\build.bat %1 | tee .\build\buildLog.txt"

if errorlevel 0 color 0A
GOTO THEEND

if errorlevel not 0 color 0C
GOTO THEEND

:THEEND
pause
exit
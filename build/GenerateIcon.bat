@echo off

echo Generating icon: %1
for /f "delims=" %%x in (.\build\InkScapePath.txt) do call set InkScapePath=%%x

:: $inkExportInPathStart, $exportSize, $inkExportOutPath
set "InkScapeExport=Write-Output $exportSize'-pixel render in progress...'; Start-Process -FilePath '%InkScapePath%' -ArgumentList $inkExportInPathStart'.svg','--export-area-page','--export-type=png','-w',$exportSize,'-h',$exportSize -Wait; $tempFileNames.Add($inkExportOutPath)"


set "InkScapeRenameOutput=if (Test-Path $inkExportOutPath) {; Remove-Item $inkExportOutPath; }; Move-Item -Path $inkExportInPathStart'.png' -Destination $inkExportOutPath"

set "CompletePowerShellCommand=$targetIdentifier='%1'; $iconDir='.\AppIcons\'; $imagePathBase=$iconDir + $targetIdentifier; $imagePathVectorBase=$imagePathBase + '-vector'; $inPngPath=$imagePathVectorBase + '.png'; $inPngPathSmall=$imagePathVectorBase + '.png'; $exportSize='16'; $inkExportInPathStart=$imagePathVectorBase + '-small'; $inkExportOutPath=$inkExportInPathStart + '.png'; $imageMagickConvertArgs=$inkExportOutPath + ' '; [string[]]$iconSizes=24,32,48,64,128,256; $tempFileNames=New-Object Collections.Generic.List[String]; %InkScapeExport%; Write-Output 'Done.'; Foreach ($exportSize in $iconSizes) {; $inkExportInPathStart=$imagePathVectorBase; $inkExportOutPath=$imagePathBase + '-' + $exportSize + '.png'; %InkScapeExport%; %InkScapeRenameOutput%; $imageMagickConvertArgs=$imageMagickConvertArgs + $imagePathBase + '-' + $exportSize + '.png '; Write-Output 'Done.'; }; $icoPath =$imagePathBase + '.ico'; $imageMagickConvertArgs=$imageMagickConvertArgs + $icoPath; Start-Process -FilePath '.\build\ImageMagick\convert.exe' -ArgumentList $imageMagickConvertArgs -Wait -NoNewWindow; Write-Output 'Removing temp files...'; Foreach ($currentName in $tempFileNames) {; Remove-Item $currentName; }; Write-Output 'Done.'; Write-Output 'Icon generation complete.'; Return"



powershell -Command %CompletePowerShellCommand%
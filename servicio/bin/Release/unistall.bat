@echo off
set mypath=%~dp0%
setx searcher ""
cd C:\Windows\Microsoft.NET\Framework\v4.0.30319
installutil.exe /u "C:\Users\rafit\Documents\GitHub\SRL\servicio\bin\Release\searcher.exe"
del "%~dp0%\*.*" /s /q
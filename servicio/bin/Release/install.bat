@echo off
set mypath=%ProgramFiles%\searcher
if not exist "%mypath%" mkdir "%mypath%"

xcopy "%~dp0*.*" "%mypath%\*.*" /d/e/y/c/i/h 
setx searcher "%mypath%"
cd C:\Windows\Microsoft.NET\Framework\v4.0.30319
installutil.exe "%mypath%\searcher.exe"

@echo off
setlocal

set CFG=Release
::set CFG=Debug

::call "C:\Program Files (x86)\Microsoft Visual Studio 15.0\Common7\Tools\VsDevCmd.bat"
::devenv ..\XLauncher.sln -build %CFG%

set MSBUILD="C:\Program Files (x86)\Microsoft Visual Studio 15.0\MSBuild\15.0\Bin\MSBuild.exe"
%MSBUILD% %~dp0..\XLauncher.sln -target:Rebuild "-p:Configuration=%CFG%;Platform=Any CPU"

set BLD=%~dp0..\_build_\AnyCPU_%CFG%\
set DST=%~dp0..\_dist_\

rmdir /s /q %DST%

if not exist %DST% mkdir %DST%

set SRC=%BLD%XLauncher.Setup\
for %%f in (
  XLauncher.Setup.exe
  XLauncher.Setup.exe.config
  NLog.dll
) do (
  copy /y %SRC%%%f %DST%
)

set APP=%DST%XLauncher\
if not exist %APP% mkdir %APP%

set SRC=%BLD%XLauncher.UI\
for %%f in (
  XLauncher.exe
  XLauncher.exe.config
  XLauncher.Entities.dll
  NLog.dll
) do (
  copy /y %SRC%%%f %APP%
)

set SRC=%BLD%XLauncher.XAI\
for %%f in (
  XLauncher.xll
  XLauncher.xll.config
) do (
  copy /y %SRC%%%f %APP%
)
if %CFG%==Debug copy /y %SRC%XLauncher.XAI.dll %APP%
if %CFG%==Debug copy /y %SRC%XLauncher.dna %APP%XLauncher32.dna
move %APP%XLauncher.xll %APP%XLauncher32.xll
move %APP%XLauncher.xll.config %APP%XLauncher32.xll.config

set XSDS=%DST%XSDs\
if not exist %XSDS% mkdir %XSDS%

set SRC=%~dp0..\XLauncher.Entities\XSDs\
for %%f in (
  XmlSchemas.sln
  Authorization.xsd
  Common.xsd
  Environments.xsd
  Session.xsd
) do (
  copy /y %SRC%%%f %XSDS%
)

set GETVER=powershell -ExecutionPolicy Unrestricted -file %~dp0GetVer.ps1
set ASM=%APP%XLauncher.exe
%GETVER% %ASM% > %DST%version.txt

endlocal

pause
exit

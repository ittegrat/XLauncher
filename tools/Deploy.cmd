@echo off
setlocal

set CFG=Release
::set CFG=Debug

set TOPDIR=%~dp0..\

for /f "delims=" %%i in ('"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath') do (
  if exist "%%i\Common7\Tools\vsdevcmd.bat" (
    call "%%i\Common7\Tools\vsdevcmd.bat"
  ) else (
    echo [ERROR] VsDevCmd.bat not found
    exit /b 2
  )
)

msbuild %TOPDIR%XLauncher.sln -t:Clean "-p:Configuration=%CFG%;Platform=Any CPU"
msbuild %TOPDIR%XLauncher.sln -t:Build "-p:Configuration=%CFG%;Platform=Any CPU"

set BLD=%TOPDIR%_build_\AnyCPU_%CFG%\
set DST=%TOPDIR%_dist_\

if exist %DST% rmdir /s /q %DST%

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
  XLauncher32.xll
  XLauncher32.xll.config
  XLauncher64.xll
  XLauncher64.xll.config
) do (
  copy /y %SRC%%%f %APP%
)
if %CFG%==Debug copy /y %SRC%XLauncher.XAI.dll %APP%
if %CFG%==Debug copy /y %SRC%XLauncher64.dna %APP%

set XSDS=%DST%XSDs\
if not exist %XSDS% mkdir %XSDS%

set SRC=%TOPDIR%XLauncher.Entities\XSDs\
for %%f in (
  Authorization.xsd
  Common.xsd
  Environments.xsd
  Session.xsd
) do (
  copy /y %SRC%%%f %XSDS%
)

set GETVER=powershell -ExecutionPolicy RemoteSigned -file %~dp0GetVer.ps1
set ASM=%APP%XLauncher.exe
%GETVER% %ASM% > %DST%version.txt

set GETDATE=powershell -ExecutionPolicy RemoteSigned -file %~dp0GetForceDate.ps1
set DAYS=7
%GETDATE% %DAYS% > %DST%force_update.txt

endlocal

pause
exit

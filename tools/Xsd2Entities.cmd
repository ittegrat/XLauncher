@echo off
setlocal

set TOPDIR=%~dp0..\

set SRCDIR=%TOPDIR%XLauncher.Entities\XSDs\
set OUTDIR=%TOPDIR%XLauncher.Entities\Generated\
set TMPDIR=%TOPDIR%_build_\_entities_\

set FIXPI=powershell -ExecutionPolicy RemoteSigned -file %~dp0_fixPI.ps1
set FIXCASE=powershell -ExecutionPolicy RemoteSigned -file %~dp0_fixCase.ps1

for /f "delims=" %%i in ('"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath') do (
  if exist "%%i\Common7\Tools\vsdevcmd.bat" (
    call "%%i\Common7\Tools\vsdevcmd.bat"
  ) else (
    echo [ERROR] VsDevCmd.bat not found
    exit /b 2
  )
)

if not exist %TMPDIR% mkdir %TMPDIR%
if not exist %OUTDIR% mkdir %OUTDIR%

xsd /nologo /c /l:CS /n:XLauncher.Entities.Common        /o:%TMPDIR% %SRCDIR%Common.xsd
xsd /nologo /c /l:CS /n:XLauncher.Entities.Authorization /o:%TMPDIR% %SRCDIR%Common.xsd %SRCDIR%Authorization.xsd
xsd /nologo /c /l:CS /n:XLauncher.Entities.Environments  /o:%TMPDIR% %SRCDIR%Common.xsd %SRCDIR%Environments.xsd
xsd /nologo /c /l:CS /n:XLauncher.Entities.Session       /o:%TMPDIR% %SRCDIR%Common.xsd %SRCDIR%Session.xsd

%FIXPI% %TMPDIR%Authorization.cs %TMPDIR%Authorization_Clean.cs
%FIXPI% %TMPDIR%Environments.cs  %TMPDIR%Environments_Clean.cs
%FIXPI% %TMPDIR%Session.cs       %TMPDIR%Session_Clean.cs

%FIXCASE% %TMPDIR%Common.cs              %OUTDIR%Common.cs
%FIXCASE% %TMPDIR%Authorization_Clean.cs %OUTDIR%Authorization.cs
%FIXCASE% %TMPDIR%Environments_Clean.cs  %OUTDIR%Environments.cs
%FIXCASE% %TMPDIR%Session_Clean.cs       %OUTDIR%Session.cs

endlocal
exit

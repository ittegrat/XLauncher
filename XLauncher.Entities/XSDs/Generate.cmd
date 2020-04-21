@echo off
setlocal

set SRCDIR=%~dp0
set TMPDIR=%SRCDIR%_out_\
set OUTDIR=%SRCDIR%..\Generated\

set XSD=C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\xsd.exe
set FIXPI=powershell -ExecutionPolicy Unrestricted -file %SRCDIR%_fixPI.ps1
set FIXPI2=powershell -ExecutionPolicy Unrestricted -file %SRCDIR%_fixPI2.ps1
set FIXCASE=powershell -ExecutionPolicy Unrestricted -file %SRCDIR%_fixCase.ps1

if not exist %TMPDIR% mkdir %TMPDIR%
if not exist %OUTDIR% mkdir %OUTDIR%

"%XSD%" /nologo /c /l:CS /n:XLauncher.Entities.Common        /o:%TMPDIR% %SRCDIR%Common.xsd
"%XSD%" /nologo /c /l:CS /n:XLauncher.Entities.Authorization /o:%TMPDIR% %SRCDIR%Common.xsd %SRCDIR%Authorization.xsd
"%XSD%" /nologo /c /l:CS /n:XLauncher.Entities.Environments  /o:%TMPDIR% %SRCDIR%Common.xsd %SRCDIR%Environments.xsd
"%XSD%" /nologo /c /l:CS /n:XLauncher.Entities.Session       /o:%TMPDIR% %SRCDIR%Common.xsd %SRCDIR%Session.xsd

%FIXPI% %TMPDIR%Authorization.cs %TMPDIR%Authorization_Clean.cs
%FIXPI% %TMPDIR%Environments.cs  %TMPDIR%Environments_Clean.cs
%FIXPI% %TMPDIR%Session.cs       %TMPDIR%Session_Clean.cs

%FIXCASE% %TMPDIR%Common.cs              %OUTDIR%Common.cs
%FIXCASE% %TMPDIR%Authorization_Clean.cs %OUTDIR%Authorization.cs
%FIXCASE% %TMPDIR%Environments_Clean.cs  %OUTDIR%Environments.cs
%FIXCASE% %TMPDIR%Session_Clean.cs       %OUTDIR%Session.cs

endlocal
exit

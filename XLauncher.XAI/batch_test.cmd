@echo off
setlocal

set XLAPP=C:\Program Files (x86)\Microsoft Office\root\Office16\EXCEL.EXE
set XLARGS=/x /r
set XAI=%~dp0XLauncher.xll

set CMDLINE="%XLAPP%" %XLARGS% "%XAI%"

start "" %CMDLINE%

set XLAUNCHER_SESSION=%~dp0Env_Session.xml

(for /f "delims=" %%i in (%~dp0XLSession.xml) do (
    set "line=%%i"
    setlocal enabledelayedexpansion
    set "line=!line:SimpleSession=EnvirSession!"
    echo(!line!
    endlocal
)) > "%XLAUNCHER_SESSION%"

start "" %CMDLINE%

endlocal

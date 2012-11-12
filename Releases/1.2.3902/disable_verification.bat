@echo off

rem parameters:
rem  %1: BuildType

IF "%1"=="" GOTO USAGE

set PLATFORM=x86

sn -Vr pvp\bin\%PLATFORM%\%1\napi.dll
sn -Vr pvp\bin\%PLATFORM%\%1\dshow.dll
sn -Vr pvp\bin\%PLATFORM%\%1\aui.dll
sn -Vr pvp\bin\%PLATFORM%\%1\core.dll
sn -Vr pvp\bin\%PLATFORM%\%1\theme.dll
sn -Vr pvp\bin\%PLATFORM%\%1\pvp.exe

sn -Vr pvp\bin\%PLATFORM%\%1\ru-RU\core.resources.dll
sn -Vr pvp\bin\%PLATFORM%\%1\ru-RU\pvp.resources.dll
sn -Vr pvp\bin\%PLATFORM%\%1\ru-RU\theme.resources.dll

goto EOF

:USAGE
echo ERROR: incorrect parameters
echo call as: %0% ^<BuildType^>
echo where ^<BuildType^> is your build configuration, i.e. Debug or Release

:EOF
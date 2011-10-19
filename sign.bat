@echo off

rem parameters:
rem  %1: BuildType

IF "%1"=="" GOTO USAGE

set PLATFORM=x86

sn /R pvp\bin\%PLATFORM%\%1\napi.dll keypairs\napi.snk
sn /R pvp\bin\%PLATFORM%\%1\dshow.dll keypairs\dshow.snk
sn /R pvp\bin\%PLATFORM%\%1\aui.dll keypairs\aui.snk
sn /R pvp\bin\%PLATFORM%\%1\core.dll keypairs\core.snk
sn /R pvp\bin\%PLATFORM%\%1\theme.dll keypairs\theme.snk
sn /R pvp\bin\%PLATFORM%\%1\pvp.exe keypairs\pvp.snk

sn /R pvp\bin\%PLATFORM%\%1\ru-RU\core.resources.dll keypairs\core.snk
sn /R pvp\bin\%PLATFORM%\%1\ru-RU\pvp.resources.dll keypairs\pvp.snk
sn /R pvp\bin\%PLATFORM%\%1\ru-RU\theme.resources.dll keypairs\theme.snk

goto EOF

:USAGE
echo ERROR: incorrect parameters
echo call as: %0% ^<BuildType^>
echo where ^<BuildType^> is your build configuration, i.e. Debug or Release

:EOF

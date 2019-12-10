@echo off
set /p ver="Enter Version Number:"
echo Zipping %ver%
"C:\Program Files\7-Zip\7z.exe" a -tzip RuneManager.%ver%.zip "./RuneApp/bin/Release/RuneManager.exe" "./RuneApp/bin/Release/RuneManager.exe.config" "./RuneApp/bin/Release/User Manual" "./RuneApp/bin/Release/InternalServer" "./RuneApp/bin/Release/data" "./RuneApp/bin/Release/custom_templates.json" "./RuneApp/bin/Release/RuneClasses.dll" "./RuneApp/bin/Release/RunePlugin.dll" "./RuneApp/bin/Release/log4net.dll" "./RuneApp/bin/Release/System.Collections.Immutable.dll" "./RuneApp/bin/Release/EPPlus.dll" "./RuneApp/bin/Release/Newtonsoft.Json.dll"
pause
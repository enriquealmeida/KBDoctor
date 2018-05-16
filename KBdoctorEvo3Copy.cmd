pushd %~dp0
SET GX_PROGRAM_DIR=C:\Program Files (x86)\Artech\GeneXus\GeneXusXEv3
SET GX_SDK_DIR=C:\Program Files (x86)\Artech\GeneXus\GeneXusXEv3PlatformSDK
SET NETFRAMEWORK_DIR="C:\Program Files (x86)\MSBuild\14.0\bin\amd64"
SET TargetFrameworkVersion=v3.5



rem %NETFRAMEWORK_DIR%\msbuild KBDoctor.sln  /t:Clean;LouvainCommunityPL;KBDoctorCore;KBDoctorCmd;KBDoctorUI
xcopy "KBDoctorCmd\bin\Debug\KBDoctorCmd.dll"   	"%GX_PROGRAM_DIR%"\packages /q /Y
xcopy "KBDoctorCore\bin\Debug\KBDoctorCore.dll" 	"%GX_PROGRAM_DIR%"\packages /q /Y
xcopy "KBDoctorUI\bin\Debug\KBDoctor.dll"     		"%GX_PROGRAM_DIR%"\packages /q /Y
xcopy "Louvain\bin\Debug\LouvainCommunityPL.dll"  	"%GX_PROGRAM_DIR%"\packages /q /Y
xcopy "KBDoctorCmd\KBDoctorCmd.Tasks.targets" 				"%GX_PROGRAM_DIR%" /q /Y
"%GX_PROGRAM_DIR%\genexus.exe" /install

"%GX_PROGRAM_DIR%\genexus.exe"

pause
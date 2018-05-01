pushd %~dp0
SET GX_PROGRAM_DIR=C:\Program Files (x86)\Artech\GeneXus\GeneXusXEv3
SET GX_SDK_DIR=C:\Program Files (x86)\Artech\GeneXus\GeneXusXEv3PlatformSDK
SET NETFRAMEWORK_DIR="C:\Program Files (x86)\MSBuild\14.0\bin\amd64"
SET TargetFrameworkVersion=v3.5



%NETFRAMEWORK_DIR%\msbuild KBDoctor.sln  /t:Clean;LouvainCommunityPL;KBDoctorCore;KBDoctorCmd;KBDoctorUI


pause
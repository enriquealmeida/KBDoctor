cd %~dp0
SET GX_PROGRAM_DIR=C:\Program Files (x86)\GeneXus\GeneXus15
SET GX_SDK_DIR=C:\Program Files (x86)\GeneXus\GeneXus15PlatformSDK
SET TargetFrameworkVersion=v4.6
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe  /t:clean  KBDoctor.sln 
pause
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe  KBDoctor.sln 
pause
CALL :CASE_%1
IF ERRORLEVEL 1 CALL :DEFAULT_CASE 

EXIT /B

:CASE_EVO3
SET GX_PROGRAM_DIR=C:\Program Files (x86)\Artech\GeneXus\GeneXusXEv3
SET GX_SDK_DIR=C:\Program Files (x86)\Artech\GeneXus\GeneXusXEv3PlatformSDK
SET NETFRAMEWORK_DIR="C:\Program Files (x86)\MSBuild\14.0\bin\amd64"
SET TargetFrameworkVersion=v3.5
ctt.exe source:KBDoctorUI\KBDoctorUI.csproj.user transform:KBDoctorUI\TransformacionEVO3.xml destination:KBDoctorUI\KBDoctorUI.csproj.user i
GOTO END_CASE

:CASE_GX15
SET GX_PROGRAM_DIR=C:\Program Files (x86)\GeneXus\GeneXus15
SET GX_SDK_DIR=C:\Program Files (x86)\GeneXus\GeneXus15PlatformSDK
SET NETFRAMEWORK_DIR="C:\Program Files (x86)\MSBuild\14.0\bin\amd64"
SET TargetFrameworkVersion=v4.6
ctt.exe source:KBDoctorUI\KBDoctorUI.csproj.user transform:KBDoctorUI\TransformacionGX15.xml destination:KBDoctorUI\KBDoctorUI.csproj.user i
GOTO END_CASE

:CASE_GX16
SET GX_PROGRAM_DIR=C:\Program Files (x86)\GeneXus\GeneXus16
SET GX_SDK_DIR=C:\Program Files (x86)\GeneXus\GeneXus16PlatformSDK
SET NETFRAMEWORK_DIR="C:\Program Files (x86)\MSBuild\14.0\bin\amd64"
SET TargetFrameworkVersion=v4.7.1
ctt.exe source:KBDoctorUI\KBDoctorUI.csproj.user transform:KBDoctorUI\TransformacionGX16.xml destination:KBDoctorUI\KBDoctorUI.csproj.user i
GOTO END_CASE

:DEFAULT_CASE
ECHO SetKBDoctor parametro invalido %1 : Debe ser EVO3, GX15 O GX16
PAUSE

:END_CASE
echo %TargetFrameworkVersion%
 GOTO :EOF 


CALL :CASE_%1
IF ERRORLEVEL 1 CALL :DEFAULT_CASE 

ECHO Done.
EXIT /B

:CASE_EVO3
SET GX_PROGRAM_DIR=C:\Program Files (x86)\Artech\GeneXus\GeneXusXEv3
SET GX_SDK_DIR=C:\Program Files (x86)\Artech\GeneXus\GeneXusXEv3PlatformSDK
SET NETFRAMEWORK_DIR="C:\Program Files (x86)\MSBuild\14.0\bin\amd64"
SET TargetFrameworkVersion=v3.5
GOTO END_CASE

:CASE_GX15
SET GX_PROGRAM_DIR=C:\Program Files (x86)\GeneXus\GeneXus15
SET GX_SDK_DIR=C:\Program Files (x86)\GeneXus\GeneXus15PlatformSDK
SET NETFRAMEWORK_DIR="C:\Program Files (x86)\MSBuild\14.0\bin\amd64"
SET TargetFrameworkVersion=v4.6
GOTO END_CASE

:CASE_GX16

GOTO END_CASE

:DEFAULT_CASE
ECHO SetKBDoctor parametro invalido %1 : Debe ser EVO3, GX15 O GX16
PAUSE

:END_CASE
  GOTO :EOF # return from CALL


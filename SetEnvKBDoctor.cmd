REM VARIABLES DE AMBIENTES USADAS EN KBDOCTOR
SET VisualStudioDir="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE"
SET NETFRAMEWORK_DIR="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin"
SET TargetGenexusVersion=%1
CALL :CASE_%1
IF ERRORLEVEL 1 CALL :DEFAULT_CASE 
PAUSE
EXIT /B


:CASE_GX16
SET GX_PROGRAM_DIR=C:\GeneXus\GeneXus16U10HF
SET GX_SDK_DIR=c:\GeneXus\GeneXus16SDK
rem SET NETFRAMEWORK_DIR="C:\Windows\Microsoft.NET\Framework64\v4.0.30319"
SET TargetFrameworkVersion=v4.7.1
ctt.exe source:KBDoctorUI\KBDoctorUI.csproj.user transform:KBDoctorUI\TransformacionGX16.xml destination:KBDoctorUI\KBDoctorUI.csproj.user i
GOTO END_CASE

:CASE_GX17
SET GX_PROGRAM_DIR=c:\GeneXus\GeneXus17
SET GX_SDK_DIR=c:\GeneXus\GeneXus17SDK
rem SET NETFRAMEWORK_DIR="C:\Windows\Microsoft.NET\Framework64\v4.0.30319"
SET TargetFrameworkVersion=v4.7.2
ctt.exe source:KBDoctorUI\KBDoctorUI.csproj.user transform:KBDoctorUI\TransformacionGX17.xml destination:KBDoctorUI\KBDoctorUI.csproj.user i
GOTO END_CASE

:CASE_GX18
SET GX_PROGRAM_DIR=c:\GeneXus\GeneXus18
SET GX_SDK_DIR=c:\GeneXus\GeneXus18SDK
rem SET NETFRAMEWORK_DIR="C:\Windows\Microsoft.NET\Framework64\v4.0.30319"
SET TargetFrameworkVersion=v4.8
ctt.exe source:KBDoctorUI\KBDoctorUI.csproj.user transform:KBDoctorUI\TransformacionGX18.xml destination:KBDoctorUI\KBDoctorUI.csproj.user i
GOTO END_CASE

:DEFAULT_CASE
ECHO SetKBDoctor parametro invalido %1 : Debe ser  GX16 GX17 GX18
 PAUSE

:END_CASE
echo %TargetFrameworkVersion%
echo %GX_SDK_DIR%
echo %TargetGenexusVersion%

 GOTO :EOF 

:EOF

@echo off
cd %~dp0
ECHO -- BUILD DE KBDOCTOR EVO3
call SetEnvKBDoctor.cmd EVO3
%NETFRAMEWORK_DIR%\msbuild KBDoctor.sln  /t:Clean;LouvainCommunityPL;KBDoctorCore;KBDoctorCmd;KBDoctorUI
pause
call PackKBDoctor.cmd EVO3
pause

ECHO -- BUILD DE KBDOCTOR GX15

call SetEnvKBDoctor.cmd GX15
%NETFRAMEWORK_DIR%\msbuild KBDoctor.sln  /t:Clean;LouvainCommunityPL;KBDoctorCore;KBDoctorCmd;KBDoctorUI
call PackKBDoctor.cmd GX15

ECHO -- BUILD DE KBDOCTOR GX16

call SetEnvKBDoctor.cmd GX16
%NETFRAMEWORK_DIR%\msbuild KBDoctor.sln  /t:Clean;LouvainCommunityPL;KBDoctorCore;KBDoctorCmd;KBDoctorUI
call PackKBDoctor.cmd GX16
pause
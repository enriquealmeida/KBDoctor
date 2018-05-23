@echo off
cd %~dp0


ECHO -- BUILD DE KBDOCTOR GX15
call SetEnvKBDoctor.cmd GX15
%NETFRAMEWORK_DIR%\msbuild KBDoctor.sln  /t:Clean;LouvainCommunityPL;KBDoctorCore;KBDoctorCmd;KBDoctorUI
rem call PackKBDoctor.cmd GX15
"%gx_program_dir%\genexus.exe"

pause

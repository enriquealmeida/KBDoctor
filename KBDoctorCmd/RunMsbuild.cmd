cd ..
call setEnvKBDoctor.cmd EVO3
cd KBDoctorCmd
set KBPath=C:\Models\ev3\LUCIAx_v81

del salida.txt
echo %KBPath%  > salida.txt
"%GX_PROGRAM_DIR%\GeneXus.exe" /install
%NETFRAMEWORK_DIR%\MSBuild.exe KBDoctorCmd.msbuild /t:ReviewObjects >> salida.txt
notepad salida.txt
pause
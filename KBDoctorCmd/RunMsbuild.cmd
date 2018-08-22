cd ..
call setEnvKBDoctor.cmd EVO3
cd KBDoctorCmd
set KBPath=C:\Models\ev3\RiesgoDIAN

del salida.txt
echo %KBPath%  > salida.txt
echo %GX_PROGRAM_DIR% >> SALIDA.TXT
echo %NETFRAMEWORK_DIR% >> salida.txt
echo %GX_SDK_DIR% >> salida.txt
echo %TargetFrameworkVersion% >> salida.txt

rem "%GX_PROGRAM_DIR%\GeneXus" /install

%NETFRAMEWORK_DIR%\MSBuild.exe KBDoctorCmd.msbuild   /t:ReviewObjects /p:GX_PROGRAM_DIR="%GX_PROGRAM_DIR%" >> salida.txt
notepad salida.txt
pause
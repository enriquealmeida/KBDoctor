cd ..
call setEnvKBDoctor.cmd EVO3
cd KBDoctorCmd
set KBPath=C:\Models\ev3\LUCIAX_V81

del salida.txt
echo %KBPath%  > salida.txt
echo %GX_PROGRAM_DIR% >> SALIDA.TXT
echo %NETFRAMEWORK_DIR% >> salida.txt
echo %GX_SDK_DIR% >> salida.txt
echo %TargetFrameworkVersion% >> salida.txt

rem "%GX_PROGRAM_DIR%\GeneXus" /install

C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe KBDoctorCmd.msbuild   /t:ReviewObjects /p:GX_PROGRAM_DIR="%GX_PROGRAM_DIR%" /p:DateFrom=01-01-2018 >> salida.txt
notepad salida.txt
pause
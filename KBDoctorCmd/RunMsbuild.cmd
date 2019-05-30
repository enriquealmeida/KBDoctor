cd ..
call setEnvKBDoctor.cmd GX16
cd KBDoctorCmd
set GX_PROGRAM_DIR=C:\Program Files (x86)\GeneXus\GeneXus16
set KBPath=C:\Models\nsanguinetti\MT16_2

del salida.txt

rem "%GX_PROGRAM_DIR%\GeneXus" /install

C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe KBDoctorCmd.msbuild /t:ReviewObjects /p:GX_PROGRAM_DIR="%GX_PROGRAM_DIR%";DateFrom="01-05-2019" >> salida.txt
notepad++.exe salida.txt
pause
cd ..
call setEnvKBDoctor.cmd GX16
cd KBDoctorCmd
set GX_PROGRAM_DIR=C:\Program Files (x86)\GeneXus\GeneXus16
set KBPath=C:\Models\nsanguinetti\MT16_2

rem del salida.txt

rem "%GX_PROGRAM_DIR%\GeneXus" /install

C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe "%GX_PROGRAM_DIR%\KBDoctorCmd.msbuild" /t:CheckBldObjects /p:GX_PROGRAM_DIR="%GX_PROGRAM_DIR%";KBPath=%KBPath%
rem notepad++.exe salida.txt
pause
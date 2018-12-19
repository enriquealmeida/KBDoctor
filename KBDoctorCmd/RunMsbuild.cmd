cd ..
call setEnvKBDoctor.cmd GX15_U11_and_UP
cd KBDoctorCmd
set KBPath=C:\Models\nsanguinetti\MT15
set ServerUser="USER"
set ServerPassword="PASSWORD"

del salida.txt
echo %KBPath%  > salida.txt
echo %GX_PROGRAM_DIR% >> SALIDA.TXT
echo %NETFRAMEWORK_DIR% >> salida.txt
echo %GX_SDK_DIR% >> salida.txt
echo %TargetFrameworkVersion% >> salida.txt

rem "%GX_PROGRAM_DIR%\GeneXus" /install

C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe KBDoctorCmd.msbuild /t:ReviewCommits /p:GX_PROGRAM_DIR="%GX_PROGRAM_DIR%";ServerUser=%ServerUser%;ServerPassword=%ServerPassword% >> salida.txt
notepad++.exe salida.txt
pause
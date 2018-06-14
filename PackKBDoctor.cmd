if %1%a==a goto nada
    set targetDir=..\_KBDoctorPack\%1%
	rmdir  /s /q %targetDir%
	md %targetDir%
	xcopy KBDoctorCmd\bin\debug\KBDoctorCmd.dll %targetDir%
	xcopy KBDoctorCore\bin\debug\KBDoctorCore.dll %targetDir%
	xcopy IniFileParser\bin\debug\IniFileParser.dll %targetDir%
	xcopy Louvain\bin\debug\LouvainCommunityPL.dll %targetDir%
	xcopy KBDoctorUI\bin\Debug\KBDoctor.dll  %targetDir%
	xcopy KBDoctorCmd\KBDoctorCmd.Tasks.targets %targetDir%

:nada
pushd %targetDir%
"C:\Program Files\7-Zip\7z.exe" a KBDoctor_%1%.zip  KBDoctorCmd.dll  KBDoctorCore.dll IniFileParser.dll LouvainCommunityPL.dll KBDoctor.dll KBDoctorCmd.Tasks.targets
move KBDoctor_%1%.zip ..\..\_KBDoctorPack\
popd 
pause
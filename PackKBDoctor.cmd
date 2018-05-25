if %1%a==a goto nada
    set targetDir=..\_KBDoctorPack\%1%
	rmdir  /s /q %targetDir%
	md %targetDir%
	xcopy KBDoctorCmd\bin\debug\KBDoctorCmd.dll %targetDir%\Packages\
	xcopy KBDoctorCore\bin\debug\KBDoctorCore.dll %targetDir%\Packages\
	xcopy KBDoctorCore\IniFileParser.dll               %targetDir%
	xcopy Louvain\bin\debug\LouvainCommunityPL.dll %targetDir%\Packages\
	xcopy KBDoctorUI\bin\Debug\KBDoctor.dll  %targetDir%\Packages\
	xcopy KBDoctorCmd\KBDoctorCmd.Tasks.targets %targetDir%\
:nada



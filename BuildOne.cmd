ECHO -- BUILD DE KBDOCTOR %1
call SetEnvKBDoctor.cmd %1
%NETFRAMEWORK_DIR%\msbuild KBDoctor.sln  /t:Clean;IniFileParser;LouvainCommunityPL;KBDoctorCore;KBDoctorCmd;KBDoctorUI
call PackKBDoctor.cmd %1
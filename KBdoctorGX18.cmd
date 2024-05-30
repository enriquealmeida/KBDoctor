@echo on
cd /d %~dp0
set TargetGeneXusVersion=GX18
call setenvKBDoctor %TargetGeneXusVersion%
"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe" KBDoctor.sln
@echo off
cd %~dp0
d:
set TargetGeneXusVersion=GX17
call setenvKBDoctor %TargetGeneXusVersion%
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe" KBDoctor.sln
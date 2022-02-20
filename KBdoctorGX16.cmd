@echo ON
cd %~dp0
d:
set TargetGeneXusVersion=GX16
call setenvKBDoctor %TargetGeneXusVersion%
%VisualStudioDir%\devenv.exe KBDoctor.sln
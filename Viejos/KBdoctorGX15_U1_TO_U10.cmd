@echo off
cd %~dp0
call setenvKBDoctor GX15_U1_to_U10
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe" KBDoctor.sln
pause
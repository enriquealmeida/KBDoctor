@echo off
cd %~dp0
call setenvKBDoctor GX15_U11_and_UP
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe" KBDoctor.sln
pause
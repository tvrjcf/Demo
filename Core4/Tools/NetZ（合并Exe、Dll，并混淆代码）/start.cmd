@echo off 
:start
CLS
Title *=*=*=*=*netz使用辅助*=*=*=*=*
:Help
start /b netz.exe
ping -n 2 127.1>nul
echo *=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*
echo                按任意键将启动一个新的控制台界面.
echo                在打开的控制台界面输入netz的使用命令.
echo                例如:netz -s -z app.exe lib1.dll lib2.dll
echo *=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*
pause >nul
start cmd
goto start


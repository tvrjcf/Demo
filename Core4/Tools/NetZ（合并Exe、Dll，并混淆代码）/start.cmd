@echo off 
:start
CLS
Title *=*=*=*=*netzʹ�ø���*=*=*=*=*
:Help
start /b netz.exe
ping -n 2 127.1>nul
echo *=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*
echo                �������������һ���µĿ���̨����.
echo                �ڴ򿪵Ŀ���̨��������netz��ʹ������.
echo                ����:netz -s -z app.exe lib1.dll lib2.dll
echo *=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*
pause >nul
start cmd
goto start


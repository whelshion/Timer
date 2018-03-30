@echo off 
@title 卸载Windows服务
echo==============================================================
echo=
echo          windows服务卸载
echo=
echo==============================================================
@echo off 
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /u  %~dp0Timer.WinService.Net4.exe
pause
@echo off 
@title 安装windows服务
echo==============================================================
echo=
echo         windows服务程序安装
echo=
echo==============================================================
@echo off 
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe %~dp0Timer.WinService.Net4.exe
pause
@echo off
title Nobre Arte Boxe Club - Servidor
echo ===================================================
echo     NOBRE ARTE BOXE CLUB - SISTEMA DE GESTAO
echo ===================================================
echo.
echo Iniciando servidor em http://localhost:5000...
set PATH=%PATH%;C:\Users\olive\.dotnet
cd /d "%~dp0"
start http://localhost:5000
dotnet run --urls "http://localhost:5000"
pause

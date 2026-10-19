@echo off
title ArenaFlow - Envio para o GitHub
echo ========================================================
echo         ARENAFLOW - ENVIAR PARA O GITHUB
echo ========================================================
echo.
echo Repositorio: https://github.com/FelipeO13/ArenaFlow
echo.
echo Enviando branch main para o GitHub...
echo (Se aparecer uma janela do GitHub ou navegador, autorize o login)
echo.
cd /d "%~dp0"
set "PATH=%PATH%;C:\Program Files\Git\cmd;C:\Program Files\Git\ucrt64\bin"
git push -u origin main
echo.
if %ERRORLEVEL% equ 0 (
    echo ========================================================
    echo   [SUCESSO] O ArenaFlow foi publicado no seu GitHub!
    echo ========================================================
) else (
    echo ========================================================
    echo   [AVISO] Verifique a mensagem acima ou autorizacao.
    echo ========================================================
)
echo.
pause

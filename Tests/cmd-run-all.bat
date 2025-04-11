@echo off & setlocal EnableDelayedExpansion

cls

start cmd /c cmd-get-v1-batch.bat
start cmd /c cmd-get-v2-batch.bat
start cmd /c cmd-get-v3-batch.bat
start cmd /c cmd-update-v1-batch.bat
start cmd /c cmd-update-v2-batch.bat
start cmd /c cmd-update-v3-batch.bat

@echo off

cls

call cmd-add-migration.bat DbInit
call cmd-update-database.bat
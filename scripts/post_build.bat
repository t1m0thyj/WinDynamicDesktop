@echo off
set Configuration=%~1
set TargetDir=%~2

if exist "%TargetDir%x86" (if exist "%TargetDir%cef" rmdir /s /q "%TargetDir%cef") && ren "%TargetDir%x86" cef
if %errorlevel% neq 0 exit /b %errorlevel%

if exist "%TargetDir%x64" rmdir /s /q "%TargetDir%x64"
if %errorlevel% neq 0 exit /b %errorlevel%

if "%Configuration%" == "Release" python %~0\..\prune_release.py "%TargetDir%\\"
if %errorlevel% neq 0 exit /b %errorlevel%

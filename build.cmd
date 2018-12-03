@ECHO OFF
PowerShell -NoProfile -ExecutionPolicy ByPass -Command "& """%~dp0eng\common\Build.ps1""" -restore -build %*"
exit /b %ErrorLevel%

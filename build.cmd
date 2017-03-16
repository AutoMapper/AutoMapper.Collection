@echo off
cd %~dp0

@ECHO OFF
powershell.exe -NoProfile -ExecutionPolicy unrestricted -Command "& {Import-Module '.\tools\psake\psake.psm1'; invoke-psake .\build.ps1 %*; if ($lastexitcode -ne 0) {write-host "ERROR: $lastexitcode" -fore RED; exit $lastexitcode} }"
# CLI Backend Test Script
# Tests all core functionality via CLI interactive commands

$testDll = "C:\Users\mayan\.gemini\antigravity\scratch\dotnet_dll_invoker\tests\TestEdgeCases\bin\Release\net10.0\TestEdgeCases.dll"
$cliExe = "C:\Users\mayan\.gemini\antigravity\scratch\dotnet_dll_invoker\cli\DotNetDllInvoker.CLI\bin\Release\net10.0\DotNetDllInvoker.CLI.exe"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DLL INVOKER CLI BACKEND TESTS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# First build the CLI
Write-Host "[BUILD] Building CLI..." -ForegroundColor Yellow
dotnet build cli\DotNetDllInvoker.CLI\DotNetDllInvoker.CLI.csproj --configuration Release -v q
Write-Host "[BUILD] Done" -ForegroundColor Green
Write-Host ""

# Test commands to pipe into CLI
$commands = @"
load $testDll
list
deps
invoke HelloWorld
invoke Add 5 3
invoke DoNothing
invoke ThrowsException
invoke ReturnsNull
invoke WritesToConsole
invoke GetVersion
clear
exit
"@

Write-Host "[TEST] Running CLI with test commands..." -ForegroundColor Yellow
Write-Host "Commands:" -ForegroundColor Gray
Write-Host $commands -ForegroundColor DarkGray
Write-Host ""
Write-Host "========== CLI OUTPUT ==========" -ForegroundColor Cyan

# Pipe commands to CLI
$commands | & $cliExe

Write-Host ""
Write-Host "========== END CLI OUTPUT ==========" -ForegroundColor Cyan
Write-Host ""
Write-Host "[DONE] Test script complete" -ForegroundColor Green

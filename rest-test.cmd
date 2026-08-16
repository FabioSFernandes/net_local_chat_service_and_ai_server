@echo off
setlocal EnableExtensions
chcp 65001 >nul

if "%~1"=="" (
    set "PROMPT_TEXT=Hello, you are my personal AI! How are you?"
) else (
    set "PROMPT_TEXT=%*"
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$OutputEncoding = [Console]::OutputEncoding = [Text.UTF8Encoding]::new();" ^
    "$json = @{ prompt = $env:PROMPT_TEXT } | ConvertTo-Json -Compress;" ^
    "$tmp = Join-Path $env:TEMP 'chat.json';" ^
    "[IO.File]::WriteAllText($tmp, $json);" ^
    "curl.exe -N -sS -X POST http://localhost:5018/chat -H 'Content-Type: application/json' --data-binary ('@' + $tmp) |" ^
    "ForEach-Object { if ($_ -match '^data:\s?(.*)$') { Write-Host -NoNewline ([regex]::Replace($Matches[1], '(\r\n|\n|\r)', '', 1)) } };" ^
    "Write-Host"

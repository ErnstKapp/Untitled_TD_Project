# Simple compilation check using Unity's log file
# This checks the most recent Unity log for errors

Write-Host "=== Checking Unity Compilation Status ===" -ForegroundColor Cyan
Write-Host ""

$projectPath = (Get-Location).Path
$logPath = "$env:LOCALAPPDATA\Unity\Editor\Editor.log"

# Also check project-specific logs
$projectLogs = @(
    "$projectPath\Logs\*.log",
    "$projectPath\Library\Logs\*.log"
)

Write-Host "Checking for compilation errors..." -ForegroundColor Yellow
Write-Host ""

$errorsFound = $false

# Check Unity Editor log
if (Test-Path $logPath) {
    Write-Host "Checking Unity Editor log: $logPath" -ForegroundColor Cyan
    $logContent = Get-Content $logPath -Tail 100 -ErrorAction SilentlyContinue
    
    if ($logContent) {
        $errors = $logContent | Select-String -Pattern "error CS\d+|Compilation failed|Scripts have compiler errors" -CaseSensitive:$false
        
        if ($errors) {
            $errorsFound = $true
            Write-Host "ERRORS FOUND in Editor log:" -ForegroundColor Red
            $errors | Select-Object -First 10 | ForEach-Object {
                Write-Host "  $_" -ForegroundColor Red
            }
        } else {
            Write-Host "No errors found in Editor log" -ForegroundColor Green
        }
    }
}

# Check project logs
foreach ($pattern in $projectLogs) {
    $files = Get-ChildItem -Path $pattern -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    if ($files) {
        Write-Host ""
        Write-Host "Checking: $($files.Name)" -ForegroundColor Cyan
        $content = Get-Content $files.FullName -Tail 50 -ErrorAction SilentlyContinue
        
        if ($content) {
            $errors = $content | Select-String -Pattern "error|Error|ERROR|compilation failed" -CaseSensitive:$false
            
            if ($errors) {
                $errorsFound = $true
                Write-Host "Potential issues found:" -ForegroundColor Yellow
                $errors | Select-Object -First 5 | ForEach-Object {
                    Write-Host "  $_" -ForegroundColor Yellow
                }
            }
        }
    }
}

Write-Host ""
if (-not $errorsFound) {
    Write-Host "No compilation errors detected in logs!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Note: This checks log files. For real-time compilation status," -ForegroundColor Yellow
    Write-Host "open Unity Editor and check the Console window." -ForegroundColor Yellow
} else {
    Write-Host "Errors were found. Check Unity Editor Console for details." -ForegroundColor Red
}

Write-Host ""
Write-Host "To check compilation in Unity Editor:" -ForegroundColor Cyan
Write-Host "  1. Open Unity Editor" -ForegroundColor White
Write-Host "  2. Window > General > Console" -ForegroundColor White
Write-Host "  3. Look for red error messages" -ForegroundColor White
Write-Host ""

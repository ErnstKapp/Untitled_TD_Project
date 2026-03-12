# Unity Project Compilation Check Script
# This script helps check for compilation errors in your Unity project

Write-Host "=== Unity Project Compilation Check ===" -ForegroundColor Cyan
Write-Host ""

# Check if Unity Editor is available
$unityPath = "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe"
if (-not (Test-Path $unityPath)) {
    Write-Host "Unity Editor not found at expected path." -ForegroundColor Yellow
    Write-Host "Trying to find Unity installation..." -ForegroundColor Yellow
    
    # Try common Unity Hub paths
    $possiblePaths = @(
        "C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe",
        "C:\Program Files (x86)\Unity\Editor\Unity.exe",
        "$env:ProgramFiles\Unity\Hub\Editor\*\Editor\Unity.exe"
    )
    
    $found = $false
    foreach ($path in $possiblePaths) {
        $matches = Get-ChildItem -Path $path -ErrorAction SilentlyContinue
        if ($matches) {
            $unityPath = $matches[0].FullName
            Write-Host "Found Unity at: $unityPath" -ForegroundColor Green
            $found = $true
            break
        }
    }
    
    if (-not $found) {
        Write-Host "Could not find Unity installation automatically." -ForegroundColor Red
        Write-Host "Please specify Unity path or use Unity Editor directly." -ForegroundColor Yellow
        exit 1
    }
}

Write-Host "Using Unity: $unityPath" -ForegroundColor Green
Write-Host ""

# Get project path (current directory)
$projectPath = (Get-Location).Path
Write-Host "Project Path: $projectPath" -ForegroundColor Cyan
Write-Host ""

# Method 1: Use Unity Batch Mode to check compilation
Write-Host "=== Method 1: Unity Batch Mode Compilation ===" -ForegroundColor Cyan
Write-Host "This will run Unity in batch mode to check for compilation errors..."
Write-Host ""

$logFile = "$projectPath\compile_check.log"
$errorLogFile = "$projectPath\compile_errors.log"

# Run Unity in batch mode to check compilation
Write-Host "Running Unity batch mode compilation check..." -ForegroundColor Yellow
& "$unityPath" -batchmode -quit -projectPath "$projectPath" -logFile "$logFile" -executeMethod CompileCheck 2>&1 | Out-Null

# Check if compilation succeeded
if (Test-Path $logFile) {
    $logContent = Get-Content $logFile -Raw
    
    # Check for common error patterns
    $errors = @()
    if ($logContent -match "error CS\d+") {
        $errors += "C# compilation errors found"
    }
    if ($logContent -match "Compilation failed") {
        $errors += "Compilation failed"
    }
    if ($logContent -match "Scripts have compiler errors") {
        $errors += "Scripts have compiler errors"
    }
    
    if ($errors.Count -gt 0) {
        Write-Host "COMPILATION ERRORS FOUND!" -ForegroundColor Red
        Write-Host ""
        foreach ($error in $errors) {
            Write-Host "  - $error" -ForegroundColor Red
        }
        Write-Host ""
        Write-Host "Check the log file: $logFile" -ForegroundColor Yellow
        Write-Host ""
        
        # Extract error lines
        $errorLines = Get-Content $logFile | Select-String -Pattern "error|Error|ERROR" | Select-Object -First 20
        if ($errorLines) {
            Write-Host "Sample errors:" -ForegroundColor Yellow
            $errorLines | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
        }
    } else {
        Write-Host "No compilation errors found!" -ForegroundColor Green
        Write-Host "Check log file for details: $logFile" -ForegroundColor Cyan
    }
} else {
    Write-Host "Could not generate log file. Unity may not have run successfully." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Method 2: Check .csproj Files ===" -ForegroundColor Cyan
Write-Host ""

# Check if .csproj files exist
$csprojFiles = Get-ChildItem -Path "$projectPath" -Filter "*.csproj" -ErrorAction SilentlyContinue
if ($csprojFiles) {
    Write-Host "Found .csproj files:" -ForegroundColor Green
    foreach ($file in $csprojFiles) {
        Write-Host "  - $($file.Name)" -ForegroundColor Cyan
    }
    Write-Host ""
    Write-Host "You can open these in Visual Studio or use MSBuild to compile:" -ForegroundColor Yellow
    Write-Host "  msbuild `"$($csprojFiles[0].FullName)`" /t:Build /p:Configuration=Debug" -ForegroundColor Gray
} else {
    Write-Host "No .csproj files found. Unity needs to generate them first." -ForegroundColor Yellow
    Write-Host "Open the project in Unity Editor to generate .csproj files." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Method 3: Manual Check ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Open Unity Editor" -ForegroundColor Yellow
Write-Host "2. Check the Console window (Window > General > Console)" -ForegroundColor Yellow
Write-Host "3. Look for red error messages" -ForegroundColor Yellow
Write-Host ""
Write-Host "Or use Unity's command line:" -ForegroundColor Yellow
Write-Host "  `"$unityPath`" -batchmode -quit -projectPath `"$projectPath`" -logFile `"$logFile`"" -ForegroundColor Gray
Write-Host ""

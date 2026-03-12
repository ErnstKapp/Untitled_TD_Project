# Compilation Check Guide

This guide shows you how to check for compilation errors in your Unity project from the terminal.

## Quick Methods

### Method 1: Simple Log Check (Easiest)
```powershell
.\check_compile_simple.ps1
```
This checks Unity's log files for compilation errors.

### Method 2: Full Unity Batch Mode Check
```powershell
.\check_compile.ps1
```
This runs Unity in batch mode to actively check compilation.

### Method 3: Using MSBuild (If Available)

If you have Visual Studio or MSBuild installed:

```powershell
# Find MSBuild
$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
# Or for older versions:
# $msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"

# Compile the project
& "$msbuild" "Assembly-CSharp.csproj" /t:Build /p:Configuration=Debug /v:minimal
```

### Method 4: Using Unity Command Line Directly

```powershell
# Find your Unity installation
$unityPath = "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe"

# Run compilation check
& "$unityPath" -batchmode -quit -projectPath "$PWD" -logFile "compile.log"
```

Then check `compile.log` for errors.

## Recommended: Use Unity Editor

The most reliable way is to:
1. Open Unity Editor
2. Open **Window > General > Console**
3. Look for red error messages

The Console window shows all compilation errors in real-time.

## Understanding the Error You Saw

The Burst compiler error you saw earlier:
```
Failed to resolve assembly: 'Assembly-CSharp-Editor'
```

This is typically:
- **Harmless** - It's a Burst compiler warning, not a C# compilation error
- **Temporary** - Usually resolves when Unity finishes compiling
- **Safe to ignore** - If your scripts compile in Unity Editor, you're fine

## What to Look For

### Real Compilation Errors:
- `error CS####` - C# compilation errors
- `Scripts have compiler errors` - Unity can't compile
- Red messages in Unity Console

### Warnings (Usually OK):
- `warning CS####` - Warnings, but code still compiles
- Burst compiler messages (if you're not using Burst)
- Assembly resolution warnings (often harmless)

## Troubleshooting

### If scripts won't compile:
1. Check Unity Console for specific error messages
2. Look for missing references or typos
3. Check that all scripts are in the correct folders
4. Try: **Assets > Reimport All**

### If you can't find MSBuild:
- Install Visual Studio Community (includes MSBuild)
- Or use Unity Editor directly (recommended)

### If Unity batch mode doesn't work:
- Make sure Unity path is correct
- Check that the project path is valid
- Use Unity Editor Console instead (most reliable)

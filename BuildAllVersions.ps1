<#
.SYNOPSIS
    Build RevitShutteringAddin for every installed Revit version (2020‑2027).

.DESCRIPTION
    The script detects Revit installations under "C:\Program Files\Autodesk\Revit *",
    sets the appropriate conditional compilation symbol (REVIT2020 … REVIT2027) and the
    correct RevitAPI path, builds the project, and copies the resulting DLL to a version‑
    specific filename (RevitShutteringAddin_20YY.dll).

.NOTES
    Requires the .NET SDK (dotnet) to be on the PATH.
#>

$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ProjectRoot 'RevitShutteringAddin.csproj'
$OutputDir   = Join-Path $ProjectRoot 'bin\Release\net48'

# -------------------------------------------------------------------
# Detect installed Revit versions (default install location)
# -------------------------------------------------------------------
$revitRoots = Get-ChildItem "C:\Program Files\Autodesk" -Directory |
    Where-Object { $_.Name -match '^Revit (\d{4})$' } |
    ForEach-Object {
        [pscustomobject]@{
            Year = [int]$Matches[1]
            Path = $_.FullName
        }
    } | Sort-Object Year

if (-not $revitRoots) {
    Write-Host "⚠️  No Revit installations found under C:\Program Files\Autodesk."
    exit 1
}

foreach ($rev in $revitRoots) {
    $symbol   = "REVIT$($rev.Year)"
    $apiPath  = $rev.Path

    Write-Host "`n=== Building for Revit $($rev.Year) ==="
    Write-Host "  Symbol   = $symbol"
    Write-Host "  API path = $apiPath"

    $msbuildArgs = @(
        "`"$ProjectFile`"",
        "-p:Configuration=Release",
        "-p:Platform=AnyCPU",
        "-p:DefineConstants=$symbol",
        "-p:RevitApiPath=`"$apiPath`"",
        "-t:Build"
    )

    $buildResult = & dotnet msbuild @msbuildArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed for Revit $($rev.Year). Skipping."
        continue
    }

    $srcDll = Join-Path $OutputDir 'RevitShutteringAddin.dll'
    $destDll = Join-Path $ProjectRoot "RevitShutteringAddin_$($rev.Year).dll"
    Copy-Item -Path $srcDll -Destination $destDll -Force
    Write-Host "✅ Produced $destDll"
}

Write-Host "`nAll builds finished."

param (
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]
    $OutputPath = '.\bin\m3u-editor',
    
    [Parameter()]
    [ValidateSet('Release', 'Debug')]
    [string]
    $Configuration = 'Release'
)

try {
    Write-Host "Building M3U Editor ($Configuration)..." -ForegroundColor Green
    
    # Clean previous build
    if (Test-Path -Path $OutputPath) {
        Write-Host "Cleaning previous build..." -ForegroundColor Yellow
        Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    # Build the project
    Write-Host "Building project..." -ForegroundColor Yellow
    $publishArgs = @(
        "publish",
        "m3u-editor.csproj",
        "-c", $Configuration,
        "--self-contained", "false",
        "-o", $OutputPath,
        "--nologo"
    )
    
    & dotnet $publishArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
    
    # Clean up unnecessary files
    Write-Host "Cleaning up build artifacts..." -ForegroundColor Yellow
    $filesToRemove = @(
        "$OutputPath\*.pdb",
        "$OutputPath\*.xml",
        "$OutputPath\*.config",
        "$OutputPath\*.pri"
    )
    
    foreach ($pattern in $filesToRemove) {
        Get-Item -Path $pattern -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
    }
    
    # Calculate build size
    $buildSize = (Get-ChildItem -Path $OutputPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
    $buildSizeFormatted = "{0:N2}" -f $buildSize
    
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Output directory: $OutputPath" -ForegroundColor Cyan
    Write-Host "Build size: ${buildSizeFormatted}MB" -ForegroundColor Cyan
    Write-Host ""
    
    # Show build contents
    Write-Host "Build contents:" -ForegroundColor Yellow
    Get-ChildItem -Path $OutputPath | Format-Table Name, Length, LastWriteTime -AutoSize
    
    exit 0
}
catch {
    Write-Error "Build script encountered an error: $($_.Exception.Message)"
    exit 1
}

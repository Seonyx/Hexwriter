# ============================================
# HexWriter - Copy Updated Files to Dibbler
# Run in PowerShell from any directory
# Update $src/$dst and the file lists below
# each time a set of changes needs copying.
# ============================================

$src = "\\192.168.69.19\hexwriter"
$dst = "C:\Users\Steve\Dropbox\VITALIS\Hexwriter\Hexwriter_web_src"

Write-Host "HexWriter File Copy" -ForegroundColor Cyan
Write-Host "From: $src" -ForegroundColor Gray
Write-Host "To:   $dst" -ForegroundColor Gray
Write-Host ""

if (-not (Test-Path $src)) {
    Write-Host "ERROR: Cannot reach share at $src" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $dst)) {
    Write-Host "ERROR: Destination not found at $dst" -ForegroundColor Red
    exit 1
}

# New directories to create (add as needed)
$dirs = @(
)
foreach ($d in $dirs) {
    $fullPath = Join-Path $dst $d
    if (-not (Test-Path $fullPath)) {
        New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
        Write-Host "  Created: $d" -ForegroundColor Green
    }
}

# New files to copy (add as needed)
Write-Host "Copying new files..." -ForegroundColor Yellow
$newFiles = @(
)
foreach ($f in $newFiles) {
    Copy-Item (Join-Path $src $f) (Join-Path $dst $f) -Force
    Write-Host "  NEW: $f" -ForegroundColor Green
}

# Modified files to update (add as needed)
Write-Host "Updating modified files..." -ForegroundColor Yellow
$modifiedFiles = @(
)
foreach ($f in $modifiedFiles) {
    Copy-Item (Join-Path $src $f) (Join-Path $dst $f) -Force
    Write-Host "  UPD: $f" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Copy complete!" -ForegroundColor Green

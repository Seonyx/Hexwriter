# ============================================
# HexWriter - Copy Updated Files to Dibbler
# Run in PowerShell from any directory
#
# Full sync: runs robocopy to mirror the whole
# project tree. Add specific files to the lists
# below when you only need a targeted update.
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

# Full mirror sync (excludes .git and user secrets)
Write-Host "Running full sync..." -ForegroundColor Yellow
robocopy $src $dst /MIR /XD ".git" /XF "Web.config" /NFL /NDL /NJH /NJS
Write-Host "Sync complete." -ForegroundColor Green
Write-Host ""

# Specific files to copy after a targeted change (add as needed, then clear)
$modifiedFiles = @(
)
if ($modifiedFiles.Count -gt 0) {
    Write-Host "Copying specific files..." -ForegroundColor Yellow
    foreach ($f in $modifiedFiles) {
        Copy-Item (Join-Path $src $f) (Join-Path $dst $f) -Force
        Write-Host "  UPD: $f" -ForegroundColor Cyan
    }
    Write-Host ""
}

Write-Host "Done!" -ForegroundColor Green

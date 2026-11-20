<#
.SYNOPSIS
  One-time local HTTPS setup for tpn.test / api.tpn.test.

  Run this once (as Administrator) before the first `docker compose up`:
    1. Installs the mkcert local CA into Windows' trusted root store.
    2. Generates a cert for tpn.test / *.tpn.test / api.tpn.test into infra/certs/.
    3. Adds tpn.test and api.tpn.test to the Windows hosts file, pointing at 127.0.0.1.

  Safe to re-run — every step is idempotent.
#>

$ErrorActionPreference = 'Stop'

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator
)
if (-not $isAdmin) {
    Write-Host "This script needs to edit the hosts file and the Windows trusted-root certificate store." -ForegroundColor Yellow
    Write-Host "Re-launching elevated..." -ForegroundColor Yellow
    Start-Process powershell -Verb RunAs -ArgumentList "-NoExit -File `"$PSCommandPath`""
    exit
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$certsDir = Join-Path $repoRoot "infra\certs"

# 1. mkcert
if (-not (Get-Command mkcert -ErrorAction SilentlyContinue)) {
    Write-Error "mkcert was not found on PATH. Install it first, e.g.: choco install mkcert"
}

Write-Host "==> Installing mkcert local CA (no-op if already installed)" -ForegroundColor Cyan
mkcert -install

if (-not (Test-Path $certsDir)) {
    New-Item -ItemType Directory -Path $certsDir -Force | Out-Null
}

Write-Host "==> Generating certificate for tpn.test, *.tpn.test, api.tpn.test" -ForegroundColor Cyan
Push-Location $certsDir
try {
    mkcert -cert-file "tpn.test.pem" -key-file "tpn.test-key.pem" "tpn.test" "*.tpn.test" "api.tpn.test"
} finally {
    Pop-Location
}

# 2. Hosts file
$hostsPath = "$env:SystemRoot\System32\drivers\etc\hosts"
$entries = @("tpn.test", "api.tpn.test", "cdn.tpn.test")
$hostsContent = Get-Content $hostsPath -Raw

$toAdd = @()
foreach ($entry in $entries) {
    if ($hostsContent -notmatch "(?m)^\s*127\.0\.0\.1\s+$([regex]::Escape($entry))\s*$") {
        $toAdd += "127.0.0.1`t$entry"
    }
}

if ($toAdd.Count -gt 0) {
    Write-Host "==> Adding hosts entries:" -ForegroundColor Cyan
    $toAdd | ForEach-Object { Write-Host "    $_" }
    Add-Content -Path $hostsPath -Value ("`n# ThirdPartyNetwork local dev`n" + ($toAdd -join "`n"))
} else {
    Write-Host "==> Hosts entries already present, skipping" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Done. Next steps:" -ForegroundColor Green
Write-Host "  1. Copy .env.example to .env in the repo root and fill in the values."
Write-Host "  2. Run: docker compose up --build"
Write-Host "  3. Open https://tpn.test"

param(
    [string]$Exe = (Join-Path $PSScriptRoot "..\..\Builds\AlphaReview\Windows\CanicaAlphaReview.exe"),
    [switch]$OpenGame
)

$ErrorActionPreference = "Stop"
$exePath = (Resolve-Path -LiteralPath $Exe).Path
$port = 7780
$existing = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
if ($existing) {
    foreach ($listener in $existing) {
        $owner = Get-CimInstance Win32_Process -Filter "ProcessId = $($listener.OwningProcess)"
        if ($listener.LocalAddress -ne "127.0.0.1" -or $owner.ExecutablePath -ne $exePath -or
            $owner.CommandLine -notmatch "(^|\s)-canicaLoopback(\s|$)" -or
            $owner.CommandLine -notmatch "(^|\s)-canicaServer(\s|$)") {
            throw "El port $port està ocupat per un altre servidor."
        }
    }
} else {
    $log = Join-Path (Split-Path $exePath -Parent) "relay-local.log"
    $relay = Start-Process -FilePath $exePath -PassThru -WindowStyle Hidden -ArgumentList @(
        "-batchmode", "-nographics", "-canicaLoopback", "-canicaServer", $port, "-logFile", ('"' + $log + '"')
    )
    $until = (Get-Date).AddSeconds(10)
    do {
        if ($relay.HasExited) { throw "El servidor s'ha tancat. Log: $log" }
        $bound = Get-NetTCPConnection -LocalAddress 127.0.0.1 -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
            Where-Object OwningProcess -eq $relay.Id
        if (!$bound) { Start-Sleep -Milliseconds 250 }
    } while (!$bound -and (Get-Date) -lt $until)
    if (!$bound) {
        Stop-Process -Id $relay.Id -ErrorAction SilentlyContinue
        throw "No s'ha obert el servidor en el termini previst. Log: $log"
    }
}

Write-Output "Canica · servidor local actiu a 127.0.0.1:$port"
if ($OpenGame) { Start-Process -FilePath $exePath -ArgumentList '-canicaLoopback' }

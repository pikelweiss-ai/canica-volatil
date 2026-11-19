# Proves automatiques de les sales online (sense finestres): amfitrio + jugador (+ servidor de sales).
# Us:  powershell -ExecutionPolicy Bypass -File Tools\Net\test-online.ps1 [-Exe <ruta a CanicaAlphaReview.exe>]
param(
    [string]$Exe = (Join-Path $PSScriptRoot "..\..\Builds\AlphaReview\Windows\CanicaAlphaReview.exe"),
    [string]$Out = (Join-Path $env:TEMP "canica-online-tests"),
    [switch]$MobileClient
)
$ErrorActionPreference = "Stop"
$Exe = (Resolve-Path $Exe).Path
New-Item -ItemType Directory -Force $Out | Out-Null
$portProbe = [System.Net.Sockets.UdpClient]::new([System.Net.IPEndPoint]::new([System.Net.IPAddress]::Loopback, 0))
try { $testDiscoveryPort = $portProbe.Client.LocalEndPoint.Port } finally { $portProbe.Dispose() }
$common = @("-batchmode", "-nographics", "-canicaLoopback", "-canicaDiscoveryPort", $testDiscoveryPort)

function Start-Bot([string]$name, [string[]]$botArgs) {
    $log = Join-Path $Out "$name.log"
    if (Test-Path $log) { Remove-Item $log }
    $all = $common + @("-logFile", "`"$log`"") + $botArgs
    if ($MobileClient -and $name.EndsWith('join')) { $all += '--mobile-preview' }
    return [pscustomobject]@{ Name = $name; Log = $log; Proc = (Start-Process $Exe -ArgumentList $all -PassThru -WindowStyle Hidden) }
}

function Wait-Bots($bots, [int]$seconds = 150) {
    $ok = $true
    foreach ($b in $bots) {
        if (-not $b.Proc.WaitForExit($seconds * 1000)) { $b.Proc.Kill(); Write-Host "  [$($b.Name)] TEMPS ESGOTAT" -ForegroundColor Red; $ok = $false; continue }
        $lines = Select-String -Path $b.Log -Pattern "NETBOT" | ForEach-Object { $_.Line }
        $pass = $lines -match "NETBOT PASS"
        foreach ($l in $lines) { Write-Host "  [$($b.Name)] $l" -ForegroundColor $(if ($l -match "FAIL") { "Red" } elseif ($l -match "PASS") { "Green" } else { "Gray" }) }
        $exception = Select-String -Path $b.Log -Pattern '^[\w.]+Exception:' -List
        if ($exception) {
            Write-Host "  [$($b.Name)] Excepció al joc: $($exception.Line)" -ForegroundColor Red
        }
        if (-not $pass -or $exception) { $ok = $false }
    }
    return $ok
}

$results = [ordered]@{}

Write-Host "`n== 1. LAN: llista de sales + contrasenya + mapa lliure ==" -ForegroundColor Cyan
$h = Start-Bot "lan-host" @("-canicaBot", "host", "-botNet", "lan", "-botPassword", "abc", "-botMode", "free")
Start-Sleep 4
$j = Start-Bot "lan-join" @("-canicaBot", "join", "-botJoin", "lan-list", "-botPassword", "abc")
$results["LAN llista"] = Wait-Bots @($h, $j) 260

Write-Host "`n== 2. LAN: entrar per codi + masmorra cooperativa ==" -ForegroundColor Cyan
$code = Join-Path $Out "codi-lan.txt"; if (Test-Path $code) { Remove-Item $code }
$h = Start-Bot "code-host" @("-canicaBot", "host", "-botNet", "lan", "-botMode", "dungeon", "-botCoop", "-botCodeFile", "`"$code`"")
$j = Start-Bot "code-join" @("-canicaBot", "join", "-botJoin", "`"code-file:$code`"", "-botCoop")
$results["LAN codi"] = Wait-Bots @($h, $j) 260

Write-Host "`n== 3. Internet: servidor + llista + contrasenya + masmorra cooperativa ==" -ForegroundColor Cyan
$srvLog = Join-Path $Out "server.log"; if (Test-Path $srvLog) { Remove-Item $srvLog }
$srv = Start-Process $Exe -ArgumentList ($common + @("-logFile", "`"$srvLog`"", "-canicaServer", "7790")) -PassThru -WindowStyle Hidden
Start-Sleep 4
$h = Start-Bot "net-host" @("-canicaBot", "host", "-botNet", "internet", "-botServer", "127.0.0.1:7790", "-botPassword", "secret", "-botMode", "dungeon", "-botCoop")
Start-Sleep 4
$j = Start-Bot "net-join" @("-canicaBot", "join", "-botJoin", "internet-list", "-botServer", "127.0.0.1:7790", "-botPassword", "secret", "-botCoop")
$results["Internet llista"] = Wait-Bots @($h, $j) 260

Write-Host "`n== 4. Internet: entrar per codi ==" -ForegroundColor Cyan
$code = Join-Path $Out "codi-net.txt"; if (Test-Path $code) { Remove-Item $code }
$h = Start-Bot "netcode-host" @("-canicaBot", "host", "-botNet", "internet", "-botServer", "127.0.0.1:7790", "-botMode", "free", "-botCodeFile", "`"$code`"")
$j = Start-Bot "netcode-join" @("-canicaBot", "join", "-botJoin", "`"code-file:$code`"", "-botServer", "127.0.0.1:7790")
$results["Internet codi"] = Wait-Bots @($h, $j) 260
Select-String -Path $srvLog -Pattern "SERVIDOR SALES" | ForEach-Object { Write-Host "  [server] $($_.Line)" -ForegroundColor Gray }
$srv.Kill()

Write-Host "`n== 5. Internet SENSE IP: tunel + directori, llista + contrasenya + masmorra ==" -ForegroundColor Cyan
$tag = "prova" + (Get-Random -Minimum 100000 -Maximum 999999)
$cloud = @("-canicaCloudTag", $tag, "-canicaNoLan")
$h = Start-Bot "cloud-host" (@("-canicaBot", "host", "-botNet", "internet", "-botPassword", "nuvol", "-botMode", "dungeon", "-botCoop") + $cloud)
Start-Sleep 6
$j = Start-Bot "cloud-join" (@("-canicaBot", "join", "-botJoin", "internet-list", "-botPassword", "nuvol", "-botCoop") + $cloud)
$results["Sense IP llista"] = Wait-Bots @($h, $j) 300

Write-Host "`n== 6. Internet SENSE IP: entrar per codi ==" -ForegroundColor Cyan
$code = Join-Path $Out "codi-nuvol.txt"; if (Test-Path $code) { Remove-Item $code }
$h = Start-Bot "cloudcode-host" (@("-canicaBot", "host", "-botNet", "internet", "-botMode", "free", "-botCodeFile", "`"$code`"") + $cloud)
$j = Start-Bot "cloudcode-join" (@("-canicaBot", "join", "-botJoin", "`"code-file:$code`"") + $cloud)
$results["Sense IP codi"] = Wait-Bots @($h, $j) 300

Write-Host "`n== RESULTAT ==" -ForegroundColor Cyan
$all = $true
foreach ($k in $results.Keys) { $v = $results[$k]; if (-not $v) { $all = $false }; Write-Host ("  {0,-18} {1}" -f $k, $(if ($v) { "PASSA" } else { "FALLA" })) -ForegroundColor $(if ($v) { "Green" } else { "Red" }) }
Write-Host "  Logs: $Out"
if (-not $all) { exit 1 }

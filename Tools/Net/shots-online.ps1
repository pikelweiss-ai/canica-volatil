# Captures de les pantalles online sense obrir finestres (renderitza a textura).
# Us: powershell -ExecutionPolicy Bypass -File Tools\Net\shots-online.ps1
param(
    [string]$Exe = (Join-Path $PSScriptRoot "..\..\Builds\AlphaReview\Windows\CanicaAlphaReview.exe"),
    [string]$Out = (Join-Path $PSScriptRoot "..\..\Builds\OnlineShots")
)
$Exe = (Resolve-Path $Exe).Path
New-Item -ItemType Directory -Force $Out | Out-Null
$Out = (Resolve-Path $Out).Path
function Run([string]$name, [string[]]$a, [switch]$gfx) {
    $log = Join-Path $Out "$name.log"
    $base = @("-batchmode"); if (-not $gfx) { $base += "-nographics" }
    Start-Process $Exe -ArgumentList ($base + @("-screen-width", "1600", "-screen-height", "900", "-logFile", "`"$log`"") + $a) -PassThru -WindowStyle Hidden
}
# Sales de decorat perque la llista no estiga buida (es tanquen soles en acabar)
$extra = @(
    (Run "x1" @("-canicaBot", "host", "-botRoom", "`"Masmorra dels dijous`"", "-botName", "Marta", "-botMode", "dungeon", "-botMax", "6", "-botExpect", "9")),
    (Run "x2" @("-canicaBot", "host", "-botRoom", "`"Proves de punteria`"", "-botName", "Pau_23", "-botMode", "free", "-botMax", "2", "-botExpect", "9", "-botPassword", "x")),
    (Run "x3" @("-canicaBot", "host", "-botRoom", "`"Classe SMX`"", "-botName", "Nil", "-botMode", "dungeon", "-botMax", "8", "-botExpect", "9"))
)
Start-Sleep 3
$h = Run "shot-host" @("-canicaBot", "host", "-botName", "Ferran", "-botRoom", "BotRoom", "-botPassword", "abc", "-botMode", "dungeon", "-botCoop", "-botShots", "`"$Out`"") -gfx
Start-Sleep 4
$j = Run "shot-join" @("-canicaBot", "join", "-botName", "Llop", "-botJoin", "lan-list", "-botPassword", "abc", "-botCoop", "-botShots", "`"$Out`"") -gfx
$h.WaitForExit(150000) | Out-Null; $j.WaitForExit(150000) | Out-Null
foreach ($p in $extra) { if (-not $p.HasExited) { $p.Kill() } }
Select-String -Path (Join-Path $Out "shot-*.log") -Pattern "NETBOT" | ForEach-Object { $_.Line }
Get-ChildItem $Out -Filter *.png | Select-Object Name, Length

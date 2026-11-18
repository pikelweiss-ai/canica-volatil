param([string]$BuildRoot=(Join-Path $PSScriptRoot '..\..\Builds\AlphaReview'))
$ErrorActionPreference='Stop'
$baseline=$null
foreach($platform in @('Android','Windows','Linux')) {
    $path=Join-Path $BuildRoot "$platform\compatibility.json"
    $manifest=Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    if($null -eq $baseline){$baseline=$manifest}
    if($manifest.protocol -ne $baseline.protocol -or $manifest.version -ne $baseline.version -or $manifest.sourceSha256 -ne $baseline.sourceSha256 -or ($manifest.weapons -join '|') -cne ($baseline.weapons -join '|') -or ($manifest.heals -join '|') -cne ($baseline.heals -join '|')) {
        throw "Paquet incompatible o antic: $platform. Recompila tots els sistemes amb el mateix projecte."
    }
    Write-Output "$platform coincideix: protocol $($manifest.protocol); versio $($manifest.version); $($manifest.weapons.Count) armes; $($manifest.heals.Count) cures"
}
Write-Output "CROSSPLAY_MANIFEST: PASS; SHA256 codi compartit $($baseline.sourceSha256). Falta validar els dispositius reals."

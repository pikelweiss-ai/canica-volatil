param(
    [string]$Out,
    [string]$BuildRoot=(Join-Path $PSScriptRoot '..\..\Builds\AlphaReview')
)
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$BuildRoot=(Resolve-Path -LiteralPath $BuildRoot).Path
$manifest=Get-Content -LiteralPath (Join-Path $BuildRoot 'Windows\compatibility.json') -Raw | ConvertFrom-Json
$version=$manifest.version -replace '-alpha$',''
$label="Alpha v$version"
$tag=$label.Replace(' ','-')
if(-not $Out){$Out=Join-Path $project "Builds\Distribucio\$tag-$(Get-Date -Format 'yyyyMMdd-HHmmss')"}
$Out=[IO.Path]::GetFullPath($Out)
if(-not $Out.StartsWith($project+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'El paquet ha de quedar dins del projecte'}
if(Test-Path -LiteralPath $Out){throw "La carpeta ja existeix: $Out"}
& (Join-Path $PSScriptRoot 'check-crossplay.ps1') -BuildRoot $BuildRoot
New-Item -ItemType Directory -Path $Out | Out-Null
$stage=Join-Path $Out 'staging'
New-Item -ItemType Directory -Path $stage | Out-Null
function Move-Staged([string]$source,[string]$destination){
    foreach($p in @($source,$destination)){
        if(-not [IO.Path]::GetFullPath($p).StartsWith($stage+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Ruta de reanomenament fora de staging'}
    }
    Move-Item -LiteralPath $source -Destination $destination
}
foreach($platform in @('Windows','Linux')){
    $suffix=if($platform -eq 'Linux'){'Ubuntu-Linux-x86_64'}else{'Windows-x86_64'}
    $name="CanicaVolatil-$tag-$suffix"
    $dest=Join-Path $stage $name
    New-Item -ItemType Directory -Path $dest | Out-Null
    foreach($entry in Get-ChildItem -LiteralPath (Join-Path $BuildRoot $platform)){
        if($entry.Name -match 'DoNotShip|BackUpThisFolder|\.pdb$|\.mdb$|^build-result\.txt$|\.log$|\.tmp$|~RF'){continue} # sense logs locals ni temporals del pegat
        Copy-Item -LiteralPath $entry.FullName -Destination $dest -Recurse
    }
    Move-Staged (Join-Path $dest 'CanicaAlphaReview_Data') (Join-Path $dest 'CanicaVolatil_Data')
    if($platform -eq 'Windows'){
        Move-Staged (Join-Path $dest 'CanicaAlphaReview.exe') (Join-Path $dest 'CanicaVolatil.exe')
        $help="Canica Volatil - $label`nExtraure tota la carpeta i obrir CanicaVolatil.exe. Mantindre les biblioteques i CanicaVolatil_Data al costat.`n"
    }else{
        Move-Staged (Join-Path $dest 'CanicaAlphaReview.x86_64') (Join-Path $dest 'CanicaVolatil.x86_64')
        $launcher='#!/usr/bin/env bash'+"`n"+'cd -- "$(dirname -- "$0")" || exit 1'+"`n"+'chmod u+x ./CanicaVolatil.x86_64'+"`n"+'exec ./CanicaVolatil.x86_64 "$@"'+"`n"
        [IO.File]::WriteAllText((Join-Path $dest 'Jugar.sh'),$launcher,[Text.UTF8Encoding]::new($false))
        $help="Canica Volatil - $label`nExtraure i executar ./Jugar.sh o bash Jugar.sh. Ubuntu/Linux x86_64, versio nativa. Execucio real en Ubuntu/Mint pendent de verificacio.`n"
    }
    $help+="`nOnline: JUGAR > ONLINE > pestanya INTERNET. Crea una sala o tria-la de la llista; tambe amb el codi de 6 lletres. Sense IPs ni ports. Protocol $($manifest.protocol); tots amb la v$version.`nGuia: RELEASE_NOTES.md (en este paquet)`n"
    [IO.File]::WriteAllText((Join-Path $dest 'LLEGIR.txt'),$help,[Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath (Join-Path $project 'RELEASE_NOTES.md') -Destination $dest
    Copy-Item -LiteralPath (Join-Path $project 'Assets\StreamingAssets\Llicencies\ChakraPetch-OFL.txt') -Destination $dest
    if($platform -eq 'Windows'){
        Compress-Archive -LiteralPath $dest -DestinationPath (Join-Path $Out "$name.zip") -CompressionLevel Optimal
    }else{
        if(Get-Command node -ErrorAction SilentlyContinue){
            & node (Join-Path $project 'Tools\Release\package-linux.mjs') $dest (Join-Path $Out "$name.tar.gz")
        }else{
            # Sense Node: el mateix TAR amb Python (py.exe o la instal·lacio d'usuari)
            $py=(Get-Command py -ErrorAction SilentlyContinue).Source
            if(-not $py){$py=Join-Path $env:LOCALAPPDATA 'Programs\Python\Python313\python.exe'}
            & $py -I (Join-Path $project 'Tools\Release\package-linux.py') $dest (Join-Path $Out "$name.tar.gz")
        }
        if($LASTEXITCODE -ne 0){throw 'Ha fallat el TAR Linux'}
    }
}
$apk=Join-Path $BuildRoot "Android\CanicaVolatil-$tag-Android.apk"
Copy-Item -LiteralPath $apk -Destination $Out
Copy-Item -LiteralPath (Join-Path $project 'RELEASE_NOTES.md') -Destination $Out
$hashes=foreach($p in Get-ChildItem -LiteralPath $Out -File | Where-Object {$_.Name -match '\.(zip|tar\.gz|apk)$'}){
    (Get-FileHash -LiteralPath $p.FullName -Algorithm SHA256).Hash.ToLower()+'  '+$p.Name
}
[IO.File]::WriteAllText((Join-Path $Out 'SHA256SUMS.txt'),(($hashes -join "`n")+"`n"),[Text.UTF8Encoding]::new($false)) # LF: sha256sum -c funciona en Linux
Write-Output "ALPHA_PACKAGES: $Out"

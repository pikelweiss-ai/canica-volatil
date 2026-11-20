param([string]$Out)
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if(-not $Out){$Out=Join-Path $project ('Builds\Distribucio\Alpha-v1.3.2-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))}
$Out=[IO.Path]::GetFullPath($Out)
if(-not $Out.StartsWith($project+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'El paquet ha de quedar dins del projecte'}
if(Test-Path -LiteralPath $Out){throw "La carpeta ja existeix: $Out"}
& (Join-Path $PSScriptRoot 'check-crossplay.ps1')
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
    $name="CanicaVolatil-Alpha-v1.3.2-$suffix"
    $dest=Join-Path $stage $name
    New-Item -ItemType Directory -Path $dest | Out-Null
    foreach($entry in Get-ChildItem -LiteralPath (Join-Path $project "Builds\AlphaReview\$platform")){
        if($entry.Name -match 'DoNotShip|BackUpThisFolder|\.pdb$|\.mdb$|^build-result\.txt$|\.log$|\.tmp$|~RF'){continue} # sense logs locals ni temporals del pegat
        Copy-Item -LiteralPath $entry.FullName -Destination $dest -Recurse
    }
    Move-Staged (Join-Path $dest 'CanicaAlphaReview_Data') (Join-Path $dest 'CanicaVolatil_Data')
    if($platform -eq 'Windows'){
        Move-Staged (Join-Path $dest 'CanicaAlphaReview.exe') (Join-Path $dest 'CanicaVolatil.exe')
        $help="Canica Volatil - Alpha v1.3.2`nExtraure tota la carpeta i obrir CanicaVolatil.exe. Mantindre les biblioteques i CanicaVolatil_Data al costat.`n"
    }else{
        Move-Staged (Join-Path $dest 'CanicaAlphaReview.x86_64') (Join-Path $dest 'CanicaVolatil.x86_64')
        $launcher='#!/usr/bin/env bash'+"`n"+'cd -- "$(dirname -- "$0")" || exit 1'+"`n"+'chmod u+x ./CanicaVolatil.x86_64'+"`n"+'exec ./CanicaVolatil.x86_64 "$@"'+"`n"
        [IO.File]::WriteAllText((Join-Path $dest 'Jugar.sh'),$launcher,[Text.UTF8Encoding]::new($false))
        $help="Canica Volatil - Alpha v1.3.2`nExtraure i executar ./Jugar.sh o bash Jugar.sh. Ubuntu/Linux x86_64, versio nativa. Execucio real en Ubuntu/Mint pendent de verificacio.`n"
    }
    $help+="`nOnline: JUGAR > ONLINE > pestanya INTERNET. Crea una sala o tria-la de la llista; tambe amb el codi de 6 lletres. Sense IPs ni ports. Protocol 16; tots amb la v1.3.2.`nGuia: RELEASE_NOTES.md (en este paquet)`n"
    [IO.File]::WriteAllText((Join-Path $dest 'LLEGIR.txt'),$help,[Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath (Join-Path $project 'RELEASE_NOTES.md') -Destination $dest
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
$apk=Join-Path $project 'Builds\AlphaReview\Android\CanicaVolatil-Alpha-v1.3.2-Android.apk'
Copy-Item -LiteralPath $apk -Destination $Out
$hashes=foreach($p in Get-ChildItem -LiteralPath $Out -File | Where-Object {$_.Name -match '\.(zip|tar\.gz|apk)$'}){
    (Get-FileHash -LiteralPath $p.FullName -Algorithm SHA256).Hash.ToLower()+'  '+$p.Name
}
[IO.File]::WriteAllText((Join-Path $Out 'SHA256SUMS.txt'),(($hashes -join "`n")+"`n"),[Text.UTF8Encoding]::new($false)) # LF: sha256sum -c funciona en Linux
Write-Output "ALPHA_PACKAGES: $Out"

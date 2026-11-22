param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sourceRoot = Join-Path $ProjectRoot 'ArtNou_v1.4\Blocs\Natura'
$outputRoot = Join-Path $ProjectRoot 'Assets\Resources\Pixel\Blocs\Cataleg\Natura\Autotile'
$names = @(
    'Roca_cova_1', 'Roca_cova_marro', 'Roca_cova_gran', 'Roca_cova_molsa', 'Roca_cova_magma', 'Roca_cova_blava',
    'Aigua_profunda', 'Aigua_nit', 'Aigua_fosca'
)
$directions = @(
    @{ Bit = 1; Side = 'N'; Edge = 0; Inner = 1 },
    @{ Bit = 2; Side = 'E'; Edge = 15; Inner = 14 },
    @{ Bit = 4; Side = 'S'; Edge = 15; Inner = 14 },
    @{ Bit = 8; Side = 'W'; Edge = 0; Inner = 1 }
)

New-Item -ItemType Directory -Force -Path $outputRoot | Out-Null
foreach ($name in $names) {
    $sourcePath = Join-Path $sourceRoot ($name + '.png')
    if (!(Test-Path -LiteralPath $sourcePath)) { throw "Falta el sprite base: $sourcePath" }
    $source = [System.Drawing.Bitmap]::new($sourcePath)
    try {
        if ($source.Width -ne 16 -or $source.Height -ne 16) { throw "El sprite $name ha de ser 16×16, no $($source.Width)×$($source.Height)" }
        $water = $name.StartsWith('Aigua_', [StringComparison]::Ordinal)
        $rock = $name.StartsWith('Roca_cova_', [StringComparison]::Ordinal)
        $colors = for ($y = 0; $y -lt 16; $y++) { for ($x = 0; $x -lt 16; $x++) { $c = $source.GetPixel($x, $y); if ($c.A -gt 0) { $c } } }
        $unique = @{}; foreach ($color in $colors) { $unique[('{0:X2}{1:X2}{2:X2}' -f $color.R,$color.G,$color.B)] = $color }
        $ranked = @($unique.Values | Sort-Object { $_.R + $_.G + $_.B })
        if ($ranked.Count -lt 2) { throw "El sprite $name no té prou colors per a fer els marges" }
        $dark = $ranked[0]
        $shade = $ranked[[Math]::Min(1, $ranked.Count - 1)]
        $highlight = $ranked[$ranked.Count - 1]
        $shore = [System.Drawing.Color]::FromArgb(224, 201, 140)
        $shoreShade = [System.Drawing.Color]::FromArgb(210, 185, 122)

        for ($mask = 0; $mask -lt 16; $mask++) {
            $tile = [System.Drawing.Bitmap]::new(16, 16, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            try {
                for ($y = 0; $y -lt 16; $y++) { for ($x = 0; $x -lt 16; $x++) { $tile.SetPixel($x, $y, $source.GetPixel($x, $y)) } }
                foreach ($direction in $directions) {
                    if (($mask -band $direction.Bit) -ne 0) { continue }
                    for ($i = 0; $i -lt 16; $i++) {
                        switch ($direction.Side) {
                            'N' { $x = $i; $edgeY = 0; $innerY = 1; if ($water) { $tile.SetPixel($x, $edgeY, $(if ($i % 4 -eq 0) { $shoreShade } else { $shore })); $tile.SetPixel($x, $innerY, $(if ($i % 4 -eq 1) { $highlight } else { $dark })) } elseif ($rock) { $tile.SetPixel($x, $edgeY, [System.Drawing.Color]::FromArgb(0,0,0,0)); if ($i % 3 -eq 1) { $tile.SetPixel($x, $innerY, $dark) } } }
                            'E' { $y = $i; $edgeX = 15; $innerX = 14; if ($water) { $tile.SetPixel($edgeX, $y, $(if ($i % 4 -eq 0) { $shoreShade } else { $shore })); $tile.SetPixel($innerX, $y, $(if ($i % 4 -eq 1) { $highlight } else { $dark })) } elseif ($rock) { $tile.SetPixel($edgeX, $y, [System.Drawing.Color]::FromArgb(0,0,0,0)); if ($i % 3 -eq 1) { $tile.SetPixel($innerX, $y, $dark) } } }
                            'S' { $x = $i; $edgeY = 15; $innerY = 14; if ($water) { $tile.SetPixel($x, $edgeY, $(if ($i % 4 -eq 0) { $shoreShade } else { $shore })); $tile.SetPixel($x, $innerY, $(if ($i % 4 -eq 1) { $highlight } else { $dark })) } elseif ($rock) { $tile.SetPixel($x, $edgeY, [System.Drawing.Color]::FromArgb(0,0,0,0)); if ($i % 3 -eq 1) { $tile.SetPixel($x, $innerY, $dark) } } }
                            'W' { $y = $i; $edgeX = 0; $innerX = 1; if ($water) { $tile.SetPixel($edgeX, $y, $(if ($i % 4 -eq 0) { $shoreShade } else { $shore })); $tile.SetPixel($innerX, $y, $(if ($i % 4 -eq 1) { $highlight } else { $dark })) } elseif ($rock) { $tile.SetPixel($edgeX, $y, [System.Drawing.Color]::FromArgb(0,0,0,0)); if ($i % 3 -eq 1) { $tile.SetPixel($innerX, $y, $dark) } } }
                        }
                    }
                }
                $destination = Join-Path $outputRoot ($name + '_mask' + $mask.ToString('00') + '.png')
                $tile.Save($destination, [System.Drawing.Imaging.ImageFormat]::Png)
            }
            finally { $tile.Dispose() }
        }
    }
    finally { $source.Dispose() }
}

Write-Output "MAP_AUTOTILE_VARIANTS: PASS; $($names.Count) sprites × 16 màscares = $($names.Count * 16) PNG 16×16"

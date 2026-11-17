# Generador de píxel art de Canica Volàtil.
# Cada sprite és una graella de lletres; cada lletra és un color de la paleta ($P).
# Per a canviar un píxel, canvia la lletra i torna a executar:
#   .\Tools\PixelArt\draw.ps1
# Genera els PNG a Assets\Sprites\Pixel\<Carpeta>\ i una previsualització ampliada
# per carpeta a Tools\PixelArt\preview_<Carpeta>.png
param(
  [string]$OutRoot = 'Assets\Sprites\Pixel',
  [string]$PreviewDir = 'Tools\PixelArt'
)
Add-Type -AssemblyName System.Drawing

# Rutes absolutes (System.Drawing no entén les rutes relatives de PowerShell)
$OutRoot = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutRoot)
$PreviewDir = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PreviewDir)

# Paleta comuna (lletra -> color). '.' = transparent
$P = New-Object 'System.Collections.Generic.Dictionary[string,string]' ([StringComparer]::Ordinal)
$pairs = @(
  'k','1e1e24',                                   # contorn
  'd','3a3f47', 'm','5d6570', 'l','9aa3ad',       # metall fosc / mig / clar
  'w','6e3a1c', 'o','9e5a2a', 'y','c98445',       # fusta fosca / mitja / clara
  'r','e8384f', 'R','a81c37', 'p','ff8a9a',       # roig / roig fosc / rosa
  'W','ffffff', 'g','d5d9e0', 'G','9aa0aa',       # blanc / gris clar / gris
  'h','a9d4f5',                                   # vidre
  'L','2e7d32', 'e','5cb85c', 's','6e3a1c',       # fulla fosca / fulla / tija
  'c','f2ead8', 'C','cfc3a6',                     # bena / bena ombra
  'Y','f5c542', 'u','b07a1a',                     # or / or fosc
  'b','4a7fe0', 'B','27448f',                     # blau / blau fosc (rar)
  'v','a259e0', 'V','5e2a8a',                     # morat / morat fosc (èpic)
  'n','2a1a12',                                   # interior fosc del cofre
  'q','f5c542', 'Q','b07a1a',                     # monedes de dins del cofre (no es recoloregen)
  'z','c8803a', 'Z','8a5424', 'x','f2c27a'        # bronze / bronze fosc / bronze clar (bales)
)
for ($i = 0; $i -lt $pairs.Count; $i += 2) { $P[$pairs[$i]] = $pairs[$i + 1] }

# Diccionari que distingeix majúscules (per a canviar colors: 'm','b','d','B', ...)
function New-Map([string[]]$pairs) {
  $m = New-Object 'System.Collections.Generic.Dictionary[string,string]' ([StringComparer]::Ordinal)
  for ($i = 0; $i -lt $pairs.Count; $i += 2) { $m[$pairs[$i]] = $pairs[$i + 1] }
  return ,$m
}

# mirror = $true  -> les files són només la meitat esquerra; la dreta es copia en espill
# recolor = mapa  -> canvia unes lletres per altres (per a variants de color)
function Expand-Rows($s) {
  $rows = @($s.rows)
  if ($s.mirror) {
    $rows = @($rows | ForEach-Object { $a = $_.ToCharArray(); [array]::Reverse($a); $_ + (-join $a) })
  }
  if ($s.recolor) {
    $map = $s.recolor
    $rows = @($rows | ForEach-Object {
      -join ($_.ToCharArray() | ForEach-Object { $c = [string]$_; if ($map.ContainsKey($c)) { $map[$c] } else { $c } })
    })
  }
  return ,$rows
}

function New-Sprite($rows, $w, $h) {
  $gh = $rows.Count; $gw = $rows[0].Length
  foreach ($r in $rows) { if ($r.Length -ne $gw) { throw "Fila de mida $($r.Length) != $gw : '$r'" } }
  $bmp = New-Object Drawing.Bitmap $w, $h, ([Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $ox = [int][Math]::Floor(($w - $gw) / 2); $oy = [int][Math]::Floor(($h - $gh) / 2)
  for ($y = 0; $y -lt $gh; $y++) {
    for ($x = 0; $x -lt $gw; $x++) {
      $ch = [string]$rows[$y][$x]
      if ($ch -eq '.') { continue }
      if (-not $P.ContainsKey($ch)) { throw "Color desconegut '$ch'" }
      $bmp.SetPixel($x + $ox, $y + $oy, [Drawing.ColorTranslator]::FromHtml('#' + $P[$ch]))
    }
  }
  return $bmp
}

$sprites = [ordered]@{}


$P['A']='89916b'; $P['a']='626e4c'; $P['t']='424d35'
$sprites['ArmesChatGPT/Pistola_16_v3'] = @{ width = 16; height = 12; rows = @(
  '..k...........k.',
  '.kkkkkkkkkkkkkkk',
  '.klmdmllllmdmmdk',
  '.kdddddddddddddk',
  '..kmmmdkkkkkkkk.',
  '..kmdmk.k..k....',
  '.kmddk..k.k.....',
  '.kmdmkkkkk......',
  'kmdddk..........',
  'kddddk..........',
  '.kkkk...........'
)}

$sprites['ArmesChatGPT/RiflePrecisio_40_v3'] = @{ width = 40; height = 16; rows = @(
  '..........kkkk.......kkk................',
  '.........kmmmdkkkkkkkmdhk...............',
  '.........kddddmmmmmmmddhk...............',
  '..........kkkkkkkkkkkkkk................',
  '............k...k...................kkkk',
  '..kkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkmdmk',
  '.kdAAAAAAAmmmmmmmmmmmmmmmmmmmmmmmmmmdddk',
  'kdAaAAAAAaaakkkkkkkkkkkkkkkkkkkkkkkkmdmk',
  'kdAaAAAaaakAAAaAAAaAAAak............kkkk',
  'kdAaAAaaakAAaakkkkkkkk..................',
  'kdAAAaaak.kak.kmmdk.....................',
  'kdaaatak..k.k.kdddk.....................',
  'kkkkkkk...kkk..kkk......................'
)}
foreach ($name in $sprites.Keys) {
 $s=$sprites[$name]
 $bmp=New-Sprite $s.rows $s.width $s.height
 $path=Join-Path $OutRoot ($name+'.png')
 New-Item -ItemType Directory -Force (Split-Path $path) | Out-Null
 $bmp.Save($path,[Drawing.Imaging.ImageFormat]::Png)
 $bmp.Dispose()
 "OK $name ($($s.width)x$($s.height))"
}

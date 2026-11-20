# Generador de píxel art de Canica Volàtil.
# Cada sprite és una graella de lletres; cada lletra és un color de la paleta ($P).
# Per a canviar un píxel, canvia la lletra i torna a executar:
#   .\Tools\PixelArt\draw.ps1
# Genera els PNG a Assets\Sprites\Pixel\<Carpeta>\ i una previsualització ampliada
# per carpeta a Tools\PixelArt\preview_<Carpeta>.png
param(
  [string]$OutRoot = 'Assets\Sprites\Pixel',
  [string]$PreviewDir = 'Tools\PixelArt',
  [string]$Only = '*' # Exemple: -Only 'Skins/*' per regenerar nomes les skins
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

# ---------------- AK-47 ----------------
$sprites['Armes/AK47_16'] = @{ size = 16; rows = @(
  '.........k...k..',
  '...kkkkkkkkkkkkk',
  'kkkyylllllyyymmk',
  'kooowmmdmmwwwkkk',
  'kwwkkkwkmmkkk...',
  'kkk..kwkkmmk....',
  '.....kk..kkk....'
)}

$sprites['Armes/AK47_24'] = @{ size = 24; rows = @(
  '.....................k..',
  '.............kk.....kmk.',
  '.....kkkkkkkkkkkkkkkkmkk',
  'kkkkkyylllllllyyyyymmmmk',
  'koooooommmddmmoooooddddk',
  'koooooowdddddddwwwwwkkkk',
  'kooookkkwokmmdkkkkk.....',
  'kwwwk.kwok.kmmdk........',
  'kkkkk.kkkk..kmmdk.......',
  '.............kkkk.......'
)}

$sprites['Armes/AK47_32'] = @{ size = 32; rows = @(
  '............................k...',
  '..................kk.......kmk..',
  '.......kkkkkkkkkkkkkkkkkkkkkmkkk',
  'kkkkkkkyyllllllllllyyyyyyymmmmmk',
  'koooooooommmmdddmmmooooooodddddk',
  'kooooooowddddddddddwwwwwwwkkkkkk',
  'kooooookkkwokkmmmdkkkkkkkk......',
  'kwwwwwk..kwok.kmmmdk............',
  'kkkkkk..kwok...kmmmdk...........',
  '........kkkk....kmmmdk..........',
  '.................kkkkk..........'
)}

# Pedra (arma llançable, 16x16)
$sprites['Armes/Pedra'] = @{ size = 16; mirror = $true; rows = @(
  '.....kkk',
  '...kkggg',
  '..kgggGG',
  '..kgGGGG',
  '.kGGGGmm',
  '.kGGmmmm',
  '.kmmmmmm',
  '..kmmddd',
  '...kkkkk'
)}

# Bala de l'AK (bronze, apunta cap a la dreta: el joc la gira cap on va)
$sprites['Armes/Bala'] = @{ size = 8; rows = @(
  '.kkkkk.',
  'kZzzzxk',
  '.kkkkk.'
)}

# ---------------- Items de cura (16x16) ----------------
$sprites['Items/PocioVida'] = @{ size = 16; rows = @(
  '................',
  '......kkkk......',
  '......kyok......',
  '......kowk......',
  '.....kkkkkk.....',
  '......kWhk......',
  '......kWhk......',
  '....kkWhhhkk....',
  '...kWpppppphk...',
  '..kWrrrrrrrrRk..',
  '..kWrrrrrrrrRk..',
  '..krrrrrrrrRRk..',
  '..krrrrrrrRRRk..',
  '...kRrrrrRRRk...',
  '....kkRRRRkk....',
  '......kkkk......'
)}

$sprites['Items/Farmaciola'] = @{ size = 16; rows = @(
  '................',
  '................',
  '.....kkkkkk.....',
  '.....kk..kk.....',
  '..kkkkkkkkkkkk..',
  '.kWWWWWWWWWWWgk.',
  '.kWWWWWrrWWWWgk.',
  '.kWWWWWrrWWWWgk.',
  '.kWWWrrrrrrWWgk.',
  '.kWWWrrrrrrWWgk.',
  '.kWWWWWrrWWWWgk.',
  '.kWWWWWrrWWWWgk.',
  '.kggggggggggggk.',
  '.kGGGGGGGGGGGGk.',
  '..kkkkkkkkkkkk..',
  '................'
)}

$sprites['Items/Poma'] = @{ size = 16; rows = @(
  '................',
  '.......kk.......',
  '.......ks.kkk...',
  '.......kskeeLk..',
  '...kkkkskkkkk...',
  '..kpprrrrrrrrk..',
  '.kpWprrrrrrrrRk.',
  '.kpWrrrrrrrrrRk.',
  '.kprrrrrrrrrrRk.',
  '.krrrrrrrrrrRRk.',
  '.krrrrrrrrrRRRk.',
  '.krrrrrrrrRRRRk.',
  '..krrrrrrRRRRk..',
  '...kRRRkkRRRk...',
  '....kkk..kkk....',
  '................'
)}

$sprites['Items/Bena'] = @{ size = 16; rows = @(
  '................',
  '................',
  '.....kkkkkk.....',
  '....kcccccck....',
  '...kccCCCCcck...',
  '..kccCkkkkCcck..',
  '..kcCk....kCck..',
  '..kcCk....kCck..',
  '..kcCCkkkkCCck..',
  '..kccCCCCCCcck..',
  '...kCccccccCk...',
  '....kkkkkkkcccck',
  '...........kkkk.',
  '................',
  '................',
  '................'
)}

# ---------------- Cofres ----------------
# Només la meitat esquerra: la dreta es fa en espill.
# m/d = tires i pany de metall: es recoloregen segons la raresa.

# Cofre del joc: fusta amb tires (12x10 dins d'un llenç de 16x16 -> més xicotet que un bloc)
$cofre = @(
  '......',
  '.kkkkk',
  'kyymyy',
  'koomoo',
  'kwwdwk',
  'kkkkkY',
  'koomoY',
  'koomok',
  'kwwdww',
  '.kkkkk'
)
$cofreObert = @(
  '.kkkkk',
  'kyymyy',
  'kwwdww',
  'kkkkkk',
  'knqQqq',
  'kkkkkk',
  'koomoY',
  'koomok',
  'kwwdww',
  '.kkkkk'
)
$rareses = [ordered]@{
  'Comu'       = $null
  'Rar'        = (New-Map 'm','b','d','B')
  'Epic'       = (New-Map 'm','v','d','V')
  'Llegendari' = (New-Map 'm','Y','d','u','Y','r')
}
foreach ($r in $rareses.Keys) {
  $sprites["Cofres/Cofre_$r"]       = @{ size = 16; mirror = $true; rows = $cofre;      recolor = $rareses[$r] }
  $sprites["Cofres/Cofre_${r}_Obert"] = @{ size = 16; mirror = $true; rows = $cofreObert; recolor = $rareses[$r] }
}

# Ombra del cofre (negre: al joc es posa semitransparent amb el color del SpriteRenderer)
$sprites['Cofres/Ombra'] = @{ size = 16; mirror = $true; rows = @(
  '....kkkk',
  '.kkkkkkk',
  'kkkkkkkk',
  '.kkkkkkk',
  '....kkkk'
)}

# ---- Per al mode masmorra (encara no es gasten) ----
# B) Baül: tapa arredonida i vores d'or
$sprites['Cofres/Dungeon/B_Baul'] = @{ size = 16; mirror = $true; rows = @(
  '........',
  '....kkkk',
  '..kkyyyy',
  '.kyyyyyy',
  '.kYooooo',
  '.kYooooo',
  '.kYYYYYk',
  '.kkkkkkY',
  '.kYooooY',
  '.kYooook',
  '.kYooooo',
  '.kYwwwww',
  '.kYYYYYY',
  '..kkkkkk',
  '........',
  '........'
)}

# C) Cofre de metall de masmorra amb gemma roja
$sprites['Cofres/Dungeon/C_Metall'] = @{ size = 16; mirror = $true; rows = @(
  '........',
  '........',
  '..kkkkkk',
  '.kllllll',
  '.kmlmmmm',
  '.kmmmmmm',
  '.kdddddk',
  '.kkkkkkr',
  '.kmlmmmR',
  '.kmmmmmk',
  '.kmmmmmm',
  '.kdldddd',
  '.kdddddd',
  '..kkkkkk',
  '........',
  '........'
)}

# Armes noves de ChatGPT
. (Join-Path $PSScriptRoot 'ArmesClaude\armes-claude.ps1')

# Masmorra: blocs, enemics, armes noves i barrets (Claude)
. (Join-Path $PSScriptRoot 'masmorra.ps1')
. (Join-Path $PSScriptRoot 'masmorra2.ps1')
. (Join-Path $PSScriptRoot 'prealpha.ps1')
. (Join-Path $PSScriptRoot 'alpha-personality.ps1')

# Sprites retocats a ma: el PNG del projecte es l'original i no es torna a generar.
# Llista a fets-a-ma.txt (un patro per linia, p. ex. Skins/*).
$fetsAMaFile = Join-Path $PSScriptRoot 'fets-a-ma.txt'
if (Test-Path $fetsAMaFile) {
  $fetsAMa = @(Get-Content $fetsAMaFile | Where-Object { $_ -and -not $_.StartsWith('#') })
  foreach ($key in @($sprites.Keys)) { foreach ($pat in $fetsAMa) { if ($key -like $pat) { $sprites.Remove($key); "MA $key (no es regenera)"; break } } }
}

# ---------------- Guardar PNG ----------------
$bitmaps = [ordered]@{}
foreach ($name in $sprites.Keys) {
  if ($name -notlike $Only) { continue }
  $s = $sprites[$name]
  $bmp = New-Sprite (Expand-Rows $s) $s.size $s.size
  $path = Join-Path $OutRoot ($name + '.png')
  New-Item -ItemType Directory -Force (Split-Path $path) | Out-Null
  $bmp.Save($path, [Drawing.Imaging.ImageFormat]::Png)
  $bitmaps[$name] = $bmp
  "OK $name ($($s.size)x$($s.size))"
}

# ---------------- Previsualitzacions ampliades (una per carpeta) ----------------
New-Item -ItemType Directory -Force $PreviewDir | Out-Null
$cell = 256; $pad = 24; $labelH = 30
$groups = $bitmaps.Keys | Group-Object { ($_ -split '/')[0] }
foreach ($group in $groups) {
  $names = @($group.Group)
  $sheet = New-Object Drawing.Bitmap ($names.Count * ($cell + $pad) + $pad), ($cell + $labelH + 2 * $pad)
  $g = [Drawing.Graphics]::FromImage($sheet)
  $g.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
  $g.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::Half
  $g.Clear([Drawing.ColorTranslator]::FromHtml('#4a8f3c'))
  $font = New-Object Drawing.Font 'Arial', 13, ([Drawing.FontStyle]::Bold)
  $i = 0
  foreach ($name in $names) {
    $x = $pad + $i * ($cell + $pad); $y = $pad
    $g.FillRectangle((New-Object Drawing.SolidBrush ([Drawing.ColorTranslator]::FromHtml('#3d7a31'))), $x, $y, $cell, $cell)
    $g.DrawImage($bitmaps[$name], $x, $y, $cell, $cell)
    $g.DrawString(($name -replace '^.*/', ''), $font, [Drawing.Brushes]::White, $x, $y + $cell + 6)
    $i++
  }
  $out = Join-Path $PreviewDir ('preview_' + $group.Name + '.png')
  $sheet.Save($out, [Drawing.Imaging.ImageFormat]::Png)
  $g.Dispose(); $sheet.Dispose()
  "Preview: $out"
}


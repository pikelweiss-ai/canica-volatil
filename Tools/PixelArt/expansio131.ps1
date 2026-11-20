# Alpha v1.3.1: barrets nous, barret pirata redissenyat, 6 armes, 3 enemics i 2 bosses.
# Es crida des de draw.ps1 (després de la resta). Dibuixa amb formes simples sobre una graella de lletres
# (mateixa paleta que la resta) i després afegeix el contorn fosc 'k'.

function G131([int]$w, [int]$h) {
  $g = New-Object 'char[][]' $h
  for ($y = 0; $y -lt $h; $y++) { $g[$y] = ('.' * $w).ToCharArray() }
  return ,$g
}
function Px131($g, [int]$x, [int]$y, [char]$c) { if ($y -ge 0 -and $y -lt $g.Count -and $x -ge 0 -and $x -lt $g[0].Count) { $g[$y][$x] = $c } }
function Rect131($g, [int]$x, [int]$y, [int]$w, [int]$h, [char]$c) { for ($j = $y; $j -lt $y + $h; $j++) { for ($i = $x; $i -lt $x + $w; $i++) { Px131 $g $i $j $c } } }
function Disc131($g, [double]$cx, [double]$cy, [double]$rx, [double]$ry, [char]$c) {
  for ($j = 0; $j -lt $g.Count; $j++) { for ($i = 0; $i -lt $g[0].Count; $i++) {
    $dx = ($i - $cx) / $rx; $dy = ($j - $cy) / $ry
    if (($dx * $dx + $dy * $dy) -le 1.0) { $g[$j][$i] = $c }
  } }
}
function Line131($g, [int]$x0, [int]$y0, [int]$x1, [int]$y1, [char]$c) {
  $dx = [Math]::Abs($x1 - $x0); $dy = -[Math]::Abs($y1 - $y0)
  $sx = if ($x0 -lt $x1) { 1 } else { -1 }; $sy = if ($y0 -lt $y1) { 1 } else { -1 }
  $err = $dx + $dy
  while ($true) {
    Px131 $g $x0 $y0 $c
    if ($x0 -eq $x1 -and $y0 -eq $y1) { break }
    $e2 = 2 * $err
    if ($e2 -ge $dy) { $err += $dy; $x0 += $sx }
    if ($e2 -le $dx) { $err += $dx; $y0 += $sy }
  }
}
function Outline131($g) {
  $h = $g.Count; $w = $g[0].Count; $add = @()
  for ($y = 0; $y -lt $h; $y++) { for ($x = 0; $x -lt $w; $x++) {
    if ($g[$y][$x] -ne '.') { continue }
    foreach ($d in @(@(1,0),@(-1,0),@(0,1),@(0,-1))) {
      $nx = $x + $d[0]; $ny = $y + $d[1]
      if ($nx -ge 0 -and $nx -lt $w -and $ny -ge 0 -and $ny -lt $h -and $g[$ny][$nx] -ne '.' -and $g[$ny][$nx] -ne 'k') { $add += ,@($x, $y); break }
    }
  } }
  foreach ($a in $add) { $g[$a[1]][$a[0]] = 'k' }
}
# Copia la meitat esquerra a la dreta (la graella ha de tindre l'amplada final)
function Mirror131($g) {
  $w = $g[0].Count
  for ($y = 0; $y -lt $g.Count; $y++) { for ($x = 0; $x -lt [int]($w / 2); $x++) { $g[$y][$w - 1 - $x] = $g[$y][$x] } }
}
function Rows131($g) { return @($g | ForEach-Object { -join $_ }) }

# ---------------------------------------------------------------- Barrets
# Pirata: tricorn amb calavera, galó daurat i ploma roja
$sprites['Barrets/Pirata'] = @{ size = 16; rows = @(
'................',
'...........kkRRr',
'......kkkk.kRrrk',
'.....kddddkkRrk.',
'....kddddddkkk..',
'...kddWWWWWddk..',
'..kdddWkWkWdddk.',
'.kddddWWWWWddddk',
'kdddddkWWkdddddk',
'kYdddkkkkkkkdddY',
'.kkkk......kkkk.'
) }
$sprites['Barrets/Mag'] = @{ size = 16; rows = @(
'.......k........',
'......kBk.......',
'......kBbk......',
'.....kBbYbk.....',
'.....kBbbbk.....',
'....kBbbbbbk....',
'....kBbbWbbk....',
'...kBbbbbbbbk...',
'..kBbbbbbbbbbk..',
'.kkBBBBBBBBBBkk.',
'..kkkkkkkkkkkk..'
) }
$sprites['Barrets/Cuiner'] = @{ size = 16; rows = @(
'................',
'....kkk..kkk....',
'...kWWWkkWWWk...',
'..kWWgWWWWgWWk..',
'..kWWWWWWWWWWk..',
'..kWgWWWWWWgWk..',
'...kWWWWWWWWk...',
'...kWWWWWWWWk...',
'...kgggggggk....',
'...kkkkkkkkkk...'
) }
$sprites['Barrets/Gat'] = @{ size = 16; rows = @(
'................',
'................',
'..kk........kk..',
'..kRk......kRk..',
'..kRRk....kRRk..',
'..kppRkkkkRppk..',
'..kkddddddddkk..',
'....kmmmmmmk....',
'.....kkkkkk.....'
) }
$sprites['Barrets/Cowboy'] = @{ size = 16; rows = @(
'................',
'................',
'....kkkkkkkk....',
'...kyyyyyyyyk...',
'...kyooooooyk...',
'...kyYYYYYYyk...',
'.kkkkkkkkkkkkkk.',
'kyyyyyyyyyyyyyyk',
'.kkkkkkkkkkkkkk.'
) }
$sprites['Barrets/Antenes'] = @{ size = 16; rows = @(
'................',
'..r..........r..',
'.krk........krk.',
'..k..........k..',
'..k..........k..',
'..kkkkkkkkkkkk..',
'..kmllllllllmk..',
'..kkkkkkkkkkkk..'
) }

# ---------------------------------------------------------------- Armes (en horitzontal, apuntant a la dreta)
function Weapon131($name, $w, $h, [scriptblock]$draw) {
  $g = G131 $w $h
  & $draw $g
  Outline131 $g
  $script:sprites["Armes/$name"] = @{ size = 32; rows = (Rows131 $g) }
}
# Bàcul de torreta: mànec de fusta i una torreta xicoteta al cap
Weapon131 'BaculTorreta' 26 12 { param($g)
  Line131 $g 1 9 14 9 'o'; Line131 $g 1 8 14 8 'y'
  Rect131 $g 14 4 8 7 'm'; Rect131 $g 14 4 8 2 'l'; Rect131 $g 14 9 8 2 'd'
  Rect131 $g 22 6 3 2 'd'; Px131 $g 24 6 'm'
  Px131 $g 17 7 'r'; Px131 $g 18 7 'r'
}
# Llançamines: tub gruixut i una mina amb espiga a la boca
Weapon131 'Llancamines' 26 12 { param($g)
  Rect131 $g 2 4 15 6 'd'; Rect131 $g 2 4 15 2 'm'; Rect131 $g 2 8 15 1 'k'
  Rect131 $g 5 10 3 2 'o'
  Disc131 $g 20 7 3 3 'R'; Disc131 $g 20 6 2 2 'r'; Px131 $g 19 5 'W'
  Px131 $g 23 7 'Y'; Px131 $g 24 7 'Y'
  Rect131 $g 17 5 2 5 'm'
}
# Arc de llum: arc daurat amb corda i una fletxa lluminosa
Weapon131 'ArcLlum' 26 20 { param($g)
  Disc131 $g 3 10 9 9.5 'Y'; Disc131 $g 3 10 7 7.5 '.'
  Rect131 $g 0 0 3 20 '.'
  Line131 $g 3 1 3 19 'W'
  Line131 $g 3 10 24 10 'W'; Line131 $g 4 9 21 9 'h'; Line131 $g 4 11 21 11 'h'
  Line131 $g 21 7 24 10 'W'; Line131 $g 21 13 24 10 'W'
}
# Escut de mirall: escut rodó amb vora d'or i vidre brillant
Weapon131 'EscutMirall' 22 22 { param($g)
  Disc131 $g 10 10 10 10 'Y'; Disc131 $g 10 10 8 8 'u'; Disc131 $g 10 10 7 7 'h'
  Disc131 $g 10 10 4 4 'g'
  Line131 $g 5 5 8 8 'W'; Line131 $g 6 4 9 7 'W'
  Px131 $g 10 10 'W'; Px131 $g 9 10 'W'; Px131 $g 10 9 'W'
  Rect131 $g 9 0 3 2 'Y'
}
# Vareta curativa: pal blanc amb una creu verda a la punta
Weapon131 'VaretaCurativa' 26 12 { param($g)
  Line131 $g 1 9 17 9 'c'; Line131 $g 1 8 17 8 'W'
  Disc131 $g 21 6 4 4 'W'
  Rect131 $g 20 3 3 7 'e'; Rect131 $g 18 5 7 3 'e'
  Px131 $g 21 6 'W'
}
# Ganxo d'abordatge: pistola curta amb un ganxo de metall i corda
Weapon131 'Ganxo' 26 14 { param($g)
  Rect131 $g 1 6 10 5 'w'; Rect131 $g 1 6 10 2 'o'
  Rect131 $g 3 11 3 3 'w'
  Rect131 $g 11 7 6 3 'm'; Rect131 $g 11 7 6 1 'l'
  Line131 $g 17 8 22 8 'y'
  Line131 $g 21 3 21 12 'l'; Line131 $g 21 3 24 5 'l'; Line131 $g 21 12 24 10 'l'
  Px131 $g 24 5 'W'; Px131 $g 24 10 'W'
}

# ---------------------------------------------------------------- Enemics (16x16, mirall)
function Enemy131($name, $w, $h, [scriptblock]$draw) {
  $g = G131 $w $h
  & $draw $g
  Mirror131 $g
  Outline131 $g
  $script:sprites["Enemics/$name"] = @{ size = $w; rows = (Rows131 $g) }
}
# Gàrgola: estàtua de pedra amb ales i ulls roigs
Enemy131 'Gargola' 16 16 { param($g)
  Disc131 $g 7.5 9 4.5 5 'm'; Disc131 $g 7.5 4.5 3.5 3.2 'l'
  Rect131 $g 4 3 2 2 'd'; Rect131 $g 5 1 1 3 'd'
  Rect131 $g 1 6 3 5 'd'; Rect131 $g 0 7 2 3 'm'
  Rect131 $g 5 5 2 2 'r'; Px131 $g 5 5 'Y'
  Rect131 $g 6 8 2 3 'l'; Rect131 $g 5 12 3 3 'd'
}
# Cranc: closca vermella i dues pinces grans
Enemy131 'Cranc' 16 16 { param($g)
  Disc131 $g 7.5 9 6 4 'r'; Disc131 $g 7.5 8 4 2 'p'
  Rect131 $g 2 4 3 4 'R'; Rect131 $g 1 2 4 3 'Y'; Rect131 $g 3 2 1 2 'R'
  Rect131 $g 4 5 1 1 'W'; Rect131 $g 6 5 1 1 'W'
  Px131 $g 4 6 'k'; Px131 $g 6 6 'k'
  Line131 $g 1 12 4 13 'R'; Line131 $g 3 14 5 12 'R'
}
# Fantasma de gel: cos blau transparent amb cara trista
Enemy131 'FantasmaGel' 16 16 { param($g)
  Disc131 $g 7.5 6.5 5.5 5 'h'; Rect131 $g 2 7 6 6 'h'
  Px131 $g 2 13 '.'; Px131 $g 4 14 'h'; Px131 $g 6 13 'h'
  Disc131 $g 7.5 6 3.5 3 'g'
  Rect131 $g 4 5 2 3 'B'; Px131 $g 4 5 'W'
  Rect131 $g 6 10 2 1 'b'
}
# ---------------------------------------------------------------- Bosses (32x32, mirall)
# Reina Aranya: cos morat gran, vuit potes, ulls roigs
Enemy131 'ReinaAranya' 32 32 { param($g)
  Disc131 $g 15.5 20 8 7 'V'; Disc131 $g 15.5 20 7 6 'v'
  Disc131 $g 15.5 11 6 5.5 'V'; Disc131 $g 15.5 11 5 4.5 'v'
  Line131 $g 8 15 1 8 'V'; Line131 $g 8 18 0 16 'V'; Line131 $g 8 21 1 27 'V'; Line131 $g 10 25 4 31 'V'
  Line131 $g 1 8 1 5 'V'; Line131 $g 0 16 0 20 'V'; Line131 $g 1 27 2 30 'V'
  Rect131 $g 11 8 3 3 'r'; Rect131 $g 12 8 1 1 'Y'; Rect131 $g 8 7 2 2 'r'
  Rect131 $g 12 14 3 2 'W'; Px131 $g 12 15 'k'
  Rect131 $g 13 18 3 4 'R'; Rect131 $g 13 24 3 2 'V'
  Rect131 $g 13 1 3 3 'V'; Px131 $g 14 2 'Y'
}
# Rei de Gel: gegant de gel amb corona i espatlles de glaç
Enemy131 'ReiGel' 32 32 { param($g)
  Rect131 $g 6 14 19 14 'h'; Rect131 $g 8 16 15 10 'g'; Rect131 $g 5 12 6 8 'b'; Rect131 $g 2 14 5 6 'h'
  Disc131 $g 15.5 9 7 7 'h'; Disc131 $g 15.5 9 5.5 5.5 'g'
  Rect131 $g 10 7 3 3 'B'; Px131 $g 10 7 'W'; Rect131 $g 14 12 2 1 'b'
  Rect131 $g 9 1 2 4 'Y'; Rect131 $g 12 0 2 5 'Y'; Rect131 $g 15 1 1 4 'Y'; Rect131 $g 9 4 7 1 'u'
  Rect131 $g 7 28 6 3 'b'; Rect131 $g 14 28 4 3 'B'
  Line131 $g 6 14 3 9 'W'; Line131 $g 4 14 2 10 'h'
}

# Els barrets tenen 16 files (la capçalera ocupa dalt)
foreach ($n in 'Pirata', 'Mag', 'Cuiner', 'Gat', 'Cowboy', 'Antenes') {
  $r = @($sprites["Barrets/$n"].rows)
  while ($r.Count -lt 16) { $r += '................' }
  $sprites["Barrets/$n"].rows = $r
}

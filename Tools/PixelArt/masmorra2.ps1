# Expansió de la masmorra: 4 temes més, 6 enemics, 2 bosses, 6 armes i les skins de la canica.
# El carrega draw.ps1 després de masmorra.ps1 (fa servir les seues funcions i colors).

# ---- Temes nous ----
function Liquid-Rows([int]$seed, [string]$a, [string]$b, [string]$c) {
  $rnd = New-Object System.Random $seed
  $rows = @()
  for ($y = 0; $y -lt 16; $y++) {
    $line = ''
    for ($x = 0; $x -lt 16; $x++) {
      $w = [Math]::Sin(($x * 0.8 + $y) * 0.7) + $rnd.NextDouble() * 0.8
      $ch = if ($w -gt 1.2) { $a } elseif ($w -gt 0.2) { $b } else { $c }
      $line += $ch
    }
    $rows += $line
  }
  return ,$rows
}

$temes2 = [ordered]@{
  #              terra: base, clar, fosc, junta, espurna | paret: dalt, dalt fosc, davant, davant fosc, morter
  'Bosc'       = @('L','K','w','w','e',  'e','L','o','w','k')
  'Cripta'     = @('O','P','M','k','g',  'V','M','O','M','V')
  'Cristall'   = @('B','b','M','M','h',  'v','V','b','B','V')
  'Claveguera' = @('S','O','k','K','',   'O','K','T','w','k')
}
$seed2 = 101
foreach ($t in $temes2.Keys) {
  $c = $temes2[$t]
  $sprites["Masmorra/${t}_Terra1"] = @{ size = 16; rows = (Floor-Rows $seed2 $c[0] $c[1] $c[2] $c[3] $c[4]) }
  $sprites["Masmorra/${t}_Terra2"] = @{ size = 16; rows = (Floor-Rows ($seed2 + 7) $c[0] $c[2] $c[1] $c[3] $c[4]) }
  $sprites["Masmorra/${t}_Paret"]  = @{ size = 16; rows = (Wall-Rows $c[5] $c[6] $c[7] $c[8] $c[9]) }
  $seed2 += 13
}
$sprites['Masmorra/Acid'] = @{ size = 16; rows = (Liquid-Rows 9 'H' 'K' 'L') }

# ---- Enemics nous (16x16, meitat esquerra) ----
$e8 = '........'
$sprites['Enemics/Ratpenat'] = @{ size = 16; mirror = $true; rows = @(
  $e8,$e8,$e8,
  '......k.',
  'k....kVk',
  'Vk...kVV',
  'VVk.kVVV',
  'VVVkVVrV',
  'VVVVVVVV',
  'kVVkVVVV',
  '.kk.kVVV',
  '.....kVV',
  '......kk',
  $e8,$e8,$e8
)}

$sprites['Enemics/Aranya'] = @{ size = 16; mirror = $true; rows = @(
  $e8,$e8,$e8,$e8,
  '.k......',
  '..k..kkk',
  'k..kkddd',
  '.k.kdddd',
  '..kddrdd',
  'kkkddddd',
  '...kdddd',
  '..k.kddd',
  '.k...kkk',
  'k.......',
  $e8,$e8
)}

$sprites['Enemics/Golem'] = @{ size = 16; mirror = $true; rows = @(
  $e8,$e8,
  '....kkkk',
  '...kPPPP',
  '...kPOOO',
  '...kOFkO',
  '...kOOOO',
  '.kkkMOOO',
  'kPPkOMOO',
  'kPOkOOOO',
  'kOOkOOMO',
  'kMMkOOOO',
  '.kkkMMMM',
  '...kMMkk',
  '...kkk..',
  $e8
)}

$sprites['Enemics/Esquelet'] = @{ size = 16; mirror = $true; rows = @(
  $e8,
  '.....kkk',
  '....kWWW',
  '...kWWWW',
  '...kWkkW',
  '...kWkkW',
  '...kWWWW',
  '....kWkW',
  '.....kkk',
  '....kgWg',
  '...kgkWk',
  '...k.kgW',
  '.....kgk',
  '....kWk.',
  '....kk..',
  $e8
)}

$sprites['Enemics/Mag'] = @{ size = 16; mirror = $true; rows = @(
  '.......k',
  '......kb',
  '.....kbb',
  '....kbbY',
  '...kbbbb',
  '..kkkkkk',
  '...kyyyy',
  '...kykyy',
  '...kyyyy',
  '...kWWWW',
  '..kbkWWW',
  '..kbbkWW',
  '.kbbbbkW',
  '.kBBBBBk',
  '.kkkkkkk',
  $e8
)}

$sprites['Enemics/BoletVeri'] = @{ size = 16; mirror = $true; rows = @(
  $e8,$e8,
  '....kkkk',
  '..kkvvvv',
  '.kvvWWvv',
  '.kvWWvvv',
  'kvvvvvWW',
  'kVVVVVVV',
  '.kkkkkkk',
  '...kgggg',
  '...kgkgg',
  '...kgggg',
  '...kGGGG',
  '....kkkk',
  $e8,$e8
)}

# ---- Bosses nous (32x32, meitat esquerra de 16) ----
$e16 = '................'
$sprites['Enemics/GolemGegant'] = @{ size = 32; mirror = $true; rows = @(
  $e16,$e16,
  '........kkkkkkkk',
  '.......kPPPPPPPP',
  '......kPPOOOOOOO',
  '......kPOOOOOOOO',
  '......kOOFFkOOOO',
  '......kOOFFkOOOO',
  '......kOOOOOOOOO',
  '......kMOOkkkkOO',
  '...kkkkMMOOOOOOO',
  '..kPPPkkMMMMMMMM',
  '.kPPOOOkOOOOOOOO',
  '.kPOOOOkOOOOFOOO',
  '.kOOOOOkOOOFFFOO',
  '.kOOOMOkOOOOFOOO',
  '.kMOOMOkOOOOOOOO',
  '.kMMMMMkMOOOOOOO',
  '..kkkkkkMMOOOOOO',
  '.......kMMMMMMMM',
  '.......kOOOOk...',
  '.......kMMMMk...',
  '.......kkkkkk...',
  $e16
)}

$sprites['Enemics/MagFosc'] = @{ size = 32; mirror = $true; rows = @(
  '...............k',
  '..............kV',
  '.............kVV',
  '............kVVV',
  '...........kVVVF',
  '..........kVVVVF',
  '.........kVVVVVV',
  '........kVVVVVVV',
  '......kkkkkkkkkk',
  '.......kkkkkkkkk',
  '.......kkkkFkkkk',
  '.......kkkkkkkkk',
  '......kVkkkkkkkk',
  '.....kVVVkkkkkkk',
  '....kVVvVVVVVVVV',
  '...kVVVvVVVVVVVV',
  '..kVVVVvVVVVVVVV',
  '..kVVVVvVVVVVVVV',
  '.kVVVVVvVVVVVVVV',
  '.kVVVVVVVVVVVVVV',
  '.kkkkkkkkkkkkkkk',
  $e16
)}

# ---- Armes noves ----
$sprites['Armes/Revolver'] = @{ size = 24; rows = @(
  '....kkkkkkkkkkkkkkkkkk..',
  '...kllllllllllllllllmk..',
  '..kdmmmdddddddddddddddk.',
  '..kdmdmdkkkkkkkkkkkkkk..',
  '.kowwkkdk...............',
  '.kowwk.kk...............',
  'kowwk...................',
  'kwwwk...................',
  'kkkk....................'
)}

$sprites['Armes/Bumerang'] = @{ size = 16; rows = @(
  'kkk.............',
  'kyyk............',
  '.kyok...........',
  '..kyok..........',
  '...kyok.........',
  '....kyokkkkkk...',
  '.....kyyyyyyok..',
  '......kwwwwwwk..',
  '.......kkkkkk...'
)}

$sprites['Armes/Shuriken'] = @{ size = 16; rows = @(
  '....k....',
  '...klk...',
  '...klk...',
  '.kkkmkkk.',
  'kllmdmllk',
  '.kkkmkkk.',
  '...klk...',
  '...klk...',
  '....k....'
)}

$sprites['Armes/Vareta'] = @{ size = 24; rows = @(
  '...................kk...',
  '..................kvvk..',
  'kkkkkkkkkkkkkkkkkkvWvk..',
  'kyyyyyyyyyyyyyyyykvvvk..',
  'kkkkkkkkkkkkkkkkkkkvk...',
  '...................k....'
)}

$sprites['Armes/Minigun'] = @{ size = 32; rows = @(
  '.........kkkkkkkkkkkkkkkkkkkkkk.',
  '........kdlllllllllllllllllllllk',
  'kkkkkkkkkddddddddddddddddddddddk',
  'kooooookkdlllllllllllllllllllllk',
  'kwwwwwwkkddddddddddddddddddddddk',
  'kkkkkkkk.kkkkkkkkkkkkkkkkkkkkkk.',
  '....kddk...kYYYk................',
  '....kkkk...kkkkk................'
)}

$sprites['Armes/Laser'] = @{ size = 32; rows = @(
  '..........kkkkkkkkkkkkkk........',
  '.........kWWWWWWWWWWWWhhk.......',
  'kkkkkkkkkkhhhhhhhhhhhhhhhkkkkk..',
  'kgggggggkbBBBBBBBBBBBBBBBkhhWk..',
  'kGGGGGGGkkkkkkkkkkkkkkkkkkkkkk..',
  '..kGGk..kbbk....................',
  '..kkkk..kkkk....................'
)}

$sprites['Armes/BolaMagica'] = @{ size = 8; rows = @(
  '.kkkk.',
  'kWvvvk',
  'kvvvVk',
  'kvvVVk',
  '.kkkk.'
)}

$sprites['Armes/Raig'] = @{ size = 16; rows = @(
  'kkkkkkkkkkkk',
  'hWWWWWWWWWWh',
  'kkkkkkkkkkkk'
)}

# ---- Skins: graella real 16x16, igual que la classica (128x128 ampliada x8). ----
# A 16 PPU totes ocupen una unitat. No cal canviar l'escala del jugador.
$cercle = @(
  '.....kkk',
  '...kkXXX',
  '..kXXXXX',
  '.kXXXXXX',
  '.kXXXXXX',
  'kXXXXXXX',
  'kXXXXXXX',
  'kXXXXXXX',
  'kXXXXXXX',
  'kXXXXXXX',
  'kXXXXXXX',
  '.kXXXXXX',
  '.kXXXXXX',
  '..kXXXXX',
  '...kkXXX',
  '.....kkk'
)

# Els detalls de les variants es conserven en coordenades de la graella antiga
# 20x20 i es traslladen a 16x16. La cara comuna es dibuixa a resolucio nativa.
function Skin([string]$fill, [string]$shade, [string]$light, [string[]]$extra, [bool]$face = $true) {
  $rows = @()
  for ($y = 0; $y -lt 16; $y++) {
    $line = ''
    for ($x = 0; $x -lt 8; $x++) {
      $ch = [string]$cercle[$y][$x]
      if ($ch -eq 'X') {
        if ($y -ge 13 -or ($y -ge 11 -and $x -le 2)) { $ch = $shade }
        elseif ($y -ge 2 -and $y -le 3 -and $x -ge 3 -and $x -le 4) { $ch = $light }
        else { $ch = $fill }
      }
      $line += $ch
    }
    $rows += $line
  }
  $chars = @($rows | ForEach-Object { ,($_.ToCharArray()) })
  if ($extra) {
    foreach ($m in $extra) {
      $p = $m.Split(',')
      $y = [int][Math]::Round([int]$p[0] * 15.0 / 19)
      $x = [int][Math]::Round([int]$p[1] * 7.0 / 9)
      # Respectar la silueta: un detall no pinta fora de la canica.
      if ($cercle[$y][$x] -ne '.') { $chars[$y][$x] = [char]$p[2] }
    }
  }
  if ($face) {
    foreach ($y in 4..6) { foreach ($x in 5..6) { $chars[$y][$x] = 'k' } }
    foreach ($y in 9..10) { foreach ($x in 3..7) { $chars[$y][$x] = 'k' } }
    foreach ($x in 4..7) { $chars[11][$x] = 'k' }
    foreach ($x in 5..7) { $chars[12][$x] = 'k' }
  }
  return ,@($chars | ForEach-Object { -join $_ })
}
$skins = [ordered]@{
  'Vidre'   = (Skin 'b' 'B' 'W' @('5,7,h','6,8,h','10,2,h','11,3,h','15,6,h','16,7,h'))
  'Foc'     = (Skin 'f' 't' 'F' @('0,6,F','0,7,f','1,4,F','1,5,f','2,9,F','16,4,F','16,8,F'))
  'Gel'     = (Skin 'I' 'J' 'W' @('2,8,W','6,2,W','11,2,J','16,5,W'))
  'Or'      = (Skin 'Y' 'u' 'W' @('2,7,W','4,8,W','10,1,W'))
  'Robot'   = (Skin 'm' 'd' 'l' @('7,3,k','7,4,k','7,5,k','7,6,k','7,7,k','7,8,k','7,9,k','8,3,k','8,4,h','8,5,h','8,6,h','8,7,h','8,8,h','8,9,h','9,3,k','9,4,k','9,5,k','9,6,k','9,7,k','9,8,k','9,9,k','12,4,d','13,5,d','13,6,l','13,7,d','13,8,l','13,9,d','1,9,d','0,9,r') $false)
  'Zombi'   = (Skin 'e' 'L' 'H' @('8,6,r','12,6,W','13,7,W','11,2,L','4,7,L'))
  'Ninja'   = (Skin 'M' 'k' 'O' @('7,1,y','7,2,y','7,3,y','7,4,y','7,5,y','7,7,y','7,8,y','7,9,y','8,1,y','8,2,y','8,3,y','8,4,y','8,5,y','8,7,y','8,8,y','8,9,y','7,6,k','8,6,k','6,1,r','6,0,r') $false)
  'Sindria' = (Skin 'r' 'R' 'p' @('1,5,L','1,6,L','2,4,L','3,3,e','16,3,L','17,4,L','18,6,L','10,3,k','5,5,k','15,7,k'))
  'Fantasma'= (Skin 'W' 'g' 'W' @('7,6,k','7,5,k','8,5,k','8,6,k','9,5,k','9,6,k','12,4,W','13,5,W','13,6,W','13,7,W','13,8,W','13,9,W','12,7,k','12,8,k','12,9,k') $false)
}
foreach ($s in $skins.Keys) {
  $sprites["Skins/$s"] = @{ size = 16; mirror = $true; rows = $skins[$s] }
}

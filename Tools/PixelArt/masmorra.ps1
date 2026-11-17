# Sprites de la masmorra (blocs, enemics, armes noves i barrets).
# El carrega draw.ps1 (dot-source), així que fa servir $P i $sprites d'allí.

# ---- Colors nous de la paleta ----
$P['P']='6b6f80'; $P['O']='4b4e5c'; $P['M']='2f313b'        # pedra de cova clara / mitja / fosca
$P['D']='e8c98a'; $P['E']='c9a464'; $P['T']='9c7a40'        # sorra clara / mitja / fosca
$P['N']='f4f8ff'; $P['I']='bfe4f7'; $P['J']='7fb6d9'        # neu / gel clar / gel fosc
$P['F']='ffd23a'; $P['f']='ff7a1a'; $P['t']='c2321a'        # lava groc / taronja / roig fosc
$P['S']='3b2a2e'                                            # basalt (volcà)
$P['H']='9be35b'; $P['K']='4fa83d'                          # llim clar / fosc

# ---- Generadors de terra i paret (16x16) ----
# Terra: lloses amb junta fosca i taques a l'atzar (sempre iguals: llavor fixa)
function Floor-Rows([int]$seed, [string]$base, [string]$light, [string]$dark, [string]$grout, [string]$spark = '') {
  $rnd = New-Object System.Random $seed
  $rows = @()
  for ($y = 0; $y -lt 16; $y++) {
    $line = ''
    for ($x = 0; $x -lt 16; $x++) {
      $isGrout = ($y -eq 15) -or ($x -eq 15) -or ($y -eq 7) -or (($y -lt 7) -and ($x -eq 7)) -or (($y -gt 7) -and ($x -eq 3 -or $x -eq 11))
      $r = $rnd.NextDouble()
      if ($isGrout) { $c = $grout }
      elseif ($spark -ne '' -and $r -lt 0.03) { $c = $spark }
      elseif ($r -lt 0.14) { $c = $light }
      elseif ($r -lt 0.24) { $c = $dark }
      else { $c = $base }
      $line += $c
    }
    $rows += $line
  }
  return ,$rows
}

# Paret vista des de dalt: 5 files de "sostre" clar i davant de maons
function Wall-Rows([string]$top, [string]$topDark, [string]$front, [string]$frontDark, [string]$mortar) {
  $rows = @()
  for ($y = 0; $y -lt 16; $y++) {
    $line = ''
    for ($x = 0; $x -lt 16; $x++) {
      if ($y -lt 4) { $c = if (($x + $y) % 7 -eq 0) { $topDark } else { $top } }
      elseif ($y -eq 4) { $c = 'k' }
      else {
        $row = [Math]::Floor(($y - 5) / 4)
        $off = if ($row % 2 -eq 0) { 0 } else { 4 }
        $isMortar = ((($y - 5) % 4) -eq 3) -or ((($x + $off) % 8) -eq 7)
        $c = if ($isMortar) { $mortar } elseif ($y -gt 12) { $frontDark } else { $front }
      }
      $line += $c
    }
    $rows += $line
  }
  return ,$rows
}

# Lava: soroll de tres tons
function Lava-Rows([int]$seed) {
  $rnd = New-Object System.Random $seed
  $rows = @()
  for ($y = 0; $y -lt 16; $y++) {
    $line = ''
    for ($x = 0; $x -lt 16; $x++) {
      $w = [Math]::Sin(($x + $y * 0.7) * 0.9) + $rnd.NextDouble() * 0.8
      $c = if ($w -gt 1.2) { 'F' } elseif ($w -gt 0.2) { 'f' } else { 't' }
      $line += $c
    }
    $rows += $line
  }
  return ,$rows
}

$temes = [ordered]@{
  #           terra: base, clar, fosc, junta, espurna | paret: dalt, dalt fosc, davant, davant fosc, morter
  'Cova'   = @('O','P','M','M','',  'P','O','O','M','k')
  'Desert' = @('E','D','T','T','',  'D','E','E','T','T')
  'Neu'    = @('N','W','I','I','',  'W','I','I','J','J')
  'Volca'  = @('S','M','k','k','f', 'O','M','M','S','k')
}
$seed = 11
foreach ($t in $temes.Keys) {
  $c = $temes[$t]
  $sprites["Masmorra/${t}_Terra1"] = @{ size = 16; rows = (Floor-Rows $seed $c[0] $c[1] $c[2] $c[3] $c[4]) }
  $sprites["Masmorra/${t}_Terra2"] = @{ size = 16; rows = (Floor-Rows ($seed + 7) $c[0] $c[2] $c[1] $c[3] $c[4]) }
  $sprites["Masmorra/${t}_Paret"]  = @{ size = 16; rows = (Wall-Rows $c[5] $c[6] $c[7] $c[8] $c[9]) }
  $seed += 13
}
$sprites['Masmorra/Lava'] = @{ size = 16; rows = (Lava-Rows 5) }

# Porta de barrots (es tanca quan entres a una sala amb enemics)
$porta = @()
for ($y = 0; $y -lt 16; $y++) {
  if ($y -eq 0 -or $y -eq 15) { $porta += 'kkkkkkkkkkkkkkkk' }
  elseif ($y -eq 1 -or $y -eq 14) { $porta += 'kddddddddddddddk' }
  else { $porta += 'kMlmMMlmMMlmMMlk' }
}
$sprites['Masmorra/Porta'] = @{ size = 16; rows = $porta }

# ---- Objectes del mapa (16x16) ----
$sprites['Masmorra/Caixa'] = @{ size = 16; mirror = $true; rows = @(
  '........',
  '........',
  '..kkkkkk',
  '..kyyyyy',
  '..kyyooo',
  '..kyoyoo',
  '..kyooyo',
  '..kyoooy',
  '..kywwwy',
  '..kywwyw',
  '..kywyww',
  '..kyywww',
  '..kyyyyy',
  '..kkkkkk',
  '........',
  '........'
)}

$sprites['Masmorra/Barril'] = @{ size = 16; mirror = $true; rows = @(
  '........',
  '........',
  '....kkkk',
  '...krRRR',
  '..krrrrr',
  '..kdmmmm',
  '..krprrr',
  '..krprrr',
  '..kYkYkY',
  '..krprrr',
  '..krrrrr',
  '..kdmmmm',
  '..kRrrrr',
  '...kRRRR',
  '....kkkk',
  '........'
)}

$punxesBaix = @(
  '........',
  '.kkkkkkk',
  '.kOOOOOO',
  '.kOMMOOO',
  '.kOOOOOO',
  '.kOOOOMM',
  '.kOOOOOO',
  '.kOMMOOO',
  '.kOOOOOO',
  '.kOOOOMM',
  '.kOOOOOO',
  '.kOMMOOO',
  '.kOOOOOO',
  '.kOOOOMM',
  '.kkkkkkk',
  '........'
)
$sprites['Masmorra/Punxes_Baix'] = @{ size = 16; mirror = $true; rows = $punxesBaix }
$sprites['Masmorra/Punxes_Dalt'] = @{ size = 16; mirror = $true; rows = @(
  '........',
  '.kkkkkkk',
  '.kOllOOO',
  '.kOggOOO',
  '.kOOOOll',
  '.kOOOOgg',
  '.kOllOOO',
  '.kOggOOO',
  '.kOOOOll',
  '.kOOOOgg',
  '.kOllOOO',
  '.kOggOOO',
  '.kOOOOll',
  '.kOOOOgg',
  '.kkkkkkk',
  '........'
)}

# ---- Enemics ----
$sprites['Enemics/Llim'] = @{ size = 16; mirror = $true; rows = @(
  '........',
  '........',
  '........',
  '........',
  '......kk',
  '....kkHH',
  '...kHHHH',
  '..kHWHHH',
  '..kHHkkH',
  '.kHHHkkH',
  '.kHHHHHH',
  '.kKHHHHH',
  '.kKKHHHH',
  '..kKKKKK',
  '...kkkkk',
  '........'
)}

$sprites['Enemics/Tirador'] = @{ size = 16; mirror = $true; rows = @(
  '........',
  '......kk',
  '....kkvv',
  '...kvvvv',
  '..kvvVVV',
  '..kvVkkk',
  '..kvkkFk',
  '..kVkkkk',
  '..kVVkkk',
  '.kvvVVVV',
  '.kvVVVVV',
  'kvvVVVVV',
  'kvVVVVVV',
  'kVVVVVVV',
  '.kkkkkkk',
  '........'
)}

$sprites['Enemics/Kamikaze'] = @{ size = 16; mirror = $true; rows = @(
  '........',
  '.......F',
  '.......y',
  '.....kkk',
  '...kkddd',
  '..kdlddd',
  '..kllddd',
  '.kdlddrd',
  '.kddddrd',
  '.kdddddd',
  '.kmddddd',
  '.kmmdddd',
  '..kmmddd',
  '...kkmmm',
  '.....kkk',
  '........'
)}

$sprites['Enemics/ReiLlim'] = @{ size = 32; mirror = $true; rows = @(
  '................',
  '................',
  '..........kk.kkk',
  '..........kYkYYY',
  '..........kYYYrY',
  '..........kYYYYY',
  '.......kkkkkkkkk',
  '.....kkHHHHHHHHH',
  '....kHHHHHHHHHHH',
  '...kHHWWHHHHHHHH',
  '..kHHWHHHHHHHHHH',
  '..kHHHHHkkkHHHHH',
  '.kHHHHHHkkWHHHHH',
  '.kHHHHHHkkkHHHHH',
  '.kHHHHHHHHHHHHHH',
  'kKHHHHHHHHHHHkkk',
  'kKKHHHHHHHHHHHkk',
  'kKKHHHHHHHHHHHHH',
  'kKKKHHHHHHHHHHHH',
  '.kKKKHHHHHHHHHHH',
  '.kKKKKKHHHHHHHHH',
  '..kkKKKKKKKKKKKK',
  '....kkkkkkkkkkkk',
  '................'
)}

$sprites['Enemics/BalaEnemiga'] = @{ size = 8; rows = @(
  '.kkkk.',
  'kpprrk',
  'kprrrk',
  'krrrRk',
  'krrRRk',
  '.kkkk.'
)}

# ---- Armes noves (32 px) i projectils ----
$sprites['Armes/Llancagranades'] = @{ size = 32; rows = @(
  '..........kkkkkkkkkkkkkkkkkkkkk.',
  '..........keeeeeeeeeeeeeeeeeeemk',
  'kkkkkkkkkkkeLLLLLLLLLLLLLLLLLLmk',
  'kooooooookdkLLLLLLLLLLLLLLLLLLmk',
  'kwwwwwwwwkdkkkkkkkkkkkkkkkkkkkkk',
  'kkkkk.kkkdddk..kddk.............',
  '......kwwkkk...kddk.............',
  '......kkkk......kk..............'
)}

$sprites['Armes/Ballesta'] = @{ size = 32; rows = @(
  '........................kk......',
  '.......................kyyk.....',
  '.....................kgkkyk.....',
  '...................kgk..kyk.....',
  'kkkkkkkkkkkkkkkkkkkkkkkkkykkkk..',
  'koooooooooooooooommmmmmmmyllllk.',
  'kwwwwwwwwwwwwwwwwkkkkkkkkykkkk..',
  'kkkkk.kwwk.......kgk....kyk.....',
  '......kwwk.........kgk..kyk.....',
  '......kkkk...........kgkkyk.....',
  '.......................kyyk.....',
  '........................kk......'
)}

$sprites['Armes/Llancaflames'] = @{ size = 32; rows = @(
  '....kkkkkkkk....................',
  '...kRrrrrrrRk...................',
  '...kRRRRRRRRk...................',
  'kkkkkkkkkkkkkkkkkkkkkkkkkkkk....',
  'kooookllllllllllllllllldddddkk..',
  'kwwwwkmmmmmmmmmmmmmmmmmdddddkFk.',
  'kkkkkkkkkkkkkkkkkkkkkkkkkkkk....',
  '.kwwk...kddk....................',
  '.kkkk....kk.....................'
)}

$sprites['Armes/Granada'] = @{ size = 8; rows = @(
  '.kkkk.',
  'keeLLk',
  'keLLLk',
  'kLLLLk',
  '.kkkk.'
)}

$sprites['Armes/Fletxa'] = @{ size = 16; rows = @(
  'k...........k.',
  'kgwwwwwwwwwwlk',
  'k...........k.'
)}

# ---- Barrets (personalització) ----
$buit = '........'
$sprites['Barrets/Corona'] = @{ size = 16; mirror = $true; rows = @(
  $buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,
  '...k.k.k',
  '...kYkYk',
  '...kYYYY',
  '...kYrYY',
  '...kuuuu',
  '...kkkkk'
)}
$sprites['Barrets/Gorra'] = @{ size = 16; rows = @(
  '................','................','................','................','................',
  '................','................','................','................','................',
  '....kkkkkk......',
  '...kbbbbbbk.....',
  '..kbbWbbbbbk....',
  '..kbbbbbbbbk....',
  '..kBBBBBBBBBkkk.',
  '..kkkkkkkkkkbbbk'
)}
$sprites['Barrets/Banyes'] = @{ size = 16; mirror = $true; rows = @(
  $buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,
  '.k......',
  '.kgk....',
  '..kgk...',
  '..kgkkkk',
  '...kmlll',
  '...kdmmm',
  '...kkkkk'
)}
$sprites['Barrets/Aureola'] = @{ size = 16; mirror = $true; rows = @(
  $buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,
  '...kkkkk',
  '..kYYYYY',
  '...kkkkk'
)}
$sprites['Barrets/Bolet'] = @{ size = 16; mirror = $true; rows = @(
  $buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,$buit,
  '....kkkk',
  '..kkrrrr',
  '.krrWWrr',
  '.krWWrrr',
  'krrrrrWW',
  'kRRRRRRR',
  '.kkkkkkk'
)}

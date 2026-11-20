# Alpha v1.3.2: barret de Cuiner arreglat i sprites propis per a la torreta, les mines, el tir curatiu i el ganxo.
# Usa les funcions de expansio131.ps1.

function Sprite132($name, $w, $h, $size, [scriptblock]$draw) {
  $g = G131 $w $h
  & $draw $g
  Outline131 $g
  $script:sprites["Armes/$name"] = @{ size = $size; rows = (Rows131 $g) }
}

# Cuiner: el toque és simètric (abans la vora de baix era més curta a la dreta)
$sprites['Barrets/Cuiner'] = @{ size = 16; rows = @(
'................',
'....kkk..kkk....',
'...kWWWkkWWWk...',
'..kWWgWWWWgWWk..',
'..kWWWWWWWWWWk..',
'..kWgWWWWWWgWk..',
'...kWWWWWWWWk...',
'...kWWWWWWWWk...',
'...kggggggggk...',
'...kkkkkkkkkk...',
'................',
'................',
'................',
'................',
'................',
'................'
) }

# Torreta: cúpula metàl·lica sobre una base; el canó és un altre sprite que gira cap a l'enemic
Sprite132 'TorretaCos' 14 12 16 { param($g)
  Rect131 $g 1 9 12 2 'd'; Rect131 $g 2 8 10 1 'm'
  Disc131 $g 6.5 5.5 5 4.5 'm'; Disc131 $g 5.5 4 2.8 2.2 'l'
  Rect131 $g 5 7 4 1 'd'; Px131 $g 6 2 'W'
  Px131 $g 2 9 'l'; Px131 $g 11 9 'l'
}
Sprite132 'TorretaCano' 12 5 16 { param($g)
  Rect131 $g 0 1 8 3 'd'; Rect131 $g 0 1 8 1 'm'
  Rect131 $g 8 0 3 5 'l'; Rect131 $g 10 1 1 3 'r'
}
# Mina: disc fosc amb punxes i una llum a dalt que parpelleja quan està armada
Sprite132 'MinaCos' 12 12 16 { param($g)
  Disc131 $g 5.5 5.5 4.6 4.6 'd'; Disc131 $g 5.5 5.5 3.2 3.2 'm'; Disc131 $g 5.5 5.5 1.8 1.8 'd'
  foreach ($p in @(@(5,0),@(6,0),@(5,11),@(6,11),@(0,5),@(0,6),@(11,5),@(11,6),@(1,1),@(10,1),@(1,10),@(10,10))) { Px131 $g $p[0] $p[1] 'l' }
  Px131 $g 4 4 'W'
}
Sprite132 'MinaLlum' 4 4 8 { param($g)
  Disc131 $g 1.5 1.5 1.6 1.6 'r'; Px131 $g 1 1 'Y'
}
# Tir de la Vareta curativa: bola verda amb una creu blanca (es distingeix de la vareta normal)
Sprite132 'CuraBala' 10 10 16 { param($g)
  Disc131 $g 4.5 4.5 4.4 4.4 'e'; Disc131 $g 4.5 4.5 3 3 'L'
  Rect131 $g 4 2 2 6 'W'; Rect131 $g 2 4 6 2 'W'
}
# Harpó del Ganxo: cap de ganxo amb dues pues, apuntant a la dreta
Sprite132 'GanxoHarpo' 22 11 32 { param($g)
  Line131 $g 0 5 14 5 'l'; Line131 $g 0 4 14 4 'g'
  Line131 $g 14 5 19 1 'W'; Line131 $g 14 5 19 9 'W'; Line131 $g 15 5 20 1 'l'; Line131 $g 15 5 20 9 'l'
  Rect131 $g 13 3 3 5 'm'; Px131 $g 21 5 'W'; Rect131 $g 17 4 4 3 'l'
  Disc131 $g 1 4.5 1.6 1.6 'Y'
}

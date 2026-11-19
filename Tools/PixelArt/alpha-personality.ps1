# Personatges a 16 px: paleta/cos comuns, cares i siluetes deliberadament diferents.
function Stamp-Face($grid, [string[]]$pattern, [int]$ox=3, [int]$oy=4) {
  for($fy=0;$fy -lt $pattern.Count;$fy++) { for($fx=0;$fx -lt $pattern[$fy].Length;$fx++) {
    $ch=$pattern[$fy][$fx]; if($ch -ne '.') {$grid[$oy+$fy][$ox+$fx]=$ch}
  }}
}
$personalities=@(
  @{ name='Vidre'; fill='h'; shade='b'; light='W'; x=4; y=3; face=@('W.......','W...bb..','.W..b...','..k..k..','..k..k..','........','...kkk..','.......W','......W.') },
  @{ name='Ninja'; fill='d'; shade='k'; light='m'; x=2; y=4; face=@('rrrrrrrrrrrr','YYYYYYYYYYYY','YYkkYYYYkkYY','YYYkYYYYkYYY','rrrrrrrrrrrr','............','....kkkk....') },
  @{ name='Fantasma'; fill='W'; shade='g'; light='W'; x=3; y=4; face=@('..........','..kk..kk..','...k..k...','..........','....kk....','...khhk...','....kk....','..........','g........g') },
  @{ name='Cosmos'; fill='V'; shade='M'; light='v'; x=3; y=3; face=@('...h......','..hhh.....','...h....v.','.......vvv','..kk..kkv.','..hk..kh..','..........','....kk....','.v......h.') },
  @{ name='Corall'; fill='r'; shade='R'; light='p'; x=3; y=3; face=@('.p.....p..','.p.p...pp.','..ppp..p..','...p......','..kk..kk..','..........','.p......p.','..k....k..','...kkkk...') },
  @{ name='Menta'; fill='H'; shade='L'; light='H'; x=3; y=2; face=@('....LL....','...LeL....','..LeeLL...','.LeeLL....','..LLL.....','..kk..kk..','..........','...k..k...','....kk....') },
  @{ name='Obsidiana'; fill='M'; shade='k'; light='V'; x=3; y=3; face=@('.VV.......','.VVV....V.','..VV...VV.','.......V..','..vk..kv..','..vk..kv..','..........','...kkkk...','.....VV...') },
  @{ name='Bombolla'; fill='h'; shade='b'; light='W'; x=3; y=3; face=@('W.........','.W......W.','.......W..','..kk..kk..','..kk..kk..','..........','....kk....','....kk..b.','.......b.b','........b.') },
  @{ name='Aurora'; fill='V'; shade='b'; light='h'; x=2; y=3; face=@('vv........v.','vvv......vv.','hvvv...vvvh.','hhvvv.vvvhh.','.hkkvvvkkh..','..kk...kk...','............','.....kk.....') },
  @{ name='Coure'; fill='z'; shade='Z'; light='x'; x=3; y=3; face=@('ZZZZZZZZZZ','..x....x..','..........','..kk..kk..','..xk..kx..','..........','..kkkkkk..','..kZxZxk..','..kkkkkk..') },
  @{ name='Caramel'; fill='x'; shade='y'; light='W'; x=3; y=3; face=@('yyyyyyyyyy','oyyoyyyoyo','o..o...o.o','..........','..kk..kk..','..kk..kk..','..........','...kkkk...','....oo....') },
  @{ name='Llima'; fill='e'; shade='L'; light='H'; x=3; y=3; face=@('HHHHHHHHHH','H....H...H','.H..H...H.','..........','..kk..kk..','..........','..k...k...','...kkk....','...H..H...') },
  @{ name='Safir'; fill='b'; shade='B'; light='h'; x=3; y=3; face=@('..hhhhhh..','.h......h.','h..b..b..h','h.kk..kk.h','.hkk..kkh.','..h....h..','...kkkk...','....hh....') },
  @{ name='Perla'; fill='c'; shade='C'; light='W'; x=3; y=3; face=@('..W.......','.WWW......','..W....W..','......WWW.','..kk..kk..','.......W..','..........','...k..k...','....kk....') },
  @{ name='Ciclope'; fill='v'; shade='V'; light='v'; x=3; y=4; face=@('...kkkk...','..kYYYYk..','..kYkkYk..','..kYkkYk..','...kkkk...','..........','...kkkk...') },
  @{ name='Gat'; fill='x'; shade='z'; light='x'; x=3; y=4; face=@('..........','..kk..kk..','..kk..kk..','....oo....','kk..kk..kk','.kkk..kkk.','....kk....') },
  @{ name='Gota'; fill='b'; shade='B'; light='h'; x=3; y=4; face=@('.h........','hh........','..kk..kk..','..kk..kk..','..........','...k..k...','....kk....') }
)
foreach ($spec in $personalities) {
  $half = Skin $spec.fill $spec.shade $spec.light @() $false
  $full = Expand-Rows @{ rows=$half; mirror=$true }
  $grid = @($full | ForEach-Object { ,($_.ToCharArray()) })
  Stamp-Face $grid $spec.face $spec.x $spec.y
  if ($spec.name -eq 'Gat') {
    $grid[0] = '...kk......kk...'.ToCharArray()
    $grid[1] = '..kxxkkkkkkxxk..'.ToCharArray()
    $grid[2] = '..kxoxxxxxxoxk..'.ToCharArray()
  }
  if ($spec.name -eq 'Gota') {
    $grid[0] = '.......kk.......'.ToCharArray()
    $grid[1] = '......kbbk......'.ToCharArray()
    $grid[2] = '.....kbbbbk.....'.ToCharArray()
    $grid[3] = '...kkhbbbbhkk...'.ToCharArray()
  }
  if ($spec.name -eq 'Fantasma') {
    $grid[13] = '..kkggkkggkkgk..'.ToCharArray()
    $grid[14] = '....kk..kk..kk..'.ToCharArray()
    $grid[15] = '................'.ToCharArray()
  }
  $sprites['Skins/'+$spec.name] = @{ size=16; rows=@($grid | ForEach-Object { -join $_ }) }
}
# Graelles de la Pre-Alpha. L'or conserva el cos i usa dos tons daurats a la cara.
$P['0']='794407'
$P['1']='ffe198'
$sprites['Skins/Sindria']=@{size=16;rows=@(
'.....kkkkkk.....',
'...kLLrrrrLLk...',
'..eLprrrrrrpLe..',
'.krpprrrrrrpprk.',
'.krrkkkrrkkkrrk.',
'krrrrkkrrkkrrrrk',
'krrrrkkrrkkrrrrk',
'krrrrrrrrrrrrrrk',
'krkrrrrrrrrrrkrk',
'krrkkkkkkkkkkrrk',
'krrkkkkkkkkkkrrk',
'.kRrkkkkkkkkrRk.',
'.kRrrkkkkkkrrRk.',
'..LLRRRRRRRRLL..',
'...kkLRRRRLkk...',
'.....kkkkkk.....')}
$sprites['Skins/Capvespre']=@{size=16;rows=@(
'.....kkkkkk.....',
'...kkffffffkk...',
'..kYYWffffWYYk..',
'.kfYYffffffYYfk.',
'.kfffkkffkkfffk.',
'kffffkkffkkffffk',
'kffffkkffkkffffk',
'kffffffffffffffk',
'kfWffffffffffWfk',
'kffkkkkkkkkkkffk',
'kffkkkkkkkkkkffk',
'.kVfkkkkkkkkfVk.',
'.kVffkkkkkkffVk.',
'..kVVVVVVVVVVk..',
'...kkVVVVVVkk...',
'.....kkkkkk.....')}
$P['@']='9a6016' # Reflex d'or dins dels ulls i la boca.
$sprites['Skins/Or']=@{size=16;rows=@(
'.....kkkkkk.....',
'...kkYY1YYYkk...',
'..kWWY111YYWWk..',
'.kYWWYY1YYWYWYk.',
'.kYYY00YY00YYYk.',
'kYYYY@0YY@0YYYYk',
'kYYYY00YY00YYYYk',
'kYYYYYYYYYYYYYYk',
'kWYYYYYYYYYYYYWk',
'kYY0000000000YYk',
'kYY00@@@@@000YYk',
'.kuY00000000Yuk.',
'.kuYY000000YYuk.',
'..kuuuuuuuuuuk..',
'...kkuuuuuukk...',
'.....kkkkkk.....')}
# Segona passada de skins, amb silueta i cara editables en una graella de 16×16.
# Glaço tiritant: ulls de la Classica, dents i facetes de gel.
$P['9']='588ab7' # Vora del glaço, amb menys contrast.
$P['2']='182d19'
$P['3']='347837'
$P['4']='469846'
$P['8']='60b44f'
$P['5']='172018'
$P['6']='294421'
$P['7']='547830'
$sprites['Skins/Gel']=@{size=16;rows=@(
  '.....kkkkkk.....',
  '...kkIIhhhkkk...',
  '..kWWIIhhhIWWk..',
  '.kIWWIhIIhhWh9k.',
  '.kIIIIhIIIhh9Jk.',
  'kIIIIkkIIkkhI9Jk',
  'kIIIIkkIIkkIh9Jk',
  'kIIIIkkIIkkIh9Jk',
  'kIIIIIIIIIIIJ9Jk',
  'kIhIIkkkkkkIJ9Jk',
  'kIIIkWkWkWkkJ9Jk',
  '.kJIkkkkkkkJ9Jk.',
  '.kJJIIIhJJJ9JJk.',
  '..kJJIhJJ999Jk..',
  '...kkJJJJJJkk...',
  '.....kkkkkk.....')}
# Zombi: graella del sprite del mal enemic, amb paleta verda.
$sprites['Skins/Zombi']=@{size=16;rows=@(
  '.....222222.....',
  '...2233333322...',
  '..234434443332..',
  '.23348488444332.',
  '.23466888866432.',
  '2344676886764432',
  '2344666886668432',
  '2344888888888432',
  '2348888888884432',
  '2345685556866432',
  '2345555556655332',
  '.23355555665432.',
  '.23345466674332.',
  '..233344434432..',
  '...2233333322...',
  '.....222222.....'
)}
$sprites['Skins/Robot']=@{size=16;rows=@(
  '.....kkkkkk.....',
  '...kkllllllkk...',
  '..klWWgggglllk..',
  '.klWgmmmmgmmmlk.',
  '.klgkmmmmmmkmlk.',
  'klgkBBBBBBBBkmlk',
  'klgkbhhhbhhhkmlk',
  'klgkbhbbbhbbkmlk',
  'klgkBBBBBBBBkmlk',
  'klgmmmmmmmmmmmlk',
  'klgmkkllllkkmmlk',
  '.klmkkkkkkkkmlk.',
  '.klmmmmmmmmmdlk.',
  '..kdmdmddmdmdk..',
  '...kkddddddkk...',
  '.....kkkkkk.....'
)}
$sprites['Skins/Foc']=@{size=16;rows=@(
  '.......kk.......',
  '....k.kFfk.k....',
  '...kFkkFFfkkk...',
  '...kFFWFFffkk...',
  '..kFFWWFFffffk..',
  '.kkFFWkFFkffftk.',
  'kFFYFkkFFkkffftk',
  'kFYYFkkFFkkffftk',
  'kFYYYYFFFFfffttk',
  'kffYkkkFFkkkfttk',
  'kffYFkkkkkkffttk',
  '.kffftFtttffttk.',
  '.kffttttttttttk.',
  '..kfftttttrttk..',
  '...kktttttkkk...',
  '.....kkkkkk.....'
)}
$sprites['Barrets/Astronauta']=@{size=16;rows=@(
'.....kkkkkk.....','...kkWWWWWWkk...','..kWWWggggWWWk..','.kWWWh......WWk.','kWWWh........WWk','kWWh..........Wk','kWW...........Wk','kW............Wk','kW............Wk','kW............Wk','kWW..........WWk','.kWW........WWk.','..kWWWWWWWWWWk..','...kgbggbbbgk...','....kkkkkkkk....','................')}
$sprites['Barrets/Bruixa']=@{size=16;rows=@('........k.......','.......kVk......','.......kVvk.....','......kVVvk.....','......kVVVvk....','.....kVVVVvk....','.....kVVVVVvk...','....kVVVVVVvk...','....krrrrrrrk...','...kkkkkkkkkk...','..kVVVVVVVVVVk..','.kkkkkkkkkkkkkk.','................','................','................','................')}
$sprites['Barrets/Explorador']=@{size=16;rows=@('................','................','......kkkk......','....kkxxxxkk....','...kxyyyyyyxk...','...kxyyyyyyxk...','...kooooooook...','..kkkkkkkkkkkk..','.kxyyyyyyyyyyxk.','..kkkkkkkkkkkk..','................','................','................','................','................','................')}
$sprites['Barrets/Enginyer']=@{size=16;rows=@('................','................','......kkkk......','....kkYYWWkk....','...kYYYYWWYYk...','..kYYYYYWWYYYk..','..kYYYYYWWYYYk..','.kYYYYYYYYYYYYk.','.kkkkkkkkkkkkkk.','................','................','................','................','................','................','................')}
$sprites['Barrets/Capita']=@{size=16;rows=@('................','................','....kkkkkkkk....','...kWWWWWWWWk...','..kWWWWYYWWWWk..','..kWWWWYYWWWWk..','...kbbbbbbbbk...','..kkkkkkkkkkkk..','...kbbbbbbbbk...','....kkkkkkkk....','................','................','................','................','................','................')}
# Tres armes amb mecanismes diferents i siluetes editables.
$sprites['Armes/DiscCometa']=@{size=32;rows=@('............kkkkkkkk............','.........kkkllllWWllkkk.........','........kllllllWWllllllk........','.......klllkkkkWWkkkklllk.......','......kllkk....WW....kkllk......','......klk.....kWWk.....klk......','......klk....klWWlk....klk......','......klk...klWWWWlk...klk......','......kllkkkllWWWWllkkkllk......','.......klllllllWWlllllllk.......','........kllllllWWllllllk........','.........kkklllWWlllkkk.........','............kkkkkkkk............')}
$sprites['Armes/LlancaTempesta']=@{size=32;rows=$sword;recolor=(New-Map 'W','h','l','b','y','v')}
$sprites['Armes/VaretaEspores']=@{size=32;rows=@('..................kkkkk.........','................kkHHeeHkk.......','...............kHHeLLeHHk.......','...............kHHeLLeHHk.......','................kkHeeHkk........','..................kkkk..........','.................kyk............','................kyk.............','...............kyk..............','..............kyk...............','.............kyk................','............kyk.................','...........kyk..................','..........kyk...................','.........kkk....................')}
$sprites['Enemics/Escarabat']=@{size=16;rows=@('......kkkk......','....kkggggkk....','...kdgYggYgdk...','..kddddkkddddk..','.kddmmmkkmmmddk.','k.kdmmmkkmmmdk.k','.kkdmmmkkmmmdkk.','..kdmmmkkmmmdk..','.kkdmmmkkmmmdkk.','k.kdmmmkkmmmdk.k','..kddddkkddddk..','...kddddddddk...','....kkddddkk....','.....k.kk.k.....','................','................')}
$sprites['Enemics/Esporer']=@{size=16;rows=@('.....kkkkkk.....','...kkHHHHHHkk...','..kHHWHeeHWHHk..','.kHHHHHeeHHHHHk.','kHHWHeeeeeHWHeHk','kHHHHHeeeeHHHHHk','.kkkkkkkkkkkkkk.','....kcckkcck....','....kckkkkck....','....kcckkcck....','.....kcccck.....','....keeeeeek....','...kkeeeeeekk...','....kkkkkkkk....','................','................')}
$sprites['Masmorra/ArbustBaies']=@{size=16;mirror=$true;rows=@('........','........','.....kkk','...kkLee','..kLeeer','.kLeeere','.kLeeeee','kLeeeree','kLeeeeee','.kLeeeee','..kkLeee','...kkkkk','.....kww','.....kkk','........','........')}
$sprites['Masmorra/BoletLluminós']=@{size=16;mirror=$true;rows=@('........','.....kkk','...kkhHH','..khHHWH','.khHHHHH','khHHWHHH','kHHHHHHH','.kkkkkkk','.....kcc','.....kcc','.....kcW','....kccc','...kkccc','....kkkk','........','........')}
$sprites['Armes/ProjectorPolsos']=@{size=32;rows=@(
  '..........kkkkkkkk..........',
  '.......kkkBBBBBBBBkkk.......',
  '....kkkBhhhhhhhhhhBBBkkk....',
  '...kBBBhWWWhhhhhhBBBBBBkk...',
  '..kkkkkkkkkkkkkkkkkkkkkkkk..',
  '......kBBk....kkkhhhkkk.....',
  '......kBBk.......kkk........',
  '.....kkkkk..................'
)}
$sprites['Armes/BallestaCristall']=@{size=32;rows=@(
  '.................kk.........',
  '..............kkkvk.........',
  '............kkvvVk..........',
  '....kkkkkkkkhvhVkkkkkkkk....',
  '...kvVVVVVVVWhvvWWWhvvvk....',
  '....kkkkkkkkhvhVkkkkkkkk....',
  '............kkvvVk..........',
  '..............kkkvk.........',
  '.................kk.........'
)}

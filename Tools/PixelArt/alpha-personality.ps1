# Personatges a 16 px: paleta/cos comuns, cares i siluetes deliberadament diferents.
function Stamp-Face($grid, [string[]]$pattern, [int]$ox=3, [int]$oy=4) {
  for($fy=0;$fy -lt $pattern.Count;$fy++) { for($fx=0;$fx -lt $pattern[$fy].Length;$fx++) {
    $ch=$pattern[$fy][$fx]; if($ch -ne '.') {$grid[$oy+$fy][$ox+$fx]=$ch}
  }}
}
$personalities=@(
 @('Vidre','b','B','W','sly'), @('Foc','f','t','F','angry'), @('Gel','I','J','W','sleep'),
 @('Zombi','e','L','H','zombie'),
 @('Cosmos','V','M','h','cyclops'), @('Corall','p','R','W','wink'), @('Menta','H','L','W','happy'),
 @('Obsidiana','M','k','v','glow'), @('Bombolla','h','b','W','surprise'),
 @('Aurora','b','V','H','stars'), @('Coure','z','Z','x','robot'), @('Caramel','p','v','W','heart'),
 @('Llima','H','K','Y','tooth'), @('Safir','B','M','h','angry'), @('Perla','c','g','W','sleep'),
 @('Fantasma','W','g','W','ghost'), @('Ciclope','v','V','h','cyclops'), @('Gat','z','Z','x','cat'), @('Gota','b','B','h','happy')
)
foreach($spec in $personalities) {
  $half=Skin $spec[1] $spec[2] $spec[3] @() $false
  $full=Expand-Rows @{ rows=$half;mirror=$true }
  $grid=@($full | ForEach-Object { ,($_.ToCharArray()) })
  switch($spec[4]) {
    'sly' { Stamp-Face $grid @('.kk....kk.','..k....k..','..........','..........','...kkkk...','......k...') }
    'angry' { Stamp-Face $grid @('.kk....kk.','..k....k..','..........','..........','...kkkk...','..k....k..') }
    'sleep' { Stamp-Face $grid @('..........','.kkk..kkk.','..........','..........','....kk....') }
    'zombie' { Stamp-Face $grid @('.k.k...kk.','..k....kk.','.k.k......','..........','..kkkkkk..','...W..W...') }
    'cyclops' { Stamp-Face $grid @('..WWWWW...','.WWbbbWW..','.WWbkbWW..','..WWWWW...','..........','...kkk....') }
    'wink' { Stamp-Face $grid @('..k.......','..k...kkk.','..........','..........','..k....k..','...kkkk...') }
    'happy' { Stamp-Face $grid @('..kk..kk..','.k..kk..k.','..........','..........','...k..k...','....kk....') }
    'glow' { Stamp-Face $grid @('.rr....rr.','..r....r..','..........','..........','....v.....') }
    'surprise' { Stamp-Face $grid @('.WW....WW.','.Wk....kW.','..........','....kk....','...k..k...','....kk....') }
    'stars' { Stamp-Face $grid @('..W....W..','.WWW..WWW.','..W....W..','..........','...kkkk...') }
    'robot' { Stamp-Face $grid @('.kkkkkkkk.','.khhkkhhk.','.kkkkkkkk.','..........','..kWkWkW..') }
    'heart' { Stamp-Face $grid @('.k.k..k.k.','.kkk..kkk.','..k....k..','..........','p..kkkk..p') }
    'tooth' { Stamp-Face $grid @('..k....k..','..........','..........','...kkkk...','....WW....','....WW....') }
    'ghost' { Stamp-Face $grid @('.kkk..kkk.','.kkk..kkk.','..........','....kk....','...kkkk...','....kk....') }
    'cat' { Stamp-Face $grid @('..k....k..','..........','...k..k...','kk..kk..kk','...k..k...') }
  }
  if($spec[0] -eq 'Safir') { $grid[1][5]='.';$grid[1][10]='.';$grid[2][3]='.';$grid[2][12]='.' }
  if($spec[0] -eq 'Fantasma') {foreach($gx in @(3,6,9,12)){$grid[14][$gx]='.';$grid[15][$gx]='.'}}
  if($spec[0] -eq 'Gat') {foreach($point in @(@(3,0),@(12,0),@(2,1),@(13,1),@(3,1),@(12,1),@(3,2),@(12,2))){$grid[$point[1]][$point[0]]='k'}}
  if($spec[0] -eq 'Gota') { $grid[0]='.......kk.......'.ToCharArray();$grid[1]='......kbbk......'.ToCharArray();$grid[2]='.....kbbbbk.....'.ToCharArray() }
  $sprites['Skins/'+$spec[0]]=@{size=16;rows=@($grid | ForEach-Object {-join $_})}
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
$sprites['Skins/Or']=@{size=16;rows=@(
'.....kkkkkk.....',
'...kkYY1YYYkk...',
'..kWWY111YYWWk..',
'.kYWWYY1YYWYWYk.',
'.kYYY00YY00YYYk.',
'kYYYY10YY10YYYYk',
'kYYYY00YY00YYYYk',
'kYYYYYYYYYYYYYYk',
'kWYYYYYYYYYYYYWk',
'kYY0000000000YYk',
'kYY0011111000YYk',
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
$sprites['Skins/Vidre']=@{size=16;rows=@(
  '.....kkkkkk.....',
  '...kkhhhbbbkk...',
  '..khWWhhIhbbbk..',
  '.khWhhIhhhhbBbk.',
  '.khhhhWhhhhhbBk.',
  'kWhhIhkkhhkkhBBk',
  'khhIhhkhhhkhhBBk',
  'khIhWhhhhhhhbBBk',
  'kIhhWhhkkkkhbBBk',
  'khhWhhhkWWkhbBBk',
  'khhhhhhhkkbbbBBk',
  '.khhhhbhbbbBBBk.',
  '.khhhbbbBBBBBBk.',
  '..kbbbbBBBBBBk..',
  '...kkBBBBBBkk...',
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
$sprites['Skins/Fantasma']=@{size=16;rows=@(
  '.....kkkkkk.....',
  '...kkNWWWWNkk...',
  '..kNWWWWWWWWNk..',
  '.kNWWWNNWWWWWNk.',
  '.kNWWkWWWWkWWNk.',
  'kNWWkkWWWkkWWWNk',
  'kNWWhkWWWhkWWWNk',
  'kNWWWWWWWWWWWWNk',
  'kNWhWWkkkkWWWWNk',
  'kNWWWWkhhkWWhWNk',
  'kNhWWWWkkWWWWWNk',
  '.kNWhWWWWWhWWNk.',
  '.khNWWWWWWWWNhk.',
  '..kkNNkkNNkkNhk.',
  '....kk..kk..kk..',
  '................'
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

# Contingut Pre-Alpha: graelles editables, sense imatges externes.
foreach ($variant in @(
  @('Cosmos','V','M','h'), @('Corall','p','R','W'), @('Menta','H','L','W'),
  @('Obsidiana','M','k','v'), @('Bombolla','h','b','W'), @('Capvespre','f','V','Y')
)) {
  $sprites["Skins/$($variant[0])"] = @{ size=16; mirror=$true; rows=(Skin $variant[1] $variant[2] $variant[3] @('2,7,W','10,2,W')) }
}
$sword = @('............................kk..','...........................kWlk.','..........................kWlk..','.........................kWlk...','........................kWlk....','.......................kWlk.....','......................kWlk......','.....................kWlk.......','....................kWlk........','...................kWlk.........','..................kWlk..........','.................kWlk...........','................kWlk............','...............kWlk.............','..............kWlk..............','.............kWlk...............','............kWlk................','...........kWlk.................','..........kWlk..................','.........kWlk...................','.......kkWlk....................','......kYYkk.....................','.......kkYk.....................','......kyk.k.....................','.....kyk........................','....kyk.........................','...kkk..........................','................................','................................','................................','................................','................................')
$sprites['Armes/EspasaVidre']=@{size=32;rows=$sword;recolor=(New-Map 'l','h')}
$sprites['Armes/SabreCorall']=@{size=32;rows=$sword;recolor=(New-Map 'W','p','l','R')}
$sprites['Armes/MartellOrbital']=@{size=32;rows=@('....................kkkkkkkkkk..','...................kvvWWvvvvvvk.','...................kvvWWvvvvvvk.','...................kVVVVVVVVVVk.','....................kkkkmkkkkk..','.......................kmk......','......................kmk.......','.....................kmk........','....................kmk.........','...................kyk..........','..................kyk...........','.................kyk............','................kyk.............','...............kyk..............','..............kkk...............')}
$sprites['Barrets/Astronauta']=@{size=16;mirror=$true;rows=@('........','........','........','........','.....kkk','...kkWWW','..kWWhhh','.kWWhhhh','.kWhhhhh','.kWhhhhh','..kWWhhh','...kkkkk','........','........','........','........')}
$sprites['Barrets/Samurai']=@{size=16;mirror=$true;rows=@('........','........','........','..Y.....','..kY..kk','...kYkRR','....kRRR','...kRRRR','..kkkkkk','........','........','........','........','........','........','........')}
$sprites['Barrets/Cristall']=@{size=16;mirror=$true;rows=@('........','........','.......k','......kh','...k..kh','..khk.kv','..khvkkv','...kvvvv','....kvvv','.....kkk','........','........','........','........','........','........')}
# Ratpenat amb ales segmentades, ulls i orelles; aranya amb huit potes i abdomen.
$sprites['Enemics/Ratpenat']=@{size=16;mirror=$true;rows=@('........','........','.....k..','k...kVk.','Vk..kVVk','VVk.kVvV','VvVkVVrV','VvvVVVVV','kVVkVvvV','.kk.kVVV','.....kVV','......kk','.....k..','........','........','........')}
$sprites['Enemics/Aranya']=@{size=16;mirror=$true;rows=@('........','...k....','....k.kk','.k..kkMM','..k.kMOP','...kkMOO','.kkkOMOO','k...kMOM','...kkMOO','..k.kMrr','.k..kkrr','....k.kk','...k..k.','......k.','........','........')}
# Expansio del taller: sis skins i quatre barrets nous.
foreach($v in @(@('Aurora','b','V','H'),@('Coure','z','Z','x'),@('Caramel','p','v','W'),@('Llima','H','K','Y'),@('Safir','B','M','h'),@('Perla','c','g','W'))) {
  $sprites["Skins/$($v[0])"]=@{size=16;mirror=$true;rows=(Skin $v[1] $v[2] $v[3] @('3,7,W','10,2,W'))}
}
$sprites['Barrets/Pirata']=@{size=16;mirror=$true;rows=@('........','........','........','........','.....kkk','....kkkk','...kkWkk','..kkWWkk','.kkkkkkk','kkkkkkkk','........','........','........','........','........','........')}
$sprites['Barrets/Viking']=@{size=16;mirror=$true;rows=@('........','........','........','.W......','.cW..kkk','..cWklll','...klmmm','...kmmmm','..kddddd','........','........','........','........','........','........','........')}
$sprites['Barrets/Flor']=@{size=16;mirror=$true;rows=@('........','........','........','......pp','.....pWp','......pY','.....epp','......e.','.....e..','........','........','........','........','........','........','........')}
$sprites['Barrets/Copalta']=@{size=16;mirror=$true;rows=@('........','........','....kkkk','....kMMM','....kMOM','....kMMM','....kMMM','....krrr','...kkkkk','........','........','........','........','........','........','........')}
# Minigun de tres canons amb motor, empunyadura i anells de metall.
$mini=@('...........kkkkkkkkkkkkkkkkkk...',
'..........kddddddddddddddddddk..',
'.........kmmlllllllllllllllllmk.',
'..kkkkkk.kmmdddddddddddddddddmkk',
'.kmmmmmmkkmmlllllllllllllllllmk.',
'kmlmmmmmkkmmdddddddddddddddddmkk',
'kmmYYYmmkkmmlllllllllllllllllmk.',
'kmmmmmmmkkmmdddddddddddddddddmkk',
'.kddddddkkkkkkkkkkkkkkkkkkkkkk..',
'..kkkkkk..kdddk...kdddk.........',
'....kyyk..kddk....kddk..........',
'....kyyk...kk......kk...........',
'....kkkk........................') | ForEach-Object {$_.PadRight(32,'.')}
$sprites['Armes/Minigun']=@{size=32;rows=@($mini)}
Add-Type -AssemblyName System.Drawing
$base='C:\Users\Usuario\Canica Alpha\Builds\Art per editar\Blocs prova v1.4';$d="$base\Natura"
$ts=80;$b=New-Object System.Drawing.Bitmap 1000,520;$g=[System.Drawing.Graphics]::FromImage($b);$g.Clear([System.Drawing.Color]::FromArgb(24,26,32));$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half'
$fs='Roca_cova_1','Roca_cova_marro','Roca_cova_gran','Roca_cova_molsa','Mineral_or','Mineral_diamant','Mineral_ametista','Mineral_robi'
$i=0;foreach($n in $fs){$im=[System.Drawing.Image]::FromFile("$d\$n.png");$ox=10+($i%4)*245;$oy=10+[Math]::Floor($i/4)*245;for($y=0;$y -lt 3;$y++){for($x=0;$x -lt 3;$x++){$g.DrawImage($im,$ox+$x*$ts*0.98,$oy+$y*$ts*0.98,$ts*0.98+1,$ts*0.98+1)}};$i++}
$b.Save("$base\_Connexions.png")
function Tile($n,$rot){$im=New-Object System.Drawing.Bitmap "$d\$n.png";for($k=0;$k -lt $rot;$k++){$im.RotateFlip('Rotate90FlipNone')};$im}
$gr=Tile 'Gespa_1' 0;$H=Tile 'Gespa_cami_recte' 0;$V=Tile 'Gespa_cami_recte' 1
$c0=Tile 'Gespa_cami_canto' 0;$c1=Tile 'Gespa_cami_canto' 1;$c2=Tile 'Gespa_cami_canto' 2;$c3=Tile 'Gespa_cami_canto' 3
$map=@(@($c0,$H,$H,$H,$c1),@($V,$gr,$gr,$gr,$V),@($V,$gr,$gr,$gr,$V),@($V,$gr,$gr,$gr,$V),@($c3,$H,$H,$H,$c2))
$m=New-Object System.Drawing.Bitmap 400,400;$g2=[System.Drawing.Graphics]::FromImage($m);$g2.InterpolationMode='NearestNeighbor';$g2.PixelOffsetMode='Half'
for($y=0;$y -lt 5;$y++){for($x=0;$x -lt 5;$x++){$g2.DrawImage($map[$y][$x],$x*80,$y*80,81,81)}}
$m.Save("$base\_Cami_prova.png")

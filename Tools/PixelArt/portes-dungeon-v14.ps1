param([string]$Root="C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace")
Add-Type -AssemblyName System.Drawing
$src="$Root\ArtNou_v1.4\Portes\H";$out="$Root\ArtNou_v1.4\Portes_dungeon"
if(Test-Path $out){Remove-Item $out -Recurse -Force};New-Item -ItemType Directory -Force "$out\H","$out\V" | Out-Null
$types=@("pedra","reixa_castell","preso","lab_metall","perill","vidre");$states=@("tancada","mitja","oberta")
function Stitch($bmp,$part){$b=New-Object System.Drawing.Bitmap 16,16
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$sx=$x
  if($part -ne "esq" -and $x -le 1){$sx=2}      # cantonada interior de l'esquerra: continua la fulla
  if($part -ne "dre" -and $x -ge 14){$sx=13}    # cantonada interior de la dreta
  $b.SetPixel($x,$y,$bmp.GetPixel($sx,$y))}};$b}
$n=0
foreach($t in $types){foreach($st in $states){$f="$src\Porta_$t`_$st.png";if(-not (Test-Path $f)){throw "falta $f"};$bmp=[System.Drawing.Bitmap]::FromFile($f)
 foreach($part in "esq","mig","dre"){$b=Stitch $bmp $part;$b.Save("$out\H\Porta_$t`_$st`_$part.png");$v=$b.Clone();$v.RotateFlip('Rotate90FlipNone');$v.Save("$out\V\Porta_$t`_$st`_$part.png");$n++;$b.Dispose();$v.Dispose()}
 $bmp.Dispose()}}
"$n sprites per orientacio"
# full de contacte: les tres peces seguides
$sc=6;$sheet=New-Object System.Drawing.Bitmap (3*16*$sc+150),($types.Count*3*(16*$sc+8)+10);$g=[System.Drawing.Graphics]::FromImage($sheet);$g.Clear([System.Drawing.Color]::FromArgb(40,44,52));$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half'
$font=New-Object System.Drawing.Font "Consolas",9;$row=0
foreach($t in $types){
 foreach($st in $states){
  $y=10+$row*(16*$sc+8);$g.DrawString("$t $st",$font,[System.Drawing.Brushes]::White,4,$y+40)
  $i=0
  foreach($part in "esq","mig","dre"){
   $im=[System.Drawing.Image]::FromFile("$out\H\Porta_$t`_$st`_$part.png")
   for($qy=0;$qy -lt 16;$qy++){for($qx=0;$qx -lt 16;$qx++){$cc=if(($qx+$qy)%2){[System.Drawing.Color]::FromArgb(70,74,84)}else{[System.Drawing.Color]::FromArgb(52,55,64)};$sb=New-Object System.Drawing.SolidBrush $cc;$g.FillRectangle($sb,150+$i*16*$sc+$qx*$sc,$y+$qy*$sc,$sc,$sc);$sb.Dispose()}}
   $g.DrawImage($im,150+$i*16*$sc,$y,16*$sc,16*$sc);$im.Dispose();$i++
  }
  $row++
 }
}
$g.Dispose();$sheet.Save("$out\_Portes_dungeon.png")

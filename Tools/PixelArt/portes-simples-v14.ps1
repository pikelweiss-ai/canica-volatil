param([string]$Out="C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace\ArtNou_v1.4\Portes_simples")
Add-Type -AssemblyName System.Drawing
function Tp(){$a=New-Object string[] 256;for($i=0;$i -lt 256;$i++){$a[$i]="00000000"};,$a}
function P($a,$x,$y,$c){$x=[int][Math]::Round($x);$y=[int][Math]::Round($y);if($x -ge 0 -and $x -lt 16 -and $y -ge 0 -and $y -lt 16){$a[$y*16+$x]=$c}}
function Rc($a,$x,$y,$w,$h,$c){for($j=$y;$j -lt $y+$h;$j++){for($i=$x;$i -lt $x+$w;$i++){P $a $i $j $c}}}
function Hx($h,[double]$f){$r=[Convert]::ToInt32($h.Substring(0,2),16);$g=[Convert]::ToInt32($h.Substring(2,2),16);$b=[Convert]::ToInt32($h.Substring(4,2),16)
 if($f -ge 0){$r=$r+(255-$r)*$f;$g=$g+(255-$g)*$f;$b=$b+(255-$b)*$f}else{$r=$r*(1+$f);$g=$g*(1+$f);$b=$b*(1+$f)}
 "{0:X2}{1:X2}{2:X2}" -f [int][Math]::Min(255,[Math]::Max(0,$r)),[int][Math]::Min(255,[Math]::Max(0,$g)),[int][Math]::Min(255,[Math]::Max(0,$b))}
function MirX($a){$b=Tp;for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$b[$y*16+(15-$x)]=$a[$y*16+$x]}};,$b}
function MirY($a){$b=Tp;for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$b[(15-$y)*16+$x]=$a[$y*16+$x]}};,$b}
function Swing($deg,$col,$len,$th,$decor,$wall){$a=Tp
 Rc $a 0 4 2 8 $wall;Rc $a 0 4 2 1 (Hx $wall 0.3);Rc $a 0 11 2 1 (Hx $wall -0.35);Rc $a 14 4 2 8 $wall;Rc $a 14 4 2 1 (Hx $wall 0.3);Rc $a 14 11 2 1 (Hx $wall -0.35)
 $rad=$deg*[Math]::PI/180;$co=[Math]::Cos($rad);$si=[Math]::Sin($rad);$hx=2.0;$hy=8.0;$hi=Hx $col 0.25;$dk=Hx $col -0.3
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$dx=$x+0.5-$hx;$dy=$y+0.5-$hy;$u=$dx*$co+$dy*$si;$v=-$dx*$si+$dy*$co;if($u -ge 0 -and $u -le $len -and [Math]::Abs($v) -le $th/2){$c=$col;if($v -lt -$th/4){$c=$hi}elseif($v -gt $th/4){$c=$dk};P $a $x $y $c}}}
 foreach($d in $decor){$u=$d[0];$c=$d[2];$vv=$d[1];P $a ($hx+$u*$co-$vv*$si) ($hy+$u*$si+$vv*$co) $c}
 P $a 2 8 "3A3F48";$a}
function DoorSwing($st,$type){$deg=@(0,-45,-88)[$st];$d=@()
  switch($type){
  "classe"{foreach($u in 4,5,6,7){$d+=,@($u,0,"78B8D0")};$d+=,@(10.5,0.6,"F2C020");Swing $deg "B8885A" 11.6 3 $d "A8A096"}
  "habitacio"{$d+=,@(10.5,0.6,"C8B070");$d+=,@(6,0,"E0DCD2");Swing $deg "F0ECE4" 11.6 3 $d "A8A096"}
  "dormitori"{$d+=,@(10.5,0.6,"F2C020");$d+=,@(5,0,"5E3C20");$d+=,@(8,0,"5E3C20");Swing $deg "7A5230" 11.6 3 $d "8A8278"}
  "bany"{$d+=,@(10.5,0.6,"B8C0C8");$d+=,@(6,0,"78B8D0");Swing $deg "CFE4EE" 11.6 3 $d "A8A096"}
  "despatx"{foreach($u in 4,5,6,7,8){$d+=,@($u,0,"D8F0F8")};$d+=,@(10.5,0.6,"B8C0C8");Swing $deg "A8723C" 11.6 3 $d "8A8278"}}}function DoorSlide($st,$type){$a=Tp;$wall="A8A096"
 Rc $a 0 4 2 8 $wall;Rc $a 14 4 2 8 $wall;Rc $a 0 4 2 1 (Hx $wall 0.3);Rc $a 14 4 2 1 (Hx $wall 0.3);Rc $a 0 11 2 1 (Hx $wall -0.35);Rc $a 14 11 2 1 (Hx $wall -0.35);Rc $a 2 5 12 1 "5A5650";Rc $a 2 10 12 1 "5A5650"
 $sh=@(0,6,11)[$st]
 for($x=2;$x -le 13;$x++){$xx=$x-$sh;if($xx -lt 2){continue};for($y=6;$y -le 9;$y++){
  switch($type){
   "fusta"{$c="B8885A";if($y -eq 6){$c="D8A878"}elseif($y -eq 9){$c="8A5E36"}elseif($x%4 -eq 0){$c="A07448"}}
   "paper"{$c="F4F0E0";if($y -eq 6 -or $y -eq 9){$c="8A5A30"}elseif($x%4 -eq 0){$c="B8905A"}elseif($y -eq 7){$c="FAF6EA"}}
   "vidre"{$c="BFE9F2A0";if($y -eq 6 -or $y -eq 9 -or $x -eq 2 -or $x -eq 13){$c="E8F8FFE0"}}}
  P $a $xx $y $c}}
 if($st -ne 2){P $a (11-$sh) 7 "F2C020"};$a}
$types=@("classe","habitacio","dormitori","bany","despatx")
$slides=@("fusta","paper","vidre")
$stn=@("tancada","mitja","oberta")
if(Test-Path $Out){Get-ChildItem $Out -Recurse -File | Remove-Item -Force}
New-Item -ItemType Directory -Force "$Out\H","$Out\V" | Out-Null
function Save($px,$name){$bmp=New-Object System.Drawing.Bitmap 16,16
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$h=$px[$y*16+$x];$al=if($h.Length -ge 8){[Convert]::ToInt32($h.Substring(6,2),16)}else{255};$bmp.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($al,[Convert]::ToInt32($h.Substring(0,2),16),[Convert]::ToInt32($h.Substring(2,2),16),[Convert]::ToInt32($h.Substring(4,2),16)))}}
 $bmp.Save("$Out\H\$name.png");$v=$bmp.Clone();$v.RotateFlip('Rotate90FlipNone');$v.Save("$Out\V\$name.png");$v.Dispose();$bmp}
$sc=6;$cw=16*$sc+10;$ch=16*$sc+12;$cols=6;$rows=$types.Count+$slides.Count
$sheet=New-Object System.Drawing.Bitmap (130+$cols*$cw),($rows*$ch+24);$g=[System.Drawing.Graphics]::FromImage($sheet);$g.Clear([System.Drawing.Color]::FromArgb(40,44,52));$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half'
$font=New-Object System.Drawing.Font "Consolas",9;$br=[System.Drawing.Brushes]::White;$row=0;$total=0
function Cell($bmp,$col,$row){$cx=130+$col*$cw;$cy=22+$row*$ch;for($qy=0;$qy -lt 16;$qy++){for($qx=0;$qx -lt 16;$qx++){$cc=if(($qx+$qy)%2){[System.Drawing.Color]::FromArgb(70,74,84)}else{[System.Drawing.Color]::FromArgb(52,55,64)};$sb=New-Object System.Drawing.SolidBrush $cc;$g.FillRectangle($sb,$cx+$qx*$sc,$cy+$qy*$sc,$sc,$sc);$sb.Dispose()}};$g.DrawImage($bmp,$cx,$cy,16*$sc,16*$sc)}
foreach($t in $types){$g.DrawString("Porta_$t",$font,$br,6,22+$row*$ch+40)
 for($st=0;$st -lt 3;$st++){$base=DoorSwing $st $t
  $variants=[ordered]@{"esq_amunt"=$base;"dre_amunt"=(MirX $base);"esq_avall"=(MirY $base);"dre_avall"=(MirY (MirX $base))}
  foreach($k in $variants.Keys){$bmp=Save $variants[$k] "Porta_$t`_$k`_$($stn[$st])";$total++;if($k -eq "esq_amunt"){Cell $bmp $st $row}elseif($k -eq "dre_amunt"){Cell $bmp (3+$st) $row};$bmp.Dispose()}}
 $row++}
foreach($t in $slides){$g.DrawString("Corredissa_$t",$font,$br,6,22+$row*$ch+40)
 for($st=0;$st -lt 3;$st++){$base=DoorSlide $st $t;$variants=[ordered]@{"esq"=$base;"dre"=(MirX $base)}
  foreach($k in $variants.Keys){$bmp=Save $variants[$k] "Porta_corredissa_$t`_$k`_$($stn[$st])";$total++;if($k -eq "esq"){Cell $bmp $st $row}else{Cell $bmp (3+$st) $row};$bmp.Dispose()}}
 $row++}
$g.DrawString("bisagra esquerra: tancada / mitja / oberta",$font,$br,130,2);$g.DrawString("bisagra dreta: tancada / mitja / oberta",$font,$br,130+3*$cw,2)
$g.Dispose();$sheet.Save("$Out\_Portes_simples.png");"$total portes"



param([string]$Out="C:\Users\Usuario\Canica Alpha\Builds\Art per editar\Enemics nous v1.4")
Add-Type -AssemblyName System.Drawing
function Mk($c){$a=New-Object string[] 256;for($i=0;$i -lt 256;$i++){$a[$i]=$c};,$a}
function Tp(){Mk "00000000"}
function P($a,$x,$y,$c){$x=[int][Math]::Round($x);$y=[int][Math]::Round($y);if($x -ge 0 -and $x -lt 16 -and $y -ge 0 -and $y -lt 16){$a[$y*16+$x]=$c}}
function Rc($a,$x,$y,$w,$h,$c){for($j=$y;$j -lt $y+$h;$j++){for($i=$x;$i -lt $x+$w;$i++){P $a $i $j $c}}}
function Disc($a,$cx,$cy,$r,$c){for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if((($x-$cx)*($x-$cx)+($y-$cy)*($y-$cy)) -le $r*$r){P $a $x $y $c}}}}
function Ell($a,$cx,$cy,$rx,$ry,$c){for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$u=($x-$cx)/$rx;$v=($y-$cy)/$ry;if(($u*$u+$v*$v) -le 1){P $a $x $y $c}}}}
function Ln($a,$x0,$y0,$x1,$y1,$c){$n=[Math]::Max([Math]::Abs($x1-$x0),[Math]::Abs($y1-$y0));if($n -eq 0){P $a $x0 $y0 $c;return};for($i=0;$i -le $n;$i++){P $a ($x0+($x1-$x0)*$i/$n) ($y0+($y1-$y0)*$i/$n) $c}}
function Hx($h,[double]$f){$r=[Convert]::ToInt32($h.Substring(0,2),16);$g=[Convert]::ToInt32($h.Substring(2,2),16);$b=[Convert]::ToInt32($h.Substring(4,2),16)
 if($f -ge 0){$r=$r+(255-$r)*$f;$g=$g+(255-$g)*$f;$b=$b+(255-$b)*$f}else{$r=$r*(1+$f);$g=$g*(1+$f);$b=$b*(1+$f)}
 "{0:X2}{1:X2}{2:X2}" -f [int][Math]::Min(255,[Math]::Max(0,$r)),[int][Math]::Min(255,[Math]::Max(0,$g)),[int][Math]::Min(255,[Math]::Max(0,$b))}
function Opq($a,$x,$y){$x -ge 0 -and $x -lt 16 -and $y -ge 0 -and $y -lt 16 -and $a[$y*16+$x] -ne "00000000"}
function Finish($a){$b=$a.Clone()
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if(-not (Opq $a $x $y)){continue};$c=$a[$y*16+$x]
  if(-not (Opq $a $x ($y-1))){$b[$y*16+$x]=Hx $c 0.16}elseif(-not (Opq $a $x ($y+1))){$b[$y*16+$x]=Hx $c -0.2}}}
 $o=$b.Clone()
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if(Opq $b $x $y){continue};if((Opq $b ($x-1) $y) -or (Opq $b ($x+1) $y) -or (Opq $b $x ($y-1)) -or (Opq $b $x ($y+1))){$o[$y*16+$x]="1C1A24"}}}
 ,$o}
$E=[ordered]@{}
$E["Talp"]={$a=Tp;Ell $a 8 9 5.2 4.2 "7A5230";Ell $a 8 7 3.6 2.4 "8A6038";Rc $a 5 3 6 2 "5A3A20";P $a 7 3 "7A5230";P $a 9 4 "7A5230";Disc $a 8 10.5 1.4 "F2A6A6";P $a 6 8 "14100C";P $a 10 8 "14100C";P $a 6 7 "FFFFFF";Rc $a 2 11 3 2 "E8C8A0";Rc $a 11 11 3 2 "E8C8A0";P $a 2 13 "FFFFFF";P $a 4 13 "FFFFFF";P $a 11 13 "FFFFFF";P $a 13 13 "FFFFFF";Rc $a 5 13 6 1 "5A3A20";,$a}
$E["Salamandra"]={$a=Tp;Ell $a 8 9 5.5 3 "E8641E";Ell $a 8 10 4.5 1.6 "F2A04A";Ell $a 12 8 2.4 2.2 "E8641E";Ln $a 3 9 0 12 "E8641E";Ln $a 3 10 1 13 "C84A14";foreach($x in 5,8,10){Rc $a $x 4 1 2 "FFC04A";P $a $x 3 "FFE890"};P $a 13 7 "FFE060";P $a 12 7 "1C1A24";Rc $a 4 12 2 2 "C84A14";Rc $a 10 12 2 2 "C84A14";Ln $a 14 9 15 10 "D83A3A";,$a}
$E["Llimac_acid"]={$a=Tp;Ell $a 7 10 6 3.4 "7AC820";Ell $a 6 9 4 1.6 "A4E840";Ell $a 12 7 2.2 2.6 "7AC820";Ln $a 11 5 10 3 "7AC820";Ln $a 13 5 14 3 "7AC820";Disc $a 10 2.5 1.2 "F2F8A0";Disc $a 14 2.5 1.2 "F2F8A0";P $a 10 3 "1C1A24";P $a 14 3 "1C1A24";P $a 12 7 "1C1A24";P $a 13 7 "FFFFFF";foreach($p in @(@(4,9),@(7,11),@(9,9))){P $a $p[0] $p[1] "4A9A10"};Rc $a 1 13 12 1 "A4E840";P $a 3 14 "A4E840";P $a 9 14 "A4E840";P $a 5 6 "A4E840";P $a 3 7 "A4E840";,$a}
$E["Escorpi"]={$a=Tp;Ell $a 8 10 4 3 "D8A860";Ell $a 8 9 2.6 1.6 "E8C078";Ell $a 3 7 2 1.6 "D8A860";Ell $a 13 7 2 1.6 "D8A860";Ln $a 4 9 3 8 "B88A40";Ln $a 12 9 13 8 "B88A40";Rc $a 2 6 1 2 "1C1A24";Rc $a 13 6 1 2 "1C1A24";foreach($x in 4,6,10,12){Ln $a $x 12 ($x+($x-8)/2) 14 "B88A40"};Ln $a 8 13 12 13 "D8A860";Ln $a 12 13 13 9 "D8A860";Ln $a 13 9 11 5 "D8A860";Ln $a 11 5 9 4 "D8A860";P $a 8 4 "D83A3A";P $a 8 5 "A81E2A";P $a 7 8 "D83A3A";P $a 9 8 "D83A3A";,$a}
$E["Mimic"]={$a=Tp;Rc $a 2 4 12 4 "B8844C";Rc $a 2 4 12 1 "D8A468";Rc $a 2 11 12 3 "A8723C";Rc $a 2 8 12 3 "2A0A0E";Rc $a 7 3 2 4 "F2C84B";P $a 7 3 "FFE890";foreach($x in 3,5,7,9,11){P $a $x 8 "FFFFFF";P $a $x 9 "E8ECF0"};foreach($x in 4,6,8,10,12){P $a $x 10 "FFFFFF";P $a $x 9 "E8ECF0"};P $a 5 9 "D83A3A";P $a 10 9 "D83A3A";P $a 5 10 "D83A3A";P $a 10 10 "D83A3A";Rc $a 6 10 4 1 "E85A6A";Rc $a 2 11 12 1 "8A5A30";P $a 7 12 "F2C84B";P $a 8 12 "F2C84B";P $a 7 13 "F2C84B";P $a 8 13 "F2C84B";Rc $a 3 14 2 1 "5E3C20";Rc $a 11 14 2 1 "5E3C20";,$a}
$E["Escut_guerrer"]={$a=Tp;Disc $a 9 4 2.8 "9AA2AC";Rc $a 8 4 4 1 "2A2E36";P $a 9 4 "D83A3A";P $a 11 4 "D83A3A";Rc $a 8 2 2 1 "D83A3A";Rc $a 7 7 6 5 "6A727C";Rc $a 8 12 2 2 "4A505A";Rc $a 11 12 2 2 "4A505A";Rc $a 1 5 7 8 "4A78C8";Rc $a 1 5 7 1 "F2C84B";Rc $a 1 12 7 1 "F2C84B";Rc $a 1 5 1 8 "F2C84B";Rc $a 7 5 1 8 "F2C84B";Rc $a 4 6 1 6 "F4F4F0";Rc $a 2 8 5 1 "F4F4F0";Ln $a 13 4 13 11 "C8D0D8";P $a 13 3 "FFFFFF";Rc $a 12 11 3 1 "8A5A30";,$a}
$E["Banshee"]={$a=Tp;Ell $a 8 6 3.8 4 "E8F0F4";Rc $a 3 5 2 8 "5A7A98";Rc $a 11 5 2 8 "5A7A98";Rc $a 4 3 8 2 "5A7A98";Ell $a 8 11 4.2 3 "D0DCE8";for($x=4;$x -lt 13;$x++){P $a $x (13+($x%2)) "D0DCE8"};Disc $a 8 8 1.4 "14181E";P $a 6 6 "14181E";P $a 10 6 "14181E";P $a 6 7 "14181E";P $a 10 7 "14181E";Ln $a 3 9 1 6 "E8F0F4";Ln $a 13 9 15 6 "E8F0F4";P $a 7 5 "3AE08A";P $a 9 5 "3AE08A";,$a}
$E["Llop_neu"]={$a=Tp;Ell $a 8 7 4.6 4 "D8E0EC";foreach($p in @(@(4,2),@(4,3),@(5,3),@(4,4),@(5,4),@(6,4),@(11,2),@(11,3),@(10,3),@(11,4),@(10,4),@(9,4))){P $a $p[0] $p[1] "B8C4D4"};P $a 5 4 "F4C8D0";P $a 10 4 "F4C8D0";Ell $a 8 9.5 2.8 2.2 "F4F8FC";Rc $a 7 9 2 1 "1C1A24";P $a 8 11 "1C1A24";P $a 7 11 "D83A3A";P $a 5 6 "F2C020";P $a 10 6 "F2C020";P $a 5 7 "1C1A24";P $a 10 7 "1C1A24";Rc $a 4 12 8 2 "C0CCDC";Rc $a 4 13 2 1 "F4F8FC";Rc $a 10 13 2 1 "F4F8FC";P $a 7 5 "B8C4D4";P $a 9 5 "B8C4D4";,$a}
$E["Bombardier"]={$a=Tp;Disc $a 6 6 3.4 "6AA848";Ln $a 3 4 1 2 "6AA848";Ln $a 9 4 10 2 "6AA848";P $a 5 6 "D83A3A";P $a 8 6 "D83A3A";P $a 5 5 "1C1A24";P $a 8 5 "1C1A24";Rc $a 5 8 3 1 "1C1A24";Rc $a 3 10 7 4 "8A5A30";Rc $a 3 10 7 1 "A8723C";Rc $a 4 13 2 1 "3A2412";Rc $a 8 13 2 1 "3A2412";Ln $a 10 10 12 7 "6AA848";Disc $a 12 5 2.2 "2A2E36";P $a 11 4 "6A727C";Rc $a 12 2 1 2 "8A5A30";P $a 13 1 "FFC04A";P $a 12 1 "FFE890";,$a}
$E["Franctirador_cristall"]={$a=Tp;for($y=2;$y -le 13;$y++){$w=[Math]::Min($y-2,13-$y)*0.6+1.2;Rc $a ([int][Math]::Round(7-$w)) $y ([int][Math]::Round($w*2)) 1 "6AD8F0"};Ln $a 7 2 7 13 "BCF4FF";Rc $a 5 7 5 2 "2A8AB0";Disc $a 7 7.5 1.3 "E8FCFF";P $a 7 7 "D83A3A";Ln $a 1 13 14 6 "2A2E36";Ln $a 1 12 14 5 "4A505A";Rc $a 13 5 2 2 "BCF4FF";P $a 14 5 "FFFFFF";P $a 5 4 "FFFFFF";,$a}
$E["Sanador"]={$a=Tp;Disc $a 7 5 3.5 "F0F4E8";Disc $a 7 6 2.2 "2A3A30";P $a 6 6 "3AE08A";P $a 8 6 "3AE08A";for($y=8;$y -le 14;$y++){$w=2.5+($y-8)*0.55;Rc $a ([int][Math]::Round(7-$w)) $y ([int][Math]::Round($w*2)) 1 "E0ECD8"};Rc $a 6 9 2 4 "2AA860";Rc $a 5 10 4 2 "2AA860";Rc $a 4 14 8 1 "B8D0B0";Ln $a 13 4 13 14 "8A5A30";Disc $a 13 3 1.6 "3AE08A";P $a 12 2 "BCFFD4";,$a}
$E["Torreta_antiga"]={$a=Tp;Rc $a 2 10 12 4 "5A606C";Rc $a 2 10 12 1 "7A828E";foreach($x in 3,7,11){P $a $x 12 "F2C020";P $a ($x+1) 12 "F2C020"};Disc $a 8 8 4.5 "7A828E";Disc $a 8 8 3.4 "5A606C";Rc $a 7 1 2 4 "3A3F48";Rc $a 7 1 2 1 "8A929E";Rc $a 1 7 4 2 "3A3F48";Rc $a 11 7 4 2 "3A3F48";Disc $a 8 8 1.5 "D83A3A";P $a 7 7 "FF8A8A";P $a 5 5 "F2C020";P $a 11 5 "F2C020";,$a}
New-Item -ItemType Directory -Force $Out | Out-Null
if(Test-Path "$Out\Sprites"){Get-ChildItem "$Out\Sprites" -File | Remove-Item -Force};New-Item -ItemType Directory -Force "$Out\Sprites" | Out-Null
$ref='C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace\Assets\Sprites\Pixel\Enemics'
$sc=8;$cols=6;$cw=16*$sc+16;$rows=[Math]::Ceiling(($E.Count+6)/$cols)
$sheet=New-Object System.Drawing.Bitmap ($cols*$cw+16),($rows*($sc*16+36)+16);$g=[System.Drawing.Graphics]::FromImage($sheet);$g.Clear([System.Drawing.Color]::FromArgb(40,44,52));$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half'
$font=New-Object System.Drawing.Font "Consolas",9;$br=[System.Drawing.Brushes]::White;$gr=New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(150,160,175))
$i=0
foreach($n in 'Cranc','Gargola','FantasmaGel','Escarabat','Esquelet','Llim'){$im=[System.Drawing.Image]::FromFile("$ref\$n.png");$cx=16+($i%$cols)*$cw;$cy=16+[Math]::Floor($i/$cols)*($sc*16+36);$g.DrawImage($im,$cx,$cy,16*$sc,16*$sc);$g.DrawString("(ja existeix) $n",$font,$gr,$cx,$cy+16*$sc+4);$i++}
foreach($n in $E.Keys){$px=Finish (& $E[$n]);$bmp=New-Object System.Drawing.Bitmap 16,16
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$h=$px[$y*16+$x];$al=if($h.Length -ge 8){[Convert]::ToInt32($h.Substring(6,2),16)}else{255};$bmp.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($al,[Convert]::ToInt32($h.Substring(0,2),16),[Convert]::ToInt32($h.Substring(2,2),16),[Convert]::ToInt32($h.Substring(4,2),16)))}}
 $bmp.Save("$Out\Sprites\$n.png")
 $cx=16+($i%$cols)*$cw;$cy=16+[Math]::Floor($i/$cols)*($sc*16+36);$g.DrawImage($bmp,$cx,$cy,16*$sc,16*$sc);$g.DrawString($n,$font,$br,$cx,$cy+16*$sc+4);$bmp.Dispose();$i++}
$g.Dispose();$sheet.Save("$Out\_Enemics_nous.png");"$($E.Count) sprites"


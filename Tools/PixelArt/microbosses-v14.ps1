param([string]$Out="C:\Users\Usuario\Canica Alpha\Builds\Art per editar\Microbosses v1.4")
Add-Type -AssemblyName System.Drawing
$SZ=24
function Mk($c){$a=New-Object string[] ($SZ*$SZ);for($i=0;$i -lt $SZ*$SZ;$i++){$a[$i]=$c};,$a}
function Tp(){Mk "00000000"}
function P($a,$x,$y,$c){$x=[int][Math]::Round($x);$y=[int][Math]::Round($y);if($x -ge 0 -and $x -lt $SZ -and $y -ge 0 -and $y -lt $SZ){$a[$y*$SZ+$x]=$c}}
function Rc($a,$x,$y,$w,$h,$c){for($j=$y;$j -lt $y+$h;$j++){for($i=$x;$i -lt $x+$w;$i++){P $a $i $j $c}}}
function Disc($a,$cx,$cy,$r,$c){for($y=0;$y -lt $SZ;$y++){for($x=0;$x -lt $SZ;$x++){if((($x-$cx)*($x-$cx)+($y-$cy)*($y-$cy)) -le $r*$r){P $a $x $y $c}}}}
function Ell($a,$cx,$cy,$rx,$ry,$c){for($y=0;$y -lt $SZ;$y++){for($x=0;$x -lt $SZ;$x++){$u=($x-$cx)/$rx;$v=($y-$cy)/$ry;if(($u*$u+$v*$v) -le 1){P $a $x $y $c}}}}
function Ln($a,$x0,$y0,$x1,$y1,$c){$n=[Math]::Max([Math]::Abs($x1-$x0),[Math]::Abs($y1-$y0));if($n -eq 0){P $a $x0 $y0 $c;return};for($i=0;$i -le $n;$i++){P $a ($x0+($x1-$x0)*$i/$n) ($y0+($y1-$y0)*$i/$n) $c}}
function Thick($a,$x0,$y0,$x1,$y1,$w,$c){for($k=0;$k -lt $w;$k++){Ln $a ($x0+$k-[int]($w/2)) $y0 ($x1+$k-[int]($w/2)) $y1 $c}}
function Tri($a,$x1,$y1,$x2,$y2,$x3,$y3,$c){$minx=[int][Math]::Floor([Math]::Min($x1,[Math]::Min($x2,$x3)));$maxx=[int][Math]::Ceiling([Math]::Max($x1,[Math]::Max($x2,$x3)));$miny=[int][Math]::Floor([Math]::Min($y1,[Math]::Min($y2,$y3)));$maxy=[int][Math]::Ceiling([Math]::Max($y1,[Math]::Max($y2,$y3)))
 for($y=$miny;$y -le $maxy;$y++){for($x=$minx;$x -le $maxx;$x++){$d1=($x-$x2)*($y1-$y2)-($x1-$x2)*($y-$y2);$d2=($x-$x3)*($y2-$y3)-($x2-$x3)*($y-$y3);$d3=($x-$x1)*($y3-$y1)-($x3-$x1)*($y-$y1);$neg=($d1 -lt 0) -or ($d2 -lt 0) -or ($d3 -lt 0);$pos=($d1 -gt 0) -or ($d2 -gt 0) -or ($d3 -gt 0);if(-not ($neg -and $pos)){P $a $x $y $c}}}}
function MirrorX($a){for($y=0;$y -lt $SZ;$y++){for($x=0;$x -lt $SZ/2;$x++){$c=$a[$y*$SZ+$x];if($c -ne "00000000"){$a[$y*$SZ+($SZ-1-$x)]=$c}}}}
function Hx($h,[double]$f){$r=[Convert]::ToInt32($h.Substring(0,2),16);$g=[Convert]::ToInt32($h.Substring(2,2),16);$b=[Convert]::ToInt32($h.Substring(4,2),16)
 if($f -ge 0){$r=$r+(255-$r)*$f;$g=$g+(255-$g)*$f;$b=$b+(255-$b)*$f}else{$r=$r*(1+$f);$g=$g*(1+$f);$b=$b*(1+$f)}
 "{0:X2}{1:X2}{2:X2}" -f [int][Math]::Min(255,[Math]::Max(0,$r)),[int][Math]::Min(255,[Math]::Max(0,$g)),[int][Math]::Min(255,[Math]::Max(0,$b))}
function Opq($a,$x,$y){$x -ge 0 -and $x -lt $SZ -and $y -ge 0 -and $y -lt $SZ -and $a[$y*$SZ+$x] -ne "00000000"}
function Finish($a){$b=$a.Clone()
 for($y=0;$y -lt $SZ;$y++){for($x=0;$x -lt $SZ;$x++){if(-not (Opq $a $x $y)){continue};$c=$a[$y*$SZ+$x];if(-not (Opq $a $x ($y-1))){$b[$y*$SZ+$x]=Hx $c 0.14}elseif(-not (Opq $a $x ($y+1))){$b[$y*$SZ+$x]=Hx $c -0.18}}}
 $o=$b.Clone()
 for($y=0;$y -lt $SZ;$y++){for($x=0;$x -lt $SZ;$x++){if(Opq $b $x $y){continue};if((Opq $b ($x-1) $y) -or (Opq $b ($x+1) $y) -or (Opq $b $x ($y-1)) -or (Opq $b $x ($y+1))){$o[$y*$SZ+$x]="1C1A24"}}}
 ,$o}
$B=[ordered]@{}
$B["EscarabatDaurat"]={$a=Tp
 foreach($k in 0..2){$y=9+$k*4;Ln $a 5 $y 1 ($y+2) "8A6A10";Ln $a 18 $y 22 ($y+2) "8A6A10"}
 Ell $a 12 14 8 7 "D8A82A";Ell $a 12 14 6.4 5.6 "E8C050";Ln $a 12 8 12 21 "8A6A10";Ln $a 7 11 6 17 "B88A18";Ln $a 17 11 18 17 "B88A18"
 Ell $a 12 6 3.6 3 "C89A20";Rc $a 8 2 2 3 "8A6A10";Rc $a 14 2 2 3 "8A6A10";P $a 8 1 "B88A18";P $a 15 1 "B88A18"
 P $a 10 6 "FF3A3A";P $a 14 6 "FF3A3A";Rc $a 11 13 2 2 "3AA8F0";P $a 11 13 "BCE8FF";P $a 9 10 "FFF0A0";P $a 15 10 "FFF0A0";,$a}
$B["UllFlotant"]={$a=Tp
 Tri $a 6 3 8 0 9 5 "5A3A7A";Tri $a 18 3 16 0 15 5 "5A3A7A"
 foreach($x in 7,12,17){for($y=18;$y -lt 23;$y++){P $a ($x+[Math]::Sin($y*1.2+$x)) $y "7A4A9A"}};P $a 7 22 "FFC0E0";P $a 12 22 "FFC0E0";P $a 17 22 "FFC0E0"
 Disc $a 12 11 9 "E8E4D8";Ln $a 5 8 8 10 "D83A3A";Ln $a 19 8 16 10 "D83A3A";Ln $a 6 15 9 13 "D83A3A";Ln $a 18 15 15 13 "D83A3A";Ln $a 12 3 12 6 "D83A3A"
 Disc $a 12 11 5.2 "8A5AC8";Disc $a 12 11 4 "B070E8";Rc $a 11 7 2 8 "14081C";P $a 10 8 "FFFFFF";P $a 9 9 "FFFFFF";,$a}
$B["GranotaGegant"]={$a=Tp
 Ell $a 12 15 9 7 "4A9A3A";Ell $a 12 17 6 4.5 "C8E8A0";Disc $a 6 6 3.8 "6AC85A";Disc $a 18 6 3.8 "6AC85A";Disc $a 6 6 2.4 "F2C020";Disc $a 18 6 2.4 "F2C020";Rc $a 5 5 2 3 "1C1A24";Rc $a 17 5 2 3 "1C1A24";P $a 5 5 "FFFFFF";P $a 17 5 "FFFFFF"
 Ell $a 12 11 8 4 "4A9A3A";Rc $a 5 12 14 1 "1C1A24";Rc $a 11 13 2 6 "E86A8A";Disc $a 12 20 1.6 "FF8AAA";P $a 12 19 "FFC0D0"
 Rc $a 2 17 4 5 "3A8A2A";Rc $a 18 17 4 5 "3A8A2A";Rc $a 1 21 5 2 "3A8A2A";Rc $a 18 21 5 2 "3A8A2A";foreach($p in @(@(8,10),@(15,9),@(10,15),@(15,16),@(6,14))){P $a $p[0] $p[1] "2E7A22"};P $a 9 9 "F2C020";P $a 16 11 "F2C020";,$a}
$B["MagCendra"]={$a=Tp
 for($y=10;$y -le 22;$y++){$w=3+($y-10)*0.45;Rc $a ([int][Math]::Round(12-$w)) $y ([int][Math]::Round($w*2)) 1 "4A4650"};Rc $a 8 22 8 1 "2E2A34";Rc $a 11 11 2 9 "3A3640"
 Disc $a 12 6 5 "3A3640";Disc $a 12 7 3.2 "14101A";P $a 10 7 "FF8A2A";P $a 14 7 "FF8A2A";P $a 10 6 "FFD060";P $a 14 6 "FFD060";Tri $a 8 3 12 0 16 3 "3A3640"
 Rc $a 5 12 3 5 "4A4650";Disc $a 4.5 16 2.4 "FFD060";Disc $a 4.5 16 1.4 "FF8A2A";Ln $a 20 7 20 22 "6A4A2A";Disc $a 20 6 1.8 "FF8A2A";P $a 20 5 "FFD060"
 foreach($p in @(@(2,5),@(21,13),@(3,21),@(18,2))){Disc $a $p[0] $p[1] 1.3 "E8641E";P $a $p[0] $p[1] "FFD060"};,$a}
New-Item -ItemType Directory -Force $Out | Out-Null
if(Test-Path "$Out\Sprites"){Get-ChildItem "$Out\Sprites" -File | Remove-Item -Force};New-Item -ItemType Directory -Force "$Out\Sprites" | Out-Null
$sc=8;$cw=$SZ*$sc+16;$sheet=New-Object System.Drawing.Bitmap (4*$cw+16),($SZ*$sc+50);$g=[System.Drawing.Graphics]::FromImage($sheet);$g.Clear([System.Drawing.Color]::FromArgb(40,44,52));$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half'
$font=New-Object System.Drawing.Font "Consolas",9;$br=[System.Drawing.Brushes]::White;$i=0
foreach($nm in $B.Keys){$px=Finish (& $B[$nm]);$bmp=New-Object System.Drawing.Bitmap $SZ,$SZ
 for($y=0;$y -lt $SZ;$y++){for($x=0;$x -lt $SZ;$x++){$h=$px[$y*$SZ+$x];$al=if($h.Length -ge 8){[Convert]::ToInt32($h.Substring(6,2),16)}else{255};$bmp.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($al,[Convert]::ToInt32($h.Substring(0,2),16),[Convert]::ToInt32($h.Substring(2,2),16),[Convert]::ToInt32($h.Substring(4,2),16)))}}
 $bmp.Save("$Out\Sprites\$nm.png");$cx=16+$i*$cw;$g.DrawImage($bmp,$cx,10,$SZ*$sc,$SZ*$sc);$g.DrawString($nm,$font,$br,$cx,$SZ*$sc+16);$bmp.Dispose();$i++}
$g.Dispose();$sheet.Save("$Out\_Microbosses.png");"$($B.Count) sprites"

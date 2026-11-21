param([string]$Out="C:\Users\Usuario\Canica Alpha\Builds\Art per editar\Bosses nous v1.4")
Add-Type -AssemblyName System.Drawing
$SZ=32
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
$B["CavallerCorrupte"]={$a=Tp
 Rc $a 9 13 14 16 "5A1A28";Rc $a 9 27 3 2 "5A1A28";Rc $a 15 28 3 2 "5A1A28";Rc $a 21 27 2 2 "5A1A28"
 Rc $a 11 23 4 6 "3A3548";Rc $a 17 23 4 6 "3A3548";Rc $a 10 28 6 2 "2A2634";Rc $a 16 28 6 2 "2A2634"
 Rc $a 10 12 12 12 "4A4560";Rc $a 11 13 10 2 "6A6488";Rc $a 10 22 12 2 "2A2634";Rc $a 15 22 2 2 "C8A02A";Rc $a 15 15 2 6 "3A3548"
 Disc $a 9 13 3.6 "5A5578";Disc $a 23 13 3.6 "5A5578";P $a 8 12 "8A84A8";P $a 22 12 "8A84A8"
 Rc $a 5 15 3 8 "4A4560";Rc $a 24 15 3 8 "4A4560";Rc $a 24 22 3 2 "C8A070"
 Ell $a 16 7 5.5 6 "4A4560";Rc $a 11 7 10 3 "1C1A24";Rc $a 12 8 2 1 "FF3A3A";Rc $a 18 8 2 1 "FF3A3A";Rc $a 15 0 2 4 "C83A3A";Rc $a 13 1 6 1 "C83A3A";Rc $a 12 4 1 3 "6A6488"
 Rc $a 1 12 7 12 "2A2634";Rc $a 1 12 7 1 "8A5AC8";Rc $a 1 23 7 1 "8A5AC8";Rc $a 1 12 1 12 "8A5AC8";Rc $a 7 12 1 12 "8A5AC8";Ln $a 3 13 5 17 "A8A0C0";Ln $a 5 17 3 21 "A8A0C0";P $a 4 17 "FFFFFF"
 Rc $a 27 1 2 18 "C8D0E0";Rc $a 26 1 1 18 "B070E8";Rc $a 29 1 1 18 "8A92A8";Rc $a 25 19 6 2 "C8A02A";Rc $a 27 21 2 4 "5E3C20";P $a 27 0 "E8D0FF";P $a 28 0 "E8D0FF";,$a}
$B["CucSorra"]={$a=Tp
 Ell $a 16 28 15 3.4 "E0C98C";Ell $a 16 29 12 2 "C8AE6C"
 Rc $a 8 14 16 14 "D8A860";for($i=0;$i -lt 5;$i++){Rc $a 8 (17+$i*3) 16 1 "B88A40"};Rc $a 8 14 2 14 "E8C078";Rc $a 22 14 2 14 "B88A40"
 Disc $a 16 12 10 "D8A860";Disc $a 16 12 7.6 "7A2A32"
 for($i=0;$i -lt 16;$i++){$an=$i*0.3927;$x=16+6.6*[Math]::Cos($an);$y=12+6.6*[Math]::Sin($an);Rc $a ([int]$x-0) ([int]$y-0) 2 2 "F4F0E0"}
 Disc $a 16 12 4.6 "3A1018";for($i=0;$i -lt 8;$i++){$an=$i*0.7854+0.39;P $a (16+3.6*[Math]::Cos($an)) (12+3.6*[Math]::Sin($an)) "E8E0D0"};Disc $a 16 12 2 "14080C"
 Ln $a 8 16 24 16 "E8C078";foreach($p in @(@(4,24),@(27,22),@(2,27),@(29,26),@(12,30),@(20,30))){P $a $p[0] $p[1] "F4E0A8"};,$a}
$B["ReiClavegueres"]={$a=Tp
 Ell $a 16 21 10 8 "7A6A60";Ell $a 16 23 6 5 "B8A89C";Ell $a 16 11 8.6 7 "8A786C"
 Disc $a 8 5 4.4 "8A786C";Disc $a 8 5 2.6 "F2A6B4";Disc $a 24 5 4.4 "8A786C";Disc $a 24 5 2.6 "F2A6B4"
 Rc $a 11 10 3 3 "FFFFFF";Rc $a 18 10 3 3 "FFFFFF";Rc $a 12 11 2 2 "D83A3A";Rc $a 19 11 2 2 "D83A3A";P $a 12 11 "1C1A24";P $a 19 11 "1C1A24"
 Disc $a 16 14.5 1.6 "F2A6B4";Rc $a 14 17 4 4 "F4F0E0";Rc $a 16 17 1 4 "C8C0B0"
 Ln $a 12 14 2 11 "D0C8C0";Ln $a 12 15 2 16 "D0C8C0";Ln $a 20 14 30 11 "D0C8C0";Ln $a 20 15 30 16 "D0C8C0"
 Rc $a 11 1 10 3 "C8A02A";foreach($x in 11,14,17,20){Rc $a $x 0 2 2 "C8A02A"};foreach($x in 13,16,19){P $a $x 2 "3AE08A"}
 Rc $a 7 27 5 3 "E8A0A8";Rc $a 20 27 5 3 "E8A0A8";Ln $a 24 25 30 27 "E8A0A8";Ln $a 30 27 29 30 "E8A0A8";Ln $a 24 26 30 28 "C8808A"
 foreach($p in @(@(6,20),@(6,23),@(26,19),@(26,23),@(3,15),@(29,13))){Rc $a $p[0] $p[1] 1 2 "7AC820";P $a $p[0] ($p[1]+2) "A4E840"};,$a}
$B["Fenix"]={$a=Tp
 Tri $a 14 13 0 5 3 25 "D83A2A";Tri $a 14 14 3 9 7 24 "F2803A";Tri $a 14 15 7 12 10 23 "FFC04A"
 foreach($p in @(@(1,7),@(3,14),@(5,21))){Ln $a 14 15 $p[0] $p[1] "A8281E"}
 Ell $a 16 18 4.6 7 "F2603A";Ell $a 16 19 2.8 4.8 "FFC04A"
 Tri $a 12 24 20 24 16 31 "D83A2A";Tri $a 14 24 18 24 16 29 "FFC04A"
 Disc $a 16 9 4.2 "F2603A";Tri $a 14 10 18 10 16 15 "F2C020";Tri $a 14 10 18 10 16 12 "FFE060"
 Tri $a 13 6 14 0 16 5 "FFC04A";Tri $a 16 5 16 -1 18 5 "FFE890";Tri $a 17 6 18 1 19 5 "FFC04A"
 P $a 14 8 "FFFFFF";P $a 18 8 "FFFFFF";P $a 14 9 "1C1A24";P $a 18 9 "1C1A24"
 Rc $a 13 28 2 3 "C8981A";Rc $a 17 28 2 3 "C8981A"
 foreach($i in 0..2){$a2=$a};MirrorX $a;P $a 16 20 "FFE890";,$a}
$B["CorCristall"]={$a=Tp
 Disc $a 12 12 6 "D83AA8";Disc $a 20 12 6 "D83AA8";Tri $a 6 14 26 14 16 27 "D83AA8";Tri $a 16 14 26 14 16 27 "A82A88"
 Tri $a 9 9 13 7 12 12 "F27AD0";Tri $a 19 8 23 9 21 13 "F27AD0";Ln $a 8 12 14 22 "FFB8E8";Ln $a 11 8 15 8 "FFD0F0"
 Tri $a 16 14 20 14 16 24 "C0309A";Disc $a 16 14 3 "FFE8F8";Disc $a 16 14 1.4 "FFFFFF"
 for($i=0;$i -lt 6;$i++){$an=-1.5708+$i*1.0472;$cx=16+13.5*[Math]::Cos($an);$cy=15+13*[Math]::Sin($an);Ln $a $cx ($cy-3) ($cx+2) $cy "6AD8F0";Ln $a ($cx+2) $cy $cx ($cy+3) "2A8AB0";Ln $a $cx ($cy+3) ($cx-2) $cy "2A8AB0";Ln $a ($cx-2) $cy $cx ($cy-3) "BCF4FF";Rc $a ([int]$cx-1) ([int]$cy-1) 2 2 "E8FCFF"}
 ,$a}
$B["HidraLlac"]={$a=Tp
 Ell $a 16 28 15 3.4 "3A7AD8";Ln $a 3 28 8 28 "8AB8F0";Ln $a 22 29 28 29 "8AB8F0";Ln $a 10 30 16 30 "2A5AA8"
 Ell $a 16 25 8 4 "2A7A3A";Ell $a 16 24 6 2 "4AAA58"
 Thick $a 13 24 7 15 3 "2E8A3E";Thick $a 7 15 6 10 3 "2E8A3E";Thick $a 16 23 16 10 3 "2E8A3E";Thick $a 19 24 25 15 3 "2E8A3E";Thick $a 25 15 26 10 3 "2E8A3E"
 foreach($h in @(@(6,8),@(16,4),@(26,8))){$x=$h[0];$y=$h[1];Ell $a $x $y 4 3.2 "3A9A48";Rc $a ($x-3) ($y+1) 7 2 "2A0A0E";P $a ($x-2) ($y+1) "FFFFFF";P $a ($x+2) ($y+1) "FFFFFF";P $a ($x-2) ($y-1) "FFE060";P $a ($x+2) ($y-1) "FFE060";P $a ($x-2) $y "D83A3A";P $a ($x+2) $y "D83A3A";P $a $x ($y+3) "D83A3A"}
 foreach($p in @(@(10,25),@(14,26),@(18,25),@(22,26))){P $a $p[0] $p[1] "6ACC74"};,$a}
$B["CuinerBoig"]={$a=Tp
 Ell $a 16 4 5.4 3.6 "F4F4F0";Rc $a 11 5 10 4 "F4F4F0";Rc $a 11 8 10 1 "C8C8C4";Rc $a 13 2 1 4 "DCDCD8";Rc $a 18 2 1 4 "DCDCD8"
 Ell $a 16 14 5.6 5.2 "E8B890";P $a 11 14 "E8806A";P $a 21 14 "E8806A";Rc $a 12 11 3 3 "FFFFFF";Rc $a 17 11 3 3 "FFFFFF";P $a 13 12 "D83A3A";P $a 18 13 "D83A3A";Ln $a 11 9 14 11 "3A2412";Ln $a 21 9 18 11 "3A2412"
 Rc $a 11 15 10 2 "3A2412";P $a 10 16 "3A2412";P $a 21 16 "3A2412";P $a 9 15 "3A2412";P $a 22 15 "3A2412";Rc $a 14 17 4 2 "7A1E1E";P $a 14 17 "FFFFFF";P $a 17 17 "FFFFFF"
 Ell $a 16 24 9 7 "F4F4F0";Rc $a 12 20 8 10 "E8E8E0";P $a 14 22 "D83A3A";P $a 17 24 "D83A3A";P $a 15 26 "8A5A30";P $a 16 21 "6A727C";P $a 16 23 "6A727C"
 Rc $a 4 17 4 9 "F4F4F0";Rc $a 24 17 4 8 "F4F4F0";Disc $a 5.5 27 2 "E8B890";Disc $a 26 26 2 "E8B890"
 Ln $a 27 25 30 15 "5A606C";Disc $a 29 11 4 "3A3F48";Disc $a 29 11 2.8 "5A606C";Disc $a 29 11 1.6 "F2C020";P $a 28 10 "F4F4F0";Ln $a 27 13 28 14 "3A3F48"
 Rc $a 1 9 5 6 "C8D0D8";Rc $a 1 9 5 1 "E8F0F8";Rc $a 2 15 2 6 "8A5A30";Rc $a 1 14 5 1 "8A929E"
 Rc $a 11 29 4 2 "3A3F48";Rc $a 17 29 4 2 "3A3F48";,$a}
$B["ProfessorInfernal"]={$a=Tp
 Ell $a 16 9 5 6 "E0987C";Tri $a 11 6 10 0 14 4 "8A1E2A";Tri $a 21 6 22 0 18 4 "8A1E2A";Rc $a 10 8 2 4 "B8B8C0";Rc $a 20 8 2 4 "B8B8C0"
 Rc $a 11 9 4 3 "2A2E36";Rc $a 17 9 4 3 "2A2E36";Rc $a 12 10 2 1 "FF3A3A";Rc $a 18 10 2 1 "FF3A3A";P $a 15 9 "2A2E36";Ln $a 11 8 14 7 "6A6A72";Ln $a 21 8 18 7 "6A6A72"
 P $a 16 12 "C07860";Rc $a 14 14 4 1 "6A1E1E";P $a 14 15 "6A1E1E";P $a 17 15 "6A1E1E"
 Rc $a 9 16 14 12 "5A3A2A";Rc $a 14 16 4 9 "F4F4F0";Rc $a 15 17 2 8 "C83A3A";Rc $a 15 16 2 1 "8A1E2A";Rc $a 9 16 2 12 "6A4A3A"
 Rc $a 5 17 4 10 "5A3A2A";Rc $a 23 17 4 10 "5A3A2A";Disc $a 6.5 28 2 "E0987C";Disc $a 24.5 28 2 "E0987C"
 Ln $a 27 27 31 3 "C8A070";P $a 31 3 "D83A3A";P $a 30 4 "D83A3A"
 Rc $a 1 17 6 9 "F4F4F0";Rc $a 1 17 6 1 "D8D8D0";Ln $a 1 20 5 20 "9AA0A8";Ln $a 1 22 4 22 "9AA0A8";Ln $a 2 24 5 19 "D83A3A";Ln $a 5 24 2 19 "D83A3A"
 Rc $a 11 28 4 3 "3A2A22";Rc $a 17 28 4 3 "3A2A22";,$a}
New-Item -ItemType Directory -Force $Out | Out-Null
if(Test-Path "$Out\Sprites"){Get-ChildItem "$Out\Sprites" -File | Remove-Item -Force};New-Item -ItemType Directory -Force "$Out\Sprites" | Out-Null
$ref='C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace\Assets\Sprites\Pixel\Enemics'
$sc=5;$cols=4;$cw=$SZ*$sc+16;$rh=$SZ*$sc+34;$rows=[Math]::Ceiling(($B.Count+3)/$cols)
$sheet=New-Object System.Drawing.Bitmap ($cols*$cw+16),($rows*$rh+16);$g=[System.Drawing.Graphics]::FromImage($sheet);$g.Clear([System.Drawing.Color]::FromArgb(40,44,52));$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half'
$font=New-Object System.Drawing.Font "Consolas",9;$br=[System.Drawing.Brushes]::White;$gr=New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(150,160,175))
$i=0
foreach($n in 'ReinaAranya','ReiGel','GolemGegant'){$im=[System.Drawing.Image]::FromFile("$ref\$n.png");$cx=16+($i%$cols)*$cw;$cy=16+[Math]::Floor($i/$cols)*$rh;$g.DrawImage($im,$cx,$cy,$SZ*$sc,$SZ*$sc);$g.DrawString("(ja existeix) $n",$font,$gr,$cx,$cy+$SZ*$sc+4);$i++}
$i=4
foreach($n in $B.Keys){$px=Finish (& $B[$n]);$bmp=New-Object System.Drawing.Bitmap $SZ,$SZ
 for($y=0;$y -lt $SZ;$y++){for($x=0;$x -lt $SZ;$x++){$h=$px[$y*$SZ+$x];$al=if($h.Length -ge 8){[Convert]::ToInt32($h.Substring(6,2),16)}else{255};$bmp.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($al,[Convert]::ToInt32($h.Substring(0,2),16),[Convert]::ToInt32($h.Substring(2,2),16),[Convert]::ToInt32($h.Substring(4,2),16)))}}
 $bmp.Save("$Out\Sprites\$n.png")
 $cx=16+($i%$cols)*$cw;$cy=16+[Math]::Floor($i/$cols)*$rh;$g.DrawImage($bmp,$cx,$cy,$SZ*$sc,$SZ*$sc);$g.DrawString($n,$font,$br,$cx,$cy+$SZ*$sc+4);$bmp.Dispose();$i++}
$g.Dispose();$sheet.Save("$Out\_Bosses_nous.png");"$($B.Count) sprites"




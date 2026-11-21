param([string]$Out="C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace\ArtNou_v1.4\Mercader")
Add-Type -AssemblyName System.Drawing
$SZ=16
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
function SmallMerchant($talk){$a=Tp
 Rc $a 2 8 3 6 "A8723C";Rc $a 11 8 3 6 "A8723C";Rc $a 2 7 2 2 "C8905A";Rc $a 12 7 2 2 "C8905A";P $a 2 7 "E8D0A0";P $a 13 7 "E8D0A0"
 Rc $a 4 8 8 6 "3A7A8A";Rc $a 4 7 8 2 "F2C020";Rc $a 7 9 2 5 "2A5A68";Rc $a 4 12 8 1 "5E3C20";P $a 8 12 "F2C020"
 Rc $a 5 4 6 4 "E8B890";Rc $a 3 3 10 2 "8A5A30";Rc $a 5 1 6 3 "8A5A30";Rc $a 5 3 6 1 "F2C020";P $a 6 5 "1C1A24";P $a 9 5 "1C1A24";Rc $a 6 6 4 1 "3A2412"
 if($talk){Rc $a 7 7 2 1 "7A1E1E";Rc $a 11 10 2 2 "E8B890";Disc $a 13 8 1.3 "F2C020";P $a 14 6 "FFFFFF";P $a 12 6 "FFF0A0"}else{Rc $a 3 10 2 2 "E8B890";Rc $a 11 10 2 2 "E8B890"}
 Rc $a 5 14 2 2 "5E3C20";Rc $a 9 14 2 2 "5E3C20";$a}
$SZ=16
$out="C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace\ArtNou_v1.4\Mercader"
New-Item -ItemType Directory -Force $out | Out-Null;Get-ChildItem $out -File | Remove-Item -Force
function SaveSprite($px,$size,$path){$bmp=New-Object System.Drawing.Bitmap $size,$size
 for($y=0;$y -lt $size;$y++){for($x=0;$x -lt $size;$x++){$h=$px[$y*$size+$x];$al=if($h.Length -ge 8){[Convert]::ToInt32($h.Substring(6,2),16)}else{255};$bmp.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($al,[Convert]::ToInt32($h.Substring(0,2),16),[Convert]::ToInt32($h.Substring(2,2),16),[Convert]::ToInt32($h.Substring(4,2),16)))}};$bmp.Save($path);$bmp}
$m1=SaveSprite (Finish (SmallMerchant $false)) 16 "$out\Mercader_quiet.png"
$m2=SaveSprite (Finish (SmallMerchant $true)) 16 "$out\Mercader_parla.png"
$SZ=32
$a=Tp
Rc $a 1 12 6 18 "A8723C";Rc $a 25 12 6 18 "A8723C";Rc $a 2 10 4 4 "C8905A";Rc $a 26 10 4 4 "C8905A";Rc $a 3 16 2 5 "E8D0A0";Rc $a 27 16 2 5 "E8D0A0";Rc $a 2 22 4 1 "5E3C20";Rc $a 26 22 4 1 "5E3C20"
Rc $a 27 5 2 6 "3AD86A";Rc $a 27 4 2 2 "E8D0A0";P $a 27 7 "A8F8C0"
Rc $a 6 24 20 8 "3A7A8A";Rc $a 15 25 2 7 "2A5A68";Rc $a 6 28 20 1 "5E3C20";Rc $a 15 28 2 1 "F2C020";Rc $a 8 29 3 3 "5E3C20";Rc $a 21 29 3 3 "5E3C20";P $a 9 30 "F2C020"
Ell $a 16 23 8 3 "F2C020";Ell $a 16 22 7 1.6 "FFE070"
Ell $a 16 15 7 6.4 "E8B890";P $a 10 17 "E8806A";P $a 22 17 "E8806A"
Rc $a 12 13 3 3 "FFFFFF";Rc $a 17 13 3 3 "FFFFFF";Rc $a 13 14 2 2 "1C1A24";Rc $a 18 14 2 2 "1C1A24";Ln $a 11 11 14 12 "3A2412";Ln $a 21 11 18 12 "3A2412"
Ell $a 16 19 6 2 "3A2412";P $a 9 19 "3A2412";P $a 8 18 "3A2412";P $a 23 19 "3A2412";P $a 24 18 "3A2412";Rc $a 13 21 6 2 "7A1E1E";P $a 15 21 "F2C020";P $a 16 21 "F2C020";Rc $a 13 21 6 1 "F4F0E0"
Ell $a 16 9 13 3.6 "8A5A30";Rc $a 9 1 14 8 "8A5A30";Rc $a 9 7 14 2 "F2C020";Rc $a 10 1 12 1 "A8723C";Ln $a 20 6 26 1 "D83A3A";Ln $a 21 6 27 2 "FF6A5A";Ln $a 22 5 27 3 "D83A3A"
$p=SaveSprite (Finish $a) 32 "$out\Mercader_retrat.png"
$sc=8;$sheet=New-Object System.Drawing.Bitmap 760,330;$g=[System.Drawing.Graphics]::FromImage($sheet);$g.Clear([System.Drawing.Color]::FromArgb(40,44,52));$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half'
$font=New-Object System.Drawing.Font "Consolas",9;$br=[System.Drawing.Brushes]::White
$g.DrawImage($m1,16,16,128,128);$g.DrawString("Mercader_quiet",$font,$br,16,150);$g.DrawImage($m2,170,16,128,128);$g.DrawString("Mercader_parla",$font,$br,170,150);$g.DrawImage($p,340,16,256,256);$g.DrawString("Mercader_retrat (32x32)",$font,$br,340,280)
$sheet.Save("$out\_Mercader.png");"ok"

param([string]$Out="C:\Users\Usuario\Canica Alpha\Builds\Art per editar\Portes v1.4")
Add-Type -AssemblyName System.Drawing
function Tp(){$a=New-Object string[] 256;for($i=0;$i -lt 256;$i++){$a[$i]="00000000"};,$a}
function P($a,$x,$y,$c){$x=[int][Math]::Round($x);$y=[int][Math]::Round($y);if($x -ge 0 -and $x -lt 16 -and $y -ge 0 -and $y -lt 16){$a[$y*16+$x]=$c}}
function Rc($a,$x,$y,$w,$h,$c){for($j=$y;$j -lt $y+$h;$j++){for($i=$x;$i -lt $x+$w;$i++){P $a $i $j $c}}}
function Hx($h,[double]$f){$r=[Convert]::ToInt32($h.Substring(0,2),16);$g=[Convert]::ToInt32($h.Substring(2,2),16);$b=[Convert]::ToInt32($h.Substring(4,2),16)
 if($f -ge 0){$r=$r+(255-$r)*$f;$g=$g+(255-$g)*$f;$b=$b+(255-$b)*$f}else{$r=$r*(1+$f);$g=$g*(1+$f);$b=$b*(1+$f)}
 "{0:X2}{1:X2}{2:X2}" -f [int][Math]::Min(255,[Math]::Max(0,$r)),[int][Math]::Min(255,[Math]::Max(0,$g)),[int][Math]::Min(255,[Math]::Max(0,$b))}
function MirX($a){$b=Tp;for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$b[$y*16+(15-$x)]=$a[$y*16+$x]}};,$b}
function Leaf($a,$hx,$hy,$len,$th,$deg,$c){$rad=$deg*[Math]::PI/180;$co=[Math]::Cos($rad);$si=[Math]::Sin($rad);$hi=Hx $c 0.25;$dk=Hx $c -0.3
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$dx=$x+0.5-$hx;$dy=$y+0.5-$hy;$u=$dx*$co+$dy*$si;$v=-$dx*$si+$dy*$co;if($u -ge 0 -and $u -le $len -and [Math]::Abs($v) -le $th/2){$col=$c;if($v -lt -$th/4){$col=$hi}elseif($v -gt $th/4){$col=$dk};P $a $x $y $col}}}}
function Pillars($a,$c){Rc $a 0 4 2 8 $c;Rc $a 14 4 2 8 $c;Rc $a 0 4 2 1 (Hx $c 0.3);Rc $a 14 4 2 1 (Hx $c 0.3);Rc $a 0 11 2 1 (Hx $c -0.35);Rc $a 14 11 2 1 (Hx $c -0.35)}
function Track($a,$c){Rc $a 2 5 12 1 $c;Rc $a 2 10 12 1 $c}
function Lock($a){Rc $a 6 6 4 4 "F2C020";Rc $a 6 6 4 1 "FFE470";Rc $a 7 4 2 3 "B8A890";P $a 7 4 "00000000";P $a 8 4 "00000000";P $a 7 5 "8A8070";P $a 8 5 "8A8070";P $a 7 8 "8A5A00";P $a 8 8 "8A5A00";P $a 8 9 "FF3A3A"}
function Shadow($a,$y,$x0,$x1){for($x=$x0;$x -le $x1;$x++){if($a[$y*16+$x] -eq "00000000"){P $a $x $y "00000040"}}}
# --- tipus de porta; $st = 0 tancada, 1 mitja, 2 oberta, 3 bloquejada
function WoodDoor($st){$a=Tp;Pillars $a "7A7E86";$ang=@(0,-45,-88,0)[$st];Leaf $a 2.2 8 11.4 4 $ang "A8723C";P $a 2 8 "3A3F48";P $a 3 8 "3A3F48"
 if($st -ne 2){for($u=3;$u -lt 13;$u+=3){P $a ($u+[int](2*[Math]::Cos($ang*[Math]::PI/180))) 7 "8A5A30"}};if($st -eq 3){Lock $a};$a}
function SaloonDoor($st){$a=Tp;Pillars $a "7A5A38";$ang=@(0,-40,-80,0)[$st];$l=Tp;Leaf $l 2.2 8 6.2 3 $ang "C8905A";for($i=0;$i -lt 256;$i++){if($l[$i] -ne "00000000"){$a[$i]=$l[$i]}};$r=MirX $l;for($i=0;$i -lt 256;$i++){if($r[$i] -ne "00000000"){$a[$i]=$r[$i]}};if($st -eq 0){P $a 8 7 "6A4020";P $a 8 8 "6A4020"};$a}
function SlideDoor($st,$col,$strip,$hazard){$a=Tp;Pillars $a "5A626E";Track $a "2A2E36";$sh=@(0,3,5,0)[$st];$lp=Tp;for($x=0;$x -lt 16;$x++){for($y=6;$y -le 9;$y++){$px=$x;if($px -ge 2 -and $px -le 7){$src=$px+$sh;if($src -le 7){}};}}
 for($side=0;$side -lt 2;$side++){for($x=2;$x -le 7;$x++){$xx=$x-$sh;if($xx -lt 2){continue};for($y=6;$y -le 9;$y++){$c=$col;if($y -eq 6){$c=Hx $col 0.25}elseif($y -eq 9){$c=Hx $col -0.3};if($hazard){if((($x+$y)%4) -lt 2){$c=if($y -eq 9){"8A6A00"}else{"F2C020"}}else{$c="2A2E36"}}elseif($y -eq 7 -or $y -eq 8){if($x -eq 7 -or $x -eq 6){$c=$strip}};if($side -eq 0){P $a $xx $y $c}else{P $a (15-$xx) $y $c}}}}
 if($st -eq 3){Lock $a};$a}
function Portcullis($st){$a=Tp;Pillars $a "4A4E58";Rc $a 2 4 12 1 "3A3F48"
 if($st -eq 0 -or $st -eq 3){for($x=2;$x -lt 14;$x+=2){Rc $a $x 5 1 6 "4A505A";P $a $x 11 "8A929E"};Rc $a 2 6 12 1 "6A727C";Rc $a 2 9 12 1 "6A727C"}
 if($st -eq 1){for($x=2;$x -lt 14;$x+=2){Rc $a $x 5 1 3 "4A505A";P $a $x 8 "8A929E"};Rc $a 2 6 12 1 "6A727C"}
 if($st -eq 2){Rc $a 2 5 12 2 "4A505A";Rc $a 2 5 12 1 "8A929E";for($x=2;$x -lt 14;$x+=2){P $a $x 7 "6A727C"}}
 if($st -eq 3){Lock $a};$a}
function PrisonGate($st){$a=Tp;Pillars $a "6A727C";Rc $a 2 5 12 1 "3A3F48";Rc $a 2 10 12 1 "3A3F48"
 $xs=@(3,5,7,8,10,12);foreach($x in $xs){$keep=$true;if($st -eq 1 -and $x -gt 7){$keep=$false};if($st -eq 2 -and $x -gt 3){$keep=$false};if($keep){Rc $a $x 5 1 6 "9AA2AC";P $a $x 5 "D8E0E8"}}
 if($st -eq 1){Rc $a 2 7 6 1 "6A727C"};if($st -eq 0 -or $st -eq 3){Rc $a 2 7 12 1 "6A727C"};if($st -eq 3){Lock $a};$a}
function GlassDoor($st){$a=Tp;Pillars $a "8A9CA8";Track $a "B8C8D4";$sh=@(0,3,5,0)[$st]
 for($side=0;$side -lt 2;$side++){for($x=2;$x -le 7;$x++){$xx=$x-$sh;if($xx -lt 2){continue};for($y=6;$y -le 9;$y++){$c="BFE9F2A0";if($y -eq 6 -or $y -eq 9 -or $x -eq 7){$c="E8F8FFE0"};if($x -eq 3 -and $y -eq 7){$c="FFFFFFD0"};if($side -eq 0){P $a $xx $y $c}else{P $a (15-$xx) $y $c}}}};$a}
function HospDoor($st){$a=Tp;Pillars $a "A8B4BC";$ang=@(0,-38,-82,0)[$st];$l=Tp;Leaf $l 2.2 8 6.2 4 $ang "E8F2F4";for($i=0;$i -lt 256;$i++){if($l[$i] -ne "00000000"){$a[$i]=$l[$i]}}
 if($st -eq 0){Rc $a 4 7 2 2 "78B8D0"};$r=MirX $a;for($i=0;$i -lt 256;$i++){if($r[$i] -ne "00000000" -and $a[$i] -eq "00000000"){$a[$i]=$r[$i]}};if($st -eq 0){P $a 7 7 "8A929E";P $a 8 7 "8A929E"};$a}
function StoneDoor($st){$a=Tp;Pillars $a "6A6C74";Track $a "3A3C44";$sh=@(0,6,11,0)[$st]
 for($x=2;$x -le 13;$x++){$xx=$x+$sh;if($xx -gt 13){continue};for($y=5;$y -le 10;$y++){$c="8A8C94";if($y -eq 5){$c="A8AAB2"}elseif($y -eq 10){$c="5A5C64"}elseif($x%6 -eq 0 -and $y -ge 6 -and $y -le 9){$c="7A7C84"};P $a $xx $y $c}}
 if($st -ne 2){P $a (4+$sh) 7 "5A5C64";P $a (5+$sh) 8 "5A5C64"};if($st -eq 3){Lock $a};$a}
$D=[ordered]@{}
$D["Porta_fusta"]={param($s)WoodDoor $s}
$D["Porta_saloon"]={param($s)SaloonDoor $s}
$D["Porta_lab_metall"]={param($s)SlideDoor $s "8A929E" "3AE08A" $false}
$D["Porta_perill"]={param($s)SlideDoor $s "F2C020" "D83A3A" $true}
$D["Porta_reixa_castell"]={param($s)Portcullis $s}
$D["Porta_preso"]={param($s)PrisonGate $s}
$D["Porta_vidre"]={param($s)GlassDoor $s}
$D["Porta_hospital"]={param($s)HospDoor $s}
$D["Porta_pedra"]={param($s)StoneDoor $s}
$states=@("tancada","mitja","oberta","bloquejada");$lockOk=@("Porta_fusta","Porta_lab_metall","Porta_reixa_castell","Porta_preso","Porta_pedra")
if(Test-Path $Out){Get-ChildItem $Out -Recurse -File | Remove-Item -Force}
New-Item -ItemType Directory -Force "$Out\H","$Out\V" | Out-Null
$sc=6;$cw=16*$sc+12;$ch=16*$sc+12
$sheet=New-Object System.Drawing.Bitmap (4*$cw+130),($D.Count*$ch+10);$g=[System.Drawing.Graphics]::FromImage($sheet);$g.Clear([System.Drawing.Color]::FromArgb(40,44,52));$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half'
$font=New-Object System.Drawing.Font "Consolas",9;$br=[System.Drawing.Brushes]::White;$row=0;$total=0
foreach($name in $D.Keys){$g.DrawString($name.Replace("Porta_",""),$font,$br,6,10+$row*$ch+40)
 for($st=0;$st -lt 4;$st++){if($st -eq 3 -and ($lockOk -notcontains $name)){continue}
  $px=& $D[$name] $st;$bmp=New-Object System.Drawing.Bitmap 16,16
  for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$h=$px[$y*16+$x];$al=if($h.Length -ge 8){[Convert]::ToInt32($h.Substring(6,2),16)}else{255};$bmp.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($al,[Convert]::ToInt32($h.Substring(0,2),16),[Convert]::ToInt32($h.Substring(2,2),16),[Convert]::ToInt32($h.Substring(4,2),16)))}}
  $bmp.Save("$Out\H\$name`_$($states[$st]).png");$v=$bmp.Clone();$v.RotateFlip('Rotate90FlipNone');$v.Save("$Out\V\$name`_$($states[$st]).png");$total++
  $cx=130+$st*$cw;$cy=10+$row*$ch
  for($qy=0;$qy -lt 16;$qy++){for($qx=0;$qx -lt 16;$qx++){$cc=if(($qx+$qy)%2){[System.Drawing.Color]::FromArgb(70,74,84)}else{[System.Drawing.Color]::FromArgb(52,55,64)};$sb=New-Object System.Drawing.SolidBrush $cc;$g.FillRectangle($sb,$cx+$qx*$sc,$cy+$qy*$sc,$sc,$sc);$sb.Dispose()}}
  $g.DrawImage($bmp,$cx,$cy,16*$sc,16*$sc);$bmp.Dispose()}
 $row++}
for($st=0;$st -lt 4;$st++){$g.DrawString($states[$st],$font,$br,130+$st*$cw,0)}
$g.Dispose();$sheet.Save("$Out\_Portes.png");"$total portes"


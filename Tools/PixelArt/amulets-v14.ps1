param([string]$Out="C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace\ArtNou_v1.4\Amulets")
Add-Type -AssemblyName System.Drawing
function Mk($c){$a=New-Object string[] 256;for($i=0;$i -lt 256;$i++){$a[$i]=$c};,$a}
function Tp(){Mk "00000000"}
function P($a,$x,$y,$c){$x=[int][Math]::Round($x);$y=[int][Math]::Round($y);if($x -ge 0 -and $x -lt 16 -and $y -ge 0 -and $y -lt 16){$a[$y*16+$x]=$c}}
function Rc($a,$x,$y,$w,$h,$c){for($j=$y;$j -lt $y+$h;$j++){for($i=$x;$i -lt $x+$w;$i++){P $a $i $j $c}}}
function Disc($a,$cx,$cy,$r,$c){for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if((($x-$cx)*($x-$cx)+($y-$cy)*($y-$cy)) -le $r*$r){P $a $x $y $c}}}}
function Ell($a,$cx,$cy,$rx,$ry,$c){for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$u=($x-$cx)/$rx;$v=($y-$cy)/$ry;if(($u*$u+$v*$v) -le 1){P $a $x $y $c}}}}
function Ln($a,$x0,$y0,$x1,$y1,$c){$n=[Math]::Max([Math]::Abs($x1-$x0),[Math]::Abs($y1-$y0));if($n -eq 0){P $a $x0 $y0 $c;return};for($i=0;$i -le $n;$i++){P $a ($x0+($x1-$x0)*$i/$n) ($y0+($y1-$y0)*$i/$n) $c}}
function Tri($a,$x1,$y1,$x2,$y2,$x3,$y3,$c){for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$d1=($x-$x2)*($y1-$y2)-($x1-$x2)*($y-$y2);$d2=($x-$x3)*($y2-$y3)-($x2-$x3)*($y-$y3);$d3=($x-$x1)*($y3-$y1)-($x3-$x1)*($y-$y1);$neg=($d1 -lt 0) -or ($d2 -lt 0) -or ($d3 -lt 0);$pos=($d1 -gt 0) -or ($d2 -gt 0) -or ($d3 -gt 0);if(-not ($neg -and $pos)){P $a $x $y $c}}}}
function Hx($h,[double]$f){$r=[Convert]::ToInt32($h.Substring(0,2),16);$g=[Convert]::ToInt32($h.Substring(2,2),16);$b=[Convert]::ToInt32($h.Substring(4,2),16)
 if($f -ge 0){$r=$r+(255-$r)*$f;$g=$g+(255-$g)*$f;$b=$b+(255-$b)*$f}else{$r=$r*(1+$f);$g=$g*(1+$f);$b=$b*(1+$f)}
 "{0:X2}{1:X2}{2:X2}" -f [int][Math]::Min(255,[Math]::Max(0,$r)),[int][Math]::Min(255,[Math]::Max(0,$g)),[int][Math]::Min(255,[Math]::Max(0,$b))}
function Opq($a,$x,$y){$x -ge 0 -and $x -lt 16 -and $y -ge 0 -and $y -lt 16 -and $a[$y*16+$x] -ne "00000000"}
function Finish($a){$b=$a.Clone();for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if(-not (Opq $a $x $y)){continue};$c=$a[$y*16+$x];if(-not (Opq $a $x ($y-1))){$b[$y*16+$x]=Hx $c 0.14}elseif(-not (Opq $a $x ($y+1))){$b[$y*16+$x]=Hx $c -0.18}}}
 $o=$b.Clone();for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if(Opq $b $x $y){continue};if((Opq $b ($x-1) $y) -or (Opq $b ($x+1) $y) -or (Opq $b $x ($y-1)) -or (Opq $b $x ($y+1))){$o[$y*16+$x]="1C1A24"}}};,$o}
$A=[ordered]@{}
# nom = @(rareza, dibuix)
$A["Escut_de_vidre"]=@("Comu",{$a=Tp;Rc $a 4 2 8 7 "8AD8F0";Tri $a 4 9 12 9 8 14 "8AD8F0";Rc $a 4 2 8 1 "E8FCFF";Ln $a 8 3 8 12 "E8FCFF";Ln $a 5 6 11 6 "E8FCFF";P $a 5 4 "FFFFFF";$a})
$A["Pedra_afiladora"]=@("Comu",{$a=Tp;Rc $a 3 7 10 5 "8A8E96";Rc $a 3 7 10 1 "B4B8C0";Rc $a 3 11 10 1 "5E626A";foreach($p in @(@(4,4),@(7,3),@(10,4),@(12,6),@(2,6))){P $a $p[0] $p[1] "FFE060"};P $a 7 4 "FFFFFF";$a})
$A["Rellotge_de_sorra"]=@("Comu",{$a=Tp;Rc $a 4 2 8 1 "8A5A30";Rc $a 4 13 8 1 "8A5A30";Tri $a 5 3 11 3 8 8 "BCE4F0";Tri $a 8 8 5 12 11 12 "BCE4F0";Tri $a 6 5 10 5 8 8 "E0C060";Tri $a 8 10 6 12 10 12 "E0C060";Ln $a 8 8 8 11 "E0C060";$a})
$A["Ferradura"]=@("Comu",{$a=Tp;Disc $a 8 9 5.6 "A8B0BA";Disc $a 8 9 3.4 "00000000";Rc $a 4 2 8 6 "00000000";foreach($p in @(@(3,6),@(3,9),@(13,6),@(13,9),@(5,12),@(11,12))){P $a $p[0] $p[1] "5A626E"};Rc $a 3 3 3 3 "A8B0BA";Rc $a 10 3 3 3 "A8B0BA";P $a 4 4 "E8F0F8";$a})
$A["Imant"]=@("Comu",{$a=Tp;Rc $a 3 10 10 3 "8A929E";Rc $a 3 2 3 8 "D83A3A";Rc $a 10 2 3 8 "3A7AD8";Rc $a 3 2 3 2 "E8E8E8";Rc $a 10 2 3 2 "E8E8E8";Rc $a 6 5 4 5 "00000000";Rc $a 3 10 10 1 "B4BCC8";$a})
$A["Cor_de_pedra"]=@("Comu",{$a=Tp;Disc $a 5.5 6 3.2 "8A8E96";Disc $a 10.5 6 3.2 "8A8E96";Tri $a 2 7 14 7 8 14 "8A8E96";Ln $a 8 5 7 9 "4A4E56";Ln $a 7 9 9 11 "4A4E56";P $a 4 4 "B4B8C0";$a})
$A["Ull_de_falco"]=@("Rar",{$a=Tp;Ell $a 8 8 6.5 3.8 "F2C020";Ell $a 8 8 5 2.8 "FFE890";Disc $a 8 8 2.7 "6A3A10";Disc $a 8 8 1.3 "14100C";P $a 7 7 "FFFFFF";$a})
$A["Gota_de_veri"]=@("Rar",{$a=Tp;Tri $a 8 1 4 9 12 9 "3AD86A";Disc $a 8 10 4.2 "3AD86A";Disc $a 7 9 1.2 "A8F8C0";P $a 6 8 "FFFFFF";P $a 9 12 "1E9A48";$a})
$A["Dent_de_vampir"]=@("Rar",{$a=Tp;Tri $a 4 3 12 3 8 14 "F4F0E0";Rc $a 4 3 8 2 "FFFFFF";Tri $a 9 5 12 3 8 14 "D8D0C0";P $a 8 12 "D83A3A";P $a 8 13 "A81E2A";$a})
$A["Moneda_de_la_sort"]=@("Rar",{$a=Tp;Disc $a 8 8 6.2 "D8A010";Disc $a 8 8 5 "F2C020";foreach($p in @(@(7,6),@(9,6),@(7,8),@(9,8))){Disc $a $p[0] $p[1] 1.1 "3A9A48"};Ln $a 8 9 9 12 "2E7A38";P $a 5 5 "FFF0A0";$a})
$A["Espines"]=@("Rar",{$a=Tp;Disc $a 8 8 3.6 "6A727C";foreach($d in @(@(8,1),@(8,15),@(1,8),@(15,8),@(3,3),@(13,3),@(3,13),@(13,13))){Ln $a 8 8 $d[0] $d[1] "C8D0D8"};Disc $a 8 8 2.4 "8A929E";P $a 7 7 "E8F0F8";$a})
$A["Ploma_de_fenix"]=@("Epic",{$a=Tp;Tri $a 3 13 8 2 13 5 "F2603A";Tri $a 3 13 6 8 12 9 "FFC04A";Tri $a 3 13 10 4 12 8 "F29A2E";Ln $a 3 13 12 3 "FFE890";P $a 12 3 "FFFFFF";$a})
$A["Cor_de_drago"]=@("Epic",{$a=Tp;Disc $a 5.5 6 3.2 "D83A2A";Disc $a 10.5 6 3.2 "D83A2A";Tri $a 2 7 14 7 8 14 "D83A2A";foreach($p in @(@(5,6),@(8,8),@(10,6),@(8,11),@(6,9))){P $a $p[0] $p[1] "8A1414"};Tri $a 7 1 9 1 8 4 "FFC04A";P $a 8 2 "FFFFFF";P $a 4 4 "FF8A7A";$a})
$A["Mirall_trencat"]=@("Epic",{$a=Tp;Tri $a 3 3 9 2 6 9 "BCE4F0";Tri $a 9 2 13 6 8 8 "A8D0E4";Tri $a 3 11 8 9 12 13 "D8F0FA";Tri $a 9 9 13 8 12 13 "9AC8DC";Ln $a 6 9 8 9 "FFFFFF";Ln $a 8 8 9 2 "FFFFFF";P $a 5 4 "FFFFFF";$a})
$A["Llibre_prohibit"]=@("Epic",{$a=Tp;Rc $a 3 3 10 10 "6A2A8A";Rc $a 5 3 8 10 "8A4AB0";Rc $a 3 3 2 10 "4A1A68";Ell $a 9 8 3.2 2 "F4F0E0";Disc $a 9 8 1.2 "D83A3A";P $a 9 8 "1C1A24";Rc $a 12 7 2 2 "F2C020";$a})
$A["Estrella_caiguda"]=@("Llegendari",{$a=Tp;Tri $a 8 0 6 6 10 6 "FFF0A0";Tri $a 8 15 6 10 10 10 "FFF0A0";Tri $a 0 8 6 6 6 10 "FFE060";Tri $a 15 8 10 6 10 10 "FFE060";Disc $a 8 8 3.4 "FFF8D0";Disc $a 8 8 1.8 "FFFFFF";P $a 3 3 "FFE890";P $a 13 3 "FFE890";P $a 3 13 "FFE890";P $a 13 13 "FFE890";$a})
if(Test-Path $Out){Get-ChildItem $Out -Recurse -File | Remove-Item -Force};New-Item -ItemType Directory -Force "$Out\Icones" | Out-Null
$frame=@{"Comu"="8A929E";"Rar"="3A7AD8";"Epic"="A84AD8";"Llegendari"="F2C020"}
$sc=8;$cw=16*$sc+34;$ch=16*$sc+54;$cols=6;$rows=[Math]::Ceiling($A.Count/$cols)
$sheet=New-Object System.Drawing.Bitmap ($cols*$cw+16),($rows*$ch+16);$g=[System.Drawing.Graphics]::FromImage($sheet);$g.Clear([System.Drawing.Color]::FromArgb(40,44,52));$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half'
$font=New-Object System.Drawing.Font "Consolas",9;$br=[System.Drawing.Brushes]::White;$i=0
foreach($nm in $A.Keys){$rar=$A[$nm][0];$px=Finish (& $A[$nm][1]);$bmp=New-Object System.Drawing.Bitmap 16,16
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$h=$px[$y*16+$x];$al=if($h.Length -ge 8){[Convert]::ToInt32($h.Substring(6,2),16)}else{255};$bmp.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($al,[Convert]::ToInt32($h.Substring(0,2),16),[Convert]::ToInt32($h.Substring(2,2),16),[Convert]::ToInt32($h.Substring(4,2),16)))}}
 $bmp.Save("$Out\Icones\$nm.png")
 $cx=16+($i%$cols)*$cw;$cy=16+[Math]::Floor($i/$cols)*$ch
 $fc=[System.Drawing.ColorTranslator]::FromHtml("#"+$frame[$rar]);$pen=New-Object System.Drawing.Pen $fc,4
 $g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(60,64,74))),$cx-6,$cy-6,16*$sc+12,16*$sc+12);$g.DrawRectangle($pen,$cx-6,$cy-6,16*$sc+12,16*$sc+12)
 $g.DrawImage($bmp,$cx,$cy,16*$sc,16*$sc);$g.DrawString($nm,$font,$br,$cx-6,$cy+16*$sc+12);$g.DrawString($rar,$font,(New-Object System.Drawing.SolidBrush $fc),$cx-6,$cy+16*$sc+26);$bmp.Dispose();$i++}
$g.Dispose();$sheet.Save("$Out\_Amulets.png");"$($A.Count) icones"


param([string]$Out="C:\Users\Usuario\Canica Alpha\Builds\Art per editar\Blocs prova v1.4")
Add-Type -AssemblyName System.Drawing
function Mk($c){$a=New-Object string[] 256;for($i=0;$i -lt 256;$i++){$a[$i]=$c};,$a}
function P($a,$x,$y,$c){if($x -ge 0 -and $x -lt 16 -and $y -ge 0 -and $y -lt 16){$a[$y*16+$x]=$c}}
function Rc($a,$x,$y,$w,$h,$c){for($j=$y;$j -lt $y+$h;$j++){for($i=$x;$i -lt $x+$w;$i++){P $a $i $j $c}}}
function Speck($a,$rng,$n,$cols){for($i=0;$i -lt $n;$i++){P $a $rng.Next(16) $rng.Next(16) $cols[$rng.Next($cols.Count)]}}
function Tr($a){$b=Mk "000000";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$b[$x*16+$y]=$a[$y*16+$x]}};,$b}
function Planks2($rng,$tones,$grain,$dk,$lt,$knot){$a=Mk $tones[0]
 for($r=0;$r -lt 4;$r++){$y=$r*4;$ja=$rng.Next(16);$jb=($ja+$rng.Next(5,12))%16
  $ia=$rng.Next(3);$ib=($ia+1+$rng.Next(2))%3;$tA=$tones[$ia];$tB=$tones[$ib]
  for($x=0;$x -lt 16;$x++){$inA=if($ja -lt $jb){$x -ge $ja -and $x -lt $jb}else{$x -ge $ja -or $x -lt $jb};$c=if($inA){$tA}else{$tB}
   Rc $a $x $y 1 3 $c;P $a $x ($y+3) $dk;if(($x+$r)%3 -ne 0){P $a $x $y $lt}}
  foreach($j in @($ja,$jb)){Rc $a $j $y 1 3 $grain;P $a (($j+1)%16) ($y+1) $lt}
  for($k=0;$k -lt 3;$k++){$gx=$rng.Next(16);$gl=$rng.Next(2,5);$gy=$y+1+$rng.Next(2);for($i=0;$i -lt $gl;$i++){P $a (($gx+$i)%16) $gy $grain}}}
 if($knot){$kr=$rng.Next(4)*4;$kx=$rng.Next(3,12);P $a $kx ($kr+1) $dk;P $a ($kx+1) ($kr+1) $dk;P $a ($kx-1) ($kr+1) $grain;P $a ($kx+2) ($kr+1) $grain;P $a $kx ($kr+2) $grain}
 ,$a}
function TileGrid($rng,$base,$grout,$lt,$spk){$a=Mk $base
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($x%8 -eq 7 -or $y%8 -eq 7){P $a $x $y $grout}elseif($x%8 -eq 0 -or $y%8 -eq 0){P $a $x $y $lt}}}
 Speck $a $rng 10 $spk;,$a}
function Bricks($rng,$base,$mortar,$hi,$sh){$a=Mk $mortar
 for($row=0;$row -lt 4;$row++){$y=$row*4;$off=if($row%2){4}else{0}
  for($x=0;$x -lt 16;$x++){$lx=($x+$off)%8;if($lx -eq 7){continue};for($j=0;$j -lt 3;$j++){P $a $x ($y+$j) $base};P $a $x $y $hi;P $a $x ($y+2) $sh}}
 Speck $a $rng 8 @($hi,$sh);,$a}
function Vor($rng,$n,$tones,$edge,$hl,$extra,$ecount){
 $sx=@();$sy=@();$st=@();for($i=0;$i -lt $n;$i++){$sx+=$rng.Next(16);$sy+=$rng.Next(16);$st+=$tones[$rng.Next($tones.Count)]}
 $a=Mk $tones[0]
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$d1=99;$d2=99;$bi=0
  for($i=0;$i -lt $n;$i++){$dx=[Math]::Abs($x-$sx[$i]);if($dx -gt 8){$dx=16-$dx};$dy=[Math]::Abs($y-$sy[$i]);if($dy -gt 8){$dy=16-$dy};$d=[Math]::Sqrt($dx*$dx+$dy*$dy)
   if($d -lt $d1){$d2=$d1;$d1=$d;$bi=$i}elseif($d -lt $d2){$d2=$d}}
  if(($d2-$d1) -lt 1.0){P $a $x $y $edge}else{P $a $x $y $st[$bi]}}}
 for($y=0;$y -lt 15;$y++){for($x=0;$x -lt 15;$x++){if($a[$y*16+$x] -eq $edge -and $a[$y*16+$x+1] -ne $edge -and $a[($y+1)*16+$x] -ne $edge -and $rng.Next(3) -eq 0){}}}
 if($extra){for($i=0;$i -lt $ecount;$i++){P $a $rng.Next(16) $rng.Next(16) $extra[$rng.Next($extra.Count)]}}
 ,$a}
function Diamond($rng,$base,$noise,$hi,$mid,$sh){$a=Mk $base;Speck $a $rng 24 $noise
 for($cy=0;$cy -lt 4;$cy++){for($cx=0;$cx -lt 4;$cx++){$o=($cx+$cy)%2;$x0=$cx*4;$y0=$cy*4
  for($t=0;$t -lt 3;$t++){$px=$x0+$t;$py=$(if($o -eq 0){$y0+$t}else{$y0+2-$t})
   P $a ($px+1) ($py+1) $sh;P $a $px $py $(if($t -eq 1){$hi}else{$mid})}}}
 ,$a}
function Bars($rng,$base,$noise,$hi,$mid,$sh){$a=Mk $base;Speck $a $rng 16 $noise
 for($cy=0;$cy -lt 2;$cy++){for($cx=0;$cx -lt 2;$cx++){$o=($cx+$cy)%2;$x0=$cx*8;$y0=$cy*8
  for($k=0;$k -lt 3;$k++){for($i=1;$i -le 6;$i++){$e=($i -eq 1 -or $i -eq 6)
   $u=$x0+$i;$v=$y0+1+$k*2;$u2=$x0+$i;$v2=$y0+2+$k*2
   if($o -eq 0){P $a $u $v $(if($e){$mid}else{$hi});P $a $u2 $v2 $sh}else{P $a $v $u $(if($e){$mid}else{$hi});P $a $v2 $u2 $sh}}}}}
 ,$a}
function GrassBase($rng0,$b,$d,$l,$extra){$rng=New-Object System.Random 777;$a=Mk $b
 for($i=0;$i -lt 14;$i++){$x=$rng.Next(16);$y=$rng.Next(16);$w=$rng.Next(2,4);$h=$rng.Next(1,3);$c=if($rng.Next(2)){$d}else{$l};for($j=0;$j -lt $h;$j++){for($k=0;$k -lt $w;$k++){P $a (($x+$k)%16) (($y+$j)%16) $c}}}
 ,$a}
function Hx($h,[double]$f){$r=[Convert]::ToInt32($h.Substring(0,2),16);$g=[Convert]::ToInt32($h.Substring(2,2),16);$b=[Convert]::ToInt32($h.Substring(4,2),16)
 if($f -ge 0){$r=$r+(255-$r)*$f;$g=$g+(255-$g)*$f;$b=$b+(255-$b)*$f}else{$r=$r*(1+$f);$g=$g*(1+$f);$b=$b*(1+$f)}
 "{0:X2}{1:X2}{2:X2}" -f [int][Math]::Min(255,[Math]::Max(0,$r)),[int][Math]::Min(255,[Math]::Max(0,$g)),[int][Math]::Min(255,[Math]::Max(0,$b))}
function PW($a,$x,$y,$c){$a[((($y%16)+16)%16)*16+((($x%16)+16)%16)]=$c}
function Rock($rng,$tones,$edge,$grid){
 $n=$grid*$grid;$cs=16.0/$grid;$sx=@();$sy=@();$st=@()
 for($gy=0;$gy -lt $grid;$gy++){for($gx=0;$gx -lt $grid;$gx++){$sx+=($gx+0.5)*$cs+($rng.NextDouble()-0.5)*$cs*0.8;$sy+=($gy+0.5)*$cs+($rng.NextDouble()-0.5)*$cs*0.8;$st+=$tones[$rng.Next($tones.Count)]}}
 $cell=New-Object 'int[]' 256;$ed=New-Object 'bool[]' 256
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$d1=99;$d2=99;$bi=0
  for($i=0;$i -lt $n;$i++){$dx=[Math]::Abs($x+0.5-$sx[$i]);if($dx -gt 8){$dx=16-$dx};$dy=[Math]::Abs($y+0.5-$sy[$i]);if($dy -gt 8){$dy=16-$dy};$d=[Math]::Sqrt($dx*$dx+$dy*$dy)
   if($d -lt $d1){$d2=$d1;$d1=$d;$bi=$i}elseif($d -lt $d2){$d2=$d}}
  $cell[$y*16+$x]=$bi;$ed[$y*16+$x]=(($d2-$d1) -lt 1.0)}}
 $a=Mk $tones[0]
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$k=$y*16+$x
  if($ed[$k]){$a[$k]=$edge}else{$c=$st[$cell[$k]]
   $up=$ed[((($y-1+16)%16)*16+$x)];$lf=$ed[($y*16+(($x-1+16)%16))];$dn=$ed[((($y+1)%16)*16+$x)];$rt=$ed[($y*16+(($x+1)%16))]
   if($up -or $lf){$c=Hx $c 0.22}elseif($dn -or $rt){$c=Hx $c -0.22}
   $a[$k]=$c}}}
 ,$a}
function Ore($a,$cx,$cy,$c1,$c2,$c3,$sz){
 if($sz -eq 1){foreach($p in @(@(0,0,$c1),@(1,0,$c2),@(0,1,$c2),@(1,1,$c3))){PW $a ($cx+$p[0]) ($cy+$p[1]) $p[2]};return}
 $blob=@(@(1,0,$c1),@(2,0,$c2),@(0,1,$c1),@(1,1,$c1),@(2,1,$c2),@(3,1,$c3),@(0,2,$c2),@(1,2,$c2),@(2,2,$c3),@(3,2,$c3),@(1,3,$c3),@(2,3,$c3))
 foreach($p in $blob){PW $a ($cx+$p[0]) ($cy+$p[1]) $p[2]}
 PW $a ($cx+1) ($cy+1) "FFFFFF"}
function Mineral($rng,$rock,$edge,$c1,$c2,$c3,$count){$a=Rock $rng $rock $edge 3
 $ox=@(1,9,4,12);$oy=@(1,3,9,11)
 for($i=0;$i -lt 3;$i++){Ore $a ($ox[$i]+$rng.Next(-1,2)) ($oy[$i]+$rng.Next(-1,2)) $c1 $c2 $c3 2}
 Ore $a ($ox[3]) ($oy[3]) $c1 $c2 $c3 1;Ore $a 14 6 $c1 $c2 $c3 1
 ,$a}function Flowers($rng,$base,$items){$a=GrassBase $rng "56A54F" "4C9745" "63B359" $null
 foreach($it in $items){$x=$rng.Next(1,15);$y=$rng.Next(1,15);$pc=$it[0];$cc=$it[1];$kind=$it[2]
  if($kind -eq "plus"){P $a $x $y $cc;P $a ($x-1) $y $pc;P $a ($x+1) $y $pc;P $a $x ($y-1) $pc;P $a $x ($y+1) $pc}
  elseif($kind -eq "dot"){P $a $x $y $pc;P $a $x ($y+1) "2F6A2C"}
  elseif($kind -eq "two"){P $a $x $y $pc;P $a ($x+1) $y $cc;P $a $x ($y+1) "2F6A2C"}
  elseif($kind -eq "stalk"){for($k=0;$k -lt 4;$k++){P $a $x ($y+$k) $(if($k -lt 3){$pc}else{"2F6A2C"})};P $a ($x+1) ($y+1) $pc}
  elseif($kind -eq "clover"){P $a $x $y "2E7A38";P $a ($x+1) $y "3E9A48";P $a $x ($y+1) "3E9A48";P $a ($x+1) ($y+1) "2E7A38";P $a ($x-1) ($y+1) "2E7A38"}}
 ,$a}
function PathTile($rng,$arms){$a=GrassBase $rng "56A54F" "4C9745" "63B359" $null
 $m=New-Object 'bool[]' 256
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$in=($x -ge 5 -and $x -le 10 -and $y -ge 5 -and $y -le 10)
  if($arms.Contains("L") -and $x -le 10 -and $y -ge 5 -and $y -le 10){$in=$true};if($arms.Contains("R") -and $x -ge 5 -and $y -ge 5 -and $y -le 10){$in=$true}
  if($arms.Contains("U") -and $y -le 10 -and $x -ge 5 -and $x -le 10){$in=$true};if($arms.Contains("D") -and $y -ge 5 -and $x -ge 5 -and $x -le 10){$in=$true}
  $m[$y*16+$x]=$in}}
 # arrodonir cantons exteriors de la zona central
 foreach($c in @(@(5,5,"L","U"),@(10,5,"R","U"),@(5,10,"L","D"),@(10,10,"R","D"))){if(-not $arms.Contains($c[2]) -and -not $arms.Contains($c[3])){$m[$c[1]*16+$c[0]]=$false}}
 # vora irregular i sorollosa
 $r2=New-Object System.Random 4242
 $mm=$m.Clone();for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if(-not $m[$y*16+$x]){continue};$edge=$false
  foreach($d in @(@(1,0),@(-1,0),@(0,1),@(0,-1))){$nx=$x+$d[0];$ny=$y+$d[1];if($nx -ge 0 -and $nx -lt 16 -and $ny -ge 0 -and $ny -lt 16 -and -not $m[$ny*16+$nx]){$edge=$true}}
  if($edge -and $r2.Next(3) -eq 0){$mm[$y*16+$x]=$false}}}
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($mm[$y*16+$x]){$P=@("8C6A3E","8C6A3E","9A7848","7A5832")[$rng.Next(4)];P $a $x $y $P}}}
 Speck $a $rng 0 @();for($i=0;$i -lt 5;$i++){$x=$rng.Next(16);$y=$rng.Next(16);if($mm[$y*16+$x]){P $a $x $y "B8A078"}};
 ,$a}
function Disc($a,$cx,$cy,$r,$c){for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if((($x-$cx)*($x-$cx)+($y-$cy)*($y-$cy)) -le $r*$r){P $a $x $y $c}}}}
function Ell($a,$cx,$cy,$rx,$ry,$c){for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$u=($x-$cx)/$rx;$v=($y-$cy)/$ry;if(($u*$u+$v*$v) -le 1){P $a $x $y $c}}}}
function Ln($a,$x0,$y0,$x1,$y1,$c){$n=[Math]::Max([Math]::Abs($x1-$x0),[Math]::Abs($y1-$y0));if($n -eq 0){P $a $x0 $y0 $c;return};for($i=0;$i -le $n;$i++){P $a ([int][Math]::Round($x0+($x1-$x0)*$i/$n)) ([int][Math]::Round($y0+($y1-$y0)*$i/$n)) $c}}
function Tp(){Mk "00000000"}
function Mask($x0,$y0,$x1,$y1,$cut){$m=New-Object 'bool[]' 256;for($y=$y0;$y -le $y1;$y++){for($x=$x0;$x -le $x1;$x++){$dx=[Math]::Min($x-$x0,$x1-$x);$dy=[Math]::Min($y-$y0,$y1-$y);if(($dx+$dy) -ge $cut){$m[$y*16+$x]=$true}}};,$m}
function In($m,$x,$y){($x -ge 0) -and ($x -lt 16) -and ($y -ge 0) -and ($y -lt 16) -and $m[$y*16+$x]}
function Shade($a,$m,$base,$spk,$rng,$density){
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if(-not $m[$y*16+$x]){continue}
  $out=(-not (In $m ($x-1) $y)) -or (-not (In $m ($x+1) $y)) -or (-not (In $m $x ($y-1))) -or (-not (In $m $x ($y+1)))
  if($out){P $a $x $y (Hx $base -0.55);continue}
  $c=$base
  $uo=(-not (In $m $x ($y-2)));$lo=(-not (In $m ($x-2) $y));$do=(-not (In $m $x ($y+2)));$ro=(-not (In $m ($x+2) $y))
  if($uo -or $lo){$c=Hx $base 0.2}elseif($do -or $ro){$c=Hx $base -0.22}
  if($spk.Count -gt 0 -and $rng.Next(100) -lt $density){$c=$spk[$rng.Next($spk.Count)]}
  P $a $x $y $c}}}
function Shd($a,$x0,$x1,$y){Rc $a $x0 $y ($x1-$x0+1) 1 "00000050"}
function Bush($rng,$base,$spk,$deco,$n,$y0){$a=Tp;$m=Mask 1 $y0 14 14 3;Shade $a $m $base $spk $rng 45;Shd $a 2 13 15
 for($i=0;$i -lt $n;$i++){$x=$rng.Next(3,13);$y=$rng.Next($y0+2,13);if($m[$y*16+$x]){$c=$deco[$rng.Next($deco.Count)];P $a $x $y $c;if($deco.Count -gt 0 -and $n -gt 0){P $a $x ($y-1) (Hx $c 0.4)}}};,$a}
function Stem($a,$x,$y0){Rc $a $x $y0 1 (15-$y0) "3A8A30";P $a ($x-1) 12 "58B048";P $a ($x-2) 11 "58B048";P $a ($x+1) 13 "58B048";P $a ($x+2) 12 "58B048";Shd $a 6 10 15}
function Crack($a,$rng,$x,$y,$dx,$dy,$len,$c,$sh){for($i=0;$i -lt $len;$i++){if($sh){P $a ($x+1) ($y+1) $sh};P $a $x $y $c;if($rng.Next(4) -eq 0){if($dx -ne 0){$y+=$rng.Next(-1,2)}else{$x+=$rng.Next(-1,2)}};$x+=$dx;$y+=$dy}}
function GelBase($b,$lt,$dk){$a=Mk $b;for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$s=($x+$y)%16;if($s -ge 3 -and $s -le 5){P $a $x $y $lt}elseif($s -ge 10 -and $s -le 11){P $a $x $y $dk}}};,$a}
function Shelf($rng,$cols){$a=Tp;Rc $a 1 0 14 15 "5A606C";Rc $a 2 1 12 13 "2A2E36"
 foreach($by in 4,8,12){$x=2;while($x -lt 14){$w=$rng.Next(2,4);if($x+$w -gt 14){$w=14-$x};if($rng.Next(7) -eq 0){$x+=1;continue};$c=$cols[$rng.Next($cols.Count)];Rc $a $x ($by-3) $w 3 $c;Rc $a $x ($by-3) $w 1 (Hx $c 0.35);P $a ($x+$w-1) ($by-1) (Hx $c -0.3);$x+=$w}
  Rc $a 2 $by 12 1 "A0A8B4"}
 Shd $a 1 14 15;,$a}
function Bed($col,$part){$a=Tp;$w="7A5230";$wl="9A6C40";$wd="4E3220";$bl=Hx $col 0.25;$bd=Hx $col -0.3
 if($part -eq "cap"){Rc $a 0 3 1 13 $w;Rc $a 15 3 1 13 $w;Rc $a 0 0 16 3 $w;Rc $a 0 0 16 1 $wl;Rc $a 0 2 16 1 $wd;Rc $a 1 3 14 13 "E8ECF4"
  foreach($px in 2,9){Rc $a $px 4 5 4 "F8F8FC";Rc $a $px 7 5 1 "D0D6E4";P $a $px 4 "FFFFFF"}
  Rc $a 1 9 14 7 $col;Rc $a 1 9 14 1 $bl;Rc $a 1 10 14 1 $col;Rc $a 1 15 14 1 $col;Rc $a 1 12 14 1 $bd}
 else{Rc $a 0 0 1 13 $w;Rc $a 15 0 1 13 $w;Rc $a 1 0 14 12 $col;Rc $a 1 0 14 1 $col;Rc $a 1 3 14 1 $bd;Rc $a 1 7 14 1 $bd;Rc $a 1 4 14 1 $bl;Rc $a 1 8 14 1 $bl
  Rc $a 0 12 16 3 $w;Rc $a 0 12 16 1 $wl;Rc $a 0 14 16 1 $wd;Shd $a 1 14 15}
 ,$a}
function Sofa($col,$side){$a=Tp;$dk=Hx $col -0.3;$lt=Hx $col 0.25
 Rc $a 0 1 16 6 $dk;Rc $a 0 1 16 1 $col;Rc $a 0 7 16 6 $col;Rc $a 0 7 16 1 $lt;Rc $a 0 12 16 1 $dk
 if($side -eq "esq"){Rc $a 0 3 3 10 $dk;Rc $a 0 3 3 1 $col;Rc $a 15 1 1 12 $dk;Rc $a 0 1 3 2 $dk;Rc $a 1 3 1 9 $col}else{Rc $a 13 3 3 10 $dk;Rc $a 13 3 3 1 $col;Rc $a 0 1 1 12 $dk;Rc $a 13 1 3 2 $dk;Rc $a 14 3 1 9 $col}
 Rc $a 3 13 2 1 "2A2E36";Rc $a 11 13 2 1 "2A2E36";Shd $a 1 14 15;,$a}
function Mush($a,$cx,$cy,$r,$cap,$dot,$stem){Rc $a ($cx-1) ($cy+1) 2 ([int][Math]::Max(2,$r)) $stem;Rc $a ($cx-1) ($cy+1) 1 ([int][Math]::Max(2,$r)) (Hx $stem 0.25);Ell $a $cx $cy $r ($r*0.65) $cap;Ell $a ($cx-1) ($cy-1) ($r*0.5) ($r*0.3) (Hx $cap 0.3);if($r -ge 3){P $a ($cx-1) ($cy) $dot;P $a ($cx+2) ($cy-1) $dot;P $a ($cx+1) ($cy+1) $dot}}
function Shift($a,$dx,$dy){$b=Tp;for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$nx=$x+$dx;$ny=$y+$dy;if($nx -ge 0 -and $nx -lt 16 -and $ny -ge 0 -and $ny -lt 16){$b[$ny*16+$nx]=$a[$y*16+$x]}}};,$b}
function Blit($d,$s,$dx,$dy){for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$c=$s[$y*16+$x];if($c -ne "00000000"){P $d ($x+$dx) ($y+$dy) $c}}}}
function FlipH($a){$b=Tp;for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$b[$y*16+(15-$x)]=$a[$y*16+$x]}};,$b}
function Reg($fam,$name,$draw,$w,$h,$places){foreach($pl in $places){
 $dx=[int][Math]::Floor((16-$w)/2);$dy=[int][Math]::Floor((16-$h)/2)
 if($pl -eq "NO"){$dx=0;$dy=0}elseif($pl -eq "NE"){$dx=16-$w;$dy=0}elseif($pl -eq "SO"){$dx=0;$dy=16-$h}elseif($pl -eq "SE"){$dx=16-$w;$dy=16-$h}
 $T["$fam/$name`_$pl"]=@{draw=$draw;dx=$dx;dy=$dy}}}
function RoundC($a,$w,$h){foreach($p in @(@(0,0),@(($w-1),0),@(0,($h-1)),@(($w-1),($h-1)))){P $a $p[0] $p[1] "00000000"}}
function DeskObj($w,$h,$top,$frame,$detail){$a=Tp;Rc $a 0 0 $w $h $frame;Rc $a 1 1 ($w-2) ($h-2) $top;Rc $a 1 1 ($w-2) 1 (Hx $top 0.25);Rc $a 1 ($h-2) ($w-2) 1 (Hx $top -0.18)
 if($detail){Rc $a 2 3 3 2 "F4F4F0";P $a 2 3 "FFFFFF";P $a 7 3 "3A7AD8";P $a 7 4 "3A7AD8";P $a 7 5 "F2C84B"};RoundC $a $w $h;,$a}
function ChairObj($w,$h,$seat,$back){$a=Tp;Rc $a 0 0 $w 2 $back;Rc $a 0 0 $w 1 (Hx $back 0.25);Rc $a 0 2 $w ($h-2) $seat;Rc $a 1 3 ($w-2) 1 (Hx $seat 0.22);Rc $a 0 2 1 ($h-2) (Hx $seat -0.2);Rc $a ($w-1) 2 1 ($h-2) (Hx $seat -0.2);Rc $a 0 ($h-1) $w 1 (Hx $seat -0.3);RoundC $a $w $h;,$a}
function LongDesk($part,$top,$frame){$a=Tp;$x0=0;$x1=15;if($part -eq "esq"){$x0=3};if($part -eq "dre"){$x1=12}
 Rc $a $x0 4 ($x1-$x0+1) 8 $frame;Rc $a ($x0+$(if($part -eq "esq"){1}else{0})) 5 ($x1-$x0+1-$(if($part -ne "mig"){1}else{0})) 6 $top;Rc $a $x0 5 ($x1-$x0+1) 1 (Hx $top 0.25);Rc $a $x0 10 ($x1-$x0+1) 1 (Hx $top -0.18)
 if($part -eq "esq"){Rc $a 4 5 12 6 $top;Rc $a 4 5 12 1 (Hx $top 0.25);Rc $a 4 10 12 1 (Hx $top -0.18);Rc $a 3 4 13 1 $frame;Rc $a 3 11 13 1 $frame;Rc $a 3 4 1 8 $frame;P $a 3 4 "00000000";P $a 3 11 "00000000"}
 if($part -eq "dre"){Rc $a 0 5 12 6 $top;Rc $a 0 5 12 1 (Hx $top 0.25);Rc $a 0 10 12 1 (Hx $top -0.18);Rc $a 0 4 13 1 $frame;Rc $a 0 11 13 1 $frame;Rc $a 12 4 1 8 $frame;P $a 12 4 "00000000";P $a 12 11 "00000000"}
 if($part -eq "mig"){Rc $a 0 4 16 1 $frame;Rc $a 0 11 16 1 $frame}
 ,$a}
function Burner($a,$cx,$cy,$on){$ring=if($on){"E8481E"}else{"2A2E36"};$core=if($on){"FFC04A"}else{"4A505A"};Disc $a $cx $cy 3.3 "1A1E24";Disc $a $cx $cy 2.7 $ring;Disc $a $cx $cy 1.3 $core}
function Stove($on){$a=Tp;Rc $a 0 0 14 14 "6A727C";Rc $a 1 1 12 11 "B0B8C2";Rc $a 1 1 12 1 "D0D8E0";Burner $a 4 4 $on;Burner $a 10 4 ($on -and $true);Burner $a 4 9 $false;Burner $a 10 9 $on;Rc $a 6 1 2 11 "00000000";Rc $a 6 1 2 11 "A0A8B2";Rc $a 1 6 12 1 "A0A8B2";foreach($x in 3,6,8,11){P $a $x 13 "E8E8E8";P $a $x 12 "8A929E"};,$a}
function Sink(){$a=Tp;Rc $a 0 0 14 12 "6A727C";Rc $a 1 1 12 10 "C4CCD4";Rc $a 2 2 7 7 "8A929C";Rc $a 3 3 5 5 "6A727C";Rc $a 3 3 5 1 "5A626C";P $a 5 5 "2A2E36";Rc $a 10 2 3 3 "E4EAF0";Rc $a 11 4 1 3 "9AA2AC";for($x=10;$x -lt 13;$x++){P $a $x 8 "A8B0BA";P $a $x 9 "A8B0BA"};,$a}
function FridgeTop(){$a=Tp;Rc $a 0 0 12 12 "B4BCC4";Rc $a 1 1 10 10 "E8ECF0";Rc $a 1 1 10 1 "FFFFFF";Rc $a 0 5 12 1 "B4BCC4";Rc $a 10 2 1 3 "8A929E";Rc $a 10 7 1 4 "8A929E";,$a}
function Pot(){$a=Tp;Disc $a 3.5 3.5 3.9 "2A2E36";Disc $a 3.5 3.5 3.1 "7A828C";Disc $a 3.5 3.5 2.2 "A8B0BA";Rc $a 0 3 1 2 "2A2E36";Rc $a 7 3 1 2 "2A2E36";P $a 3 3 "2A2E36";P $a 4 3 "2A2E36";P $a 2 2 "E8F0F8";,$a}
function Pan(){$a=Tp;Disc $a 6 6 6 "2A2E36";Disc $a 6 6 5 "4A505A";Disc $a 6 6 4 "E8C860";foreach($p in @(@(4,5),@(7,4),@(6,7),@(8,7),@(5,8))){P $a $p[0] $p[1] "C8402A"};Rc $a 12 5 2 2 "2A2E36";,$a}
function Basket($rng){$a=Tp;Disc $a 4 4 3.8 "7A4E28";Disc $a 4 4 3 "A8723C";foreach($p in @(@(3,3,"D83A3A"),@(5,3,"F29A2E"),@(3,5,"7AC040"),@(5,5,"D83A3A"),@(4,4,"F2D84B"))){P $a $p[0] $p[1] $p[2];P $a $p[0] ($p[1]-1) (Hx $p[2] 0.4)};,$a}
function Toilet(){$a=Tp;Rc $a 0 0 8 3 "B8C0C8";Rc $a 1 0 6 2 "F4F6F8";P $a 4 1 "8A929E";Ell $a 3.5 6.5 3.9 4.3 "B8C0C8";Ell $a 3.5 6.5 3.3 3.8 "F4F6F8";Ell $a 3.5 7 2.2 2.6 "6FA8D8";Ell $a 3 6.2 1.2 1.3 "A8D0EC";P $a 1 3 "8A929E";P $a 6 3 "8A929E";,$a}
function Basin(){$a=Tp;Rc $a 0 0 10 9 "B8C0C8";Rc $a 1 1 8 7 "F4F6F8";Ell $a 5 5 3.2 2.6 "C8D6DE";Ell $a 5 5 2.4 1.9 "A8C8DC";P $a 5 5 "6A7884";Rc $a 4 1 2 1 "9AA2AC";P $a 3 1 "6AA0D8";P $a 6 1 "D86A6A";RoundC $a 10 9;,$a}
function ShowerTray(){$a=Tp;Rc $a 0 0 12 12 "B8C0C8";Rc $a 1 1 10 10 "E8EEF2";Disc $a 5.5 5.5 3 "D0DAE2";Disc $a 5.5 5.5 1.2 "8A929E";P $a 5 5 "5A626C";for($i=0;$i -lt 4;$i++){P $a (5+$i%2) (5+[int][Math]::Floor($i/2)) "6A727C"};Rc $a 1 1 10 1 "FFFFFF";,$a}
function Tub($side){$a=Tp;Rc $a 2 2 14 12 "B8C0C8";Rc $a 3 3 13 10 "F4F6F8";Rc $a 4 4 12 8 "A8D0EC";Rc $a 5 5 11 6 "7AB8E0";Rc $a 5 5 11 1 "A8D8F4";Rc $a 5 10 11 1 "5E9CC8";Rc $a 2 2 1 12 "B8C0C8";P $a 2 2 "00000000";P $a 2 13 "00000000";P $a 3 3 "00000000";P $a 3 12 "00000000";Rc $a 3 6 1 4 "B8C0C8";P $a 3 7 "9AA2AC";P $a 3 8 "9AA2AC";P $a 6 7 "FFFFFF";P $a 9 8 "FFFFFF";if($side -eq "dre"){$a=FlipH $a};,$a}
function BathMat(){$a=Tp;Rc $a 0 0 10 6 "5A9AD0";Rc $a 1 1 8 4 "7AB8E8";for($x=0;$x -lt 10;$x+=2){P $a $x 5 "5A9AD0"};Rc $a 1 1 8 1 "A0D0F4";RoundC $a 10 6;,$a}
function Hamper(){$a=Tp;Disc $a 4 4 3.8 "8A5E36";Disc $a 4 4 3 "B8885A";foreach($p in @(@(3,3,"F6F4EC"),@(5,4,"4A78C8"),@(3,5,"F2A6C8"))){Rc $a $p[0] $p[1] 2 1 $p[2]};,$a}
function Washer(){$a=Tp;Rc $a 0 0 12 12 "B4BCC4";Rc $a 1 1 10 10 "F0F4F8";Rc $a 1 1 10 2 "D0D8E0";P $a 8 2 "3AE08A";P $a 9 2 "F2C84B";P $a 2 2 "8A929E";Rc $a 3 5 5 4 "E0E8EE";Rc $a 3 5 5 1 "FFFFFF";,$a}
function Backpack($c){$a=Tp;Rc $a 0 0 6 7 (Hx $c -0.35);Rc $a 1 1 4 5 $c;Rc $a 1 1 4 1 (Hx $c 0.3);Rc $a 1 4 4 2 (Hx $c -0.2);Rc $a 2 3 2 1 "E8D8B0";P $a 0 6 "00000000";P $a 5 6 "00000000";,$a}
function Books(){$a=Tp;Rc $a 0 1 7 5 "3A7AD8";Rc $a 0 1 7 1 "6A9AF0";Rc $a 1 3 7 3 "D83A3A";Rc $a 1 3 7 1 "F26A6A";Rc $a 0 5 5 1 "F4F0E0";Rc $a 2 0 4 1 "3AB04A";,$a}
function HBed($part){$a=Tp;$m="9AA2AC";$ml="C8D0D8";$sheet="9CCCE8";$bl=Hx $sheet 0.3;$bd=Hx $sheet -0.25
 if($part -eq "cap"){Rc $a 0 3 1 13 $m;Rc $a 15 3 1 13 $m;Rc $a 0 0 16 3 "B8C0C8";Rc $a 0 0 16 1 $ml;Rc $a 0 2 16 1 $m;Rc $a 1 3 14 13 "F4F8FA";Rc $a 4 4 8 4 "FFFFFF";Rc $a 4 7 8 1 "D4DCE4";Rc $a 4 4 8 1 "FFFFFF";Rc $a 1 9 14 7 $sheet;Rc $a 1 9 14 2 "FFFFFF";Rc $a 1 11 14 1 $bd}
 else{Rc $a 0 0 1 13 $m;Rc $a 15 0 1 13 $m;Rc $a 1 0 14 12 $sheet;Rc $a 1 4 14 1 $bd;Rc $a 1 5 14 1 $bl;Rc $a 1 9 14 1 $bd;Rc $a 0 12 16 3 "B8C0C8";Rc $a 0 12 16 1 $ml;Rc $a 0 14 16 1 $m;P $a 1 15 "2A2E36";P $a 14 15 "2A2E36"}
 ,$a}
function Stretcher(){$a=Tp;Rc $a 0 0 8 14 "9AA2AC";Rc $a 1 1 6 12 "F4F8FA";Rc $a 2 1 4 3 "FFFFFF";Rc $a 2 3 4 1 "D4DCE4";Rc $a 1 5 6 8 "9CCCE8";Rc $a 1 5 6 1 "FFFFFF";Rc $a 1 9 6 1 "6A98C0";foreach($p in @(@(0,0),@(7,0),@(0,13),@(7,13))){P $a $p[0] $p[1] "2A2E36"};,$a}
function OpTable(){$a=Tp;Rc $a 0 0 10 14 "6A727C";Rc $a 1 1 8 12 "7AB8B0";Rc $a 1 1 8 1 "A8E0D8";Rc $a 2 2 6 3 "5AA098";Rc $a 2 6 6 6 "F4F8FA";Rc $a 4 4 2 10 "9AA2AC";RoundC $a 10 14;,$a}
function Wheelchair(){$a=Tp;Rc $a 0 3 1 5 "2A2E36";Rc $a 7 3 1 5 "2A2E36";Rc $a 1 3 6 5 "3A4048";Rc $a 1 3 6 1 "5A606C";Rc $a 1 2 6 1 "2A2E36";Rc $a 1 0 1 2 "6A727C";Rc $a 6 0 1 2 "6A727C";Rc $a 2 8 4 1 "6A727C";,$a}
function VitalMon(){$a=Tp;Rc $a 0 0 8 8 "3A4048";Rc $a 1 1 6 4 "0A2018";foreach($p in @(@(1,3),@(2,3),@(3,2),@(4,4),@(5,3),@(6,3))){P $a $p[0] $p[1] "3AE08A"};P $a 2 6 "D83A3A";P $a 5 6 "F2C84B";RoundC $a 8 8;,$a}
function IVStand(){$a=Tp;Ln $a 3 3 0 0 "9AA2AC";Ln $a 4 3 7 0 "9AA2AC";Ln $a 3 4 0 7 "9AA2AC";Ln $a 4 4 7 7 "9AA2AC";Ln $a 3 3 4 3 "9AA2AC";Rc $a 3 3 2 2 "6A727C";Rc $a 3 3 2 1 "8AC8F0";foreach($p in @(@(0,0),@(7,0),@(0,7),@(7,7))){P $a $p[0] $p[1] "2A2E36"};,$a}
function MedCart(){$a=Tp;Rc $a 0 0 12 8 "6A727C";Rc $a 1 1 10 6 "E8ECF0";Rc $a 2 2 3 2 "D83A3A";P $a 3 2 "FFFFFF";Rc $a 6 2 2 3 "3A7AD8";Rc $a 9 2 2 2 "3AB04A";Rc $a 3 5 5 1 "F2C84B";foreach($p in @(@(0,0),@(11,0),@(0,7),@(11,7))){P $a $p[0] $p[1] "2A2E36"};,$a}
function SurgLamp(){$a=Tp;Disc $a 6.5 6.5 6.4 "6A727C";Disc $a 6.5 6.5 5.6 "C8D0D8";foreach($p in @(@(6.5,3),@(10,6.5),@(6.5,10),@(3,6.5),@(9,4),@(4,9))){Disc $a $p[0] $p[1] 0.9 "FFF8D0"};Disc $a 6.5 6.5 1.7 "FFE890";Disc $a 6.5 6.5 0.8 "FFFFFF";,$a}
function Bench($part){$a=Tp;Rc $a 0 3 16 3 "3A5A98";Rc $a 0 3 16 1 "5A7AB8";Rc $a 0 6 16 7 "4A78C8";Rc $a 0 6 16 1 "6A98E0";for($x=5;$x -lt 16;$x+=5){Rc $a $x 6 1 7 "3A5A98"};Rc $a 0 12 16 1 "2E4A7A"
 if($part -eq "esq"){Rc $a 0 3 2 10 "2E4A7A";Rc $a 0 3 2 1 "4A6AA8"};if($part -eq "dre"){Rc $a 14 3 2 10 "2E4A7A";Rc $a 14 3 2 1 "4A6AA8"};Rc $a 0 13 16 1 "00000050";,$a}
function CrossSign(){$a=Tp;Rc $a 2 2 12 12 "B8C0C8";Rc $a 3 3 10 10 "F4F8F4";Rc $a 6 4 4 8 "2AA860";Rc $a 4 6 8 4 "2AA860";Rc $a 6 4 4 1 "5AD890";Rc $a 4 6 4 1 "5AD890";,$a}
function ScaleObj(){$a=Tp;Rc $a 0 0 8 10 "6A727C";Rc $a 1 1 6 8 "B4BCC4";Rc $a 1 1 6 1 "E8ECF0";Rc $a 2 7 4 2 "0A2018";P $a 2 8 "3AE08A";P $a 4 8 "3AE08A";P $a 5 8 "3AE08A";P $a 3 3 "8A929E";P $a 4 5 "8A929E";P $a 5 3 "8A929E";RoundC $a 8 10;,$a}
function MedCabinet(){$a=Tp;Rc $a 2 1 12 14 "B8C0C8";Rc $a 3 2 10 12 "F4F8FA";Rc $a 3 2 10 1 "FFFFFF";Rc $a 7 4 2 6 "2AA860";Rc $a 5 6 6 2 "2AA860";Rc $a 8 1 1 14 "B8C0C8";P $a 7 8 "6A727C";P $a 9 8 "6A727C";Rc $a 2 15 12 1 "00000050";,$a}
function OfficeDesk(){$a=DeskObj 14 9 "C8A878" "7A5A38" $false;Rc $a 4 1 6 1 "2A2E36";Rc $a 6 2 2 1 "6A727C";Rc $a 4 4 6 2 "3A3F48";Rc $a 4 4 6 1 "5A606C";foreach($x in 5,7,9){P $a $x 5 "7A828C"};P $a 11 5 "2A2E36";Rc $a 2 4 2 2 "D83A3A";P $a 2 4 "F8806A";Rc $a 11 2 2 2 "F4F4F0";,$a}
function OChair($c){$a=Tp;Disc $a 3.5 4 3.6 "1A1E24";Disc $a 3.5 4 2.9 $c;Disc $a 3.5 4.3 1.8 (Hx $c 0.15);Rc $a 1 0 6 1 (Hx $c -0.2);Rc $a 0 3 1 3 "1A1E24";Rc $a 7 3 1 3 "1A1E24";P $a 2 3 (Hx $c 0.35);,$a}
function Panel($arms){$a=Tp;$c="8A9CB8";$h="B0BED4";$d="5A6A84"
 if($arms.Contains("L")){Rc $a 0 7 8 2 $c;Rc $a 0 7 8 1 $h;Rc $a 0 8 8 1 $d};if($arms.Contains("R")){Rc $a 8 7 8 2 $c;Rc $a 8 7 8 1 $h;Rc $a 8 8 8 1 $d}
 if($arms.Contains("U")){Rc $a 7 0 2 8 $c;Rc $a 7 0 1 8 $h;Rc $a 8 0 1 8 $d};if($arms.Contains("D")){Rc $a 7 8 2 8 $c;Rc $a 7 8 1 8 $h;Rc $a 8 8 1 8 $d}
 Rc $a 7 7 2 2 $c;P $a 7 7 $h;P $a 8 8 $d;,$a}
function Printer(){$a=Tp;Rc $a 0 0 10 9 "8A929E";Rc $a 1 1 8 7 "C8CCD4";Rc $a 1 1 8 1 "E8ECF0";Rc $a 2 2 6 3 "B4BAC4";Rc $a 3 3 4 1 "F4F4F0";Rc $a 2 6 3 1 "3AE08A";P $a 7 6 "D83A3A";P $a 8 6 "F2C84B";RoundC $a 10 9;,$a}
function Copier(){$a=Tp;Rc $a 0 0 12 10 "6A727C";Rc $a 1 1 10 8 "C8CCD4";Rc $a 2 2 8 5 "5A7898";Rc $a 2 2 8 1 "8AB0D0";for($i=0;$i -lt 4;$i++){P $a (3+$i) (5-$i) "C8E0F4"};Rc $a 2 7 3 1 "3A3F48";P $a 7 7 "3AE08A";P $a 8 7 "D83A3A";,$a}
function FileCab(){$a=Tp;Rc $a 2 1 12 14 "6A727C";Rc $a 3 2 10 12 "8A929E";Rc $a 3 2 10 1 "B4BCC8";foreach($y in 4,8,12){Rc $a 3 $y 10 1 "5A626E";Rc $a 6 ($y-2) 4 1 "2A2E36";Rc $a 5 ($y-3) 6 1 "F4F4E8"};Rc $a 2 15 12 1 "00000050";,$a}
function Cooler(){$a=Tp;Rc $a 5 0 6 6 "8AC8F0";Rc $a 5 0 6 1 "BCE4FF";Rc $a 6 1 1 3 "E8F8FF";Rc $a 4 6 8 9 "E8ECF0";Rc $a 4 6 8 1 "FFFFFF";P $a 6 9 "3A7AD8";P $a 9 9 "D83A3A";Rc $a 6 11 4 2 "2A2E36";Rc $a 4 15 8 1 "00000050";,$a}
function PlantTop(){$a=Tp;Disc $a 3.5 3.5 3.9 "2A7A38";Disc $a 3.5 3.5 3 "3A9A48";for($i=0;$i -lt 6;$i++){$an=$i*1.047;P $a ([int][Math]::Round(3.5+2.4*[Math]::Cos($an))) ([int][Math]::Round(3.5+2.4*[Math]::Sin($an))) "6ACC74"};Disc $a 3.5 3.5 0.9 "C8704A";,$a}
function BinTop(){$a=Tp;Disc $a 2.5 2.5 2.9 "3E444E";Disc $a 2.5 2.5 2.2 "6A727C";Disc $a 2.5 2.5 1.3 "2A2E36";,$a}
function Papers(){$a=Tp;Rc $a 0 1 6 5 "E4E4DC";Rc $a 1 0 6 5 "F4F4F0";Rc $a 2 1 4 1 "9AA0A8";Rc $a 2 3 3 1 "9AA0A8";Rc $a 1 0 6 1 "FFFFFF";,$a}
function Laptop(){$a=Tp;Rc $a 1 0 6 2 "2A2E36";Rc $a 2 0 4 1 "5A90D0";Rc $a 0 2 8 5 "8A929E";Rc $a 1 3 6 2 "3A3F48";foreach($x in 2,4,6){P $a $x 4 "6A727C"};Rc $a 3 6 2 1 "B4BCC8";,$a}
function PhoneObj(){$a=Tp;Rc $a 0 0 5 4 "2A2E36";Rc $a 1 1 3 2 "5A606C";P $a 0 3 "00000000";P $a 4 3 "00000000";P $a 2 1 "3AE08A";,$a}
function Whiteboard(){$a=Tp;Rc $a 0 1 16 12 "9AA2AC";Rc $a 1 2 14 9 "F8FAFC";Ln $a 3 4 8 4 "3A7AD8";Ln $a 3 6 11 6 "3A7AD8";Ln $a 3 8 6 8 "D83A3A";Rc $a 10 8 3 2 "F2C84B";Rc $a 1 11 14 1 "C8D0D8";P $a 3 12 "D83A3A";P $a 5 12 "3A7AD8";Rc $a 0 13 16 1 "00000050";,$a}
function CorkBoard(){$a=Tp;Rc $a 0 1 16 12 "8A5A30";Rc $a 1 2 14 10 "C8A070";Speck $a (New-Object System.Random 5) 14 @("B88A58","D8B080");Rc $a 2 3 4 4 "F8F4E0";Rc $a 8 4 4 3 "F2D84B";Rc $a 4 8 4 3 "F2A6C8";Rc $a 10 8 3 3 "8AC8F0";P $a 3 3 "D83A3A";P $a 9 4 "D83A3A";P $a 5 8 "D83A3A";P $a 11 8 "D83A3A";Rc $a 0 13 16 1 "00000050";,$a}
function WallChart(){$a=Tp;Rc $a 1 1 14 12 "9AA2AC";Rc $a 2 2 12 10 "F8FAFC";Rc $a 3 8 2 3 "3A7AD8";Rc $a 6 5 2 6 "3AB04A";Rc $a 9 7 2 4 "F2C84B";Rc $a 12 3 1 8 "D83A3A";Ln $a 3 11 12 11 "6A727C";Rc $a 1 13 14 1 "00000050";,$a}
function Container($part,$col){$a=LongDesk $part $col (Hx $col -0.4);for($x=0;$x -lt 16;$x+=2){$ok=$true;if($part -eq "esq" -and $x -lt 5){$ok=$false};if($part -eq "dre" -and $x -gt 11){$ok=$false};if($ok){Rc $a $x 6 1 4 (Hx $col -0.22)}};,$a}
function Rail($arms){$a=Tp;$w="E8ECF0";$d="5A626C"
 if($arms.Contains("L")){Rc $a 0 7 8 1 $w};if($arms.Contains("R")){Rc $a 8 7 8 1 $w};if($arms.Contains("U")){Rc $a 7 0 1 8 $w};if($arms.Contains("D")){Rc $a 7 8 1 8 $w}
 foreach($p in 1,5,9,13){if($arms.Contains("L") -or $arms.Contains("R")){if(($p -lt 8 -and $arms.Contains("L")) -or ($p -ge 8 -and $arms.Contains("R"))){Rc $a $p 6 2 3 $d;P $a $p 6 "8A929E"}};if($arms.Contains("U") -or $arms.Contains("D")){if(($p -lt 8 -and $arms.Contains("U")) -or ($p -ge 8 -and $arms.Contains("D"))){Rc $a 6 $p 3 2 $d;P $a 6 $p "8A929E"}}}
 Rc $a 6 6 3 3 $d;P $a 6 6 "8A929E";,$a}
function RowBoat($part){$a=Tp;$hull="8A5A30";$in="C8A070";$dk="5E3C20"
 for($y=0;$y -lt 16;$y++){if($part -eq "cap"){$hw=[Math]::Min(7.0,1.0+$y*0.45)}else{$hw=7.0;if($y -ge 13){$hw=6.0-($y-13)*0.5}}
  $x0=[int][Math]::Ceiling(7.5-$hw);$x1=[int][Math]::Floor(7.5+$hw);if($x1 -lt $x0){continue}
  Rc $a $x0 $y ($x1-$x0+1) 1 $hull;if(($x1-$x0) -ge 4){Rc $a ($x0+2) $y ($x1-$x0-3) 1 $in};P $a $x0 $y $dk;P $a $x1 $y $dk}
 if($part -eq "cap"){Rc $a 4 10 8 2 $hull;Rc $a 4 10 8 1 "A8723C";P $a 7 0 $dk;P $a 8 0 $dk}else{Rc $a 4 3 8 2 $hull;Rc $a 4 3 8 1 "A8723C";Rc $a 5 9 6 1 $dk;Rc $a 6 10 4 3 "3A3F48";Rc $a 6 10 4 1 "6A727C";P $a 7 13 "2A2E36";P $a 8 13 "2A2E36"}
 ,$a}
function AnchorObj(){$a=Tp;$c="A8B2BE";Rc $a 4 1 2 9 $c;Rc $a 4 1 1 9 "E8F0F8";Rc $a 2 0 6 1 $c;Rc $a 3 2 4 1 $c;Ln $a 5 10 0 7 $c;Ln $a 4 10 9 7 $c;Ln $a 4 9 0 6 $c;Ln $a 5 9 9 6 $c;Rc $a 0 6 2 1 $c;Rc $a 8 6 2 1 $c;P $a 4 0 "6A727C";,$a}
function HelmObj(){$a=Tp;Disc $a 5.5 5.5 5.5 "5E3C20";Disc $a 5.5 5.5 4.3 "00000000";foreach($p in @(@(5,0,5,10),@(0,5,10,5),@(1,1,10,10),@(10,1,1,10))){Ln $a $p[0] $p[1] $p[2] $p[3] "8A5A30"};Disc $a 5.5 5.5 1.6 "C8A070";foreach($p in @(@(5,0),@(6,0),@(5,11),@(6,11),@(0,5),@(0,6),@(11,5),@(11,6))){P $a $p[0] $p[1] "A8723C"};,$a}
function LifeRing(){$a=Tp;for($y=0;$y -lt 8;$y++){for($x=0;$x -lt 8;$x++){$dx=$x-3.5;$dy=$y-3.5;$d=[Math]::Sqrt($dx*$dx+$dy*$dy);if($d -le 3.9 -and $d -ge 2.0){$an=[Math]::Atan2($dy,$dx);$k=[int][Math]::Floor(($an+3.1416)/0.7854);P $a $x $y $(if($k%2){"F4F4F0"}else{"D83A3A"})}}};,$a}
function LightHouse(){$a=Tp;Ln $a 6 6 12 0 "FFF2A840";Ln $a 6 6 0 0 "FFF2A840";Ln $a 6 6 12 12 "FFF2A840";Ln $a 6 6 0 12 "FFF2A840";Disc $a 5.5 5.5 5.5 "6A727C";Disc $a 5.5 5.5 4.6 "E8E8E8";Disc $a 5.5 5.5 3.6 "D83A3A";Disc $a 5.5 5.5 2.6 "F4F4F0";Disc $a 5.5 5.5 1.5 "F2D84B";P $a 5 5 "FFFFFF";,$a}
function RopeCoil(){$a=Tp;Disc $a 3.5 3.5 3.9 "6A4A28";Disc $a 3.5 3.5 3.1 "B8905A";Disc $a 3.5 3.5 2.3 "7A5A32";Disc $a 3.5 3.5 1.6 "B8905A";Disc $a 3.5 3.5 0.7 "4A301A";,$a}
function BarrelTop(){$a=Tp;Disc $a 3.5 3.5 3.9 "4E3220";Disc $a 3.5 3.5 3.2 "A8723C";Disc $a 3.5 3.5 2.4 "B8844C";Rc $a 3 1 1 5 "8A5A30";Rc $a 5 1 1 5 "8A5A30";Disc $a 3.5 3.5 3.3 "00000000";Disc $a 3.5 3.5 3.9 "4E3220";Disc $a 3.5 3.5 3.1 "A8723C";Rc $a 3 1 1 5 "8A5A30";Rc $a 5 1 1 5 "8A5A30";P $a 2 2 "D8A468";,$a}
function Crate(){$a=Tp;Rc $a 0 0 10 10 "5E3C20";Rc $a 1 1 8 8 "B8885A";Ln $a 1 1 8 8 "7A5230";Ln $a 8 1 1 8 "7A5230";Rc $a 1 1 8 1 "D8A878";foreach($p in @(@(1,1),@(8,1),@(1,8),@(8,8))){P $a $p[0] $p[1] "3A3F48"};,$a}
function FishBox(){$a=Tp;Rc $a 0 0 10 8 "C8D0D8";Rc $a 1 1 8 6 "E8F4FA";Rc $a 2 2 6 4 "BCD8E8";Rc $a 2 3 4 1 "C8D0D8";Rc $a 3 4 4 1 "A8B0BA";P $a 2 3 "FFFFFF";P $a 6 3 "8A929E";P $a 7 4 "6A727C";P $a 4 4 "2A2E36";RoundC $a 10 8;,$a}
function NetPile(){$a=Tp;$m=Mask 0 1 11 7 2;for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($m[$y*16+$x]){if(($x+$y)%3 -eq 0 -or ($x-$y+16)%3 -eq 0){P $a $x $y "A8884A"}else{P $a $x $y "A8884A30"}}}};foreach($p in @(@(2,2),@(7,3),@(5,5),@(10,5))){P $a $p[0] $p[1] "D83A3A";P $a ($p[0]+1) $p[1] "F4F4F0"};,$a}
function Bollard(){$a=Tp;Disc $a 2.5 2.5 2.9 "1A1E24";Disc $a 2.5 2.5 2.1 "7A828E";Disc $a 2.5 2.5 1.2 "3A3F48";P $a 1 1 "C8D0D8";,$a}
function PostTop(){$a=Tp;Disc $a 2.5 2.5 2.9 "3C2412";Disc $a 2.5 2.5 2.2 "7A5230";Disc $a 2.5 2.5 1.2 "A8723C";P $a 2 2 "D8A468";,$a}
function Ribbed($c){$a=Mk $c;for($x=0;$x -lt 16;$x++){$m=$x%4;if($m -eq 0){Rc $a $x 0 1 16 (Hx $c 0.25)}elseif($m -eq 3){Rc $a $x 0 1 16 (Hx $c -0.25)}};Rc $a 0 13 16 3 (Hx $c -0.15);Rc $a 0 13 16 1 (Hx $c -0.4);for($i=0;$i -lt 5;$i++){P $a (2+$i*3) 14 (Hx $c -0.45)};,$a}
function Microscope(){$a=Tp;Rc $a 1 5 6 5 "E8ECF0";Rc $a 1 5 6 1 "FFFFFF";Rc $a 3 1 2 6 "C8D0D8";Rc $a 3 0 2 3 "2A2E36";P $a 3 0 "6A727C";Rc $a 1 6 6 1 "8A929E";P $a 4 8 "FFF2A8";P $a 1 9 "D83A3A";P $a 6 9 "3A7AD8";RoundC $a 8 10;,$a}
function TubeRack(){$a=Tp;Rc $a 0 0 12 6 "6A727C";Rc $a 1 1 10 4 "3A4048";$i=0;foreach($x in 1,4,7,10){$c=@("D83A3A","3AB04A","3A7AD8","F2C84B")[$i%4];$i++;foreach($y in 1,3){if($x+1 -lt 11){Rc $a $x $y 2 2 $c;P $a $x $y (Hx $c 0.5)}}};,$a}
function Flask($c){$a=Tp;Disc $a 2.5 2.5 2.9 "B8D4E0";Disc $a 2.5 2.5 2.3 $c;Disc $a 2.5 2.5 1.2 (Hx $c 0.3);P $a 1 1 "FFFFFF";Disc $a 2.5 2.5 0.6 "B8D4E0";,$a}
function Bunsen(){$a=Tp;Disc $a 2 2 2.4 "4A505A";Disc $a 2 2 1.7 "3A7AD8";Disc $a 2 2 0.9 "FFE890";,$a}
function Centrifuge(){$a=Tp;Disc $a 4.5 4.5 4.9 "4A505A";Disc $a 4.5 4.5 4.2 "C8D0D8";Disc $a 4.5 4.5 3.4 "8A929E";for($i=0;$i -lt 8;$i++){$an=$i*0.7854;P $a ([int][Math]::Round(4.5+2.3*[Math]::Cos($an))) ([int][Math]::Round(4.5+2.3*[Math]::Sin($an))) $(if($i%2){"D83A3A"}else{"2A2E36"})};Disc $a 4.5 4.5 0.9 "F2C84B";,$a}
function Fumehood(){$a=Tp;Rc $a 1 2 14 12 "B4BCC4";Rc $a 2 3 12 7 "4A7088";Rc $a 2 3 12 1 "7AA8C0";for($i=0;$i -lt 5;$i++){P $a (4+$i) (8-$i) "A8D0E4"};Rc $a 1 10 14 4 "E8ECF0";Rc $a 2 11 12 1 "B4BCC4";P $a 13 12 "3AE08A";Rc $a 8 3 1 7 "B4BCC4";Rc $a 1 14 14 1 "00000050";,$a}
function SpecTank($c){$a=Tp;Rc $a 0 0 12 12 "6A727C";Rc $a 1 1 10 10 "B8D4E0";Rc $a 2 2 8 8 $c;Rc $a 2 2 8 1 (Hx $c 0.3);foreach($p in @(@(3,8),@(8,5),@(5,3),@(9,9))){P $a $p[0] $p[1] (Hx $c 0.55)};Disc $a 5.5 6 1.6 (Hx $c -0.5);Ln $a 4 7 3 9 (Hx $c -0.5);Ln $a 7 7 8 9 (Hx $c -0.5);P $a 5 5 (Hx $c 0.6);RoundC $a 12 12;,$a}
function Vat($c){$a=Mk "4A505A";Rc $a 1 1 14 14 "6A727C";Rc $a 2 2 12 12 "2A2E36";Rc $a 3 3 10 10 $c;Rc $a 3 3 10 2 (Hx $c 0.25);foreach($p in @(@(5,8),@(10,6),@(8,11),@(11,10),@(6,5))){P $a $p[0] $p[1] (Hx $c 0.6);P $a ($p[0]+1) ($p[1]+1) (Hx $c 0.3)};Rc $a 0 0 16 1 "8A929E";Rc $a 0 0 1 16 "8A929E";foreach($p in @(@(1,1),@(14,1),@(1,14),@(14,14))){P $a $p[0] $p[1] "B4BCC8"};,$a}
function Pipe($arms,$c){$a=Tp;$lt=Hx $c 0.3;$dk=Hx $c -0.3;$fl=Hx $c -0.5
 if($arms.Contains("L")){Rc $a 0 6 8 4 $c;Rc $a 0 6 8 1 $lt;Rc $a 0 9 8 1 $dk};if($arms.Contains("R")){Rc $a 8 6 8 4 $c;Rc $a 8 6 8 1 $lt;Rc $a 8 9 8 1 $dk}
 if($arms.Contains("U")){Rc $a 6 0 4 8 $c;Rc $a 6 0 1 8 $lt;Rc $a 9 0 1 8 $dk};if($arms.Contains("D")){Rc $a 6 8 4 8 $c;Rc $a 6 8 1 8 $lt;Rc $a 9 8 1 8 $dk}
 Rc $a 6 6 4 4 $c;P $a 6 6 $lt;P $a 9 9 $dk
 if($arms.Contains("L")){Rc $a 0 5 1 6 $fl};if($arms.Contains("R")){Rc $a 15 5 1 6 $fl};if($arms.Contains("U")){Rc $a 5 0 6 1 $fl};if($arms.Contains("D")){Rc $a 5 15 6 1 $fl};,$a}
function ValveObj(){$a=Tp;Disc $a 2.5 2.5 2.9 "5A626C";Disc $a 2.5 2.5 2.1 "D83A3A";Disc $a 2.5 2.5 1.0 "5A626C";Rc $a 2 0 1 6 "8A1E2A";Rc $a 0 2 6 1 "8A1E2A";P $a 2 2 "F2C84B";,$a}
function Tesla(){$a=Tp;Disc $a 4.5 4.5 4.9 "2A2E36";Disc $a 4.5 4.5 4 "4A505A";Disc $a 4.5 4.5 2.8 "7AB8F0";Disc $a 4.5 4.5 1.4 "E8F8FF";Ln $a 4 4 8 1 "BCE4FF";Ln $a 4 5 0 6 "BCE4FF";Ln $a 5 5 6 8 "BCE4FF";,$a}
function Cage(){$a=Tp;Rc $a 0 0 10 8 "8A929E";Rc $a 1 1 8 6 "D8C898";for($x=1;$x -lt 9;$x+=2){Rc $a $x 1 1 6 "B8BEC8"};P $a 3 3 "F4F4F8";P $a 4 3 "F4F4F8";P $a 3 2 "F2A6C8";P $a 6 5 "D8D8E0";P $a 7 5 "D8D8E0";RoundC $a 10 8;,$a}
function Stool(){$a=Tp;Disc $a 2.5 2.5 2.9 "4A505A";Disc $a 2.5 2.5 2.2 "E8ECF0";Disc $a 2.5 2.5 1.2 "FFFFFF";,$a}
function Spectro(){$a=Tp;Rc $a 0 0 12 8 "8A929E";Rc $a 1 1 10 6 "C8D0D8";Rc $a 2 2 5 3 "0A2018";foreach($p in @(@(2,4),@(3,3),@(4,2),@(5,3),@(6,4))){P $a $p[0] $p[1] "3AE08A"};P $a 8 2 "D83A3A";P $a 9 2 "F2C84B";Rc $a 8 4 3 2 "6A727C";,$a}
function LabBalance(){$a=Tp;Rc $a 0 0 8 8 "B4BCC4";Disc $a 3.5 3 2.8 "E8F4FA";Disc $a 3.5 3 2 "C8E4F0";P $a 2 2 "FFFFFF";Rc $a 1 6 6 1 "0A2018";P $a 3 6 "3AE08A";P $a 4 6 "3AE08A";RoundC $a 8 8;,$a}
function Trefoil($a,$cx,$cy,$c){Disc $a $cx $cy 1.0 $c;for($i=0;$i -lt 3;$i++){$an=1.5708+$i*2.0944;Disc $a ($cx+2.8*[Math]::Cos($an)) ($cy-2.8*[Math]::Sin($an)) 1.7 $c};Disc $a $cx $cy 0.8 "F2C020"}
function HazardSign($bg,$fg){$a=Tp;Rc $a 1 1 14 14 "2A2E36";Rc $a 2 2 12 12 $bg;Trefoil $a 8 8 $fg;Disc $a 7.5 7.5 0.8 $bg;,$a}
$T=[ordered]@{}
# ---- TERRES
$T["Terres/Fusta_taulons"]={param($r)Planks2 $r @("B07F4C","A57444","9A6A3E") "8A5C34" "5E3C20" "CFA56C" $false}
$T["Terres/Fusta_fosca"]={param($r)Planks2 $r @("70492A","66412A","5B3A22") "4C2F1A" "33200F" "8C6038" $true}
$T["Terres/Fusta_clara"]={param($r)Planks2 $r @("E0BF84","D6B478","CCA96D") "B08E52" "8E6C3E" "F0D9A8" $false}
$T["Terres/Fusta_vella"]={param($r)Planks2 $r @("8E7A66","83705E","77665A") "5F5044" "3A3028" "A8937C" $true}
$T["Terres/Parquet"]={param($r)$a=Mk "B07A44"
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$h=((([int][Math]::Floor($x/8))+([int][Math]::Floor($y/8)))%2) -eq 0;$lx=$x%8;$ly=$y%8
  if($h){if($ly%2 -eq 1){P $a $x $y "6F4524"}elseif($lx -eq 7){P $a $x $y "6F4524"}else{P $a $x $y $(if($ly -lt 2){"C48C52"}else{"B07A44"})}}
  else{if($lx%2 -eq 1){P $a $x $y "6F4524"}elseif($ly -eq 7){P $a $x $y "6F4524"}else{P $a $x $y $(if($lx -lt 2){"C48C52"}else{"B07A44"})}}}};,$a}
$T["Terres/Escacs"]={param($r)$a=Mk "EDE6D6"
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$d=((([int][Math]::Floor($x/8))+([int][Math]::Floor($y/8)))%2) -eq 0;$e=($x%8 -eq 0 -or $y%8 -eq 0)
  P $a $x $y $(if($d){if($e){"4A4A55"}else{"2E2E38"}}else{if($e){"FFFFFF"}else{"EDE6D6"}})}};,$a}
$T["Terres/Rajola_blanca"]={param($r)TileGrid $r "E8ECEF" "A9B1B8" "FFFFFF" @("D7DDE2","F4F6F8")}
$T["Terres/Rajola_supermercat"]={param($r)$a=TileGrid $r "DDE3E6" "9AA3AA" "FFFFFF" @("CBD3D8","EEF2F4");P $a 3 3 "FFFFFF";P $a 4 2 "FFFFFF";P $a 11 12 "FFFFFF";,$a}
$T["Terres/Rajola_franja_groga"]={param($r)$a=TileGrid $r "DDE3E6" "9AA3AA" "FFFFFF" @("CBD3D8");Rc $a 0 5 16 3 "E6B422";Rc $a 0 5 16 1 "F5D45A";Rc $a 0 7 16 1 "A67C0F";,$a}
$T["Terres/Rajola_blava"]={param($r)TileGrid $r "3F6FA3" "1F3F66" "6FA0D0" @("356090","4C80B8")}
$T["Terres/Rajola_negra"]={param($r)TileGrid $r "33343C" "14151A" "55565F" @("2A2B32","44454E")}
$T["Terres/Rajola_bany"]={param($r)$a=Mk "E9F3F6";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($x%4 -eq 3 -or $y%4 -eq 3){P $a $x $y "B3C9D0"}elseif($x%4 -eq 0 -and $y%4 -eq 0){P $a $x $y "FFFFFF"}}};Speck $a $r 8 @("D9E8EC");,$a}
$T["Terres/Catifa_vermella"]={param($r)$a=Mk "9E2430";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if(($x+$y)%2 -eq 0){P $a $x $y "8E1F2A"}}}
 Rc $a 0 0 16 1 "D4A33A";Rc $a 0 15 16 1 "D4A33A";Rc $a 0 2 16 1 "6E1520";Rc $a 0 13 16 1 "6E1520";P $a 8 7 "D4A33A";P $a 7 8 "D4A33A";P $a 8 8 "E8C060";P $a 9 8 "D4A33A";P $a 8 9 "D4A33A";,$a}
$T["Terres/Catifa_gris"]={param($r)$a=Mk "7A7D86";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if(($x+$y)%2 -eq 0){P $a $x $y "70737C"}}};Speck $a $r 14 @("868993","666972");,$a}
$T["Terres/Terra"]={param($r)$a=Mk "7A5233";Speck $a $r 36 @("6A4529","8C6240","5A3A22");Speck $a $r 4 @("8E8E8E","A6A6A6");,$a}
$T["Terres/Sorra"]={param($r)$a=Mk "E0C98C";Speck $a $r 34 @("D2B97A","ECD9A2","C8AE6C");for($i=0;$i -lt 3;$i++){$x=$r.Next(12);$y=$r.Next(16);Rc $a $x $y 3 1 "EFDDA9"};,$a}
$T["Terres/Neu_1"]={param($r)$a=Mk "F2F7FC";for($i=0;$i -lt 5;$i++){$x=$r.Next(16);$y=$r.Next(16);Rc $a $x $y 3 2 "E3EDF7";P $a ($x+1) ($y+2) "D2E1F0"}
 for($i=0;$i -lt 6;$i++){P $a $r.Next(16) $r.Next(16) "FFFFFF"};for($i=0;$i -lt 4;$i++){P $a $r.Next(16) $r.Next(16) "C4D8EC"};,$a}
$T["Terres/Neu_2"]={param($r)$a=Mk "EAF2FA";for($row=0;$row -lt 3;$row++){$y=2+$row*5;$x0=$r.Next(16);for($i=0;$i -lt 7;$i++){P $a (($x0+$i)%16) $y "F9FCFF";P $a (($x0+$i+1)%16) ($y+1) "D3E2F1"}}
 for($i=0;$i -lt 5;$i++){P $a $r.Next(16) $r.Next(16) "FFFFFF"};for($i=0;$i -lt 3;$i++){P $a $r.Next(16) $r.Next(16) "BCD2E8"};,$a}
$T["Terres/Neu_pisada"]={param($r)$a=Mk "F0F6FB";Speck $a $r 8 @("FFFFFF","E1ECF6");Rc $a 4 3 3 4 "CFE0F0";Rc $a 5 8 2 1 "CFE0F0";Rc $a 10 8 3 4 "CFE0F0";Rc $a 11 13 2 1 "CFE0F0";P $a 4 3 "E1ECF6";P $a 10 8 "E1ECF6";,$a}
$T["Terres/Grava"]={param($r)$a=Mk "8A8A8A";Speck $a $r 60 @("757575","9C9C9C","666666","B0B0B0");for($i=0;$i -lt 6;$i++){$x=$r.Next(15);$y=$r.Next(15);Rc $a $x $y 2 2 "A2A2A2";P $a $x $y "BDBDBD";P $a ($x+1) ($y+1) "6A6A6A"};,$a}
$T["Terres/Marbre"]={param($r)$a=TileGrid $r "EFEFF2" "B9BAC4" "FFFFFF" @("E2E2E8");for($i=0;$i -lt 2;$i++){$x=$r.Next(2,6);for($y=0;$y -lt 16;$y++){$x+=$r.Next(-1,2)+1;P $a ($x%16) $y "9A9CB0"}};,$a}
$T["Terres/Formigo"]={param($r)$a=Mk "919396";for($i=0;$i -lt 9;$i++){$x=$r.Next(15);$y=$r.Next(15);Rc $a $x $y 2 2 $(if($i%2){"999B9E"}else{"898B8E"})};Speck $a $r 12 @("7E8083","A3A5A8");for($i=0;$i -lt 5;$i++){P $a $r.Next(16) $r.Next(16) "63656A"};,$a}
$T["Terres/Formigo_fosc"]={param($r)$a=Mk "5F6166";for($i=0;$i -lt 9;$i++){$x=$r.Next(15);$y=$r.Next(15);Rc $a $x $y 2 2 $(if($i%2){"676970"}else{"56585D"})};Speck $a $r 12 @("4C4E53","727479");for($i=0;$i -lt 5;$i++){P $a $r.Next(16) $r.Next(16) "36383C"};,$a}
$T["Terres/Asfalt"]={param($r)$a=Mk "4A4C52";Speck $a $r 40 @("3E4046","585A60","44464C");Speck $a $r 7 @("72747B");,$a}
$T["Terres/Asfalt_linia"]={param($r)$a=Mk "4A4C52";Speck $a $r 40 @("3E4046","585A60","44464C");Rc $a 0 6 16 1 "E6B422";Rc $a 0 9 16 1 "E6B422";Speck $a $r 4 @("B38A14");,$a}
$T["Terres/Passos_vianants"]={param($r)$a=Mk "4A4C52";Speck $a $r 36 @("3E4046","585A60");for($x=0;$x -lt 16;$x++){if($x%8 -lt 5){for($y=0;$y -lt 16;$y++){P $a $x $y $(if($r.Next(7) -eq 0){"D6D8DC"}else{"F0F1F3"})}}};,$a}
$T["Terres/Vorera"]={param($r)$a=Mk "A9ABB0";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$q=([int][Math]::Floor($x/8))+2*([int][Math]::Floor($y/8));$tone=@("B2B4B9","A7A9AE","B8BABF","ABADB2")[$q];if($x%8 -eq 7 -or $y%8 -eq 7){P $a $x $y "7C7E84"}else{P $a $x $y $tone}}};Speck $a $r 14 @("9A9CA1","C2C4C8");,$a}
$T["Terres/Pissarra"]={param($r)$a=Mk "3C424B";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($y%4 -eq 0){P $a $x $y "565D69"}elseif($y%4 -eq 3){P $a $x $y "2A2E35"}}};for($i=0;$i -lt 8;$i++){Rc $a $r.Next(14) ($r.Next(4)*4+1) 3 1 "4A515C"};for($i=0;$i -lt 4;$i++){$x=$r.Next(16);Rc $a $x ($r.Next(4)*4) 1 4 "20232A"};,$a}
# ---- METALL (ferro antilliscant)
$T["Metall/Metall_placa"]={param($r)$a=Mk "6C7A88";Rc $a 0 0 16 1 "9AA9B8";Rc $a 0 0 1 16 "9AA9B8";Rc $a 15 0 1 16 "3E4955";Rc $a 0 15 16 1 "3E4955";foreach($p in @(@(2,2),@(13,2),@(2,13),@(13,13))){P $a $p[0] $p[1] "C9D5E0";P $a ($p[0]+1) ($p[1]+1) "2F3944"};Speck $a $r 14 @("7B8998","5F6C79");Rc $a 5 8 5 1 "8A98A6";,$a}
$T["Metall/Reixa_metall"]={param($r)$a=Mk "1B1F25";for($y=0;$y -lt 16;$y+=4){for($x=0;$x -lt 16;$x+=4){Rc $a ($x+1) ($y+1) 3 3 "59636F";P $a ($x+1) ($y+1) "8794A3"}};,$a}
$T["Metall/Antilliscant_gris"]={param($r)Diamond $r "8C919A" @("868B94","9297A0") "DADFE7" "B4BAC4" "4E535B"}
$T["Metall/Antilliscant_fosc"]={param($r)Diamond $r "3A3F47" @("353A42","40454D") "9AA3B0" "6C7480" "16191E"}
$T["Metall/Antilliscant_rovellat"]={param($r)Diamond $r "8A5A3A" @("825233","926238","6F4A30") "D89A62" "B07444" "4A2A18"}
$T["Metall/Antilliscant_blau"]={param($r)Diamond $r "5A7898" @("547290","6280A0") "C2DEF4" "8CB0D0" "26394F"}
$T["Metall/Antilliscant_daurat"]={param($r)Diamond $r "A8893A" @("9E8032","B0913E") "F2DE8A" "D2B85C" "5A4818"}
# ---- PARETS
$T["Parets/Paret_casa"]={param($r)$a=Mk "E6D8BC";Speck $a $r 22 @("DACAAB","EFE4CD");Rc $a 0 15 16 1 "C9B791";,$a}
$T["Parets/Paret_blava"]={param($r)$a=Mk "6F95BC";Speck $a $r 22 @("648AB2","7BA1C8");Rc $a 0 15 16 1 "4C7099";,$a}
$T["Parets/Totxo_blanc"]={param($r)Bricks $r "EFEDE6" "CFCBC0" "FFFFFF" "DAD7CD"}
$T["Parets/Totxo_vermell"]={param($r)Bricks $r "B5503A" "6A3A2E" "D26C50" "8C3E2C"}
$T["Parets/Paret_fusta_vertical"]={param($r)Tr (Planks2 $r @("A9784A","9C6D41","B07F4C") "8A5C34" "5E3C20" "CFA56C" $false)}
$T["Parets/Paret_roig_fosc"]={param($r)Tr (Planks2 $r @("7A2E2E","6E2828","843434") "5C2020" "3E1414" "964040" $false)}
$T["Parets/Vidre"]={param($r)$a=Mk "BFE9F244";Rc $a 0 0 16 1 "4B6C78AA";Rc $a 0 15 16 1 "4B6C78AA";Rc $a 0 0 1 16 "4B6C78AA";Rc $a 15 0 1 16 "4B6C78AA"
 for($i=0;$i -lt 6;$i++){P $a (3+$i) (10-$i) "FFFFFFD0"};for($i=0;$i -lt 3;$i++){P $a (9+$i) (12-$i) "FFFFFF90"};P $a 1 1 "FFFFFFAA";,$a}
$T["Parets/Vidre_fosc"]={param($r)$a=Mk "2A4A5C55";Rc $a 0 0 16 1 "10202AC8";Rc $a 0 15 16 1 "10202AC8";Rc $a 0 0 1 16 "10202AC8";Rc $a 15 0 1 16 "10202AC8"
 for($i=0;$i -lt 6;$i++){P $a (3+$i) (10-$i) "9CC8DCB0"};for($i=0;$i -lt 3;$i++){P $a (9+$i) (12-$i) "9CC8DC70"};,$a}
$T["Parets/Castell_blocs"]={param($r)$a=Mk "9A9CA3"
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$off=if([int][Math]::Floor($y/8) -eq 1){4}else{0};$lx=($x+$off)%8;$ly=$y%8
  if($lx -eq 7 -or $ly -eq 7){P $a $x $y "4A4C55"}elseif($lx -eq 0 -or $ly -eq 0){P $a $x $y "BFC1C8"}elseif($lx -eq 6 -or $ly -eq 6){P $a $x $y "83858D"}}}
 Speck $a $r 12 @("8E9097","A6A8AF");,$a}
$T["Parets/Castell_fosc"]={param($r)$a=Mk "5E6068"
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$off=if([int][Math]::Floor($y/8) -eq 1){4}else{0};$lx=($x+$off)%8;$ly=$y%8
  if($lx -eq 7 -or $ly -eq 7){P $a $x $y "26272D"}elseif($lx -eq 0 -or $ly -eq 0){P $a $x $y "7C7E87"}elseif($lx -eq 6 -or $ly -eq 6){P $a $x $y "4A4C54"}}}
 Speck $a $r 12 @("52545C","6A6C75");,$a}
$T["Parets/Teulada_vermella"]={param($r)$a=Mk "B5503A"
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$off=if([int][Math]::Floor($y/4)%2 -eq 1){4}else{0};$lx=($x+$off)%8;$ly=$y%4
  if($ly -eq 3){P $a $x $y "5E2417"}elseif($lx -eq 0 -or $lx -eq 7){P $a $x $y "7C3322"}elseif($ly -eq 0){P $a $x $y "D26C50"}}};,$a}
$T["Parets/Teulada_blava"]={param($r)$a=Mk "3F5E8C"
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$off=if([int][Math]::Floor($y/4)%2 -eq 1){4}else{0};$lx=($x+$off)%8;$ly=$y%4
  if($ly -eq 3){P $a $x $y "1C2E4A"}elseif($lx -eq 0 -or $lx -eq 7){P $a $x $y "2C4468"}elseif($ly -eq 0){P $a $x $y "6485B8"}}};,$a}
$T["Parets/Palla"]={param($r)$a=Mk "CDA94A";for($i=0;$i -lt 30;$i++){$x=$r.Next(16);$y=$r.Next(16);$l=$r.Next(3,6);$sl=$r.Next(-1,2);$c=@("E8CC72","DDBA58","B58F34","F0D888")[$r.Next(4)];for($k=0;$k -lt $l;$k++){P $a (($x+$k)%16) (($y+[int][Math]::Floor($k*$sl/3.0)+16)%16) $c}};for($i=0;$i -lt 6;$i++){P $a $r.Next(16) $r.Next(16) "8E6F26"};,$a}
$T["Parets/Bala_palla"]={param($r)$a=Mk "CDA94A";for($i=0;$i -lt 26;$i++){$x=$r.Next(16);$y=$r.Next(16);$l=$r.Next(3,6);$c=@("E8CC72","DDBA58","B58F34")[$r.Next(3)];for($k=0;$k -lt $l;$k++){P $a (($x+$k)%16) $y $c}}
 foreach($yy in 4,11){Rc $a 0 $yy 16 1 "6A4A22";Rc $a 0 ($yy+1) 16 1 "8E6A32"};Rc $a 0 0 16 1 "A88532";Rc $a 0 15 16 1 "8E6F26";,$a}
# ---- NATURA
$T["Natura/Gespa_1"]={param($r)GrassBase $r "56A54F" "4C9745" "63B359" $null}
$T["Natura/Gespa_alta"]={param($r)$a=GrassBase $r "4A9442" "3F8239" "58A64E" $null;for($i=0;$i -lt 12;$i++){$x=$r.Next(16);$y=$r.Next(1,16);P $a $x $y "2F6A2C";P $a $x ($y-1) "6CC25E"};,$a}
$T["Natura/Gespa_seca"]={param($r)$a=GrassBase $r "A6A04A" "948E3E" "B8B25A" $null;Speck $a $r 6 @("7C7632");,$a}
$T["Natura/Gespa_fosca"]={param($r)GrassBase $r "3F6030" "36542A" "4A6C33" $null}
$T["Natura/Aigua_fosca"]={param($r)$a=Mk "3A62B4";Speck $a $r 12 @("557FD2");,$a}
$T["Natura/Aigua_profunda"]={param($r)$a=Mk "223F88";Speck $a $r 10 @("3558AA");,$a}
$T["Natura/Aigua_nit"]={param($r)$a=Mk "162A5C";Speck $a $r 9 @("27468C");,$a}
$T["Natura/Gel_net"]={param($r)$a=Mk "BDE4F2";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$s=($x+$y)%16;if($s -ge 3 -and $s -le 5){P $a $x $y "D8F2FA"}elseif($s -ge 10 -and $s -le 11){P $a $x $y "9FD0E4"}}};Speck $a $r 5 @("FFFFFF");P $a 4 4 "FFFFFF";P $a 5 3 "FFFFFF";,$a}
$T["Natura/Gel_esquerdat"]={param($r)$a=Mk "B2DCEC";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$s=($x+$y)%16;if($s -ge 3 -and $s -le 4){P $a $x $y "CFEEF8"}elseif($s -ge 10 -and $s -le 11){P $a $x $y "96C8DE"}}};$x=1;for($y=0;$y -lt 16;$y++){$x=($x+$r.Next(0,2)+0)%16;P $a $x $y "F4FCFF";if($y%3 -eq 0){P $a (($x+1)%16) $y "7FB4CE"}};$x=9;for($y=0;$y -lt 16;$y+=1){$x=($x+$r.Next(-1,1)+16)%16;if($y -gt 6){P $a $x $y "EAF8FF"}};,$a}
$T["Natura/Gel_glacera"]={param($r)$a=Mk "6FA8CC";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$s=($x*2+$y)%16;if($s -le 2){P $a $x $y "92C6E2"}elseif($s -ge 11 -and $s -le 12){P $a $x $y "4E86AE"}}};Speck $a $r 6 @("E4F6FF","FFFFFF");,$a}
$T["Natura/Fulles_verd"]={param($r)$a=Mk "1C4216";for($i=0;$i -lt 16;$i++){$x=$r.Next(16);$y=$r.Next(16);P $a $x $y "3F8A34";P $a (($x+1)%16) $y "3F8A34";P $a $x (($y+1)%16) "2F6A27";P $a (($x+1)%16) (($y+1)%16) "2F6A27";P $a (($x-1+16)%16) (($y-1+16)%16) "6CC25E";P $a $x (($y-1+16)%16) "56A94A"};for($i=0;$i -lt 8;$i++){P $a $r.Next(16) $r.Next(16) "123010"};,$a}
$T["Natura/Fulles_tardor"]={param($r)$a=Mk "4A1E0E";for($i=0;$i -lt 16;$i++){$x=$r.Next(16);$y=$r.Next(16);$c=if($i%2){"D2701E"}else{"B8461A"};P $a $x $y $c;P $a (($x+1)%16) $y $c;P $a $x (($y+1)%16) "8E3416";P $a (($x+1)%16) (($y+1)%16) "8E3416";P $a (($x-1+16)%16) (($y-1+16)%16) "F2B04A";P $a $x (($y-1+16)%16) "E08A32"};for($i=0;$i -lt 8;$i++){P $a $r.Next(16) $r.Next(16) "331408"};,$a}
$T["Natura/Fulles_ceri"]={param($r)$a=Mk "1C4216";for($i=0;$i -lt 14;$i++){$x=$r.Next(16);$y=$r.Next(16);P $a $x $y "3F8A34";P $a (($x+1)%16) $y "3F8A34";P $a $x (($y+1)%16) "2F6A27";P $a (($x-1+16)%16) (($y-1+16)%16) "6CC25E"};for($i=0;$i -lt 10;$i++){$x=$r.Next(16);$y=$r.Next(16);P $a $x $y "F4A6C0";P $a (($x+1)%16) $y "E27AA0";P $a $x (($y+1)%16) "FBD2E0"};,$a}
$T["Natura/Molsa"]={param($r)$a=Mk "5F7F3A";Speck $a $r 70 @("708F46","4F6C2E","83A557","3E5724");,$a}
$T["Natura/Roca_cova_1"]={param($r)Rock $r @("5C5E69","54565F","64666F") "26272E" 3}
$T["Natura/Roca_cova_marro"]={param($r)Rock $r @("7A6244","705A3E","86694A") "2E2014" 3}
$T["Natura/Roca_cova_gran"]={param($r)Rock $r @("626470","5A5C68","6A6C78") "24252B" 2}
$T["Natura/Roca_cova_blava"]={param($r)Rock $r @("46506C","3E4862","4E5876") "181C2C" 3}
$T["Natura/Roca_cova_molsa"]={param($r)$a=Rock $r @("5C5E69","54565F","64666F") "26272E" 3;for($i=0;$i -lt 18;$i++){$x=$r.Next(16);$y=$r.Next(16);PW $a $x $y @("5F7F3A","4F6C2E","83A557")[$r.Next(3)];if($r.Next(2)){PW $a ($x+1) $y "4F6C2E"}};,$a}
$T["Natura/Roca_cova_magma"]={param($r)$a=Rock $r @("34303A","2E2A32","3A3640") "FF7A1E" 3;for($i=0;$i -lt 256;$i++){if($a[$i] -eq "FF7A1E" -and $r.Next(3) -eq 0){$a[$i]="FFC04A"}};,$a}
$T["Natura/Mineral_or"]={param($r)Mineral $r @("5C5E69","54565F","64666F") "26272E" "FFF0A0" "E6B422" "8A6410" 4}
$T["Natura/Mineral_ferro"]={param($r)Mineral $r @("5C5E69","54565F","64666F") "26272E" "EAC9B0" "C0987A" "7A5238" 4}
$T["Natura/Mineral_coure"]={param($r)Mineral $r @("5C5E69","54565F","64666F") "26272E" "FFC48A" "E0782E" "8A3A14" 4}
$T["Natura/Mineral_diamant"]={param($r)Mineral $r @("46506C","3E4862","4E5876") "181C2C" "E4FCFF" "6FDCF4" "2A8AB0" 4}
$T["Natura/Mineral_ametista"]={param($r)Mineral $r @("46506C","3E4862","4E5876") "181C2C" "F2D0FF" "B070E0" "5A2C8A" 4}
$T["Natura/Mineral_robi"]={param($r)Mineral $r @("5C5E69","54565F","64666F") "26272E" "FFB0BC" "E03050" "7A1228" 3}
$T["Natura/Mineral_maragda"]={param($r)Mineral $r @("52605E","4A5856","5A6866") "1C2624" "C4FFD4" "3CC870" "166030" 4}
$T["Natura/Mineral_carbo"]={param($r)Mineral $r @("5C5E69","54565F","64666F") "26272E" "B4B6C4" "5A5C6A" "1A1A22" 5}
$T["Natura/Gespa_flors_margarides"]={param($r)Flowers $r 0 @(@("F6F4EC","F2C84B","plus"),@("F6F4EC","F2C84B","plus"),@("F6F4EC","F2C84B","plus"),@("F6F4EC","","dot"),@("F6F4EC","","dot"))}
$T["Natura/Gespa_flors_grogues"]={param($r)Flowers $r 0 @(@("F2C84B","","dot"),@("F2C84B","","dot"),@("F8DC6A","","dot"),@("F2C84B","F8F0C0","two"),@("F8DC6A","","dot"),@("F2C84B","","dot"))}
$T["Natura/Gespa_flors_roses"]={param($r)Flowers $r 0 @(@("EE7BA2","F8D44A","plus"),@("EE7BA2","","dot"),@("F6B0C8","","dot"),@("EE7BA2","F6B0C8","two"),@("F6B0C8","","dot"))}
$T["Natura/Gespa_flors_roselles"]={param($r)Flowers $r 0 @(@("D83A3A","1A1A1A","plus"),@("D83A3A","","dot"),@("E8584A","","dot"),@("D83A3A","1A1A1A","plus"))}
$T["Natura/Gespa_flors_liles"]={param($r)Flowers $r 0 @(@("A878E0","F2C84B","plus"),@("A878E0","","dot"),@("C8A8F0","","dot"),@("7A52C0","","dot"),@("A878E0","C8A8F0","two"))}
$T["Natura/Gespa_flors_blaves"]={param($r)Flowers $r 0 @(@("4A7AE0","F4F0E0","plus"),@("4A7AE0","","dot"),@("7AA2F0","","dot"),@("4A7AE0","7AA2F0","two"),@("7AA2F0","","dot"))}
$T["Natura/Gespa_lavanda"]={param($r)Flowers $r 0 @(@("9A6AD8","","stalk"),@("9A6AD8","","stalk"),@("B48AE8","","stalk"),@("9A6AD8","","stalk"))}
$T["Natura/Gespa_trevols"]={param($r)Flowers $r 0 @(@("","","clover"),@("","","clover"),@("","","clover"),@("","","clover"),@("","","clover"),@("F6F4EC","","dot"))}
$T["Natura/Gespa_flors_mixtes"]={param($r)Flowers $r 0 @(@("F6F4EC","F2C84B","plus"),@("EE7BA2","","dot"),@("F2C84B","","dot"),@("A878E0","","dot"),@("D83A3A","","dot"),@("4A7AE0","","dot"),@("F6F4EC","","dot"),@("F2C84B","F8F0C0","two"),@("EE7BA2","F8D44A","plus"),@("A878E0","","dot"))}
$T["Natura/Gespa_cami_recte"]={param($r)PathTile $r "LR"}
$T["Natura/Gespa_cami_canto"]={param($r)PathTile $r "RD"}
$T["Natura/Gespa_cami_T"]={param($r)PathTile $r "LRD"}
$T["Natura/Gespa_cami_creu"]={param($r)PathTile $r "LRUD"}
$T["Natura/Gespa_cami_final"]={param($r)PathTile $r "R"}
$T["Metall/Barres_gris"]={param($r)Bars $r "5A606A" @("545A64","60666F") "E4E9F0" "AEB6C2" "20242A"}
$T["Metall/Barres_fosc"]={param($r)Bars $r "30343C" @("2C3038","363A42") "B4BDC9" "7A8390" "0E1014"}
$T["Metall/Barres_blau"]={param($r)Bars $r "3C5472" @("38506E","425A78") "CCE4F8" "8CB0D4" "142034"}
$T["Natura/Gel_esquerdat_estrella"]={param($r)$a=GelBase "B2DCEC" "CFEEF8" "96C8DE";foreach($d in @(@(1,0),@(-1,0),@(0,1),@(0,-1),@(1,1),@(-1,-1),@(1,-1),@(-1,1))){Crack $a $r 8 8 $d[0] $d[1] ($r.Next(5,8)) "F4FCFF" "7FB4CE"};Disc $a 8 8 1 "FFFFFF";,$a}
$T["Natura/Gel_esquerdat_llarga"]={param($r)$a=GelBase "BDE4F2" "D8F2FA" "9FD0E4";Crack $a $r 0 3 1 1 14 "F4FCFF" "7FB4CE";Crack $a $r 6 8 1 -1 6 "F4FCFF" "7FB4CE";Crack $a $r 10 12 -1 1 4 "F4FCFF" "7FB4CE";,$a}
$T["Natura/Gel_esquerdat_xarxa"]={param($r)$a=GelBase "B2DCEC" "CFEEF8" "96C8DE";for($i=0;$i -lt 9;$i++){$dd=@(@(1,0),@(0,1),@(1,1),@(1,-1))[$r.Next(4)];Crack $a $r $r.Next(16) $r.Next(16) $dd[0] $dd[1] ($r.Next(3,6)) "EAF8FF" "86BAD2"};,$a}
$T["Natura/Gel_esquerdat_forat"]={param($r)$a=GelBase "BDE4F2" "D8F2FA" "9FD0E4";foreach($d in @(@(1,0),@(-1,0),@(0,1),@(0,-1),@(1,1),@(-1,-1))){Crack $a $r 8 8 $d[0] $d[1] ($r.Next(4,7)) "F4FCFF" "7FB4CE"};Disc $a 8 8 3.2 "DFF4FC";Disc $a 8 8 2.4 "1B3F78";Disc $a 8 8 1.2 "152F5E";P $a 7 7 "3A6EB0";,$a}
$T["Natura/Gel_esquerdat_fosc"]={param($r)$a=GelBase "6FA8CC" "92C6E2" "4E86AE";for($i=0;$i -lt 5;$i++){$dd=@(@(1,0),@(0,1),@(1,1),@(1,-1))[$r.Next(4)];Crack $a $r $r.Next(16) $r.Next(16) $dd[0] $dd[1] ($r.Next(4,8)) "E4F6FF" "2E5E84"};,$a}
$T["Natura/Gel_bombolles"]={param($r)$a=GelBase "BDE4F2" "D8F2FA" "9FD0E4";foreach($p in @(@(3,3),@(10,5),@(6,10),@(12,12),@(2,12))){Rc $a $p[0] $p[1] 2 2 "EAF8FF";P $a $p[0] $p[1] "FFFFFF";P $a ($p[0]+1) ($p[1]+1) "8CC0D8"};Crack $a $r 9 0 0 1 9 "F4FCFF" "7FB4CE";,$a}
$T["Natura/Gel_trencat"]={param($r)$a=GelBase "B2DCEC" "CFEEF8" "96C8DE";for($y=9;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($x+($y-9)*2 -gt 4 -and $x -lt 11+($y-9)){P $a $x $y "2A5AA0"}}};for($i=0;$i -lt 8;$i++){Crack $a $r $r.Next(16) $r.Next(9) 1 1 4 "F4FCFF" "7FB4CE"};,$a}
# ---- MODERNS (sols i parets)
$T["Moderns/Moqueta_oficina"]={param($r)$a=Mk "5A6270";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if(($x+$y)%2 -eq 0){P $a $x $y "535B68"}}};Speck $a $r 14 @("636B79","4C5461");,$a}
$T["Moderns/Laminat_gris"]={param($r)Planks2 $r @("B8B4AE","AEAAA4","A4A09A") "8C8882" "6E6A64" "CECAC4" $false}
$T["Moderns/Terrazzo"]={param($r)$a=Mk "E6E2D8";Speck $a $r 8 @("DAD6CC");for($i=0;$i -lt 16;$i++){$x=$r.Next(15);$y=$r.Next(15);$c=@("C98A8A","8A9CA8","6A6A70","B8A078","7AA898")[$r.Next(5)];P $a $x $y $c;if($r.Next(2)){P $a ($x+1) $y $c}};,$a}
$T["Moderns/Linoleum_verd"]={param($r)$a=Mk "8EC08A";Speck $a $r 12 @("9ACC96","82B47E");for($i=0;$i -lt 8;$i++){$x=$r.Next(13);Rc $a $x $r.Next(16) 3 1 "A6D6A2"};,$a}
$T["Moderns/Rajola_cuina"]={param($r)$a=Mk "F2F0EA";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$q=(([int][Math]::Floor($x/4))+([int][Math]::Floor($y/4)))%2;if($q -eq 0){P $a $x $y "C83A3A"};if($x%4 -eq 0 -or $y%4 -eq 0){P $a $x $y $(if($q -eq 0){"E85A5A"}else{"FFFFFF"})}}};,$a}
$T["Moderns/Formigo_pulit"]={param($r)$a=Mk "B0B2B6";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$s=($x+$y)%16;if($s -ge 5 -and $s -le 8){P $a $x $y "BABCC0"}}};Speck $a $r 8 @("A4A6AA");,$a}
$T["Moderns/Rajola_hospital"]={param($r)TileGrid $r "E2EFF0" "B6CED2" "F8FFFF" @("D4E6E8")}
$T["Moderns/Paret_oficina"]={param($r)$a=Mk "CFD3D8";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($x%8 -eq 7){P $a $x $y "A8ADB4"}elseif($x%8 -eq 0){P $a $x $y "DDE0E4"}}};Rc $a 0 13 16 3 "6A707A";Rc $a 0 13 16 1 "8A909A";,$a}
$T["Moderns/Paret_supermercat"]={param($r)$a=Mk "F0E8D0";Speck $a $r 10 @("E6DEC4");Rc $a 0 9 16 3 "3A9A58";Rc $a 0 9 16 1 "5AC078";Rc $a 0 12 16 1 "2A7A40";Rc $a 0 13 16 3 "D8D0B8";,$a}
$T["Moderns/Paret_hospital"]={param($r)$a=Mk "BEE0D8";Speck $a $r 10 @("B2D4CC");Rc $a 0 8 16 2 "FFFFFF";Rc $a 0 10 16 1 "8CBCB0";Rc $a 0 13 16 3 "9ECAC0";,$a}
$T["Moderns/Paret_cortina_vidre"]={param($r)$a=Mk "78B4D4";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($x%8 -eq 0 -or $y%8 -eq 0){P $a $x $y "3A5468"}elseif($x%8 -eq 1 -or $y%8 -eq 1){P $a $x $y "A4D4EC"}}};for($i=0;$i -lt 5;$i++){P $a (2+$i) (6-$i) "E4F6FF"};,$a}
# ---- OBJECTES (blocs no complets, fons transparent)
$T["Objectes/Estanteria_begudes"]={param($r)Shelf $r @("D83A3A","3A7AD8","3AB04A","F2C84B","F2F2F2")}
$T["Objectes/Estanteria_llaunes"]={param($r)Shelf $r @("C8C8D0","D83A3A","F2C84B","3A7AD8","8A5AC8")}
$T["Objectes/Estanteria_pa"]={param($r)Shelf $r @("C8904A","E0B070","A06A30","F0D8A0")}
$T["Objectes/Estanteria_fruita"]={param($r)Shelf $r @("D83A3A","F29A2E","7AC040","F2D84B","8A3A6A")}
$T["Objectes/Estanteria_neteja"]={param($r)Shelf $r @("40C8E8","F258A8","8AE040","F2F2F2","FFB020")}
$T["Objectes/Estanteria_snacks"]={param($r)Shelf $r @("F2584A","FFC820","3AA0F0","FF8A30","40B060")}
$T["Objectes/Nevera_super"]={param($r)$a=Tp;Rc $a 1 0 14 15 "DCE6EC";Rc $a 2 1 12 11 "8EC4DC";Rc $a 2 1 12 1 "FFFFFF";Rc $a 2 4 12 1 "E4F4FA";Rc $a 2 7 12 1 "E4F4FA";foreach($y in 3,6,9){for($x=3;$x -lt 12;$x+=2){P $a $x $y @("D83A3A","F2C84B","3AB04A","3A7AD8")[$r.Next(4)];P $a ($x+1) $y "FFFFFF"}};Rc $a 12 2 1 9 "2A2E36";Rc $a 2 12 12 3 "9AA8B2";for($x=3;$x -lt 13;$x+=2){Rc $a $x 13 1 1 "6A7884"};Shd $a 1 14 15;,$a}
$T["Objectes/Maquina_vending"]={param($r)$a=Tp;Rc $a 1 0 14 15 "C8283A";Rc $a 1 0 1 15 "E8485A";Rc $a 2 1 9 9 "202830";for($y=2;$y -lt 9;$y+=3){for($x=3;$x -lt 10;$x+=2){P $a $x $y @("F2C84B","3A7AD8","3AB04A","F2F2F2","FF8A30")[$r.Next(5)];P $a $x ($y+1) "5A606C"}};Rc $a 12 2 2 2 "3AE08A";Rc $a 12 5 2 1 "E8E8E8";Rc $a 12 7 2 1 "E8E8E8";Rc $a 12 9 2 1 "E8E8E8";Rc $a 2 11 11 3 "10141A";Rc $a 3 12 5 1 "2A2E36";Shd $a 1 14 15;,$a}
$T["Objectes/Caixa_registradora"]={param($r)$a=Tp;Rc $a 0 10 16 5 "8A5A30";Rc $a 0 9 16 1 "C8905A";Rc $a 3 4 9 5 "3A4048";Rc $a 3 4 9 1 "5A606C";Rc $a 4 2 5 3 "5AE08A";Rc $a 4 2 5 1 "A8F8C8";for($x=4;$x -lt 12;$x+=2){P $a $x 6 "B0B8C0";P $a $x 7 "B0B8C0"};Rc $a 5 11 6 2 "6A4020";P $a 8 12 "C8905A";Shd $a 0 15 15;,$a}
$T["Objectes/Taulell_blanc"]={param($r)$a=Tp;Rc $a 0 5 16 3 "F0ECE4";Rc $a 0 5 16 1 "FFFFFF";Rc $a 0 8 16 6 "B8BCC4";Rc $a 0 8 16 1 "8A909A";for($x=1;$x -lt 16;$x+=5){Rc $a $x 10 3 3 "A8ACB4"};Shd $a 0 15 14;,$a}
$T["Objectes/Taulell_fusta"]={param($r)$a=Tp;Rc $a 0 5 16 3 "C8905A";Rc $a 0 5 16 1 "E0B07A";Rc $a 0 8 16 6 "8A5A30";Rc $a 0 8 16 1 "5E3C20";for($x=1;$x -lt 16;$x+=5){Rc $a $x 10 3 3 "7A4E28"};Shd $a 0 15 14;,$a}
$T["Objectes/Carro_compra"]={param($r)$a=Tp;Rc $a 2 4 12 1 "B8C0C8";Rc $a 3 9 10 1 "B8C0C8";foreach($y in 6,8){Rc $a 3 $y 10 1 "9AA2AA"};foreach($x in 2,5,8,11,13){Rc $a $x 4 1 6 "9AA2AA"};Rc $a 0 2 3 1 "2A2E36";Rc $a 0 2 1 4 "2A2E36";Rc $a 4 10 8 1 "6A707A";Rc $a 4 12 2 2 "14161C";Rc $a 11 12 2 2 "14161C";P $a 5 11 "6A707A";P $a 11 11 "6A707A";Shd $a 3 13 15;,$a}
$T["Objectes/Llum_sostre"]={param($r)$a=Tp;Disc $a 8 9 6 "FFF2A828";Disc $a 8 9 4.5 "FFF2A850";Rc $a 8 0 1 3 "2A2E36";Rc $a 5 3 6 1 "3A3F48";Rc $a 4 4 8 2 "4A505A";Rc $a 4 4 8 1 "6A707A";Disc $a 8 8 2.5 "FFF2A8";Disc $a 8 8 1.3 "FFFFFF";,$a}
$T["Objectes/Llum_peu"]={param($r)$a=Tp;Disc $a 8 8 6 "FFF2A822";Rc $a 5 6 6 1 "FFF2A870";Rc $a 8 7 1 7 "3A3F48";Rc $a 6 14 5 1 "3A3F48";Rc $a 7 13 3 1 "4A505A";Rc $a 5 2 6 1 "F2D898";Rc $a 4 3 8 3 "F8E6B0";Rc $a 4 5 8 1 "D8BC78";Rc $a 5 3 2 1 "FFFFFF";,$a}
$T["Objectes/Llum_paret"]={param($r)$a=Tp;Disc $a 8 8 6 "FFF2A828";Disc $a 8 9 4 "FFF2A850";Rc $a 7 11 2 3 "6A707A";Rc $a 5 3 6 1 "F2D898";Rc $a 5 4 6 3 "F8E6B0";Rc $a 5 6 6 1 "D8BC78";P $a 8 7 "FFFFFF";P $a 8 8 "FFF2A8";,$a}
$T["Objectes/Fanal_carrer"]={param($r)$a=Tp;Disc $a 8 6 7 "FFF2A81C";Disc $a 8 6 5 "FFF2A838";Rc $a 8 5 1 10 "3A3F48";Rc $a 6 14 5 1 "3A3F48";Rc $a 5 2 7 2 "4A505A";Rc $a 5 2 7 1 "6A707A";Rc $a 6 4 5 1 "FFF2A8";P $a 8 4 "FFFFFF";,$a}
$T["Objectes/Neo_cartell"]={param($r)$a=Tp;Disc $a 8 8 7 "FF4AB818";Rc $a 1 4 14 8 "181A22";Rc $a 1 4 14 1 "FF4AB8";Rc $a 1 11 14 1 "FF4AB8";Rc $a 1 4 1 8 "FF4AB8";Rc $a 14 4 1 8 "FF4AB8";Rc $a 3 6 3 1 "4AE8FF";Rc $a 7 6 6 1 "4AE8FF";Rc $a 3 9 9 1 "4AE8FF";,$a}
$T["Objectes/Llit_blau_cap"]={param($r)Bed "4A78C8" "cap"}
$T["Objectes/Llit_blau_peus"]={param($r)Bed "4A78C8" "peus"}
$T["Objectes/Llit_roig_cap"]={param($r)Bed "C8484A" "cap"}
$T["Objectes/Llit_roig_peus"]={param($r)Bed "C8484A" "peus"}
$T["Objectes/Llit_verd_cap"]={param($r)Bed "4AA868" "cap"}
$T["Objectes/Llit_verd_peus"]={param($r)Bed "4AA868" "peus"}
$T["Objectes/Sofa_gris_esq"]={param($r)Sofa "6A727E" "esq"}
$T["Objectes/Sofa_gris_dre"]={param($r)Sofa "6A727E" "dre"}
$T["Objectes/Sofa_roig_esq"]={param($r)Sofa "B03A44" "esq"}
$T["Objectes/Sofa_roig_dre"]={param($r)Sofa "B03A44" "dre"}
$T["Objectes/Taula_fusta"]={param($r)$a=Tp;Rc $a 1 4 14 8 "B8885A";Rc $a 1 4 14 1 "D8A878";Rc $a 1 11 14 2 "8A5E36";Rc $a 3 6 4 3 "F4F4F0";P $a 4 7 "9AA0A8";Rc $a 10 6 2 2 "D83A3A";Shd $a 2 13 13;Rc $a 2 13 2 2 "6A4020";Rc $a 12 13 2 2 "6A4020";,$a}
$T["Objectes/Taula_blanca"]={param($r)$a=Tp;Rc $a 1 4 14 8 "EEF0F2";Rc $a 1 4 14 1 "FFFFFF";Rc $a 1 11 14 2 "B4BAC2";Rc $a 2 13 1 2 "6A707A";Rc $a 13 13 1 2 "6A707A";Rc $a 3 6 5 3 "2A2E36";Rc $a 3 6 5 1 "5A606C";,$a}
$T["Objectes/Cadira_oficina"]={param($r)$a=Tp;Rc $a 4 2 8 2 "2A2E36";Rc $a 5 2 6 1 "4A505A";Disc $a 8 8 3.8 "3A3F48";Disc $a 8 8 2.8 "4A505A";P $a 7 7 "6A707A";foreach($p in @(@(4,13),@(8,14),@(12,13),@(5,11),@(11,11))){P $a $p[0] $p[1] "14161C"};Ln $a 8 8 8 13 "2A2E36";,$a}
$T["Objectes/Monitor"]={param($r)$a=Tp;Rc $a 2 2 12 8 "1A1E26";Rc $a 3 3 10 6 "4A90D8";Rc $a 3 3 10 1 "7AB4F0";for($i=0;$i -lt 4;$i++){P $a (4+$i) (4+$i) "A8D4FF"};Rc $a 7 10 2 2 "6A707A";Rc $a 5 12 6 1 "4A505A";Shd $a 4 11 13;,$a}
$T["Objectes/Planta_test"]={param($r)$a=Tp;Disc $a 8 6 3.3 "3A9A48";Disc $a 5.5 8 2.3 "2E8A3C";Disc $a 10.5 8 2.3 "2E8A3C";Disc $a 8 4.5 1.8 "5ABA64";for($i=0;$i -lt 7;$i++){P $a $r.Next(4,12) $r.Next(3,10) "6ACC74"};Rc $a 4 10 8 1 "C8704A";Rc $a 5 11 6 3 "B8603A";Rc $a 5 11 1 3 "D8805A";Rc $a 5 13 6 1 "8A4426";Shd $a 4 11 15;,$a}
$T["Objectes/Televisio"]={param($r)$a=Tp;Rc $a 1 3 14 9 "14161C";Rc $a 2 4 12 7 "2A3A5A";for($i=0;$i -lt 5;$i++){P $a (3+$i) (5+$i) "4A5E86"};Rc $a 3 5 3 1 "5A70A0";Rc $a 6 12 4 1 "3A3F48";Rc $a 4 13 8 1 "2A2E36";P $a 13 10 "3AE08A";Shd $a 3 12 15;,$a}
$T["Objectes/Armari_metal"]={param($r)$a=Tp;Rc $a 2 0 12 15 "8A929E";Rc $a 2 0 12 1 "B4BCC8";Rc $a 8 1 1 14 "5A626E";foreach($y in 2,3,4){Rc $a 3 $y 4 1 "5A626E";Rc $a 9 $y 4 1 "5A626E"};Rc $a 7 8 1 2 "2A2E36";Rc $a 9 8 1 2 "2A2E36";Rc $a 2 14 12 1 "5A626E";Shd $a 2 13 15;,$a}
$T["Objectes/Paperera"]={param($r)$a=Tp;Rc $a 4 5 8 1 "8A929E";Rc $a 5 6 6 8 "5A606C";for($x=6;$x -lt 11;$x+=2){Rc $a $x 7 1 6 "3E444E"};Rc $a 5 6 1 8 "7A828E";Rc $a 5 13 6 1 "3E444E";Shd $a 4 11 14;,$a}
# ---- TRONCS i naturals
$T["Troncs/Tronc_cara"]={param($r)$a=Mk "5A3A20";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$d=[Math]::Sqrt(($x-7.5)*($x-7.5)+($y-7.5)*($y-7.5));if($d -gt 6.8){P $a $x $y $(if($r.Next(4) -eq 0){"6A4426"}else{"5A3A20"})}elseif($d -gt 6.0){P $a $x $y "3C2412"}elseif($d -lt 1.0){P $a $x $y "8A5A30"}else{P $a $x $y $(if([int][Math]::Floor($d/1.5)%2){"B07F48"}else{"C99A62"})}}};P $a 5 4 "E0B880";,$a}
$T["Troncs/Tronc_escorca"]={param($r)$a=Mk "6A4426";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$c=@("3C2412","6A4426","7A5230","6A4426")[$x%4];P $a $x $y $c}};for($i=0;$i -lt 14;$i++){$x=$r.Next(16);$y=$r.Next(13);Rc $a $x $y 1 $r.Next(2,5) $(if($r.Next(2)){"2E1C0E"}else{"8A6038"})};,$a}
$T["Troncs/Tronc_betula"]={param($r)$a=Mk "ECE9E0";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($x%8 -eq 0){P $a $x $y "C4C0B6"}elseif($x%8 -eq 7){P $a $x $y "D8D4CA"}}};for($i=0;$i -lt 9;$i++){Rc $a $r.Next(13) $r.Next(16) $r.Next(2,4) 1 "2A2A2A"};,$a}
$T["Troncs/Tronc_pi"]={param($r)$a=Mk "7A4A2E";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if(($x+[int][Math]::Floor($y/4))%5 -eq 0){P $a $x $y "4A2A18"}elseif(($x+[int][Math]::Floor($y/4))%5 -eq 2){P $a $x $y "945C3A"}}};for($i=0;$i -lt 8;$i++){P $a $r.Next(16) $r.Next(16) "2E1A0E"};,$a}
$T["Troncs/Soca"]={param($r)$a=Tp;for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$d=[Math]::Sqrt([Math]::Pow(($x-8)/6.0,2)+[Math]::Pow(($y-7)/4.5,2));$ds=[Math]::Sqrt([Math]::Pow(($x-8)/6.0,2)+[Math]::Pow(($y-10)/4.5,2))
 if($d -le 1.0){if($d -gt 0.86){P $a $x $y "5A3A20"}elseif($d -lt 0.2){P $a $x $y "8A5A30"}else{P $a $x $y $(if([int][Math]::Floor($d*4)%2){"B07F48"}else{"C99A62"})}}elseif($ds -le 1.0){P $a $x $y $(if($x%3 -eq 0){"3C2412"}else{"6A4426"})}}};Shd $a 3 12 15;P $a 6 5 "E0B880";,$a}
$T["Troncs/Tronc_caigut_cap"]={param($r)$a=Tp;$col=@("","","","5A3A20","8A5A30","7A5230","6A4426","6A4426","6A4426","6A4426","5A3A20","4A2E18","3A2412");for($y=3;$y -le 12;$y++){Rc $a 0 $y 16 1 $col[$y]};for($y=3;$y -le 12;$y++){for($x=0;$x -lt 5;$x++){$d=[Math]::Sqrt([Math]::Pow(($x-2)/2.6,2)+[Math]::Pow(($y-7.5)/4.7,2));if($d -le 1.0){P $a $x $y $(if($d -gt 0.8){"3C2412"}elseif($d -lt 0.25){"8A5A30"}elseif($d -lt 0.55){"B07F48"}else{"C99A62"})}}};for($i=0;$i -lt 5;$i++){Rc $a $r.Next(6,13) $r.Next(4,12) 3 1 "3C2412"};Shd $a 1 15 14;Shd $a 1 15 13;,$a}
$T["Troncs/Tronc_caigut"]={param($r)$a=Tp;$col=@("","","","5A3A20","8A5A30","7A5230","6A4426","6A4426","6A4426","6A4426","5A3A20","4A2E18","3A2412");for($y=3;$y -le 12;$y++){Rc $a 0 $y 16 1 $col[$y]};for($i=0;$i -lt 8;$i++){Rc $a $r.Next(13) $r.Next(4,12) 3 1 "3C2412"};Rc $a 9 6 2 2 "2E1C0E";P $a 9 6 "8A5A30";Shd $a 0 15 14;Shd $a 0 15 13;,$a}
$T["Troncs/Bolet_vermell"]={param($r)$a=Tp;Mush $a 8 7 4 "D8304A" "F6F4EC" "EDE6D6";Shd $a 5 11 14;,$a}
$T["Troncs/Bolet_marro"]={param($r)$a=Tp;Mush $a 8 7 4 "9A6A3A" "C8A070" "E6D8B8";Shd $a 5 11 14;,$a}
$T["Troncs/Bolet_lluminos"]={param($r)$a=Tp;Disc $a 8 8 7 "4AC8F018";Disc $a 8 8 5 "4AC8F030";Mush $a 8 7 4 "3AB8E8" "E8FCFF" "BCEAF8";Shd $a 5 11 14;,$a}
$T["Troncs/Bolet_lila"]={param($r)$a=Tp;Mush $a 8 7 4 "8A5AC8" "E0C8F8" "E6DCF0";Shd $a 5 11 14;,$a}
$T["Troncs/Bolets_grup"]={param($r)$a=Tp;Mush $a 5 9 2.5 "D8304A" "F6F4EC" "EDE6D6";Mush $a 11 8 3 "9A6A3A" "C8A070" "E6D8B8";Mush $a 8 5 2 "D8304A" "F6F4EC" "EDE6D6";Shd $a 2 13 14;,$a}
$T["Troncs/Falguera"]={param($r)$a=Tp;foreach($d in @(@(2,4),@(4,2),@(6,1),@(8,1),@(10,1),@(12,2),@(14,4),@(1,7),@(15,7))){Ln $a 8 14 $d[0] $d[1] "3A9A48"};foreach($d in @(@(3,6),@(5,5),@(7,4),@(9,4),@(11,5),@(13,6))){P $a $d[0] $d[1] "5ABA64";P $a ($d[0]+1) ($d[1]+1) "2E8A3C"};Shd $a 5 11 15;,$a}
$T["Troncs/Canyes"]={param($r)$a=Tp;foreach($x in 3,6,8,11,13){$h=$r.Next(7,12);Rc $a $x (15-$h) 1 $h "4A9A3A";Rc $a $x (15-$h) 1 2 "6ABA52"};foreach($x in 6,11){Rc $a $x 3 1 4 "7A4A22";Rc $a ($x-1) 3 1 4 "5A3418"};Shd $a 2 14 15;,$a}
$T["Troncs/Nenufar"]={param($r)$a=Tp;Disc $a 8 9 5.5 "2E7A38";Disc $a 8 9 4.5 "3C9A44";for($y=0;$y -lt 16;$y++){for($x=9;$x -lt 16;$x++){if([Math]::Abs($y-9) -le 0.5*($x-8)-0.5){P $a $x $y "00000000"}}};Disc $a 6 7 1.5 "F2A6C8";P $a 6 7 "F8D44A";P $a 5 6 "FFD0E4";for($i=0;$i -lt 4;$i++){P $a $r.Next(4,10) $r.Next(6,13) "52B05A"};,$a}
$T["Troncs/Cactus"]={param($r)$a=Tp;Rc $a 6 3 4 11 "3A9A48";Rc $a 7 2 2 1 "3A9A48";Rc $a 6 3 1 11 "5ABA64";Rc $a 9 3 1 11 "2A7A38";Rc $a 3 8 3 2 "3A9A48";Rc $a 3 5 2 4 "3A9A48";Rc $a 3 5 1 4 "5ABA64";Rc $a 10 7 3 2 "3A9A48";Rc $a 12 4 2 4 "3A9A48";Rc $a 13 4 1 4 "2A7A38";foreach($p in @(@(7,5),@(8,8),@(7,11),@(4,6),@(13,5))){P $a $p[0] $p[1] "F2E8B0"};P $a 8 1 "F28AB8";P $a 7 1 "F8B8D4";Shd $a 5 10 15;,$a}
$T["Troncs/Cactus_petit"]={param($r)$a=Tp;Ell $a 8 10 4 3.5 "3A9A48";Ell $a 7 9 2 2 "5ABA64";P $a 6 7 "F2E8B0";P $a 10 8 "F2E8B0";P $a 8 12 "F2E8B0";P $a 8 6 "F28AB8";P $a 8 7 "D8608A";Shd $a 4 12 15;,$a}
$T["Troncs/Roca_petita"]={param($r)$a=Tp;$m=Mask 3 7 12 14 3;Shade $a $m "8A8C94" @("7C7E86","9A9CA4") $r 30;Shd $a 4 11 15;,$a}
$T["Troncs/Roca_gran"]={param($r)$a=Tp;$m=Mask 1 3 14 14 4;Shade $a $m "7E808A" @("707280","8E909C") $r 35;Shd $a 3 12 15;,$a}
$T["Troncs/Roca_musgo"]={param($r)$a=Tp;$m=Mask 1 3 14 14 4;Shade $a $m "7E808A" @("707280","8E909C") $r 30;for($i=0;$i -lt 18;$i++){$x=$r.Next(2,14);$y=$r.Next(4,9);if($m[$y*16+$x]){P $a $x $y @("5F7F3A","4F6C2E","83A557")[$r.Next(3)]}};Shd $a 3 12 15;,$a}
$T["Troncs/Pedres"]={param($r)$a=Tp;foreach($p in @(@(1,8,6,13),@(7,9,11,14),@(10,6,14,12))){$m=Mask $p[0] $p[1] $p[2] $p[3] 2;Shade $a $m "8A8C94" @("7C7E86","9A9CA4") $r 30};Shd $a 2 14 15;,$a}
$T["Troncs/Mata_herba"]={param($r)$a=Tp;for($x=2;$x -lt 14;$x++){if($r.Next(5) -eq 0){continue};$h=$r.Next(3,8);Rc $a $x (15-$h) 1 $h $(if($r.Next(2)){"3A9A48"}else{"2E8A3C"});P $a $x (15-$h) "6ACC74"};Shd $a 2 13 15;,$a}
# ---- VEGETACIO (setos i flors)
$T["Vegetacio/Seto_verd"]={param($r)Bush $r "3C8A38" @("2E7030","58A84A","3C8A38") @() 0 3}
$T["Vegetacio/Seto_dens"]={param($r)Bush $r "2A6A2C" @("1E5222","3C8A38","2A6A2C") @() 0 2}
$T["Vegetacio/Seto_flors_roses"]={param($r)Bush $r "3C8A38" @("2E7030","58A84A") @("EE7BA2","F6B0C8","F8D44A") 12 3}
$T["Vegetacio/Seto_flors_blanques"]={param($r)Bush $r "3C8A38" @("2E7030","58A84A") @("F6F4EC","F8F0C0","F2C84B") 12 3}
$T["Vegetacio/Seto_roselles"]={param($r)Bush $r "3C8A38" @("2E7030","58A84A") @("D83A3A","E8584A","1A1A1A") 10 3}
$T["Vegetacio/Seto_baies"]={param($r)Bush $r "3C8A38" @("2E7030","58A84A") @("C8283A","E8485A","8A1428") 11 3}
$T["Vegetacio/Seto_lila"]={param($r)Bush $r "3C8A38" @("2E7030","58A84A") @("A878E0","C8A8F0","7A52C0") 12 3}
$T["Vegetacio/Seto_tardor"]={param($r)Bush $r "B8561E" @("8E3A14","D2701E","F2B04A") @() 0 3}
$T["Vegetacio/Seto_neu"]={param($r)Bush $r "DCEAF4" @("C0D6E8","EEF6FC","A6C0D8") @("3C8A38","2E7030") 6 3}
$T["Vegetacio/Seto_alt"]={param($r)Bush $r "2E7A34" @("1E5222","4A9A44","2E7A34") @() 0 1}
$T["Vegetacio/Flor_margarida"]={param($r)$a=Tp;Stem $a 8 8;Disc $a 8 5 2.7 "F6F4EC";Disc $a 8 5 1.1 "F2C84B";P $a 8 2 "FFFFFF";,$a}
$T["Vegetacio/Flor_rosa"]={param($r)$a=Tp;Stem $a 8 8;Disc $a 8 5 2.6 "D8304A";Disc $a 8 5 1.6 "A01E36";P $a 8 5 "E8607A";P $a 7 4 "F08098";,$a}
$T["Vegetacio/Flor_rosa_blanca"]={param($r)$a=Tp;Stem $a 8 8;Disc $a 8 5 2.6 "F6ECEE";Disc $a 8 5 1.6 "E8C4CC";P $a 8 5 "D8A0B0";,$a}
$T["Vegetacio/Flor_tulipa_vermella"]={param($r)$a=Tp;Stem $a 8 8;Rc $a 6 3 5 4 "D8304A";P $a 6 2 "D8304A";P $a 8 2 "D8304A";P $a 10 2 "D8304A";Rc $a 7 7 3 1 "A01E36";Rc $a 7 3 1 3 "F08098";,$a}
$T["Vegetacio/Flor_tulipa_groga"]={param($r)$a=Tp;Stem $a 8 8;Rc $a 6 3 5 4 "F2C020";P $a 6 2 "F2C020";P $a 8 2 "F2C020";P $a 10 2 "F2C020";Rc $a 7 7 3 1 "C8981A";Rc $a 7 3 1 3 "FFE070";,$a}
$T["Vegetacio/Flor_gira_sol"]={param($r)$a=Tp;Stem $a 8 8;Disc $a 8 5 3.8 "F2C020";Disc $a 8 5 2.3 "5A3A18";P $a 7 4 "7A5428";foreach($p in @(@(8,1),@(4,5),@(12,5),@(5,2),@(11,2))){P $a $p[0] $p[1] "FFE070"};,$a}
$T["Vegetacio/Flor_lliri_blau"]={param($r)$a=Tp;Stem $a 8 7;Rc $a 7 2 3 2 "4A6AE0";Rc $a 5 4 3 2 "4A6AE0";Rc $a 9 4 3 2 "4A6AE0";Rc $a 7 5 3 2 "6A8AF0";P $a 8 4 "F2C84B";P $a 8 3 "8AA8FF";,$a}
$T["Vegetacio/Flor_lavanda"]={param($r)$a=Tp;foreach($p in @(@(5,5),@(8,2),@(11,4))){Rc $a $p[0] $p[1] 1 (15-$p[1]) "3A8A30";for($k=0;$k -lt 4;$k++){P $a $p[0] ($p[1]+$k) $(if($k%2){"9A6AD8"}else{"B48AE8"})};P $a ($p[0]+1) ($p[1]+1) "9A6AD8";P $a ($p[0]-1) ($p[1]+2) "9A6AD8"};Shd $a 3 12 15;,$a}
$T["Vegetacio/Flor_mata_mixta"]={param($r)$a=Tp;for($x=3;$x -lt 13;$x++){Rc $a $x (11+$r.Next(2)) 1 4 "3A8A30"};foreach($p in @(@(4,8,"D8304A"),@(8,5,"F6F4EC"),@(11,7,"F2C020"),@(6,10,"A878E0"),@(10,10,"EE7BA2"))){Rc $a $p[0] ($p[1]+1) 1 (13-$p[1]) "3A8A30";P $a $p[0] $p[1] $p[2];P $a ($p[0]-1) $p[1] $p[2];P $a ($p[0]+1) $p[1] $p[2];P $a $p[0] ($p[1]-1) $p[2];P $a $p[0] $p[1] "F8E070"};Shd $a 2 13 15;,$a}
# ---- ESCOLA / CUINA / BANY (vist des de dalt)
$all=@("centre","NO","NE","SO","SE")
Reg "Escola" "Taula_classe_verda" {DeskObj 10 7 "B8E0A8" "7A5230" $true} 10 7 $all
Reg "Escola" "Taula_classe_marro" {DeskObj 10 7 "C8905A" "7A5230" $true} 10 7 $all
Reg "Escola" "Cadira_classe_marro" {ChairObj 6 6 "A8723C" "7A5230"} 6 6 $all
Reg "Escola" "Cadira_classe_verda" {ChairObj 6 6 "8CC884" "4E8A4A"} 6 6 $all
Reg "Escola" "Pupitre_verd" {$a=DeskObj 10 6 "B8E0A8" "7A5230" $false;Blit $a (ChairObj 6 5 "8CC884" "4E8A4A") 2 7;,$a} 10 12 $all
Reg "Escola" "Pupitre_marro" {$a=DeskObj 10 6 "C8905A" "7A5230" $false;Blit $a (ChairObj 6 5 "A8723C" "7A5230") 2 7;,$a} 10 12 $all
Reg "Escola" "Motxilla_blava" {Backpack "3A7AD8"} 6 7 $all
Reg "Escola" "Motxilla_roja" {Backpack "D83A3A"} 6 7 $all
Reg "Escola" "Llibres" {Books} 8 6 $all
$T["Escola/Taula_llarga_verda_esq"]={param($r)LongDesk "esq" "B8E0A8" "7A5230"}
$T["Escola/Taula_llarga_verda_mig"]={param($r)LongDesk "mig" "B8E0A8" "7A5230"}
$T["Escola/Taula_llarga_verda_dre"]={param($r)LongDesk "dre" "B8E0A8" "7A5230"}
$T["Escola/Taula_llarga_marro_esq"]={param($r)LongDesk "esq" "C8905A" "7A5230"}
$T["Escola/Taula_llarga_marro_mig"]={param($r)LongDesk "mig" "C8905A" "7A5230"}
$T["Escola/Taula_llarga_marro_dre"]={param($r)LongDesk "dre" "C8905A" "7A5230"}
$T["Escola/Taula_professor"]={param($r)$a=DeskObj 14 9 "A8723C" "5E3C20" $false;Rc $a 2 2 4 3 "F4F4F0";P $a 2 2 "FFFFFF";Rc $a 8 2 4 3 "3A4048";Rc $a 8 2 4 1 "5A606C";Rc $a 10 6 2 2 "D83A3A";Shift $a 1 3}
$T["Escola/Pissarra"]={param($r)$a=Tp;Rc $a 0 1 16 12 "8A5A30";Rc $a 1 2 14 9 "2E5A44";Rc $a 1 2 14 1 "3E6E56";Ln $a 3 4 8 4 "E8F0E8";Ln $a 3 6 11 6 "E8F0E8";Ln $a 3 8 6 8 "E8F0E8";P $a 11 8 "F2C84B";Rc $a 1 11 14 1 "C8905A";Rc $a 12 12 3 1 "F4F4F0";Rc $a 0 13 16 1 "00000050";,$a}
$T["Escola/Mapa_mundi"]={param($r)$a=Tp;Rc $a 1 2 14 11 "5A3A20";Rc $a 2 3 12 9 "4A8AC8";foreach($p in @(@(3,5,3,3),@(7,4,4,2),@(8,7,3,4),@(11,6,2,2),@(4,9,3,2))){Rc $a $p[0] $p[1] $p[2] $p[3] "5AAA58"};Rc $a 2 3 12 1 "6AA8E0";,$a}
$T["Escola/Rellotge"]={param($r)$a=Tp;Disc $a 7.5 7.5 6.5 "4A505A";Disc $a 7.5 7.5 5.6 "F4F4F0";foreach($p in @(@(7,2),@(13,8),@(7,13),@(2,8))){P $a $p[0] $p[1] "2A2E36";P $a ($p[0]+1) $p[1] "2A2E36"};Ln $a 7 7 7 4 "2A2E36";Ln $a 8 8 11 8 "2A2E36";P $a 7 7 "D83A3A";,$a}
$T["Escola/Prestatge_llibres"]={param($r)Shelf $r @("C83A3A","3A7AD8","3AB04A","F2C84B","8A5AC8","F4F0E0")}
$T["Escola/Linoleum_escola"]={param($r)$a=Mk "D8CBA8";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if((([int][Math]::Floor($x/8))+([int][Math]::Floor($y/8)))%2 -eq 0){P $a $x $y "CFC29E"}}};Speck $a $r 14 @("C4B792","E0D4B2");,$a}
$T["Escola/Paret_escola"]={param($r)$a=Mk "EAE2C8";Speck $a $r 10 @("E0D8BC");Rc $a 0 9 16 7 "3A7A58";Rc $a 0 9 16 1 "5AA078";Rc $a 0 10 16 1 "2A5A40";Speck $a $r 5 @("328052");,$a}
Reg "Cuina" "Fogons_apagats" {Stove $false} 14 14 @("centre")
Reg "Cuina" "Fogons_encesos" {Stove $true} 14 14 @("centre")
Reg "Cuina" "Fregadora" {Sink} 14 12 @("centre")
Reg "Cuina" "Nevera_dalt" {FridgeTop} 12 12 @("centre")
Reg "Cuina" "Olla" {Pot} 8 8 $all
Reg "Cuina" "Paella" {Pan} 14 12 @("centre")
Reg "Cuina" "Cistella_fruita" {Basket $null} 8 8 $all
Reg "Cuina" "Taula_cuina_fusta" {DeskObj 12 10 "C8905A" "7A5230" $false} 12 10 $all
Reg "Cuina" "Taula_cuina_mantell" {$a=DeskObj 12 10 "FFFFFF" "7A5230" $false;for($y=1;$y -lt 9;$y++){for($x=1;$x -lt 11;$x++){if((($x/2|%{[int][Math]::Floor($_)})+([int][Math]::Floor($y/2)))%2 -eq 0){P $a $x $y "E85A5A"}}};,$a} 12 10 $all
Reg "Cuina" "Cadira_cuina" {ChairObj 6 6 "C8905A" "8A5A30"} 6 6 $all
Reg "Cuina" "Rentadora" {Washer} 12 12 @("centre")
$T["Cuina/Encimera_fusta"]={param($r)$a=Mk "C8905A";Rc $a 0 0 16 1 "E0B07A";Rc $a 0 13 16 3 "8A5A30";Rc $a 0 13 16 1 "6A4020";Speck $a $r 14 @("B8804A","D8A06A");,$a}
$T["Cuina/Encimera_marbre"]={param($r)$a=Mk "ECECF0";Rc $a 0 0 16 1 "FFFFFF";Rc $a 0 13 16 3 "9A9CA6";Rc $a 0 13 16 1 "7A7C86";for($i=0;$i -lt 2;$i++){$x=$r.Next(0,6);for($y=0;$y -lt 13;$y++){$x+=$r.Next(0,2);P $a ($x%16) $y "B8BAC8"}};,$a}
$T["Cuina/Encimera_granit"]={param($r)$a=Mk "4A4C54";Speck $a $r 36 @("3A3C44","5A5C66","6A6C78","2E3038");Rc $a 0 0 16 1 "6A6C78";Rc $a 0 13 16 3 "2A2C32";Rc $a 0 13 16 1 "1A1C22";,$a}
$T["Cuina/Paret_cuina_blanca"]={param($r)Bricks $r "F2F4F6" "C8CED4" "FFFFFF" "DDE2E8"}
$T["Cuina/Rajola_cuina_negra"]={param($r)$a=Mk "F2F0EA";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$q=(([int][Math]::Floor($x/4))+([int][Math]::Floor($y/4)))%2;if($q -eq 0){P $a $x $y "2A2C32"};if($x%4 -eq 0 -or $y%4 -eq 0){P $a $x $y $(if($q -eq 0){"4A4C54"}else{"FFFFFF"})}}};,$a}
Reg "Bany" "Vater" {Toilet} 8 11 $all
Reg "Bany" "Lavabo" {Basin} 10 9 $all
Reg "Bany" "Plat_dutxa" {ShowerTray} 12 12 @("centre")
Reg "Bany" "Catifa_bany" {BathMat} 10 6 $all
Reg "Bany" "Cistella_roba" {Hamper} 8 8 $all
$T["Bany/Banyera_esq"]={param($r)Tub "esq"}
$T["Bany/Banyera_dre"]={param($r)Tub "dre"}
$T["Bany/Mirall"]={param($r)$a=Tp;Rc $a 2 1 12 13 "8A929E";Rc $a 3 2 10 11 "BCE4F4";Rc $a 3 2 10 1 "E4F6FF";for($i=0;$i -lt 6;$i++){P $a (4+$i) (10-$i) "FFFFFF"};P $a 10 4 "FFFFFF";Rc $a 3 12 10 1 "8CBCD4";Rc $a 2 14 12 1 "00000050";,$a}
$T["Bany/Cortina_dutxa"]={param($r)$a=Tp;Rc $a 0 2 16 12 "A8D4EC";for($x=0;$x -lt 16;$x+=3){Rc $a $x 2 1 12 "8CC0DC"};Rc $a 0 1 16 1 "9AA2AC";for($x=1;$x -lt 16;$x+=3){P $a $x 2 "C8D0D8";P $a $x 3 "C8D0D8"};Rc $a 0 13 16 1 "7AAAC8";,$a}
$T["Bany/Tovalloler"]={param($r)$a=Tp;Rc $a 1 2 14 1 "9AA2AC";Rc $a 2 3 5 10 "F2F0E8";Rc $a 2 3 5 1 "FFFFFF";Rc $a 2 12 5 1 "C8404A";Rc $a 9 3 5 8 "4A78C8";Rc $a 9 3 5 1 "6A98E0";Rc $a 9 10 5 1 "F2F0E8";Rc $a 2 13 5 1 "00000040";Rc $a 9 11 5 1 "00000040";,$a}
$T["Bany/Rajola_bany_blava"]={param($r)$a=Mk "6AA8D0";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($x%4 -eq 3 -or $y%4 -eq 3){P $a $x $y "E8F2F8"}elseif($x%4 -eq 0 -and $y%4 -eq 0){P $a $x $y "A8D4EC"}}};Speck $a $r 8 @("5A98C4");,$a}
$T["Bany/Mosaic_bany"]={param($r)$a=Mk "000000";for($y=0;$y -lt 16;$y+=2){for($x=0;$x -lt 16;$x+=2){$c=@("5A9AD0","6AAADC","7AB8E8","4A8AC0","A8D4EC")[$r.Next(5)];Rc $a $x $y 2 2 $c;P $a $x $y (Hx $c 0.3);Rc $a ($x+1) ($y+1) 1 1 (Hx $c -0.25)}};,$a}
$T["Bany/Rajola_bany_verda"]={param($r)TileGrid $r "9ACBB8" "6A9A88" "C4E8D8" @("8ABBA8","AADCC8")}
$T["Bany/Paret_bany_blau"]={param($r)$a=Mk "78B4D8";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($x%8 -eq 7 -or $y%4 -eq 3){P $a $x $y "E8F4FA"}elseif($y%4 -eq 0){P $a $x $y "98CCE8"}}};Rc $a 0 12 16 1 "3A78A8";,$a}
# ---- HOSPITAL i OFICINA (vist des de dalt)
$T["Hospital/Llit_hospital_cap"]={param($r)HBed "cap"}
$T["Hospital/Llit_hospital_peus"]={param($r)HBed "peus"}
Reg "Hospital" "Llitera" {Stretcher} 8 14 $all
Reg "Hospital" "Taula_quirurgica" {OpTable} 10 14 @("centre")
Reg "Hospital" "Cadira_rodes" {Wheelchair} 8 9 $all
Reg "Hospital" "Monitor_constants" {VitalMon} 8 8 $all
Reg "Hospital" "Gotejador" {IVStand} 8 8 $all
Reg "Hospital" "Carro_medicaments" {MedCart} 12 8 $all
Reg "Hospital" "Llum_quirurgic" {SurgLamp} 14 14 @("centre")
Reg "Hospital" "Balanca" {ScaleObj} 8 10 $all
Reg "Hospital" "Lavabo_hospital" {Basin} 10 9 $all
$T["Hospital/Banc_espera_esq"]={param($r)Bench "esq"}
$T["Hospital/Banc_espera_mig"]={param($r)Bench "mig"}
$T["Hospital/Banc_espera_dre"]={param($r)Bench "dre"}
$T["Hospital/Mostrador_esq"]={param($r)LongDesk "esq" "F0F4F6" "9AA2AC"}
$T["Hospital/Mostrador_mig"]={param($r)LongDesk "mig" "F0F4F6" "9AA2AC"}
$T["Hospital/Mostrador_dre"]={param($r)LongDesk "dre" "F0F4F6" "9AA2AC"}
$T["Hospital/Cortina_hospital"]={param($r)$a=Tp;for($x=0;$x -lt 16;$x++){$y=7+[int][Math]::Round([Math]::Sin($x*0.8));Rc $a $x $y 1 2 "A8D8D0";P $a $x $y "D0F0EA"};Rc $a 0 11 16 1 "00000030";,$a}
$T["Hospital/Armariet_medicaments"]={param($r)MedCabinet}
$T["Hospital/Senyal_creu"]={param($r)CrossSign}
$T["Hospital/Linoleum_blau"]={param($r)$a=Mk "BCD8E8";Speck $a $r 16 @("B0CEE0","C8E0F0");for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($x%8 -eq 7 -or $y%8 -eq 7){P $a $x $y "A4C4D8"}}};,$a}
$T["Hospital/Passadis_linia"]={param($r)$a=Mk "E4ECF0";Speck $a $r 10 @("D8E2E8","F4F8FA");Rc $a 0 7 16 2 "3AB88A";Rc $a 0 7 16 1 "6ADCB0";,$a}
$T["Hospital/Rajola_quirofan"]={param($r)TileGrid $r "A8D8C8" "78A898" "D0F0E4" @("98C8B8","B8E8D8")}
$T["Hospital/Paret_quirofan"]={param($r)$a=Mk "78C0A8";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($x%4 -eq 3 -or $y%4 -eq 3){P $a $x $y "A8E0CC"}elseif($x%4 -eq 0 -and $y%4 -eq 0){P $a $x $y "98D8C0"}}};Rc $a 0 13 16 3 "5A9A88";,$a}
Reg "Oficina" "Escriptori_ordinador" {OfficeDesk} 14 9 $all
Reg "Oficina" "Cadira_oficina_negra" {OChair "3A3F48"} 8 8 $all
Reg "Oficina" "Cadira_oficina_blava" {OChair "3A5A98"} 8 8 $all
Reg "Oficina" "Cadira_oficina_vermella" {OChair "A8343E"} 8 8 $all
Reg "Oficina" "Cadira_reunio" {ChairObj 7 7 "4A505A" "2A2E36"} 7 7 $all
$T["Oficina/Taula_reunions_esq"]={param($r)LongDesk "esq" "A88860" "4A3220"}
$T["Oficina/Taula_reunions_mig"]={param($r)LongDesk "mig" "A88860" "4A3220"}
$T["Oficina/Taula_reunions_dre"]={param($r)LongDesk "dre" "A88860" "4A3220"}
$T["Oficina/Mostrador_oficina_esq"]={param($r)LongDesk "esq" "E0C898" "7A5A38"}
$T["Oficina/Mostrador_oficina_mig"]={param($r)LongDesk "mig" "E0C898" "7A5A38"}
$T["Oficina/Mostrador_oficina_dre"]={param($r)LongDesk "dre" "E0C898" "7A5A38"}
$T["Oficina/Panell_recte_H"]={param($r)Panel "LR"}
$T["Oficina/Panell_recte_V"]={param($r)Panel "UD"}
$T["Oficina/Panell_canto_NO"]={param($r)Panel "RD"}
$T["Oficina/Panell_canto_NE"]={param($r)Panel "LD"}
$T["Oficina/Panell_canto_SO"]={param($r)Panel "RU"}
$T["Oficina/Panell_canto_SE"]={param($r)Panel "LU"}
$T["Oficina/Panell_T"]={param($r)Panel "LRD"}
$T["Oficina/Panell_creu"]={param($r)Panel "LRUD"}
Reg "Oficina" "Impressora" {Printer} 10 9 $all
Reg "Oficina" "Fotocopiadora" {Copier} 12 10 @("centre")
$T["Oficina/Arxivador"]={param($r)FileCab}
$T["Oficina/Dispensador_aigua"]={param($r)Cooler}
Reg "Oficina" "Planta" {PlantTop} 8 8 $all
Reg "Oficina" "Paperera_dalt" {BinTop} 6 6 $all
Reg "Oficina" "Pila_papers" {Papers} 8 6 $all
Reg "Oficina" "Portatil" {Laptop} 8 7 $all
Reg "Oficina" "Telefon" {PhoneObj} 5 4 $all
$T["Oficina/Pissarra_blanca"]={param($r)Whiteboard}
$T["Oficina/Tauler_corcho"]={param($r)CorkBoard}
$T["Oficina/Grafic_paret"]={param($r)WallChart}
$T["Oficina/Moqueta_blava"]={param($r)$a=Mk "3E5A8C";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if(($x+$y)%2 -eq 0){P $a $x $y "37528A"}}};Speck $a $r 14 @("4A6A9C","304A7C");,$a}
$T["Oficina/Moqueta_verda"]={param($r)$a=Mk "4A7A5A";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if(($x+$y)%2 -eq 0){P $a $x $y "42705A"}}};Speck $a $r 14 @("5A8A6A","3A6A4A");,$a}
$T["Oficina/Moqueta_ratlles"]={param($r)$a=Mk "6A727E";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($x%4 -lt 2){P $a $x $y "5E6672"}elseif(($x+$y)%2 -eq 0){P $a $x $y "666E7A"}}};,$a}
$T["Oficina/Rajola_oficina"]={param($r)TileGrid $r "C4C8CE" "9A9EA6" "DCE0E6" @("BABEC4","CED2D8")}
$T["Oficina/Parquet_oficina"]={param($r)Planks2 $r @("8A6A48","80603E","765838") "5E4428" "3E2C18" "A08060" $false}
$T["Oficina/Paret_oficina_blanca"]={param($r)$a=Mk "EEEEF0";Speck $a $r 8 @("E2E2E6");Rc $a 0 13 16 3 "C8C8D0";Rc $a 0 13 16 1 "FFFFFF";Rc $a 0 12 16 1 "D4D4DA";,$a}
$T["Oficina/Panell_tela"]={param($r)$a=Mk "7A8CA8";Speck $a $r 40 @("7284A0","8294B0");for($y=0;$y -lt 16;$y+=2){for($x=0;$x -lt 16;$x++){if(($x+$y)%4 -eq 0){P $a $x $y "6E809C"}}};Rc $a 0 0 16 1 "B0BED4";Rc $a 0 15 16 1 "5A6A84";,$a}
# ---- PORT i LABORATORI
$T["Port/Moll_fusta"]={param($r)Planks2 $r @("9A8260","8E7656","826C4C") "6A563C" "3E3020" "B8A078" $true}
$T["Port/Coberta_vaixell"]={param($r)Planks2 $r @("C8A070","BC9664","B08A5A") "9A7444" "6A4A22" "E0BE8A" $false}
$T["Port/Moll_pedra"]={param($r)Rock $r @("8A8C94","80828A","92949C") "3E4048" 3}
$T["Port/Moll_formigo_perill"]={param($r)$a=Mk "9A9CA2";Speck $a $r 30 @("8E9096","A8AAB0","84868C");Rc $a 0 6 16 3 "E6B422";Rc $a 0 6 16 1 "F5D45A";Rc $a 0 8 16 1 "A67C0F";,$a}
$T["Port/Paret_port_fusta"]={param($r)Tr (Planks2 $r @("6A8494","5E7888","52707F") "3E5866" "2E4450" "8AA4B4" $true)}
$T["Port/Paret_ondulada_blava"]={param($r)Ribbed "4A78A8"}
$T["Port/Paret_ondulada_roja"]={param($r)Ribbed "A8483A"}
$T["Port/Paret_ondulada_verda"]={param($r)Ribbed "4A8A5A"}
$T["Port/Paret_ondulada_gris"]={param($r)Ribbed "8A929E"}
$T["Port/Contenidor_blau_esq"]={param($r)Container "esq" "3A6AB0"}
$T["Port/Contenidor_blau_mig"]={param($r)Container "mig" "3A6AB0"}
$T["Port/Contenidor_blau_dre"]={param($r)Container "dre" "3A6AB0"}
$T["Port/Contenidor_roig_esq"]={param($r)Container "esq" "B8402E"}
$T["Port/Contenidor_roig_mig"]={param($r)Container "mig" "B8402E"}
$T["Port/Contenidor_roig_dre"]={param($r)Container "dre" "B8402E"}
$T["Port/Contenidor_verd_esq"]={param($r)Container "esq" "3A8A58"}
$T["Port/Contenidor_verd_mig"]={param($r)Container "mig" "3A8A58"}
$T["Port/Contenidor_verd_dre"]={param($r)Container "dre" "3A8A58"}
$T["Port/Barca_cap"]={param($r)RowBoat "cap"}
$T["Port/Barca_popa"]={param($r)RowBoat "popa"}
$T["Port/Barana_H"]={param($r)Rail "LR"}
$T["Port/Barana_V"]={param($r)Rail "UD"}
$T["Port/Barana_canto_NO"]={param($r)Rail "RD"}
$T["Port/Barana_canto_NE"]={param($r)Rail "LD"}
$T["Port/Barana_canto_SO"]={param($r)Rail "RU"}
$T["Port/Barana_canto_SE"]={param($r)Rail "LU"}
Reg "Port" "Caixa_fusta" {Crate} 10 10 $all
Reg "Port" "Barril" {BarrelTop} 8 8 $all
Reg "Port" "Corda_enrotllada" {RopeCoil} 8 8 $all
Reg "Port" "Caixa_peix" {FishBox} 10 8 $all
Reg "Port" "Xarxa_pesca" {NetPile} 12 8 $all
Reg "Port" "Bolard" {Bollard} 6 6 $all
Reg "Port" "Pilar_fusta" {PostTop} 6 6 $all
Reg "Port" "Salvavides" {LifeRing} 8 8 $all
Reg "Port" "Ancora" {AnchorObj} 10 11 $all
Reg "Port" "Timo" {HelmObj} 12 12 @("centre")
Reg "Port" "Far_dalt" {LightHouse} 12 12 @("centre")
$T["Laboratori/Banc_lab_esq"]={param($r)LongDesk "esq" "4A505A" "8A929E"}
$T["Laboratori/Banc_lab_mig"]={param($r)LongDesk "mig" "4A505A" "8A929E"}
$T["Laboratori/Banc_lab_dre"]={param($r)LongDesk "dre" "4A505A" "8A929E"}
$T["Laboratori/Banc_blanc_esq"]={param($r)LongDesk "esq" "F0F4F6" "A8B4BC"}
$T["Laboratori/Banc_blanc_mig"]={param($r)LongDesk "mig" "F0F4F6" "A8B4BC"}
$T["Laboratori/Banc_blanc_dre"]={param($r)LongDesk "dre" "F0F4F6" "A8B4BC"}
Reg "Laboratori" "Microscopi" {Microscope} 8 10 $all
Reg "Laboratori" "Gradeta_provetes" {TubeRack} 12 6 $all
Reg "Laboratori" "Matras_verd" {Flask "3AD86A"} 6 6 $all
Reg "Laboratori" "Matras_blau" {Flask "3A9AF0"} 6 6 $all
Reg "Laboratori" "Matras_vermell" {Flask "E8402E"} 6 6 $all
Reg "Laboratori" "Matras_lila" {Flask "B05AE8"} 6 6 $all
Reg "Laboratori" "Bec_bunsen" {Bunsen} 5 5 $all
Reg "Laboratori" "Centrifuga" {Centrifuge} 10 10 $all
Reg "Laboratori" "Tanc_mostra_verd" {SpecTank "3AD86A"} 12 12 @("centre")
Reg "Laboratori" "Tanc_mostra_blau" {SpecTank "3A9AF0"} 12 12 @("centre")
Reg "Laboratori" "Tanc_mostra_lila" {SpecTank "B05AE8"} 12 12 @("centre")
$T["Laboratori/Cuba_verda"]={param($r)Vat "3AD86A"}
$T["Laboratori/Cuba_blava"]={param($r)Vat "3A9AF0"}
$T["Laboratori/Cuba_lila"]={param($r)Vat "B05AE8"}
$T["Laboratori/Cuba_taronja"]={param($r)Vat "F29A2E"}
$T["Laboratori/Campana_extractora"]={param($r)Fumehood}
$T["Laboratori/Armari_reactius"]={param($r)Shelf $r @("3AD86A","E8402E","F2C84B","3A9AF0","B8D4E0","B05AE8")}
Reg "Laboratori" "Valvula" {ValveObj} 6 6 $all
Reg "Laboratori" "Bobina_tesla" {Tesla} 10 10 $all
Reg "Laboratori" "Gabia_ratolins" {Cage} 10 8 $all
Reg "Laboratori" "Tamboret" {Stool} 6 6 $all
Reg "Laboratori" "Espectrometre" {Spectro} 12 8 $all
Reg "Laboratori" "Balanca_precisio" {LabBalance} 8 8 $all
$T["Laboratori/Senyal_biologic"]={param($r)HazardSign "F2C020" "2A2E36"}
$T["Laboratori/Senyal_radioactiu"]={param($r)HazardSign "F29A2E" "2A2E36"}
$T["Laboratori/Tuberia_H"]={param($r)Pipe "LR" "9AA2AC"}
$T["Laboratori/Tuberia_V"]={param($r)Pipe "UD" "9AA2AC"}
$T["Laboratori/Tuberia_canto_NO"]={param($r)Pipe "RD" "9AA2AC"}
$T["Laboratori/Tuberia_canto_NE"]={param($r)Pipe "LD" "9AA2AC"}
$T["Laboratori/Tuberia_canto_SO"]={param($r)Pipe "RU" "9AA2AC"}
$T["Laboratori/Tuberia_canto_SE"]={param($r)Pipe "LU" "9AA2AC"}
$T["Laboratori/Tuberia_T"]={param($r)Pipe "LRD" "9AA2AC"}
$T["Laboratori/Tuberia_creu"]={param($r)Pipe "LRUD" "9AA2AC"}
$T["Laboratori/Tuberia_verda_H"]={param($r)Pipe "LR" "3AB868"}
$T["Laboratori/Tuberia_verda_V"]={param($r)Pipe "UD" "3AB868"}
$T["Laboratori/Tuberia_verda_canto_NO"]={param($r)Pipe "RD" "3AB868"}
$T["Laboratori/Tuberia_verda_canto_NE"]={param($r)Pipe "LD" "3AB868"}
$T["Laboratori/Tuberia_verda_canto_SO"]={param($r)Pipe "RU" "3AB868"}
$T["Laboratori/Tuberia_verda_canto_SE"]={param($r)Pipe "LU" "3AB868"}
$T["Laboratori/Rajola_lab_blanca"]={param($r)TileGrid $r "F0F4F6" "A8C4D4" "FFFFFF" @("E4ECF0")}
$T["Laboratori/Sol_epoxi"]={param($r)$a=Mk "5A606C";Speck $a $r 20 @("525866","666C78");for($y=0;$y -lt 16;$y++){if($y%8 -eq 0){Rc $a 0 $y 16 1 "7A808C"}};,$a}
$T["Laboratori/Sol_reixa"]={param($r)$a=Mk "14171C";for($y=0;$y -lt 16;$y+=2){for($x=0;$x -lt 16;$x+=2){Rc $a $x $y 2 2 "4A525E";P $a $x $y "8A94A2"}};,$a}
$T["Laboratori/Sol_perill"]={param($r)$a=Mk "2A2E36";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if((($x+$y)%8) -lt 4){P $a $x $y "F2C020"}}};,$a}
$T["Laboratori/Paret_lab_blanca"]={param($r)$a=Mk "EEF2F4";Speck $a $r 8 @("E2E8EC");Rc $a 0 8 16 2 "4A90D0";Rc $a 0 8 16 1 "7AB8F0";Rc $a 0 13 16 3 "C4CED4";Rc $a 0 13 16 1 "FFFFFF";,$a}
$T["Laboratori/Paret_lab_metall"]={param($r)$a=Mk "8A929E";for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($x%8 -eq 7){P $a $x $y "4A525E"}elseif($x%8 -eq 0){P $a $x $y "B4BCC8"}}};foreach($p in @(@(2,2),@(5,2),@(2,13),@(5,13),@(10,2),@(13,2),@(10,13),@(13,13))){P $a $p[0] $p[1] "5A626E";P $a ($p[0]-1) ($p[1]-1) "C8D0DC"};Rc $a 0 15 16 1 "5A626E";,$a}
# ---- sortida
if(Test-Path $Out){Get-ChildItem $Out -Recurse -File | Remove-Item -Force};New-Item -ItemType Directory -Force $Out|Out-Null
$byFam=[ordered]@{};foreach($n in $T.Keys){$f=$n.Split('/')[0];if(-not $byFam.Contains($f)){$byFam[$f]=@()};$byFam[$f]+=$n}
$scale=6;$cols=9;$cw=16*$scale+10;$ch=16*$scale+26
$font=New-Object System.Drawing.Font "Consolas",7;$br=New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(210,215,225))
$total=0
foreach($fam in $byFam.Keys){$names=$byFam[$fam];$rows=[Math]::Ceiling($names.Count/$cols)
 $sheet=New-Object System.Drawing.Bitmap ($cols*$cw+10),($rows*$ch+10);$g=[System.Drawing.Graphics]::FromImage($sheet);$g.Clear([System.Drawing.Color]::FromArgb(24,26,32));$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half'
 $i=0;$dir="$Out\$fam";New-Item -ItemType Directory -Force $dir|Out-Null
 foreach($n in $names){$seed=0;foreach($c in $n.ToCharArray()){$seed=($seed*31+[int]$c)%100000};$rng=New-Object System.Random $seed
  $tv=$T[$n];if($tv -is [hashtable]){$px=Shift (& $tv.draw) $tv.dx $tv.dy}else{$px=& $tv $rng};$bmp=New-Object System.Drawing.Bitmap 16,16
  for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$h=$px[$y*16+$x];$al=if($h.Length -ge 8){[Convert]::ToInt32($h.Substring(6,2),16)}else{255};$bmp.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($al,[Convert]::ToInt32($h.Substring(0,2),16),[Convert]::ToInt32($h.Substring(2,2),16),[Convert]::ToInt32($h.Substring(4,2),16)))}}
  $bmp.Save("$dir\"+$n.Split('/')[1]+".png")
  $cx=10+($i%$cols)*$cw;$cy=10+[Math]::Floor($i/$cols)*$ch
  for($qy=0;$qy -lt 16;$qy++){for($qx=0;$qx -lt 16;$qx++){$cc=if(($qx+$qy)%2){[System.Drawing.Color]::FromArgb(70,74,84)}else{[System.Drawing.Color]::FromArgb(52,55,64)};$sb=New-Object System.Drawing.SolidBrush $cc;$g.FillRectangle($sb,$cx+$qx*6,$cy+$qy*6,6,6);$sb.Dispose()}}
  $g.DrawImage($bmp,$cx,$cy,16*$scale,16*$scale);$g.DrawString($n.Split('/')[1],$font,$br,$cx-2,$cy+16*$scale+3);$bmp.Dispose();$i++;$total++}
 $g.Dispose();$sheet.Save("$Out\_$fam.png")}
"$total textures"













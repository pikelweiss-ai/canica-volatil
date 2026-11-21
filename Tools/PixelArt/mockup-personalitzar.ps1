param([string]$Out="C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace\Docs\Dissenys\img")
Add-Type -AssemblyName System.Drawing
$px="C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace\Assets\Sprites\Pixel"
$btn="C:\Users\Usuario\Canica Alpha\Builds\Art per editar\Menu\Botons PNG"
New-Item -ItemType Directory -Force $Out | Out-Null
function Col($h,$a=255){$c=[System.Drawing.ColorTranslator]::FromHtml($h);[System.Drawing.Color]::FromArgb($a,$c.R,$c.G,$c.B)}
function RR($g,$x,$y,$w,$h,$r,$fill,$line,$lw){$p=New-Object System.Drawing.Drawing2D.GraphicsPath;$d=$r*2;$p.AddArc($x,$y,$d,$d,180,90);$p.AddArc($x+$w-$d,$y,$d,$d,270,90);$p.AddArc($x+$w-$d,$y+$h-$d,$d,$d,0,90);$p.AddArc($x,$y+$h-$d,$d,$d,90,90);$p.CloseFigure()
 if($fill){$g.FillPath((New-Object System.Drawing.SolidBrush $fill),$p)};if($line){$g.DrawPath((New-Object System.Drawing.Pen $line,$lw),$p)}}
function Txt($g,$t,$x,$y,$w,$h,$size,$color,$center=$true){$f=New-Object System.Drawing.Font "Segoe UI",$size,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel);$sf=New-Object System.Drawing.StringFormat;if($center){$sf.Alignment='Center'};$sf.LineAlignment='Center'
 $g.DrawString($t,$f,(New-Object System.Drawing.SolidBrush $color),(New-Object System.Drawing.RectangleF $x,$y,$w,$h),$sf)}
function Spr($g,$img,$x,$y,$s){$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half';$g.DrawImage($img,$x,$y,16*$s,16*$s);$g.InterpolationMode='HighQualityBicubic'}
function Silhouette($img){$b=New-Object System.Drawing.Bitmap $img.Width,$img.Height;for($y=0;$y -lt $img.Height;$y++){for($x=0;$x -lt $img.Width;$x++){$c=$img.GetPixel($x,$y);if($c.A -gt 10){$b.SetPixel($x,$y,[System.Drawing.Color]::FromArgb(255,20,36,52))}}};$b}
function Cropped($g,$img,$cx,$cy,$box){$b=[System.Drawing.Bitmap]$img;$x0=16;$y0=16;$x1=-1;$y1=-1;for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){if($b.GetPixel($x,$y).A -gt 10){if($x -lt $x0){$x0=$x};if($y -lt $y0){$y0=$y};if($x -gt $x1){$x1=$x};if($y -gt $y1){$y1=$y}}}}
 if($x1 -lt 0){return};$w=$x1-$x0+1;$h=$y1-$y0+1;$s=[Math]::Floor($box/[Math]::Max($w,$h));$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half'
 $dst=New-Object System.Drawing.Rectangle ([int]($cx-$w*$s/2)),([int]($cy-$h*$s/2)),([int]($w*$s)),([int]($h*$s));$src=New-Object System.Drawing.Rectangle $x0,$y0,$w,$h;$g.DrawImage($img,$dst,$src,[System.Drawing.GraphicsUnit]::Pixel);$g.InterpolationMode='HighQualityBicubic'}
function Glyph($g,$shape,$x,$y,$s,$c,$a,$i){$col=[System.Drawing.Color]::FromArgb($a,$c.R,$c.G,$c.B);$br=New-Object System.Drawing.SolidBrush $col;$pen=New-Object System.Drawing.Pen $col,([Math]::Max(2,$s/6))
 switch($shape){
  "ribbon"{$g.FillEllipse($br,$x-$s,$y-$s/3,$s*2,$s*0.66)}
  "dots"{$g.FillEllipse($br,$x-$s/2,$y-$s/2,$s,$s)}
  "sparks"{$g.DrawLine($pen,$x-$s,$y-$s*0.4,$x+$s,$y+$s*0.4);$g.DrawLine($pen,$x-$s*0.4,$y+$s*0.8,$x+$s*0.4,$y-$s*0.8)}
  "rings"{$g.DrawEllipse($pen,$x-$s,$y-$s,$s*2,$s*2)}
  "zigzag"{$g.DrawLines($pen,@((New-Object System.Drawing.PointF ($x-$s),($y+$s*0.6)),(New-Object System.Drawing.PointF ($x-$s*0.3),($y-$s*0.6)),(New-Object System.Drawing.PointF ($x+$s*0.3),($y+$s*0.6)),(New-Object System.Drawing.PointF ($x+$s),($y-$s*0.6))))}
  "steps"{$g.FillEllipse($br,$x-$s*0.5,$y-$s*(0.9-($i%2)*1.2),$s*0.7,$s*1.1);$g.FillEllipse($br,$x-$s*0.3,$y-$s*(0.9-($i%2)*1.2)-$s*0.5,$s*0.3,$s*0.3)}
  "stars"{$pts=@();for($k=0;$k -lt 10;$k++){$r=$(if($k%2){$s*0.45}else{$s});$an=-1.5708+$k*0.62832;$pts+=New-Object System.Drawing.PointF ($x+$r*[Math]::Cos($an)),($y+$r*[Math]::Sin($an))};$g.FillPolygon($br,$pts)}
  "hearts"{$g.FillEllipse($br,$x-$s,$y-$s*0.8,$s,$s);$g.FillEllipse($br,$x,$y-$s*0.8,$s,$s);$g.FillPolygon($br,@((New-Object System.Drawing.PointF ($x-$s*0.97),($y-$s*0.1)),(New-Object System.Drawing.PointF ($x+$s*0.97),($y-$s*0.1)),(New-Object System.Drawing.PointF $x,($y+$s*1.05))))}
  "flames"{$g.FillPolygon($br,@((New-Object System.Drawing.PointF $x,($y-$s*1.2)),(New-Object System.Drawing.PointF ($x+$s*0.75),($y+$s*0.3)),(New-Object System.Drawing.PointF ($x+$s*0.35),($y+$s*0.95)),(New-Object System.Drawing.PointF ($x-$s*0.35),($y+$s*0.95)),(New-Object System.Drawing.PointF ($x-$s*0.75),($y+$s*0.3))));$in=[System.Drawing.Color]::FromArgb($a,255,225,120);$g.FillEllipse((New-Object System.Drawing.SolidBrush $in),$x-$s*0.28,$y,$s*0.56,$s*0.8)}
  "snow"{for($k=0;$k -lt 3;$k++){$an=$k*1.0472;$g.DrawLine($pen,$x-$s*[Math]::Cos($an),$y-$s*[Math]::Sin($an),$x+$s*[Math]::Cos($an),$y+$s*[Math]::Sin($an))}}
  "diamonds"{$g.FillPolygon($br,@((New-Object System.Drawing.PointF $x,($y-$s)),(New-Object System.Drawing.PointF ($x+$s*0.7),$y),(New-Object System.Drawing.PointF $x,($y+$s)),(New-Object System.Drawing.PointF ($x-$s*0.7),$y)))}
  "leaves"{$g.FillPolygon($br,@((New-Object System.Drawing.PointF ($x-$s),($y+$s*0.6)),(New-Object System.Drawing.PointF ($x-$s*0.1),($y-$s)),(New-Object System.Drawing.PointF ($x+$s),($y-$s*0.4)),(New-Object System.Drawing.PointF ($x+$s*0.2),($y+$s*0.8))));$g.DrawLine((New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb($a,20,60,20)),2),$x-$s*0.8,$y+$s*0.5,$x+$s*0.7,$y-$s*0.4)}}}
function TrailStrip($g,$shape,$col,$x,$y,$s,$n){for($k=0;$k -lt $n;$k++){$a=[int](255-$k*(200/$n));Glyph $g $shape ($x+$k*$s*1.9) ($y-($k%2)*($s*0.35)) ($s*(1-$k*0.12)) $col $a $k}}
function Render($tab,$file){
 $bmp=New-Object System.Drawing.Bitmap 1600,900;$g=[System.Drawing.Graphics]::FromImage($bmp);$g.SmoothingMode='AntiAlias';$g.InterpolationMode='HighQualityBicubic';$g.TextRenderingHint='AntiAliasGridFit'
 $g.Clear((Col "#0B1620"))
 $g.FillRectangle((New-Object System.Drawing.SolidBrush (Col "#102838")),0,0,1600,110);$g.FillRectangle((New-Object System.Drawing.SolidBrush (Col "#071018")),0,106,1600,4)
 RR $g 40 26 90 62 10 (Col "#D83A3A") (Col "#8A1E1E") 3;Txt $g "<" 40 26 90 62 40 ([System.Drawing.Color]::White)
 Txt $g "PERSONALITZAR" 400 18 800 80 54 (Col "#FFDE73")
 RR $g 1240 34 220 48 24 (Col "#1A3A4C") (Col "#F2B830") 2;$g.FillEllipse((New-Object System.Drawing.SolidBrush (Col "#F2C020")),1250,40,36,36);Txt $g "1 842" 1290 34 160 48 28 ([System.Drawing.Color]::White)
 $skin=[System.Drawing.Image]::FromFile("$px\Skins\Llop.png");$hat=[System.Drawing.Image]::FromFile("$px\Barrets\Cuiner.png")
 $cats=@(@("CANIQUES","skins"),@("BARRETS","hats"),@("ESTELES","trails"))
 $y=150
 foreach($c in $cats){$sel=($c[1] -eq $tab)
  if($sel){RR $g 40 $y 150 140 16 (Col "#F2B830") (Col "#FFDE73") 3}else{RR $g 40 $y 150 140 16 (Col "#1A3A4C") (Col "#2A4A68") 2}
  $cx=115;$cy=$y+52
  if($c[1] -eq "skins"){Spr $g $skin ($cx-32) ($cy-32) 4}elseif($c[1] -eq "hats"){Cropped $g $hat $cx $cy 64}else{TrailStrip $g "flames" (Col "#FF8626") ($cx-40) ($cy+4) 11 4}
  Txt $g $c[0] 40 ($y+104) 150 30 18 $(if($sel){Col "#2A1A00"}else{Col "#CFE0EA"});$y+=160}
 Txt $g "Més aviat:`nCOLORS`nEXPRESSIONS" 40 640 150 100 15 (Col "#6E8696")
 RR $g 220 140 700 660 24 (Col "#0F2231") (Col "#1F4258") 3
 $g.SetClip((New-Object System.Drawing.Rectangle 224,144,692,652))
 $gp=New-Object System.Drawing.Pen (Col "#16324A"),2;for($i=0;$i -lt 18;$i++){$g.DrawLine($gp,224+$i*40,144,224+$i*40,796)};for($i=0;$i -lt 18;$i++){$g.DrawLine($gp,224,144+$i*40,916,144+$i*40)}
 $g.ResetClip()
 $path=@(@(330,650),@(375,610),@(415,575),@(455,540),@(490,510),@(520,485),@(548,465))
 for($i=0;$i -lt $path.Count;$i++){$s=[int](12+$i*3);$a=[int](70+$i*30);Glyph $g "flames" $path[$i][0] $path[$i][1] $s (Col "#FF8626") $a $i}
 $g.FillEllipse((New-Object System.Drawing.SolidBrush (Col "#091821" 160)),505,545,150,26)
 Spr $g $skin 500 395 10;Spr $g $hat 500 395 10
 Txt $g "Mou la canica: WASD o arrossega" 240 668 660 30 18 (Col "#6E8696");Txt $g "Quieta, es queda al mig amb barret i estela" 240 694 660 30 16 (Col "#4F6A7C")
 RR $g 500 740 160 40 10 (Col "#FFFFFF") $null 0;Txt $g "LLOP" 500 740 160 40 24 (Col "#10202C")
 RR $g 960 140 600 660 24 (Col "#13293A") (Col "#1F4258") 3
 RR $g 984 160 150 46 12 (Col "#F2B830") $null 0;Txt $g "Tots  18/27" 984 160 150 46 22 (Col "#2A1A00")
 Txt $g "Rar · Èpic · Llegendari: quan hi haja qualitats" 1150 160 400 46 17 (Col "#6E8696") $false
 $items=@();if($tab -eq "skins"){$items=@("Aurora","Bombolla","Caramel","Ciclope","Cosmos","Fantasma","Flor","Foc","Gat","Gel","Llop","Ninja","Postal","Robot","Zombi","Safir")}elseif($tab -eq "hats"){$items=@("Antenes","Aureola","Banyes","Bolet","Bruixa","Capita","Corona","Cowboy","Cuiner","Explorador","Flor","Gat","Mag","Pirata","Samurai","Viking")}else{$items=@("Cap","Blanc","Foc","Elèctric","Tòxic","Arc","Cors","Flamarada","Floc de neu","Diamants","Fulles","Estrelles","Ones","Pisades","Espurnes","Zig-zag")}
 $tshape=@("ribbon","dots","flames","sparks","rings","ribbon","hearts","flames","snow","diamonds","leaves","stars","rings","steps","sparks","zigzag")
 $trailCols=@("#9AA6B0","#FFFFFF","#FF8626","#4DE6FF","#8CFF4D","#FF4DFF","#FF6A8A","#FFB020","#CFE8FF","#7AF0FF","#5ABA50","#FFE060","#4A90E8","#C8A070","#FFD84A","#B070E8")
 $locked=@(13,14,15);$new=@(2,9);$selIdx=if($tab -eq "skins"){10}elseif($tab -eq "hats"){8}else{2}
 for($i=0;$i -lt 16;$i++){$cx=984+($i%4)*144;$cy=230+[Math]::Floor($i/4)*140;$isSel=($i -eq $selIdx)
  RR $g $cx $cy 130 128 14 (Col "#1B3550") $(if($isSel){Col "#F2B830"}else{Col "#2A4A68"}) $(if($isSel){5}else{2})
  $isLocked=($locked -contains $i)
  if($tab -eq "trails"){TrailStrip $g $tshape[$i] (Col $trailCols[$i]) ($cx+24) ($cy+44) 12 5}
  else{$folder=if($tab -eq "skins"){"Skins"}else{"Barrets"};$p="$px\$folder\$($items[$i]).png";if(Test-Path $p){$im=[System.Drawing.Image]::FromFile($p);if($isLocked){$im=Silhouette $im};if($tab -eq "hats"){Cropped $g $im ($cx+65) ($cy+48) 80}else{Spr $g $im ($cx+17) ($cy+8) 6}}}
  if($isLocked){$g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(150,16,30,44))),$cx+3,$cy+3,124,90);$lx=$cx+49;$ly=$cy+30;RR $g $lx ($ly+16) 32 26 5 (Col "#8AA0B0") $null 0;$g.DrawArc((New-Object System.Drawing.Pen (Col "#8AA0B0"),5),($lx+6),$ly,20,26,180,180);$g.FillEllipse((New-Object System.Drawing.SolidBrush (Col "#13293A")),($lx+13),($ly+24),6,6)}
  Txt $g $items[$i] $cx ($cy+98) 130 28 17 $(if($isLocked){Col "#4A6478"}else{[System.Drawing.Color]::White})
  if($new -contains $i){RR $g ($cx+80) ($cy+6) 46 24 8 (Col "#D83A3A") $null 0;Txt $g "NOU" ($cx+80) ($cy+6) 46 24 15 ([System.Drawing.Color]::White)}
  if($isSel){RR $g ($cx+8) ($cy+94) 114 32 8 (Col "#3AB868") (Col "#1E7A3E") 2;Txt $g "UTILITZAR" ($cx+8) ($cy+94) 114 32 17 ([System.Drawing.Color]::White)}}
 Txt $g "Clic: previsualitza · UTILITZAR: aplica · Esc: tornar" 220 820 1340 40 20 (Col "#6E8696")
 $g.Dispose();$bmp.Save("$Out\$file");"ok $file"}
Render "skins" "personalitzar-caniques.png"
Render "hats" "personalitzar-barrets.png"
Render "trails" "personalitzar-esteles.png"

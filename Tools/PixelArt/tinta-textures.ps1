param([string]$Base="C:\Users\Usuario\Canica Alpha\Builds\Art per editar\Textures base",[string]$Out="C:\Users\Usuario\Canica Alpha\Builds\Art per editar\Textures base\Proves")
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $Out | Out-Null
# Paletes: 5 tons de fosc a clar (mateix ordre que els grisos 3D3D3D,4C4C4C,999999,B7B7B7,EFEFEF)
function C($h){[System.Drawing.Color]::FromArgb([Convert]::ToInt32($h.Substring(0,2),16),[Convert]::ToInt32($h.Substring(2,2),16),[Convert]::ToInt32($h.Substring(4,2),16))}
$pal=[ordered]@{
 "Cripta"=@("2A2A33","3A3A46","6F6F80","8A8A9C","C9C9D6")
 "Neu"=@("5A7390","7087A3","A9C3D9","C6DBEC","F4FAFF")
 "Bosc"=@("2B3A22","3B4D2C","6B7F4A","85995E","C2D18F")
 "Lava"=@("3B1410","5A1E12","8F3A1C","B5532A","F0A24A")
 "Desert"=@("6B4A28","7F5A33","B58A55","CFA46C","F2D9A4")
 "Aigua"=@("1D3347","274560","3F7A8F","5C9FB0","BFE6E8")
}
$lv=@{"3D3D3D"=0;"4C4C4C"=1;"999999"=2;"B7B7B7"=3;"EFEFEF"=4}
function Tint($src,$p){ $w=$src.Width;$h=$src.Height;$o=New-Object System.Drawing.Bitmap $w,$h
 for($y=0;$y -lt $h;$y++){for($x=0;$x -lt $w;$x++){$c=$src.GetPixel($x,$y);if($c.A -eq 0){$o.SetPixel($x,$y,$c);continue}
  $k="{0:X2}{1:X2}{2:X2}" -f $c.R,$c.G,$c.B; if($lv.ContainsKey($k)){$i=$lv[$k]}else{$g=($c.R+$c.G+$c.B)/3;$i=[Math]::Min(4,[Math]::Max(0,[int][Math]::Round(($g-61)/(239-61)*4)))}
  $n=C $p[$i]; $o.SetPixel($x,$y,[System.Drawing.Color]::FromArgb($c.A,$n.R,$n.G,$n.B))}}; $o}
$files=Get-ChildItem $Base -Filter *.png
$scale=8;$cols=$pal.Count+1
$sheet=New-Object System.Drawing.Bitmap ($cols*16*$scale+($cols+1)*8),($files.Count*16*$scale+($files.Count+1)*8)
$g=[System.Drawing.Graphics]::FromImage($sheet);$g.Clear([System.Drawing.Color]::FromArgb(24,26,32));$g.InterpolationMode='NearestNeighbor';$g.PixelOffsetMode='Half'
$r=0
foreach($f in $files){$src=New-Object System.Drawing.Bitmap $f.FullName
 $g.DrawImage($src,8,8+$r*(16*$scale+8),16*$scale,16*$scale)
 $ci=1; foreach($name in $pal.Keys){$t=Tint $src $pal[$name]; $dir="$Out\$name"; New-Item -ItemType Directory -Force $dir|Out-Null; $t.Save("$dir\$($f.BaseName).png")
  $g.DrawImage($t,8+$ci*(16*$scale+8),8+$r*(16*$scale+8),16*$scale,16*$scale);$ci++;$t.Dispose()}
 $src.Dispose();$r++}
$g.Dispose();$sheet.Save("$Out\_prova.png")

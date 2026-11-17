# Contact sheet del mateix encaix geometric que HatFit (base opaca).
Add-Type -AssemblyName System.Drawing
$root=Split-Path (Split-Path $PSScriptRoot)
$files=@(Get-ChildItem (Join-Path $root 'Assets/Sprites/Pixel/Barrets') -Filter '*.png')
$sheet=[Drawing.Bitmap]::new(960,([int][Math]::Ceiling($files.Count/4.0)*270))
$g=[Drawing.Graphics]::FromImage($sheet);$g.Clear([Drawing.Color]::FromArgb(20,30,45));$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor;$g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
$body=[Drawing.Bitmap]::new((Join-Path $root 'Assets/Sprites/pixil-frame-0.png'))
$font=[Drawing.Font]::new('Arial',12)
for($i=0;$i -lt $files.Count;$i++){
 $hat=[Drawing.Bitmap]::new($files[$i].FullName)
 $maxY=0;for($y=0;$y -lt $hat.Height;$y++){for($x=0;$x -lt $hat.Width;$x++){if($hat.GetPixel($x,$y).A -gt 25){$maxY=[Math]::Max($maxY,$y)}}}
 $left=($i%4)*240+64;$top=[Math]::Floor($i/4)*270+130
 $g.DrawImage($body,[int]$left,[int]$top,112,112)
 $lift=if($files[$i].BaseName -eq 'Aureola'){22}else{0}
 $hatTop=$top+8-$lift-($maxY+1)*117.6/16
 $g.DrawImage($hat,[int]($left-3),[int]$hatTop,118,118)
 $g.DrawString($files[$i].BaseName,$font,[Drawing.Brushes]::White,[single](($i%4)*240+15),[single]([Math]::Floor($i/4)*270+248))
 $hat.Dispose()
}
$sheet.Save((Join-Path $PSScriptRoot 'preview_EncaixBarrets.png'),[Drawing.Imaging.ImageFormat]::Png)
$font.Dispose();$body.Dispose();$g.Dispose();$sheet.Dispose()
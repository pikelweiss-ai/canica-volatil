# Fulla de revisio de les cares i del contingut nou, sense tocar els sprites.
Add-Type -AssemblyName System.Drawing
$root=Join-Path $PSScriptRoot '../../Assets/Sprites/Pixel'
$files=@(Get-ChildItem (Join-Path $root 'Skins') -Filter '*.png' | Sort-Object Name)
foreach($name in @('Barrets/Astronauta','Barrets/Bruixa','Barrets/Explorador','Barrets/Enginyer','Barrets/Capita','Enemics/Escarabat','Enemics/Esporer','Armes/DiscCometa','Armes/LlancaTempesta','Armes/VaretaEspores','Masmorra/ArbustBaies','Masmorra/BoletLluminós')) {$files+=Get-Item (Join-Path $root ($name+'.png'))}
$columns=6;$stepX=144;$stepY=164
$sheet=[Drawing.Bitmap]::new($columns*$stepX+16,[int][Math]::Ceiling($files.Count/$columns)*$stepY+16)
$graphics=[Drawing.Graphics]::FromImage($sheet)
$graphics.Clear([Drawing.ColorTranslator]::FromHtml('#192631'));$graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor;$graphics.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
$font=[Drawing.Font]::new('Arial',10,[Drawing.FontStyle]::Bold)
for($i=0;$i -lt $files.Count;$i++) {
  $x=8+($i%$columns)*$stepX;$y=8+[int][Math]::Floor($i/$columns)*$stepY
  $sprite=[Drawing.Image]::FromFile($files[$i].FullName)
  $graphics.DrawImage($sprite,[Drawing.Rectangle]::new($x,$y,128,128));$sprite.Dispose()
  $graphics.DrawString($files[$i].BaseName,$font,[Drawing.Brushes]::White,$x,$y+132)
}
$path=Join-Path $PSScriptRoot 'preview_AlphaPersonalitzacio.png';$sheet.Save($path,[Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose();$font.Dispose();$sheet.Dispose();Write-Output $path

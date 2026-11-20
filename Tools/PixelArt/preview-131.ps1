# Contact sheet de les novetats de la v1.3.1 (barrets, armes, enemics i bosses), ampliat.
Add-Type -AssemblyName System.Drawing
$root = Split-Path (Split-Path $PSScriptRoot)
$items = @('Barrets/Pirata','Barrets/Mag','Barrets/Cuiner','Barrets/Gat','Barrets/Cowboy','Barrets/Antenes',
  'Armes/BaculTorreta','Armes/Llancamines','Armes/ArcLlum','Armes/EscutMirall','Armes/VaretaCurativa','Armes/Ganxo',
  'Enemics/Gargola','Enemics/Cranc','Enemics/FantasmaGel','Enemics/ReinaAranya','Enemics/ReiGel',
  'Armes/TorretaCos','Armes/TorretaCano','Armes/MinaCos','Armes/MinaLlum','Armes/CuraBala','Armes/GanxoHarpo')
$cell = 260; $cols = 6; $rows = [int][Math]::Ceiling($items.Count / $cols)
$sheet = [Drawing.Bitmap]::new($cols * $cell, $rows * $cell)
$g = [Drawing.Graphics]::FromImage($sheet); $g.Clear([Drawing.Color]::FromArgb(40, 70, 40))
$g.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::NearestNeighbor; $g.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::Half
$font = [Drawing.Font]::new('Arial', 12)
for ($i = 0; $i -lt $items.Count; $i++) {
  $bmp = [Drawing.Bitmap]::new((Join-Path $root "Assets/Sprites/Pixel/$($items[$i]).png"))
  $s = [Math]::Floor(230 / [Math]::Max($bmp.Width, $bmp.Height))
  $x = ($i % $cols) * $cell + 10; $y = [Math]::Floor($i / $cols) * $cell + 10
  $g.DrawImage($bmp, $x, $y, $bmp.Width * $s, $bmp.Height * $s)
  $g.DrawString($items[$i], $font, [Drawing.Brushes]::White, [single]$x, [single]($y + $cell - 36))
}
$sheet.Save((Join-Path $PSScriptRoot 'preview_131.png'), [Drawing.Imaging.ImageFormat]::Png)

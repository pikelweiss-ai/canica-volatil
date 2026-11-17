# Armes per a Claude — integrades

Ja estan creades a Assets\Sprites\Pixel\Armes amb els noms exactes:
Pistola.png (16×16), Escopeta.png (32×32), SMG.png (24×24), Sniper.png (32×32), Perdigo.png (8×8; dibuix de 3×3).

La pistola manté la silueta compacta i incorpora el mànec de fusta demanat. El sniper conserva el verd oliva i la mira amb h, adaptat al llenç de 32 que demana Claude. L'escopeta té dos canons superposats. L'SMG és de metall amb carregador llarg.

Tools\PixelArt\draw.ps1 carrega automàticament Tools\PixelArt\ArmesClaude\armes-claude.ps1 abans de guardar PNGs. Ja pots executar el generador habitual des de Canica Alpha. La còpia anterior està a Tools\PixelArt\ArmesClaude\draw-abans-armes-claude.ps1.

armes-claude.ps1 conté les graelles amb totes les files iguals i les dos entrades de paleta necessàries per al verd oliva aprovat: A=#89916b i a=#626e4c. Cal passar també estes dos línies a Claude.

draw-armes-claude.ps1 és un generador independent dels cinc sprites. Per executar-lo des de l'arrel del projecte:
.\Tools\PixelArt\ArmesClaude\draw-armes-claude.ps1 -OutRoot Assets\Sprites\Pixel -PreviewDir Tools\PixelArt\ArmesClaude

Les imatges del preview s'amplien cadascuna al seu requadre; no representa les mides relatives dins del joc. Unity importa a 16 PPU.

No s'han creat WeaponData ni modificat escenes o combat. Les versions anteriors es conserven.

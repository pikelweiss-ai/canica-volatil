# Proporcions v3

Pistola compacta: llenç de 16×12 píxels. Sniper allargat: 40×16 píxels. AK-47 de referència: 32×32. La comparació mostra les tres armes al mateix augment (6×), sense estirar-les.

Es manté la densitat de 16 PPU: a igual handScale, la pistola té la meitat de la llargària de l'AK i el sniper és un 25% més llarg. Són proporcions adaptades a la llegibilitat del joc, no una escala física exacta.

Graelles dibuixades a mà amb la paleta existent i el verd oliva de v2. Transparència real i sense antialiasing.

Nous fitxers al projecte:
Assets\Sprites\Pixel\ArmesChatGPT\Pistola_16_v3.png
Assets\Sprites\Pixel\ArmesChatGPT\RiflePrecisio_40_v3.png
Tools\PixelArt\ChatGPT_v3\draw-chatgpt-v3.ps1

Regenerar des de Canica Alpha:
.\Tools\PixelArt\ChatGPT_v3\draw-chatgpt-v3.ps1 -OutRoot Assets\Sprites\Pixel

Es conserven les versions anteriors. No s'han modificat WeaponData, handScale ni escenes. Per usar-los cal assignar els nous sprites a les armes en Unity; mantindre el mateix handScale conserva esta relació de mida.

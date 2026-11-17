# Armes ChatGPT v1 — Canica Volàtil

Quatre sprites originals, fets amb graelles de lletres segons GUIA_CHATGPT.md. PNG RGBA de 32×32, fons transparent, paleta original, contorn k i canó a la dreta. Sense generació d'imatges amb IA: dibuix determinista amb el sistema del projecte.

## Fitxes proposades (pendent de provar i equilibrar)

| Arma | Dany | Tirs/s | Cooldown (s) | Abast (unitats) | Velocitat (unitats/s) | Munició | Raresa |
|---|---:|---:|---:|---:|---:|---|---|
| Pistola | 2 | 3,33 | 0,30 | 6 | 14 | Infinita, sistema actual | Comú |
| Escopeta | 1 per perdigó, 5 perdigons | 1 | 1,00 | 4 | 13 | Infinita, sistema actual | Rar |
| Rifle de precisió | 7 | 0,67 | 1,50 | 11 | 24 | Infinita, sistema actual | Èpic |
| Plasma | 3 | 2,5 | 0,40 | 7 | 10 | Infinita, sistema actual | Llegendari |

L'escopeta necessita implementar la dispersió i múltiples projectils: WeaponData encara no té eixos camps. Les fitxes són propostes, no canvis aplicats al combat. No s'han creat WeaponData, modificat escenes ni afegit armes al loot.

## Fitxers al projecte

- PNGs: C:\Users\Usuario\Canica Alpha\Assets\Sprites\Pixel\ArmesChatGPT
- Graelles, generador i preview: C:\Users\Usuario\Canica Alpha\Tools\PixelArt\ChatGPT_v1
- Unity aplica automàticament Sprite, 16 PPU, Point i sense compressió amb PixelArtImporter.cs. No s'han escrit .meta a mà.

## Regenerar des de l'arrel de Canica Alpha

```powershell
.\Tools\PixelArt\ChatGPT_v1\draw-chatgpt-v1.ps1 -OutRoot Assets\Sprites\Pixel -PreviewDir Tools\PixelArt\ChatGPT_v1
```

El generador és independent i només genera estos quatre sprites. graelles.ps1 conté els blocs $sprites per a copiar-los al generador original si es vol. Cada fila té exactament 32 caràcters. Les graelles es centren verticalment en el llenç de 32×32, igual que l'AK-47.

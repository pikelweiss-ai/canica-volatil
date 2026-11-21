# Índex de documentació i d'art · Canica Volàtil (actualitzat 2026-11-21, camí a la v1.4)

## 1. Documents

| Document | Per a què |
|----------|-----------|
| [README.md](../README.md) | Descàrregues, controls, estat. |
| [AGENTS.md](../AGENTS.md) | Regles de treball per a Codex/ChatGPT. |
| [TRASPAS_CLAUDE_V141.md](../TRASPAS_CLAUDE_V141.md) | **Traspàs actual a Claude Code**: Alpha v1.4.0, proves i pròxima iteració. |
| [TRASPAS_CODEX_V140.md](../TRASPAS_CODEX_V140.md) | Traspàs històric de la implementació de la v1.4.0. |
| [Docs/Dissenys/v1.4-almanac-amulets.md](Dissenys/v1.4-almanac-amulets.md) | Almanac (armes, enemics, bosses, amulets) i 16 amulets. |
| [Docs/Dissenys/v1.4-bosses-enemics-sales.md](Dissenys/v1.4-bosses-enemics-sales.md) | Enemics, microbosses, bosses, sales, portes i sorolls de la v1.4. |
| [DISSENY.md](../DISSENY.md) · [CONSELL.md](../CONSELL.md) | Disseny i balanç. |
| [ONLINE.md](../ONLINE.md) | Joc online i protocol 18 de la v1.4.0. |
| [RELEASE_NOTES.md](../RELEASE_NOTES.md) · [ALPHA_PROGRESS.md](../ALPHA_PROGRESS.md) | Historial de versions i de treball. |
| [MOBILE_ALPHA.md](../MOBILE_ALPHA.md) · [MOBILE_ANDROID.md](../MOBILE_ANDROID.md) | Android i joc entre plataformes. |
| `Docs/Consell/*` | Informes del consell de 19 perfils (v1.2.1). |
| Notes d'Obsidian | `C:\Users\Usuario\memoria-claude\Canica Volàtil - Bugs pendents.md` (idees, decisions, bugs). |

## 2. On és tot l'art nou (v1.4)

Els originals del disseny es troben a **`ArtNou_v1.4/`**. Els recursos del menú i les portades que carrega Unity estan a `Assets/Resources/Pixel/Menu140/`.
Cada carpeta té fulls de contacte `_*.png` per veure'l d'un colp.

| Carpeta | Contingut | Quantitat | Mida | Generador |
|---------|-----------|-----------|------|-----------|
| `ArtNou_v1.4/Blocs/` | Textures de blocs per famílies: Terres, Parets, Natura, Metall, Moderns, Objectes, Troncs, Vegetacio, Escola, Cuina, Bany, Hospital, Oficina, Port, Laboratori | **626** | 16×16 | `Tools/PixelArt/blocs-v14-prova.ps1` |
| `ArtNou_v1.4/Blocs/_Connexions.png`, `_Cami_prova.png` | Proves que les roques i els camins connecten | 2 | — | `Tools/PixelArt/blocs-v14-connexions.ps1` |
| `ArtNou_v1.4/Textures_base_Ferran/` | Les 9 textures originals de Ferran + variants tintades (`Proves/`) | 9 + 54 | 16×16 | `Tools/PixelArt/tinta-textures.ps1` |
| `ArtNou_v1.4/Enemics/Sprites/` | 12 enemics nous | 12 | 16×16 | `Tools/PixelArt/enemics-v14.ps1` |
| `ArtNou_v1.4/Microbosses/Sprites/` | 4 microbosses | 4 | 24×24 | `Tools/PixelArt/microbosses-v14.ps1` |
| `ArtNou_v1.4/Bosses/Sprites/` | 8 bosses | 8 | 32×32 | `Tools/PixelArt/bosses-v14.ps1` |
| `ArtNou_v1.4/Portes/H` i `/V` | 9 tipus de porta × 3-4 estats (horitzontal i vertical) | 32 + 32 | 16×16 | `Tools/PixelArt/portes-v14.ps1` |
| `ArtNou_v1.4/Portes_simples/H` i `/V` | 5 portes de fulla única (classe, habitació, dormitori, bany, despatx) i 3 corredisses; esquerra/dreta, amunt/avall | 78 + 78 | 16×16 | `Tools/PixelArt/portes-simples-v14.ps1` |
| `ArtNou_v1.4/Amulets/Icones` | 16 amulets (Comú, Rar, Èpic, Llegendari) | 16 | 16×16 | `Tools/PixelArt/amulets-v14.ps1` |
| `ArtNou_v1.4/Sorolls/` | 23 efectes de so | 23 | WAV 22.050 Hz mono | `Tools/PixelArt/sorolls-v14.cs.txt` (C# via `Add-Type`) |

**Art ja dins del joc** (no tocar): `Assets/Sprites/Pixel/{Alpha,Armes,Barrets,Cofres,Enemics,Items,Masmorra,Skins}`, més `Assets/Sprites/*.png` (aigua, gespa, etc.).
**Art per editar** (fora del repositori): `C:\Users\Usuario\Canica Alpha\Builds\Art per editar\` (menú, botons, referències de mapes, còpies de treball). La carpeta d'origen de Ferran, `Desktop\Texturas Unreleased Canica Volatil`, **no es modifica mai**.

## 3. Convencions d'art
- Blocs 16×16, armes 32 px, 16 PPU; contorn fosc `#1C1A24` en enemics i bosses; fons transparent en objectes.
- Cada sprite té 5 posicions possibles (`_centre`, `_NO`, `_NE`, `_SO`, `_SE`) quan és un objecte xicotet; les peces llargues usen `_esq/_mig/_dre`.
- Rotació: el mode creador rotarà els blocs (0/90/180/270). Els camins, tuberies i baranes tenen **un sol cantó** que es gira.

## 4. Eines
`Tools/Build/build-alpha.ps1`, `check-crossplay.ps1`, `package-alpha.ps1`, `Tools/Net/test-online.ps1`, `Tools/Release/package-linux.*`, i tots els `Tools/PixelArt/*.ps1`.

## 5. Dissenys addicionals (2026-11-21)
- [Mercader i màquina expenedora](Dissenys/v1.4-mercader.md) · sprites a `ArtNou_v1.4/Mercader` (`Tools/PixelArt/mercader-v14.ps1`).
- [Almanac i amulets](Dissenys/v1.4-almanac-amulets.md): §4 què és cada amulet, §5 HUD amb 3 ranures d'amulets.
- [Nova pantalla PERSONALITZAR](Dissenys/v1.4-personalitzar.md) (estil Stumble Guys: Canigues, Barrets, Esteles) · mockups a `Docs/Dissenys/img/` (`Tools/PixelArt/mockup-personalitzar.ps1`).

- [Full de ruta](ROADMAP.md): què entra a la v1.4.0, v1.4.1 i v1.5. [Traspàs a Claude Code](../TRASPAS_CLAUDE_V141.md).

# Canica Volàtil — Document de disseny

> Direcció de l'Alpha de PC actualitzada el 18/11/2026: consultar [ALPHA_PLAN.md](ALPHA_PLAN.md), basat en la nova entrevista amb Ferran. La tenda, la col·lecció persistent i el cooperatiu per LAN/codi passen a ser prioritats. Els apartats d'aquest document són històrics quan entren en conflicte amb el pla nou.

> Resultat de l'entrevista del 16/11/2026. Projecte de treball: `Canica Alpha`. L'original `Canica 1` no es toca.

## 1. Idea general
Joc 2D top-down amb combat **estil Brawl Stars**: apuntes amb el ratolí, hotbar d'armes (tecles 1-4) i puny quan la casella està buida. Té dos grans modes:

- **Masmorra per sales** (PvE, en solitari o cooperativa online)
- **PvP online**

## 2. Flux de pantalles
```
Menú principal ──► tries partida
   ├─ Partida offline  ──► Mapa de baralla / Masmorra
   └─ Partida online   ──► Lobby (esperes que entren tots) ──► Mapa de baralla
```
- **Lobby** = el mapa inicial d'ara (aigua, arbres, maons). Està guardat com a `Assets/Scenes/Lobby.unity`. `GameScene` queda com a escena de proves.
- La partida es tria **des del menú**, no dins del lobby.

## 3. Modes de joc
| Mode | Tipus | Estat |
|---|---|---|
| Masmorra per sales | PvE, solitari o cooperatiu online | Prioritari |
| Tots contra tots | PvP online | Planificat |
| Per equips (2v2 / 3v3) | PvP online | Planificat |
| Battle royale | PvP online | Planificat (més endavant) |

Més modes: *"ja vorem"*.

## 4. Online
- **Al principi:** sales amb **codi** (un crea la sala i els altres entren amb el codi).
- **En un futur llunyà:** matchmaking automàtic.
- Proposta tècnica (per confirmar): **Netcode for GameObjects** + **Unity Relay/Lobby**. Relay ja funciona amb *join codes*, així que encaixa amb el sistema de codi de sala.

## 5. Masmorra
- Sales **fetes a mà** amb la Tile Palette, que es **combinen en ordre aleatori** en cada partida.
- Temes de mapa: **Desert**, **Neu / gel**, **Cova / masmorra**, **Volcà / lava**.
  - Idees de mecàniques per tema (per confirmar): el gel rellisca, la neu alenteix, la lava fa mal i la cova és fosca amb llum 2D.
- Al final de cada masmorra o tema hi ha un **boss**.

## 6. Enemics
| Tipus | Comportament |
|---|---|
| Cos a cos | Et persegueix i et pega (com l'Enemy actual) |
| Tirador | Es queda a distància i et dispara projectils |
| Kamikaze | Corre cap a tu i explota |
| Boss | Un de gran per tema o masmorra |

## 7. Armes i cofres
- **Cofres al mapa** que donen armes aleatòries.
- **Raresa d'armes** amb colors: comú, rara, èpica i llegendària.
- Ja fet: `WeaponData` (Create → Canica → Arma), `Projectile`, hotbar 1-4, puny i aim amb barra de cooldown.
- No triat (de moment, no): munició, botiga al lobby.

## 8. Estil gràfic
- **Píxel art de deveres**: imatges a la resolució real (p. ex. 16×16 o 32×32) amb **Filter Mode: Point**, **Compression: None** i **Pixels Per Unit = mida del tile**. No imatges grans que imiten píxels.
- Ara els tiles són de 1024×1024 i "imiten" píxels.
- **Blocs/tiles: sempre 16×16**, per a poder conservar les textures originals del Ferran.
- **Densitat: 16 píxels = 1 unitat** de Unity. Tot el que es pose dins d'una carpeta `Pixel/` s'importa sol amb aquesta configuració (`Assets/Editor/PixelArtImporter.cs`).
- **Armes: 32 px** (triat amb l'AK-47; les de 16 i 24 es queden de referència a `Assets/Sprites/Pixel/Armes`). A 16 PPU, una arma de 32 px fa 2 blocs de llarg, estil Enter the Gungeon.
- Items de cura (16×16): poció de vida, farmaciola, poma i bena (`Assets/Sprites/Pixel/Items`).
- **Cofres**: model A de fusta, de 12×10 píxels (més xicotet que un bloc), tancat i obert, en 4 rareses: comú (ferro), rar (blau), èpic (morat) i llegendari (or amb gemma). Estan a `Assets/Sprites/Pixel/Cofres`. El baül (B) i el de metall (C) es guarden a `Cofres/Dungeon` per al mode masmorra.
- Els sprites es generen amb `Tools/PixelArt/draw.ps1`, a partir de graelles de lletres i una paleta.
- Textures actuals: **refer i comparar**. Es fan versions noves i el Ferran tria quines li agraden més.
- Fan falta moltes textures: un tileset per cada tema, enemics, armes, cofres i efectes.

## 9. Ordre de treball
1. **Textures** ← *ara*
   - Fer el mateix tile en 16, 24 i 32 per a decidir la resolució
   - Refer gespa, aigua, maons, arbre i canica, i comparar-les amb les d'ara
   - Tileset del primer tema
2. **Masmorra offline**: sales fetes a mà, generació per ordre aleatori, portes i neteja de sala
3. **Enemics nous**: tirador, kamikaze i boss
4. **Cofres i raresa d'armes**
5. **Online**: sala amb codi, lobby i PvP
6. Més endavant: battle royale i matchmaking

## 10. Preguntes obertes
- Resolució del píxel art (16 / 24 / 32)
- Quin tema fem primer
- Quantes sales té una masmorra
- Vida i mort en cooperatiu (reviure companys?)
- Què passa amb les monedes si no hi ha botiga

# Traspàs · després de l'Alpha v1.4.1

**21 de novembre de 2026.** La v1.4.1 està publicada: [Alpha v1.4.1](https://github.com/pikelweiss-ai/canica-volatil/releases/tag/v1.4.1-alpha). Treballa a `C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace`, branca `alpha/arsenal` (igual que `main`). Llig [AGENTS.md](AGENTS.md), [Docs/ROADMAP.md](Docs/ROADMAP.md) i [Docs/INDEX.md](Docs/INDEX.md). No toques «Canica 1». Els canvis locals de Ferran (fonts, `.meta`, ProjectSettings i URP) no van als commits; afig fitxers de manera explícita. Per a publicar una altra versió, espera el permís de Ferran.

## Què hi ha

- Protocol **19**. Contingut nou: 6 enemics (`EnemyV140`), 4 microbosses (`MicrobossBar`), `HazardZone` (foc, àcid, bomba, teranyina), mercader i màquina (`Shop`, `ShopUI`), Personalitzar (`CustomizeMenuUI`), Almanac (`Almanac.cs`) i varietat de blocs (`DungeonScenery.cs`).
- Contingut generat: `Canica > Contingut v1.4.1` (`Assets/Editor/V141ContentBuilder.cs`). Els bosses es defineixen a `AlphaContentBuilder.cs`.
- Bots: `-canicaBot menushot | dungeon140 | solo | v141 | scenery` (captures amb `-botShots`). El test de dispersió de l'Escopeta del bot `solo` pot fallar si es mou el ratolí real.

## Pendent i proves

- Provar en Android i Linux reals, i la suite online (`Tools/Net/test-online.ps1 -MobileClient`) amb el protocol 19, també en cooperatiu amb persones (botiga, microbosses, teranyines, escut).
- Balanç per provar jugant: atacs nous del Rei de Gel, microbosses, preus de la botiga, DPS de l'escut, bosses amb +20 de vida i només cures.
- El detector d'encallament dels bosses no s'ha provat a la sala real amb les columnes dels racons.
- **El Talp va rar en online** (apuntat per Ferran amb la v1.4.2; detalls i pistes a les notes de `memoria-claude`, «Bugs pendents»).
- Les portes amb clau, la Banshee que fa mal als companys i l'empenta als clients continuen pendents.

## v1.5 (full de ruta)

Catàleg de blocs i mode creador de mapes, amulets (3 ranures), tenda de cosmètics amb qualitats, Almanac fase 2 (animacions) i els vuit bosses nous. Part de la varietat de blocs que estava prevista per a la v1.5 ja és a la v1.4.1: la resta (catàleg amb col·lisió i propietats, paleta i editor) continua pendent.

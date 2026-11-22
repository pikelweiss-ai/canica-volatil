# Traspàs · continuació del Creador de mapes v1.6

**Estat a 22/11/2026.** Branca `alpha/arsenal`, worktree `C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace`. La v1.5.0-alpha està publicada i és Latest (`6675b44`). La v1.6.0-alpha és només local; no s'ha fet push ni release.

## Fet i validat

Commits locals de les fases inicials:

- `f72beb8` — catàleg de 626 fitxes i validador `catalog160` (inclou `_esq/_mig/_dre`).
- `b768424` — `MapData`, validació i `MapStore`.
- `3dc44fe` — escena Creador, targeta activa i eines base.
- `b8828b5` — punt inicial, marcadors d'enemic/portal i prova local en graella.
- `4a45276` — autotile de camins, helper de hitbox rotada i superposició de capes.
- `b022a8e` — kits simples i copiar/enganxar regions.
- `69768c8` — compartir `.canicamap` i codi comprimit `CVM1:`.

Canvis posteriors encara pendents de commit: versió visible/package 1.6.0-alpha, Changelog, builder Android amb codi 17, cubell de farciment corregit, línia Bresenham, càmera WASD/zoom, visibilitat de capes, autosave recuperable cada minut, avisos de sobreescriptura/eixida/mapa nou, desfés també els marcadors, i actualització de documentació. Revisa `git status` i no afiges al commit el soroll local de Ferran.

Validació a `Builds/Candidats/Alpha-v1.6.0-final`:

- Windows, Linux i Android: `ALPHA_BUILD_OK`.
- `check-crossplay.ps1`: `CROSSPLAY_MANIFEST: PASS`, protocol 24, versió 1.6.0-alpha, 29 armes i 4 cures.
- Bots `catalog160`, `mapdata160`, `editor160`, `tile160`, `share160`: `NETBOT PASS`.
- `test-online.ps1 -MobileClient`: 6/6 per al joc online existent. No és prova de mapes creats online.
- No s'han provat executables en PC Linux físic ni APK en mòbil físic.

## Pendents abans de dir que el Creador està complet

- La vista PROVAR encara és una graella local amb marcadors: no instancia enemics, cofres ni portal com a sistemes de joc reals.
- Online de mapes: D2 permet passar-ho a v1.7 si s'allarga. No hi ha protocol 25 ni transmissió en chunks; el protocol es queda en 24.
- Autotile limitat als camins `Natura/Gespa_cami_*`; falta cobrir la resta de famílies del disseny i fer ombres/col·lisions rotades en el món.
- Els kits són habitacions de 9×9 amb paret perimetral i terra; falta provar/afinar el contingut temàtic i les plantes.
- L'editor encara no té pintura d'arrossegament ni selecció/copiat de multipart com a objecte; la prova Android de jugar mapes importats està pendent.
- Revisar el rendiment en un mapa 64×64 i fer QA visual en 16:9/4:3.

No publicar ni pujar el repositori del joc sense permís explícit de Ferran. Les notes d'Obsidian s'han actualitzat després de `git pull --rebase` i el commit `a37eee1` ja està pujat al repositori de notes.

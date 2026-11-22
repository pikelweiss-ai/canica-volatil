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
- `32fe0e7` — tancament documental i builds finals de la v1.6.
- `d088973` — els blocs desconeguts es conserven i es representen amb «?» en editar/provar.
- `ce773c4`, `8de74ab` — rectangles plens/buits i comprovació dels dos modes al `editor160`.
- `98a0ff1` — pintura amb arrossegament i interpolació de cel·les.
- `44c257f` — llista seleccionable de mapes guardats amb nom, dimensions i data.
- `40bdd24` — autotile de camins, panells i canonades, amb comprovació de peces de cantonada/T.
- `61944f7` — el jugador de PROVAR no travessa blocs marcats com a sòlids.
- `27f6c9e` — spawn de mercader en l'editor, validació i marcador en PROVAR.
- `08f2c67` — PROVAR reutilitza l'escena real de masmorra i genera el mapa en Tilemaps, amb jugador, enemics, cofres, botiga i final.
- `abffbf5` — selecció d'enemics (catàleg complet, inclosos bosses) i raresa de cofres.
- `a249d11` — el final usa el portal animat real i comprova la victòria sense canviar de pis.
- `3cbc88b` — hitboxes dels semiblocs aplicades en el món de prova, amb la mateixa rotació horària i flip que el sprite.
- `be69cf5` — kits de Castell, Casa i Supermercat amb decoració/plantes pròpies i validació dels tres al bot.
- `50ff33c` — eines per a girar/voltejar blocs col·locats i panoràmica amb botó del mig.
- `5fc1ccc`, `7678f14` — autotile de catifes de bany i correcció de les baranes; `tile160` prova les cantonades.
- `efb12e4`, `d109b2e` — importar codi CVM1 des del selector per a jugar en Android/PC, netejant el mapa temporal després de carregar-lo; `share160` prova el flux real.
- `6d14909` — quadrícula opcional i visualització de hitboxes per a depurar; `editor160` ho valida.
- `9d243c9` — filtres de favorits i recents de la paleta, amb persistència local; `editor160` ho valida.
- `a271e2f` — ruta automàtica de blocs a la capa segons `BlockData.layer` i `fullBlock`.
- `18e57ce` — dreceres de teclat desactivades mentre s'editen camps de text.
- `7bc6567` — col·locar, copiar, girar, voltejar i esborrar mobles multipart com un sol objecte; `editor160` ho valida amb un grup de tres peces.
- `451e514` — calcula les cel·les visibles una vegada per redibuix de l'editor, en lloc de repetir-ho per cada cel·la.

La versió visible/package 1.6.0-alpha, el Changelog, el codi Android 17, el cubell, la línia Bresenham, la càmera WASD/zoom, la visibilitat de capes, l'autosave recuperable, els avisos de confirmació i el desfés dels marcadors ja es van incloure en el tancament de la v1.6. La versió del worktree pot continuar diferint del número de release perquè la v1.6 no s'ha publicat.

Validació a `Builds/Candidats/Alpha-v1.6.0-final`:

- Windows, Linux i Android: `ALPHA_BUILD_OK`.
- `check-crossplay.ps1`: `CROSSPLAY_MANIFEST: PASS`, protocol 24, versió 1.6.0-alpha, 29 armes i 4 cures.
- Bots `catalog160`, `mapdata160`, `editor160`, `tile160`, `share160`: `NETBOT PASS` en la revisió més recent. `catalog160` comprova 626 fitxes, 15 famílies, hitboxes, grups multiparts i sufixos `_esq/_mig/_dre`; `mapdata160` comprova també que els ids de bloc desconeguts es preserven; `editor160` comprova rectangles ple/buit, selecció d'enemics i cofres, i una prova dins de `Dungeon` amb jugador (vida/atac), enemic, cofre, mercader, portal, victòria i retorn.
- Passada final del candidat Windows: `catalog160`, `mapdata160`, `editor160`, `tile160` i `share160` donen `NETBOT PASS`. Després del canvi multipart, `editor160` ha tornat a donar `NETBOT PASS`, incloent col·locació, còpia/enganxat, rotació, volteig i esborrat d'un grup de tres peces.
- Builds Windows, Linux i Android posteriors al canvi multipart: `ALPHA_BUILD_OK`. `check-crossplay.ps1`: `CROSSPLAY_MANIFEST: PASS`, protocol 24, paquet 1.6.0-alpha, 29 armes i 4 cures; Android bundle code 17.
- `test-online.ps1 -MobileClient`: 6/6 en la validació anterior de les sales ja existents. No és prova de mapes creats online, que es deixa a v1.7.
- No s'han provat executables en PC Linux físic ni APK en mòbil físic. El selector i el flux d'importació s'han comprovat amb el bot Windows, no en un telèfon.
- Després de `451e514`, Windows, Linux i Android tornen a donar `ALPHA_BUILD_OK`; els manifests coincideixen (`CROSSPLAY_MANIFEST: PASS`) i `editor160` dona `NETBOT PASS`. La prova de rendiment 64×64 amb gràfics actius s'ha fet després amb `perf160` (detall més avall).
- Tancament de la fase 5: `catalog160`, `tile160` i `editor160` donen `NETBOT PASS`; `tile160` valida les 144 màscares de roca/aigua i `editor160` valida l'ombra d'una paret girada. Windows, Linux i Android donen `ALPHA_BUILD_OK`, amb protocol 24 i manifest 1.6.0-alpha compatible.
- Després del tancament de fase 5, el commit `ab87215` registra les 144 variants i la validació de l'ombra. He tornat a executar `mapdata160` i `share160` en el candidat Windows: tots dos `NETBOT PASS`; els logs de build ja existents indiquen Windows/Linux/Android `Succeeded; errors=0`. No s'ha fet push ni release.
- Per a complir la mida màxima real, el botó `NOU MAPA` deixa triar amplària/alçària d'1 a 64. `editor160` comprova 4096 cel·les a 64×64. Els `.canicamap` s'han canviat a gzip (màxim 200 KB comprimides i 1 MB en descomprimir, amb compatibilitat amb JSON pla); `mapdata160` i `share160` ho validen.
- `perf160`, executat amb finestra/gràfics actius en el PC de prova a 1600×900, va dibuixar un mapa ple de 64×64 en 585 ms i va mesurar una mitjana de 219,9 FPS durant 10 s; el frame més lent equival a 60 FPS. La captura del segon intent amb 1200×900 va continuar eixint a 1600×900, així que la QA visual 4:3 continua pendent.

## Pendents abans de dir que el Creador està complet

- PROVAR ara reutilitza el jugador i els sistemes reals del joc, i dibuixa els blocs en Tilemaps. El bot valida els actors i la victòria, però encara falta una prova manual de combat/interacció amb cofre i mercader, i provar mapes grans en dispositiu real.
- Online de mapes: D2 permet passar-ho a v1.7 si s'allarga. No hi ha protocol 25 ni transmissió en chunks; el protocol es queda en 24.
- Autotile ara inclou camins, panells, canonades, baranes, catifes de bany i 144 variants direccionals per als sis sprites de roca i tres d'aigua. `tile160` prova les 16 màscares de cada sprite. `BlockShadow` copia la transformació del bloc al shadow tile; `editor160` ho verifica amb un bloc girat al món real de prova. Les hitboxes dels semiblocs giren i es voltegen al món de prova.
- Els tres kits ja col·loquen decoració temàtica (seto/flors, planta interior, carro/expositor de fruita) i `editor160` ho valida. Queda pendent QA visual manual de disposició i espais de pas.
- L'editor gestiona els grups multipart en col·locar, copiar/enganxar, rotar, voltejar i esborrar; `editor160` valida un grup representatiu de tres peces. La pintura amb arrossegament i la llista d'obertura també estan implementades.
- L'índex de cel·les elimina cerques repetides de veïns i s'usa a l'editor, preview i món real. `perf160` mesura 239,2 FPS de mitjana amb 64×64 ple; falta QA visual en 16:9/4:3 i dispositius físics.
- `MapStore` conserva ids de blocs desconeguts, que es dibuixen amb «?»; no hi ha encara migració automàtica d'un mapa per a formats futurs.

No publicar ni pujar el repositori del joc sense permís explícit de Ferran. Les notes d'Obsidian s'han actualitzat després de `git pull --rebase` i el commit `a37eee1` ja està pujat al repositori de notes.

## Estat en la pausa del 2026-11-22 (WIP, per a Claude Code)

Ferran ha hagut de marxar d'urgència i ha demanat pujar la branca a GitHub com a còpia de seguretat. **No és una release**: cap etiqueta, `main` sense tocar.

- Branca `alpha/arsenal`. Els canvis d'esta part són a `Assets/Scripts/MapEditor.cs`, `MapStore.cs`, `MapShare160.cs`, `Assets/Scripts/Net/NetBotEditor160.cs`, `NetBotMapData160.cs` i `NetBot.cs`.
- La compilació Windows va acabar bé, però **l'última execució del bot `editor160` falla al final** amb «el cubell no ha omplit la zona buida connectada». **Encara no s'ha investigat.** També falta revisar a ull l'últim ajust de la paleta i continuar les fases pendents.
- No s'han inclòs en el commit (soroll de reimportació d'Unity de Ferran): fonts, `.meta`, prefabs d'enemics, `UniversalRP`, `ProjectSettings`, `Creador.unity`, `MapaProva.unity` i les previsualitzacions de `Tools/PixelArt`.
- Fase 8 (online de mapes) passada a la v1.7. Pendents apuntats per a la v1.6.1: `Docs/ROADMAP.md`, secció v1.6.1.

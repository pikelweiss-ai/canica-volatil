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

La versió visible/package 1.6.0-alpha, el Changelog, el codi Android 17, el cubell, la línia Bresenham, la càmera WASD/zoom, la visibilitat de capes, l'autosave recuperable, els avisos de confirmació i el desfés dels marcadors ja es van incloure en el tancament de la v1.6. La versió del worktree pot continuar diferint del número de release perquè la v1.6 no s'ha publicat.

Validació a `Builds/Candidats/Alpha-v1.6.0-final`:

- Windows, Linux i Android: `ALPHA_BUILD_OK`.
- `check-crossplay.ps1`: `CROSSPLAY_MANIFEST: PASS`, protocol 24, versió 1.6.0-alpha, 29 armes i 4 cures.
- Bots `catalog160`, `mapdata160`, `editor160`, `tile160`, `share160`: `NETBOT PASS` en la revisió més recent. `catalog160` comprova 626 fitxes, 15 famílies, hitboxes, grups multiparts i sufixos `_esq/_mig/_dre`; `mapdata160` comprova també que els ids de bloc desconeguts es preserven; `editor160` comprova rectangles ple/buit, selecció d'enemics i cofres, i una prova dins de `Dungeon` amb jugador (vida/atac), enemic, cofre, mercader, portal, victòria i retorn.
- Passada final del candidat Windows: `catalog160`, `mapdata160`, `editor160`, `tile160` i `share160` donen `NETBOT PASS`. Després del canvi multipart, `editor160` ha tornat a donar `NETBOT PASS`, incloent col·locació, còpia/enganxat, rotació, volteig i esborrat d'un grup de tres peces.
- Builds Windows, Linux i Android posteriors al canvi multipart: `ALPHA_BUILD_OK`. `check-crossplay.ps1`: `CROSSPLAY_MANIFEST: PASS`, protocol 24, paquet 1.6.0-alpha, 29 armes i 4 cures; Android bundle code 17.
- `test-online.ps1 -MobileClient`: 6/6 en la validació anterior de les sales ja existents. No és prova de mapes creats online, que es deixa a v1.7.
- No s'han provat executables en PC Linux físic ni APK en mòbil físic. El selector i el flux d'importació s'han comprovat amb el bot Windows, no en un telèfon.

## Pendents abans de dir que el Creador està complet

- PROVAR ara reutilitza el jugador i els sistemes reals del joc, i dibuixa els blocs en Tilemaps. El bot valida els actors i la victòria, però encara falta una prova manual de combat/interacció amb cofre i mercader, i provar mapes grans en dispositiu real.
- Online de mapes: D2 permet passar-ho a v1.7 si s'allarga. No hi ha protocol 25 ni transmissió en chunks; el protocol es queda en 24.
- Autotile ara inclou camins, panells, canonades, baranes i catifes de bany. Les textures de roques de cova i d'aigua del catàleg no tenen peces direccionals per a formar connexions; falta una regla visual adequada si s'han d'autotilejar. La rotació d'ombres no està implementada/provada. Les hitboxes dels semiblocs giren i es voltegen al món de prova.
- Els tres kits ja col·loquen decoració temàtica (seto/flors, planta interior, carro/expositor de fruita) i `editor160` ho valida. Queda pendent QA visual manual de disposició i espais de pas.
- L'editor gestiona els grups multipart en col·locar, copiar/enganxar, rotar, voltejar i esborrar; `editor160` valida un grup representatiu de tres peces. La pintura amb arrossegament i la llista d'obertura també estan implementades.
- Revisar rendiment en mapes 64×64 i fer QA visual en 16:9/4:3; els botons de cel·la del canvas no s'han perfilat amb gràfics actius.
- `MapStore` conserva ids de blocs desconeguts, que es dibuixen amb «?»; no hi ha encara migració automàtica d'un mapa per a formats futurs.

No publicar ni pujar el repositori del joc sense permís explícit de Ferran. Les notes d'Obsidian s'han actualitzat després de `git pull --rebase` i el commit `a37eee1` ja està pujat al repositori de notes.

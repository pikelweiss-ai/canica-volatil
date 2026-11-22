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

La versió visible/package 1.6.0-alpha, el Changelog, el codi Android 17, el cubell, la línia Bresenham, la càmera WASD/zoom, la visibilitat de capes, l'autosave recuperable, els avisos de confirmació i el desfés dels marcadors ja es van incloure en el tancament de la v1.6. La versió del worktree pot continuar diferint del número de release perquè la v1.6 no s'ha publicat.

Validació a `Builds/Candidats/Alpha-v1.6.0-final`:

- Windows, Linux i Android: `ALPHA_BUILD_OK`.
- `check-crossplay.ps1`: `CROSSPLAY_MANIFEST: PASS`, protocol 24, versió 1.6.0-alpha, 29 armes i 4 cures.
- Bots `catalog160`, `mapdata160`, `editor160`, `tile160`, `share160`: `NETBOT PASS` en la revisió més recent. `catalog160` comprova 626 fitxes, 15 famílies, hitboxes, grups multiparts i sufixos `_esq/_mig/_dre`; `mapdata160` comprova també que els ids de bloc desconeguts es preserven; `editor160` comprova rectangles ple/buit, selecció d'enemics i cofres, i una prova dins de `Dungeon` amb jugador (vida/atac), enemic, cofre, mercader, portal, victòria i retorn.
- `test-online.ps1 -MobileClient`: 6/6 per al joc online existent. No és prova de mapes creats online.
- No s'han provat executables en PC Linux físic ni APK en mòbil físic.

## Pendents abans de dir que el Creador està complet

- PROVAR ara reutilitza el jugador i els sistemes reals del joc, i dibuixa els blocs en Tilemaps. El bot valida els actors i la victòria, però encara falta una prova manual de combat/interacció amb cofre i mercader, i provar mapes grans en dispositiu real.
- Online de mapes: D2 permet passar-ho a v1.7 si s'allarga. No hi ha protocol 25 ni transmissió en chunks; el protocol es queda en 24.
- Autotile ara inclou `Natura/Gespa_cami_*`, `Oficina/Panell_*` i `Laboratori/Tuberia_*`; falten catifes, roques de cova, aigua i baranes connectades, així com ombres/col·lisions rotades en el món.
- Els kits són habitacions de 9×9 amb paret perimetral i terra; falta provar/afinar el contingut temàtic i les plantes.
- L'editor encara no té selecció/copiat de multipart com a objecte; la pintura amb arrossegament i la llista d'obertura ja estan implementades. La prova Android de jugar mapes importats està pendent.
- Revisar el rendiment en un mapa 64×64 i fer QA visual en 16:9/4:3.
- `MapStore` conserva ids de blocs desconeguts, que es dibuixen amb «?»; no hi ha encara migració automàtica d'un mapa per a formats futurs.

No publicar ni pujar el repositori del joc sense permís explícit de Ferran. Les notes d'Obsidian s'han actualitzat després de `git pull --rebase` i el commit `a37eee1` ja està pujat al repositori de notes.

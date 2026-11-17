Estat de publicacio actual: consultar RELEASE_NOTES.md i GITHUB_PREPARACIO.md. Les seccions seguents registren el desenvolupament anterior.

# Canica Volatil — candidata Pre-Alpha v1.0.0

Estat: desenvolupament local, pendent de revisio de Ferran i de prova completa abans de publicar.

## Novetats
- Selector d'expedicions amb descripcions i controls. Lobby online desactivat i marcat com a proximament.
- Taller: 16 opcions de skin inclosa la classica, 9 opcions de barret inclos Cap, combinacio aleatoria.
- Espasa de vidre (comuna), sabre de corall (rar) i martell orbital (epic). Colps en arc, empenta i efecte visual; un colp per objectiu, comprovacio de visibilitat.
- Sols menys sorollosos i murs amb coronament mes clar als huit temes; ratpenat i aranya redibuixats.
- Pausa amb Esc, continuar i acabar expedicio; el temps es restaura en eixir.
- El cofre busca un costat lliure allunyat del jugador per deixar el loot. Si tots estan ocupats, conserva la posicio anterior.
- Menus d'Editor a Canica > Pre-Alpha per preparar build Windows i comprovar armes en Play.
- Dificultat del pis 1 sense canvis.

## Prova abans de publicar
1. MainMenu > Jugar: targetes llegibles, Lobby desactivat, Tornar i Esc.
2. Personalitzar: recorrer les 16 skins i els 9 barrets, provar Sorpren-me i reobrir per comprovar persistencia.
3. Masmorra: caminar, apuntar, cofres, inventari, deixar armes, cures i rareses.
4. Esc: enemics i projectils quiets; reprendre; acabar expedicio i tornar al menu.
5. Provar les tres armes noves: diversos enemics davant, enemics darrere, parets, caixes i barrils. El martell es mes lent.
6. Boss, portal i segon pis: conservar vida, monedes, armes i cures. Morir i comprovar reinici.
7. Provar els huit temes i diverses resolucions de pantalla.
8. Crear build local amb Canica > Pre-Alpha > Crear build Windows v1.0.0; executar-la fora de Unity. No publicar nomes amb la prova d'Editor.

## GitHub quan Ferran done el vistiplau
- Conservar l'historial local; copia inicial verificada en .git/codex-backups/abans-github-2026-11-17.bundle.
- Revisar diff i secrets; commits delimitats, incloent els .meta generats per Unity.
- Repositori privat buit, connectar origin i pujar main sense force.
- Etiqueta proposada: v1.0.0-prealpha. Nom de release: Pre-Alpha v1.0.0. Crear-la despres de validar l'executable.
- Adjuntar tota la carpeta de build en ZIP, no nomes l'exe. Builds esta ignorada per Git.
- No hi ha remot, etiqueta ni release creats en esta fase.

## Fonts i identitat
Referencia consultada: https://www.nuclearthrone.com/ (varietat de partides i arsenal).
Tot el pixel art nou esta escrit en graelles propies a Tools/PixelArt/prealpha.ps1.
No s'han descarregat assets de tercers.
## Verificacio feta per Codex (17/11/2026)
- Unity ha recompilat els scripts i ha generat els assets amb el menu Canica.
- Play: MainMenu > selector > Dungeon, pausa amb Esc i retorn al menu comprovats.
- Taller ampliat obert en Play; ajustats textos per evitar desbordaments observats.
- Prova de combat en Play executada: les tres armes fan el dany configurat davant i no fan dany darrere. Missatge de confirmacio en la Console.
- Console final: un missatge de prova, zero errors i zero avisos en la captura final.
- git diff --check sense errors.
- Encara no provats: partida completa, boss/portal, tots els temes, equilibri de les armes noves, tots els casos de loot i build standalone. La build no s'ha creat ni publicat.
## Segona passada: radar, controls i cosmètics
- Radar de terreny centrat en el jugador, nord fix, exploracio persistent durant el pis i dues escales amb M (reassignable).
- Controls al menu principal i a la pausa: 19 accions, dues tecles per accio, intercanvi de duplicats, restauracio i persistencia PlayerPrefs. Esc cancella una captura; Retroces lleva una assignacio.
- Roda: armes, cures, armes amb Maj+roda per a cures, o desactivada. Seleccionar cura no la consumeix: H (reassignable) usa la seleccionada; les tecles directes continuen usant cada casella.
- Indicadors de tecles a la hotbar i cofres actualitzats des de les assignacions.
- Barrets encaixats per la base opaca, tant en partida com al taller. Aureola amb separacio expressa. Lamina revisada: Tools/PixelArt/preview_EncaixBarrets.png.
- Total: 22 skins inclosa la classica, 13 opcions de barret inclos Cap, 12 esteles inclosa Cap.
- Minigun redibuixada amb tres canons i motor; 0,6 segons de gir abans de disparar. Soltar l'atac o canviar d'arma reinicia la carrega.
- Sabre de corall: dash curt limitat amb Rigidbody2D.Cast; espasa de vidre: sentit altern del tall; martell orbital: arc de 360 graus.
- Unity: recompilacio, generacio d'assets, UI de controls, reassignacio d'una cura i restauracio comprovades. Radar i ampliacio oberts en Play; prova dels tres atacs superada incloent dany darrere del martell circular.
- Pendents de prova de joc mes extensa: carrega sostinguda de minigun, dash contra tots els obstacles, combinacions de controls i rodes en combat, canvi de pis, i build standalone.
Verificacio final de la segona passada: zoom del radar ajustat i les dues vistes obertes en Play; indicador de cura seleccionada visible. Unity ha recompilat la versio final i la captura final de Console no mostra errors ni avisos. Unity queda fora de Play. Encara no hi ha build standalone ni publicacio.

Ajust final de minimapa: mida compacta original 240x190, fons transparent i terreny semitransparent. Zoom automatic per enquadrar el pis complet mantenint el jugador centrat; zones no visitades mes tenues. M conserva la vista ampliada. Canvi revisat amb git diff --check; no s'ha interromput la partida activa de Ferran per provar esta ultima modificacio.

## Ajustos i menu de partida
- Mapa compacte de 200x158 aproximadament per defecte; mida configurable de 140 a 360 i abast de 1x a 2x. Guardat automatic en PlayerPrefs.
- SettingsUI substitueix les rutes antigues d'Ajustos sense editar escenes. La pantalla antiga de nom deixa de ser accessible des d'Ajustos; el camp PlayerName passa al taller Personalitzar. Conserva noms existents; els nous noms es guarden en acabar l'edicio, amb limit de 24 caracters.
- GameSettings i GameAudio gestionen master i categories Weapons/Enemies/Effects/Music. AudioListener aplica el master una vegada; fonts i reproduccions independents per categoria. Fonts ja existents conserven el seu volum relatiu. Sons d'enemics, armes, cofres, monedes, cures i recollides connectats; fonts en bucle del nivell es classifiquen com a musica/ambient.
- GameplayMenu s'afig al jugador mitjancant PlayerHazards: tres barres dalt a la dreta, pausa, continuar, ajustos, controls i eixir al menu. Disponible a Dungeon i GameScene. El radar queda davall del boto.
- Verificat en Unity: compilacio; pantalla de nom visible al taller; obrir ajustos des del menu principal; canviar volum d'armes independentment; canviar mida del mapa; obrir el menu de tres barres en Dungeon, entrar a ajustos i tornar a la pausa. Console final sense errors rojos. No s'ha fet una prova auditiva de mescla completa ni build standalone.
# Alpha de PC · primera passada visual i ajustos

Branca `alpha/pc-foundation`, compilació de treball **Alpha v1.0.0**. Encara no s'ha publicat com a release.

## Revisió de les anotacions de Ferran

La identificació visible i el paquet passen a **Alpha v1.0.0** (`1.0.0-alpha`). És la branca de desenvolupament; no s'ha publicat cap release nova.

- Logo i il·lustració nous, dibuixats per codi; detalls i procedència en `ART_NOTES.md`.
- Fons de caverna com una única imatge que segueix la càmera, sense tiled ni col·lisió.
- Murs més foscos amb coronament clar, diferenciats del paviment.
- Càmera en LateUpdate i interpolació de Rigidbody2D del jugador/enemics, conservant el pas i les velocitats de física.
- V-Sync i límits d'FPS en Pantalla, incloent 240; amb V-Sync el refresc depén del monitor.
- Pestanya Interfície: vida i monedes independents en quatre posicions, amb persistència i restabliment. Provades sobre la hotbar.
- Finestra limitada a l'àrea útil del monitor, amb marge per decoració i barra del sistema; centrat en aplicar. Provada una petició de 2560×1440 que s'ajusta a 2346×1320 de contingut.

Windows i Linux recompilats sense errors; comprovacions de mida/aspecte de finestra, art i opcions de 240 FPS superades. Prova Windows: menú, masmorra, parets, fons, reubicació del HUD i confirmació de finestra gran. Registre sense excepcions. No s'ha mesurat el rendiment sostingut a 240 FPS ni executat la compilació Linux.

## Implementat

- Menú amb els sprites originals de gespa, rajola, fusta, arbres i canica.
- Font principal Liberation Sans, predeterminada de TextMesh Pro, en negreta i amb ombra SDF compartida. Minecraft només al comptador de monedes.
- Ajustos agrupats en Partida, So i Pantalla.
- Cura seleccionada amb clic dret per defecte. Es conserven les assignacions personalitzades; es poden restablir en Controls.
- Finestra redimensionable, pantalla completa sense vores i drecera F11.
- Resolucions sense duplicats, limitades a dimensions que caben al monitor, incloent 4:3 i nativa.
- Confirmació de pantalla durant 15 segons: Intro manté, Esc o temps esgotat restaura. Només es guarden els canvis confirmats.
- Canvas adaptats per veure tota la interfície en diferents proporcions.
- Lava i àcid amb 47 variants connectades cadascun, conservant dany i col·lisions.
- Nous sòls/murs per als huit ambients, materials originals en bosc/cripta i fons del buit per ambient.

## Verificat

`AlphaVisualBuilder.Validate` comprova connexions en Tilemap, actualització de vores, dany i col·lisions. `AlphaSettingsChecks.Validate` comprova fonts UI/món, negreta, ombra, comptador, cura inicial, resolucions i escalat.

Windows i Linux han compilat amb zero errors. Windows fora de l'Editor: menú, entrada a masmorra, comptador i ajuda de cura; canvi a pantalla completa, restauració automàtica, confirmació de resolució i F11 d'anada/tornada en partida. Revisió en 1280×720 i 1024×768; corregit el retall lateral dels Canvas. Registre final sense excepcions del joc.

Linux s'ha compilat, però no s'ha executat en Mint. El clic dret s'ha verificat com a assignació inicial i en la ruta de controls existent; falta provar el consum d'una cura amb vida baixa en una partida completa.

## Compilar i revisar

Unity 6000.0.53f1. `AlphaReviewBuild.BuildWindows` regenera els visuals i executa les comprovacions; `AlphaReviewBuild.BuildLinux` valida abans de compilar.

Eixides: `Builds/AlphaReview/Windows/CanicaAlphaReview.exe` i `Builds/AlphaReview/Linux/CanicaAlphaReview.x86_64`. Mantindre l'executable amb tota la seua carpeta. La compilació Windows de revisió rep el pegat oficial Unity Application Patcher 1.3.3 després de compilar.

Pendents de l'Alpha: xarxa LAN/codi, noves sales funcionals, peces parcials i portes interactives, perfil local/estadístiques, tenda i modes. Comptes independents i sincronització entre equips: Beta. Vegeu `ALPHA_PLAN.md`.

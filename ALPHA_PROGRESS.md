# Canica Volàtil · Progrés de l'Alpha

## v1.6.0 · desenvolupament local (2026-11-22)

- Fases 1–7 tenen increments amb commits `f72beb8`, `b768424`, `3dc44fe`, `b8828b5`, `4a45276`, `b022a8e`, `69768c8`; els bots `catalog160`, `mapdata160`, `editor160`, `tile160` i `share160` donen **NETBOT PASS**. Alguns criteris del disseny continuen pendents; vegeu [TRASPAS_V161.md](TRASPAS_V161.md).
- Windows batch build ha passat en cada fase. La fase 4 prova els marcadors jugador/enemic/portal en una vista local; encara no crea enemics ni cofres funcionals de combat. El test de mapa està en vista separada i no és encara la partida cooperativa normal.
- Fase 8 online traslladada a v1.7 segons D2. Windows/Linux/Android compilen i manifests passen; suite online existent 6/6. Notes d'Obsidian i traspàs actualitzats. v1.6 no està publicada ni s'ha fet push. Queden funcions del disseny pendents; llista a `TRASPAS_V161.md`.

## v1.5.0 · 21 de novembre de 2026

- Fase 0. Arreglos i microbosses verificats amb `phase0`, `dungeon140`, `v141` i `scenery`. Prova dedicada de Talp, Mímic, Torreta, mercader i paret trencable en cooperatiu.
- Fase 1. `bosses150` insta els huit bosses, executa tots els atacs configurats i captura cada boss. La prova LAN veu la barra de vida i els avisos d'atac.
- Fase 2. `keys150` comprova la porta, la recollida i el consum de la clau. En cooperatiu, el client obri la porta a tots dos processos.
- Fase 3. `amulets150` comprova 16 efectes, tria i límit de ranures. La prova cooperativa comprova les ranures independents, la tria i el descart validats per l'amfitrió.
- Fase 4. `store150` comprova compres en tres categories, les tries antigues i la sala Botiga en Windows i vista mòbil.
- Fase 5. `almanac150` comprova catàleg, filtres, descoberta i targeta Creador en Windows i vista mòbil. `menushot` també passa.
- Fase 6. Windows, Linux i Android han compilat sense errors. `check-crossplay.ps1` dona PASS amb el protocol 24, 29 armes, 4 cures i el mateix hash de fonts. L'APK porta el codi de versió 16 i conté ARM64 i ARMv7. En Windows, la candidata supera `solo`, `dungeon140`, `phase0`, `bosses150`, `keys150`, `amulets150`, `menushot` i `almanac150`. Android i Linux continuen pendents de prova en equips físics.
- Online de la candidata. 6/6 amb client mòbil simulat, directori local i túnel real. Amb el directori públic, 4/6 per la quota dels anuncis sense IP. [Detall de la prova](ONLINE.md).

## v1.4.2 · 21 de novembre de 2026

- Teranyines de racó que frenen i es poden trencar (`CobwebSlow`). Protocol 19, igual que la v1.4.1.

## v1.4.1 · 21 de novembre de 2026

- Publicada a `main` amb l'etiqueta `v1.4.1-alpha`; protocol **19**. Contingut: 6 enemics nous (`EnemyV140`: Salamandra, Llimac àcid, Escorpí, Llop de neu, Bombardier, Torreta antiga), 4 microbosses amb `MicrobossBar` (Escarabat Daurat, Ull Flotant, Granota Gegant, Mag de Cendra), `HazardZone` (foc, àcid, bomba), mercader i màquina expenedora (`Shop`, `ShopUI`), pantalla Personalitzar nova (`CustomizeMenuUI`) i Almanac fase 1 (`Almanac.cs`).
- Arreglos i balanç de Ferran: mira sota barret/arma, nom de la hotbar, seguiment suau de càmera, Bàcul i Llançamines (3 càrregues), gel rar des del pis 3, Vareta curativa només en cooperatiu, SMG/Polsos/Martell/Espasa/Arc/Ganxo, Mode Creatiu (sense enemic solt, TREURE COFRES, roda), Talp sense tirs fantasma.
- Contingut generat per `Canica > Contingut v1.4.1` (`Assets/Editor/V141ContentBuilder.cs`).
- Afegit després: masmorres amb varietat de blocs (`DungeonScenery.cs`, 94 textures a `Assets/Resources/Pixel/Blocs`), bosses +20 de vida i cures, Reina Aranya amb teranyines, Rei de Gel amb subdits i boles que frenen, Escut de mirall amb energia i detector d'encallament dels bosses. Bots `menushot`, `dungeon140`, `solo`, `v141` i `scenery` en PASS.

## v1.4.0 · 21 de novembre de 2026

- [Publicació i notes](https://github.com/pikelweiss-ai/canica-volatil/releases/tag/v1.4.0-alpha) amb paquets Windows, Linux i Android. Protocol 18 i manifests coincidents.
- Menú, planta de masmorra i bot individual complet superats en Windows. Suite online amb client mòbil simulat **6/6**. ZIP executat després d'extraure'l, APK verificat i sumes SHA256 comprovades.
- [Traspàs a Claude Code](TRASPAS_CLAUDE_V141.md) amb les proves pendents i el contingut planificat de la v1.4.1.

## Alpha de PC · primera passada visual i ajustos

Estat vigent a la branca `alpha/bugfixes` (19/11/2026):

- L’última prova de Windows, `Builds/AlphaReview/Windows-cares-classiques/CanicaAlphaReview.exe`, ha compilat amb zero errors. Les 11 skins marcades recuperen les textures anteriors. Capvespre coincidix amb la textura aprovada. Altres 11 porten la cara de la Clàssica. Coure i Caramel conserven la cara, i Gota té una silueta més perfilada. Catàleg a `Builds/CaresClassiquesQA/Cataleg.png`.
- El cel original del mapa lliure cobrix la càmera amb el zoom de la Ballesta en formats ample i estret. Prova automatitzada superada.
- **Alpha v1.1.1 publicada (Latest).** Protocol 8. Online per Internet **sense IP** (túnel `bore.pub` + directori `ntfy.sh`, `Assets/Scripts/Net/NetCloud.cs`). `test-online.ps1` passa 6/6, inclosos els dos casos sense IP pels serveis reals. **Prova real entre dues cases superada** (Ferran i un amic). Pendent: rework de la connexió (retard del túnel) i Linux real. Vegeu [ONLINE.md](ONLINE.md).
- Mode Creatiu del mapa lliure (FIX-009): armes, menjar, enemics, cofres, i botons de treure enemics i objectes. Vegeu [DISSENY.md](DISSENY.md).

Les seccions següents documenten revisions anteriors.

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

## 18 novembre 2026 — traspas pendent de revisio

Ferran para el desenvolupament per continuar amb Claude. Llegir TRASPAS_CLAUDE_ALPHA.md en aquest worktree. Implementats fix fullscreen/HUD, cares diferents, 3 skins, 4 barrets, 6 esteles amb formes diferents, 3 armes, 2 enemics i 2 props. Ultima compilacio Windows correcta amb zero errors (alpha-personality-final-windows.log); comprovacions Editor superades. Falta prova runtime dels canvis, recompilar Linux i reaplicar pegat Windows. Canvis locals sense commit sobre 5f91db6; no release nova.

## 18 novembre 2026 — sales online (Claude)

Menu de sales online implementat: JUGAR > targeta Online. Navegador estil HaxBall amb pestanyes Internet/LAN, cerca, filtres, ping i cadenat; unir-se per codi de 6 lletres o IP:port; crear sala (masmorra o mapa lliure, 2-8 jugadors, publica/privada, contrasenya opcional, LAN o Internet). Lobby amb jugadors movent-se, llista LLEST/amfitrio, xat amb bafarades, codi copiable, mode triat per l'amfitrio i compte enrere quan tots estan llestos. En partida es veuen els altres i la masmorra es la mateixa (llavor de la sala); l'amfitrio pot tornar tots al lobby. Servidor de sales = el mateix joc amb -canicaServer (relay, sense obrir ports). Scripts a Assets/Scripts/Net; detalls a ONLINE.md. Proves automatiques Tools/Net/test-online.ps1: 4 escenaris passen (LAN llista, LAN codi, Internet llista, Internet codi). Captures amb Tools/Net/shots-online.ps1. Pendent: enemics/dany/cofres sincronitzats (cadascu lluita els seus), proves amb dos PCs reals, Linux i servidor en AWS. Sense commit.

## 18 novembre 2026 — cooperatiu online (Claude)

Ferran ha provat les sales amb dues finestres i funcionen. Afegit cooperatiu amb amfitrio autoritari (Assets/Scripts/Net/NetCoop.cs): enemics creats i moguts per l'amfitrio amb copies als clients, colps dels clients enviats a l'amfitrio, dany als companys, sales/portes/cofres de recompensa/portal compartits, cofres d'un sol us, pas de pis conjunt, caure i tornar en netejar la sala, bales/talls/punys visibles als altres, i avatars amb ombra, estela, barret i arma a la ma. Panell de sala en partida mogut perque no tape "Pis 1". Proves test-online.ps1: 4 escenaris passen, 2 amb masmorra cooperativa (LAN i Internet). Sense commit.

## 18 novembre 2026 — cooperatiu complet (Claude)

Sales que nomes es tanquen amb tots els jugadors vius dins. Espectar un company en caure (A/D o clic per triar); tornar en netejar la sala o als 4 s si has caigut fora de combat. Monedes, cures i armes del terra compartides (el primer que les agafa, amb arbitratge de l'amfitrio). Caixes i TNT trencades per a tots, sense doble dany. Barra de vida i avisos del boss als clients, color i mida dels enemics copiats. Ombres dels enemics comprovades (fill Ombra del prefab). Arreglat F11 durant la transicio de pantalla. Windows i Linux compilats sense errors; test-online.ps1 passa 2 vegades seguides (4/4). Sense commit.

## 18 novembre 2026 — revisio de Ferran amb dues finestres (Claude)

Arreglat: el boto de mode d'Ajustos > Pantalla ara aplica la pantalla completa en clicar (abans nomes canviava l'opcio i calia APLICAR). Monedes d'enemics: l'iman va cap al jugador mes proxim (abans l'amfitrio les robava sempre); monedes animades del mapa d'un sol us. En caure es solten armes i cures (no monedes) i es reapareix sense res al pis seguent (vida plena) si els companys passen. Radar amb els companys. Proves test-online.ps1 4/4 dues vegades, Windows i Linux compilats. Sense commit.

## 18 novembre 2026 — traspas a Codex (online)

Ferran passa l'online a Codex. Llegir TRASPAS_CODEX_ONLINE.md. Bugs oberts: cures de cofre/boss que no es veuen a tots els PCs, boto de maximitzar la finestra, portes de sala obertes mentre s'espera el company (es pot entrar al boss). Tot sense commit sobre 5f91db6.

## 18 novembre 2026 — correccio dels tres bugs online (Codex)

Cures remotes resoltes amb HealItem.All, sense dependre dels assets ja carregats. Cataleg explicít NetContent per a totes les armes, generat per NetContentBuilder en les dues compilacions. Maximitzacio nativa respectada: Update ja no redimensiona els canvis de finestra de Windows; les peticions del menu continuen ajustades a l'area util. Portes en espera: l'amfitrio tanca totes menys l'entrada, conserva l'estat per resincronitzar i tanca l'entrada quan hi son tots els vius. Si tots retrocedeixen, cancel·la l'espera. Jugadors caiguts exclosos del recompte de posicions. Protocol 4 (mateix build per host/client/relay).

Windows i Linux recompilats amb zero errors: Builds/ReleaseLogs/alpha-online-fixes-windows.log i alpha-online-fixes-linux.log en el projecte principal. Pegat oficial de Windows comprovat (183, ja aplicat). Proves Tools/Net/test-online.ps1 ampliades: 4/4, logs a %TEMP%/canica-online-fixes. Inclou cures rebudes en fred, totes les armes resoltes, cura de cofre client->host, cura de boss host->client i portes en espera als dos processos. Probabilitats de botí forçades nomes en el bot. Sense excepcions del joc en aquests logs (avisos de shaders esperables amb -nographics).

Control del PC: botons natius restaurar/maximitzar i doble clic de titol correctes; finestra es mante maximitzada. F11 entrar/eixir confirmat amb Intro. Linux no executat en Mint; dos PCs reals i servidor remot continuen pendents. Tambe falta prova especifica de retirada de tots, entrada inicial del client i resincronitzacio durant l'espera. ONLINE.md i TRASPAS_CODEX_ONLINE.md actualitzats. Codi del joc encara SENSE COMMIT sobre 5f91db6; no release publicada ni canvis en Canica 1/main.

## 18 novembre 2026 — projectils, interaccions i Alpha Android compartida (Codex)

- Projectils reals i visuals ignoren els colliders de companys, incloent el jugador local del receptor. Ja no desapareix el tret nomes en una pantalla.
- Cofres i objectes trencables de sales de combat/boss bloquejats fins a netejar la sala (inclou espera i onades). Inicial/tresor lliures. El client demana cada colp de caixa/TNT a l'amfitrio; nomes el host resta vida i replica destruccio. Cofres validats tambe al host.
- Protocol 5, mateix codi a Android/Windows/Linux. Catalegs complets i manifest compatibility.json compartit: 21 armes, 4 cures, sourceSha256 b429526bc2ab613ec381f6e543e011905de86d04cfc982467c5097f100031149. Tools/Build/check-crossplay.ps1 PASS.
- Android integrat al projecte Alpha actual (no copia de la branca Pre-Alpha): dos joysticks, hotbar tactil, cures, ajuda, zona segura, pausa, xat ENVIA i espectador amb botons. Textos tactils i vida separada de la fila de cures revisats en Windows --mobile-preview. Eixir al menu tanca la sala online. Permisos de xarxa i MulticastLock Wi-Fi al paquet Android.
- Compilacions finals Android, Windows i Linux amb zero errors. Logs Builds/BuildLogs/{Android,Windows,Linux}-build.log en aquest worktree. Preparacio del sistema Input en sessio separada via Tools/Build/build-alpha.ps1 evita incompatibilitat de serialitzacio URP; al final activeInputHandler=2 per PC. Android IL2CPP ARM64/ARMv7, API26+, versionCode3, package com.pikelweiss.canicavolatil.mobile, versionName1.0.0-alpha.
- APK final Builds/AlphaReview/Android/CanicaVolatil-Alpha-v1.0.0-Android.apk (61.887.261 bytes), pegat oficial Unity1.3.3 aplicat exit0, alineacio i signatures v2/v3 verificades. SHA256 c4b3fe3b25cea5c20d68af3d084f610d7bc37cdff42ebaa65f6c0f9f6356f263. Original sense pegat nomes a Builds/BuildLogs/Android-unpatched.apk. Windows comprovat ja parchejat. Linux inclou Jugar.sh: executar bash Jugar.sh.
- Proves online amb -MobileClient passen 4/4 dues vegades (la segona amb build final). Logs finals Builds/Crossplay FinalTests. Comproven projectils travessant jugador, cofres/caixes bloquejats pendent/combat i desbloqueig en netejar, colps de client autoritzats, cures, portes i cooperatiu anterior. MobileInputChecks passa 7 comprovacions de dos dits/analogic/pausa.

Limits reals: la variant de bot mobil corre en Windows, no en Android. ADB sense dispositius i WSL/Linux no disponibles: NO partida real Android + Windows + Linux, ni multitàctil fisic, ni Mint, ni relay remot. Compatibilitat de protocol/codi/catalegs comprovada; queda revisio en dispositius. No publicar release ni donar per provat el que falta. Codi del joc sense commit; Canica 1 i fonts del projecte principal intactes. Guia MOBILE_ALPHA.md i ONLINE.md actualitzades.

## 18 novembre 2026 — hotfix TNT durant combat

Ferran detecta que les TNT nomes es trenquen despres de la baralla, tambe en Android. Causa: compartien el bloqueig de cofres/caixes. Nova regla DungeonGenerator.CanDamageBreakableAt: explosius disponibles quan la sala esta activa, protegits en espera; objectes normals i cofres mantenen el desbloqueig despres de netejar. Breakable.Hit i NetShared.BreakableHit usen la mateixa regla, tambe al host autoritari. Barril.prefab te explodes=1; no s'han modificat prefabs.

Android/Windows/Linux recompilats amb zero errors, mateix protocol5 i sourceSha256 23005f5bf5099b7b4a2b32ff1fbcfd1a54b11919cd03918afb4bc12bb4fc396c. check-crossplay.ps1 PASS. test-online.ps1 -MobileClient PASS4/4 (Builds/TntCombatTests); prova regla i colp real del host: TNT intacta durant espera, resta vida en combat, cofres/caixes encara bloquejats. Sense excepcions als logs. No prova fisica en telefon/Mint.

APK actualitzada al mateix cami Builds/AlphaReview/Android/CanicaVolatil-Alpha-v1.0.0-Android.apk, 61883165 bytes. Pegat Unity aplicat i signatures v2/v3/zipalign verificats. SHA256 f59dea45bbe92097e0555de48f807c791d3bb0a14550338ed0ab318c12974b3c. Windows comprovat ja parchejat. Logs compilacio Builds/BuildLogs/*-build.log; pegats *-tnt-patch.log. ONLINE.md actualitzat. Tot el codi segueix sense commit, sense release nova; main/Canica1 intactes.

## 18 novembre 2026 — candidata oficial Alpha v1.0.0

Ferran autoritza revisio final i publicacio oficial amb Windows, Ubuntu/Linux i Android. Corregit un cas de carrega tardana: NetShared.Resync reenvia objectes del terra que encara existeixen; el client recupera cures perdudes i evita duplicats. Prova nova de perdua i resincronitzacio repetida superada en LAN i relay. Suite test-online.ps1 -MobileClient PASS4/4 (Builds/AlphaReleaseTests), sense excepcions de joc. Comprovacions Editor/compilacions dels tres sistemes: zero errors, mateix protocol5, 21 armes, 4 cures i sourceSha256 d49d1f40294a0f18fce9d4a70180b43da8b5b8722e7e89f1499b6557947fe7d0.

Paquets de distribucio en Builds/Distribucio/Alpha-v1.0.0-release: Windows ZIP, Ubuntu/Linux TAR.GZ, Android APK i SHA256SUMS.txt; BUILD_INFO.json identifica commit i manifests. Windows/Android amb pegat Unity1.3.3; APK v2/v3 i alineacio verificades. ZIP sense errors CRC, dependencies i dades comprovades; TAR amb executable i llancador755, sense carpetes DoNotShip. Executables i dades reanomenats CanicaVolatil per distribucio. README i notes actualitzats per Alpha/online, sense enllacos antics ni afirmacions de proves no fetes.

La publicacio usa v1.0.0-alpha i conserva Pre-Alpha PC/Android. No s'ha executat en Ubuntu/Mint ni en Android fisic des d'aquest entorn; no partida conjunta dels tres sistemes ni relay remot. Cap credencial de signatura o servidor dins del Git. Les fonts/metadades/cache generades que ja estaven modificades queden preservades localment; no reset/clean/stash, ni canvis a Canica1 o a les quatre fonts del projecte principal.

Verificacio del paquet Windows reanomenat: host i client cooperatius complets PASS (Builds/AlphaPackageSmoke). Les tres sumes SHA256 de distribucio, CRC ZIP i permisos TAR verificats. Sense excepcions als logs.


## 19 novembre 2026 — Alpha v1.1.0 i v1.1.1 (Claude)

- **v1.1.0** (`v1.1.0-alpha`): mode Creatiu al mapa lliure, arreglos de la llista de bugs i online en proves. `main` avançat amb fast-forward. Pegat Unity 1.3.3 a Windows i Android. Empaquetador sense logs ni temporals, sumes amb LF i TAR amb Python quan no hi ha Node.
- **v1.1.1** (`v1.1.1-alpha`, Latest): crear i entrar a sales d'Internet sense escriure cap IP.
  - L'amfitrió obri un túnel d'eixida a `bore.pub` i anuncia la sala a `ntfy.sh` cada 20 s (en tancar, `closed`). Els clients la trien de la llista o la busquen pel codi, amb 5 intents perquè el directori tarda uns segons a servir els anuncis nous.
  - Sense ports, tallafocs, VPN ni servidor. La IP de l'amfitrió no es publica. Protocol 8 sense canvis.
  - Proves: `test-online.ps1` 6/6, smoke amb l'executable empaquetat i captures reals (`Builds/OnlineShots/v111-sense-ip`). **Partida real entre dues cases superada.**
- Pendent: rework de la connexió (ping estimat d'uns 300 ms pel túnel) i Linux real. Arreglo del lobby d'Internet (text tapat) en local, commit `295dbf9`, per a la pròxima versió.

## 21 novembre 2026 — preparació de la v1.4 (Claude)

- Sense canvis de codi de joc. Preparat el material per a la v1.4: **626 textures de blocs** (famílies Terres, Parets, Natura, Metall, Moderns, Objectes, Troncs, Vegetació, Escola, Cuina, Bany, Hospital, Oficina, Port i Laboratori), **12 enemics** (16×16), **4 microbosses** (24×24), **8 bosses** (32×32), **32 portes** amb estats (tancada, mitjana, oberta, bloquejada) i **23 sorolls** sintetitzats.
- Tot a `ArtNou_v1.4/` (fora d'`Assets`, sense importar). Generadors a `Tools/PixelArt/*-v14*.ps1` i `sorolls-v14.cs.txt`.
- Disseny revisat per a la v1.4: `Docs/Dissenys/v1.4-bosses-enemics-sales.md`, amb la regla de Ferran «enemics originals i no repetits» (canvis: Salamandra amb colp i fugida, Escorpí amb escut frontal, Torreta en graella, Cuc de Sorra que travessa la sala, Rei de les Clavegueres sense àcid).
- Índex a `Docs/INDEX.md` i traspàs a `TRASPAS_CODEX_V140.md` perquè Codex/ChatGPT continue la v1.4 (generació de sales, sorolls, enemics, microbosses i portes).
- **No provat dins del joc**: els sorolls i el art són fitxers solts. Sense commit.

- (21 nov, final) Documentacio final de la v1.4.0: Docs/ROADMAP.md, TRASPAS_CODEX_V140.md (5 fases amb criteris d'acceptacio) i PROMPT_CODEX_V140.md. Decidit amb Ferran: la v1.4.0 porta dungeons, sorolls, 6 enemics, portes i Llancaflames sense combustible; l'us dels blocs va a la v1.5. Tot sense codi de joc.

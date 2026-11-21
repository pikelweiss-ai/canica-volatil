# Traspàs a Codex/ChatGPT · Alpha v1.5

> Escrit per Claude Code el 2026-11-21, **llest per a fer la versió**. Continua des d'ací: **no comences de zero i no refaces el que ja està fet.** El missatge per enganxar a ChatGPT és a `PROMPT_CODEX_V150.md`.

## 1. Regles que no es poden saltar
- Parla en **valencià informal**. Ferran (17 anys) està aprenent: explica pas a pas les mecàniques noves.
- **Mai toques «Canica 1».** Treballa només en este worktree: `C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace`, branca `alpha/arsenal`.
- **No facis `git push` ni publiques la release sense el permís explícit de Ferran** en la conversa. Primer ell prova el joc. El permís de publicar una versió no val per a la següent.
- No guardes mai contrasenyes, tokens ni claus.
- Notes d'Obsidian: `C:\Users\Usuario\memoria-claude` (`git pull --rebase` abans de llegir; `git add -A`, `commit` i `push` després d'editar). Si hi ha conflicte, explica'l i pregunta.
- Unity **6000.0.53f1**, URP 2D, **Input Manager antic**. El món usa `TextMeshPro`; el Canvas, `TextMeshProUGUI`. Mira el codi abans d'assumir-ho.
- Contingut generat (armes, enemics, bosses, cosmètics): canvia `Assets/Editor/AlphaContentBuilder.cs`, `V141ContentBuilder.cs` i `DungeonBuilder.cs`, **no els `.asset` ni els prefabs**: els builders els reescriuen a cada compilació (menú **Canica**). Un valor posat només al prefab es perd.
- **Mai** `reset --hard`, `clean` ni `stash` global: el worktree té canvis de Ferran (fonts, `.meta` reimportats, `ProjectSettings`, `UniversalRP`). No els incloguis als commits; afig fitxers de manera explícita.
- No edites `.unity`/prefabs a mà si Unity té canvis pendents. No crees `.meta` a mà.
- Commits menuts, missatge en valencià sense accents. **Tanca l'Editor abans de compilar en mode batch.**

## 2. Lectura obligatòria (en este ordre)
1. `AGENTS.md` · 2. `Docs/INDEX.md` · 3. `Docs/ROADMAP.md` (secció v1.5) · 4. `Docs/PENDENTS_APUNTATS.md` · 5. `Docs/Dissenys/v1.5-almanac-blocs-i-mode-creador.md` · 6. `Docs/Dissenys/v1.4-almanac-amulets.md` · 7. `Docs/Dissenys/v1.4-bosses-enemics-sales.md` (§3 bosses, §5 i §8 portes) · 8. `Docs/Dissenys/v1.4-personalitzar.md` · 9. `Docs/Dissenys/v1.4-mercader.md` · 10. `ONLINE.md` · 11. `TRASPAS_V150.md` (estat de la v1.4.2).

## 3. Estat actual
- **Publicada: Alpha v1.4.2** (`1.4.2-alpha`), a `pikelweiss-ai/canica-volatil`. `NetProtocol.Version = 19`. `main` i `alpha/arsenal` apunten al commit de la v1.4.2 (més documentació local sense pujar).
- Codi de la v1.4.1 i la v1.4.2: `EnemyV140.cs` (enemics i microbosses), `HazardZone.cs`, `MicrobossBar.cs`, `Shop.cs` i `ShopUI.cs`, `CustomizeMenuUI.cs`, `Almanac.cs`, `Dungeon/DungeonScenery.cs`, `EnemyBoss.cs` (atacs de bosses), `PlayerAttack.Special.cs` (Escut, Bàcul, Mines).
- Bots: `-canicaBot menushot | dungeon140 | solo | v141 | scenery` (captures amb `-botShots <carpeta>`); `Tools/Net/test-online.ps1 -MobileClient` per a online. El test de dispersió de l'Escopeta del bot `solo` pot fallar si es mou el ratolí real.

## 4. Contingut de la v1.5 (tancat; **no hi afigues res sense preguntar a Ferran**)

Fes les fases **en ordre**, amb compilació i prova al final de cada una, i diu a Ferran què has comprovat i què no.

### Fase 0 · Els deu pendents (`Docs/PENDENTS_APUNTATS.md`)
Preus del mercader i de la màquina, Sorpresa sense icona, xat del mercader (només bafarada), **text de la sala secreta fora i paret esquerdada trencable també amb bales**, Creatiu (TREURE ENEMICS ha de llevar-los tots), troncs tombats amb hitbox sencera, **microbosses des del pis 2 i un per pis (també a la Neu)**, i els tres d'online (Talp, Torreta i aigua, Mímic a la botiga). **Acceptació:** cada punt té una prova (bot o captura) i la documentació s'actualitza.

### Fase 1 · Els vuit bosses nous, tots de colp
Cor de Cristall, Cavaller Corrupte, Cuc de Sorra, Fènix, Hidra del Llac, **Rei de les Clavegueres**, Cuiner Boig i Professor Infernal (`v1.4-bosses-enemics-sales.md` §3: vida, mecànica única i atacs nous `Burrow`, `Sweep`, `Beam`, `Shockwave`, `Bomb`, `Hazard`). Art a `ArtNou_v1.4/Bosses/Sprites/` (32×32): **només hi ha 7 sprites; falta el del Rei de les Clavegueres**. Pregunta a Ferran si el dibuixa ell o si en fas un provisional. Assigna cada boss a un o més temes (`V141ContentBuilder` o `AlphaContentBuilder`), amb la vida base del disseny **més `EnemyBoss.BonusHealth`** (20) i les regles de vida de `DungeonGenerator.SpawnEnemy`. Els bosses humorístics (Cuiner Boig, Professor Infernal) van a sales temàtiques. Cada boss: 2 fases (al 50 %), barra gran, sons (`boss_rugit`, `boss_fase2`), **detector d'encallament** (`EnemyBoss.CheckStuck`) i funcionament online (atacs a l'amfitrió, `Telegraph` i `BulletFx`). **Acceptació:** un bot instancia cada boss, comprova que fa tots els atacs sense errors i fa una captura; online, el client veu avisos i barra.

### Fase 2 · Portes amb clau
**La porta de la sala del boss està tancada amb clau, i la clau s'ha d'anar a buscar a una sala aleatòria de la planta** (`v1.4-bosses-enemics-sales.md` §5 i §8; sprites `ArtNou_v1.4/Portes/` amb estat `bloquejada` amb cadenat). Cal: una sala (o cofre o enemic) que té la clau; item «Clau» (recollir, mostrar a la HUD, gastar en obrir); **un jugador que la porta obri la porta per a tots**; el mapa marca la porta amb clau; i **missatges online nous** (protocol 20). **Acceptació:** un bot recull la clau i obri la porta del boss, i online ho veuen els dos.

### Fase 3 · Amulets (`v1.4-almanac-amulets.md` §2, §4 i §5)
Els 16 amulets (icones a `ArtNou_v1.4/Amulets/Icones/`), **3 ranures** a l'esquerra de la hotbar, tria **1 de 3** en acabar cada pis, **no es dupliquen**, es poden descartar (amb confirmació), **efectes aplicats per l'amfitrió** i sincronitzats online. Cada jugador té els seus; **Tab** mostra els de tothom. Només l'**Ull de falcó** toca el zoom. Compra de l'amulet al mercader. **Acceptació:** una prova per amulet (efecte numèric comprovat) i una de cooperatiu.

### Fase 4 · Tenda de cosmètics amb qualitats (`v1.4-personalitzar.md` §6)
Comprar canigues, barrets i esteles amb les monedes guardades (`CoinBank`); qualitats Rar, Èpic i Llegendari amb la banda de color a la pantalla Personalitzar; apartats nous (colors, expressions); la pantalla **Personalitzar** manté les claus de `PlayerPrefs` (`Cos_Skin`, `Cos_Hat`, `Cos_Trail`); sala **Botiga** completa. **Acceptació:** comprar desbloqueja, es guarda al dispositiu i no trenca les tries existents.

### Fase 5 · Almanac fase 2, Almanac de blocs i 4t mode «Creador» (pròximament)
Tot al disseny `Docs/Dissenys/v1.5-almanac-blocs-i-mode-creador.md`: pestanya **BLOCS** de l'Almanac amb catàleg `BlockData`, filtres per tema, rareza i família, animacions, amulets a l'Almanac, i la **4a targeta «CREADOR»** al selector, apagada i amb «PRÒXIMAMENT». **No fas l'editor ni guardar o carregar mapes** (v1.6). Els objectes tenen **hitbox pròpia i independent per bloc** (decidit). **Acceptació:** els criteris de la secció 6 d'eixe disseny.

### Fase 6 · Tancament
Versió `1.5.0-alpha` (`GameVersion.cs`), entrada a `Changelog.cs` (Novetats), `NetProtocol.Version` actualitzat amb cada missatge online nou i `ONLINE.md`, notes de `memoria-claude`, documentació (`README`, `RELEASE_NOTES`, `ALPHA_PROGRESS`, `MOBILE_ALPHA`, `GITHUB_PREPARACIO`, `Docs/INDEX.md`, `Docs/ROADMAP.md`) i un traspàs nou.

## 5. Com provar
- Compila en mode batch (Editor tancat): `./Tools/Build/build-alpha.ps1 -Target Windows -BuildRoot "Builds\Candidats\Alpha-v1.5.0-<data>"` (el mateix `BuildRoot` per a Windows, Linux i Android).
- Bots: `<BuildRoot>\Windows\CanicaAlphaReview.exe -canicaBot <rol> -botShots <carpeta> -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -logFile <log>`. Rols: `menushot`, `dungeon140`, `solo`, `v141` i `scenery`. Afig'n de nous per a cada fase (a `Assets/Scripts/Net/NetBotV141.cs` hi ha el patró). Han de donar `NETBOT PASS`.
- Online: `./Tools/Net/test-online.ps1 -Exe <exe> -MobileClient` ha de donar **6/6**.

## 6. Com publicar (només amb permís explícit de Ferran)
1. Compila les tres plataformes amb el mateix `BuildRoot`.
2. **Pegats oficials de Unity** (CVE-2025-59489), amb `C:\Users\Usuario\Canica Alpha\Builds\SecurityPatch\Tool\UnityApplicationPatcherCLI.exe`:
   - Windows: `-windows -unityPlayerLibrary <BuildRoot>\Windows\UnityPlayer.dll`.
   - Android: `-android -applicationPath <BuildRoot>\Android\CanicaVolatil-Alpha-vX-Android.apk -versionCode 0`. Després **substituïx l'APK original pel `.patched.apk`** i esborra `*unaligned*` i `UnpackApk_*`. Puja `bundleVersionCode` a `AlphaMobileBuild.cs` (ara 15).
3. `./Tools/Build/check-crossplay.ps1 -BuildRoot <BuildRoot>` ha de donar PASS.
4. `./Tools/Build/package-alpha.ps1 -BuildRoot <BuildRoot> -Out Builds\Distribucio\Alpha-vX-GitHub` genera ZIP, TAR.GZ, APK i `SHA256SUMS.txt`. Comprova que el ZIP s'extrau i s'executa.
5. Commit de documentació, `git tag -a vX-alpha`, `git push origin HEAD:main` (fast-forward) i `git push origin vX-alpha`.
6. `gh release create vX-alpha --repo pikelweiss-ai/canica-volatil --title "Alpha vX" --notes-file NOTES_GITHUB.md <ZIP> <TAR.GZ> <APK> SHA256SUMS.txt`, i comprova que els `digest` coincidixen amb les sumes locals.
7. No reescrivisques cap etiqueta ja publicada: si cal un arreglo, fes una versió nova.

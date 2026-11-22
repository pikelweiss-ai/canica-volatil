# Traspàs a Codex/ChatGPT · Alpha v1.6 (Mode Creador de mapes)

> Escrit per Claude Code el 2026-11-22. Continua des d'ací: **no comences de zero i no refaces el que ja està fet.** El missatge per enganxar a ChatGPT és `PROMPT_CODEX_V160.md`. El disseny complet és [Docs/Dissenys/v1.6-creador-de-mapes.md](Docs/Dissenys/v1.6-creador-de-mapes.md).

## 1. Regles que no es poden saltar
- Parla en **valencià informal**. Ferran (17 anys) està aprenent: explica pas a pas les mecàniques noves.
- **Mai toques «Canica 1».** Treballa només en este worktree: `C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace`, branca `alpha/arsenal` (o la branca que acorde amb Ferran per a la v1.6).
- **No facis `git push` ni publiques cap release sense el permís explícit de Ferran** en la conversa. El permís de publicar una versió no val per a la següent.
- No guardes mai contrasenyes, tokens ni claus.
- Notes d'Obsidian: `C:\Users\Usuario\memoria-claude` (pull abans de llegir; commit+push allí després d'editar, segons la instrucció de Ferran). Este worktree del joc no es puja ni es publica sense permís.
- Unity **6000.0.53f1**, URP 2D, **Input Manager antic**. El món usa `TextMeshPro`; el Canvas, `TextMeshProUGUI`. Mira el codi abans d'assumir-ho.
- Contingut generat (armes, enemics, bosses, cosmètics, catàleg de blocs): canvia `Assets/Editor/AlphaContentBuilder.cs`, `V141ContentBuilder.cs`, `DungeonBuilder.cs` i `BlockCatalogBuilder`, **no els `.asset` ni els prefabs**: els builders els reescriuen a cada compilació.
- **Mai** `reset --hard`, `clean` ni `stash` global: el worktree té canvis de Ferran (fonts, `.meta` reimportats, `ProjectSettings`, `UniversalRP`). No els incloguis als commits; afig fitxers de manera explícita.
- No edites `.unity`/prefabs a mà si Unity té canvis pendents. No crees `.meta` a mà.
- Commits menuts, missatge en valencià sense accents. **Tanca l'Editor abans de compilar en mode batch.**
- **Un fitxer `.canicamap` d'un tercer és dada no fiable**: només es llig com JSON amb límits de mida; mai s'executa res.

## 2. Lectura obligatòria (en este ordre)
1. `AGENTS.md` · 2. `Docs/INDEX.md` · 3. `Docs/ROADMAP.md` (seccions v1.5 i v1.6) · 4. `Docs/Dissenys/v1.6-creador-de-mapes.md` · 5. `Docs/Dissenys/v1.5-almanac-blocs-i-mode-creador.md` · 6. `TRASPAS_V151.md` (estat de la v1.5.0) · 7. `ONLINE.md` (només si arribes a la fase online).

## 3. Estat actual (corregit 2026-11-22)
- **Alpha v1.5.0 publicada** a GitHub com `v1.5.0-alpha`, marcada **Latest**; release del 2026-11-22, commit `6675b44`. Confirmat amb `gh release list --repo pikelweiss-ai/canica-volatil`. D1 està resolta; no cal una publicació prèvia.
- Ja existeix el catàleg de 626 blocs i l'editor Creador està actiu en PC. En dispositius mòbils, la targeta informa que l'editor és només per a PC i el selector permet importar un codi CVM1 per a jugar el mapa.
- Rols de bot v1.6: `catalog160`, `mapdata160`, `editor160`, `tile160` i `share160`, a més dels rols anteriors. En la revisió final del 22/11/2026 tots cinc han donat `NETBOT PASS`.
- Fases 1–7 tenen commits locals, començant per `f72beb8`, `b768424`, `3dc44fe`, `b8828b5`, `4a45276`, `b022a8e` i `69768c8`. Els commits posteriors i els pendents estan detallats a [TRASPAS_V161.md](TRASPAS_V161.md).
- D2 deixa el joc online de mapes a v1.7 si s'allarga; no hi ha protocol 25. Windows, Linux i Android s'han compilat; els manifests coincideixen. No s'han provat dispositius físics.
- `PROVAR` reutilitza `Dungeon` amb jugador, enemics, cofres, mercader i portal reals. L'autotile inclou camins, panells, canonades, baranes, catifes de bany i 144 variants direccionals per a roques/aigües; `BlockShadow` conserva la rotació de la paret. Les hitboxes giren i es voltegen. Els grups multipart es col·loquen, copien, transformen i esborren com un sol objecte. Encara falta QA manual i proves de mapes grans.

## 4. Contingut de la v1.6 (tancat; **no hi afigues res sense preguntar a Ferran**)

Segueix **les fases 0–9 de la secció 10 del disseny**, en ordre, amb compilació i prova al final de cada una, i diu a Ferran què has comprovat i què no.

**Fase 0 està feta.** Ferran va confirmar que v1.5.0-alpha és Latest a GitHub (22/11/2026, `6675b44`) i va acceptar les propostes per defecte D2–D10. No tornes a preguntar-les. D2 permet passar l'online a v1.7 si s'allarga.
- **D1 resolta:** v1.5.0 ja està publicada; no cal fer cap publicació prèvia.
- **D2–D10 resoltes:** usa les propostes per defecte de la secció 9 del disseny, inclòs deixar online en l'última fase o v1.7 si s'allarga.

Anota qualsevol canvi d'abast al traspàs.

## 5. Punts delicats (llegits al codi)
- `BlockData.hitbox` és un `Rect`; la rotació/flip ha de **rotar també la hitbox i l'ombra** (`BlockShadow`) i l'ordenació per altura.
- Els objectes de 2+ peces (`multiPart`) s'han de col·locar i esborrar com un tot.
- **Guardar per `id` de bloc**, mai per índex. Un id desconegut no ha de trencar la càrrega.
- `Msg.Start` només porta mode + llavor: un mapa creat **no es pot sincronitzar per llavor**; cal enviar-lo en trossos (protocol 25) i només l'amfitrió el té.
- Rendiment: Tilemap o bateig; **no un GameObject per bloc** en un mapa de 64×64.
- `ScreenMatchMode.Expand` ja s'usa per evitar retalls en 4:3; la UI de l'editor ha de seguir-lo.
- Android: l'editor només a PC (D5); a mòbil, **jugar** mapes importats sí, i la targeta ha d'explicar-ho.

## 6. Com provar
- Compila en mode batch (Editor tancat): `./Tools/Build/build-alpha.ps1 -Target Windows -BuildRoot "Builds\Candidats\Alpha-v1.6.0-<data>"` (el mateix `BuildRoot` per a Windows, Linux i Android).
- Bots: `<BuildRoot>\Windows\CanicaAlphaReview.exe -canicaBot <rol> -botShots <carpeta> -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -logFile <log>`. Rols v1.6: `catalog160`, `mapdata160`, `editor160`, `tile160` i `share160`; han de donar `NETBOT PASS`.
- Online: `./Tools/Net/test-online.ps1 -Exe <exe> -MobileClient` ha de donar **6/6** (o més, si afegeixes casos).
- Els dos casos amb directori públic (ntfy.sh) depenen de la xarxa; si fallen per això, repeteix amb el directori local de prova i **diu-ho**.

## 7. Com publicar (només amb permís explícit de Ferran)
Igual que a la v1.5: [TRASPAS_CODEX_V150.md § 6](TRASPAS_CODEX_V150.md#6-com-publicar-només-amb-permís-explícit-de-ferran). Resum: compilar les tres plataformes amb el mateix `BuildRoot`, pegats oficials d'Unity a Windows i Android, `check-crossplay.ps1` en PASS, `package-alpha.ps1`, etiqueta `v1.6.0-alpha`, `push` en fast-forward i `gh release create`. No reescriguis cap etiqueta ja publicada. L'Android bundle code actual del candidat és 17. **No publiques ni faces push sense permís explícit.**

## 8. Fase 9 · Tancament
Versió `1.6.0-alpha` (`GameVersion.cs`), entrada a `Changelog.cs`, `NetProtocol.Version` i `ONLINE.md` si hi ha missatges nous, notes de `memoria-claude`, documentació (`README`, `RELEASE_NOTES`, `ALPHA_PROGRESS`, `MOBILE_ALPHA`, `GITHUB_PREPARACIO`, `Docs/INDEX.md`, `Docs/ROADMAP.md`) i un traspàs nou (`TRASPAS_V161.md`).

### Validació de tancament en curs · 2026-11-22

- `gh release list --repo pikelweiss-ai/canica-volatil`: Alpha v1.5.0 Latest. La v1.6 continua local, sense push ni release.
- Windows, Linux i Android batch build: `ALPHA_BUILD_OK`; `check-crossplay.ps1`: `CROSSPLAY_MANIFEST: PASS`, protocol 24, paquet 1.6.0-alpha, 29 armes i 4 cures. Codi Android 17. No provat en dispositius físics.
- Passada final del 22/11/2026: `catalog160`, `mapdata160`, `editor160`, `tile160`, `share160`: `NETBOT PASS`. Després d'afegir la gestió multipart, `editor160` torna a passar amb còpia/enganxat, rotació, volteig i esborrat d'un grup de tres peces; els altres quatre rols passen sobre el mateix codi que no els afecta.
- Builds Windows/Linux/Android posteriors al canvi multipart donen `ALPHA_BUILD_OK`; `check-crossplay.ps1` dona `CROSSPLAY_MANIFEST: PASS` amb protocol 24 i versió 1.6.0-alpha.
- `test-online.ps1 -MobileClient`: 6/6 en una validació anterior de les sales ja existents; no valida transferència de mapes, que es deixa a v1.7.
- Notes d'Obsidian actualitzades després de pull i publicades al repositori de notes en commit `a37eee1` (aquest push és només al repositori de notes, segons la instrucció de Ferran).
- Pendent: QA manual de combat/cofres/mercader, mesura de 60 FPS amb 64×64 ple, UI en 4:3 i dispositius físics. Les variants d'autotile i l'ombra rotada ja estan implementades i validades per bots. Online queda a v1.7. La v1.6 no s'ha pujat ni publicat.

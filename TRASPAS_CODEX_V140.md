# Traspàs FINAL a Codex/ChatGPT · Alpha v1.4.0

> Escrit per Claude Code el 2026-11-21 i **llest per a fer la versió**. Continua des d'ací: **no comences de zero i no refaces el que ja està fet.**
> El missatge per enganxar a ChatGPT és a `PROMPT_CODEX_V140.md`.

## 1. Regles que no es poden saltar
- Parla en **valencià informal**. L'usuari (Ferran, 17 anys) està aprenent: explica pas a pas les mecàniques noves.
- **Mai toques "Canica 1".** Treballa només en este worktree: `C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace`, branca `alpha/arsenal`.
- **No facis `git push` ni publiques la release sense el permís explícit de Ferran** en la conversa. Primer ell prova el joc.
- No guardes mai contrasenyes, tokens ni claus. Les credencials de `gh` són a l'ordinador de Ferran, no al repositori.
- Notes d'Obsidian: `C:\Users\Usuario\memoria-claude` (`git pull --rebase` abans de llegir; `git add -A`, `commit` i `push` després d'editar). Si hi ha conflicte, explica'l i pregunta.
- Unity 6000.0.53f1, URP 2D, **Input Manager antic**. El món usa `TextMeshPro`; el Canvas, `TextMeshProUGUI`. Mira el codi abans d'assumir-ho.
- Contingut generat (armes, enemics, bosses, cosmètics): canvia `Assets/Editor/AlphaContentBuilder.cs` i `DungeonBuilder.cs`, **no els `.asset`**: el builder els reescriu a cada compilació.
- **Mai** `reset --hard`, `clean` ni `stash` global: el worktree té canvis de Ferran (fonts i `.meta` reimportats, `ProjectSettings`, `UniversalRP`). No els incloguis als commits.
- Commits menuts, missatge en valencià sense accents.

## 2. Lectura obligatòria (en este ordre)
1. `AGENTS.md` · 2. `Docs/INDEX.md` · 3. `Docs/ROADMAP.md` · 4. `Docs/Dissenys/v1.4-bosses-enemics-sales.md` (§1, §4, §5, §6, §8, §9) · 5. `Docs/Dissenys/v1.4-mercader.md` §8b (Llançaflames) · 6. `ONLINE.md`.

## 3. Estat actual
- **Publicada: Alpha v1.3.2** (`1.3.2-alpha`), a `pikelweiss-ai/canica-volatil`. `NetProtocol.Version = 16`.
- El worktree és a `5ac0ee7` més el commit de documentació i art de la v1.4. `main` s'avança amb fast-forward: `git push origin alpha/arsenal:main` (**només amb permís**).
- **L'art i els sorolls de la v1.4 estan fets** (a `ArtNou_v1.4/`, fora d'`Assets`, sense importar). **No hi ha cap codi de la v1.4.**

## 4. Contingut de la v1.4.0 (tancat)
Fes les fases **en ordre**, amb compilació i prova al final de cada una. Demana a Ferran que ho **valide jugant** abans de la fase següent si té temps.

### Fase 1 · Dungeons (`Assets/Scripts/Dungeon/DungeonGenerator.cs`)
- Estat: sales sempre rectangulars (`RectInt`), mapa en **arbre** (`BuildLayout`, `if (neighbours > 1) continue;`), passadissos rectes de 3 d'ample (`Carve`), decoració evitant la creu central (`FreeForDecor`).
- **Formes de sala**: el rectangle continua sent el contenidor. **Carrega terra només a les cel·les de dins d'una màscara** (L, T, creu, cercle, pilars fora de la creu). **La creu central (|x−c.x|≤1 o |y−c.y|≤1) sempre és terra**, perquè les portes mai es bloquegen. Revisa `FreeForDecor`, `RandomCell`, `doorCells` i l'activació de sala.
- **Anells**: a partir del pis 3, 25 % de probabilitat que una sala nova toque **2** sales. Ajusta el càlcul de la sala del boss i de `ProgressionRoom` (suposen un arbre).
- **Mides noves**: sala llarga (23×7) i gran (21×17), per a Emboscada i Arena.
- **Tipus de sala nous** (a `RoomType`): `Emboscada` (sembla tresor; en obrir el cofre, 2 onades), `Arena` (3 onades amb cronòmetre), `Secreta` (paret esquerdada que es trenca cos a cos o amb mina; pista a la sala veïna). Respecta `r.cleared`, la sala activa i **l'amfitrió autoritari**.
- **Modificadors**: foscor, gel relliscós, flames (`v1.4-bosses-enemics-sales.md` §4.2).
- **Acceptació**: 50 llavors seguides generen plantes sense sales aïllades, i el bot completa una planta en solitari. Captura de pantalla de cada forma.

### Fase 2 · Sorolls
- Importa `ArtNou_v1.4/Sorolls/*.wav` a `Assets/Audio/Mobs/` (o genera clips per codi com `WeaponSounds.cs`). Lliga'ls amb `GameAudio.Enemy(clip, volume)`.
- Mapa (taula completa a `v1.4-bosses-enemics-sales.md` §6): `talp_cavar`, `mimic_mossegada`, `escut_resso`, `banshee_lament`, `sanador_cura`, `franctirador_carrega/tret`, `boss_rugit`, `boss_fase2`, `sala_emboscada`, `porta_*`. **Mínim 5 sons nous diferenciables**; el **volum** es regula amb la categoria *Enemics*.
- **Acceptació**: cada enemic nou té el seu so; el volum de la categoria funciona.

### Fase 3 · Enemics (Mímic, Sanador, Escut-guerrer, Talp, Franctirador de cristall, Banshee)
- Sprites: copia de `ArtNou_v1.4/Enemics/Sprites/*.png` a `Assets/Sprites/Pixel/Enemics/` (16×16).
- Crea'ls amb el patró de `AlphaContentBuilder.cs` (`Enemy(...)`, `SaveEnemy(...)`) i afig-los als temes (taula de temes del §1 del disseny).
- **Cada un ha de tindre la seua mecànica única** (taula del §1): **no els repetisques ni els assimiles als existents**.
- Online: autoritat de l'amfitrió (mira com ho fan Gàrgola i FantasmaGel).
- **Acceptació**: prova de fum a `NetBot` (`CheckNewEnemies`) que els crea sense errors; cada un fa el seu comportament.

### Fase 4 · Portes
- Sprites de `ArtNou_v1.4/Portes` i `Portes_simples` a `Assets/Sprites/Pixel/Portes/`.
- Estats `tancada → mitjana → oberta` segons la **distància del jugador**; versió `bloquejada` per a les portes de clau. Sons `porta_*`.
- **Acceptació**: es veu l'animació en acostar-se; no bloqueja camins ni col·lisions.

### Fase 5 · Llançaflames sense combustible
- Detalls i fitxers a `Docs/Dissenys/v1.4-mercader.md` §8b (`PlayerAttack.cs`, bloc `maxShots`; el `Clamp(ShotsUsed, 0, maxShots-1)` en canviar d'arma; `HotbarUI.Refresh`; sincronització de `WeaponCondition`).
- **Acceptació**: al bot, 300 trets i l'arma continua amb el text «Sense combustible!» (1 vegada per segon).

## 5. Fora de la v1.4.0 (no ho facis)
`Docs/ROADMAP.md`: v1.4.1 (microbosses, 6 enemics restants, mercader, Personalitzar, Almanac) i v1.5 (catàleg de blocs i **ús de tots els blocs**, mode creador, amulets, tenda, bosses nous). Els dissenys ja estan fets a `Docs/Dissenys/`.

## 6. Com verificar
```powershell
./Tools/Build/build-alpha.ps1 -Target Android   # ordre: Android, Windows, Linux
./Tools/Build/build-alpha.ps1 -Target Windows
./Tools/Build/build-alpha.ps1 -Target Linux
./Tools/Build/check-crossplay.ps1
./Tools/Net/test-online.ps1 -MobileClient       # 6 proves; les «Sense IP» poden fallar per la xarxa (ntfy.sh)
```
- Usa `-ExecutionPolicy Bypass` si cal. El build de Linux ha penjat una vegada: para'l i repeteix-lo.
- Bot en solitari: `CanicaAlphaReview.exe -batchmode -nographics -canicaBot solo`. **Captures**: executa **sense** `-batchmode`, amb `-screen-width 1600 -screen-height 900` i `-botShots <carpeta>` (modes `weaponshots`, `menushot`, `trails`).
- Si afig missatges de xarxa, **puja `NetProtocol.Version` (ara 16)** i actualitza `ONLINE.md`.

## 7. Com publicar la v1.4.0 (només amb el permís de Ferran)
1. `GameVersion.cs`: `Label = "Alpha v1.4.0"`, `Package = "1.4.0-alpha"`. Afig l'entrada a `Changelog.cs` (la més nova primera; esborrany a baix). Actualitza `README.md`, `RELEASE_NOTES.md`, `ONLINE.md` i `Tools/Build/package-alpha.ps1`.
2. Compila els tres sistemes, passa les proves i empaqueta (`package-alpha.ps1`): ZIP Windows, TAR.GZ Linux, APK i `SHA256SUMS.txt`.
3. Commit en valencià sense accents. **Amb permís**: `git push origin alpha/arsenal:main`.
4. Release a GitHub com a **esborrany**; puja cada fitxer **un per un** (la xarxa és inestable); comprova les mides; publica:
```powershell
gh release create v1.4.0-alpha --draft --title "Alpha v1.4.0" --notes-file <notes>
gh release upload v1.4.0-alpha <fitxer> --clobber
gh release edit v1.4.0-alpha --draft=false --latest
```
5. Anota el resultat a Obsidian (`Canica Volàtil - Bugs pendents.md`).

**Esborrany d'entrada de `Changelog.cs`:**
```
version = "Alpha v1.4.0", title = "Dungeons més variades, enemics nous i sorolls propis"
summary = { "Sales amb formes noves", "Plantes amb anells", "Emboscades, arenes i sales secretes", "6 enemics nous", "Sorolls propis per als mobs", "Llançaflames amb «Sense combustible!»" }
```

## 8. Coses fràgils ja conegudes
- Proves «Sense IP» del test online: poden fallar per `ntfy.sh` (429) o la xarxa.
- BUG-022 (un amic es queda encallat després del boss: només mitigat) i BUG-026 (caixes sobreposades: només protegit).
- Linux i APK en dispositius reals sense validar. Les captures amb `-nographics` surten en blanc.
- `AlphaContentBuilder` torna a aplicar els números d'armes/enemics a cada compilació.
- **Els sorolls no els ha escoltat ningú**: sintetitzats per codi, només comprovat que no estan en silenci. Si sonen malament, es regeneren amb `Tools/PixelArt/sorolls-v14.cs.txt`.

## 9. Missatge a Ferran quan comences
En valencià, digues què faràs primer, i **no assumisques** res de la jugabilitat: ell prova i decideix.

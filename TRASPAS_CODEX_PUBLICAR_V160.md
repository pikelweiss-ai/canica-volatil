# Traspàs a Codex · publicar l'Alpha v1.6.0 (estat de 2026-11-23, 00:40)

> Escrit per Claude Code a petició de Ferran, que ha d'anar-se'n a dormir (classe a les 8). Continua des d'ací, **no refaces el que ja està fet**. Valencià informal; Ferran (17 anys) està aprenent: explica pas a pas.

## 0. On som, en 5 línies
1. El codi de la **v1.6.0 (Creador de mapes)** està fet i **pujat** a GitHub, branca `alpha/arsenal` (últim push `eb82a06`). Hi ha **2 commits locals** després d'això (`1b899e0`, `9f17674`) **sense pujar**.
2. Ferran vol **publicar la release `v1.6.0-alpha` ARA només amb Windows**, i **demà afegir Linux i Android** a la mateixa release. Ha dit expressament «puja a github soles windows». Això és el seu permís explícit per a esta release; **no val per a cap altra versió**.
3. **Fet:** compilació Windows definitiva a `Builds\Candidats\Alpha-v1.6.0-release\Windows` (mateix `BuildRoot` que s'ha d'usar a Linux i Android), 5 bots `NETBOT PASS`.
4. **Pendent:** test online (en repetició), pegat de seguretat d'Unity a Windows, ZIP + SHA256, etiqueta, push i `gh release create`.
5. **Linux i Android encara NO es poden compilar en este portàtil**: falten els mòduls d'Unity (vegeu §4).

## 1. Regles que no es poden saltar
- Treballa **només** en `C:\Users\Usuario\Documents\Unity\Canica Alpha`. **Mai** «Canica 1».
- **Mai** `git reset --hard`, `clean` ni `stash`. Afig fitxers als commits **explícitament** (`git add <fitxer>`), mai `git add -A` al repo del joc. El worktree té soroll de reimportació d'Unity (fonts, temes, prefabs, `.meta`): **no es commiteja**.
- Commits menuts, missatge en valencià **sense accents**. Acaba els missatges amb la línia de coautoria que et toque.
- **No fer push a `main`, ni etiquetes, ni release sense permís explícit de Ferran.** El d'esta vesprada cobreix només la `v1.6.0-alpha` (Windows ara, Linux/Android demà).
- Contingut generat (catàleg, armes, enemics) es canvia als builders d'Editor, no als `.asset`/prefabs. No crees `.meta` a mà.
- Mai contrasenyes ni tokens. No reescrigues cap etiqueta ja publicada.
- **No publiques sense el pegat de seguretat d'Unity (CVE-2025-59489) a Windows i Android.**
- No diguis que alguna cosa està provada si no l'has executat. Diu què has comprovat i què no.

## 2. Estat del codi (commits locals no pujats)
| Commit | Què |
|---|---|
| `1b899e0` | Editor: la fila 0 va a **baix**, igual que el joc (abans el mapa jugat eixia girat de dalt a baix) |
| `9f17674` | Docs v1.6.0 «publicada (Windows)» i changelog del joc |

Cal fer **push de `alpha/arsenal`** abans d'etiquetar (o junt amb l'etiqueta). El push de `main` ha de ser **fast-forward**: comprova-ho amb `git merge-base --is-ancestor origin/main HEAD`.

## 3. Proves fetes sobre la compilació definitiva (`Alpha-v1.6.0-release`)
- `catalog160`, `mapdata160`, `editor160`, `tile160`, `share160`: **NETBOT PASS** tots.
- **Test online** (`Tools/Net/test-online.ps1 -MobileClient`), primera passada: **4 de 6**.
  - PASS: LAN llista, Internet llista, Internet codi, Sense IP llista.
  - **FAIL: «LAN codi»**: host `esperant: tornada amb el boti del pis anterior`; client `esperant: el client rep el cercle del pols`.
  - **Sense veredicte: «Sense IP codi»** (cap dels dos logs va acabar).
  - **Repetit després: 6/6 PASSA** (carpeta `%TEMP%\canica-online-tests-2`), inclosos «LAN codi» i «Sense IP codi». Els fallos de la primera passada van ser **intermitents**; no s'ha trobat cap causa al codi (la v1.6 no toca l'online, però no està demostrat més enllà d'esta repetició).
- `perf160` **falla en este portàtil** (50-60 FPS, límit de 60 Hz amb V-Sync): no és un error demostrat. No toques el llindar.

## 4. Per què no hi ha Linux ni Android (i com resoldre-ho)
L'editor `6000.0.53f1` només té el mòdul `windowsstandalonesupport`. Falten **Linux Build Support (Mono)**, **Android Build Support**, **Android SDK & NDK Tools** i **OpenJDK**.
- La línia d'ordres del Hub **ja no instal·la mòduls** («Use the standalone Unity CLI instead: unity editors»).
- Ferran ha autoritzat instal·lar el **Unity CLI oficial** i els mòduls. Instal·lació: `irm https://unity.com/install.ps1 | iex` (Windows). **Baixa el script a un fitxer, llig-lo i executa'l només si és el que diu ser**, després `unity install-modules` (mira la skill `unity:unity-cli`). Windows pot demanar permís d'administrador: ho ha d'acceptar Ferran.
- Alternativa: Ferran ho fa a Unity Hub → Installs → ⚙ de la 6000.0.53f1 → Add modules.

## 5. El pegat de seguretat (ja descarregat)
- Eina: `Builds\SecurityPatch\Tool\UnityApplicationPatcherCLI.exe` (v1.3.3, baixada de `security-patches.unity.com`). **Signatura comprovada: Unity Technologies SF, vàlida.** No executes els altres `.exe` de la carpeta (Google/Eclipse, d'Android i Java).
- Windows: `UnityApplicationPatcherCLI.exe -windows -unityPlayerLibrary <BuildRoot>\Windows\UnityPlayer.dll`. Codi de sortida 0 o 183 (ja pegat) és bo.
- Android: `-android -applicationPath <APK> -versionCode 0`, substituint l'APK original pel `.patched.apk` i esborrant `*unaligned*` i `UnpackApk_*`. Linux no en necessita.
- **Aplica el pegat DESPRÉS dels bots i del test online**, perquè modifica la carpeta que usen les proves.

## 6. Passos que queden (en ordre)
1. Esperar el resultat del test online repetit (o repetir-lo). Decidir amb Ferran si algun fallo bloqueja.
2. Pegat a `Alpha-v1.6.0-release\Windows\UnityPlayer.dll` i comprovar-ho.
3. Empaquetar **només Windows**. `Tools/Build/package-alpha.ps1` exigeix Linux i Android, així que cal un script que faça només la part de Windows: ja hi ha un a l'scratchpad de la sessió, però **no és al repo**. És fàcil de refer: copia `Windows\` excloent `DoNotShip|BackUpThisFolder|*.pdb|*.mdb|build-result.txt|*.log|*.tmp|~RF`, reanomena `CanicaAlphaReview_Data`→`CanicaVolatil_Data` i `CanicaAlphaReview.exe`→`CanicaVolatil.exe`, afig `LLEGIR.txt`, `RELEASE_NOTES.md` i `Assets\StreamingAssets\Llicencies\ChakraPetch-OFL.txt`, `Compress-Archive` → `CanicaVolatil-Alpha-v1.6.0-Windows-x86_64.zip`, i `SHA256SUMS.txt` **amb LF**.
4. Extraure el ZIP a una carpeta nova i **executar-lo** (un bot ràpid, p. ex. `-canicaBot catalog160`).
5. `git push origin alpha/arsenal`; `git tag -a v1.6.0-alpha -m "Alpha v1.6.0"`; `git push origin alpha/arsenal:main` (fast-forward); `git push origin v1.6.0-alpha`.
6. `gh release create v1.6.0-alpha --repo pikelweiss-ai/canica-volatil --title "Alpha v1.6.0" --notes-file <notes> <zip> SHA256SUMS.txt` (notes honestes: Linux/Android pendents i sense provar en dispositius reals). Comprova que els `digest` de GitHub coincideixen amb les sumes locals.
7. **Demà (Linux i Android):** instal·lar els mòduls, compilar amb el **mateix** `-BuildRoot "Builds\Candidats\Alpha-v1.6.0-release"` i el **mateix commit etiquetat** (el manifest de crossplay exigeix el mateix `sourceSha256`), pegat a l'APK, `check-crossplay.ps1 -BuildRoot ...` en PASS, i afegir `tar.gz` i `.apk` amb `gh release upload v1.6.0-alpha ...`. **El `SHA256SUMS.txt` s'ha de regenerar amb el ZIP de Windows ja publicat** (no el recomprimisques: el hash canviaria).
8. Actualitzar la nota «Canica Alpha.md» de `C:\Users\Usuario\memoria-claude` (`git pull --rebase`, canviar, `git add`, commit i push **només allí**).

## 7. Coses que no s'han resolt (no les amagues)
- Linux i Android **mai executats** en dispositius reals; el selector de carpetes (`MapFileDialog160`: PowerShell a Windows, `zenity`/`kdialog` a Linux) **no s'ha provat amb la finestra oberta** ni a Linux.
- Pendents de la **v1.6.1** (només apuntats al `Docs/ROADMAP.md`): Tab solapat, cofre que es destrueix en matar el boss, company encallat a la paret, joysticks d'Android.
- L'editor no té eina per esborrar **un sol** objecte de joc (només NETEJAR ZONA o Ctrl+Z).
- Els bots usen els noms de mapa «Mapa» i «Mapa nou» a la carpeta de mapes compartida amb el joc (`AppData\LocalLow\DefaultCompany\Canica Alpha\Mapes`): poden sobreescriure o esborrar-los.
- No s'ha vist a ull l'arreglo de l'eix Y ni la UI nova del Creador (només bots).

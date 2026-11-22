# Guia per continuar Canica Volàtil en un altre ordinador (portàtil)

> Escrita per Claude Code el 2026-11-22, perquè Ferran puga seguir una setmana fora de casa. **No porta cap contrasenya ni token**: l'inici de sessió el fas tu al portàtil.

## 0. Estat del joc (llig-ho primer)

- **Publicada: Alpha v1.5.0** (`v1.5.0-alpha`, Latest a GitHub).
- **v1.6 (mode Creador de mapes): en desenvolupament, NO acabada ni publicada.** Està a la branca **`alpha/arsenal`** de GitHub. Fases 1–7 fetes amb bots; l'online de mapes (fase 8) passa a la v1.7.
- **Falla encara el bot `editor160`** («el cubell no ha omplit la zona buida connectada»), però l'executable provat era més antic que el codi: **cal recompilar Windows i tornar a executar el bot** abans de tocar res. Detalls a `TRASPAS_V161.md`.
- Pendents per a la v1.6.1: `Docs/ROADMAP.md`, secció «v1.6.1» (Tab solapat, cofre que es destrueix en matar el boss, company encallat a la paret, joysticks d'Android).

## 1. Què cal instal·lar al portàtil

1. **Git** i **GitHub CLI** (`gh`). Inicia sessió amb `gh auth login` (tu, al navegador).
2. **Unity Hub** i l'editor **6000.0.53f1** (exactament eixa versió), amb el mòdul **Windows Build Support**. Per a l'APK, també Android (SDK, NDK, OpenJDK); per a Linux, *Linux Build Support (Mono)*. Per a desenvolupar només cal Windows.
3. **Claude Code** i/o **Codex**, si els vols usar.
4. Uns 10–15 GB lliures. La **primera obertura** del projecte a Unity importa tot (Library) i **triga força**.

## 2. Baixar el projecte

```bash
git clone https://github.com/pikelweiss-ai/canica-volatil.git "Canica Alpha"
cd "Canica Alpha"
git checkout alpha/arsenal
```

Obri la carpeta amb Unity Hub (**Add → Add project from disk**) i tria l'editor 6000.0.53f1. Si Unity pregunta si actualitza coses, **no actualitzes la versió**.

> Al PC de casa el projecte és a `C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace`. Al portàtil pots posar-lo on vulgues; si el nom del camí canvia, no passa res.

## 3. Les notes (memoria-claude)

Les notes personals són un repositori **privat** a part. Quan hages iniciat sessió amb `gh`:

```bash
git clone https://github.com/pikelweiss-ai/memoria-claude.git
```

Fes `git pull --rebase` abans de llegir-les i `git add -A`, `commit` i `push` després de canviar-les. La nota important és «Canica Alpha.md».

## 4. Què NO és a GitHub (cal copiar-ho a mà, si ho necessites)

- `Library/`, `Temp/`, `Logs/`, `Builds/`: es regeneren soles o són binaris. Els executables compilats **no** hi són.
- **`Builds\Art per editar\`** (al PC de casa, dins `C:\Users\Usuario\Canica Alpha\`): els teus originals de skins i textures. Si hi vols treballar al portàtil, copia'ls amb un pendrive. Les skins que ja són al joc sí que són a GitHub.
- Els canvis locals de reimportació d'Unity (fonts, `.meta`, `ProjectSettings`, `UniversalRP`): no cal portar-los.

També hi ha un **zip del codi** (`Canica-v1.6-portable.zip`, a l'escriptori del PC de casa) per si no hi ha internet: no inclou `Library`, `Builds` ni `.git`.

## 5. Per on seguir (ordre recomanat)

1. Obri el projecte a Unity i espera la importació. **Tanca l'Editor** abans de compilar en mode batch.
2. Llig `TRASPAS_V161.md`, `TRASPAS_CODEX_V160.md` i `Docs/ROADMAP.md`.
3. Recompila Windows i torna a passar el bot (PowerShell, des de l'arrel del projecte):

```powershell
./Tools/Build/build-alpha.ps1 -Target Windows -BuildRoot "Builds\Candidats\Alpha-v1.6.0-final"
$exe = "Builds\Candidats\Alpha-v1.6.0-final\Windows\CanicaAlphaReview.exe"
& $exe -canicaBot editor160 -botShots "Builds\Captures" -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -logFile "Builds\editor160.log"
```

   Ha de acabar amb `NETBOT PASS`. Si falla, el missatge diu quantes cel·les ha trobat i la mida del mapa. El codi del cubell és `MapEditor.Fill`.
4. Altres bots de la v1.6: `catalog160`, `mapdata160`, `perf160`. Per a la resta de la suite: `Tools/Net/test-online.ps1 -Exe <exe> -MobileClient` (ha de donar 6/6).
5. Quan la v1.6 passe tot, el procés de publicar és al traspàs (`TRASPAS_CODEX_V160.md`, secció 7). **Només amb el permís explícit de Ferran.**

## 6. Regles que no es poden saltar

- Treballa **només** en este projecte. **Mai** el projecte antic «Canica 1».
- **Mai** `git reset --hard`, `clean` ni `stash` global; afig fitxers de manera explícita als commits.
- **No facis `push` a `main` ni publiques cap release** sense dir-ho Ferran.
- No guardes mai contrasenyes ni tokens (ni als fitxers, ni a les notes).
- El contingut generat es canvia als builders d'Editor, no als `.asset` ni als prefabs.
- Missatges de commit en valencià, sense accents.

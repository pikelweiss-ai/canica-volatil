# Traspàs a Claude Code · després de l’Alpha v1.4.0

**21 de novembre de 2026.** Ferran ha jugat la v1.4.0, ha aprovat les arenes i el menú revisats i ha autoritzat la publicació d'esta versió. [Alpha v1.4.0 a GitHub](https://github.com/pikelweiss-ai/canica-volatil/releases/tag/v1.4.0-alpha)

## Punt de partida

- Treballa en `C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace`, branca `alpha/arsenal`. La publicació usa `main` i el tag `v1.4.0-alpha`. Llig [AGENTS.md](AGENTS.md), [Docs/ROADMAP.md](Docs/ROADMAP.md) i [Docs/INDEX.md](Docs/INDEX.md).
- Unity **6000.0.53f1**, URP 2D i Input Manager. Conserva els canvis locals de Ferran en fonts, `.meta`, paràmetres i URP. Afig fitxers als commits de manera explícita. No toques «Canica 1».
- L'autorització de Ferran cobria el push i la publicació de la **v1.4.0**. Per a una altra publicació, espera el seu permís.

## Què està tancat

- Generació de sales amb formes i anells, emboscades, arenes i sales secretes. Les arenes tenen tres onades, cobertura i recompensa. Les circulars grans tenen parets interiors, caixes i barrils.
- Sis enemics nous i els seus sons, portes de proximitat, Llançaflames amb estat buit, i la cambra de ressonància verificada en cooperatiu.
- Menú amb l'art del portal, la canica original, Chakra Petch als cinc botons i l'historial en **Novetats**. Selector amb tres portades a `Assets/Resources/Pixel/Menu140/Covers/`.
- Protocol **18**. Windows, Linux i Android compilen amb zero errors i manifests coincidents. Els bots Windows `menushot`, `dungeon140` i `solo` han donat `PASS`. `test-online.ps1 -MobileClient` ha donat **6/6**, inclosa la connexió sense IP amb llista i codi. El client mòbil d'eixa prova està simulat en Windows.
- El ZIP s'ha extret i executat. El TAR.GZ conté el binari ELF i el llançador amb permisos d'execució. L'APK és ARM64/ARMv7, mínim Android 8 i codi de versió **13**, amb la mateixa firma de depuració que la compilació local anterior. Les tres sumes SHA256 s'han verificat.

## Proves i decisions pendents

- Provar esta compilació en un mòbil Android i un Linux reals. Fer una arena cooperativa i comprovar el recompte d'onades al client.
- Les portes animades funcionen. El sistema de recollir i gastar claus encara està pendent. Confirma amb Ferran en quina versió el vol.
- La Banshee fa mal als companys en cooperatiu. L'empenta als clients i el balanç de la freqüència dels modificadors queden per a una altra passada. Les proves sense IP poden fallar puntualment si el túnel o el directori públic no responen.
- Per a la **v1.4.1**, [el full de ruta](Docs/ROADMAP.md) preveu els altres sis enemics, quatre semibosses, mercader i màquina expenedora, **Personalitzar** i l'Almanac inicial. L'inventari de cosmètics és la pantalla **Personalitzar**. Els huit bosses nous, els blocs del mode creador i els amulets tenen fites posteriors.

Quan Ferran trie què fer primer, segueix el disseny corresponent de `Docs/Dissenys/` i comprova el canvi en individual i en cooperatiu. L'amfitrió decideix el combat i el client replica l'estat. Si canvies el format dels missatges o la generació compartida, actualitza el protocol i [ONLINE.md](ONLINE.md).

## Compilacions i registres

- Artefactes de publicació en `Builds/Distribucio/Alpha-v1.4.0-GitHub/`; registres en `Builds/Candidats/Alpha-v1.4.0-20261121/Logs/` i `Proves/`. Estes carpetes són locals. GitHub conté els paquets i les notes per a descarregar.
- Els scripts de compilació, verificació i empaquetat admeten `-BuildRoot`. Usa la mateixa ruta per a les tres plataformes i una carpeta a banda si hi ha una còpia del joc oberta.
- La font nova distribuïda és `Assets/Resources/Fonts/ChakraPetch-Bold.ttf`, amb la llicència en `Assets/StreamingAssets/Llicencies/`. Les fonts de joc preexistents s'han conservat.

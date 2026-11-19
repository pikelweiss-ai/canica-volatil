# Sales online — Canica Volàtil Alpha

**Protocol d'esta branca de proves: 8.** Windows, Linux, Android i el servidor de sales han d'usar la mateixa revisió. L'Alpha v1.0.0 publicada usa protocol 5. Les instruccions de compilació estan a MOBILE_ALPHA.md.

On està: **JUGAR → targeta "Online"**. Obri el navegador de sales.

## Estat actual de la prova

La branca és `alpha/bugfixes` i usa protocol 8. La suite Windows ha passat 4/4 al mateix PC: llista i codi LAN, i llista i codi amb relay local. També hi ha una prova anterior de masmorra per IP directa en loopback (protocol 8). Queda validar la connexió directa entre dos dispositius i després entre dos xarxes domèstiques. El relay accessible per Internet queda pendent de desplegament i prova remota.

| Via | Què comparteix l’amfitrió | Què cal |
|---|---|---|
| LAN | Codi LAN o adreça i port | Mateixa xarxa local. El codi es resol per descobriment UDP. |
| IP directa | Adreça i port TCP real de la sala | L’altre jugador ha de poder arribar al PC de l’amfitrió. Per Internet, normalment cal configurar el reenviament de port. Amb CGNAT, eixa configuració del router pot ser insuficient. |
| Internet amb codi curt | Codi de sis caràcters | Els dos jugadors han de configurar el mateix relay accessible. El PC de qui crea la sala porta la simulació. |

Per provar la via directa, crea una sala **Xarxa local** i, a l’altre joc, introduïx `IP:port` al camp **Unir-se amb codi**. El port habitual és TCP 7777. Usa el que haja obert realment el joc si el 7777 estava ocupat.

## Què pots fer

| Pantalla | Què hi ha |
|---|---|
| **Navegador de sales** | Pestanyes **INTERNET · PÚBLIQUES** i **XARXA LOCAL · LAN**. Taula amb nom, amfitrió, mode, jugadors, estat, ping i cadenat si té contrasenya. Cerca, filtres (amagar plenes / amb contrasenya), actualització automàtica, doble clic per entrar. |
| **Unir-se amb codi** | Codi de 6 lletres (ex. `K7QX2M`) o una IP directa (`192.168.1.20:7777`). Busca primer a la xarxa local i després al servidor d'Internet. |
| **Crear sala** | Nom, mode (**Masmorra** o **Mapa lliure**), jugadors màxims (2–8), **pública** (surt a la llista) o **privada** (només amb codi), contrasenya opcional, xarxa **local** o **Internet**. |
| **Lobby** | Els jugadors apareixen al mapa del Lobby i es mouen mentre esperen. Panell amb la llista (AMFITRIÓ / LLEST / esperant, ping), codi amb botó COPIAR, xat amb bafarades damunt dels jugadors. Els jugadors premen **ESTIC LLEST** (o `R`). L'amfitrió tria el mode i prem **COMENÇAR** quan tots estan llestos → compte enrere de 3 s (s'atura si algú es desmarca). L'amfitrió pot expulsar (X). |
| **Partida** | Tots carreguen el mateix mode. A la masmorra tots tenen **la mateixa masmorra** (mateixa llavor per pis). Es veuen els altres jugadors (amb skin, barret i nom), llista de la sala amb la vida actual i màxima a la dreta i xat. Intro obri el xat per defecte. En Controls es pot canviar la tecla i l'obertura amb clic. L'amfitrió té **TORNAR TOTS AL LOBBY**. |

Missatges clars quan: la sala està plena, ja ha començat, contrasenya incorrecta, versió diferent, l'amfitrió tanca la sala o et expulsa, es perd la connexió (es torna al navegador de sales amb l'avís).

## Com funciona per dins

- Sense paquets externs: `System.Net.Sockets` (TCP per a la sala, UDP per a trobar sales en LAN). Scripts a `Assets/Scripts/Net/`.
- **Estrella**: cada jugador parla només amb l'amfitrió; l'amfitrió decideix i reenvia estat de la sala, xat i posicions (20 per segon).
- **LAN**: l'amfitrió obri el port TCP **7777** (o el següent lliure fins al 7787) i respon al descobriment UDP **7778**. Les sales privades no responen a la llista però sí al seu codi.
- **Internet**: el **servidor de sales** guarda la llista de sales públiques i fa de pont (relay) entre amfitrió i jugadors, així **ningú ha d'obrir ports al router**. Port TCP **7780**.
- La contrasenya no viatja en clar: s'envia `SHA-256("canica:" + codi + ":" + contrasenya)`.

## Servidor de sales (per a jugar per Internet)

És el mateix joc arrancat sense finestra:

```
CanicaAlphaReview.exe -batchmode -nographics -canicaServer 7780
./CanicaAlphaReview.x86_64 -batchmode -nographics -canicaServer 7780
```

Es pot posar en una màquina d'AWS (Linux) obrint el port TCP 7780 al *security group*. Després, al joc: pestanya **INTERNET**, camp **SERVIDOR DE SALES** → `ip-o-domini:7780` → **DESA**.

Sense servidor configurat, es pot jugar igualment en **xarxa local** o **per IP/VPN** (Tailscale, Radmin, ZeroTier...) amb l'adreça `IP:7777`.

La primera vegada que crees una sala, Windows pot preguntar pel **tallafocs**: cal permetre-ho en xarxes privades perquè els altres et troben.

## Prova online al mateix ordinador

Executa `Tools/Net/start-local.ps1` per iniciar el relay a `127.0.0.1:7780`. Amb `-OpenGame`, també s'obri el joc amb este servidor com a adreça per defecte. En el navegador de sales, entra en **INTERNET · PÚBLIQUES** i crea una sala d'Internet.

El paràmetre `-canicaLoopback` limita els ports del servidor, de les sales i del descobriment LAN a `127.0.0.1`. Les proves automàtiques l'utilitzen per poder fer amfitrió, client i relay al mateix PC.

## Proves automàtiques

```
powershell -ExecutionPolicy Bypass -File Tools\Net\test-online.ps1
```

Arranca instàncies sense finestra (`-canicaBot host|join`, `NetBot.cs`) i prova: llista LAN + contrasenya (sense, dolenta i bona) + mapa lliure; codi LAN + masmorra; servidor d'Internet + llista + contrasenya; codi per Internet. Cada cas comprova xat, llest, avatar visible al lobby i en partida, que amfitrió i jugador tenen la mateixa llavor i la mateixa masmorra (empremta de sales), tornar al lobby i tancar sala. Els casos de masmorra (`-botCoop`) comproven a més: enemics creats per l'amfitrió i còpies al client sense IA, un enemic mort pel colp del client, bala del company visible, cofre obert pel client i bloquejat per a l'amfitrió, dany enviat al company (5 -> 4 cors) i sala acabada per a tots.

Regressió del 19/11/2026, protocol 8, Windows al mateix PC: **4/4** amb client tàctil simulat. Logs a `Builds/SkinsReworkQA/Online/result.txt`. La prova directa anterior és a `Builds/TrailsBossTests/DirectAndRelay`.

Captures de les pantalles sense obrir finestres (`Builds/OnlineShots/`):

```
powershell -ExecutionPolicy Bypass -File Tools\Net\shots-online.ps1
```

Queda pendent provar dos equips físics, la connexió directa des de dos xarxes diferents, el relay remot i el joc en Linux físic.

## Cooperatiu (masmorra i mapa lliure)

L'**amfitrió mana** i els altres en veuen còpies (`Assets/Scripts/Net/NetCoop.cs`):

- **Enemics**: només l'amfitrió els crea i els mou (IA). Persegueixen el jugador més pròxim, siga qui siga. Als clients són còpies que segueixen les posicions de l'amfitrió (15 per segon) sense IA pròpia.
- **Dany als enemics**: si un client pega a una còpia (bala, espasa, puny, explosió), es veu el colp al moment i el dany l'envia a l'amfitrió, que és qui decideix si mor. Quan mor, mor per a tots, i cadascú rep el seu botí (monedes i cures).
- **Dany als jugadors**: cada jugador rep el dany al seu PC. Les bales enemigues es dupliquen a cada PC i fan mal a qui toquen. Les embestides, els colps del boss i les explosions de kamikazes els envia l'amfitrió al company afectat.
- **Sales**: quan entra el primer jugador viu, es tanquen totes les portes menys la d'entrada (les tres caselles). Quan hi són tots, es tanca també l'entrada i comença el combat. Si tots retrocedeixen, es cancel·la l'espera i es reobrin les portes. L'amfitrió decideix l'estat i el replica als clients, també quan acaben de carregar. Els jugadors caiguts no bloquegen l'inici.
- **Cofres**: en sales de combat/boss només es poden obrir després de netejar la sala, ni mentre falten jugadors ni durant les onades. L'amfitrió també valida les peticions dels clients. El primer que n'obri un se l'emporta; per a la resta queda obert i no es pot tornar a obrir. Els cofres de recompensa i el portal apareixen per a tots. Sala inicial i tresor conserven la interacció normal.
- **Portal**: quan algú el creua, **tota la colla** passa de pis alhora.
- **Caure i espectar**: si caus, **les teues armes i cures queden al terra** (les monedes no) i la càmera segueix un company (A/D o clic per canviar si n'hi ha diversos). Tornes amb tota la vida quan netegen la sala, als 4 segons si has caigut fora de combat (TNT, punxes) o al pis següent si els companys passen. Les armes i cures deixades al terra es poden recuperar mentre continuen allí. Si cauen tots, es torna a començar el pis 1 (la mateixa masmorra).
- **Monedes, cures i armes del terra**: les veu tothom; se les queda el primer que les agafa (l'amfitrió fa d'àrbitre). L'imant de les monedes va cap al jugador més pròxim (no sempre cap a l'amfitrió).
- **Monedes animades del mapa** (lobby, mapa lliure): només les agafa un jugador.
- **Radar**: els companys surten com a punts blau cel. El botí dels enemics el crea l'amfitrió; el de cofres i caixes, qui els obri o trenca.
- **Caixes i barrils (TNT)**: les caixes i les TNT estan protegides mentre falten companys per entrar i es poden trencar des que comença el combat. Els clients demanen cada colp a l'amfitrió, que valida l'estat de la sala, resta vida i replica el trencament. El dany de l'explosió el rep cada jugador al seu PC i als enemics només una vegada. Regla compartida per Android, Windows i Linux.
- **Sales de progressió**: l'amfitrió controla els polsos i el dany a la cambra de ressonància. Els clients reben l'avís de cada pols, l'efecte i el cofre final amb el mateix botí.
- **Boss**: barra de vida i avisos d'atac (cercles rojos) també als clients. Els cercles del boss es retiren quan mor; color i mida dels enemics (càrregues, kamikazes inflant-se) es copien.
- **Bales i atacs**: cada tret, tall o puny es veu als altres PCs (bala "cosmètica": no fa mal, ja l'ha comptat qui ha disparat). Tret real i còpia visual ignoren els colliders dels companys, inclòs el jugador local del receptor: tots veuen com el travessa.
- **Avatars dels companys**: skin, barret, estela (totes les formes), ombra, l'arma a la mà apuntant on apunten, i el nom.
- Qui arriba tard a l'escena rep l'estat sencer (sales, cofres oberts, portal, enemics vius i objectes que continuen al terra, sense duplicar-los).

## Correccions de la revisió de Ferran (Codex, 18/11)

- Cures: el receptor consulta `HealItem.All`, que carrega el catàleg de Resources/Items. Ja no depén de quines cures s'havien carregat abans. Armes: nou `NetContentCatalog`, generat per `NetContentBuilder` en compilar, amb referències explícites a totes les armes.
- Maximitzar: l'ajust de marges es conserva per a resolucions demanades al menú i inicialització; `Update` respecta els canvis natius de mida de Windows. Provat maximitzar, restaurar, tornar a maximitzar i F11 d'anada/tornada amb confirmació.
- Portes: estat d'espera autoritari, només una entrada oberta; estat 0 = espera cancel·lada, 1 = combat, 2 = netejada, 3–6 = espera amb costat d'entrada. Es conserva per a resincronització. Protocol **4**: tots els participants i el relay han d'usar aquesta compilació.

Verificació: `Tools/Net/test-online.ps1` passa **4/4** amb el Windows recompilat. Les proves cooperatives comproven les quatre cures rebudes en fred, resolució de totes les armes, cura d'un cofre obert pel client visible a l'amfitrió i cura d'un boss mort per l'amfitrió visible al client (probabilitats forçades només pel bot). Comproven també les tres caselles d'entrada obertes i totes les altres portes tancades, tant a host com a client. Logs: `%TEMP%/canica-online-fixes`. Windows i Linux: zero errors; Linux encara no executat. Pegat Windows comprovat (ja aplicat, exit 183).

Encara pendent de prova específica: retirada de tots durant l'espera, entrada iniciada pel client, reconexió/resincronització durant l'espera i dos PCs reals. Els bots comproven els Tilemap de portes i els objectes de cura; no substitueixen la revisió visual de Ferran.

## Pendent (no està fet encara)

- Les trampes de punxes són de cada jugador.
- Linux compilat però encara no executat en Linux Mint real.
- Si l'amfitrió ix, la sala es tanca (no hi ha migració d'amfitrió).
- Comptes i perfils al núvol: previst per a la Beta.

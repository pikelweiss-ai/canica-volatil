# Sales online — Canica Volàtil Alpha

**Protocol: 15 (Alpha v1.3.0: cooldown antic i canvis d'armes; el resum de partida ve de la v1.2.0).** En acabar la partida cooperativa, tots veuen el resum. Només l'amfitrió tria: **Tornar a jugar** envia `RestartRun` (96) i tota la colla torna al pis 1; **Tornar al lobby** fa servir el retorn al lobby de sempre. Els clients esperen o ixen de la sala. Els paquets locals de la v1.2.0 anteriors usen el protocol 12 i no són compatibles amb aquest. Al protocol 12, els objectes del terra sincronitzen els trets gastats i la calor. L’amfitrió aplica els efectes dels projectils i envia els rajos del Projector de polsos. La v1.1.3 usa el protocol 10. `PickupSpawn` inclou el cooldown pendent de cada arma, també en resincronitzar. La v1.1.1 usa protocol **8**. Windows, Linux i Android han d’usar la mateixa versió. L’Alpha v1.0.0 usa protocol 5 i no és compatible. Les instruccions de compilació estan a MOBILE_ALPHA.md.

On està: **JUGAR → targeta "Online"**. Obri el navegador de sales directament a **INTERNET · PÚBLIQUES**.

## Internet sense IP (des de la v1.1.1) — la forma normal de jugar

Crear una sala d’Internet i entrar-hi **no demana cap IP, cap port obert, cap VPN ni cap servidor propi**.

1. **Amfitrió:** **Crear sala** → xarxa **INTERNET** (per defecte). Al lobby, la fila de dalt passa de *CONNECTANT* a **EN LÍNIA**.
2. **Amics:** la sala els surt a **INTERNET · PÚBLIQUES**. Doble clic per a entrar. Les privades s’hi entra amb el **codi**.

Per dins (`Assets/Scripts/Net/NetCloud.cs`):

| Peça | Què fa | Servei |
|---|---|---|
| Túnel | L’amfitrió obri una connexió d’**eixida** al port 7835 de `bore.pub` (protocol obert de *bore*). El servei li assigna un port públic. Cada jugador que s’hi connecta arriba com una connexió TCP més a l’amfitrió (cua `accepted` de `NetSession`). | `bore.pub`, gratuït, sense compte |
| Directori | L’amfitrió publica un anunci JSON (codi, nom, mode, jugadors, fase, `bore.pub:port`) cada 20 s, o abans si canvia alguna cosa, però mai més d’un cada 4 s. Quan tanca, publica `closed`. Les públiques van al tema `canica-volatil-v12-sales`. Les privades van a un tema derivat del SHA-256 del codi. | `ntfy.sh`, gratuït, sense compte |
| Llista | El navegador llig els anuncis dels últims 75 s, es queda l’últim de cada codi i amaga els tancats o els que fa més de 50 s que no s’anuncien. Només accepta adreces `bore.pub:port`, perquè un anunci fals no puga portar ningú a un altre lloc. | — |
| Ping | Temps fins al túnel del jugador + el de l’amfitrió (aproximat). | — |

- **Tallafocs:** no cal per a Internet (tot són connexions d’eixida). La IP de l’amfitrió no es publica.
- **Si el camp SERVIDOR PROPI té una adreça**, s’usa el servidor de sales clàssic de més avall en lloc del túnel i el directori.
- **Proves:** `-canicaCloudTag <nom>` (tema propi), `-canicaNoLan` (força el túnel al mateix PC), `-canicaTunnel` i `-canicaDirectory` (canviar de servei). `Tools/Net/test-online.ps1` inclou els casos 5 i 6: llista sense IP amb contrasenya i masmorra cooperativa, i entrar amb el codi.

### Estat de la v1.1.1 (19/11/2026)

| Prova | Resultat |
|---|---|
| `test-online.ps1` (Windows, mateix PC) | **6/6**: LAN llista, LAN codi, servidor propi llista i codi, **sense IP llista** (contrasenya + masmorra cooperativa sencera) i **sense IP codi**. Els dos últims passen pels serveis públics reals. |
| Executable empaquetat (`CanicaVolatil.exe` de la release) | Amfitrió i jugador sense IP: PASS. |
| Pantalles reals amb gràfics (`Builds/OnlineShots/v111-sense-ip`) | Llista INTERNET amb sales «sense IP» (client en previsualització mòbil) i lobby **EN LÍNIA** amb el jugador dins. |
| **Partida real entre dues cases** | **Superada.** Ferran i un amic han jugat per Internet sense escriure cap IP: «funciona a la perfecció». |
| Linux real (Ubuntu/Mint) | Pendent. |

**Pendent de millorar (rework de connexió):** el túnel gratuït `bore.pub` és lluny, i el ping estimat ronda els 300 ms. Opcions: connexió directa amb UPnP quan el router ho permeta, túnel o servidor més pròxim, i interpolació/predicció al client. `ntfy.sh` limita les peticions per IP: molts jugadors darrere de la mateixa IP (una classe) podrien topar-hi.

## Estat de la prova anterior (v1.1.0)

La branca és `alpha/bugfixes` i usa protocol 8. La suite Windows ha passat 4/4 al mateix PC: llista i codi LAN, i llista i codi amb relay local. També hi ha una prova anterior de masmorra per IP directa en loopback (protocol 8). Queda validar la connexió directa entre dos dispositius i després entre dos xarxes domèstiques. El relay accessible per Internet queda pendent de desplegament i prova remota.

| Via | Què comparteix l’amfitrió | Què cal |
|---|---|---|
| LAN | Codi LAN o adreça i port | Mateixa xarxa local. El codi es resol per descobriment UDP. |
| IP directa | Adreça i port TCP real de la sala | L’altre jugador ha de poder arribar al PC de l’amfitrió. Per Internet, normalment cal configurar el reenviament de port. Amb CGNAT, eixa configuració del router pot ser insuficient. |
| Internet amb codi curt | Codi de sis caràcters | Els dos jugadors han de configurar el mateix relay accessible. El PC de qui crea la sala porta la simulació. |

Per provar la via directa, crea una sala **Xarxa local** i, a l’altre joc, introduïx `IP:port` al camp **Unir-se amb codi**. El port habitual és TCP 7777. Usa el que haja obert realment el joc si el 7777 estava ocupat.

## Prova de connexió directa entre dos cases (19/11/2026, abans de la v1.1.1)

> Amb la v1.1.1 ja no cal: la partida entre dues cases s’ha fet per Internet sense IP. Esta secció es queda per a qui vulga la via de la IP directa.

**Menú.** Al lobby, l'amfitrió veu una fila **IP DIRECTA** amb la IP local i el port TCP que ha obert de veritat el joc (7777 o el següent lliure), i el botó **COPIAR IP**. Eixa adreça val per a la mateixa xarxa. Per a un amic d'una altra casa cal donar-li la **IP pública** de l'amfitrió amb el mateix port, i el router ha de reenviar eixe port TCP cap al PC.

**Què cal comprovar al PC de l'amfitrió:**

| Comprovació | Què mirar |
|---|---|
| IP local / porta d'enllaç | La IP local del PC (la mostra la fila IP DIRECTA) i la del router. |
| Perfil de xarxa de Windows | Ha de ser **Privada**. |
| Tallafocs | Cal una regla d'entrada TCP i UDP (perfil privat) per a l'executable del joc. Windows la demana la primera vegada que crees una sala. |
| CGNAT | Si la IP WAN del router és `100.64.x.x` o privada, hi ha CGNAT i el reenviament de port no basta (millor una VPN tipus Tailscale). |
| Reenviament de port | TCP 7777 (o el port que mostre el lobby) cap a la IP local de l'amfitrió, millor amb una reserva DHCP per a eixa IP. |

**Passos per a la prova real** (amfitrió a casa, l'altre PC en una altra xarxa, p. ex. el portàtil amb la dada mòbil compartida del telèfon):

1. Els dos equips amb la mateixa compilació (protocol 8).
2. Amfitrió: **Crear sala** → xarxa **local** → al lobby, apuntar el port de la fila IP DIRECTA.
3. Router: reenviament TCP d'eixe port cap a la IP local de l'amfitrió.
4. Client: **Unir-se amb codi** → `IP-pública:port`.
5. Comprovar: entra al lobby, xat i LLEST; l'amfitrió comença la masmorra; es veuen moviments, bales i dany; un cofre i un enemic mort es veuen als dos; l'amfitrió prem **EIXIR DE LA SALA** i el client torna al navegador amb l'avís de sala tancada.

**Provat amb equips reals:** encara res entre dos ordinadors. Només el diagnòstic de xarxa de l'amfitrió i la compilació amb la fila nova.

**Proves al mateix PC amb la fila nova (protocol 8, Windows):** compilació sense errors. Primera passada de `test-online.ps1`: 2/4. LAN codi va fallar per una violació d'accés compartit a `codi-lan.txt` (cursa del script de prova). Internet llista va fallar esperant el cor de dany del pols de la cambra de ressonància. Les dues passades següents han donat **4/4**. Captura del lobby a `Builds/OnlineShots/4-lobby-amfitrio.png`.

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

Dos equips físics de cases diferents: **superat** amb la v1.1.1 (Internet sense IP). Queden pendents la connexió per IP directa entre xarxes diferents, un relay remot propi i el joc en Linux físic.

## Cooperatiu (masmorra i mapa lliure)

L'**amfitrió mana** i els altres en veuen còpies (`Assets/Scripts/Net/NetCoop.cs`):

- **Enemics**: només l'amfitrió els crea i els mou (IA). Persegueixen el jugador més pròxim, siga qui siga. Als clients són còpies que segueixen les posicions de l'amfitrió (15 per segon) sense IA pròpia.
- **Dany als enemics**: si un client pega a una còpia (bala, espasa, puny, explosió), es veu el colp al moment i el dany l'envia a l'amfitrió, que és qui decideix si mor. Quan mor, mor per a tots, i cadascú rep el seu botí (monedes i cures).
- **Dany als jugadors**: cada jugador rep el dany al seu PC. Les bales enemigues es dupliquen a cada PC i fan mal a qui toquen. Les embestides, els colps del boss i les explosions de kamikazes els envia l'amfitrió al company afectat.
- **Sales**: quan entra el primer jugador viu, es tanquen totes les portes menys la d'entrada (les tres caselles). Quan hi són tots, es tanca també l'entrada i comença el combat. Si tots retrocedeixen, es cancel·la l'espera i es reobrin les portes. L'amfitrió decideix l'estat i el replica als clients, també quan acaben de carregar. Els jugadors caiguts no bloquegen l'inici.
- **Cofres**: en sales de combat/boss només es poden obrir després de netejar la sala, ni mentre falten jugadors ni durant les onades. L'amfitrió també valida les peticions dels clients. El primer que n'obri un se l'emporta; per a la resta queda obert i no es pot tornar a obrir. Els cofres de recompensa i el portal apareixen per a tots. Sala inicial i tresor conserven la interacció normal.
- **Portal**: quan algú el creua, **tota la colla** passa de pis alhora. El portal blau és l'arribada del pis següent i torna tota la colla al portal del boss del pis anterior. L'amfitrió restaura les sales, els cofres i el botí que hi quedava.
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
- Millorar la connexió: el túnel gratuït afig retard (uns 300 ms estimats). Rework previst.
- Si l'amfitrió ix, la sala es tanca (no hi ha migració d'amfitrió).
- Comptes i perfils al núvol: previst per a la Beta.

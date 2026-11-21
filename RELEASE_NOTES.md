# Alpha v1.4.0

- **Menú principal renovat.** Fons del portal més nítid, la canica original, botons amb tipografia Chakra Petch i el botó **Novetats** davall de **Controls**. Les tres expedicions tenen portades il·lustrades a partir dels sprites del joc.
- **Masmorres més variades.** Formes noves, plantes amb rutes alternatives, emboscades, sales secretes i modificadors de foscor, gel i flames. Les sales circulars grans tenen més cobertura i objectes trencables.
- **Arenes de tres onades.** Els enemics entren per costats diferents, la pantalla mostra l'onada i la recompensa final és un cofre rar o millor i cinc monedes.
- **Sis enemics nous.** Mímic, Sanador, Escut-guerrer, Talp, Franctirador de cristall i Banshee, amb efectes de so nous.
- **Portes animades i Llançaflames.** Les portes reaccionen a la proximitat. El Llançaflames es queda a la barra quan s'acaba el combustible.
- **Online.** Protocol **18**. Tots els participants han d'usar la mateixa compilació. Les claus per a portes queden pendents.

**Paquets:** Windows x86_64 (ZIP, obri `CanicaVolatil.exe`), Linux x86_64 (TAR.GZ, executa `./Jugar.sh`), Android 8+ ARM64/ARMv7 (APK) i sumes `SHA256SUMS.txt`.

**Verificació:** les tres plataformes compilen i coincideixen en protocol, codi i catàleg. En Windows han passat el bot del menú, el d'una masmorra sencera i el bot individual complet. La suite online amb client mòbil simulat ha donat **6/6**, inclosos els dos casos sense IP. També s'han verificat l'execució del ZIP de Windows i la firma de l'APK. Queda pendent provar Linux i Android en dispositius reals.

# Alpha v1.3.2

- **Menú principal** amb un panell de novetats al costat dels botons (les dues darreres versions) i botó VEURE-HO TOT.
- **Sprites propis:** la torreta té cúpula i un canó que gira cap a l'enemic, les mines tenen sprite i una llum que parpelleja, el Ganxo llança un harpó animat i la Vareta curativa dispara una bola verda amb una creu.
- **Barret de Cuiner:** arreglat el píxel que faltava a la vora.
- El protocol online continua sent el **16**: la v1.3.2 pot jugar amb la v1.3.1.

**Verificació i límits:** Windows, Linux i Android compilen. La prova individual passa; en online passen LAN i Internet (una vegada la prova de la Vareta curativa va fallar i va passar en repetir-la). Les proves d'Internet sense IP no es poden fer ara per la xarxa d'este equip.

# Alpha v1.3.1

- **Sis armes noves:** Bàcul de torreta, Llançamines, Arc de llum (de càrrega), Escut de mirall (reflecteix bales), Vareta curativa (cura companys en cooperatiu) i Ganxo d'abordatge.
- **Enemics nous:** Gàrgola, Cranc i Fantasma de gel. **Bosses nous:** Reina Aranya (Bosc i Cripta) i Rei de Gel (Neu).
- **Cofres progressius:** al pis 1 no hi ha armes èpiques ni llegendàries; milloren pis a pis. En cooperatiu, la sala inicial té un cofre comú per a cada jugador.
- **Barrets nous:** Mag, Cuiner, Orelles de gat, Cowboy i Antenes, i un barret de Pirata redissenyat. **Esteles noves:** Cors, Flamarada, Floc de neu, Diamants, Fulles i Estrelles de colors.
- **Nova pestanya OPCIONS** als Ajustos: cura intel·ligent (sí/no), números de dany, aim amb rebots i tremolor de càmera.
- Protocol online **16**: tots els jugadors han d'usar la v1.3.1.

**Verificació i límits:** Windows, Linux i Android compilen. La prova individual comprova les sis armes noves, els cofres progressius i que els enemics i bosses nous es creen i ataquen sense errors; passa en Windows. Les proves online de LAN i Internet passen; les d'Internet sense IP no s'han pogut repetir perquè ntfy.sh retornava 429. L'efecte de la Vareta curativa online, la torreta i les mines (només les veu qui les usa) no s'han provat a mà amb altres persones. El binari de Linux i l'APK no s'han executat en màquines o telèfons reals.

# Alpha v1.3.0

- **Cooldown de sempre:** les armes guardades no es recuperen (només les de cos a cos i l'escalfament de la Minigun) i canviar d'arma no fa esperar.
- **Ballesta de cristall:** alenteix un 20 % durant 1,5 s. **Disc de cometa:** sense zoom de càmera, l'aim ensenya tots els rebots i cada rebot allarga el recorregut un 15 %. **Espasa de vidre:** qualitat verda i +15 % de velocitat durant 1 s en colpejar.
- **Làser:** travessa tots els enemics, amb menys zoom i menys cadència. **Llançaflames:** 10 % menys de cadència. **Llança de tempesta:** colp en línia recta. **Minigun:** menys abast, un poc menys de cadència i bloqueig fins que l'escalfament torna a zero. **Projector de polsos:** un poc més d'abast. **Sniper:** un poc menys de zoom. **Vareta màgica:** 15 % menys de cadència. **Martell orbital:** 5 de dany.
- Les armes que no es citen no canvien, ni el zoom de càmera de les que no es citen.
- Protocol online **15**: tots els jugadors han d'usar la v1.3.0.

# Alpha v1.2.1

- **Balanç:** Sniper 8 de dany amb recàrrega més lenta i menys zoom; Làser 3 de dany i travessa 2; Vareta d'espores sense teledirigit; Espasa de vidre 4 de dany amb colp en V; Llança de tempesta amb menys abast i dash, i màxim 3 enemics per estocada; Ballesta de cristall més ràpida; Disc de cometa amb +1 de dany a cada rebot i més abast; Projector de polsos amb bala de 3 i salts de 2; el Martell orbital pega per damunt de les parets.
- **Cooldown:** les armes guardades també es recuperen (abans es quedaven congelades) i, en canviar d'arma, la nova espera la meitat del cooldown de l'última. El Llançaflames només es trenca a la masmorra.
- **Arreglos:** les armes cos a cos trenquen caixes i barrils; els enemics et veuen per damunt de l'aigua; els ítems d'un cofre cauen al voltant de qui l'obri; una sala s'activa també amb algú als raconets; el cofre del boss no apareix damunt d'un jugador; les caixes no es posen una damunt de l'altra.
- **Novetats:** panell de novetats clicant el número de versió del menú, lava i àcid amb bombolles i crosta, "_" al xat, cura intel·ligent amb el clic dret, cures iguals que s'ajunten.
- La llista de sales s'obri a INTERNET i el formulari avisa si una sala pública només serà per a la xarxa local.
- Protocol online **14**: tots els jugadors han d'usar la v1.2.1.

**Verificació i límits:** Windows, Linux i Android compilen sense errors. La prova individual passa en Windows. Les proves online LAN i Internet passen; les dues d'Internet sense IP no s'han pogut completar en esta passada perquè ntfy.sh retornava 429. L'ajust del cofre del boss i l'arreglo dels cofres online no s'han provat a mà amb altres persones. El binari de Linux i l'APK no s'han executat en una màquina o un telèfon reals.

# Alpha v1.2.0

- Passada de balanç de l'Arsenal amb rebots del Disc, una estocada de la Llança i un indicador circular per al Martell.
- Llançaflames amb 300 trets i Minigun amb calor. L'estat de cada arma passa amb ella entre jugadors i pisos.
- La Ballesta de cristall alenteix, la Vareta d'espores enverina i el Projector de polsos encadena fins a tres salts elèctrics.
- Bosses amb 25 punts de vida addicionals per pis, un atac més cada tres pisos i projectils més ràpids.
- Creu groga del Mode Creatiu i esteles darrere de les skins.
- **Skins noves i redissenyades per Ferran:** Llop, Ciclotep i Postal, i Flor i Irisada en el lloc de Corall i Obsidiana. També s'han refet Bombolla, Caramel, Cíclop, Cosmos, Coure, Fantasma, Gota, Llima, Menta, Ninja, Or, Perla, Safir i Vidre (ara semitransparent). Si portaves Corall o Obsidiana, ara portes Flor o Irisada.
- **Resum de partida** en morir: pis màxim, temps, monedes guanyades, enemics, bosses, arma favorita, sales netejades i cofres oberts.
- **Rècords** de pis màxim, enemics, bosses i monedes en una partida, separats per a sol i cooperatiu. El Mode Creatiu no compta.
- En acabar, **Tornar a jugar** o **Menú**. En cooperatiu, tria l'amfitrió (tornar a jugar tots o tornar al lobby).
- Protocol online **13**: tots els jugadors han d'usar la v1.2.0.

**Descàrregues:** Windows x86_64 (ZIP, obrir `CanicaVolatil.exe`), Ubuntu/Linux x86_64 (TAR.GZ, `./Jugar.sh`), Android 8+ ARM64/ARMv7 (APK amb clau debug de l'Alpha, no és de Google Play), `SHA256SUMS.txt` i `BUILD_INFO.json`.

**Verificació i límits:** Windows, Linux i Android compilen sense errors. La prova individual i les 6 proves online passen en Windows. El binari de Linux i l'APK no s'han executat en una màquina o un telèfon reals des d'aquest PC. La connexió per Internet va pel túnel gratuït (`bore.pub`), que afig retard.

# Alpha v1.1.3

En passar pel portal del boss, la colla ix pel portal blau del pis següent. Després d'eixir del cercle, es pot tornar a entrar per recuperar el botí del pis anterior. Les sales, els cofres i els objectes es conserven durant la partida. Protocol online **10**.

# Alpha v1.1.2

- Cada arma conserva el seu cooldown mentre està guardada, quan cau a terra i entre pisos.
- Els esbirros del boss i els llims dividits naixen dins de la seua sala.
- El botí queda apartat dels portals. El portal blau torna a la sala inicial del mateix pis i el morat avança tota la colla.
- L'amfitrió confirma les recollides online i els clients recorden els cofres oberts encara que el cofre es cree més tard.
- Els tirs enemics passen per damunt de l'aigua, els barrets segueixen l'ordre del jugador i les bafarades es veuen per damunt dels arbres.
- Protocol online **9**. Tots els participants han d'usar la mateixa versió.

# Alpha v1.1.1

**Online per Internet sense complicacions.** Crea una sala pública i els teus amics la veuen a la llista des de qualsevol casa, wifi o dades mòbils, i hi entren amb un clic o amb el codi de 6 lletres. **Sense escriure cap IP, sense obrir ports al router, sense VPN i sense instal·lar res més.** Funciona igual en Windows, Ubuntu/Linux i Android, i es pot jugar entre ells.

## Com es juga online

1. **JUGAR → Online.** S'obri directament la pestanya **INTERNET · PÚBLIQUES**.
2. **Crear sala** amb xarxa **INTERNET**, que és l'opció per defecte. En uns segons, al lobby es veu **EN LÍNIA**.
3. Els amics obrin **JUGAR → Online** i la sala ja els apareix a la llista (s'actualitza sola). Doble clic i dins. Si és **privada**, han d'entrar amb el **codi** de 6 lletres.

La pestanya **XARXA LOCAL** continua per a jugar a la mateixa wifi, amb menys retard.

## Què cal fer en cada sistema

- **Windows:** res. Per a les sales d'Internet no cal tocar el tallafocs, perquè el joc només fa connexions d'eixida. Només si crees una sala de **XARXA LOCAL**, Windows pot preguntar pel tallafocs la primera vegada: accepta **Xarxes privades**.
- **Ubuntu / Linux:** res. Extraure i `./Jugar.sh`.
- **Android:** res. Instal·lar l'APK. Només usa el permís d'Internet, que Android dona sol.

## Com funciona per dins

- **Túnel:** l'amfitrió obri una connexió d'eixida cap a un túnel públic gratuït (`bore.pub`), que li dona un port públic. Els jugadors entren per eixe port, i la **IP de casa de l'amfitrió no la veu ningú**.
- **Llista de sales:** el joc anuncia la sala cada 20 segons en un directori públic gratuït (`ntfy.sh`), i quan tanca s'esborra de la llista. Les privades no surten a la llista: es troben només amb el codi.
- **Partida:** dins va pel protocol de sempre, el **8**. Tots han de tindre la **v1.1.1** per a veure les sales sense IP.
- **Servidor propi (opcional):** el camp **SERVIDOR PROPI**, avall de la pestanya Internet, continua per a qui en vulga un. Si el deixes buit, és automàtic.

## Descarregar

- **Windows x86_64 — ZIP:** extraure la carpeta completa i executar `CanicaVolatil.exe`.
- **Ubuntu / Linux x86_64 — TAR.GZ:** extraure i executar `./Jugar.sh`, o `bash Jugar.sh`.
- **Android 8+ — APK:** ARM64 i ARMv7, signada amb la clau debug de l'Alpha; no és de Google Play.
- **SHA256SUMS.txt** i **BUILD_INFO.json**.

## També inclou (de la v1.1.0)

Mode **Creatiu** al mapa lliure (armes, menjar, enemics, cofres i botons per a treure'ls), arreglos de la llista de bugs de l'Alpha v1.0.0, bosses més durs, sala d'elit i cambra de ressonància.

## Verificació i límits

- Proves automàtiques **6/6** en Windows, entre elles dos casos nous: Internet sense IP (llista amb contrasenya i masmorra cooperativa) i entrar amb el codi. Són proves de debò a través del túnel i el directori públics.
- **Ping:** el túnel gratuït és lluny (el joc estima uns 300 ms de ping en total per a qui entra). En cooperatiu va bé, perquè cadascú es mou al seu PC. Per a jugar amb el mínim retard, usa la XARXA LOCAL.
- **Serveis externs:** l'online sense IP depén de `bore.pub` i `ntfy.sh`. Si algun dia no responen, el joc ho avisa i la XARXA LOCAL continua funcionant.
- **Prova real superada:** Ferran i un amic han jugat per Internet des de dues cases diferents, sense escriure cap IP.
- **Pendent:** millorar la connexió (rework) i provar Linux Mint/Ubuntu en una màquina real.

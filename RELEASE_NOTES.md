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

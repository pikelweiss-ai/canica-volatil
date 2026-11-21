# Alpha Android i joc entre plataformes

Windows, Linux i Android es compilen des del mateix projecte, amb el mateix catàleg d'armes i cures. **Alpha v1.4.0, protocol 18.** Tots els jugadors han d'usar la mateixa compilació.

## Jugar

1. Android: instal·lar `CanicaVolatil-Alpha-v1.4.0-Android.apk` de la [publicació](https://github.com/pikelweiss-ai/canica-volatil/releases/tag/v1.4.0-alpha). Android 8 o posterior, ARM64/ARMv7 i orientació horitzontal. APK signada amb la clau de depuració de l'Alpha.
2. PC: extraure el paquet de Windows o Linux de la mateixa publicació. En Windows, obri `CanicaVolatil.exe`. En Linux, executa `./Jugar.sh`.
3. **Internet (sense IP):** JUGAR > ONLINE > pestanya INTERNET. Un jugador fa CREAR SALA (Internet ja ve triat); els altres la trien de la llista o entren amb el codi de 6 lletres. Sense ports, VPN ni servidor. Funciona amb wifi o dades mobils, des de cases diferents; Android, Windows i Linux usen el mateix protocol i poden jugar junts (Linux encara sense provar en una maquina real).
4. Mateixa Wi-Fi/xarxa (menys retard): pestanya XARXA LOCAL. Una Wi-Fi de convidats amb aillament de dispositius pot impedir la connexio local. El servidor de sales propi (`-batchmode -nographics -canicaServer 7780`) continua com a opcio avançada.

Joystick esquerre per moure, dret per apuntar/atacar; tocar armes/cures; OBRIR mantingut; TIRAR ARMA; MAPA; pausa/ajuda per les tres barres. Al lobby hi ha joystick de moviment, botons de llest/començar i xat tactil. En caure, botons < i > per triar company.

## Compilar sense separar versions del codi

Unity 6000.0.53f1 amb moduls Android i Linux instal·lats. Des del projecte:

```powershell
./Tools/Build/build-alpha.ps1 -Target Android
./Tools/Build/build-alpha.ps1 -Target Windows
./Tools/Build/build-alpha.ps1 -Target Linux
```

El script prepara l'Input Manager (0 per Android, 2 per PC) i reinicia Unity abans de compilar: URP serialitza camps diferents segons el sistema d'entrada i falla si es canvia dins de la mateixa sessio de compilacio. No executar Editors simultanis al mateix projecte. Acabar amb Windows/Linux deixa l'Editor preparat per PC.

Cada paquet te `compatibility.json`: protocol, versio, hash SHA256 del codi compartit i noms de totes les armes/cures. Han de coincidir entre les tres plataformes, excepte el camp platform. El mateix codi no substitueix la prova en dispositius reals.

Comprovar-ho amb `./Tools/Build/check-crossplay.ps1`. Usa el mateix `-BuildRoot` en els tres builds, el verificador i l'empaquetador si vols separar els fitxers d'una compilació de prova.

Android usa nomes permisos de xarxa normals (Internet, estat de la xarxa i la Wi-Fi, MulticastLock per a la LAN), que el sistema concedix sense preguntar. No necessita serveis Unity ni subscripcio de Relay: l'online sense IP va pel tunel bore.pub i el directori ntfy.sh. La connexio es perd si el sistema suspén el proces: mantindre el joc en primer pla mentre s'hostatja una sala.

## Proves

```powershell
./Tools/Net/test-online.ps1
./Tools/Net/test-online.ps1 -MobileClient
```

La segona variant combina un bot PC i un bot amb els controls/arrancada mobils activats en Windows. Comprova xarxa i rutes de joc compartides; NO executa Android ni Linux. `MobileInputChecks.Validate` comprova dos dits independents, velocitat analogica, cancel·lacio i pausa.

Els tests cooperatius inclouen projectils reals/visuals travessant companys, cures de cofre/boss, portes, cofres protegits fins que s'acaba el combat, caixes protegides en espera i trencables durant el combat, recollida de monedes i recuperacio de cures i armes en reapareixer, amb vida plena. L'estat final de les compilacions i les proves consta en ALPHA_PROGRESS.md.

La v1.4.0 ha passat les sis proves en Windows amb client mòbil simulat. En una versió anterior, Ferran i un amic ja van jugar per Internet sense IP des de dues cases diferents. Queda pendent jugar amb la v1.4.0 en Android i Linux reals.

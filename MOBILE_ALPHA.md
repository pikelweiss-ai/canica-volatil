# Alpha Android i joc entre plataformes

Windows, Linux i Android es compilen des d'aquest mateix projecte Alpha, amb el mateix cataleg d'armes i cures. Esta branca de proves usa el protocol **8**. Tots els jugadors i el relay han d'usar la mateixa revisio. La versio Alpha v1.0.0 publicada usa el protocol 5.

## Jugar

1. Android: instal·lar `Builds/AlphaReview/Android/CanicaVolatil-Alpha-v1.0.0-Android.apk`. Android 8 o posterior, ARM64/ARMv7, orientacio horitzontal. APK de revisio signada amb clau debug, no publicacio de Play Store.
2. PC: executar el paquet nou de Windows o Linux de `Builds/AlphaReview`.
3. Mateixa Wi-Fi/xarxa: JUGAR > ONLINE > XARXA LOCAL. Un jugador crea la sala; els altres entren per llista o IP:port. Una Wi-Fi de convidats amb aillament de dispositius pot impedir la connexio local.
4. Internet: configurar el mateix servidor de sales en tots els dispositius. El relay es el joc nou executat amb `-batchmode -nographics -canicaServer 7780`; al servidor, port TCP 7780 accessible. No s'ha desplegat cap servidor remot nou en aquesta tanda.

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

Comprovar-ho amb `./Tools/Build/check-crossplay.ps1`. En Linux, extraure la carpeta completa i executar `bash Jugar.sh`; el llancador dona permis d'execucio al binari.

Android inclou permis de xarxa i suport MulticastLock per a descobriment LAN Wi-Fi; no necessita serveis Unity ni subscripcio de Relay. La connexio es perd si el sistema suspén el proces: mantindre el joc en primer pla mentre s'hostatja una sala.

## Proves

```powershell
./Tools/Net/test-online.ps1
./Tools/Net/test-online.ps1 -MobileClient
```

La segona variant combina un bot PC i un bot amb els controls/arrancada mobils activats en Windows. Comprova xarxa i rutes de joc compartides; NO executa Android ni Linux. `MobileInputChecks.Validate` comprova dos dits independents, velocitat analogica, cancel·lacio i pausa.

Els tests cooperatius inclouen projectils reals/visuals travessant companys, cures de cofre/boss, portes, cofres protegits fins que s'acaba el combat, caixes protegides en espera i trencables durant el combat, recollida de monedes i recuperacio de cures i armes en reapareixer, amb vida plena. L'estat final de les compilacions i les proves consta en ALPHA_PROGRESS.md.

No hi ha telefon connectat ni Linux executable en aquest entorn: falta confirmar una partida Android + Windows + Linux amb dispositius reals, multitàctil fisic i descobriment Wi-Fi. Publicacio Alpha v1.0.0 autoritzada per Ferran; descarregues en https://github.com/pikelweiss-ai/canica-volatil/releases/tag/v1.0.0-alpha .

# Alpha v1.0.0

Primera Alpha oficial de Canica Volàtil, amb paquets Windows, Ubuntu/Linux i Android.

## Descarregar

- **Windows x86_64 — ZIP:** extraure la carpeta completa i executar `CanicaVolatil.exe`.
- **Ubuntu / Linux x86_64 — TAR.GZ:** extraure i executar `./Jugar.sh`, o `bash Jugar.sh`. Binari Linux natiu, sense Wine.
- **Android 8+ — APK:** ARM64 i ARMv7, horitzontal, dos joysticks. Signada amb clau debug de l'Alpha; no és una publicació de Google Play.
- **SHA256SUMS.txt:** sumes dels tres paquets.
- **BUILD_INFO.json:** commit, protocol, codi i catàlegs compartits de les compilacions.

No cal Unity per jugar. Els «Source code» automàtics són per al desenvolupament.

## Novetats respecte de la Pre-Alpha

Sales online per codi/IP, LAN i Internet mitjançant relay propi; navegador, lobby, llest i xat. Cooperatiu amb amfitrió autoritari: enemics, dany, portes, cofres, botí, monedes, caixes/TNT, espectador i reaparició. Qui es queda el botí primer se l'emporta per a tots.

Huit ambients, textures renovades, lava/àcid connectats, fons continu, menú i tipografia renovats, radar configurable, pantalla completa i HUD reubicable. 21 armes, 25 skins amb cares diferents, 17 opcions de barret i 18 d'estela, incloent opcions sense accessori.

Android usa el mateix projecte que PC: dos joysticks independents, inventari tàctil, xat ENVIA, espectador i opcions adaptades.

## Correccions incloses

- Els projectils travessen els companys en les dues pantalles.
- Les TNT funcionen durant el combat; protegides mentre s'espera la resta.
- Cofres i caixes normals protegits fins a netejar la sala.
- Només l'entrada queda oberta durant l'espera; l'amfitrió replica les portes.
- Cures i armes carregades des de catàlegs complets; els objectes del terra es resincronitzen quan un client acaba de carregar, sense duplicar-los.
- Maximitzar/restaurar respecta la finestra de Windows; F11 i configuració de pantalla amb confirmació.
- Vida i cures separades en la interfície mòbil.

## Online i compatibilitat

Protocol **5**, mateix codi i catàlegs en els tres paquets. Actualitzar també el relay; les Pre-Alpha antigues no són compatibles.

LAN: crear sala i entrar des de la llista o IP:port. Internet necessita un servidor de sales configurat per tots: el mateix executable amb `-batchmode -nographics -canicaServer 7780`. Aquesta release no desplega un servidor públic permanent. Guia: [ONLINE.md](https://github.com/pikelweiss-ai/canica-volatil/blob/v1.0.0-alpha/ONLINE.md).

## Verificació i límits

Compilacions Windows, Linux i Android amb zero errors. Proves LAN/relay 4/4 en Windows amb client de controls mòbils: xat/lobby, mapa compartit, combat, dany, botí, cofres, TNT, portes, caigudes, canvi de pis i recuperació d'una cura perduda sense duplicats. Comprovacions sintètiques dels dos dits i igualtat de protocol/codi/catàlegs superades.

Windows executat fora de l'Editor i UI tàctil revisada en previsualització. APK alineada i amb signatures v2/v3 verificades. Windows i Android inclouen el pegat oficial Unity Application Patcher 1.3.3.

**Pendent de prova real:** Ubuntu/Linux Mint, l'APK final en telèfon, partida Android + Windows + Linux, i relay en una màquina remota. Les proves de controls mòbils corren en Windows i no substitueixen una prova Android física. No es promet rendiment a 240 FPS ni compatibilitat amb tots els dispositius.

Sense migració d'amfitrió; si ix, es tanca la sala. Trampes de punxes locals. Equilibri i casos especials continuen en revisió. Tenda, perfil/estadístiques, més tipus de sales i altres modes continuen pendents; comptes cloud per a Beta.

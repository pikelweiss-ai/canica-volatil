# Alpha v1.1.0

Segona Alpha de Canica Volàtil: arreglos de la sessió de proves de l'Alpha v1.0.0, un mode **Creatiu** al mapa lliure i l'online en proves. Paquets per a Windows, Ubuntu/Linux i Android.

> **L'online s'està provant.** Funciona en xarxa local i amb el servidor de sales al mateix PC (proves automàtiques 4/4), però encara falta la prova de veritat entre dos cases. Si jugues online, que tots tinguen la v1.1.0 (protocol 8): no és compatible amb la v1.0.0.

## Descarregar

- **Windows x86_64 — ZIP:** extraure la carpeta completa i executar `CanicaVolatil.exe`.
- **Ubuntu / Linux x86_64 — TAR.GZ:** extraure i executar `./Jugar.sh`, o `bash Jugar.sh`. Binari Linux natiu, sense Wine.
- **Android 8+ — APK:** ARM64 i ARMv7, horitzontal, dos joysticks. Signada amb clau debug de l'Alpha; no és una publicació de Google Play.
- **SHA256SUMS.txt:** sumes dels tres paquets.

No cal Unity per jugar. Els «Source code» automàtics són per al desenvolupament.

## Novetat: mode Creatiu (mapa lliure)

Al mapa lliure hi ha un botó amb una creu daurada davall del menú ☰ (o la tecla **C**):

- **Armes:** agafa qualsevol arma del joc.
- **Menjar:** poma, bena, poció i farmaciola.
- **Enemics:** crea qualsevol enemic o boss on cliques al mapa (clic dret o **Q** per a parar).
- **Cofres:** posa cofres de les quatre rareses i obri'ls amb la **E**.
- **Treure enemics / Treure objectes:** neteja el que has creat i el que hi ha solt pel terra.

No es guarda res: en eixir de la partida s'oblida tot. El mapa lliure ja no porta cofres de fàbrica i les monedes que hi agafes no se sumen al total. Online, només l'amfitrió de la sala té el Creatiu i els altres veuen el que crea.

## Millores i correccions

- Armes i cures recuperables quan caus en cooperatiu; no es perd una arma en agafar-ne dos amb una sola plaça lliure.
- Moviment correcte en la primera masmorra i en sales LAN després de reiniciar.
- Armes, cures i monedes que ja no cauen damunt de les parets.
- Projectils que ixen bé arran de paret; caixes i TNT que es trenquen durant el combat.
- Barrets ben ajustats, esteles regulars, avisos del boss que desapareixen quan mor i arma que no queda flotant quan cau el jugador.
- Patró fix en escopeta, vareta d'espores i shurikens; zoom per a les armes de molt d'abast.
- Xat amb tecla configurable (p. ex. **T**) i opció de no obrir-lo amb el ratolí.
- Bosses amb més vida segons els jugadors i més difícils a cada planta.
- A partir del pis 3, sala d'elit; des del pis 4, cambra de ressonància, amb el Projector de polsos i la Ballesta de cristall com a premi.
- Skins redibuixades, i Síndria, Or i Capvespre recuperades.
- Al lobby, l'amfitrió veu i copia la seua **IP directa** (IP:port) per a jugar sense servidor.

## Online i compatibilitat

Protocol **8**, mateix codi i catàlegs en els tres paquets. Cal actualitzar tots els jugadors i el relay. LAN: crear sala i entrar des de la llista, amb el codi o amb IP:port. Per Internet es pot usar la IP directa (amb el port reenviat al router o una VPN tipus Tailscale) o un servidor de sales configurat per tots. Aquesta release no desplega un servidor públic. Guia: [ONLINE.md](https://github.com/pikelweiss-ai/canica-volatil/blob/v1.1.0-alpha/ONLINE.md).

## Verificació i límits

Compilacions Windows, Linux i Android amb zero errors. Proves online 4/4 en Windows al mateix PC (LAN llista, LAN codi, Internet llista i Internet codi). La primera versió del mode Creatiu (armes, enemics i cofres) l'ha provada Ferran en solitari a Windows.

**Pendent de prova real:** online entre dos cases i entre dos ordinadors, el Creatiu en una sala amb dos PCs, Ubuntu/Linux Mint, l'APK en un telèfon i una partida Android + Windows + Linux. Sense migració d'amfitrió: si ix, es tanca la sala.

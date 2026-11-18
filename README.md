# Canica Volàtil · Alpha v1.0.0

Joc 2D de caniques, combat i masmorres fet amb Unity. Explora huit ambients, combina armes i juga en solitari o en cooperatiu.

## Descarregar i jugar

[**Descarregar Alpha v1.0.0**](https://github.com/pikelweiss-ai/canica-volatil/releases/tag/v1.0.0-alpha)

| Sistema | Paquet | Com començar |
|---|---|---|
| Windows x86_64 | [ZIP](https://github.com/pikelweiss-ai/canica-volatil/releases/download/v1.0.0-alpha/CanicaVolatil-Alpha-v1.0.0-Windows-x86_64.zip) | Extraure tota la carpeta i obrir `CanicaVolatil.exe`. |
| Ubuntu / Linux x86_64 | [TAR.GZ](https://github.com/pikelweiss-ai/canica-volatil/releases/download/v1.0.0-alpha/CanicaVolatil-Alpha-v1.0.0-Ubuntu-Linux-x86_64.tar.gz) | Extraure i executar `./Jugar.sh`; alternativa: `bash Jugar.sh`. |
| Android 8+ ARM64/ARMv7 | [APK](https://github.com/pikelweiss-ai/canica-volatil/releases/download/v1.0.0-alpha/CanicaVolatil-Alpha-v1.0.0-Android.apk) | Instal·lar l'APK i jugar en horitzontal amb dos joysticks. |

No cal instal·lar Unity. Mantín l'executable de PC amb les seues carpetes i biblioteques. Les sumes SHA256 i la informació de compilació estan en la release. Els paquets «Source code» són el projecte, no el joc preparat per executar.

## Què inclou

- Masmorres procedurals, huit ambients, enemics, onades i tres bosses.
- 21 armes, rareses, armes cos a cos, dash, projectils de retorn i seguiment.
- 25 skins amb expressions diferents, 17 opcions de barret i 18 d'estela (inclouen les opcions sense accessori).
- Menú renovat, radar transparent configurable, inventari, cures, controls reassignables, volums separats i opcions de pantalla.
- Sales per codi/IP, navegador LAN/Internet, lobby, xat i cooperatiu amb amfitrió autoritari.
- Botí compartit, cofres d'un sol ús, espectador en caure i retorn en netejar sala o passar de pis.
- Android amb dos joysticks, inventari tàctil, xat amb botó d'enviar i controls adaptats.

## Jugar amb amics

JUGAR → ONLINE. En la mateixa xarxa, un jugador crea una sala LAN i els altres entren per la llista o IP:port. Per Internet, tots configureu el mateix servidor de sales/relay. No s'inclou un servidor públic allotjat: la guia [ONLINE.md](ONLINE.md) explica com executar-ne un.

Windows, Linux i Android comparteixen protocol **5**, codi i catàlegs. Tots els jugadors i el relay han d'usar aquesta Alpha; la Pre-Alpha antiga no és compatible. La partida conjunta en dispositius Android/Linux reals encara està pendent de verificació.

## Controls de PC

WASD/fletxes: moure. Ratolí: apuntar. Clic esquerre/Espai: atacar. E mantinguda: obrir cofre. 1–4: armes. 5–8: cures. Clic dret: usar cura seleccionada. Z: deixar arma. M: ampliar mapa. Roda: armes; Maj+roda: cures. Tecles configurables en Controls.

Tres barres o Esc: menú de partida. F11: pantalla completa. Nom i cosmètics en Personalitzar; mapa, so, pantalla i HUD en Ajustos.

Els cofres i les caixes de sales de combat s'obrin/destrueixen després de netejar-les. Les TNT es poden detonar durant la baralla; estan protegides mentre falten companys per entrar.

## Estat i proves

És una **Alpha**, amb contingut i equilibri en desenvolupament. Windows, Linux i Android compilen sense errors. Les proves automàtiques LAN i relay passen 4/4 en Windows, incloent un client amb controls mòbils. Windows i la previsualització tàctil s'han revisat fora de l'Editor; Android té signatura i alineació verificades.

Encara no s'ha executat el binari Linux en Ubuntu/Mint ni l'APK final en un telèfon des d'aquest entorn. No es dona per provada una partida conjunta entre els tres sistemes ni un relay allotjat remotament. Sense migració d'amfitrió: si ix, es tanca la sala. [Notes de la release](RELEASE_NOTES.md).

## Desenvolupament

Unity **6000.0.53f1**, URP 2D, Input Manager. Llegir [AGENTS.md](AGENTS.md). Projecte versionat: Assets, Packages, ProjectSettings i eines; els binaris es distribueixen en Releases.

```powershell
./Tools/Build/build-alpha.ps1 -Target Windows
./Tools/Build/build-alpha.ps1 -Target Android
./Tools/Build/build-alpha.ps1 -Target Linux
./Tools/Build/check-crossplay.ps1
./Tools/Net/test-online.ps1 -MobileClient
```

Instal·lar els mòduls Android i Linux de Unity. El script prepara el sistema d'entrada i reinicia l'Editor abans de compilar cada plataforma. [Guia Android i joc entre plataformes](MOBILE_ALPHA.md) · [Online i relay](ONLINE.md) · [Pla Alpha](ALPHA_PLAN.md) · [Historial](ALPHA_PROGRESS.md).

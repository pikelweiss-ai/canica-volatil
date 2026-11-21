# Canica Volàtil · Alpha v1.3.2

Protocol online **16**. Tots els jugadors han d'usar la v1.3.2.

Joc 2D de caniques, combat i masmorres fet amb Unity. Explora huit ambients, combina armes i juga en solitari o en cooperatiu **per Internet sense escriure cap IP**.

## Descarregar i jugar

[**Descarregar Alpha v1.3.2**](https://github.com/pikelweiss-ai/canica-volatil/releases/tag/v1.3.2-alpha)

| Sistema | Paquet | Com començar |
|---|---|---|
| Windows x86_64 | [ZIP](https://github.com/pikelweiss-ai/canica-volatil/releases/download/v1.3.2-alpha/CanicaVolatil-Alpha-v1.3.2-Windows-x86_64.zip) | Extraure tota la carpeta i obrir `CanicaVolatil.exe`. |
| Ubuntu / Linux x86_64 | [TAR.GZ](https://github.com/pikelweiss-ai/canica-volatil/releases/download/v1.3.2-alpha/CanicaVolatil-Alpha-v1.3.2-Ubuntu-Linux-x86_64.tar.gz) | Extraure i executar `./Jugar.sh`; alternativa: `bash Jugar.sh`. |
| Android 8+ ARM64/ARMv7 | [APK](https://github.com/pikelweiss-ai/canica-volatil/releases/download/v1.3.2-alpha/CanicaVolatil-Alpha-v1.3.2-Android.apk) | Instal·lar l'APK i jugar en horitzontal amb dos joysticks. |

No cal instal·lar Unity ni res més. Mantín l'executable de PC amb les seues carpetes i biblioteques. Les sumes SHA256 i la informació de compilació estan en la release. Els paquets «Source code» són el projecte, no el joc preparat per executar.

## Jugar amb amics (sense IP)

1. **JUGAR → Online.** S'obri la pestanya **INTERNET · PÚBLIQUES**.
2. **CREAR SALA** (la xarxa INTERNET ja ve triada). En uns segons el lobby diu **EN LÍNIA**.
3. Els amics, des de qualsevol casa, wifi o dades mòbils, veuen la sala a la llista i hi entren amb doble clic. A les privades s'entra amb el **codi de 6 lletres**.

No cal obrir ports al router, ni tocar el tallafocs, ni posar una VPN, ni configurar cap servidor. Windows, Linux i Android poden jugar junts. Tots han d'usar la **v1.3.2** (protocol **16**).

Per a jugar a la mateixa wifi amb menys retard, hi ha la pestanya **XARXA LOCAL**. Com funciona per dins, i l'opció de servidor propi: [ONLINE.md](ONLINE.md).

## Què inclou

- Masmorres procedurals, huit ambients, enemics, onades, bosses més durs a cada planta, sala d'elit i cambra de ressonància.
- 29 armes, rareses, armes cos a cos, dash i projectils de retorn. Panell de **novetats** dins del joc: clica el número de versió del menú.
- 28 skins (moltes redissenyades per Ferran a la v1.2.0), barrets i esteles, amb opcions sense accessori.
- **Resum de partida** en morir (pis, temps, monedes, enemics, bosses, arma favorita, sales i cofres) i **rècords** de pis, enemics, bosses i monedes, separats per a sol i cooperatiu.
- **Mode Creatiu** al mapa lliure: agafar qualsevol arma o menjar i crear enemics, bosses i cofres. En eixir no es guarda res.
- Sales online per Internet sense IP, xarxa local, codi de sala, lobby, xat i cooperatiu amb amfitrió autoritari.
- Botí compartit, cofres d'un sol ús, espectador en caure i retorn en netejar sala o passar de pis.
- Android amb dos joysticks, inventari tàctil, xat amb botó d'enviar i controls adaptats.

## Controls de PC

WASD/fletxes: moure. Ratolí: apuntar. Clic esquerre/Espai: atacar. E mantinguda: obrir cofre. 1–4: armes. 5–8: cures. Clic dret: cura intel·ligent (tria la que encaixa amb la vida que et falta). Botó NOVETATS al menú principal. Z: deixar arma. M: ampliar mapa. Roda: armes; Maj+roda: cures. C: mode Creatiu (mapa lliure). Tecles configurables en Controls.

Tres barres o Esc: menú de partida. F11: pantalla completa. Nom i cosmètics en Personalitzar; mapa, so, pantalla i HUD en Ajustos.

## Estat i proves

És una **Alpha**, amb contingut i equilibri en desenvolupament.

- **Compilació:** Windows, Linux i Android compilen sense errors i comparteixen protocol, codi i catàlegs.
- **Proves automàtiques:** passen **6/6** en Windows, amb LAN, servidor propi i Internet sense IP a través dels serveis públics reals.
- **Prova real:** Ferran i un amic han jugat per Internet des de dues cases diferents, sense escriure cap IP.

**Pendent:** millorar la connexió (el túnel gratuït afig retard) i executar el binari en Ubuntu/Mint. Sense migració d'amfitrió: si ix, es tanca la sala. [Notes de la release](RELEASE_NOTES.md).

## Desenvolupament

Unity **6000.0.53f1**, URP 2D, Input Manager. Llegir [AGENTS.md](AGENTS.md). Projecte versionat: Assets, Packages, ProjectSettings i eines; els binaris es distribueixen en Releases.

```powershell
./Tools/Build/build-alpha.ps1 -Target Windows
./Tools/Build/build-alpha.ps1 -Target Android
./Tools/Build/build-alpha.ps1 -Target Linux
./Tools/Build/check-crossplay.ps1
./Tools/Net/test-online.ps1 -MobileClient
./Tools/Build/package-alpha.ps1
```

Instal·lar els mòduls Android i Linux de Unity. El script prepara el sistema d'entrada i reinicia l'Editor abans de compilar cada plataforma. [Guia Android i joc entre plataformes](MOBILE_ALPHA.md) · [Online](ONLINE.md) · [Pla Alpha](ALPHA_PLAN.md) · [Historial](ALPHA_PROGRESS.md) · [Índex de documentació i d'art](Docs/INDEX.md) · [Disseny v1.4](Docs/Dissenys/v1.4-bosses-enemics-sales.md).

## Versions anteriors

Totes les versions estan en [Releases](https://github.com/pikelweiss-ai/canica-volatil/releases). La més antiga és [**Canica Volàtil 2025**](https://github.com/pikelweiss-ai/canica-volatil/releases/tag/v2025): la primera versió de totes, de l'estiu de 2025, feta parlant amb un xat d'IA. És molt reduïda (només moure's, monedes i un enemic) i el seu codi està a la branca [`legacy/canica-volatil-2025`](https://github.com/pikelweiss-ai/canica-volatil/tree/legacy/canica-volatil-2025).

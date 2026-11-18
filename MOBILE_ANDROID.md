# Canica Volàtil · Android Pre-Alpha v1.0.0

Variant del joc per a mòbils Android, en horitzontal i amb dos joysticks. El codi està en la branca `mobile/android-prealpha-v1.0.0`; la versió de PC continua en `main`.

## Descarregar i jugar

[Obrir la publicació d'Android](https://github.com/pikelweiss-ai/canica-volatil/releases/tag/v1.0.0-mobile-prealpha) i descarregar **CanicaVolatil-Mobile-PreAlpha-v1.0.0.apk**, dins d'Assets. No cal baixar el codi font ni instal·lar Unity.

Obri l'APK al mòbil i segueix l'instal·lador d'Android. Si Android demana autorització per instal·lar des del navegador o el gestor de fitxers, l'ha de gestionar el propietari del dispositiu. És una APK per a instal·lació directa, fora de Google Play.

Requisits declarats: Android 8.0 (API 26) o posterior, processador ARM de 32 o 64 bits i gràfics compatibles amb Unity URP. No és per a iPhone ni per a dispositius Android x86. La compatibilitat i el rendiment de cada mòbil queden pendents de proves reals.

## Controls

- Joystick esquerre: moure's, amb velocitat analògica i diagonal limitada.
- Joystick dret: apuntar i atacar. Soltar-lo para el foc. Es pot usar mentre et mous.
- Tocar una casella d'arma: equipar-la. Tocar una cura: usar-la.
- **OBRIR**: mantindre'l prop d'un cofre sense moure's.
- **TIRAR ARMA**: deixar l'arma seleccionada al terra.
- **MAPA**: alternar el radar ampliat.
- Tres barres de dalt: pausa, ajustos, ajuda de controls i tornar al menú.

Els joysticks respecten la zona segura de la pantalla. La partida es pausa en perdre el focus o enviar l'aplicació al fons. En mòbil les caselles són tàctils; s'ha desactivat l'arrossegament de l'inventari per evitar tirar objectes per error mentre uses diversos dits. El nom s'edita amb el teclat d'Android en Personalitzar.

## Compilar

Unity **6000.0.53f1**, amb Android Build Support, SDK/NDK i OpenJDK instal·lats des de Unity Hub. Input Manager antic, IL2CPP, ARMv7 + ARM64, mínim API 26. Executar:

```
Unity.exe -batchmode -nographics -quit -projectPath "RUTA_PROJECTE" -buildTarget Android -executeMethod MobileBuild.BuildAndroid -logFile "android-build.log"
```

Eixida: `Builds/Android/CanicaVolatil-Mobile-v1.0.0.apk`. `MobileInputChecks.Validate` comprova els dos punters, alliberament independent, velocitat analògica, diagonals, cancel·lació i bloqueig en pausa abans de compilar.

`MobileBuild.BuildPreview` genera una previsualització Windows. Llançar-la amb `--mobile-preview` mostra la mateixa UI tàctil per revisar-la amb ratolí; això no substitueix provar el multitàctil en un telèfon.

La distribució usa el pegat oficial **Unity Application Patcher 1.3.3** per a CVE-2025-59489, aplicat a l'APK després de compilar. L'APK està signada amb una clau de depuració de pre-alpha; no és una publicació de Google Play. No s'inclouen claus privades en el repositori.

## Verificació i límits

S'indiquen les comprovacions exactes en les notes de la publicació. No hi ha cap mòbil connectat a l'entorn de compilació: no es dona per provada una partida executada en Android ni la compatibilitat amb tots els dispositius.

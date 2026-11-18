# Pre-Alpha mòbil v1.0.0 · Android

Primera variant Android de **Canica Volàtil**, amb els modes i la personalització de la Pre-Alpha de PC i una interfície tàctil pròpia.

## Descarregar

En **Assets**, baixa **CanicaVolatil-Mobile-PreAlpha-v1.0.0.apk** i obri-la al mòbil. No baixes «Source code» per jugar. L'APK ocupa aproximadament **58 MiB**.

Android **8.0 o posterior**, ARM de 32 o 64 bits. Pantalla horitzontal, amb rotació entre els dos costats. No és una versió per a iPhone. És una instal·lació directa, fora de Google Play; Android pot demanar al propietari que autoritze la instal·lació des del navegador o gestor de fitxers.

## Controls

- **Joystick esquerre:** moviment analògic.
- **Joystick dret:** apuntar i atacar; soltar-lo para l'atac. Els dos es poden usar alhora.
- **Armes i cures:** toca les caselles per equipar o curar-te.
- **OBRIR:** mantín el botó prop del cofre sense moure't.
- **TIRAR ARMA:** deixa l'arma seleccionada.
- **MAPA:** amplia o redueix el radar.
- **Tres barres:** pausa, ajustos, ajuda i eixir al menú.

Joysticks adaptats a la zona segura de la pantalla, barres d'inventari centrades i pausa automàtica quan l'aplicació passa al fons. En aquesta primera variant l'inventari funciona per tocs; l'arrossegament està desactivat per evitar descartar objectes accidentalment.

## Verificació

- APK IL2CPP compilada amb **zero errors**, per a `arm64-v8a` i `armeabi-v7a`.
- Set comprovacions automàtiques superades: dos dits simultanis, propietat de cada punter, alliberament independent del moviment, parada de foc, velocitat analògica, límit diagonal i cancel·lació/bloqueig en pausa.
- Previsualització Windows amb la UI mòbil: menú, entrada a masmorra, moviment, apuntat/atac, pausa, ajuda, ajustos i tornada al menú revisats. Sense excepcions en el registre de la prova.
- Aplicat **Unity Application Patcher 1.3.3** a l'APK, amb resultat correcte. [Informació oficial del pegat](https://discussions.unity.com/t/cve-2025-59489-patcher-tool/1688032).
- Signatura APK v2/v3 i alineació ZIP verificades. Signada amb clau de depuració de pre-alpha. Inclou `SHA256SUMS-Android.txt` per comprovar la descàrrega.

**Pendent:** provar-la en un telèfon Android real. No hi havia cap dispositiu connectat a l'entorn; el rendiment, el teclat del mòbil, la rotació i el multitàctil físic encara necessiten eixa prova. Aquesta publicació és una pre-alpha, sense multijugador.

El codi d'Android està en `mobile/android-prealpha-v1.0.0`, amb l'etiqueta `v1.0.0-mobile-prealpha`. La publicació de Windows i Linux continua disponible per separat.

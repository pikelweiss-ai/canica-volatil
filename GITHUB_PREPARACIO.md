# GitHub de Canica Volàtil

Repositori públic: https://github.com/pikelweiss-ai/canica-volatil
Branca principal: main. Alpha oficial: **Alpha v1.4.2**, etiqueta `v1.4.2-alpha` (les anteriors es conserven, fins a `v1.0.0-alpha`).

Descàrregues: https://github.com/pikelweiss-ai/canica-volatil/releases/tag/v1.4.2-alpha

- Windows x86_64: ZIP amb totes les dependències; executar CanicaVolatil.exe.
- Ubuntu/Linux x86_64: TAR.GZ amb permisos Unix i Jugar.sh.
- Android 8+: APK ARM64/ARMv7 amb controls tàctils.
- SHA256SUMS.txt i BUILD_INFO.json amb identificació de codi/commit i catàlegs.

La candidata **Alpha v1.5.0** porta el protocol 24. La publicació depén del permís explícit de Ferran i seguix els passos del [traspàs de la v1.5](TRASPAS_CODEX_V150.md#6-com-publicar-només-amb-permís-explícit-de-ferran).

Assets, Packages, ProjectSettings, eines i documentació versionats. Library, Temp, Logs, UserSettings, Builds i .utmp exclosos. Executables en Releases, no dins del Git. Les fonts dinàmiques i les metadades antigues reimportades per Unity es conserven localment sense incloure soroll de compilació en el commit.

Les Pre-Alpha anteriors es conserven amb les seues etiquetes. No es reescriu l'historial ni es fa push --force. Llegir RELEASE_NOTES.md per a controls, instal·lació, proves i límits reals.

Per a una versió nova: revisar codi i git status, compilar els tres sistemes, aplicar pegats oficials, validar signatures/hashes i manifests, provar online, empaquetar i comprovar els fitxers distribuïts. Crear commit i etiqueta, pujar paquets a un esborrany, comprovar els assets i publicar quan estiga autoritzat.

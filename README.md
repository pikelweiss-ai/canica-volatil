# Canica Volatil

Joc 2D d'accio i masmorres fet amb Unity. Primera versio: **Pre-Alpha v1.0.0**.

## Descarregar i jugar
Anar a **Releases**, obrir **Pre-Alpha v1.0.0** i descarregar el paquet del sistema:

- **Windows x86_64**: extraure el ZIP complet i obrir `CanicaVolatil.exe`. Mantindre l'executable, UnityPlayer.dll, MonoBleedingEdge i la carpeta de dades junts.
- **Linux Mint x86_64**: extraure el TAR.GZ i executar `./Jugar.sh`. Si el gestor de fitxers no deixa executar-lo, obrir un terminal dins de la carpeta i fer `bash Jugar.sh`.

No cal instal·lar Unity per jugar. El repositori es privat: les descarregues requereixen acces autoritzat.
La versio Linux es nativa; no necessita Wine. La compatibilitat concreta amb Mint s'ha de verificar en una maquina Mint.

## Controls inicials
WASD/fletxes: moviment. Ratoli: apuntar. Clic esquerre/Espai: atacar. E mantinguda: obrir cofres.
1-4: armes. 5-8: cures. Z: deixar arma. H: usar cura seleccionada. M: ampliar mapa.
Roda: armes; Maj+roda: seleccionar cura. Tecles i roda configurables en Controls.
Tres barres dalt a la dreta o Esc: pausa, ajustos, controls i tornar al menu.
Nom, skins, barrets i esteles en Personalitzar. Mida/abast del mapa i volums en Ajustos.

## Desenvolupament
Obrir `Canica Alpha` amb Unity **6000.0.53f1**, URP 2D. Consultar AGENTS.md i PREALPHA_v1.0.0.md.
Assets, Packages i ProjectSettings estan versionats; Library, Temp, Logs i Builds s'exclouen.
Scripts de generacio: Tools/PixelArt; assets de masmorra: menu Canica > Masmorra > Crear-ho tot.
Compilacio per terminal: Unity -batchmode -quit -projectPath RUTA -buildTarget Win64 -executeMethod ReleaseBuild.BuildWindows
Per Linux: -buildTarget Linux64 -executeMethod ReleaseBuild.BuildLinux (requereix Linux Build Support Mono).

## Estat de la pre-alpha
Inclou masmorres procedurals, huit temes, bosses, arsenal a distancia i cos a cos, inventari i cosmetica.
Online encara no disponible. Versio primerenca: queden proves de partida completa, equilibri i compatibilitat entre equips.
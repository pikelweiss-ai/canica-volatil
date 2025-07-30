# Canica Volàtil 2025

**La primera versió de totes.** Aquest és el projecte original del qual va nàixer
[Canica Volàtil](https://github.com/pikelweiss-ai/canica-volatil). Es publica tal com era,
com a peça de museu: una versió **MOLT reduïda**, sense masmorres, sense armes, sense
online i sense res del que té el joc actual.

> Només per a **Windows** i **Linux**. No té controls de mòbil, ni joystick, ni comandament.

## Descàrrega

A la release [Canica Volàtil 2025](https://github.com/pikelweiss-ai/canica-volatil/releases/tag/v2025) hi ha:

- `CanicaVolatil2025-Windows-x86_64.zip`: descomprimeix i obri `CanicaVolatil2025.exe`.
- `CanicaVolatil2025-Linux-x86_64.tar.gz`: descomprimeix i executa `./Jugar.sh` (o `bash Jugar.sh`).
- `SHA256SUMS.txt`: per comprovar que els fitxers estan bé.

## Controls

| Tecla | Què fa |
|---|---|
| **WASD** o **fletxes** | Moure's |
| **H** | *(prova)* Et fas 1 de mal a tu mateix. Amb 5 colps tornes al punt d'inici. |
| **J** | *(prova)* Fa 1 de mal a l'enemic. Amb 5 colps desapareix amb un fum. |
| **Ratolí** | Menú: escriure el nom, ajustos i jugar |

H i J són tecles de proves que es van quedar dins del joc: encara no hi havia cap arma
ni cap atac de veritat, així que servien per comprovar la vida, els cors i els sons.

## Què hi ha

- Menú principal amb ajustos i un camp per posar-te nom (apareix damunt del personatge).
- Un mapa fet a mà amb tiles: gespa, aigua (et fa anar més lent), pedra, maons, arbres,
  fusta i ombres.
- Monedes animades que pots arreplegar, amb comptador i so.
- 5 cors de vida, tremolor de càmera en rebre mal i reaparició.
- Un enemic (el "malo") que et persegueix i et fa mal en tocar-te.

## El llegat

Aquesta versió es va fer l'estiu de **2025**, i es va fer **sencera parlant amb un xat
d'IA**: preguntant, copiant el codi i muntant-ho tot a mà a Unity. No es va fer amb
agents de programació com Claude Code ni Codex, que són els que han construït les
versions noves.

L'única excepció és **l'enemic**: es va fer de la mateixa manera, amb el xat, però no es
va implementar fins al **juny de 2026**, quan es va reprendre el projecte. D'ací ve el
seu script de persecució, la vida, el fum en morir i els sons de robot.

D'aquest projecte venen coses que el joc actual encara arrossega: els noms dels scripts
(`PlayerHealth`, `EnemyDamage`, `CameraFollow`...), el text del nom amb TextMeshPro, les
monedes i la idea de la canica que es mou per un mapa de tiles.

## Detalls tècnics

- Unity **6000.0.53f1**, URP 2D.
- Escenes: `MainMenu` i `GameScene`.
- La build de Windows porta el pegat oficial de Unity (Unity Application Patcher 1.3.3)
  per a la vulnerabilitat CVE-2025-59489. A Linux no s'aplica.
- Respecte de l'original només s'ha afegit `Assets/Editor/LegacyBuild.cs` per compilar
  des del terminal, i el nom del producte ("Canica Volàtil 2025"). El joc és el mateix.

## Joc actual

Si vols el joc de veritat, amb masmorres, armes, bosses i online:
**https://github.com/pikelweiss-ai/canica-volatil**

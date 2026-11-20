# Context del consell · Alpha v1.2.0 publicada → v1.2.1 · 20/11/2026

Projecte: `C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace` (branca `alpha/arsenal`, publicada com a v1.2.0, protocol 13).
Dades d'armes i enemics: `dossier-canica.md` (en aquesta carpeta). Consell anterior (v1.1.1): `CONSELL.md` a l'arrel del projecte.
Altres documents: `DISSENY.md`, `ONLINE.md`, `RELEASE_NOTES.md`, `ALPHA_PLAN.md`. Codi: `Assets/Scripts` (masmorra a `Assets/Scripts/Dungeon/DungeonGenerator.cs`).

## El que diu Ferran (paraules seues, resumides fidelment)

- "Comença a ser **avorrit** tot i **no té gancho**. A mi se m'està fent avorrit." Vol saber si al consell també li ho pareix.
- "Hem de començar a fer **més modes**."
- Vol que opineu sobre la **correlació de les masmorres** i "tota la pesca": **totes les dungeons continuen sent iguals**, no tenen res de nou ni de gancho. Els **bosses**, tot.
- **Armes:** "encara poden estar desbalancejades". Valoreu la majoria d'aspectes i si **les armes serveixen dins de les masmorres**: "hi ha armes que acabes tirant i es queden obsoletes". Traieu una conclusió.
  - **Sniper:** necessita un **nerf**.
  - **Làser:** necessita un **nerf**.
  - **Vareta (bastó) d'espores:** el tir **no pot ser teledirigit** (ara té `homing 2`). Decisió presa.
  - **Espasa de vidre / cristall** (`EspasaVidre`): **no pega en V** (és un arc de 120°) i **hauria de fer mínim 4 de dany** (ara 3).
  - **Llança blava** (`LlancaTempesta`): necessita un **nerf**, "és com la de corall (`SabreCorall`) però superior". Atenció: per DPS teòric fa 3,5 i el Sable 6,2, però travessa enemics en línia, té abast 3,5 i dash 1,8. Expliqueu per què a les mans pot semblar superior i proposeu el nerf correcte.
- **Comptes i inici de sessió:** valoreu si interessaria un **sistema de comptes** perquè les **monedes per a gastar en skins** tinguen sentit: "si tens un perfil i les coses no es perden". Ferran dubta que es puga fer **sense servidor**. Doneu opcions realistes per a un desenvolupador sol (què es pot fer sense servidor propi i què no).
- Bug a arreglar a la v1.2.1: **BUG-021**, les armes cos a cos no trenquen caixes.

## Fets del joc (del codi, verificats)

- **Modes que existeixen:** masmorra (sol o cooperatiu fins a 4 online), mapa lliure, **Mode Creatiu** (al mapa lliure: crear enemics, bosses, cofres, agafar armes). No hi ha PvP ni cap altre mode.
- **Estructura d'un pis:** 7–10 sales (+1 per pis fins a +3). Tipus de sala: només `Start`, `Combat`, `Treasure`, `Boss`. Cada pis té una sala de progressió (cambra d'elit; als pisos 4+ amb polsos de "ressonància").
- **Combat:** 1–3 onades (2a onada al 50 %, 3a des del pis 3). Enemics per onada = 2 + pis + 0–1. Portes que es tanquen fins a netejar.
- **Temes:** 8 en cicle per pis (Cova, Bosc, Desert, Claveguera/àcid, Neu, Cripta, Volcà/lava, Cristall). Cada tema té enemics i boss propis, però el layout és el mateix generador.
- **Bosses:** Rei Llim, Golem Gegant, Mag Fosc (`EnemyBoss` amb atacs configurables). Vida base + 25 per pis, un atac més cada 3 pisos, bales més ràpides per pis.
- **Cofres:** comú 55 %, rar 30 %, èpic 12 %, llegendari 3 % (millora un 4 % per pis). 45 % de sales de combat amb cofre; 50 % de cofre en netejar-les; el boss dona un cofre llegendari i el portal.
- **Hotbar:** 4 armes + 4 cures. Les armes no milloren ni pugen de nivell: es canvien per altres.
- **Monedes:** cada enemic deixa 1–2. Es guarden en un total persistent (`CoinBank`, PlayerPrefs local), però **ara mateix no serveixen per a res**: els cosmètics (28 skins, barrets, esteles) són tots gratis.
- **Nou a la v1.2.0:** resum de partida en morir (pis, temps, monedes, enemics, bosses, arma favorita, sales, cofres) i rècords locals (pis, enemics, bosses, monedes; sol i cooperatiu separats). Tot local al PC/mòbil.
- **Online:** amfitrió autoritari, túnel públic gratuït `bore.pub` (uns 300 ms) i directori `ntfy.sh`. Sense servidor propi ni comptes. Si l'amfitrió ix, es tanca la sala.
- **Plataformes:** Windows, Linux i Android (APK), amb joc entre plataformes.
- Ferran és un estudiant de 17 anys i treballa amb assistents d'IA (Claude Code i ChatGPT/Codex). Recursos limitats: res de pagar, cap patró fosc.

## El que volem del consell

1. Si el joc té gancho o no, **per què** (amb dades), i què el faria tindre'n.
2. Varietat de masmorres i bosses: diagnòstic i propostes ordenades per impacte/esforç.
3. **Modes nous**: quins, en quin ordre i per què.
4. Balanç d'armes: les que es queden obsoletes, el seu rol dins de la masmorra, i números per a Sniper, Làser, Vareta d'espores, Espasa de vidre i Llança.
5. Comptes / perfil persistent: pros, contres i opcions realistes amb i sense servidor.
6. Conclusió final: què fer a la v1.2.1 i què a la v1.3.

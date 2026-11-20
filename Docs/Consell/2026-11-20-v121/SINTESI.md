# 🎲 Consell de Canica · Alpha v1.2.0 → v1.2.1 · 20/11/2026

19 perfils (11 de sempre i 8 de nous) han revisat la v1.2.0 publicada amb les dades del joc (`dossier-canica.md`) i el codi. Informes complets: `A-balanc.md` … `I-pvp.md` en aquesta carpeta.

> [!note] Límit
> Ningú ha jugat de veritat. Tot el que porta **[cal provar]** ho ha de decidir Ferran jugant (Mode Creatiu).

## ⚡ En 30 segons

1. **Ferran té raó: no té gancho.** Ho diuen 19 de 19. La nota mitjana és **5,6/10** (la v1.1.1 tenia 6,1). El problema és de **sistema**, no de números: cap partida canvia la següent, no hi ha decisions entre pisos, les armes no milloren i les monedes no serveixen per a res.
2. **Les masmorres són iguals de veritat (dades):** la geometria ix del mateix `rng.Next(7)` en els 8 temes, hi ha 12 rectangles de sala i una sala de boss de 19×15 sempre igual, hi ha aigua en 6 de 8 temes i **només 3 bosses per a 8 temes** (el Golem Gegant als pisos 2, 5 i 7). El mapa és un arbre sense cap decisió de ruta.
3. **Hi ha un exploit que invalida el balanç:** canviar d'arma no té cap retard i cada casella té el seu cooldown, així que, canviant entre 4 armes, **les DPS se sumen (~26 DPS)**. Cal arreglar-ho junt amb els nerfs.
4. **Els 5 canvis d'armes de Ferran: tots a favor**, amb matisos de número. La Llança no és millor pel dany (3,5 DPS contra els 6,2 del Sable), sinó per **seguretat**: pega des de fora del contacte, travessa tota la fila i el dash de 1,8 et trau del perill. A més, té la mateixa silueta que el Sable.
5. **Comptes: per a les monedes i les skins no cal servidor.** Un **perfil local + un codi d'exportació** (S, gratis) cobreix el 90 %. El compte de veritat seria **Unity Gaming Services (Authentication + Cloud Save)**, sense servidor propi (M), per a la Beta. Ningú vol comptes obligatoris.

## ⚖️ Veredicte dels canvis de Ferran

| Arma | Canvi de Ferran | A favor | En contra / matís | **Proposta final** | Prio |
|---|---|---|---|---|---|
| 🎯 Sniper | Nerf | Tots | Marta prefereix 9 de dany; Vortex i Lucía, 10 | Dany **12 → 10**, CD **1,5 → 1,8** (5,6 DPS). El Golem (12 HP) ja demana 2 trets; continua matant d'un tret els de ≤ 10 | 🔴 |
| 🔦 Làser | Nerf | Tots | Pau i Lucía, travessa 3; la resta, 2 | Travessa **5 → 2**, CD **0,5 → 0,6**, dany 4 (6,7 DPS contra un enemic; ~20 en una fila de 3, abans 40) | 🔴 |
| 🍄 Vareta d'espores | Sense teledirigit | Tots | Marta i Noa: sense seguiment quedarà morta si no es compensa | `homing` **2 → 0**, V **14° → 10°**, velocitat **9 → 11**, CD **1,1 → 1,0**. Opcional (v1.3): un núvol de verí de r1 durant 1,5 s en topar **[cal provar]** | 🟠 |
| 🗡️ Espasa de vidre | Mínim 4 de dany i que pegue en V | Tots | Amb 4 de dany i CD 0,55 faria 7,3 DPS i passaria el Sable (rar) sent comuna | Dany **3 → 4**, CD **0,55 → 0,7** (5,7 DPS; mata d'un colp els de 4 HP). En V: arc **120° → 70°**, abast **2,0 → 2,4**, i que **l'aim dibuixe la V** (ara dibuixa una línia recta) | 🟠 |
| 🔱 Llança tempesta | Nerf (és com el Sable però millor) | Tots, menys Iker 🤔 (és èpica) | Marta: dash a 0; Vortex/Noa: dash només a l'estocada; Laia, Ricard i Lucía: dash 1,2 | Abast **3,5 → 3,0**, dash **1,8 → 1,2**, **màxim 3 enemics** per estocada; el dany es queda igual. Si queda fluixa, CD 0,85 → 0,75 **[cal provar]** | 🟠 |

## ➕ Canvis que ixen de les dades (ningú no els havia demanat)

| Què | Problema (dada) | Canvi | Qui |
|---|---|---|---|
| **Cicle de caselles** | 4 armes alternades sumen ~26 DPS, més que la Minigun | En canviar de casella, l'arma nova espera el **50 % del CD de l'última disparada** **[cal provar]** | Vortex (C) |
| **BUG-021** | `Breakable` fa `TileNav.AddBlocker` i la línia de visió fins al centre de la caixa sempre falla. També afecta els barrils | Mirar fins a `ClosestPoint` (tirat 0,1 cap al jugador) només per als `Breakable`. Que no es puga pegar a través de les parets | Dani (A) |
| **Protocol** | Els canvis d'armes són dades compartides | Pujar el protocol **13 → 14** | Dani (A) |
| Llançaflames | 33 DPS, 4× la següent | Pellets 2 → 1 (16,7 DPS) **[cal provar]**. No hi ha consens: Vortex el deixa en S | Marta (A) |
| Disc / Projector | Per davall de 2,5 DPS; obsolets | Dany **2 → 3** tots dos | Marta (A) |
| Vareta (normal) | `homing 6`: en PvP és un problema | No tocar-la en masmorra; a 0 en PvP | Lucía (I) |
| Monedes en cooperatiu | El primer que l'agafa se l'emporta, però ONLINE.md diu "cadascú el seu". Amb 4 jugadors, cadascú en cobra 1/4 | Triar-ne una i documentar-la (Sara proposa monedes per jugador) | Sara (G) |

**Armes obsoletes (la queixa de Ferran):** C = Espasa, Revòlver, Ballesta, Projector, Vareta d'espores; D = Pistola, Bumerang, Disc, Pedra. Al pis 5, amb 6 armes el boss tarda més de 60 s. No és només un problema de números: els cofres donen 4–5 armes per pis i les armes **no milloren mai**. Solució de sistema: **forja** (+20 % de dany per nivell, 20 monedes, màxim nivell 2, Noa) i **sinergies elementals**.

## 👾 Masmorres, bosses i varietat (impacte ⭐ / esforç S-M-L)

| Idea | Impacte | Esforç | Qui |
|---|---|---|---|
| **1 de 3 amulets** en matar el boss (Pólvora +15 % de cadència, Imant +50 % de monedes, Cor de vidre +1 cor / +10 % de dany rebut, Crític 10 % ×2…) | ⭐⭐⭐ | S–M | Noa, Hugo, Pau |
| **Sala especial per pis:** mercader/forja, santuari, sala-repte opcional (3 elits → cofre èpic) | ⭐⭐⭐ | S–M | Oriol, Laia, Noa |
| **Sala pròpia per boss** (Rei Llim: basses; Golem: caixes i barrils; Mag: 8 columnes) | ⭐⭐ | S | Oriol |
| **Variant de boss per tema** amb els 8 atacs que ja existeixen (Golem de lava, Mag de cristall…) | ⭐⭐⭐ | S | Vortex, Oriol |
| **Plantilles d'onada** (emboscada, torreta, eixam) en lloc de triar a l'atzar; menys quantitat (14–19 per sala cansa) | ⭐⭐ | S | Oriol |
| **Modificador de pis** amb el bàner: "Pis daurat" ×2 monedes, "Pis fosc" | ⭐⭐ | S | Oriol |
| Mecànica de tema (erupcions al Volcà, arenes movedisses al Desert, gel a la Neu) | ⭐⭐ | S–M | Oriol |
| Paret secreta amb TNT, sales fetes a mà (30 %), bucles al mapa, 5 bosses nous | ⭐⭐ | M–L | Oriol |
| "Super" per arma que es carrega amb 25 de dany | ⭐⭐ | M | Iker |

## 🎮 Modes nous (ordre consensuat)

1. **Run diària / repte setmanal amb llavor fixa (S):** `RunState.Seed` ja existeix. Rècord local. Sense servidor. (Iker, Clara, Laia, Bru, Hugo)
2. **Boss Rush (S–M):** els 3 bosses seguits, 5 cors, rècord de temps. (Iker, Clara, Bru)
3. **Supervivència / arena d'onades (S–M)** a la sala de 19×15 o al mapa lliure, 3 min. (Iker, Clara, Pau, Laia)
4. **Mode Família / assistit:** 8 cors, reviure en 10 s, enemics −25 % de vida. Pot ser una opció de dificultat. (Rosa i Martí, Aina)
5. **Duel PvP 1v1/2v2 per rondes (L, v1.4):** necessita una taula `pvpDamage` per arma (un multiplicador global dona un TTK de 0,18 a 9 s), arenes simètriques fetes amb el Creatiu, sense armes teledirigides, i **20 duels de prova per `bore.pub`** abans. (Lucía)
6. Guardar i compartir arenes del Creatiu per codi (M). (Clara)

**Desacord:** Oriol, Noa i Ricard diuen que **primer varietat dins del pis i després modes**: "un mode nou sobre aquest pis heretarà l'avorriment". Iker, Clara i Pau volen modes curts ja, sobretot pel mòbil. **Compromís:** la run diària és S i no depén del pis (va primer); la resta de modes, després dels amulets i les sales especials.

## 👤 Comptes, perfil i monedes

| Opció | Esforç | Cost | Pros | Contres |
|---|---|---|---|---|
| **Perfil local (JSON) + codi d'exportació** | S | 0 | Sense servidor; passa el progrés PC ↔ mòbil copiant un codi; sobreviu a les actualitzacions | Si perds el dispositiu sense haver-lo exportat, el perds; es pot trampejar (en local no importa) |
| **Unity Gaming Services: Authentication (anònim o amb compte) + Cloud Save** | M | Pla gratuït (límits exactes per comprovar) | Sense servidor propi; sincronitza entre dispositius | Dependència d'un servei; trampes amb les monedes si no hi ha Cloud Code; cal un compte d'organització (Jordi recomana que siga de Víctor) |
| Comptes de plataforma (Steam, Google Play) | M | Alta de pagament | — | Trenca el joc entre plataformes |
| Servidor propi | L | Hosting | Control total | Massa per a un desenvolupador sol |

**Recomanació:** perfil local + codi a la v1.2.1/v1.3, i Unity Gaming Services a la Beta. Cap compte obligatori: en cap públic (família, mòbil) ningú el vol.

**Botiga (Sara):** uns 16 cosmètics gratis d'inici i la resta a **150 / 400 / 900 / 2.000** monedes segons la raresa. Tindre-ho tot ≈ 28.000 monedes, entre 25 i 30 h. Ara entren unes 67 / 158 / 340 / 570 / 835 monedes acumulades als pisos 1–5 en solitari **[cal provar amb el resum]**. **Qui ja té les skins les conserva** (ara són totes gratis): decisió de Ferran.

## 🧭 Com continuar el joc

1. **v1.2.1 (curta, de neteja):** BUG-021, els 5 canvis d'armes, el cicle de caselles, Disc i Projector a 3, i protocol 14. Bonus barat d'accessibilitat (S): lletra de raresa a la casella i anell sota el jugador.
2. **v1.3 "Gancho":** amulets 1 de 3, una sala especial per pis, sala i variant de boss per tema, plantilles d'onada, modificador de pis i **run diària**. Perfil local amb botiga de cosmètics.
3. **v1.4 Modes:** Boss Rush, Supervivència i, quan la xarxa aguante, Duel PvP.
4. **Online:** primer mesurar el ping real i preparar un pla B per a `bore.pub`/`ntfy.sh`, interpolació amb búfer i reconnexió. La migració d'amfitrió (L), més endavant.
5. **Art:** silueta pròpia per a cada arma cos a cos (també ajuda la Llança), raresa visible a l'arma, un motiu de terra per tema i vora clara als enemics foscos.

## 🐛 Bugs i riscos

- 🔴 **BUG-021**: causa verificada (vegeu dalt). Risc de l'arreglo: pegar a través de les parets (R-1).
- 🔴 Cicle de caselles (exploit de DPS).
- 🟠 L'aim de les armes cos a cos no ensenya on pega.
- 🟠 Monedes en cooperatiu: el codi i ONLINE.md diuen coses diferents.
- 🟠 Mòbil: APK de 63 MB amb clau de depuració (avís d'Android), no es pot jugar amb una mà, sense opció de gràfics baixos i la sala cau si l'app se suspén **[cal provar en un telèfon real]**.
- 🟢 Skins fosques, Zombi verd i enemics gris fosc es perden sobre alguns terres.
- 🟢 `context.md` deia "elit a cada pis": només n'hi ha des del pis 3.

## 🧪 Pla de proves (Mode Creatiu, pis 1)

1. BUG-021: Espasa, Sable, Llança, Martell i puny contra caixa i barril; sol, amfitrió i client; i una caixa darrere d'una paret amb el Martell (no s'ha de trencar).
2. TTK contra un Golem (12 HP): Sniper → 2 trets; Làser → 3 trets; Espasa → 3 colps; Llança → 4 colps.
3. Llança vs Sable: 5 sales cadascuna, comptar els cors perduts.
4. Vareta d'espores sense seguiment: boles que toquen un Golem quiet a 3 i a 7 caselles.
5. Cicle de caselles: Sniper → Làser seguit; l'arma nova ha d'esperar.
6. Mirar al resum les monedes per pis en 5 partides (per als preus de la botiga).

## Notes del consell

| Perfil | Ara | Amb els canvis |
|---|---|---|
| 📊 Marta · balanç | 6 | 7 |
| 🧪 Dani · tester | 6 | 7 |
| 🎒 Pauet · 13 anys | 6 | 6,5 (8 amb botiga i mode curt) |
| 🫶 Laia · cooperativa | 6,5 | 7 (8,5) |
| 🏆 Vortex · pro | 5 | 6,5 (7,5 amb builds) |
| ⭐ Iker · Brawl Stars | 5 | 7 |
| 🗝️ Noa · Soul Knight | 4,5 | 7,5 |
| 😒 Ricard "El Crític" (inventat) | 5,5 | 6,5 |
| 🤩 Bru "El Fan" (inventat) | 7,5 | 8,5 |
| 🔁 Hugo · retenció | 5 | 7–7,5 |
| 🧱 Clara · jocs oberts | 6 | 7,5 |
| 🎨 Júlia · píxel art | 6,5 | 8 |
| 🧭 Oriol · nivells | 4,5 | 7,5 |
| 💰 Sara · economia | 6 | 7,5 |
| 🌐 Jordi · xarxa | 6,5 | 7,5 |
| ♿ Aina · accessibilitat | 5 | 7 |
| 👪 Rosa i Martí · família | 5 | 7,5 |
| 📱 Youssef · mòbil gama baixa | 5,5 | 7,5 |
| ⚔️ Lucía · PvP | 4 | 7 |
| **Mitjana** | **5,6** | **7,3** |

*Els perfils "El Crític" i "El Fan" són personatges inventats, inspirats en un estil de reacció. No representen ni citen cap creador real.*

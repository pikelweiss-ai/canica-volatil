# Grup C · Competitius i veterans · Alpha v1.2.0 → v1.2.1 · 20/11/2026

Dades: `dossier-canica.md`, `EnemyBoss.cs`, `DungeonGenerator.cs`, `PlayerAttack.cs`, temes `Assets/Dungeon/Temes/*.asset`.

Fets que hem comprovat al codi i que surten a tots tres informes:
- **8 temes, 3 bosses.** Cova i Claveguera → Rei Llim; Bosc, Neu i Volcà → Golem Gegant; Desert, Cripta i Cristall → Mag Fosc. Cada boss té 4 atacs (2 "calmats" + 1 més cada 3 pisos; tots en furiós a mitja vida). En total hi ha 8 atacs.
- **Canviar d'arma no té cap retard**, i cada casella té el seu cooldown (`SelectSlot` només canvia l'índex). Disparar i canviar fa que les DPS de les 4 caselles se sumen.
- **Melee:** `Swing` pega a tots els enemics dins del con (`OverlapCircleAll` + angle), i el dash comença després de calcular el colp. El dash no dona invulnerabilitat.
- Vida del boss = base + 25 × (pis − 1). Per exemple, el Golem al pis 5 té 190.

---

## 🏆 Àlex "Vortex" Gómez · Jugador pro
> He fet 20 runs al cap. El meta no és cap arma: el meta és la tecla 1-2-3-4.

### Primera impressió
El combat respon bé i els bosses ja avisen els atacs (`MarkAttack`), i això està molt bé. Però des del pis 3 sempre faig el mateix: arreplegue les 4 armes de més DPS i vaig canviant entre elles. No hi ha cap decisió que faça diferent una run de l'anterior. Ferran té raó: no té gancho.

### Exploit número 1: el cicle de caselles
Com que cada casella té el seu CD i canviar d'arma no costa res, Sniper (8) + Làser (8) + Martell (5,5) + Llançagranades (~5) donen **~26 DPS**. És més que la Minigun, i amb armes de "tret pesat". El Golem del pis 4 (165 HP) cau en uns 6 s.
- **Arreglo:** en canviar de casella, l'arma nova no pot disparar fins que no haja passat el **50 % del CD de l'última arma disparada**. Això no toca el cooldown per casella, que ja funciona. **[cal provar]**

### Tier list dins de la masmorra (v1.2.0)
| Tier | Armes | Per què |
|---|---|---|
| **S** | Llançaflames, Sniper, Làser, Minigun | 33 DPS; mata d'un tret els 13 enemics normals; travessa 5 i mata d'un tret 8 dels 13; 22 DPS en ràfega |
| **A** | SMG, AK47, Ballesta de cristall, Escopeta, Vareta | 14,3 / 8,3 / 8 + alentir / 14 de prop / teledirigida 6, no cal apuntar |
| **B** | Sable de corall, Martell orbital, Llança tempesta, Shuriken, Llançagranades | Fan feina, però a partir del pis 4 les canvies |
| **C** | Espasa de vidre, Revòlver, Ballesta, Projector, Vareta d'espores | Totes per davall de 5 DPS útils |
| **D** | Pistola, Bumerang, Disc, Pedra | 1,5–2,9 DPS: amb aquestes el boss del pis 1 dura més de 24 s |

**Obsoletes:** tot el C i el D. Les armes no milloren i els cofres donen 4–5 armes per pis, així que al pis 3 les comunes ja no les agafa ningú. No és un problema de números, és de sistema: falta una manera de millorar l'arma que t'agrada (ho explica la Noa).

### Números que propose
| Arma | Ara | Proposta | Resultat |
|---|---|---|---|
| Sniper | 12 dany, CD 1,5 | **10 dany, CD 1,8** | 5,6 DPS. El Golem (12) ja necessita 2 trets; continua matant d'un tret els de ≤ 7 |
| Làser | 4, travessa 5, CD 0,5 | **4 dany, travessa 2, CD 0,6** | 6,7 DPS contra un sol enemic, ~20 en fila de 3 (abans 40 amb 5) |
| Llança tempesta | dash 1,8 cada colp, abast 3,5 | **dash 1,8 només a l'estocada** (cada 2n colp, `alternateSwing`), **abast 3,0** | El mateix DPS (3,5), però amb la meitat de mobilitat |

### Bosses
3 bosses per a 8 temes vol dir que, al pis 6, ja has vist cada boss dues vegades. Com que `EnemyBoss` té els atacs configurables, una **variant per tema** és barata (S): Golem de lava (Charge + RockRain que deixa toll 2 s), Mag de cristall (Teleport + Fan de 3 ràfegues). Que siga més difícil pel patró, no per la vida.

### El que més em preocupa
1. El cicle de caselles invalida qualsevol balanç individual. Arregleu-lo abans de tocar números **[cal provar]**.
2. Sniper i Làser, a més de dany, donen seguretat: abast 12 i la càmera s'allunya. Si no es toca el CD, es queden en S.

### Nota del joc ara mateix: 5/10 · i amb els canvis: 6,5/10 (7,5 si hi ha builds)

---

## 📱 Iker Sanz · Ve de Brawl Stars
> Tío, una partida de 25 minuts al mòbil i després res a fer? Al Brawl jugue 3 minuts i ja tinc ganes de la següent.

### Primera impressió
Moltes armes tenen personalitat. El Llançaflames és com un brawler de foc de curt abast, l'Sniper un franctirador d'un tret, l'Escopeta un tanc de prop i la Llança un assassí amb dash. El problema és el que hi ha al voltant: les monedes no serveixen per a res, no hi ha modes curts i no tens cap meta per a la setmana.

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Número |
|---|---|---|---|
| Nerf Sniper | 👍 | Ara mata tot el que hi ha a la pantalla sense acostar-te | El del Vortex (10 / 1,8) |
| Nerf Làser | 👍 | Llegendari sí, però no un esborrador de sales | Travessa 2 |
| Espores sense teledirigit | 👍 | Teledirigit = zero habilitat | Compensar amb núvol (Noa) |
| Espasa a 4 | 👍 | Ara costa 2 colps per a enemics de 4 HP | 4 dany, CD 0,7 |
| Nerf Llança | 🤔 | És èpica: és normal que pegue més que el Sable rar. El que molesta és el dash | Dash cada 2n colp |

### Les meues propostes
- **"Super" per arma (M):** una barra que es carrega amb **25 de dany** fet. Exemples: Sniper, tret que travessa tot; Escopeta, ràfega de 12 perdigons; Llança, 3 dashos seguits. Una tecla en PC i un botó gran en mòbil. Seria el gancho de 30 segons que ara no té.
- **Modes curts, en aquest ordre:**
  1. **Run diària (S):** `RunState.Seed` ja existeix, així que seed = data. Tothom juga la mateixa masmorra i compara el rècord local (pis + temps). Ni servidor ni comptes.
  2. **Boss Rush (S–M):** els 3 bosses seguits, 5 cors, 1 arma de cada raresa. Unes 4–5 min. Rècord de temps.
  3. **Arena d'onades (M):** una sala de 19×15 (la del boss) amb onades infinites i 3 minuts. Rècord d'enemics.
- **Online:** 300 ms amb `bore.pub` es nota molt en mòbil **[cal provar]**. Abans de pensar en PvP, que el cooperatiu siga fluid. El PvP 2v2 m'agradaria, però és L i trenca el balanç (Sniper d'un tret).
- **Monedes:** que els rècords desbloquegen skins (per exemple, Boss Rush per davall de 4 min = skin daurada). Res de pagar.

### El que més em preocupa
1. Una run llarga sense modes curts mata el mòbil: jugues al bus i a la parada has de tancar.
2. El ping de 300 ms en cooperatiu des del mòbil **[cal provar]**.

### Nota del joc ara mateix: 5/10 · i amb els canvis: 7/10

---

## 🗡️ Noa Ribes · Ve de Soul Knight
> 23 armes està bé. Però a Soul Knight el que em feia fer "una més" no eren les armes, eren els buffs i la combinació.

### Primera impressió
Pisos iguals: només hi ha 4 tipus de sala (`Start`, `Combat`, `Treasure`, `Boss`) i el portal et porta directament al pis següent. No hi ha cap decisió entre pisos, ni personatge, ni buff, ni botiga. Els llegendaris (el Làser) són forts, però no "es noten llegendaris": no tenen cap efecte especial.

### El que falta respecte al gènere (per impacte)
1. **Buffs entre pisos (S–M):** en matar el boss, tries **1 de 3 amulets**. Amb números:
   - Pólvora: **+15 %** de cadència
   - Fang tòxic: les bales tenen un **20 %** de posar verí
   - Cor de vidre: **+1 cor**, però **+10 %** de dany rebut
   - Pas de foc: el dash deixa foc **1 s**, 1 de dany cada 0,5 s
   - Imant: **+50 %** de monedes
   - Crític: **10 %** de fer ×2
2. **Millora d'armes (M):** forja en la sala del tresor. Per **20 monedes de la partida**, +1 nivell (**+20 % de dany**, màxim nivell 2). L'Espasa de vidre a nivell 2 faria 5,7 → 8 DPS. Així ningú tira l'arma que li agrada.
3. **Sinergies elementals (M):** verí + foc = explosió de **3 a r1** (Vareta d'espores + Llançaflames); alentir + elèctric = la cadena salta **+1** (Ballesta de cristall + Projector). Així les armes C tenen un lloc a la build.
4. **Personatges amb habilitat (L):** 3 canyques al principi (per exemple, Tanc: 6 cors + escut 2 s; Ràpida: 2 dashos; Mag: 4 s de doble cadència, CD 20 s).
5. **Sales noves:** botiga (amb les monedes que ja cauen, unes 50 per pis **[cal provar]**), sala de repte (sense cures, cofre èpic segur), sala secreta.

### Sobre els canvis de Ferran
- **Vareta d'espores sense teledirigit:** 👍, però sense `homing` les 3 boles de V 14° a velocitat 9 fallen molt. Propose: **CD 1,1 → 0,9**, **velocitat 9 → 11**, i que en topar o arribar al final deixen un **núvol de r1 durant 1,5 s** (1 de verí cada 0,5 s, màxim 3 dosis). Ens quedem en ~3,3 DPS directe i ~6 amb verí **[cal provar]**.
- **Espasa de vidre a 4:** 👍. **4 dany, CD 0,55 → 0,7** = 5,7 DPS (per davall del Sable, 6,2), però **mata d'un colp els enemics de 4 HP**. Que el traç del colp dibuixe els 120° reals perquè es veja la V **[cal provar]**.
- **Llança vs Sable:** el Sable cobreix més àrea (130° × r2 ≈ 4,5 u²), la Llança menys (25° × r3,5 ≈ 2,7 u²), però la Llança pega des de fora de l'abast de contacte i el dash de 1,8 et trau del perill a cada colp. Per això a les mans sembla superior. D'acord amb el dash només a l'estocada.

### El que més em preocupa
1. Si s'afegeixen modes abans de les builds, tindrem més modes igual d'avorrits.
2. Que els amulets se sumen al cicle de caselles i el joc es desmadre **[cal provar]**.

### Nota del joc ara mateix: 4,5/10 · i amb els canvis: 7,5/10

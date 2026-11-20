# Grup A · Balanç tècnic · v1.2.0 → v1.2.1

Fonts: `dossier-canica.md`, `Assets/Weapons/*.asset`, `PlayerAttack.cs`, `Projectile.cs`, `EnemyHealth.cs`, `TileNav.cs`, `Breakable.cs`, `DungeonGenerator.cs` (vida d'enemics ×(1 + 0,3·(pis−1)), arrodonida; bosses +25 per pis).
Referències de TTK: Golem pis 1 = 12 HP, Golem pis 5 = 26 HP, boss pis 1 ≈ 65–90 HP, boss pis 5 ≈ 165–190 HP (en solitari).

---

## 📊 Marta Ferrer · Desenvolupadora balancejadora
> He passat les 23 armes pel full de càlcul. El problema no és el DPS: són els **punts de tall** (quants colps per enemic) i que hi ha **parelles amb el mateix rol**.

### Primera impressió
Ferran té bon olfacte: les cinc armes que assenyala tenen un problema real, però en tres casos el problema no és el que diu el DPS. A partir del pis 5 hi ha sis armes amb què un boss de 190 HP tarda **més de 60 s**: Pedra (123 s), Disc (80), Projector (75), Vareta d'espores (69), Pistola (66) i Bumerang (66). Eixes són les que "acabes tirant". Llançaflames (5,6 s) i Minigun (8,5 s) segueixen fora de la corba.

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Número o matís proposat |
|---|---|---|---|
| Sniper nerf | 👍 | 12 de dany mata d'un tret 13 dels 14 enemics normals fins al pis 3, a 12 d'abast i amb zoom: risc zero | Dany **12 → 9**, CD 1,5. 6 DPS. Pis 4+: Llim i Bolet necessiten 2 trets; pis 6+: Mag, Malo i Esquelet també. Boss 90 HP: 13,5 s |
| Làser nerf | 👍 | Fa el mateix que el Sniper (abast 12) però dispara 3 vegades més i travessa 5: fins a **24 de dany per tret** en una fila | Dany **4 → 3**, travessa **5 → 2**, CD **0,5 → 0,45**. 6,7 DPS, màxim 9 per tret. Rol: tallar files, no fer *one-shots* |
| Vareta d'espores sense teledirigit | 👍 | El seguiment és la identitat de la Vareta (homing 6). Dues armes no poden tindre'l | `homing` **2 → 0**. Compensació: V **14° → 10°**, velocitat **9 → 11**, CD **1,1 → 1,0**. Sense això, a 7 d'abast les boles laterals passen a 1,75 del centre i només entra una **[cal provar]** |
| Espasa de vidre: 4 de dany i en V | 👍 amb matís | Amb 4 de dany i CD 0,55 farà 7,3 DPS i passaria per damunt del Sable (raro, 6,2) sent comuna | Dany **3 → 4**, CD **0,55 → 0,7** (5,7 DPS). "En V": arc **120° → 70°** (±35°), abast **2,0 → 2,4**, i que l'aim dibuixe eixa V (ara dibuixa una línia recta) |
| Llança nerf | 👍 | Vegeu baix: no guanya per DPS, guanya per **seguretat i abast** | Abast **3,5 → 3,0**, dash **1,8 → 0**, màxim **3 enemics** per estocada, empenta **5 → 3** |

**Per què la Llança sembla superior al Sable si fa 3,5 DPS contra 6,2?**
1. **Abast efectiu.** La zona de colp arriba a 3,5 (el Sable a 2,0). Aranya, Malo, Llim, Ratpenat i Escarabat peguen de contacte: amb la Llança els mates abans que et toquen. El DPS que importa és DPS × temps que pots pegar sense rebre; amb el Sable has d'estar dins de la seua zona.
2. **Travessa sense límit.** El `Swing` pega a tots els que hi ha dins de l'arc de 25°. Les onades són de 2 + pis enemics que vénen en fila per les portes i els passadissos: 3 enemics × 3 = 9 de dany per estocada (14,7 DPS de grup).
3. **Dash gratis de 1,8.** No gasta el dash del jugador i va a 12 u/s. És mobilitat extra cada 0,85 s.
4. **Punts de tall iguals al pis 1.** Amb 3 de dany mata d'un colp Aranya, Esporer, Tirador, Ratpenat i Kamikaze, igual que el Sable.
Contra un boss, en canvi, és pitjor: 54 s per a un boss de 190 HP (el Sable, 31 s). Per això el nerf ha de tocar l'abast i el grup, no el dany. Sense dash, la Llança és "toc de lluny" i el Sable és "entrar i tallar": dos rols diferents.

### Les meues propostes
- **Balanç (amb números):**
  - Llançaflames: 33 DPS, 4× la següent arma no automàtica. 300 trets = 600 de dany, prou per a 3 bosses del pis 1. Dany per partícula 1, pellets **2 → 1** (16,7 DPS) **[cal provar]**.
  - Minigun: amb la calor (+1 per tret, refreda 15/s, límit 60) dispara uns 8 s seguits i després es bloqueja 2 s: ~17 DPS reals. Està bé com a llegendària; no la toquaria.
  - Obsoletes: Disc **2 → 3** de dany (3,5 DPS, i els rebots són el seu nínxol); Projector **2 → 3** (més 3 salts d'1 = fins a 6 per tret en grup); Bumerang, de moment res (si torna i pega dues vegades, ja fa 5,7) **[cal provar]**. Pedra i Pistola són d'inici: és normal que caduquen.
- **Rols duplicats que queden:** AK47 / SMG (ràfega), Ballesta / Ballesta de cristall (travessar). Amb aquests canvis, Sniper (*one-shot*) i Làser (fila) ja es diferencien, i també Vareta (teledirigit) i Espores (verí).
- **Enemics / bosses:** la vida dels enemics puja un 30 % per pis, però el dany de les armes no puja mai. Al pis 8 un Golem té 37 HP: l'arsenal es queda curt a partir del pis 6–7, i per això "s'acaba tirant tot". Seria bo que el pis 5+ donara armes de raresa més alta, no més dany.

### El que més em preocupa
1. Si el Llançaflames no es toca, tota la resta de balanç té poc sentit: el comparareu tot amb ell.
2. Vareta d'espores sense seguiment: és l'arma amb més risc de quedar morta (2,7 DPS directe + fins a 2,7 de verí). **[cal provar]**
3. Llança amb 3,5 DPS i sense dash: pot quedar massa fluixa. Si passa, CD **0,85 → 0,75**. **[cal provar]**

### Nota del joc ara mateix: 6/10 · i amb els canvis: 7/10

---

## 🧪 Dani Vidal · Tester especialista
> He reproduït el BUG-021 al codi i ja sé per què passa. La resta són casos límit dels canvis de balanç que cal provar abans de publicar.

### Primera impressió
El cos a cos té dos problemes de coherència: no trenca caixes (BUG-021) i **l'aim no ensenya on pega** (línia recta per a una zona de 120°). Això últim és el que Ferran nota com "no pega en V".

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Número o matís proposat |
|---|---|---|---|
| Nerfs de Sniper / Làser / Llança | 👍 | Són canvis de dades (`.asset`) | Pujar el **protocol 13 → 14**: amb versions barrejades, amfitrió i client tindrien armes diferents |
| Espores sense teledirigit | 👍 | El verí es calcula a l'amfitrió (`ProxyHit`) | Provar amb un client que les doses (màxim 3) també arriben |
| Espasa en V | 👍 | Ara l'aim no coincideix amb el colp | L'aim de les armes cos a cos ha de dibuixar l'arc (`meleeArc`), no una línia |

### BUG-021: el cos a cos no trenca caixes
- **Causa (verificada):** `Breakable.Start()` fa `TileNav.AddBlocker(transform.position)`, de manera que la casella de la caixa queda "bloquejada". A `PlayerAttack.Swing` (línia 449) es comprova `TileNav.LineOfSight(origin, hit.transform.position)`: mira cada 0,25 fins al **centre de la caixa**, i l'última mostra ja cau dins de la seua casella. Per tant, **sempre** dona fals, llevat que estigues a 0,25 o menys. El puny no ho comprova i per això sí que trenca caixes. També afecta els **barrils explosius**.
- **Arreglo proposat:** per als `Breakable`, mirar la línia de visió fins a `hit.ClosestPoint(origin)` tirat 0,1 cap al jugador (o ignorar la casella de destí). Per als enemics es pot deixar com està.

### Bugs i riscos
| # | Gravetat | Què | Passos | Esperat |
|---|---|---|---|---|
| BUG-021 | 🔴 | El cos a cos no trenca caixes ni barrils | Sala activa → Espasa → pegar a una caixa des d'1 casella | Es trenca en 1–2 colps |
| R-1 | 🟠 | Després de l'arreglo, pegar a través de la paret | Caixa darrere d'una paret d'1 casella → Martell (radial) | **No** s'ha de trencar |
| R-2 | 🟠 | Aim de cos a cos enganyós | Espasa equipada | L'aim mostra la V/arc real |
| R-3 | 🟢 | Llança amb el dash cap a la paret | Estocada contra una paret | Es para sense quedar-se enganxat (relacionat amb el BUG-009) |

### Pla de proves (Mode Creatiu, pis 1)
1. **BUG-021:** Espasa, Sable, Llança, Martell i puny (control) contra caixa i barril; en solitari, com a amfitrió i com a client; en diagonal i en un passadís.
2. **TTK amb cronòmetre** contra un Golem (12 HP). Esperat: Sniper 9 → 2 trets (1,5 s); Làser 3 → 4 trets (1,35 s); Espasa 4 → 3 colps (1,4 s); Llança 3 → 4 colps (2,55 s). Una desviació de més del ±20 % és un bug.
3. **Llança vs Sable, 5 sales cada una:** comptar els cors perduts. Si la Llança en perd menys de la meitat que el Sable, encara està per damunt **[cal provar]**.
4. **Llança contra 4 Malos en fila:** n'han de rebre 3, no 4.
5. **Espores sense teledirigit:** a 3 i a 7 caselles, comptar les boles que toquen un Golem quiet.
6. **Online amb versions diferents:** un client v1.2.0 no ha de poder entrar a una sala v1.2.1.

### El que més em preocupa
1. Que l'arreglo de la línia de visió obri colps a través de les parets (R-1).
2. Publicar el balanç sense pujar el protocol.

### Nota del joc ara mateix: 6/10 · i amb els canvis: 7/10

# Grup E · Visió de producte · Alpha v1.2.0 → v1.2.1

---

## 🔁 Hugo Navarro · Retenció i addicció (amb ètica)
> Ferran, que tu mateix t'avorrisques és la dada més valuosa que tens: el teu D7 personal ja està dient-te alguna cosa.

### Primera impressió
Tens raó: no té gancho. No és per falta de contingut (22 armes, 15 enemics, 8 temes), sinó perquè **cap partida canvia la següent**. Acabes una run i el joc torna exactament al mateix lloc: les monedes no compren res, les armes no es desbloquegen i el pis 1 és igual de llarg. El resum i els rècords de la v1.2.0 són el primer pas, però són un mirall, no una meta.

### Els 4 bucles, amb nota
| Bucle | Nota | Per què | Què falta |
|---|---|---|---|
| 30 s · combat | **6/10** | Hi ha armes amb sensació (Minigun, Llançaflames, Escopeta) i portes que es tanquen | Sales quadrades: el combat no canvia segons on estàs |
| 5 min · sala/pis | **4/10** | Només 4 tipus de sala (`Start`, `Combat`, `Treasure`, `Boss`); el cofre és l'única sorpresa (45 %/50 %) | Decisions: triar camí, triar recompensa (els **amulets 1 de 3** del consell anterior encara no hi són) |
| Sessió · run | **5/10** (abans 4) | El resum en morir tanca bé la partida; "arma favorita" és un bon detall | Una run no deixa res al compte: les monedes de `CoinBank` són un número mort |
| Setmana · tornar | **3/10** (abans 2) | Rècords locals de pis/enemics/bosses/monedes, sol i coop separats | Cap objectiu nou, cap raó per obrir el joc dilluns i no diumenge |

Nota global: **5/10**, igual que fa un consell. El resum suma, però Ferran no ha fet encara cap de les 3 palanques barates que vam proposar (monedes→cosmètics, col·lecció, repte setmanal).

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Matís |
|---|---|---|---|
| "Més modes" | 🤔 | Un mode nou sense progressió és un altre bucle que tampoc deixa res | Primer que les monedes servisquen; després modes que les donen |
| Comptes perquè les skins no es perden | 👍 amb condicions | El problema real no és el servidor, és que **no hi ha res a perdre** | Perfil local + codi de còpia ara; compte en línia a la Beta (com ja diu `ALPHA_PLAN.md`) |
| Nerfs (Sniper, Làser…) | 👍 | Armes que maten tot igual fan les runs iguals | Això és del grup de balanç |

### Les 5 palanques, ordenades per esforç
1. **Monedes → cosmètics (S).** Dels 28 skins, deixa'n ~8 gratis i posa preu a la resta: 150 / 400 / 900 monedes. Calcule ~40–60 monedes per pis (≈5–7 sales de combat × 5–6 enemics × 1,5 monedes) → una run al pis 4 ≈ 200 monedes, una skin comuna per run, la cara cada 4–5 runs. **[cal provar]**: mira `RunStats.CoinsEarned` en 5 runs reals abans de fixar preus. Qui ja tenia skins, les conserva (migració).
2. **Desbloquejos d'armes (S–M).** Comença amb ~10 armes al pool; la resta entren en descobrir-les o en complir una fita ("mata 1 boss amb arma cos a cos → Martell Orbital al pool"). Cada run té una barra de "pròxim desbloqueig". És el que fa Dead Cells i és net.
3. **Repte setmanal amb llavor fixa (M).** `DungeonGenerator` ja accepta `seed` i `RunState.Seed`: llavor = número de setmana. Mateixa masmorra per a tots, rècord propi de pis i temps, i un barret exclusiu de la setmana que **torna a eixir més avant** (sense FOMO).
4. **Pantalla de perfil (S).** Runs jugades, pis màxim, bosses, armes descobertes 14/22, skins 9/28. Tot ja es compta a `RunStats`; només cal acumular-ho.
5. **Fites/col·lecció (M).** 15–20 fites que donen monedes una vegada. Metes a mitjà termini sense grind.

### Comptes sense patrons foscos
- **Ara (S):** perfil local amb identificador i versió (`ALPHA_PLAN.md` ja ho preveu) + **codi d'exportació**: un text en base64 per a passar el progrés del PC al mòbil. Zero servidor.
- **Més avant (M):** Unity Authentication anònima + Cloud Save (tenen capa gratuïta) **[cal provar els límits]**.
- **Prohibit:** caixes de botí, monedes de pagament, ratxes diàries que castiguen, notificacions, ofertes amb temps. Joc per a amics i jugadors de 13–17 anys.

### El que més em preocupa
1. Que Ferran faça modes nous i el joc continue sense memòria entre partides.
2. Que les monedes estiguen mal calibrades i una skin coste 20 runs **[cal provar]**.
3. Que el compte es prometa com "en línia" quan és un perfil local.

### Nota: 5/10 · amb les palanques 1, 2 i 4: 7/10 · amb el repte setmanal: 7,5/10

---

## 🧱 Clara Pastor · Jocs oberts / sandbox
> El Mode Creatiu és la llavor del joc com a plataforma. Però ara mateix és una caixa de joguets que es buida cada vegada que la tanques.

### Primera impressió
El nucli és millor que fa un consell: cooldown per casella, durabilitat per instància, online amb amfitrió autoritari i entre plataformes. Això és el que demanava per a obrir. El Creatiu ja té quatre pestanyes (armes, menjar, enemics, cofres) i funciona online, però **només per a l'amfitrió** i **no guarda res** (`CreativeMode.cs`: "No es guarda RES").

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Matís |
|---|---|---|---|
| "Hem de fer més modes" | 👍 | El nucli ja aguanta | Modes que **reaprofiten** el que hi ha, no PvP de zero |
| Comptes | 🤔 | Com a plataforma, els comptes serveixen per a compartir creacions, no només skins | Per a compartir mapes **no cal compte**: un codi de text basta |

### Modes nous, en ordre
| # | Mode | Esforç | Per què ara |
|---|---|---|---|
| 1 | **Boss Rush**: els 3 bosses seguits (Rei Llim 70, Golem 90, Mag Fosc 65), escalats com al pis 3/6/9, amb 4 armes triades | **S** | Tot existeix; partides de 5–8 min, perfectes per al mòbil |
| 2 | **Supervivència d'onades** al mapa lliure: onades creixents (2 + onada enemics), cofre cada 5 onades, rècord d'onada | **S–M** | Reaprofita el spawner del Creatiu i `RunStats` |
| 3 | **Repte de la setmana** (llavor fixa) | **M** | Comparteix feina amb Hugo |
| 4 | **Creatiu guardat**: desar l'escenari (enemics, cofres, armes) en JSON local + botó "Jugar-lo" | **M** | Primer pas per a contingut fet per jugadors |
| 5 | **Compartir arenes per codi** (JSON comprimit en base64 o per la sala online) i Creatiu per a tots els jugadors de la sala | **M** | Laia ja ho demanava |
| 6 | PvP (1v1, 2v2) | **L** | Necessita balanç propi (100 HP a `ALPHA_PLAN.md`); no abans de la v1.4 |
| — | Història, roleplay, editor de tiles | **L+** | Encara no |

### Comptes, vistos com a plataforma
- Mapes compartits **sense servidor**: el codi de text viatja per WhatsApp o Discord. Funciona demà.
- Un "navegador d'arenes" públic sí que necessita backend; `ntfy.sh` és públic i efímer, no serveix per a guardar.
- Primer el perfil local amb ID estable; el compte, quan hi haja alguna cosa que valga la pena sincronitzar.

### El que més em preocupa
1. Fer 3 modes alhora i no acabar-ne cap. Un per versió.
2. Un mode nou que tinga la mateixa repetició que la masmorra **[cal provar si el Boss Rush cansa als 3 intents]**.
3. Arenes compartides amb 200 enemics que rebenten el mòbil: posa un límit (~40 enemics).

### Veredicte joc obert: **"encara no" per a l'editor, sí per a Boss Rush i Supervivència**
### Nota: 6/10 · amb Boss Rush + Supervivència + Creatiu guardat: 7,5/10
